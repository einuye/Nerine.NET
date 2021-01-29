using Nerine.Collections.Tables;
using System.Collections.Generic;
using Nerine.Exceptions;
using Nerine.IO;

namespace Nerine.Collections
{
    public class Collection
    {
        public List<Table> Tables = new List<Table>();

        public string Name;

        public Collection(string name)
        {
            Name = name;
        }

        public QueryBuilder StartBuilder()
        {
            return new QueryBuilder(this);
        }

        public Table? GetTable(string name)
        {
            return Tables.Find(x => x.Name == name);
        }

        public Table AddTable(string name, List<Column> columns)
        {
            if (Tables.Find(x => x.Name == name) != null)
                throw new TableExistsException("This table already exists!");

            var table = new Table()
            {
                Name = name,
                Columns = columns
            };
            Tables.Add(table);

            return table;
        }

        // TODO: Find a better way to do this.
        public byte[] ToBytes()
        {
            var bytes = new DynamicByteArray();
            var data = new DynamicByteArray();

            // write type
            bytes.Write<byte>((byte) BlockType.Collection);

            // write name
            data.Write<byte>(0x1F);
            data.Write<short>((short)Name.Length);
            data.Write<string>(Name);

            // write tables
            foreach (var table in Tables)
            {
                data.Write<byte[]>(table.ToBytes());
            }

            // write length
            bytes.Write<int>(data.Length);

            // write the data
            bytes.Write<byte[]>(data.Bytes);

            return bytes.Bytes;
        }
    }
}
