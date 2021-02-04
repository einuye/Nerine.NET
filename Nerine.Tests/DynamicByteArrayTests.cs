using Nerine.IO;
using NUnit.Framework;


namespace Nerine.Tests
{
    public class DynamicByteArrayTests
    {
        [Test]
        public void WriteByOffset()
        {
            var bytes = new DynamicByteArray(new byte[] { 1, 3, 5 });

            // write a 2
            bytes.Write<byte>(2, 1);

            // and then a 4
            bytes.Write<byte>(4, 3);

            Assert.AreEqual(2, bytes.Bytes[1]);
            Assert.AreEqual(4, bytes.Bytes[3]);
        }
    }
}
