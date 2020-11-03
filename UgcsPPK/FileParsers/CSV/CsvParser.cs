using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace FileParsers.CSV
{
    public class CsvParser : Parser
    {
        public CsvParser()
        {
        }

        public override List<GeoCoordinates> Parse(string logPath)
        {
            if (!File.Exists(logPath))
                throw new FileNotFoundException($"File {logPath} does not exist");
            var logName = Path.GetFileName(logPath);
            DateTime currentDate = ParseCurrentDate(logName);
            var coordinates = new List<GeoCoordinates>();
            using (StreamReader reader = File.OpenText(logPath))
            {
                string line;
                if (HasHeader)
                {
                    line = reader.ReadLine();
                    FindIndexesByHeaders(line);
                }
                var format = new CultureInfo("en-US", false);
                format.NumberFormat.NumberDecimalSeparator = DecimalSeparator;
                double previousTime = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith(CommentPrefix))
                        continue;
                    var data = line.Split(new[] { Separator }, StringSplitOptions.None);
                    var lat = double.Parse(data[LatitudeIndex], NumberStyles.Float, format);
                    var lon = double.Parse(data[LongitudeIndex], NumberStyles.Float, format);
                    var traceNumber = int.Parse(data[TraceNumberIndex]);
                    var timeOfTheCurrentDay = DateTime.Parse(data[DateIndex]);
                    var totalMS = timeOfTheCurrentDay.Second * 1000 + timeOfTheCurrentDay.Minute * 60000 + timeOfTheCurrentDay.Hour * 3600000 + timeOfTheCurrentDay.Millisecond;
                    if (previousTime > totalMS)
                        currentDate.AddDays(1);
                    var currentDateAndTime = currentDate.AddMilliseconds(totalMS);
                    previousTime = totalMS;
                    coordinates.Add(new GeoCoordinates(currentDateAndTime, lat, lon, traceNumber));
                }
            }
            return coordinates;
        }

        private DateTime ParseCurrentDate(string logName)
        {
            Regex r = DateTimeRegex != null ? new Regex(DateTimeRegex) : new Regex(@"\d{4}-\d{2}-\d{2}");
            Match m = r.Match(logName);
            if (!m.Success)
                throw new Exception("Incorrect file name. Set date of logging.");

            return DateTime.ParseExact(m.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public override Result CreatePpkCorrectedFile(string oldFile, string newFile, IEnumerable<GeoCoordinates> coordinates, CancellationTokenSource token)
        {
            if (!File.Exists(oldFile))
                throw new Exception($"File {oldFile} does not exist");
            var result = new Result();
            var text = new StringBuilder();
            var format = new CultureInfo("en-US", false);
            format.NumberFormat.NumberDecimalSeparator = DecimalSeparator;
            using (StreamReader reader = File.OpenText(oldFile))
            {
                string line;
                if (HasHeader)
                {
                    line = reader.ReadLine();
                    FindIndexesByHeaders(line);
                    text.Append(line + "\n");
                }
                while ((line = reader.ReadLine()) != null)
                {
                    if (token.IsCancellationRequested)
                        break;
                    try
                    {
                        if (line.StartsWith(CommentPrefix))
                        {
                            text.Append(line);
                            continue;
                        }
                        var data = line.Split(new[] { Separator }, StringSplitOptions.None);
                        var traceNumber = int.Parse(data[TraceNumberIndex]);
                        var lon = coordinates.Where(c => c.TraceNumber == traceNumber).FirstOrDefault();
                        var lat = coordinates.Where(c => c.TraceNumber == traceNumber).FirstOrDefault();
                        if (lat != null && lon != null)
                        {
                            data[LongitudeIndex] = coordinates.Where(c => c.TraceNumber == traceNumber).FirstOrDefault().Longitude.ToString(format);
                            data[LatitudeIndex] = coordinates.Where(c => c.TraceNumber == traceNumber).FirstOrDefault().Latitude.ToString(format);
                            text.Append(string.Join(Separator, data) + "\n");
                            result.CountOfReplacedLines++;
                        }
                        else
                            text.Append(line);
                    }
                    catch (Exception)
                    {
                        text.Append(line);
                    }
                    finally
                    {
                        result.CountOfLines++;
                        CountOfReplacedLines++;
                    }
                }
                File.WriteAllText(newFile, text.ToString());
                return result;
            }
        }

        private void FindIndexesByHeaders(string line)
        {
            List<string> headers;
            headers = line.Split(new[] { Separator }, StringSplitOptions.None).ToList();
            LongitudeIndex = headers.FindIndex(h => h.Equals(LongitudeColumnName));
            LatitudeIndex = headers.FindIndex(h => h.Equals(LatitudeColumnName));
            DateIndex = headers.FindIndex(h => h.Equals(DateColumnName));
            TraceNumberIndex = headers.FindIndex(h => h.Equals(TraceNumberColumnName));
            if (LongitudeIndex == -1 || LatitudeIndex == -1 || DateIndex == -1 || TraceNumberIndex == -1)
                throw new Exception("Column names are not matched");
        }
    }
}