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

        public override List<IGeoCoordinates> Parse(string logPath)
        {
            if (Template == null)
                throw new NullReferenceException("Template is not set");
            if (!File.Exists(logPath))
                throw new FileNotFoundException($"File {logPath} does not exist");
            if (Template.DataMapping.Date?.Source == Yaml.Data.Source.FileName)
                ParseDateFromNameOfFile(logPath);
            var coordinates = new List<IGeoCoordinates>();
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

                format = new CultureInfo("en-US", false);
                format.NumberFormat.NumberDecimalSeparator = Template.Format.DecimalSeparator;
                var traceCount = 0;
                DateTime? firstDateTime = null;
                var timestampOfTheFirsDatetTime = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith(Template.Format.CommentPrefix))
                        continue;
                    var data = line.Split(new[] { Template.Format.Separator }, StringSplitOptions.None);
                    var lat = ParseDouble(Template.DataMapping.Latitude, data[(int)Template.DataMapping.Latitude.Index]);
                    var lon = ParseDouble(Template.DataMapping.Longitude, data[(int)Template.DataMapping.Longitude.Index]);
                    var timestamp = ParseInt(Template.DataMapping.Timestamp, data[(int)Template.DataMapping.Timestamp.Index]);
                    var traceNumber = Template.DataMapping.TraceNumber != null && Template.DataMapping.TraceNumber.Index != null ?
                        ParseInt(Template.DataMapping.TraceNumber, data[(int)Template.DataMapping.TraceNumber.Index]) : traceCount;
                    var isRowHasTime = DateTime.TryParse(data[(int)Template.DataMapping.Time.Index], out _);
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

        public override Result CreatePpkCorrectedFile(string oldFile, string newFile, IEnumerable<IGeoCoordinates> coordinates, CancellationTokenSource token)
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
                line = SkipLines(reader);
                ppkFile.WriteLine(skippedLines.ToString().TrimEnd(new char[] { '\n' }));
                while ((line = reader.ReadLine()) != null)
                {
                    if (token.IsCancellationRequested)
                        break;
                    try
                    {
                        if (line.StartsWith(Template.Format.CommentPrefix))
                            continue;
                        var data = line.Split(new[] { Template.Format.Separator }, StringSplitOptions.None);
                        var traceNumber = Template.DataMapping.TraceNumber != null && Template.DataMapping.TraceNumber.Index != null ? int.Parse(data[(int)Template.DataMapping.TraceNumber.Index]) : traceCount;
                        var coordinateFound = dict.TryGetValue(traceNumber, out IGeoCoordinates coordinate);
                        if (coordinateFound)
                        {
                            data[(int)Template.DataMapping.Longitude.Index] = coordinate.Longitude.ToString(format);
                            data[(int)Template.DataMapping.Latitude.Index] = coordinate.Latitude.ToString(format);
                            data[(int)Template.DataMapping.Date.Index] = coordinate.DateTime.ToString(Template.DataMapping.Date.Format, CultureInfo.InvariantCulture);
                            data[(int)Template.DataMapping.Time.Index] = coordinate.DateTime.ToString(Template.DataMapping.Time.Format, CultureInfo.InvariantCulture);
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