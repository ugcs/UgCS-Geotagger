using FileParsers.Yaml.Data;
using YamlDotNet.Serialization;

namespace FileParsers.Yaml
{
    public class DataMapping
    {
        [YamlMember(Alias = "latitude")]
        public BaseData Latitude { get; set; }

        [YamlMember(Alias = "longitude")]
        public BaseData Longitude { get; set; }

        [YamlMember(Alias = "altitude")]
        public BaseData Altitude { get; set; }

        [YamlMember(Alias = "date")]
        public Date Date { get; set; }

        [YamlMember(Alias = "time")]
        public DateTime Time { get; set; }

        [YamlMember(Alias = "date-time")]
        public DateTime DateTime { get; set; }

        [YamlMember(Alias = "timestamp")]
        public BaseData Timestamp { get; set; }

        [YamlMember(Alias = "trace-number")]
        public BaseData TraceNumber { get; set; }
    }
}