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

        [YamlMember(Alias = "has-complete-date")]
        public bool HasCompleteDate { get; set; }

        [YamlMember(Alias = "date-regex")]
        public string DateFormatRegex { get; set; }

        [YamlMember(Alias = "column-lengths")]
        public List<ushort> ColumnLengths { get; set; }

        public bool IsFormatValid(FileType type, Columns columns)
        {
            return type switch
            {
                FileType.CSV => IsDecimalSeparatorValid() && IsDateFieldsValid() && Separator != null && CommentPrefix != null && columns != null && (HasHeader ? (!string.IsNullOrWhiteSpace(columns.Latitude?.Header)
                && !string.IsNullOrWhiteSpace(columns.Longitude?.Header) && !string.IsNullOrWhiteSpace(columns.TraceNumber?.Header) && !string.IsNullOrWhiteSpace(columns.Timestamp?.Header)
                && columns.Latitude?.Index == null && columns.Longitude?.Index == null && columns.Timestamp?.Index == null && columns.TraceNumber?.Index == null)
                : (columns.Latitude?.Index != null && columns.Longitude?.Index != null && columns.Timestamp?.Index != null && columns.TraceNumber?.Index != null) &&
                string.IsNullOrWhiteSpace(columns.Longitude?.Header) && string.IsNullOrWhiteSpace(columns.TraceNumber?.Header) && string.IsNullOrWhiteSpace(columns.Timestamp?.Header)),
                FileType.ColumnsFixedWidth => IsDecimalSeparatorValid() && ColumnLengths != null && CommentPrefix != null && columns != null && columns.Latitude?.Index != null &&
                columns.Longitude?.Index != null && columns.Timestamp?.Index != null,
                _ => false,
            };
        }

        public bool IsDateFieldsValid()
        {
            return !HasCompleteDate || !string.IsNullOrWhiteSpace(DateFormatRegex);
        }

        public bool IsDecimalSeparatorValid()
        {
            return DecimalSeparator != null && (DecimalSeparator.Equals(".") || DecimalSeparator.Equals(","));
        }
    }
}