using FileParsers.Yaml;

namespace UgCSPPK.Models
{
    public class PositioningSolutionFile : DataFile
    {
        public PositioningSolutionFile(string filePath, Template template) : base(filePath, template)
        {
        }
    }
}