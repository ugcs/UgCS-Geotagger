﻿using System.ComponentModel;
using YamlDotNet.Serialization;

namespace UgCSPPK.Models.Yaml
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
    }

    [DefaultValue(Unknown)]
    public enum FileType
    {
        CSV,
        ColumnsFixedWidth,
        Unknown
    };
}