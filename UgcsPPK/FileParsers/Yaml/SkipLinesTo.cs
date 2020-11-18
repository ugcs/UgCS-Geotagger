using YamlDotNet.Serialization;

namespace FileParsers.Yaml
{
    public class SkipLinesTo
    {
        [YamlMember(Alias = "match-regex")]
        public string MatchRegex { get; set; }

        [YamlMember(Alias = "skip-matched-line")]
        public bool SkipMatchedLine { get; set; }
    }
}
