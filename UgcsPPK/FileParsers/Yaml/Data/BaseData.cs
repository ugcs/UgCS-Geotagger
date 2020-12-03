using YamlDotNet.Serialization;

namespace FileParsers.Yaml.Data
{
    public class BaseData
    {
        [YamlMember(Alias = "header")]
        public string Header { get; set; }

        [YamlMember(Alias = "index")]
        public int? Index { get; set; }

        [YamlMember(Alias = "regex")]
        public string Regex { get; set; }
    }
}