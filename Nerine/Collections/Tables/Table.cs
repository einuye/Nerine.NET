using System;
using System.Collections.Generic;
using System.Diagnostics;
using Nerine.Data;
using Nerine.Exceptions;
using Nerine.IO;

namespace Nerine.Collections.Tables
{
    public class Table
    {
        public string Name;

        public List<Column> Columns = new List<Column>();
        public List<Row> Rows = new List<Row>();

        public Row Select(Predicate<Row> function)
        {
            return Rows.Find(function);
        }

        public Row Insert(Dictionary<string, Object> values)
        {
            // size validato
            if (values.Count < Columns.Count)
                throw new InvalidQueryException("The amount of values provided is less than the columns of table {Name}.");

            // validate individual values
            for (int i = 0; i < Columns.Count; i++)
            {
                var column = Columns[i];

                if (column.Primary)
                {
                    if (Rows.Find(x => x.Values[column.Name] != null) != null)
                        throw new InvalidQueryException(
                            $"Column ${column.Name} is set as unique, and a duplicate row was attempted to be added.");
                }

                if (values[column.Name] == null || 
                    values[column.Name].GetType() == typeof(string) && string.IsNullOrEmpty(values[column.Name] as string))
                {
                    // there is no default, and the value is null.
                    if (column.Default == null || !column.Nullable)
                        throw new InvalidQueryException($"Column {column.Name} cannot be null, or empty.");

                    values.Add(column.Name, column.Default);
                }
                else
                {
                    var val = values[column.Name];

                    if (val.GetType() != Structure.GetType(column.Type))
                        throw new InvalidQueryException($"An invalid value was provided for column {column.Name}");
                }
            }

            var row = new Row()
            {
                Values = values
            };

            Rows.Add(row);
            return row;
        }

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
                columnData.Write<byte>(Convert.ToByte(column.Primary)); // primary bit
                columnData.Write<byte>(Convert.ToByte(column.Nullable)); // is-nullable bit
                columnData.Write<byte>(0x1F); // this is a string.
                columnData.Write<short>((short)column.Name.Length);
                columnData.Write<string>(column.Name);

                if (column.Default != null)
                {
                    columnData.Write<byte>(0x1E); // there is a default present.
                    if (column.Type == StructureType.String)
                    {
                        columnData.Write<byte>(0x1F); // this default is a string.
                        columnData.Write<short>((short)(column.Default as string).Length);
                        columnData.Write<string>(column.Default as string);
                    }
                    else
                    {
                        if (Structure.GetType(column.Type) == typeof(byte))
                        {
                            columnData.Write<byte>((byte)column.Default);
                        }
                    }
                }
            }

            foreach (var row in Rows)
            {
                rowData.Write<byte>(Constants.SEPARATOR); //separator
                foreach (var val in row.Values)
                {
                    if (val.GetType() == typeof(string))
                    {
                        rowData.Write<byte>(0x1F); // this is a string.
                        rowData.Write<short>((short)(val.Value as string).Length);
                        rowData.Write<string>(val.Value as string);
                    } else if (val.GetType() == typeof(byte))
                    {
                        rowData.Write<byte>((byte)val.Value); //separator
                    }
                }
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
