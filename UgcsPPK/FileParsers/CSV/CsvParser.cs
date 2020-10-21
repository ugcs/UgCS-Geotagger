using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
                throw new Exception($"File {logPath} does not exist");
            var logName = Path.GetFileName(logPath);
            DateTime currentDate = ParseCurrentDate(logName);
            using (var reader = new StreamReader(logPath))
            using (var csv = new CsvReader(reader))
            {
                var format = new CultureInfo("en-US", false);
                format.NumberFormat.NumberDecimalSeparator = DecimalSeparator;
                csv.Configuration.CultureInfo = format;
                csv.Configuration.Delimiter = Separator;
                GeoCoordinatesMap.HasHeader = HasHeader;
                if (HasHeader)
                {
                    GeoCoordinatesMap.TimeColumnName = DateColumnName;
                    GeoCoordinatesMap.LatitudeColumnName = LatitudeColumnName;
                    GeoCoordinatesMap.LongitudeColumnName = LongitudeColumnName;
                }
                else
                {
                    GeoCoordinatesMap.TimeIndex = DateIndex;
                    GeoCoordinatesMap.LatitudeIndex = LatitudeIndex;
                    GeoCoordinatesMap.LongitudeIndex = LongitudeIndex;
                }

                csv.Configuration.RegisterClassMap<GeoCoordinatesMap>();
                var coordinates = csv.GetRecords<GeoCoordinates>().ToList();
                double previousTime = 0;
                foreach (var coordinate in coordinates)
                {
                    var timeOfTheCurrentDay = DateTime.Parse(coordinate.Time);
                    var totalMS = timeOfTheCurrentDay.Second * 1000 + timeOfTheCurrentDay.Minute * 60000 + timeOfTheCurrentDay.Hour * 3600000 + timeOfTheCurrentDay.Millisecond;
                    if (previousTime > totalMS)
                        currentDate.AddDays(1);
                    double dateInMs = CalculateMsInCurrentDay(currentDate);
                    coordinate.TimeInMs = dateInMs + totalMS;
                    previousTime = totalMS;
                    coordinate.Date = currentDate.AddMilliseconds(totalMS);
                }
                return coordinates;
            }
        }

        private DateTime ParseCurrentDate(string logName)
        {
            Regex r = new Regex(@"\d{4}-\d{2}-\d{2}");
            Match m = r.Match(logName);
            if (!m.Success)
                throw new Exception("Incorrect file name. Set date of logging. The Format is: yyyy-MM-dd");

            return DateTime.ParseExact(m.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private double CalculateMsInCurrentDay(DateTime date)
        {
            return date.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
    }
}