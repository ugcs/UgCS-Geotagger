using System.Collections.Generic;

namespace FileParsers
{
    public interface IGeoCoordinateParser
    {
        List<GeoCoordinates> Parse(string path);

        Result CreatePpkCorrectedFile(string oldFile, string newFile, IEnumerable<GeoCoordinates> coordinates);
    }
}