using YamlDotNet.Serialization;

namespace FileParsers.Yaml.Data
{
    public class DateTime : BaseData
    {
        [YamlMember(Alias = "format")]
        public string Format { get; set; }
    }
}