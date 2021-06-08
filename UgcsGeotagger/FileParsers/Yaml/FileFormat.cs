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

        [YamlMember(Alias = "column-lengths")]
        public List<ushort> ColumnLengths { get; set; }

        public bool IsFormatValid(FileType type, DataMapping columns)
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

        private bool IsDateTimeColumnsValid(DataMapping columns)
        {
            return (!string.IsNullOrEmpty(columns.Date?.Format) && !string.IsNullOrEmpty(columns.Time?.Format) && ((columns.Date?.Source == Data.Source.FileName
                && !(string.IsNullOrWhiteSpace(columns.Time?.Header) && columns.Time?.Index == null)) ||
                (columns.Date?.Source == Data.Source.FileName && string.IsNullOrWhiteSpace(columns.Time?.Header) && columns.Time?.Index != null))) ||
                (!(string.IsNullOrWhiteSpace(columns.DateTime?.Header) && columns.DateTime?.Index == null)) ||
                string.IsNullOrWhiteSpace(columns.DateTime?.Header) && columns.DateTime?.Index != null && !string.IsNullOrEmpty(columns.DateTime?.Format) ||
                (((!(string.IsNullOrWhiteSpace(columns.Time?.Header) && columns.Time?.Index == null)) &&
                !string.IsNullOrWhiteSpace(columns.Date?.Header) && columns.Date?.Index == null) ||
                (string.IsNullOrWhiteSpace(columns.Time?.Header) && columns.Time?.Index != null) &&
                (string.IsNullOrWhiteSpace(columns.Date?.Header) && columns.Date?.Index != null)
                && !string.IsNullOrEmpty(columns.Date?.Format) && !string.IsNullOrEmpty(columns.Time?.Format));
        }

        private bool IsDecimalSeparatorValid()
        {
            return DecimalSeparator != null && (DecimalSeparator.Equals(".") || DecimalSeparator.Equals(","));
        }
    }
}