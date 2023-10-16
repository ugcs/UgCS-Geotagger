using FileParsers.Exceptions;
using FileParsers.Yaml;
using FileParsers.Yaml.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace FileParsers
{
    public abstract class Parser : IGeoCoordinateParser
    {
        protected StringBuilder skippedLines;
        protected System.DateTime? dateFromNameOfFile;
        protected CultureInfo format;
        public Template Template { get; }
        private int _countOfReplacedLines;

        public int CountOfReplacedLines
        {
            get => _countOfReplacedLines;
            set
            {
                _countOfReplacedLines = value;
                if (CountOfReplacedLines % 100 == 0)
                {
                    OnOneHundredLinesReplaced?.Invoke(CountOfReplacedLines);
                }
            }
        }

        public event Action<int> OnOneHundredLinesReplaced;

        public abstract List<IGeoCoordinates> Parse(string path);

        public abstract Result CreateFileWithCorrectedCoordinates(string oldFile, string newFile, IEnumerable<IGeoCoordinates> coordinates, CancellationTokenSource token);

        public Parser(Template template)
        {
            Template = template;
        }

        protected string SkipLines(StreamReader reader)
        {
            string line;
            skippedLines = new StringBuilder();
            if (Template.SkipLinesTo != null)
            {
                while ((line = reader.ReadLine()) != null)
                {
                    skippedLines.Append(line + "\n");
                    var regex = new Regex(Template.SkipLinesTo.MatchRegex);
                    if (regex.IsMatch(line))
                    {
                        break;
                    }
                }

                if (Template.SkipLinesTo.SkipMatchedLine)
                {
                    line = reader.ReadLine();
                    skippedLines.Append(line + "\n");
                    return line;
                }

                return line;
            }

            return null;
        }

        protected double? ParseDouble(BaseData data, string column)
        {
            double result;
            if (!string.IsNullOrEmpty(data.Regex))
            {
                var match = FindByRegex(data.Regex, column);
                if (match.Success)
                {
                    if (double.TryParse(match.Value, NumberStyles.Float, format, out result))
                    {
                        return result;
                    }

                    return null;
                }
            }

            if (double.TryParse(column, NumberStyles.Float, format, out result))
            {
                return result;
            }

            return null;
        }

        protected int? ParseInt(BaseData data, string column)
        {
            int result;
            if (!string.IsNullOrEmpty(data.Regex))
            {
                var match = FindByRegex(data.Regex, column);
                if (match.Success)
                {
                    if (int.TryParse(match.Value, out result))
                    {
                        return result;
                    }

                    return null;
                }
            }

            if (int.TryParse(column, out result))
            {
                return result;
            }

            return null;
        }

        protected System.DateTime? ParseDateTime(string[] data)
        {
            if (Template.DataMapping.DateTime != null && Template.DataMapping.DateTime?.Index != -1)
            {
                return Template.DataMapping.DateTime.Type switch
                {
                    Yaml.Data.Type.GPST => GpsToUTC(data[(int)Template.DataMapping.DateTime.Index]),
                    _ => ParseDateAndTime(Template.DataMapping.DateTime, data[(int)Template.DataMapping.DateTime.Index]),
                };
            }

            if (Template.DataMapping.Time != null && Template.DataMapping.Time.Index != -1 && Template.DataMapping?.Date != null && Template.DataMapping.Date?.Index != null)
            {
                var date = ParseDateAndTime(Template.DataMapping.Date, data[(int)Template.DataMapping.Date.Index]);
                var time = ParseDateAndTime(Template.DataMapping.Time, data[(int)Template.DataMapping.Time.Index]);
                var totalMS = CalculateTotalMS(time.Value);
                var dateTime = date?.AddMilliseconds(totalMS);
                return dateTime;
            }
            
            if (Template.DataMapping.Time != null && Template.DataMapping.Time?.Index != -1 && dateFromNameOfFile != null)
            {
                var time = ParseDateAndTime(Template.DataMapping.Time, data[(int)Template.DataMapping.Time.Index]);
                var totalMS = CalculateTotalMS(time.Value);
                var dateTime = dateFromNameOfFile.Value.AddMilliseconds(totalMS);
                return dateTime;
            }


            throw new IncorrectDateFormatException("Cannot parse DateTime form file");
        }

        // TODO: Add tests for new format
        private System.DateTime GpsToUTC(string gpsTime)
        {
            var data = gpsTime.Split(new[] { " " }, StringSplitOptions.None);
            var weeksInDays = int.Parse(data[0]);
            var secondsAndMs = double.Parse(data[1], CultureInfo.InvariantCulture);
            System.DateTime datum = new System.DateTime(1980, 1, 6, 0, 0, 0);
            System.DateTime week = datum.AddDays(weeksInDays * 7);
            System.DateTime time = week.AddSeconds(secondsAndMs);
            return time;
        }

        private System.DateTime? ParseDateAndTime(Yaml.Data.DateTime data, string column)
        {
            System.DateTime result;
            if (!string.IsNullOrEmpty(data.Regex))
            {
                var match = FindByRegex(data.Regex, column);
                if (match.Success)
                {
                    if (System.DateTime.TryParseExact(match.Value, data.Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                    {
                        return result;
                    }


                    return null;
                }
            }

            if (System.DateTime.TryParseExact(column, data.Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }

            return null;
        }

        private int CalculateTotalMS(System.DateTime time)
        {
            return time.Second * 1000 + time.Minute * 60000 + time.Hour * 3600000 + time.Millisecond;
        }

        private Match FindByRegex(string regex, string column)
        {
            var r = new Regex(regex);
            var m = r.Match(column);
            return m;
        }
    }
}