using YamlDotNet.Serialization;

namespace UgCSPPK.Models.Yaml
{
    public class Columns
    {
        [YamlMember(Alias = "latitude")]
        public Column Latitude { get; set; }

        [YamlMember(Alias = "longitude")]
        public Column Longitude { get; set; }

        [YamlMember(Alias = "timestamp")]
        public Column Timestamp { get; set; }
        [YamlMember(Alias = "trace-number")]
        public Column TraceNumber { get; set; }
    }
}