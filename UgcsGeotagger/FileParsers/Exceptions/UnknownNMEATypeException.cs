using System;

namespace FileParsers.Exceptions
{
    public class UnknownNMEATypeException : Exception
    {
        public UnknownNMEATypeException()
        {
        }

        public UnknownNMEATypeException(string message)
            : base(message)
        {
        }

        public UnknownNMEATypeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}