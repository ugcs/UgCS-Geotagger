using FileParsers.Exceptions;
using FileParsers.Yaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace FileParsers.SegYLog
{
    public class SegYLogParser : IGeoCoordinateParser
    {
        private const int TextBytesOffset = 3200;
        private const int SamplesPerTraceOffset = 3222;
        private const int HeadersOffset = 3600;
        private const int TraceNumberOffset = 8;
        private const int AltitudeOffset = 40;
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
        private readonly bool isAltitudeSet = false;
        public string PayloadType { get; private set; }
        public short TracesLength { get; private set; }

        private int _countOfReplacedLines;

        public int CountOfReplacedLines
        {
            get => _countOfReplacedLines;
            private set
            {
                _countOfReplacedLines = value;
                if (CountOfReplacedLines % 100 == 0)
                    OnOneHundredLinesReplaced?.Invoke(CountOfReplacedLines);
            }
        }

        public event Action<int> OnOneHundredLinesReplaced;

        public SegYLogParser(bool isAltitudeSet)
        {
            this.isAltitudeSet = isAltitudeSet;
        }


        public List<IGeoCoordinates> Parse(string segyPath)
        {
            if (!File.Exists(segyPath))
                throw new FileNotFoundException($"File {segyPath} does not exist");
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
                    throw new UnknownSegyTypeException($"Not supported SEG-Y type: {PayloadType}");
                }
            }
            using (BinaryReader reader = new BinaryReader(File.Open(segyPath, FileMode.Open)))
            {
                TracesLength = BitConverter.ToInt16(reader.ReadBytes(SamplesPerTraceOffset).Skip(SamplesPerTraceOffset - sizeof(short)).Take(sizeof(short)).ToArray(), 0);
            }
            var coordinates = new List<IGeoCoordinates>();
            using (BinaryReader reader = new BinaryReader(File.Open(segyPath, FileMode.Open)))
            {
                var data = reader.ReadBytes((int)reader.BaseStream.Length).Skip(HeadersOffset).ToArray();
                for (int i = 0; i < data.Length; i += TraceHeaderOffset + TracesLength * 2)
                {
                    GeoCoordinates coordinate = PayloadType switch
                    {
                        Gpr => CreateGprCoordinates(data, i),
                        EchoSounder => CreateEchoSounderCoordinates(data, i),
                        _ => throw new Exception($"Not supported SEG-Y type: {PayloadType}"),
                    };
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
            var altitude = BitConverter.ToSingle(data, i + AltitudeOffset);
            var longitude = (BitConverter.ToDouble(data, i + LongitudeGprOffset) - BitConverter.ToDouble(data, i + LongitudeGprOffset) % 1 / 60) / 100f;
            var latitude = (BitConverter.ToDouble(data, i + LatitudeGprOffset) - BitConverter.ToDouble(data, i + LatitudeGprOffset) % 1 / 60) / 100f;
            return new GeoCoordinates(latitude, longitude, altitude);
        }

        private double ConvertToGrpFormat(double value)
        {
            return Math.Truncate(value) * 100 + (value - Math.Truncate(value)) * 60;
        }

        private int ConvertToEchoSounderFormat(double value)
        {
            return (int)Math.Round(value * SecondsInDegree * 1000);
        }

        private int DeserializeTraceNumber(byte[] data, int i)
        {
            return BitConverter.ToInt32(data, i);
        }

        public Result CreatePpkCorrectedFile(string oldFile, string newFile, IEnumerable<IGeoCoordinates> coordinates, CancellationTokenSource token)
        {
            var result = new Result();
            var startPosition = HeadersOffset;
            byte[] bytes = File.ReadAllBytes(oldFile);
            CountOfReplacedLines = 0;
            byte[] lonToBytes;
            byte[] latToBytes;
            byte[] altToBytes;
            for (int i = startPosition; i < bytes.Length; i += TraceHeaderOffset + TracesLength * 2)
            {
                if (token.IsCancellationRequested)
                    break;
                var traceNumber = DeserializeTraceNumber(bytes, i + TraceNumberOffset);
                var coordinate = coordinates.Where(c => c.TraceNumber == traceNumber).FirstOrDefault();
                if (coordinate != null)
                {
                    switch (PayloadType)
                    {
                        case Gpr:
                            lonToBytes = BitConverter.GetBytes(ConvertToGrpFormat(coordinate.Longitude));
                            latToBytes = BitConverter.GetBytes(ConvertToGrpFormat(coordinate.Latitude));
                            for (int j = 0; j < sizeof(double); j++)
                                bytes[i + LongitudeGprOffset + j] = lonToBytes[j];
                            for (int j = 0; j < sizeof(double); j++)
                                bytes[i + LatitudeGprOffset + j] = latToBytes[j];
                            lonToBytes = BitConverter.GetBytes((float)ConvertToGrpFormat(coordinate.Longitude));
                            latToBytes = BitConverter.GetBytes((float)ConvertToGrpFormat(coordinate.Latitude));
                            altToBytes = BitConverter.GetBytes((float)coordinate.Altitude);
                            for (int j = 0; j < sizeof(float); j++)
                                bytes[i + LongitudeOffset + j] = lonToBytes[j];
                            for (int j = 0; j < sizeof(float); j++)
                                bytes[i + LatitudeOffset + j] = latToBytes[j];
                            if (isAltitudeSet)
                            {
                                for (int j = 0; j < sizeof(float); j++)
                                    bytes[i + AltitudeOffset + j] = altToBytes[j];
                            }
                            break;

                        case EchoSounder:
                            lonToBytes = BitConverter.GetBytes(ConvertToEchoSounderFormat(coordinate.Longitude));
                            latToBytes = BitConverter.GetBytes(ConvertToEchoSounderFormat(coordinate.Latitude));
                            for (int j = 0; j < sizeof(int); j++)
                                bytes[i + LongitudeOffset + j] = lonToBytes[j];
                            for (int j = 0; j < sizeof(int); j++)
                                bytes[i + LatitudeOffset + j] = latToBytes[j];
                            break;

                        default:
                            throw new Exception($"Not supported SEG-Y type: {PayloadType}");
                    }
                    result.CountOfReplacedLines++;
                }
                CountOfReplacedLines++;
                result.CountOfLines++;
            }
            File.WriteAllBytes(newFile, bytes);
            return result;
        }

        public SegYLogParser()
        {
        }
    }
}