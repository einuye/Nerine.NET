using System.Collections.Generic;
using System.IO;
using System.Text;
using Nerine.Collections.Tables;
using Nerine.Data;
using NUnit.Framework;

namespace Nerine.Tests
{
    public class DatabaseTest
    {
        public static string DatabasePath = Directory.GetCurrentDirectory() + "/database.ndb";
        public static Database db = new Database(DatabasePath);

        // Database Tests
        [Test]
        public void TestAdd()
        {
            // add collection and a table
            var coll2 = db.AddCollection("test");
            var t21 = coll2.AddTable("test", new List<Column>()
            {
                new Column()
                {
                    Name = "string",
                    Type = StructureType.String,
                    Primary = true,
                },
                new Column()
                {
                    Name = "byte",
                    Type = StructureType.Byte,
                    Primary = true,
                    Default = (byte)1
                }
            });

            db.Save();
        }

        [Test]
        public void TestInsert()
        {
            var table = db.GetCollection("test").GetTable("test");

            table.Insert(new Dictionary<string, object>()
            {
                {"string", "string"},
                {"byte", (byte)2},
            });

            //db.Save();
        }
        [Test]
        public void TestQueryBuilder()
        {

            var coll = db.GetCollection("test");
            var qb = coll.StartBuilder();

            var query = qb
                .Select(new[] { "byte" })
                .From("test")
                .End();

            var query2 = qb
                .Select(new[] { "byte", "string" })
                .From("test")
                .Where(x =>
                {
                    return (byte) x["byte"] == (byte) 1;
                })
                .End();

            var query3 = qb
                .Insert(new Dictionary<string, object>()
                {
                    {"string", "stringy"},
                    {"byte", (byte)2},
                })
                .Into("test")
                .End();

            return;
        }

        // File-based Tests
        [Test]
        public void TestRead()
        {

            var file = File.OpenRead(DatabasePath);
            var reader = new BinaryReader(file);

            // check header
            Assert.AreEqual("NERINE", Encoding.UTF8.GetString(reader.ReadBytes(6)));
            //Assert.AreEqual(Database.DATABASE_FORMAT, reader.ReadInt32());
        }

        [Test]
        public void TestSave()
        {
            db.Save();
        }
    }
}