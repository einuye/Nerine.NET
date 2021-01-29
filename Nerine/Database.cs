 using System;
 using System.Collections.Generic;
 using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
 using Nerine.Collections;
 using Nerine.Collections.Tables;
 using Nerine.Data;
using Nerine.Exceptions;
using Nerine.IO;

namespace Nerine
{
    public class Database
    {
        public const int DATABASE_VERSION = 1;
        public const int DATABASE_FORMAT = 1;
        public const int TOO_OLD_THRESHOLD = 3;

        private string filePath;
        private Stream stream;

        private BinaryWriter writer;
        private BinaryReader reader;

        #region Database Information
        public int FormatVersion;

        public long CreationDate;

        public long LastEditedDate;

        public List<Collection> Collections = new List<Collection>();

        public byte[] Sha256Checksum;
        #endregion

        public Database(string path)
        {
            filePath = path;

            // create database if it doesnt exist
            if (!File.Exists(path))
            {
                WriteHeader();
            }
            
            // ready filestreams
            stream = File.OpenRead(filePath);

            if (stream.Length == 0) // no content; write basic header
            {
                stream.Close();
                WriteHeader();
            }
            else
            {
                stream.Close(); // unnecessary

                //validate database
                if (!ValidateHeader())
                    throw new InvalidDatabaseException("This database is invalid.");

                // read database
                CreateRead(delegate(BinaryReader reader)
                {
                    // ignore first 10 bytes
                    stream.Position += 10;

                    // read blocks
                    while (true)
                    {
                        try
                        {
                            var type = reader.ReadByte();
                            var length = reader.ReadInt32();

                            switch (type)
                            {
                                // Metadata Block
                                case (byte)BlockType.Metadata:
                                    CreationDate = reader.ReadInt64();
                                    LastEditedDate = reader.ReadInt64();
                                    FormatVersion = reader.ReadInt32();
                                    break;

                                // Collection Block
                                case (byte)BlockType.Collection:
                                    ReadDatabase();
                                    break;

                                case (byte)BlockType.EndOfFile:
                                    stream.Position -= 4;
                                    Sha256Checksum = reader.ReadBytes(32);
                                    break;
                                default:
                                    throw new InvalidBlockTypeException("An invalid block was parsed!");
                            }
                        }
                        catch (IOException e)
                        {
                            // end of file.
                            break;
                        }
                    }

                    return true;
                });
            }

            return;
        }

        #region Database Methods
        public Collection? GetCollection(string name)
        {
            return Collections.Find(x => x.Name == name);
        }

        public Collection AddCollection(Collection coll)
        {
            if (Collections.Find(x => x.Name == coll.Name) != null)
                throw new CollectionExistsException("This collection already exists!");

            // add collection
            Collections.Add(coll);

            return coll;
        }

        public Collection AddCollection(string name)
        {
            if (Collections.Find(x => x.Name == name) != null)
                throw new CollectionExistsException("This collection already exists!");
            
            // add collection
            var coll = new Collection(name);
            Collections.Add(coll);

            return coll;
        }

        #endregion

        #region Private Database Reading

        private void ReadDatabase()
        {
            // parse database name
            reader.ReadByte(); // ignore first byte;
            var strlen = reader.ReadInt16();
            stream.Position -= 3;

            var name = (DatabaseString) Structure.Construct(StructureType.String, reader.ReadBytes(3 + strlen));

            var coll = new Collection(name.Value);

            Debug.WriteLine("create db");

            // read sub blocks
            while (true)
            {
                var subtype = reader.ReadByte();
                var sublength = reader.ReadInt32();

                var done = false;

                switch (subtype)
                {
                    case (byte) BlockType.Table: // a table
                        Debug.WriteLine("read table");
                        ReadTable(coll);
                        break;

                    default: // end of database, most likely
                        stream.Position -= 5;
                        done = true;
                        break;
                }

                if (done)
                {
                    break;
                }
            }

            AddCollection(coll);
        }

        private void ReadTable(Collection coll)
        {
            // parse table name
            reader.ReadByte(); // ignore first byte;
            var strlen = reader.ReadInt16();
            stream.Position -= 3;

            var name = (DatabaseString)Structure.Construct(StructureType.String, reader.ReadBytes(3 + strlen));
            var col = new List<Column>();
            var row = new List<Row>();

            var done = false;

            // read sub blocks
            while (true)
            {
                var subtype = reader.ReadByte();
                var sublength = reader.ReadInt32();

                switch (subtype)
                {
                    case (byte)BlockType.Rows:
                        Debug.WriteLine("read rows");
                        ReadRows(col, row);
                        break;

                    case (byte)BlockType.Columns:
                        Debug.WriteLine("read columns");
                        ReadColumns(col);
                        break;

                    default: // end of table, most likely
                        stream.Position -= 5;
                        done = true;
                        break;

                }

                if (done)
                {
                    Debug.WriteLine("Done.");
                    break;
                }
            }

            coll.Tables.Add(new Table()
            {
                Name = name.Value,
                Columns = col,
                Rows = row
            });
        }

