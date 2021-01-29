using System.Collections.Generic;   
using Nerine.IO;

namespace Nerine.Collections.Tables
{
    public class Table
    {
        public string Name;

        public List<Column> Columns = new List<Column>();

        // TODO: Find a better way to do this.
        public byte[] ToBytes()
        {
            var bytes = new DynamicByteArray();

            var columns = new DynamicByteArray();
            var columnData = new DynamicByteArray();

            var rows = new DynamicByteArray();
            var rowData = new DynamicByteArray();

            // plug table name in before columns
            columns.Write<byte>(0x1F); // this is a string.
            columns.Write<short>((short)Name.Length);
            columns.Write<string>(Name);

            // write types
            bytes.Write<byte>((byte) BlockType.Table);
            columns.Write<byte>((byte)BlockType.Columns);
            rows.Write<byte>((byte)BlockType.Rows);

            // write columns
            foreach (var column in Columns)
            {
                columnData.Write<byte>(Constants.SEPARATOR); //separator
                columnData.Write<byte>((byte)column.Type); // the type
                columnData.Write<byte>(0x1F); // this is a string.
                columnData.Write<short>((short)column.Name.Length);
                columnData.Write<string>(column.Name);
            }

            // write lengths
            columns.Write<int>(columnData.Length);
            rows.Write<int>(rowData.Length);

            // write data
            columns.Write<byte[]>(columnData.Bytes);
            rows.Write<byte[]>(rowData.Bytes);

            // write overall length
            bytes.Write<int>(columns.Length + rows.Length);

            // write overall data
            bytes.Write<byte[]>(columns.Bytes);
            bytes.Write<byte[]>(rows.Bytes);

            return bytes.Bytes;
        }
    }
}
