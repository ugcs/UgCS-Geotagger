using System;

namespace FileParsers
{
    public interface IGeoCoordinates
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double TimeInMs { get;  set; }
        public int TraceNumber { get; set; }
        public DateTime DateTime { get; set; }
    }
}
