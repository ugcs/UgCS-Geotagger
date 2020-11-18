using FileParsers.Yaml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace FileParsers.FixedColumnWidth
{
    public class FixedColumnWidthParser : Parser
    {
        public FixedColumnWidthParser(Template template) : base(template)
        {
        }

        public override List<GeoCoordinates> Parse(string logPath)
        {
            if (Template == null)
                throw new NullReferenceException("Template is not set");
            if (!File.Exists(logPath))
                throw new FileNotFoundException($"File {logPath} does not exist");
            var coordinates = new List<GeoCoordinates>();
            var format = new CultureInfo("en-US", false).NumberFormat;
            format.NumberDecimalSeparator = Template.Format.DecimalSeparator;
            using (StreamReader reader = File.OpenText(logPath))
            {
                string line = SkipLines(reader);
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith(Template.Format.CommentPrefix) || string.IsNullOrWhiteSpace(line))
                        continue;
                    var data = new List<string>();
                    foreach (var col in Template.Format.ColumnLengths)
                    {
                        var column = line.Substring(0, col);
                        data.Add(column);
                        line = line.Substring(col);
                    }
                    var date = DateTime.Parse(data[(int)Template.Columns.DateTime.Index]);
                    var lat = double.Parse(data[(int)Template.Columns.Latitude.Index], NumberStyles.Float, format);
                    var lon = double.Parse(data[(int)Template.Columns.Longitude.Index], NumberStyles.Float, format);
                    coordinates.Add(new GeoCoordinates(date, lat, lon));
                }
            }
            return coordinates;
        }

        public override Result CreatePpkCorrectedFile(string oldFile, string newFile, IEnumerable<GeoCoordinates> coordinates, CancellationTokenSource token)
        {
            return new Result();
        }
    }
}