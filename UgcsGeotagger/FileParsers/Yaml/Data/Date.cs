using System.ComponentModel;
using YamlDotNet.Serialization;

namespace FileParsers.Yaml.Data
{
    public class Date : DateTime
    {
        [YamlMember(Alias = "source")]
        public Source Source { get; set; }
    }

    [DefaultValue(Column)]
    public enum Source
    {
        Column,
        FileName
    };
}