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

        public override List<IGeoCoordinates> Parse(string logPath)
        {
            if (Template == null)
                throw new NullReferenceException("Template is not set");
            if (!File.Exists(logPath))
                throw new FileNotFoundException($"File {logPath} does not exist");
            var coordinates = new List<IGeoCoordinates>();
            format = new CultureInfo("en-US", false);
            format.NumberFormat.NumberDecimalSeparator = Template.Format.DecimalSeparator;
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
                    var date = ParseDateTime(data.ToArray());
                    var lat = ParseDouble(Template.DataMapping.Latitude, data[(int)Template.DataMapping.Latitude.Index]);
                    var lon = ParseDouble(Template.DataMapping.Longitude, data[(int)Template.DataMapping.Longitude.Index]);
                    var alt = Template.DataMapping.Altitude != null && Template.DataMapping.Altitude.Index != null ?
                        ParseDouble(Template.DataMapping.Altitude, data[(int)Template.DataMapping.Altitude.Index]) : 0.00;
                    coordinates.Add(new GeoCoordinates(date, lat, lon, alt));
                }
            }
            return coordinates;
        }

        public override Result CreatePpkCorrectedFile(string oldFile, string newFile, IEnumerable<IGeoCoordinates> coordinates, CancellationTokenSource token)
        {
            return new Result();
        }
    }
}