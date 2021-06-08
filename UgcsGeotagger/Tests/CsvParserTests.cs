using FileParsers.CSV;
using FileParsers.Exceptions;
using FileParsers.Yaml;
using NUnit.Framework;
using System;
using System.IO;

namespace Tests
{
    public class CsvParserTests : Test
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestValidCsv()
        {
            var path = YamlTestDataFolder + YamlCsvFolder + "ValidCsvTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            var parser = new CsvParser(template);
            try
            {
                parser.Parse(Path.GetFullPath(CSVTestDataFolder + "2020-07-29-14-37-42-position.csv"));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            Assert.Pass(TestPassed);
        }

        [Test]
        public void TestColumnsMissmatchingCsv()
        {
            var path = YamlTestDataFolder + YamlCsvFolder + "ValidCsvTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            var parser = new CsvParser(template);
            try
            {
                parser.Parse(Path.GetFullPath(CSVTestDataFolder + "2020-07-29-14-37-42-position-missed-headers.csv"));
            }
            catch (ColumnsMatchingException e)
            {
                Assert.Pass(e.Message);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            Assert.Fail(TestFailed);
        }

        [Test]
        public void TestMissedDateCsv()
        {
            var path = YamlTestDataFolder + YamlCsvFolder + "ValidCsvTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            var parser = new CsvParser(template);
            try
            {
                parser.Parse(Path.GetFullPath(CSVTestDataFolder + "Missed-date-position.csv"));
            }
            catch (IncorrectDateFormatException e)
            {
                Assert.Pass(e.Message);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            Assert.Fail(TestFailed);
        }

        [Test]
        public void TestValidCsvWithoutHeaders()
        {
            var path = YamlTestDataFolder + YamlCsvFolder + "ValidCsvTemplateWithoutHeaders.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            var parser = new CsvParser(template);
            try
            {
                var result = parser.Parse(Path.GetFullPath(CSVTestDataFolder + "2020-07-29-14-37-42-position-no-headers.csv"));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            Assert.Pass(TestPassed);
        }

        [Test]
        public void TestInvalidCSV()
        {
            var path = YamlTestDataFolder + YamlCsvFolder + "ValidCsvTemplateWithoutHeaders.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            var parser = new CsvParser(template);
            try
            {
                var result = parser.Parse(Path.GetFullPath(CSVTestDataFolder + "2020-07-29-14-37-42-position-invalid.csv"));
            }
            catch (Exception e)
            {
                Assert.Pass(e.Message);
            }
            Assert.Fail(TestFailed);
        }

        [Test]
        public void TestMagdroneCSV()
        {
            var path = YamlTestDataFolder + YamlCsvFolder + YamlMagdroneFolder + "MagDroneValidTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            var parser = new MagDroneCsvParser(template);
            try
            {
                var result = parser.Parse(Path.GetFullPath(CSVTestDataFolder + YamlMagdroneFolder + "Magdrone.csv"));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            Assert.Pass(TestPassed);
        }

        [Test]
        public void TestMagdroneCSVWithInvalidTemplate()
        {
            var path = YamlTestDataFolder + YamlCsvFolder + YamlMagdroneFolder + "MagDroneInvalidTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            var parser = new MagDroneCsvParser(template);
            try
            {
                var result = parser.Parse(Path.GetFullPath(CSVTestDataFolder + YamlMagdroneFolder + "Magdrone.csv"));
            }
            catch (Exception e)
            {
                Assert.Pass(e.Message);
            }
            Assert.Fail(TestFailed);
        }

        [Test]
        public void TestMagArrow()
        {
            var path = YamlTestDataFolder + YamlCsvFolder + YamlMagarrowFolder + "MagArrowValidTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            var parser = new CsvParser(template);
            try
            {
                var result = parser.Parse(Path.GetFullPath(CSVTestDataFolder + YamlMagarrowFolder + "MagArrow.csv"));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            Assert.Pass(TestPassed);
        }

        [Test]
        public void TestNmea()
        {
            var path = YamlTestDataFolder + YamlCsvFolder + YamlNmeaFolder + "NmeaValidTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            var parser = new NmeaCsvParser(template);
            try
            {
                var result = parser.Parse(Path.GetFullPath(CSVTestDataFolder + YamlNmeaFolder + "2020-10-23-09-16-13-pergam-falcon.log"));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            Assert.Pass(TestPassed);
        }
    }
}