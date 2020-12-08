using FileParsers;
using FileParsers.SegYLog;
using FileParsers.Yaml;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace UgCSPPK.Models
{
    public class FileToUpdate : DataFile
    {
        private CoveringStatus _coveringStatus = CoveringStatus.NotCovered;

        public CoveringStatus CoveringStatus
        {
            get => _coveringStatus;
            private set
            {
                this.RaiseAndSetIfChanged(ref _coveringStatus, value);
            }
        }

        public string ResultMessage { get; private set; } = "";
        public HashSet<PositioningSolutionFile> CoverageFiles { get; private set; } = new HashSet<PositioningSolutionFile>();
        public string LinkedFile { get; set; }

        public FileToUpdate(string filePath, Template template) : base(filePath, template)
        {
            FindLinkedFile(filePath);
        }

        public SegYLogParser SegyParser { get; } = new SegYLogParser();

        private void FindLinkedFile(string filePath)
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                var files = Directory.GetFiles(directory, "*.sgy");
                Regex r = new Regex(@"\d{4}-\d{2}-\d{2}-\d{2}-\d{2}-\d{2}");
                Match csvFileDateMatch = r.Match(filePath);
                if (csvFileDateMatch.Success)
                {
                    var csvFileDate = DateTime.ParseExact(csvFileDateMatch.Value, "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
                    foreach (var f in files)
                    {
                        Match m = r.Match(f);
                        var linkedFileDate = DateTime.ParseExact(m.Value, "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
                        if (linkedFileDate == csvFileDate)
                        {
                            LinkedFile = f;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }

        public void CheckCoveringStatus(List<PositioningSolutionFile> psfFiles)
        {
            if (psfFiles.Count == 0)
                return;
            var minTime = psfFiles.Min(f => f.StartTime);
            var maxTime = psfFiles.Max(f => f.EndTime);
            foreach (var f in psfFiles)
            {
                if ((f.StartTime <= StartTime && f.EndTime >= EndTime) || (f.StartTime <= EndTime && EndTime <= f.EndTime) || (f.StartTime <= StartTime && StartTime <= f.EndTime) || (StartTime <= f.StartTime && f.EndTime <= EndTime))
                    CoverageFiles.Add(f);
            }
            if (CoverageFiles.Count > 0 && minTime <= StartTime && maxTime >= EndTime)
                CoveringStatus = CoveringStatus.Covered;
            else if (CoverageFiles.Count > 0)
                CoveringStatus = CoveringStatus.PartiallyCovered;
            else
                CoveringStatus = CoveringStatus.NotCovered;
        }

        public Task<string> UpdateCoordinates(CancellationTokenSource source)
        {
            string message;
            if (CoverageFiles.Count == 0)
            {
                message = "Coverage files do not set";
                return Task.FromResult(message);
            }

            List<IGeoCoordinates> correctedCoordinates = new List<IGeoCoordinates>();
            var coverageCoordinates = new List<IGeoCoordinates>();
            foreach (var f in CoverageFiles)
                coverageCoordinates.AddRange(f.Coordinates);
            try
            {
                correctedCoordinates = Interpolator.SetPpkCorrectedCoordinates(Coordinates, coverageCoordinates, source);
                var ppkCorrectedFile = CreateFileWithPpkSuffix(FilePath);
                var result = Parser.CreatePpkCorrectedFile(FilePath, ppkCorrectedFile, correctedCoordinates, source);
                message = $"{FileName}: {result.CountOfReplacedLines} of {result.CountOfLines} were replaced;";
                log.Info(message);
            }
            catch (Exception e)
            {
                message = $"Error during updating {FilePath}: {e.Message}";
                log.Error(e.Message);
            }
            if (source.IsCancellationRequested)
                return Task.FromResult(message);
            try
            {
                if (LinkedFile != null)
                {
                    SegyParser.Parse(LinkedFile);
                    var ppkCorrectedSegyFile = CreateFileWithPpkSuffix(LinkedFile);
                    var result = SegyParser.CreatePpkCorrectedFile(LinkedFile, ppkCorrectedSegyFile, correctedCoordinates, source);
                    message += $"\n{FileName}: {result.CountOfReplacedLines} of {result.CountOfLines} were replaced";
                    log.Info(message);
                    return Task.FromResult(message);
                }
            }
            catch (Exception e)
            {
                message += $"\nError during updating {LinkedFile}: {e.Message}";
                log.Error(e.Message);
            }
            return Task.FromResult(message);
        }

        public void UnsetCoverageFile(PositioningSolutionFile file)
        {
            CoverageFiles.Remove(file);
            if (CoverageFiles.Count == 0)
                CoveringStatus = CoveringStatus.NotCovered;
        }

        private string CreateFileWithPpkSuffix(string fullPath)
        {
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"File {fullPath} does not exist");
            var newName = $"{fullPath.Insert(fullPath.LastIndexOf("."), $"-{PPK.ToLower()}")}";
            if (File.Exists(newName))
                File.Delete(newName);
            return newName;
        }

        public int CalculateCountOfLines()
        {
            return LinkedFile != null ? Coordinates.Count * 2 : Coordinates.Count;
        }
    }

    public enum CoveringStatus
    {
        Covered,
        NotCovered,
        PartiallyCovered
    }
}