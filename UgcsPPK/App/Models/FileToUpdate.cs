using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UgCSPPK.Models.Yaml;

namespace UgCSPPK.Models
{
    public class FileToUpdate : DataFile
    {
        private PositioningSolutionFile coverageFile;
        public CoveringStatus CoveringStatus { get; private set; }

        public string LinkedFile { get; set; }

        public FileToUpdate(string filePath, Template template) : base(filePath, template)
        {
            FindLinkedFile(filePath);
        }

        private void FindLinkedFile(string filePath)
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
                        LinkedFile = Path.GetFileName(f);
                        break;
                    }
                }
            }
        }

        public void CheckCoveringStatus(List<PositioningSolutionFile> psfFiles)
        {
            if (coverageFile != null)
                return;
            foreach (var f in psfFiles)
            {
                if (f.EndTime >= EndTime && f.StartTime <= StartTime)
                {
                    coverageFile = f;
                    CoveringStatus = CoveringStatus.Covered;
                }
                else if ((f.EndTime >= EndTime && f.StartTime > StartTime) || (f.EndTime < EndTime && f.StartTime <= StartTime))
                {
                    coverageFile = f;
                    CoveringStatus = CoveringStatus.PartiallyCovered;
                }
                else
                    CoveringStatus = CoveringStatus.NotCovered;
            }
        }
    }

    public enum CoveringStatus
    {
        Covered,
        NotCovered,
        PartiallyCovered
    }
}