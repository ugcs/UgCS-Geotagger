using System.Collections.Generic;
using System.Threading;

namespace FileParsers
{
    public interface IGeoCoordinateParser
    {
        List<GeoCoordinates> Parse(string path);

        Result CreatePpkCorrectedFile(string oldFile, string newFile, IEnumerable<GeoCoordinates> coordinates, CancellationTokenSource token);
    }
}