        private void ReadColumns(List<Column> col)
        {
            while (true)
            {
                if (reader.ReadByte() == Constants.SEPARATOR)
                {
                    var type = (StructureType)reader.ReadByte();
                    var primary = BitConverter.ToBoolean(reader.ReadBytes(1));
                    var nullable = BitConverter.ToBoolean(reader.ReadBytes(1));

                    // read column name
                    reader.ReadByte(); // ignore first byte;
                    var strlen2 = reader.ReadInt16();
                    stream.Position -= 3;

                    var name2 = (DatabaseString)Structure.Construct(StructureType.String, reader.ReadBytes(3 + strlen2));

                    var colm = new Column()
                    {
                        Name = name2.Value,
                        Type = type,
                        Primary = primary,
                        Nullable = nullable
                    };

                    // there is a default
                    if (reader.ReadByte() == 0x1E)
                    {
                        object def;
                        if (type == StructureType.String)
                        {
                            // read default str
                            reader.ReadByte(); // ignore first byte;
                            var strlen3 = reader.ReadInt16();
                            stream.Position -= 3;
                            var name3 = (DatabaseString)Structure.Construct(StructureType.String, reader.ReadBytes(3 + strlen3));

                            def = name3.Value;
                        }
                        else
                        {
                            def = Structure.Construct(type, reader.ReadBytes(Structure.GetSize(type))).Value;
                        }

                        colm.Default = def;
                    }
                    else
                    {
                        stream.Position -= 1;
                    }
                       

                    col.Add(colm);
                }
                else
                {
                    stream.Position -= 1;
                    break;
                }
            }
        }

        private void ReadRows(List<Column> col, List<Row> row)
        {
            while (true)
            {
                var sep = reader.ReadByte();
                if (sep == Constants.SEPARATOR)
                {
                    if (sep > 0xF0 || sep != Constants.SEPARATOR)
                    {
                        stream.Position -= 1;
                        break;
                    }

                    var i = 0;
                    var addableRow = new Row();

                    while (true)
                    {

                        try
                        {
                            Debug.WriteLine(i);
                            if (col[i] == null)
                            {
                                break;
                            }

                            var column = col[i];

                            if (reader.ReadByte() == 0x00)
                            {
                                if (column.Default == null || !column.Nullable)
                                    throw new InvalidDatabaseException(
                                        $"A row has a null value, while column {column.Name} is not nullable.");

                                addableRow.Values.Add(null);
                            }
                            else
                            {
                                stream.Position -= 1;

                                // parse data
                                if (column.Type == StructureType.String)
                                {
                                    reader.ReadByte(); // ignore first byte;
                                    var strlen3 = reader.ReadInt16();
                                    stream.Position -= 3;
                                    var name3 = (DatabaseString) Structure.Construct(StructureType.String,
                                        reader.ReadBytes(3 + strlen3));

                                    addableRow.Values.Add(name3.Value);
                                }
                                else if (column.Type == StructureType.Byte)
                                {
                                    addableRow.Values.Add(reader.ReadByte());
                                }
                            }

                            i++;
                        }
                        catch (Exception e)
                        {
                            // oh ok
                            break;
                        }
                    }

                    row.Add(addableRow);
                }
                else
                {
                    stream.Position -= 1;
                    break;
                }
            }
        }

        #endregion

        public void Save()
        {
            // save metadata
            WriteMetadata();

            using (var memoryStream = new MemoryStream())
            {

                CreateRead(delegate (BinaryReader reader)
                {
                    // copy first 35 bytes to memory stream
                    var bytes = reader.ReadBytes(35);
                    memoryStream.Write(bytes, 0, bytes.Length);

                    return true;
                });


                // save database data
                CreateWrite(memoryStream, delegate(BinaryWriter writer)
                {
                    // write collections
                    foreach (var collection in Collections)
                    {
                        writer.Write(collection.ToBytes());
                    }

                    // write end-of-file
                    using (SHA256 s256 = SHA256.Create())
                    {
                        var hash = s256.ComputeHash(memoryStream);
                        writer.Write((byte)0xFF);
                        writer.Write(hash);
                    }

                    // write to file
                    using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(memoryStream.ToArray(), 0, memoryStream.ToArray().Length);
                    }

                    return true;
                });
            }
        }

