using System;

namespace UgCSGeotagger.Models
{
    public interface IDataFile
    {
        public string FileName { get; }
        public string TypeOfFile { get; }
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public bool IsValid { get; }
    }
}