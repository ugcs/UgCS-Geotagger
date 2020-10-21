using System;

namespace UgCSPPK.Models
{
    public interface IDataFile
    {
        public string Name { get; }
        public string TypeOfFile { get; }
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public bool IsValid { get; }
    }
}