        #region File Functions
        private (bool, bool) CreateReadWrite(Stream readStream, Stream writeStream, Func<BinaryReader, BinaryWriter, (bool, bool)> function)
        {
            writer = new BinaryWriter(readStream);
            reader = new BinaryReader(writeStream);

            var res = function(reader, writer);

            writer.Flush();

            reader.Close();
            writer.Close();
            return res;
        }

        private (bool, bool) CreateReadWrite(Stream stream, Func<BinaryReader, BinaryWriter, (bool, bool)> function)
        {
            writer = new BinaryWriter(stream);
            reader = new BinaryReader(stream);

            var res = function(reader, writer);

            writer.Flush();

            reader.Close();
            writer.Close();
            return res;
        }
        private (bool, bool) CreateReadWrite(Func<BinaryReader, BinaryWriter, (bool, bool)> function)
        {
            stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);

            writer = new BinaryWriter(stream);
            reader = new BinaryReader(stream);

            var res = function(reader, writer);

            writer.Flush();

            reader.Close();
            writer.Close();
            return res;
        }

        private bool CreateRead(Func<BinaryReader, bool> function)
        {
            stream = File.OpenRead(filePath);
            reader = new BinaryReader(stream);

            var res = function(reader);

            stream.Close();
            reader.Close();
            return res;
        }

        private bool CreateWrite(Stream writeStream, Func<BinaryWriter, bool> function)
        {
            writer = new BinaryWriter(writeStream);

            var res = function(writer);

            writer.Flush();

            stream.Close();
            writer.Close();
            return res;
        }

        private bool CreateWrite(Func<BinaryWriter, bool> function)
        {
            stream = File.OpenWrite(filePath);
            writer = new BinaryWriter(stream);

            var res = function(writer);

            writer.Flush();

            stream.Close();
            writer.Close();
            return res;
        }

        private void WriteMetadata()
        {
            // write metadata
            CreateReadWrite(delegate (BinaryReader reader, BinaryWriter writer)
            {
                // start after header
                stream.Position = 10;

                try
                {
                    var meta = reader.ReadByte();

                    if (meta != 0xF1)
                        throw new InvalidDatabaseException("The metadata block is invalid.");

                    // read creation time & length (bit skip);
                    reader.ReadInt32();
                    reader.ReadInt64();

                    // write edit time to metadata
                    writer.Write(DateTimeOffset.Now.ToUnixTimeSeconds());
                }
                catch (IOException e)
                {
                    // couldnt find metadata header, so we'll write it
                    writer.Write((byte)BlockType.Metadata); // write type

                    // write data
                    var bytes = new DynamicByteArray();

                    bytes.Write<long>(DateTimeOffset.Now.ToUnixTimeSeconds()); // create date
                    bytes.Write<long>(DateTimeOffset.Now.ToUnixTimeSeconds()); // edit time
                    bytes.Write<int>(DATABASE_VERSION);

                    // write chunk
                    writer.Write(bytes.Length); // write length
                    writer.Write(bytes.Bytes); // write chunk
                }

                // save first major bytes to memory stream
                return (true, true);
            });
        }
        private void WriteHeader()
        {
            CreateWrite(delegate(BinaryWriter writer)
            {
                // write base header
                var header = Encoding.UTF8.GetBytes("NERINE");

                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(header);

                writer.Write(header);
                writer.Write(DATABASE_FORMAT);

                return true;
            });
        }

        private bool ValidateHeader()
        {
            return CreateRead(delegate(BinaryReader reader)
            {
                // first hand verification of base length
                if (reader.BaseStream.Length == 0 || reader.BaseStream.Length < 9)
                    return false;

                string name = Encoding.UTF8.GetString(reader.ReadBytes(6));
                int format = reader.ReadInt32();

                // set a threshold for db formats
                if (DATABASE_FORMAT - format >= TOO_OLD_THRESHOLD)
                    throw new InvalidDatabaseException("The format of this database is too old for use!");

                if (name != "NERINE")
                    return false;

                return true;
            });
        }
        #endregion
    }

    public enum BlockType
    {
        Metadata = 0xF1,
        Collection = 0xF2,

        Table = 0xFA,

        // Sub-block Types
        Columns = 0xE1,
        Rows = 0xE2,

        // End of file
        EndOfFile = 0xFF
    }
}