using System;

namespace Nerine.Exceptions
{
    public class CollectionExistsException : Exception
    {
        public CollectionExistsException(string message)
            : base(message)
        {

        }
    }
}