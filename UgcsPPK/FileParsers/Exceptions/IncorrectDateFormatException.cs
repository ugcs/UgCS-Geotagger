using System;

namespace FileParsers.Exceptions
{
    public class IncorrectDateFormatException : Exception
    {
        public IncorrectDateFormatException()
        {
        }

        public IncorrectDateFormatException(string message)
            : base(message)
        {
        }

        public IncorrectDateFormatException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}