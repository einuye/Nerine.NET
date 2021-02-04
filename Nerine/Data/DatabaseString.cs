using System.IO;
using System.Text;

namespace Nerine.Data
{
    public class DatabaseString : Structure
    {
        public string Value;

        public DatabaseString(byte[] bytes)
        {
            using (var bw = new BinaryReader(new MemoryStream(bytes)))
            {
                if (bw.ReadByte() == 0x1F) // string
                {
                    var length = bw.ReadInt16();
                    Value = Encoding.UTF8.GetString(bw.ReadBytes(length));
                }

                bw.Close();
            }
        }
    }
}
