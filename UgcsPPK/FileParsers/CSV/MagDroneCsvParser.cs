using FileParsers.Yaml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace FileParsers.CSV
{
    public class MagDroneCsvParser : CsvParser
    {
        public MagDroneCsvParser(Template template) : base(template)
        {
        }

        public override List<GeoCoordinates> Parse(string logPath)
        {
            if (!File.Exists(logPath))
                throw new FileNotFoundException($"File {logPath} does not exist");
            var logName = Path.GetFileName(logPath);
            DateTime currentDate = HasCompleteDate ? DateTime.Now : ParseCurrentDate(logName);
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
                var traceCount = 0;
                var closestTime = DateTime.Now;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith(CommentPrefix))
                        continue;
                    var data = line.Split(new[] { Separator }, StringSplitOptions.None);
                    var lat = double.Parse(data[LatitudeIndex], NumberStyles.Float, format);
                    var lon = double.Parse(data[LongitudeIndex], NumberStyles.Float, format);
                    var traceNumber = TraceNumberIndex != -1 ? int.Parse(data[TraceNumberIndex]) : traceCount;
                    traceCount++;
                    var timestamp = int.Parse(data[0]);
                    DateTime date;
                    var isRowHasTime = DateTime.TryParse(data[DateIndex], out date);
                    date = isRowHasTime ? date : closestTime.AddMilliseconds(timestamp);
                    if (isRowHasTime)
                        closestTime = date;
                    if (!HasCompleteDate)
                    {
                        var totalMS = date.Second * 1000 + date.Minute * 60000 + date.Hour * 3600000 + date.Millisecond;
                        if (previousTime > totalMS)
                            currentDate.AddDays(1);
                        var currentDateAndTime = currentDate.AddMilliseconds(totalMS);
                        previousTime = totalMS;
                        coordinates.Add(new GeoCoordinates(currentDateAndTime, lat, lon, traceNumber));
                    }
                    else
                        coordinates.Add(new GeoCoordinates(date, lat, lon));
                }
            }
            return coordinates;
        }
    }
}
