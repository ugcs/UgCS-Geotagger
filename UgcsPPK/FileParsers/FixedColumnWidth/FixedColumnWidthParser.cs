using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace FileParsers.FixedColumnWidth
{
    public class FixedColumnWidthParser : Parser
    {
        public FixedColumnWidthParser()
        {
        }

        public override List<GeoCoordinates> Parse(string logPath)
        {
            try
            {
                if (!File.Exists(logPath))
                    throw new Exception($"File {logPath} does not exist");
                var coordinates = new List<GeoCoordinates>();
                var decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                var format = new CultureInfo("en-US", false).NumberFormat;
                format.NumberDecimalSeparator = DecimalSeparator;
                using (StreamReader reader = File.OpenText(logPath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith(CommentPrefix) || string.IsNullOrWhiteSpace(line))
                            continue;
                        var data = new List<string>();
                        foreach (var col in ColumnLengths)
                        {
                            var column = line.Substring(0, col);
                            data.Add(column);
                            line = line.Substring(col);
                        }
                        var date = DateTime.Parse(data[DateIndex]);
                        var lat = double.Parse(data[LatitudeIndex], NumberStyles.Float, format);
                        var lon = double.Parse(data[LongitudeIndex], NumberStyles.Float, format);
                        coordinates.Add(new GeoCoordinates(date, lat, lon));
                    }
                }
                return coordinates;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public override void CreatePpkCorrectedFile(string oldFile, string newFile, IEnumerable<GeoCoordinates> coordinates)
        {
            
        }
    }
}