using System;
using System.Collections.Generic;
using System.Threading;

namespace FileParsers
{
    public abstract class Parser : IGeoCoordinateParser
    {
        public string CommentPrefix { get; set; }
        public string DecimalSeparator { get; set; }
        public int DateIndex { get; set; }
        public int LongitudeIndex { get; set; }
        public int LatitudeIndex { get; set; }
        public int TraceNumberIndex { get; set; }
        public string Separator { get; set; }
        public string DateColumnName { get; set; }
        public string LongitudeColumnName { get; set; }
        public string LatitudeColumnName { get; set; }
        public string TraceNumberColumnName { get; set; }
        public string DateTimeRegex { get; set; }
        public bool HasHeader { get; set; }
        public List<ushort> ColumnLengths { get; set; }
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
    }
}