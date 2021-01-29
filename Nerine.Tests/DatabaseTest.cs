using System.IO;
using System.Text;
using NUnit.Framework;

namespace Nerine.Tests
{
    public class DatabaseTest
    {
        public static string DatabasePath = Directory.GetCurrentDirectory() + "/database.ndb";
        public static Database db = new Database(DatabasePath);
        [Test]
        public void TestCreation()
        {

            var file = File.OpenRead(DatabasePath);
            var reader = new BinaryReader(file);

            // check header
            Assert.AreEqual("NERINE", Encoding.UTF8.GetString(reader.ReadBytes(6)));
            Assert.AreEqual(Database.DATABASE_FORMAT, reader.ReadInt32());
        }

        [Test]
        public void TestSave()
        {
            db.Save();
        }
    }
}