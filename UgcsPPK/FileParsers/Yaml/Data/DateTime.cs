using System.ComponentModel;
using YamlDotNet.Serialization;

namespace FileParsers.Yaml.Data
{
    public class DateTime : BaseData
    {
        [YamlMember(Alias = "format")]
        public string Format { get; set; }
        [YamlMember(Alias = "type")]
        public Type Type { get; set; }
    }

    [DefaultValue(UTC)]
    public enum Type
    {
        UTC,
        GPST
    };
}