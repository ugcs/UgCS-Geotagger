using FileParsers.Exceptions;
using FileParsers.Yaml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace FileParsers.CSV
{
    public class CsvParser : Parser
    {
        
        public CsvParser(Template template) : base(template)
        {
        }

        public override List<GeoCoordinates> Parse(string logPath)
        {
            if (!File.Exists(logPath))
                throw new FileNotFoundException("File {logPath} does not exist");
            if (Template == null)
                throw new NullReferenceException($"Template is not set");
            var logName = Path.GetFileName(logPath);
            if (Template.Format.HasFileNameDate)
                ParseDateFromNameOfFile(logPath);
            var coordinates = new List<GeoCoordinates>();
            using (StreamReader reader = File.OpenText(logPath))
            {
                string line = SkipLines(reader);
                if (Template.Format.HasHeader)
                {
                    if (line == null)
                        line = reader.ReadLine();
                    FindIndexesByHeaders(line);
                    skippedLines.Append(line + "\n");
                }
                var format = new CultureInfo("en-US", false);
                format.NumberFormat.NumberDecimalSeparator = Template.Format.DecimalSeparator;
                var traceCount = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith(Template.Format.CommentPrefix))
                        continue;
                    var data = line.Split(new[] { Template.Format.Separator }, StringSplitOptions.None);
                    var lat = double.Parse(data[(int)Template.Columns.Latitude.Index], NumberStyles.Float, format);
                    var lon = double.Parse(data[(int)Template.Columns.Longitude.Index], NumberStyles.Float, format);
                    var traceNumber = Template.Columns.TraceNumber != null && Template.Columns.TraceNumber.Index != null ? int.Parse(data[(int)Template.Columns.TraceNumber.Index]) : traceCount;
                    traceCount++;
                    var date = ParseDateTime(data);
                    coordinates.Add(new GeoCoordinates(date, lat, lon, traceNumber));
                }
            }
            return coordinates;
        }

        protected void ParseDateFromNameOfFile(string logName)
        {
            Regex r = Template.Format.DateFormatRegex != null ? new Regex(Template.Format.DateFormatRegex) : new Regex(@"\d{4}-\d{2}-\d{2}");
            Match m = r.Match(logName);
            if (!m.Success)
                throw new IncorrectDateFormatException("Incorrect file name. Set date of logging.");

            dateFromNameOfFile = DateTime.ParseExact(m.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        protected DateTime ParseDateTime(string[] data)
        {
            if (Template.Columns.DateTime != null && Template.Columns.DateTime?.Index != -1)
            {
                return DateTime.Parse(data[(int)Template.Columns.DateTime.Index]);
            }
            else if (Template.Columns.Time != null && Template.Columns.Time.Index != -1 && Template.Columns.Date != null && Template.Columns.Date.Index != -1)
            {
                var date = DateTime.Parse(data[(int)Template.Columns.Date.Index]);
                var time = DateTime.Parse(data[(int)Template.Columns.Time.Index]);
                var totalMS = time.Second * 1000 + time.Minute * 60000 + time.Hour * 3600000 + time.Millisecond;
                var dateTime = date.AddMilliseconds(totalMS);
                return dateTime;
            }
            else if (Template.Columns.Time != null && Template.Columns.Time.Index != -1 && dateFromNameOfFile != null)
            {
                var time = DateTime.Parse(data[(int)Template.Columns.Time.Index]);
                var totalMS = time.Second * 1000 + time.Minute * 60000 + time.Hour * 3600000 + time.Millisecond;
                var dateTime = dateFromNameOfFile.Value.AddMilliseconds(totalMS);
                return dateTime;
            }
            else
                throw new IncorrectDateFormatException("Cannot parse DateTime form file");
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
                        var lon = coordinates.Where(c => c.TraceNumber == traceNumber).FirstOrDefault();
                        var lat = coordinates.Where(c => c.TraceNumber == traceNumber).FirstOrDefault();
                        if (lat != null && lon != null)
                        {
                            data[(int)Template.Columns.Longitude.Index] = coordinates.Where(c => c.TraceNumber == traceNumber).FirstOrDefault().Longitude.ToString(format);
                            data[(int)Template.Columns.Latitude.Index] = coordinates.Where(c => c.TraceNumber == traceNumber).FirstOrDefault().Latitude.ToString(format);
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

        protected void FindIndexesByHeaders(string line)
        {
            List<string> headers;
            headers = line.Split(new[] { Template.Format.Separator }, StringSplitOptions.None).ToList();
            Template.Columns.Latitude.Index = headers.FindIndex(h => h.Equals(Template.Columns.Latitude.Header));
            Template.Columns.Longitude.Index = headers.FindIndex(h => h.Equals(Template.Columns.Longitude.Header));
            if (Template.Columns.Date != null && Template.Columns.Date.Header != null)
                Template.Columns.Date.Index = headers.FindIndex(h => h.Equals(Template.Columns.Date.Header));
            if (Template.Columns.Time != null && Template.Columns.Time.Header != null)
                Template.Columns.Time.Index = headers.FindIndex(h => h.Equals(Template.Columns.Time.Header));
            if (Template.Columns.DateTime != null && Template.Columns.DateTime.Header != null)
                Template.Columns.DateTime.Index = headers.FindIndex(h => h.Equals(Template.Columns.Time.Header));
            if (Template.Columns.Timestamp != null && Template.Columns.Timestamp.Header != null)
                Template.Columns.Timestamp.Index = headers.FindIndex(h => h.Equals(Template.Columns.Timestamp.Header));
            if (Template.Columns.TraceNumber != null && Template.Columns.TraceNumber.Header != null)
                Template.Columns.TraceNumber.Index = headers.FindIndex(h => h.Equals(Template.Columns.TraceNumber.Header));

            if (Template.Columns.Latitude.Index == -1 || Template.Columns.Longitude.Index == -1)
                throw new ColumnsMatchingException("Column names are not matched");
        }
    }
}