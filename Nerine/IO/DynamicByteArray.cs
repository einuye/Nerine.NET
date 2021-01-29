using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Nerine.IO
{
    public class DynamicByteArray
    {
        public byte[] Bytes;
        public int Length => Bytes.Length;
        public int Offset = 0;

        private BinaryWriter writer;

        public DynamicByteArray()
        {
            Bytes = new byte[0];
        }

        public DynamicByteArray(byte[] bytes)
        {
            Bytes = bytes;
        }

        public void Write(byte[] bytes)
        {
            if (bytes == null)
                return;

            // write data
            byte[] res = new byte[Bytes.Length + bytes.Length];

            // copy main to result
            Array.Copy(Bytes, 0, res, 0, Bytes.Length);

            // copy data to res
            Array.Copy(bytes, 0, res, Offset, bytes.Length);

            // add offset
            Offset += bytes.Length;

            // reset the byte array
            Bytes = res;
        }

        public void Write(byte[] bytes, int offset)
        {
            if (bytes == null)
                return;

            if (offset < Bytes.Length)
            {
                // write data
                byte[] res = new byte[Bytes.Length + bytes.Length];

                // copy main to result
                Array.Copy(Bytes, 0, res, 0, offset);

                // copy data to res
                Array.Copy(bytes, 0, res, offset, bytes.Length);

                // copy data after
                Array.Copy(Bytes, offset, res, offset + bytes.Length, Bytes.Length - bytes.Length);

                // add offset
                Offset += bytes.Length;

                // reset the byte array
                Bytes = res;
            }
            else
            {
                Write(bytes);
            }
        }

        public void Write<T>(T data)
        {
            if (data == null)
                return;
            
            // get the data
            byte[] bytes = GetBytes<T>(data);

            // write data
            Write(bytes);
        }


        public void Write<T>(T data, int offset)
        {
            if (data == null)
                return;

            byte[] bytes = GetBytes<T>(data);

            if (offset < Bytes.Length)
            {
                Write(bytes, offset);
            }
            else
            {
                Write(bytes);
            }
        }

        private byte[] GetBytes<T>(T data)
        {

            byte[] setter = new byte[4];

            if (typeof(T) == typeof(string))
            {
                setter = Encoding.UTF8.GetBytes(data as string);
            }
            else if (typeof(T) == typeof(byte[]))
            {
                setter = data as byte[];
            }
            else
            {
                // data
                int size = SizeOf<T>();

                // create two buffers
                setter = new byte[size]; // managed buffer
                var ptr = Marshal.AllocHGlobal(size); // unmanaged buffer

                // copy bytes
                Marshal.StructureToPtr(data, ptr, false);

                // Copy data from unmanaged to managed
                Marshal.Copy(ptr, setter, 0, size);

                // Release unmanaged memory.
                Marshal.FreeHGlobal(ptr);
            }

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(setter);

            return setter;
        }
        public static int SizeOf(Type type)
        {

            return Marshal.SizeOf(type);
        }

        public static int SizeOf<T>()
        {
            Type type = typeof(T);

            return Marshal.SizeOf(type);
        }

    }
}
