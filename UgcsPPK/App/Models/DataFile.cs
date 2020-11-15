using App.ViewModels;
using FileParsers;
using FileParsers.CSV;
using FileParsers.FixedColumnWidth;
using FileParsers.Yaml;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            Parser = CreateParser(template);
            if (Parser != null)
            {
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

        private Parser CreateParser(Template template)
        {
            return template.FileType switch
            {
                FileType.ColumnsFixedWidth => new FixedColumnWidthParser(template),
                FileType.CSV => template.Code != "magdrone" ? new CsvParser(template) : new MagDroneCsvParser(template),
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