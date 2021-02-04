using System;

namespace Nerine.Exceptions
{
    public class TableExistsException : Exception
    {
        public TableExistsException(string message)
            : base(message)
        {

        }
    }
}