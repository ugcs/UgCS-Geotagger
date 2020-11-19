using FileParsers.Yaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace FileParsers
{
    public abstract class Parser : IGeoCoordinateParser
    {
        protected StringBuilder skippedLines;
        protected DateTime? dateFromNameOfFile;
        public Template Template { get; }
        private int _countOfReplacedLines;

        public int CountOfReplacedLines
        {
            get => _countOfReplacedLines;
            set
            {
                _countOfReplacedLines = value;
                if (CountOfReplacedLines % 100 == 0)
                    OnOneHundredLinesReplaced?.Invoke(CountOfReplacedLines);
            }
        }

        public event Action<int> OnOneHundredLinesReplaced;

        public abstract List<GeoCoordinates> Parse(string path);

        public abstract Result CreatePpkCorrectedFile(string oldFile, string newFile, IEnumerable<GeoCoordinates> coordinates, CancellationTokenSource token);

        public Parser(Template template)
        {
            Template = template;
        }

        protected string SkipLines(StreamReader reader)
        {
            string line;
            skippedLines = new StringBuilder();
            if (Template.SkipLinesTo != null)
            {
                while ((line = reader.ReadLine()) != null)
                {
                    skippedLines.Append(line + "\n");
                    var regex = new Regex(Template.SkipLinesTo.MatchRegex);
                    if (regex.IsMatch(line))
                        break;
                }
                if (Template.SkipLinesTo.SkipMatchedLine)
                {
                    line = reader.ReadLine();
                    skippedLines.Append(line + "\n");
                    return line;
                }
                else
                    return line;
            }
            return null;
        }
    }
}