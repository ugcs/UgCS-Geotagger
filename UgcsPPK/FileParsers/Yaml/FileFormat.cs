using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace FileParsers.Yaml
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

        [YamlMember(Alias = "has-file-name-date")]
        public bool HasFileNameDate { get; set; }

        [YamlMember(Alias = "date-regex")]
        public string DateFormatRegex { get; set; }

        [YamlMember(Alias = "column-lengths")]
        public List<ushort> ColumnLengths { get; set; }

        public bool IsFormatValid(FileType type, Columns columns)
        {
            return type switch
            {
                FileType.CSV => IsDecimalSeparatorValid() && Separator != null && CommentPrefix != null && columns != null && IsDateTimeColumnsValid(columns) && (HasHeader ? (!string.IsNullOrWhiteSpace(columns.Latitude?.Header)
                && !string.IsNullOrWhiteSpace(columns.Longitude?.Header)
                && columns.Latitude?.Index == null && columns.Longitude?.Index == null)
                : (columns.Latitude?.Index != null && columns.Longitude?.Index != null) &&
                string.IsNullOrWhiteSpace(columns.Longitude?.Header)),
                FileType.ColumnsFixedWidth => IsDecimalSeparatorValid() && ColumnLengths != null && CommentPrefix != null && columns != null && columns.Latitude?.Index != null &&
                columns.Longitude?.Index != null,
                _ => false,
            };
        }

        private bool IsDateTimeColumnsValid(Columns columns)
        {
            return (!(string.IsNullOrEmpty(DateFormatRegex)) && ((HasFileNameDate && !(string.IsNullOrWhiteSpace(columns.Time?.Header) && columns.Time?.Index == null)) ||
                (HasFileNameDate && (string.IsNullOrWhiteSpace(columns.Time?.Header) && columns.Time?.Index != null)))) ||
                ((!(string.IsNullOrWhiteSpace(columns.DateTime?.Header) && columns.DateTime?.Index == null)) ||
                ((string.IsNullOrWhiteSpace(columns.DateTime?.Header) && columns.DateTime?.Index != null))) ||
                ((!(string.IsNullOrWhiteSpace(columns.Time?.Header) && columns.Time?.Index == null)) &&
                ((!string.IsNullOrWhiteSpace(columns.Date?.Header) && columns.Date?.Index == null)) ||
                ((string.IsNullOrWhiteSpace(columns.Time?.Header) && columns.Time?.Index != null)) &&
                (string.IsNullOrWhiteSpace(columns.Date?.Header) && columns.Date?.Index != null));
        }

        private bool IsDecimalSeparatorValid()
        {
            return DecimalSeparator != null && (DecimalSeparator.Equals(".") || DecimalSeparator.Equals(","));
        }
    }
}