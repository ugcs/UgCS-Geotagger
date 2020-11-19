using FileParsers.Yaml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace FileParsers.CSV
{
    public class MagDroneCsvParser : CsvParser
    {
        public MagDroneCsvParser(Template template) : base(template)
        {
        }

        public override List<GeoCoordinates> Parse(string logPath)
        {
            if (Template == null)
                throw new NullReferenceException("Template is not set");
            if (!File.Exists(logPath))
                throw new FileNotFoundException($"File {logPath} does not exist");
            if (Template.Format.HasFileNameDate)
                ParseDateFromNameOfFile(logPath);
            var coordinates = new List<GeoCoordinates>();
            using (StreamReader reader = File.OpenText(logPath))
            {
                string line = SkipLines(reader);
                if (Template.Format.HasHeader)
                {
                    if (line == null)
                    {
                        line = reader.ReadLine();
                        FindIndexesByHeaders(line);
                        skippedLines.Append(line + "\n");
                    }
                    else
                        FindIndexesByHeaders(line);
                }

                var format = new CultureInfo("en-US", false);
                format.NumberFormat.NumberDecimalSeparator = Template.Format.DecimalSeparator;
                var traceCount = 0;
                DateTime? firstDateTime = null;
                var timestampOfTheFirsDatetTime = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith(Template.Format.CommentPrefix))
                        continue;
                    var data = line.Split(new[] { Template.Format.Separator }, StringSplitOptions.None);
                    var lat = double.Parse(data[(int)Template.Columns.Latitude.Index], NumberStyles.Float, format);
                    var lon = double.Parse(data[(int)Template.Columns.Longitude.Index], NumberStyles.Float, format);
                    var timestamp = int.Parse(data[(int)Template.Columns.Timestamp.Index]);
                    var traceNumber = Template.Columns.TraceNumber != null && Template.Columns.TraceNumber.Index != null ? int.Parse(data[(int)Template.Columns.TraceNumber.Index]) : traceCount;
                    var isRowHasTime = DateTime.TryParse(data[(int)Template.Columns.Time.Index], out _);
                    if (!isRowHasTime)
                        if (firstDateTime != null)
                        {
                            var dateTime = firstDateTime.Value.AddMilliseconds(timestamp - timestampOfTheFirsDatetTime);
                            coordinates.Add(new GeoCoordinates(dateTime, lat, lon, traceNumber));
                        }
                        else
                            continue;
                    else
                    {
                        var date = ParseDateTime(data);
                        if (firstDateTime == null)
                        {
                            firstDateTime = date;
                            timestampOfTheFirsDatetTime = timestamp;
                        }
                        coordinates.Add(new GeoCoordinates(date, lat, lon, traceNumber));
                    }
                    traceCount++;
                }
            }
            return coordinates;
        }

        public override Result CreatePpkCorrectedFile(string oldFile, string newFile, IEnumerable<GeoCoordinates> coordinates, CancellationTokenSource token)
        {
            if (!File.Exists(oldFile))
                throw new FileNotFoundException("File {oldFile} does not exist");
            if (Template == null)
                throw new NullReferenceException($"Template is not set");
            var result = new Result();
            var format = new CultureInfo("en-US", false);
            format.NumberFormat.NumberDecimalSeparator = Template.Format.DecimalSeparator;
            using StreamReader reader = File.OpenText(oldFile);
            string line;
            var traceCount = 0;
            var dict = coordinates.ToDictionary(k => k.TraceNumber);
            using (StreamWriter ppkFile = new StreamWriter(newFile))
            {
                ppkFile.WriteLine(skippedLines.ToString());
                while ((line = reader.ReadLine()) != null)
                {
                    if (token.IsCancellationRequested)
                        break;
                    try
                    {
                        if (line.StartsWith(Template.Format.CommentPrefix))
                            continue;
                        var data = line.Split(new[] { Template.Format.Separator }, StringSplitOptions.None);
                        var traceNumber = Template.Columns.TraceNumber != null && Template.Columns.TraceNumber.Index != null ? int.Parse(data[(int)Template.Columns.TraceNumber.Index]) : traceCount;
                        var coordinateFound = dict.TryGetValue(traceNumber, out GeoCoordinates coordinate);
                        if (coordinateFound)
                        {
                            data[(int)Template.Columns.Longitude.Index] = dict[traceNumber].Longitude.ToString(format);
                            data[(int)Template.Columns.Latitude.Index] = dict[traceNumber].Latitude.ToString(format);
                            data[(int)Template.Columns.Date.Index] = dict[traceNumber].DateTime.Date.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                            data[(int)Template.Columns.Time.Index] = dict[traceNumber].DateTime.TimeOfDay.ToString("hh\\:mm\\:ss\\.fff");
                            ppkFile.WriteLine(string.Join(Template.Format.Separator, data));
                            result.CountOfReplacedLines++;
                        }
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        result.CountOfLines++;
                        CountOfReplacedLines++;
                        traceCount++;
                    }
                }
            }

            return result;
        }
    }
}