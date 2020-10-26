using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileParsers.SegYLog
{
    public class SegYLogParser : IGeoCoordinateParser
    {
        private const int TextBytesOffset = 3200;
        private const int SamplesPerTraceOffset = 3222;
        private const int HeadersOffset = 3600;
        private const int ScalarOffset = 70;
        private const int LongitudeOffset = 72;
        private const int LatitudeOffset = 76;
        private const int LongitudeGprOffset = 182;
        private const int LatitudeGprOffset = 190;
        private const int TraceHeaderOffset = 240;
        private const int SecondsInDegree = 3600;
        private const string Gpr = "Georadar's settings information";
        private const string EchoSounder = "Echosounder's settings information";
        private const string Unknown = "Unknown";
        public string PayloadType { get; private set; }
        public short TracesLength { get; private set; }

        public List<GeoCoordinates> Parse(string segyPath)
        {
            if (!File.Exists(segyPath))
                throw new Exception($"File {segyPath} does not exist");
            using (BinaryReader reader = new BinaryReader(File.Open(segyPath, FileMode.Open)))
            {
                var textBytes = reader.ReadBytes(TextBytesOffset);
                var text = Encoding.UTF8.GetString(textBytes);
                if (text.Contains(Gpr))
                    PayloadType = Gpr;
                else if (text.Contains(EchoSounder))
                    PayloadType = EchoSounder;
                else
                {
                    PayloadType = Unknown;
                    throw new Exception($"Not supported SEG-Y type: {PayloadType}");
                }
            }
            using (BinaryReader reader = new BinaryReader(File.Open(segyPath, FileMode.Open)))
            {
                TracesLength = BitConverter.ToInt16(reader.ReadBytes(SamplesPerTraceOffset).Skip(SamplesPerTraceOffset - sizeof(short)).Take(sizeof(short)).ToArray(), 0); // have to check
            }
            var coordinates = new List<GeoCoordinates>();
            using (BinaryReader reader = new BinaryReader(File.Open(segyPath, FileMode.Open)))
            {
                var data = reader.ReadBytes((int)reader.BaseStream.Length).Skip(HeadersOffset).ToArray();
                for (int i = 0; i < data.Length; i += TraceHeaderOffset + TracesLength * 2)
                {
                    GeoCoordinates coordinate;
                    switch (PayloadType)
                    {
                        case Gpr:
                            coordinate = CreateGprCoordinates(data, i);
                            break;

                        case EchoSounder:
                            coordinate = CreateEchoSounderCoordinates(data, i);
                            break;

                        default:
                            throw new Exception($"Not supported SEG-Y type: {PayloadType}");
                    }
                    coordinates.Add(coordinate);
                }
            }
            return coordinates;
        }

        private GeoCoordinates CreateEchoSounderCoordinates(byte[] data, int i)
        {
            var scalar = BitConverter.ToInt16(data, i + ScalarOffset);
            var longitude = (scalar > 0 ? BitConverter.ToInt32(data, i + LongitudeOffset) * scalar : BitConverter.ToInt32(data, i + LongitudeOffset) / Math.Abs(scalar)) / (float)SecondsInDegree;
            var latitude = (scalar > 0 ? BitConverter.ToInt32(data, i + LatitudeOffset) * scalar : BitConverter.ToInt32(data, i + LatitudeOffset) / Math.Abs(scalar)) / (float)SecondsInDegree;
            return new GeoCoordinates(latitude, longitude);
        }

        private GeoCoordinates CreateGprCoordinates(byte[] data, int i)
        {
            var longitude = (BitConverter.ToDouble(data, i + LongitudeGprOffset) - BitConverter.ToDouble(data, i + LongitudeGprOffset) % 1 / 60) / 100f;
            var latitude = (BitConverter.ToDouble(data, i + LatitudeGprOffset) - BitConverter.ToDouble(data, i + LatitudeGprOffset) % 1 / 60) / 100f;
            return new GeoCoordinates(latitude, longitude);
        }

        private double ConvertToGrpFormat(double value)
        {
            return Math.Truncate(value) * 100 + (value - Math.Truncate(value)) * 60;
        }

        private int ConvertToEchoSounderFormat(double value)
        {
            return (int)Math.Round(value * SecondsInDegree * 1000);
        }

        public void CreatePpkCorrectedFile(string oldFile, string newFile, IEnumerable<GeoCoordinates> coordinates)
        {
            var position = HeadersOffset;
            byte[] bytes = File.ReadAllBytes(oldFile);
            byte[] lonToBytes;
            byte[] latToBytes;
            foreach (var coordinate in coordinates)
            {
                var scalar = BitConverter.ToInt16(bytes, position + ScalarOffset);
                switch (PayloadType)
                {
                    case Gpr:
                        lonToBytes = BitConverter.GetBytes(ConvertToGrpFormat(coordinate.Longitude));
                        latToBytes = BitConverter.GetBytes(ConvertToGrpFormat(coordinate.Latitude));
                        for (int j = 0; j < sizeof(double); j++)
                            bytes[position + LongitudeGprOffset + j] = lonToBytes[j];
                        for (int j = 0; j < sizeof(double); j++)
                            bytes[position + LatitudeGprOffset + j] = latToBytes[j];
                        lonToBytes = BitConverter.GetBytes((float)ConvertToGrpFormat(coordinate.Longitude));
                        latToBytes = BitConverter.GetBytes((float)ConvertToGrpFormat(coordinate.Latitude));
                        for (int j = 0; j < sizeof(float); j++)
                            bytes[position + LongitudeOffset + j] = lonToBytes[j];
                        for (int j = 0; j < sizeof(float); j++)
                            bytes[position + LatitudeOffset + j] = latToBytes[j];
                        break;

                    case EchoSounder:
                        lonToBytes = BitConverter.GetBytes(ConvertToEchoSounderFormat(coordinate.Longitude));
                        latToBytes = BitConverter.GetBytes(ConvertToEchoSounderFormat(coordinate.Latitude));
                        for (int j = 0; j < sizeof(int); j++)
                            bytes[position + LongitudeOffset + j] = lonToBytes[j];
                        for (int j = 0; j < sizeof(int); j++)
                            bytes[position + LatitudeOffset + j] = latToBytes[j];
                        break;

                    default:
                        throw new Exception($"Not supported SEG-Y type: {PayloadType}");
                }
                position += TraceHeaderOffset + TracesLength * 2;
            }
            File.WriteAllBytes(newFile, bytes);
        }

        public SegYLogParser()
        {
        }
    }
}