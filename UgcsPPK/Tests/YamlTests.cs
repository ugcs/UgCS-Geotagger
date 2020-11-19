using FileParsers.Yaml;
using NUnit.Framework;
using System;
using System.IO;
using YamlDotNet.Core;

namespace Tests
{
    public class YamlTests : Test
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestUnknownType()
        {
            var path = YamlTestDataFolder + "UnknownTypeTemplate.yaml";
            var file = File.ReadAllText(path);
            try
            {
                var template = deserializer.Deserialize<Template>(file);
            }
            catch (YamlException)
            {
                Assert.Pass(TestPassed);
            }
            Assert.Fail(TestFailed);
        }

        [Test]
        public void TestWrongRegex()
        {
            var path = YamlTestDataFolder + "WrongRegexTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            Assert.IsFalse(template.IsTemplateValid());
        }

        [Test]
        public void TestDecimalSeparator()
        {
            var path = YamlTestDataFolder + "MissDecimalSeparatorTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            Assert.IsFalse(template.IsTemplateValid());
        }

        [Test]
        public void TestSeparator()
        {
            var path = YamlTestDataFolder + "MissSeparatorTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            Assert.IsTrue(template.Format.Separator == null);
        }

        [Test]
        public void TestCommentPrefix()
        {
            var path = YamlTestDataFolder + "MissCommentPrefixTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            Assert.IsTrue(template.Format.CommentPrefix == null);
        }

        [Test]
        public void TestName()
        {
            var path = YamlTestDataFolder + "MissNameTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            Assert.IsTrue(string.IsNullOrWhiteSpace(template.Name));
        }

        [Test]
        public void TestCode()
        {
            var path = YamlTestDataFolder + "MissCodeTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            Assert.IsTrue(string.IsNullOrWhiteSpace(template.Code));
        }

        [Test]
        public void TestDateFormat()
        {
            var path = YamlTestDataFolder + "MissDateFormatTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            Assert.IsFalse(template.IsTemplateValid());
        }

        [Test]
        public void TestValidCsvTemplate()
        {
            var path = YamlTestDataFolder + YamlCsvFolder + "ValidCsvTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            Assert.IsTrue(template.IsTemplateValid());
        }

        [Test]
        public void TestNoHeadersWhenSetTemplate()
        {
            var path = YamlTestDataFolder + YamlCsvFolder + "NoHeadersWhenSetTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            Assert.IsFalse(template.IsTemplateValid());
        }

        [Test]
        public void TestMissedColumnTemplate()
        {
            var path = YamlTestDataFolder + YamlCsvFolder + "MissedColumnTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            Assert.IsFalse(template.IsTemplateValid());
        }

        [Test]
        public void TestNoIndexesWhenNoHeadersSetTemplate()
        {
            var path = YamlTestDataFolder + YamlCsvFolder + "NoIndexesWhenNoHeadersSetTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            Assert.IsFalse(template.IsTemplateValid());
        }

        [Test]
        public void TestIndexesAndHeadersTohetherTemplate()
        {
            var path = YamlTestDataFolder + YamlCsvFolder + "HeadersAndIndexesTogetherTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            Assert.IsFalse(template.IsTemplateValid());
        }

        [Test]
        public void TestIndexesAndHeadersTohetherSecondCaseTemplate()
        {
            var path = YamlTestDataFolder + YamlCsvFolder + "HeadersAndIndexesTogetherTemplateSecondCase.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            Assert.IsFalse(template.IsTemplateValid());
        }

        [Test]
        public void TestMissedColumnWidthsTemplate()
        {
            var path = YamlTestDataFolder + ColumnFixedWidthFolder + "MissedColumnWidths.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            Assert.IsFalse(template.IsTemplateValid());
        }

        [Test]
        public void TestFixedColumnValidTemplate()
        {
            var path = YamlTestDataFolder + ColumnFixedWidthFolder + "ValidTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            Assert.IsTrue(template.IsTemplateValid());
        }

        [Test]
        public void TestFTUValidTemplate()
        {
            var path = YamlTestDataFolder + FTUTemplatesFolder + "ValidTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            Assert.IsTrue(template.IsTemplateValid());
        }

        [Test]
        public void TestPSFValidTemplates()
        {
            var path = YamlTestDataFolder + PSFTemplatesFolder + "ValidTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            path = YamlTestDataFolder + PSFTemplatesFolder + "SecondValidTemplate.yaml";
            file = File.ReadAllText(path);
            var secondTemplate = deserializer.Deserialize<Template>(file);
            Assert.IsTrue(template.IsTemplateValid() && secondTemplate.IsTemplateValid());
        }

        [Test]
        public void TestValidMagDrone()
        {
            var path = YamlTestDataFolder + YamlCsvFolder + YamlMagdroneFolder + "MagDroneValidTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = deserializer.Deserialize<Template>(file);
            Assert.IsTrue(template.IsTemplateValid());
        }
    }
}