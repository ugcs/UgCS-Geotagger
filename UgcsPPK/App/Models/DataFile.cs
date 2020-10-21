using FileParsers;
using FileParsers.CSV;
using FileParsers.FixedColumnWidth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UgCSPPK.Models.Yaml;

namespace UgCSPPK.Models
{
    public class DataFile : IDataFile
    {
        protected List<GeoCoordinates> coordinates;
        protected Parser parser;
        public string Name { get; protected set; }
        public string TypeOfFile { get; protected set; }
        public DateTime StartTime { get; protected set; }
        public DateTime EndTime { get; protected set; }
        public bool IsValid { get; protected set; }

        protected DataFile(string filePath, Template template)
        {
            Name = Path.GetFileName(filePath);
            parser = CreateParser(template.FileType);
            if (parser != null)
            {
                parser.CommentPrefix = template.Format.CommentPrefix;
                parser.DateIndex = template.Columns.Timestamp.Index;
                parser.LatitudeIndex = template.Columns.Latitude.Index;
                parser.LongitudeIndex = template.Columns.Longitude.Index;
                parser.ColumnLengths = template.Format.ColumnLengths;
                parser.DecimalSeparator = template.Format.DecimalSeparator;
                parser.DateColumnName = template.Columns.Timestamp.Header;
                parser.LatitudeColumnName = template.Columns.Latitude.Header;
                parser.LongitudeColumnName = template.Columns.Longitude.Header;
                parser.HasHeader = template.Format.HasHeader;
                parser.Separator = template.Format.Separator;
                coordinates = parser.Parse(filePath);
                if (coordinates != null)
                {
                    SetTypeOfFile(template);
                    SetStartTime(coordinates);
                    SetEndTime(coordinates);
                    IsValid = true;
                }
                else
                    IsValid = false;
            }
        }

        private Parser CreateParser(FileType fileType)
        {
            switch (fileType)
            {
                case FileType.ColumnsFixedWidth:
                    return new FixedColumnWidthParser();

                case FileType.CSV:
                    return new CsvParser();

                case FileType.Unknown:
                default:
                    return null;
            }
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