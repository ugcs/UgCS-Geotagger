using System;

namespace FileParsers
{
    public class GeoCoordinates : IGeoCoordinates
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double TimeInMs { get; set; }
        public int TraceNumber { get; set; }
        private DateTime _dateTime;

        public DateTime DateTime
        {
            get
            {
                return _dateTime;
            }
            set
            {
                _dateTime = value;
                TimeInMs = _dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            }
        }

        public GeoCoordinates(DateTime dateTime, double latitide, double longitude, double altitude, int traceNumber)
        {
            DateTime = dateTime;
            Latitude = latitide;
            Longitude = longitude;
            Altitude = altitude;
            TraceNumber = traceNumber;
        }

        public GeoCoordinates(DateTime date, double latitide, double longitude, double altitude)
        {
            DateTime = date;
            Latitude = latitide;
            Longitude = longitude;
            Altitude = altitude;
        }

        public GeoCoordinates(double latitide, double longitude)
        {
            Latitude = latitide;
            Longitude = longitude;
        }

        public GeoCoordinates(double latitide, double longitude, double altitude)
        {
            Latitude = latitide;
            Longitude = longitude;
            Altitude = altitude;
        }

        public GeoCoordinates()
        {
        }
    }
}