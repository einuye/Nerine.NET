using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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