using FileParsers.FixedColumnWidth;
using FileParsers.Yaml;
using NUnit.Framework;
using System;
using System.IO;

namespace Tests
{
    public class FixedColumnWidthParserTests : Test
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestValidPos()
        {
            var path = YamlTestDataFolder + ColumnFixedWidthFolder + "ValidTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = _deserializer.Deserialize<Template>(file);
            var parser = new FixedColumnWidthParser(template);
            try
            {
                parser.Parse(Path.GetFullPath(FCWTestDataFolder + "Valid.pos"));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            
            Assert.Pass(TestPassed);
        }

        [Test]
        public void TestInvalidColumnsLengths()
        {
            var path = YamlTestDataFolder + ColumnFixedWidthFolder + "InvalidColumnsLengthsTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = _deserializer.Deserialize<Template>(file);
            var parser = new FixedColumnWidthParser(template);
            try
            {
                parser.Parse(Path.GetFullPath(FCWTestDataFolder + "Valid.pos"));
            }
            catch (Exception e)
            {
                Assert.Pass(e.Message);
            }

            Assert.Fail(TestFailed);
        }
    }
}