using System.IO;

namespace Nerine.Data
{
    public class DatabaseByte : Structure
    {
        public byte Value;
        public DatabaseByte(byte[] bytes)
        {
            using (var bw = new BinaryReader(new MemoryStream(bytes)))
            {
                Value = bw.ReadByte();

                bw.Close();
            }
        }
    }
}
