namespace Tests
{
    using YamlDotNet.Serialization;

    public class Test
    {
        protected const string TestPassed = "Test Passed";
        protected const string TestFailed = "Test Failed";
        protected const string YamlTestDataFolder = "./TestData/Yaml/";
        protected const string YamlCsvFolder = "CSV/";
        protected const string YamlNmeaFolder = "Nmea/";
        protected const string YamlMagdroneFolder = "MagDrone/";
        protected const string YamlMagarrowFolder = "MagArrow/";
        protected const string ColumnFixedWidthFolder = "ColumnFixedWidth/";
        protected const string FTUTemplatesFolder = "./Mapping/FTUTemplates/";
        protected const string PSFTemplatesFolder = "./Mapping/PSFTemplates/";
        protected const string TemplatesFolder = "./Mapping/";
        protected const string SegyTestDataFolder = "./TestData/Parsers/Segy/";
        protected const string CSVTestDataFolder = "./TestData/Parsers/CSV/";
        protected const string FCWTestDataFolder = "./TestData/Parsers/FixedColumnWidth/";
        protected readonly Deserializer _deserializer = new Deserializer();
    }
}