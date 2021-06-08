using FileParsers.Yaml;

namespace FileParsers.CSV
{
    public class CSVParsersFactory
    {
        private const string MagDrone = "magdrone";
        private const string NMEA = "nmea";

        public CsvParser CreateCSVParser(Template template)
        {
            return template.Code switch
            {
                MagDrone => new MagDroneCsvParser(template),
                NMEA => new NmeaCsvParser(template),
                _ => new CsvParser(template),
            };
        }
    }
}