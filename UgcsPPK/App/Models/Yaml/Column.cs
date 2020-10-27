using YamlDotNet.Serialization;

namespace UgCSPPK.Models.Yaml
{
    public class Column
    {
        [YamlMember(Alias = "header")]
        public string Header { get; set; }

        [YamlMember(Alias = "index")]
        public int? Index { get; set; }
    }
}