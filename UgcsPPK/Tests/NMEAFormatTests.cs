using FileParsers;
using NUnit.Framework;
using System.Globalization;

namespace Tests
{
    public class NMEAFormatTests : Test
    {
        private const double Delta = 0.00001;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestNMEAToGoogleCoordinates()
        {
            var format = new CultureInfo("en-US", false);
            format.NumberFormat.NumberDecimalSeparator = ".";
            var nmeaCoordinate = new NMEACoordinates()
            {
                Format = format,
                NMEALatitude = "5651.7446",
                NMEALongitude = "2406.7933",
                NorthOrSouth = "N",
                EastOrWest = "E"
            };
            var latitude = nmeaCoordinate.ConvertToGoogleCoordinates(nmeaCoordinate.NMEALatitude, nmeaCoordinate.NorthOrSouth);
            var longitude = nmeaCoordinate.ConvertToGoogleCoordinates(nmeaCoordinate.NMEALongitude, nmeaCoordinate.EastOrWest);
            Assert.IsTrue(latitude - 56.86241 < Delta && longitude - 24.113221 < Delta);
        }

        [Test]
        public void TestGoogleToNMEACoordinates()
        {
            var format = new CultureInfo("en-US", false);
            format.NumberFormat.NumberDecimalSeparator = ".";
            var nmeaCoordinate = new NMEACoordinates()
            {
                Format = format,
                Latitude = 56.86241,
                Longitude = 24.113221,
                NorthOrSouth = "N",
                EastOrWest = "E"
            };
            var latitude = double.Parse(nmeaCoordinate.ConvertToNMEACoordringates(nmeaCoordinate.Latitude), format);
            var longitude = double.Parse(nmeaCoordinate.ConvertToNMEACoordringates(nmeaCoordinate.Longitude), format);
            Assert.IsTrue(latitude - 5651.7446 < Delta && longitude - 2406.7933 < Delta);
        }

        [Test]
        public void TestNMEAMessageParsing()
        {
            var format = new CultureInfo("en-US", false);
            format.NumberFormat.NumberDecimalSeparator = ".";
            var message = "$GNRMC,091613.699,A,5651.7446,N,02406.7933,E,0.18,-49.88,231020,,,A*69";
            var coordinate = new NMEACoordinates()
            {
                Format = format,
            };
            coordinate.ParseNMEAMessage(message);
            Assert.Pass("Test Passed");
        }

        [Test]
        public void TestCheckSumCalculation()
        {
            var format = new CultureInfo("en-US", false);
            format.NumberFormat.NumberDecimalSeparator = ".";
            var message = "$GNRMC,091613.699,A,5651.7446,N,02406.7933,E,0.18,-49.88,231020,,,A*69";
            var coordinate = new NMEACoordinates()
            {
                Format = format,
            };
            coordinate.ParseNMEAMessage(message);
            var ckecksum = coordinate.CalculateCheckSum().ToString("X");
            message = "$GNRMC,091617.692,A,5651.7447,N,02406.7932,E,0.11,-50.19,231020,,,A*6F";
            coordinate.ParseNMEAMessage(message);
            var secondChecksum = coordinate.CalculateCheckSum().ToString("X");
            Assert.IsTrue(ckecksum == "69" && secondChecksum == "6F");
        }
    }
}