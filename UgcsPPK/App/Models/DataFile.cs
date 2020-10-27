using App.ViewModels;
using FileParsers;
using FileParsers.CSV;
using FileParsers.FixedColumnWidth;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UgCSPPK.Models.Yaml;

namespace UgCSPPK.Models
{
    public class DataFile : ViewModelBase, IDataFile
    {
        private static ILog log = LogManager.GetLogger(typeof(DataFile));
        public const string PPK = "PPK";
        public List<GeoCoordinates> Coordinates { get; }
        public string FileName { get; protected set; }
        public string FilePath { get; protected set; }
        public string TypeOfFile { get; protected set; }
        public DateTime StartTime { get; protected set; }
        public DateTime EndTime { get; protected set; }
        public bool IsValid { get; protected set; }
        protected Parser parser;

        protected DataFile(string filePath, Template template)
        {
            FileName = Path.GetFileName(filePath);
            FilePath = filePath;
            parser = CreateParser(template.FileType);
            if (parser != null)
            {
                parser.CommentPrefix = template.Format.CommentPrefix;
                parser.DateIndex = template.Columns.Timestamp.Index;
                parser.LatitudeIndex = template.Columns.Latitude.Index;
                parser.LongitudeIndex = template.Columns.Longitude.Index;
                parser.TraceNumberIndex = template.Columns.TraceNumber.Index;
                parser.ColumnLengths = template.Format.ColumnLengths;
                parser.DecimalSeparator = template.Format.DecimalSeparator;
                parser.DateColumnName = template.Columns.Timestamp.Header;
                parser.LatitudeColumnName = template.Columns.Latitude.Header;
                parser.LongitudeColumnName = template.Columns.Longitude.Header;
                parser.TraceNumberColumnName = template.Columns.TraceNumber.Header;
                parser.HasHeader = template.Format.HasHeader;
                parser.Separator = template.Format.Separator;
                try
                {
                    Coordinates = parser.Parse(filePath);
                }
                catch(Exception e)
                {
                    log.Error(e.StackTrace);
                }
                if (Coordinates != null)
                {
                    SetTypeOfFile(template);
                    SetStartTime(Coordinates);
                    SetEndTime(Coordinates);
                    IsValid = true;
                }
                else
                    IsValid = false;
            }
        }

        private Parser CreateParser(FileType fileType)
        {
            return fileType switch
            {
                FileType.ColumnsFixedWidth => new FixedColumnWidthParser(),
                FileType.CSV => new CsvParser(),
                _ => null,
            };
        }

        private void SetTypeOfFile(Template template)
        {
            TypeOfFile = template.Name;
        }

        private void SetStartTime(List<GeoCoordinates> posLogData)
        {
            StartTime = posLogData.Min(d => d.Date);
        }

        private void SetEndTime(List<GeoCoordinates> posLogData)
        {
            EndTime = posLogData.Max(d => d.Date);
        }
    }
}