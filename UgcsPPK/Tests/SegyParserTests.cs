using FileParsers.Exceptions;
using FileParsers.SegYLog;
using NUnit.Framework;
using System;
using System.IO;

namespace Tests
{
    public class SegyParserTests : Test
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestValidSegy()
        {
            var parser = new SegYLogParser();
            try
            {
                var result = parser.Parse(Path.GetFullPath(SegyTestDataFolder + "2020-07-29-14-37-42-gpr.sgy"));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            Assert.Pass(TestPassed);
        }

        [Test]
        public void TestUnknownSegy()
        {
            var parser = new SegYLogParser();
            try
            {
                var result = parser.Parse(Path.GetFullPath(SegyTestDataFolder + "2020-07-29-14-37-42-unknown.sgy"));
            }
            catch (UnknownSegyTypeException e)
            {
                Assert.Pass(e.Message);
            }
            catch (Exception)
            {
                Assert.Fail(TestFailed);
            }
            Assert.Pass(TestPassed);
        }
    }
}