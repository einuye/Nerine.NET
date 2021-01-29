using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nerine.Exceptions
{
    public class InvalidDatabaseException : Exception
    {
        public InvalidDatabaseException(string message)
            : base(message)
        {

        }
    }
}
