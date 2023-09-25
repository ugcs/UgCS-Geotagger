namespace Tests
{
    using FileParsers.Yaml;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.IO;

    public class YamlProductionTests : Test
    {
        [Test]
        public void ValidTemplateTest()
        {
            foreach (var templateFile in Directory.EnumerateFiles(TemplatesFolder, "*.yaml", SearchOption.AllDirectories))
            {
                var file = File.ReadAllText(templateFile);

                var template = _deserializer.Deserialize<Template>(file);

                Assert.IsTrue(template.IsTemplateValid());
            }
        }

        [Test]
        public void UniqueFTUMatchRegexTest()
        {
            var regs = new HashSet<string>();

            foreach (var templateFile in Directory.EnumerateFiles(FTUTemplatesFolder, "*.yaml", SearchOption.AllDirectories))
            {
                var file = File.ReadAllText(templateFile);
                var template = _deserializer.Deserialize<Template>(file);
                Assert.IsTrue(template.IsTemplateValid());
                var addResult = regs.Add(template.MatchRegex);
                Assert.IsTrue(addResult);
            }
        }

        [Test]
        public void UniquePSFMatchRegexTest()
        {
            var regs = new HashSet<string>();

            foreach (var templateFile in Directory.EnumerateFiles(PSFTemplatesFolder, "*.yaml", SearchOption.AllDirectories))
            {
                var file = File.ReadAllText(templateFile);
                var template = _deserializer.Deserialize<Template>(file);
                Assert.IsTrue(template.IsTemplateValid());
                var addResult = regs.Add(template.MatchRegex);
                Assert.IsTrue(addResult);
            }
        }
    }
}