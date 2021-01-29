using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Nerine.Data
{
    public class Structure
    {
        public static Structure Construct(StructureType type, byte[] bytes)
        {
            switch (type)
            {
                case StructureType.String:
                    return new DatabaseString(bytes);
            }
            return new Structure();
        }
    }

    public enum StructureType
    {
        String = 1,
    }
}
