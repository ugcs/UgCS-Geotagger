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
        protected static ILog log = LogManager.GetLogger(typeof(DataFile));
        public const string PPK = "PPK";
        public List<GeoCoordinates> Coordinates { get; }
        public string FileName { get; protected set; }
        public string FilePath { get; protected set; }
        public string TypeOfFile { get; protected set; }
        public DateTime StartTime { get; protected set; }
        public DateTime EndTime { get; protected set; }
        public bool IsValid { get; protected set; }
        public Parser Parser { get; protected set; }

        protected DataFile(string filePath, Template template)
        {
            FileName = Path.GetFileName(filePath);
            FilePath = filePath;
            Parser = CreateParser(template.FileType);
            if (Parser != null)
            {
                Parser.CommentPrefix = template.Format.CommentPrefix;
                Parser.DateIndex = template.Columns.Timestamp?.Index ?? 0;
                Parser.LatitudeIndex = template.Columns.Latitude?.Index ?? 0;
                Parser.LongitudeIndex = template.Columns.Longitude?.Index ?? 0;
                Parser.TraceNumberIndex = template.Columns.TraceNumber?.Index ?? 0;
                Parser.ColumnLengths = template.Format.ColumnLengths;
                Parser.DecimalSeparator = template.Format.DecimalSeparator;
                Parser.DateColumnName = template.Columns.Timestamp?.Header;
                Parser.LatitudeColumnName = template.Columns.Latitude?.Header;
                Parser.LongitudeColumnName = template.Columns.Longitude?.Header;
                Parser.TraceNumberColumnName = template.Columns.TraceNumber?.Header;
                Parser.HasHeader = template.Format.HasHeader;
                Parser.Separator = template.Format.Separator;
                Parser.DateTimeRegex = template.Format.DateFormatRegex?.ToString();
                try
                {
                    Coordinates = Parser.Parse(filePath);

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
                catch (Exception e)
                {
                    log.Error(e.Message);
                }
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
            try
            {
                StartTime = posLogData.Min(d => d.Date);
            }
            catch (ArgumentNullException e)
            {
                log.Error(e.Message);
            }
        }

        private void SetEndTime(List<GeoCoordinates> posLogData)
        {
            try
            {
                EndTime = posLogData.Max(d => d.Date);
            }
            catch (ArgumentNullException e)
            {
                log.Error(e.Message);
            }
        }
    }
}