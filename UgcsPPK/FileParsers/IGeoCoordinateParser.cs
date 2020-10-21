using System.Collections.Generic;

namespace FileParsers
{
    public interface IGeoCoordinateParser
    {
        List<GeoCoordinates> Parse(string path);
    }
}