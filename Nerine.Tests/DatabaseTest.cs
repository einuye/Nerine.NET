using System.IO;
using NUnit.Framework;

namespace Nerine.Tests
{
    public class DatabaseTest
    {
        [Test]
        public void TestCreation()
        {
            var path = Directory.GetCurrentDirectory() + "/test.ner";
            new Database(path);

            var file = File.OpenRead(path);
            var reader = new BinaryReader(file);
            
            // check header
            Assert.AreEqual(69411615, reader.ReadInt32());
        }
    }
}