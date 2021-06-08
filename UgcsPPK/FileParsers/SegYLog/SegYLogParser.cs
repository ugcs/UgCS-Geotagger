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
    public class SegYLogParser : Parser
    {
        private const short TextBytesOffset = 3200;
        private const short SamplesPerTraceOffset = 3222;
        private const short SamplesFormatOffset = 3226;
        private const short HeadersOffset = 3600;
        private const short TraceNumberOffset = 0;
        private const short AltitudeOffset = 40;
        private const short ScalarOffset = 70;
        private const short LongitudeOffset = 72;
        private const short LatitudeOffset = 76;
        private const short YearOffset = 156;
        private const short DayOfYearOffset = 158;
        private const short HourOffset = 160;
        private const short MinuteOffset = 162;
        private const short SecondOffset = 164;
        private const short MSecondOffset = 168;
        private const short LongitudeGprOffset = 182;
        private const short LatitudeGprOffset = 190;
        private const short TraceHeaderOffset = 240;
        private const short SecondsInDegree = 3600;
        private const string Gpr = "Georadar's settings information";
        private const string EchoSounder = "Echosounder's settings information";
        private const string Unknown = "Unknown";
        public string PayloadType { get; private set; }
        public short TracesLength { get; private set; }
        public int SampleFormatBytes { get; private set; }

        public override List<IGeoCoordinates> Parse(string segyPath)
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
                var samples = (SegYSampleFormat)BitConverter.ToInt16(reader.ReadBytes(SamplesFormatOffset).Skip(SamplesFormatOffset - sizeof(short)).Take(sizeof(short)).ToArray(), 0);
                SampleFormatBytes = samples == SegYSampleFormat.IbmFloat32BitFormat || samples == SegYSampleFormat.Integer32BitFormat ? 4 : 2;
                var data = reader.ReadBytes((int)reader.BaseStream.Length).Skip(HeadersOffset - SamplesFormatOffset).ToArray();
                for (int i = 0; i < data.Length; i += TraceHeaderOffset + TracesLength * SampleFormatBytes)
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
            var traceNumber = BitConverter.ToInt32(data, i + TraceNumberOffset);
            var year = BitConverter.ToInt16(data, i + YearOffset);
            var dayOfYear = BitConverter.ToInt16(data, i + DayOfYearOffset);
            var hours = BitConverter.ToInt16(data, i + HourOffset);
            var minutes = BitConverter.ToInt16(data, i + MinuteOffset);
            var seconds = BitConverter.ToInt16(data, i + SecondOffset);
            var mSeconds = BitConverter.ToInt16(data, i + MSecondOffset);
            if (IsTimeParameterValid(year, 0, DateTime.Now.Year + 1) && IsTimeParameterValid(dayOfYear, 0, 367)
                && IsTimeParameterValid(hours, -1, 24) && IsTimeParameterValid(minutes, -1, 59) && IsTimeParameterValid(seconds, -1, 59)
                && IsTimeParameterValid(mSeconds, -1, 1000))
            {
                var date = new DateTime(year, 1, 1);
                date = date.AddDays(dayOfYear - 1);
                date = date.AddHours(hours);
                date = date.AddMinutes(minutes);
                date = date.AddSeconds(seconds);
                date = date.AddMilliseconds(mSeconds);
                return new GeoCoordinates(date, latitude, longitude, altitude, traceNumber);
            }
            else return new GeoCoordinates(latitude, longitude, altitude, traceNumber);
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

        public override Result CreatePpkCorrectedFile(string oldFile, string newFile, IEnumerable<IGeoCoordinates> coordinates, CancellationTokenSource token)
        {
            var result = new Result();
            var startPosition = HeadersOffset;
            byte[] bytes = File.ReadAllBytes(oldFile);
            CountOfReplacedLines = 0;
            byte[] lonToBytes;
            byte[] latToBytes;
            byte[] altToBytes;
            for (int i = startPosition; i < bytes.Length; i += TraceHeaderOffset + TracesLength * SampleFormatBytes)
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
                            if (coordinate.Longitude.HasValue)
                            {
                                lonToBytes = BitConverter.GetBytes(ConvertToGrpFormat(coordinate.Longitude.Value));
                                for (int j = 0; j < sizeof(double); j++)
                                    bytes[i + LongitudeGprOffset + j] = lonToBytes[j];
                                lonToBytes = BitConverter.GetBytes((float)ConvertToGrpFormat(coordinate.Longitude.Value));
                                for (int j = 0; j < sizeof(float); j++)
                                    bytes[i + LongitudeOffset + j] = lonToBytes[j];
                            }

                            if (coordinate.Latitude.HasValue)
                            {
                                latToBytes = BitConverter.GetBytes(ConvertToGrpFormat(coordinate.Latitude.Value));
                                for (int j = 0; j < sizeof(double); j++)
                                    bytes[i + LatitudeGprOffset + j] = latToBytes[j];
                                latToBytes = BitConverter.GetBytes((float)ConvertToGrpFormat(coordinate.Latitude.Value));
                                for (int j = 0; j < sizeof(float); j++)
                                    bytes[i + LatitudeOffset + j] = latToBytes[j];
                            }

                            if (coordinate.Altitude.HasValue)
                            {
                                altToBytes = BitConverter.GetBytes((float)coordinate.Altitude);
                                for (int j = 0; j < sizeof(float); j++)
                                    bytes[i + AltitudeOffset + j] = altToBytes[j];
                            }
                            break;

                        case EchoSounder:
                            if (coordinate.Longitude.HasValue)
                            {
                                lonToBytes = BitConverter.GetBytes(ConvertToEchoSounderFormat(coordinate.Longitude.Value));
                                for (int j = 0; j < sizeof(int); j++)
                                    bytes[i + LongitudeOffset + j] = lonToBytes[j];
                            }

                            if (coordinate.Latitude.HasValue)
                            {
                                latToBytes = BitConverter.GetBytes(ConvertToEchoSounderFormat(coordinate.Latitude.Value));
                                for (int j = 0; j < sizeof(int); j++)
                                    bytes[i + LatitudeOffset + j] = latToBytes[j];
                            }
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

        private bool IsTimeParameterValid(int value, int min, int max)
        {
            return value > min && value < max;
        }

        public SegYLogParser(Template template) : base(template)
        {
        }
    }
}