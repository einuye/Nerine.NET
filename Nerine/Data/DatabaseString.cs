
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Nerine.Data
{
    public class DatabaseString : Structure
    {
        public string String;

        public DatabaseString(byte[] bytes)
        {
            Debug.WriteLine(bytes.Length);
            using (var bw = new BinaryReader(new MemoryStream(bytes)))
            {
                if (bw.ReadByte() == 0x1F) // string
                {
                    var length = bw.ReadInt16();
                    String = Encoding.UTF8.GetString(bw.ReadBytes(length));
                }

                bw.Close();
            }
        }
    }
}
