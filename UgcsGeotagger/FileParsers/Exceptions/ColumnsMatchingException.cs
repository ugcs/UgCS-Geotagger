using System;

namespace FileParsers.Exceptions
{
    public class ColumnsMatchingException : Exception
    {
        public ColumnsMatchingException()
        {
        }

        public ColumnsMatchingException(string message)
            : base(message)
        {
        }

        public ColumnsMatchingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}