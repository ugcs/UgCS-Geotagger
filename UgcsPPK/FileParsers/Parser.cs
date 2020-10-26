using System.Collections.Generic;

namespace FileParsers
{
    public abstract class Parser : IGeoCoordinateParser
    {
        public string CommentPrefix { get; set; } = "#";
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
        public bool HasHeader { get; set; }
        public List<ushort> ColumnLengths { get; set; }
        public abstract List<GeoCoordinates> Parse(string path);
        public  abstract void CreatePpkCorrectedFile(string oldFile, string newFile, IEnumerable<GeoCoordinates> coordinates);
    }
}