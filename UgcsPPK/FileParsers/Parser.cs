using System.Collections.Generic;

namespace FileParsers
{
    public abstract class Parser : IGeoCoordinateParser
    {
        public string CommentPrefix { get; set; }
        public string DecimalSeparator { get; set; }
        public ushort DateIndex { get; set; }
        public ushort LongitudeIndex { get; set; }
        public ushort LatitudeIndex { get; set; }
        public string Separator { get; set; }
        public string DateColumnName { get; set; }
        public string LongitudeColumnName { get; set; }
        public string LatitudeColumnName { get; set; }
        public bool HasHeader { get; set; }
        public List<ushort> ColumnLengths { get; set; }

        public abstract List<GeoCoordinates> Parse(string path);
    }
}