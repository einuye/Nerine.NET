using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
