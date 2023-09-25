namespace Tests
{
    using FileParsers.Yaml;
    using NUnit.Framework;
    using System.IO;
    using YamlDotNet.Core;

    public class YamlTests : Test
    {
        [Test]
        public void UnknownTypeTest()
        {
            const string path = YamlTestDataFolder + "UnknownTypeTemplate.yaml";
            var file = File.ReadAllText(path);
            try
            {
                _ = _deserializer.Deserialize<Template>(file);
            }
            catch (YamlException)
            {
                Assert.Pass(TestPassed);
            }

            Assert.Fail(TestFailed);
        }

        [Test]
        public void WrongRegexTest()
        {
            const string path = YamlTestDataFolder + "WrongRegexTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = _deserializer.Deserialize<Template>(file);
            Assert.IsFalse(template.IsTemplateValid());
        }

        [Test]
        public void DecimalSeparatorTest()
        {
            const string path = YamlTestDataFolder + "MissDecimalSeparatorTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = _deserializer.Deserialize<Template>(file);
            Assert.IsFalse(template.IsTemplateValid());
        }

        [Test]
        public void SeparatorTest()
        {
            const string path = YamlTestDataFolder + "MissSeparatorTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = _deserializer.Deserialize<Template>(file);
            Assert.IsTrue(template.Format.Separator == null);
        }

        [Test]
        public void CommentPrefixTest()
        {
            const string path = YamlTestDataFolder + "MissCommentPrefixTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = _deserializer.Deserialize<Template>(file);
            Assert.IsTrue(template.Format.CommentPrefix == null);
        }

        [Test]
        public void NameTest()
        {
            const string path = YamlTestDataFolder + "MissNameTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = _deserializer.Deserialize<Template>(file);
            Assert.IsTrue(string.IsNullOrWhiteSpace(template.Name));
        }

        [Test]
        public void CodeTest()
        {
            const string path = YamlTestDataFolder + "MissCodeTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = _deserializer.Deserialize<Template>(file);
            Assert.IsTrue(string.IsNullOrWhiteSpace(template.Code));
        }

        [Test]
        public void ValidCsvTemplateTest()
        {
            const string path = YamlTestDataFolder + YamlCsvFolder + "ValidCsvTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = _deserializer.Deserialize<Template>(file);
            Assert.IsTrue(template.IsTemplateValid());
        }

        [Test]
        public void NoHeadersWhenSetTemplateTest()
        {
            const string path = YamlTestDataFolder + YamlCsvFolder + "NoHeadersWhenSetTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = _deserializer.Deserialize<Template>(file);
            Assert.IsFalse(template.IsTemplateValid());
        }

        [Test]
        public void MissedColumnTemplateTest()
        {
            const string path = YamlTestDataFolder + YamlCsvFolder + "MissedColumnTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = _deserializer.Deserialize<Template>(file);
            Assert.IsFalse(template.IsTemplateValid());
        }

        [Test]
        public void NoIndexesWhenNoHeadersSetTemplateTest()
        {
            const string path = YamlTestDataFolder + YamlCsvFolder + "NoIndexesWhenNoHeadersSetTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = _deserializer.Deserialize<Template>(file);
            Assert.IsFalse(template.IsTemplateValid());
        }

        [Test]
        public void IndexesAndHeadersTemplateTest()
        {
            const string path = YamlTestDataFolder + YamlCsvFolder + "HeadersAndIndexesTogetherTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = _deserializer.Deserialize<Template>(file);
            Assert.IsFalse(template.IsTemplateValid());
        }

        [Test]
        public void TestIndexesAndHeadersSecondCaseTemplate()
        {
            const string path = YamlTestDataFolder + YamlCsvFolder + "HeadersAndIndexesTogetherTemplateSecondCase.yaml";
            var file = File.ReadAllText(path);
            var template = _deserializer.Deserialize<Template>(file);
            Assert.IsFalse(template.IsTemplateValid());
        }

        [Test]
        public void MissedColumnWidthsTemplateTest()
        {
            const string path = YamlTestDataFolder + ColumnFixedWidthFolder + "MissedColumnWidths.yaml";
            var file = File.ReadAllText(path);
            var template = _deserializer.Deserialize<Template>(file);
            Assert.IsFalse(template.IsTemplateValid());
        }

        [Test]
        public void FixedColumnValidTemplateTest()
        {
            const string path = YamlTestDataFolder + ColumnFixedWidthFolder + "ValidTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = _deserializer.Deserialize<Template>(file);
            Assert.IsTrue(template.IsTemplateValid());
        }

        [Test]
        public void ValidMagDroneTest()
        {
            const string path = YamlTestDataFolder + YamlCsvFolder + YamlMagdroneFolder + "MagDroneValidTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = _deserializer.Deserialize<Template>(file);
            Assert.IsTrue(template.IsTemplateValid());
        }

        [Test]
        public void ValidNmeaTest()
        {
            const string path = YamlTestDataFolder + YamlCsvFolder + YamlNmeaFolder + "NmeaValidTemplate.yaml";
            var file = File.ReadAllText(path);
            var template = _deserializer.Deserialize<Template>(file);
            Assert.IsTrue(template.IsTemplateValid());
        }
    }
}