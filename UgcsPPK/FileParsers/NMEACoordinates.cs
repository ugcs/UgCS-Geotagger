using FileParsers.Exceptions;
using System;
using System.Globalization;
using System.Linq;

namespace FileParsers
{
    public class NMEACoordinates : GeoCoordinates, IGeoCoordinates
    {
        public const string North = "N";
        public const string South = "S";
        public const string East = "E";
        public const string West = "W";
        public string NMEAType { get; set; }
        public string UTCPosition { get; set; }
        public string PositionStatus { get; set; }
        public string NorthOrSouth { get; set; }
        public string EastOrWest { get; set; }
        public string NMEALatitude { get; set; }
        public string NMEALongitude { get; set; }
        public string Speed { get; set; }
        public string TrakeMade { get; set; }
        public string UTDate { get; set; }
        public string MagneticVariationDegrees { get; set; }
        public string VarDir { get; set; }
        public string ModeInd { get; set; }
        public CultureInfo Format { get; set; }

        public string ConvertToNMEACoordringates(double coordinate)
        {
            return Math.Abs((Math.Truncate(coordinate) * 100 + (coordinate - Math.Truncate(coordinate)) * 60)).ToString("0.####", Format ?? CultureInfo.InvariantCulture);
        }

        public double ConvertToGoogleCoordinates(string coordinate, string cardinalDirection)
        {
            var digitCount = coordinate[4].ToString() == Format.NumberFormat.NumberDecimalSeparator ? 2 : 3;
            var integerPart = coordinate.Take(digitCount).ToArray();
            coordinate = coordinate.Substring(digitCount);
            var result = int.Parse(new string(integerPart)) + double.Parse(coordinate, Format) / 60;
            if (cardinalDirection.Equals(West) || cardinalDirection.Equals(South))
                result = -result;
            return result;
        }

        public int CalculateCheckSum()
        {
            var line = $"{NMEAType}{UTCPosition}{PositionStatus}{NMEALatitude}{NorthOrSouth}{NMEALongitude}{EastOrWest}{Speed}{TrakeMade}{UTDate}{MagneticVariationDegrees}" +
                $"{VarDir}{ModeInd}";
            int checkSum = 0;
            for (int i = 0; i < line.Length; i++)
                checkSum ^= Convert.ToByte(line[i]);
            return checkSum;
        }

        public string CreateNMEAMessage(string checksum)
        {
            return $" ${NMEAType},{UTCPosition},{PositionStatus},{NMEALatitude},{NorthOrSouth},{NMEALongitude},{EastOrWest},{Speed},{TrakeMade},{UTDate},{MagneticVariationDegrees}," +
                $"{VarDir},{ModeInd}*{checksum}";
        }

        public void ParseNMEAMessage(string message)
        {
            var data = message.Split(new[] { "," }, StringSplitOptions.None);
            if (data[0].Contains("$"))
                data[0] = data[0].Replace("$", "").Trim();
            if (data[0] != "GPRMC" && data[0] != "GNRMC")
                throw new UnknownNMEATypeException("Unsupported NMEA type");
            NMEAType = data[0].Substring(0);
            UTCPosition = data[1];
            PositionStatus = data[2];
            NMEALatitude = data[3];
            NorthOrSouth = data[4];
            NMEALongitude = data[5];
            EastOrWest = data[6];
            Speed = data[7];
            TrakeMade = data[8];
            UTDate = data[9];
            MagneticVariationDegrees = data[10];
            VarDir = data[11];
            ModeInd = data[12][0].ToString();
            Latitude = ConvertToGoogleCoordinates(NMEALatitude, NorthOrSouth);
            Longitude = ConvertToGoogleCoordinates(NMEALongitude, EastOrWest);
        }

        public void SetNewNmeaCoordinates()
        {
            NMEALatitude = ConvertToNMEACoordringates(Latitude);
            NMEALongitude = ConvertToNMEACoordringates(Longitude);
            NorthOrSouth = Latitude > 0 ? North : South;
            EastOrWest = Longitude > 0 ? East : West;
        }
    }
}