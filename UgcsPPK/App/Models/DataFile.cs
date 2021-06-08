using App.ViewModels;
using FileParsers;
using FileParsers.CSV;
using FileParsers.FixedColumnWidth;
using FileParsers.SegYLog;
using FileParsers.Yaml;
using log4net;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UgCSPPK.Models
{
    public class DataFile : ViewModelBase, IDataFile
    {
        private readonly CSVParsersFactory cSVParsersFactory = new CSVParsersFactory();
        protected static ILog log = LogManager.GetLogger(typeof(DataFile));
        public const string Precise = "precise";
        public const string PositionSolutionFileAbbr = "PSF";
        public const string FileToUpdateAbbr = "FTU";
        public List<IGeoCoordinates> Coordinates { get; }
        public string FileName { get; protected set; }
        public string FilePath { get; protected set; }
        public string TypeOfFile { get; protected set; }
        public DateTime StartTime { get; protected set; }
        public DateTime EndTime { get; protected set; }
        public bool IsValid { get; protected set; }
        public Parser Parser { get; protected set; }
        public FileType Type { get; protected set; }

        private bool _isSelected = false;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isSelected, value);
            }
        }

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
                        Type = template.FileType;
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
                FileType.CSV => cSVParsersFactory.CreateCSVParser(template),
                FileType.Segy => new SegYLogParser(template),
                _ => null,
            };
        }

        private void SetTypeOfFile(Template template)
        {
            TypeOfFile = template.Name;
        }

        private void SetStartTime(List<IGeoCoordinates> posLogData)
        {
            try
            {
                StartTime = posLogData.Min(d => d.DateTime.Value);
            }
            catch (ArgumentNullException e)
            {
                log.Error(e.Message);
            }
        }

        private void SetEndTime(List<IGeoCoordinates> posLogData)
        {
            try
            {
                EndTime = posLogData.Max(d => d.DateTime.Value);
            }
            catch (ArgumentNullException e)
            {
                log.Error(e.Message);
            }
        }
    }
}