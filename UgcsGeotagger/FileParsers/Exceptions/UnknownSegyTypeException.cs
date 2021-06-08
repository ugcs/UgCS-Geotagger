using System;

namespace FileParsers.Exceptions
{
    public class UnknownSegyTypeException : Exception
    {
        public UnknownSegyTypeException()
        {
        }

        public UnknownSegyTypeException(string message)
            : base(message)
        {
        }

        public UnknownSegyTypeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}