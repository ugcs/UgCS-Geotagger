using System;

namespace FileParsers
{
    public class GeoCoordinates
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double TimeInMs { get; set; }
        public int TraceNumber { get; set; }
        public DateTime Date { get; set; }

        public GeoCoordinates(DateTime date, double latitide, double longitude, int traceNumber)
        {
            Date = date;
            TimeInMs = date.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            Latitude = latitide;
            Longitude = longitude;
            TraceNumber = traceNumber;
        }

        public GeoCoordinates(DateTime date, double latitide, double longitude)
        {
            Date = date;
            TimeInMs = date.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            Latitude = latitide;
            Longitude = longitude;
        }

        public GeoCoordinates(double latitide, double longitude)
        {
            Latitude = latitide;
            Longitude = longitude;
        }

        public GeoCoordinates()
        {
        }
    }
}