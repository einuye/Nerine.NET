using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Nerine.Data
{
    public class Structure<T>
    {
        /*public static Structure<T> Decode<T>()
        {
            if (typeof)
        }*/

        public static int SizeOf<T>()
        {
            Type type = typeof(T);

            return Marshal.SizeOf(type);
        }

        public static T Transfer<T>(byte[] data)
        {
            Type type = typeof(T);
            object d;

            if (type == typeof(byte))
                d = data[0];
            else if (type == typeof(ushort))
                d = BitConverter.ToUInt16(data, 0);
            else if (type == typeof(short))
                d = BitConverter.ToInt16(data, 0);
            else if (type == typeof(uint))
                d = BitConverter.ToUInt32(data, 0);
            else if (type == typeof(int))
                d = BitConverter.ToInt32(data, 0);
            else if (type == typeof(ulong))
                d = BitConverter.ToUInt64(data, 0);
            else if (type == typeof(long))
                d = BitConverter.ToInt64(data, 0);
            else if (type == typeof(float))
                d = BitConverter.ToSingle(data, 0);
            else if (type == typeof(double))
                d = BitConverter.ToDouble(data, 0); // oh no!!!!! double floating point values suck
            else if (type == typeof(string))
                d = Encoding.UTF8.GetString(data);
            else
                d = default(T);

            return (T)d;
        }
    }

    public enum StructureType
    {
        String = 1,
    }
}
