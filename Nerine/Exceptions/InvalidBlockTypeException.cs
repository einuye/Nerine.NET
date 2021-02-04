using System;

namespace Nerine.Exceptions
{
    public class InvalidBlockTypeException : Exception
    {
        public InvalidBlockTypeException(string message)
            : base(message)
        {

        }
    }
}