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
    public class NmeaCsvParser : CsvParser
    {
        public NmeaCsvParser(Template template) : base(template)
        {
        }

        public override List<IGeoCoordinates> Parse(string logPath)
        {
            if (!File.Exists(logPath))
                throw new FileNotFoundException("File {logPath} does not exist");
            if (Template == null)
                throw new NullReferenceException($"Template is not set");
            if (Template.DataMapping.Date?.Source == Yaml.Data.Source.FileName)
                ParseDateFromNameOfFile(logPath);
            var coordinates = new List<IGeoCoordinates>();
            using (StreamReader reader = File.OpenText(logPath))
            {
                string line = SkipLines(reader);
                format = new CultureInfo("en-US", false);
                format.NumberFormat.NumberDecimalSeparator = Template.Format.DecimalSeparator;
                var traceCount = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith(Template.Format.CommentPrefix))
                        continue;
                    var data = line.Split(new[] { Template.Format.Separator }, StringSplitOptions.None);
                    var date = ParseDateTime(data);
                    var nmeaCoordinate = new NMEACoordinates()
                    {
                        Format = format
                    };
                    nmeaCoordinate.ParseNMEAMessage(data[Template.DataMapping.Longitude.Index.Value]);
                    nmeaCoordinate.TraceNumber = traceCount;
                    nmeaCoordinate.DateTime = date;
                    traceCount++;
                    coordinates.Add(nmeaCoordinate);
                }
            }
            return coordinates;
        }

        public override Result CreateFileWithCorrectedCoordinates(string oldFile, string newFile, IEnumerable<IGeoCoordinates> coordinates, CancellationTokenSource token)
        {
            if (!File.Exists(oldFile))
                throw new FileNotFoundException("File {oldFile} does not exist");
            if (Template == null)
                throw new NullReferenceException($"Template is not set");
            var result = new Result();
            format = new CultureInfo("en-US", false);
            format.NumberFormat.NumberDecimalSeparator = Template.Format.DecimalSeparator;
            using StreamReader reader = File.OpenText(oldFile);
            string line;
            var traceCount = 0;
            var dict = coordinates.ToDictionary(k => k.TraceNumber);
            using (StreamWriter correctedFile = new StreamWriter(newFile))
            {
                if (Template.SkipLinesTo != null)
                {
                    line = SkipLines(reader);
                    correctedFile.WriteLine(skippedLines.ToString().TrimEnd(new char[] { '\n' }));
                }
                if (Template.Format.HasHeader)
                {
                    line = reader.ReadLine();
                    correctedFile.WriteLine(Regex.Replace(line, @"\s", ""));
                }
                while ((line = reader.ReadLine()) != null)
                {
                    if (token.IsCancellationRequested)
                        break;
                    try
                    {
                        if (line.StartsWith(Template.Format.CommentPrefix))
                            continue;
                        var data = line.Split(new[] { Template.Format.Separator }, StringSplitOptions.None);
                        var traceNumber = traceCount;
                        var coordinateFound = dict.TryGetValue(traceNumber, out IGeoCoordinates coordinate);
                        if (coordinateFound)
                        {
                            NMEACoordinates nmeaCoordinate = coordinate as NMEACoordinates;
                            nmeaCoordinate.SetNewNmeaCoordinates();
                            var checksum = nmeaCoordinate.CalculateCheckSum();
                            data[(int)Template.DataMapping.Longitude.Index] = nmeaCoordinate.CreateNMEAMessage(checksum.ToString("X"));
                            correctedFile.WriteLine(string.Join(Template.Format.Separator, data));
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