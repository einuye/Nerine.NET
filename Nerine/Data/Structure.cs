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
        public object Value = null;

        public static int GetSize(StructureType type)
        {
            switch (type)
            {
                case StructureType.Byte:
                    return 1;
            }

            return 1;
        }
        public static Type GetType(StructureType type)
        {
            switch (type)
            {
                case StructureType.Byte:
                    return typeof(byte);
            }

            return null;
        }

        public static Structure Construct(StructureType type, byte[] bytes)
        {
            switch (type)
            {
                case StructureType.String:
                    return new DatabaseString(bytes);

                case StructureType.Byte:
                    return new DatabaseByte(bytes);
            }

            // default
            return new Structure();
        }
    }

    public enum StructureType
    {
        String = 1,
        Byte = 2,
    }
}
