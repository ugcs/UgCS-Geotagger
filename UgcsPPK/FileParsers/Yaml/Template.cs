using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace FileParsers.Yaml
{
    public class Template
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "code")]
        public string Code { get; set; }

        [YamlMember(Alias = "match-regex")]
        public string MatchRegex { get; set; }

        [YamlMember(Alias = "file-type")]
        public FileType FileType { get; set; }

        [YamlMember(Alias = "file-format")]
        public FileFormat Format { get; set; }

        [YamlMember(Alias = "columns")]
        public Columns Columns { get; set; }

        [YamlMember(Alias = "skip-lines-to")]
        public SkipLinesTo SkipLinesTo { get; set; }

        public bool IsTemplateValid()
        {
            return IsValidRegex() && !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Code) && FileType != FileType.Unknown && IsFormatValid();
        }

        private bool IsValidRegex()
        {
            if (string.IsNullOrWhiteSpace(MatchRegex)) return false;
            try
            {
                Regex.Match("", MatchRegex);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        private bool IsFormatValid()
        {
            return Format != null && Format.IsFormatValid(FileType, Columns);
        }
    }

    [DefaultValue(Unknown)]
    public enum FileType
    {
        CSV,
        ColumnsFixedWidth,
        Unknown
    };
}