using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace UgCSPPK.Models.Yaml
{
    public class FileFormat
    {
        [YamlMember(Alias = "comment-prefix")]
        public string CommentPrefix { get; set; }

        [YamlMember(Alias = "separator")]
        public string Separator { get; set; }

        [YamlMember(Alias = "decimal-separator")]
        public string DecimalSeparator { get; set; }

        [YamlMember(Alias = "has-header")]
        public bool HasHeader { get; set; }

        [YamlMember(Alias = "column-lengths")]
        public List<ushort> ColumnLengths { get; set; }
    }
}