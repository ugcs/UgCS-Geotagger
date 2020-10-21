using CsvHelper.Configuration;

namespace FileParsers.CSV
{
    public class GeoCoordinatesMap : ClassMap<GeoCoordinates>
    {
        public static string LatitudeColumnName { get; set; } = "Latitude";
        public static string LongitudeColumnName { get; set; } = "Longitude";
        public static string TimeColumnName { get; set; } = "Time";
        public static ushort LatitudeIndex { get; set; } = 5;
        public static ushort LongitudeIndex { get; set; } = 6;
        public static ushort TimeIndex { get; set; } = 1;

        public static bool HasHeader { get; set; } = true;

        public GeoCoordinatesMap()
        {
            if (HasHeader)
            {
                Map(t => t.Latitude).Name(LatitudeColumnName);
                Map(t => t.Longitude).Name(LongitudeColumnName);
                Map(t => t.Time).Name(TimeColumnName);
            }
            else
            {
                Map(t => t.Latitude).Index(LatitudeIndex);
                Map(t => t.Longitude).Index(LongitudeIndex);
                Map(t => t.Time).Index(TimeIndex);
            }
        }
    }
}