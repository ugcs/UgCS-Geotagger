using FileParsers.Yaml;

namespace UgCSGeotagger.Models
{
    public class PositioningSolutionFile : DataFile
    {
        public PositioningSolutionFile(string filePath, Template template) : base(filePath, template)
        {
        }
    }
}