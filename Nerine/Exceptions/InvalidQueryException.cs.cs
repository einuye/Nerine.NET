using System;

namespace Nerine.Exceptions
{
    public class InvalidQueryException : Exception
    {
        public InvalidQueryException(string message)
            : base(message)
        {

        }
    }
}