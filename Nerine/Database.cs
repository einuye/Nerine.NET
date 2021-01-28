using System;
using System.IO;

namespace Nerine
{
    public class Database
    {
        private string filePath;
        private Stream writeStream;
        private Stream readStream;

        private BinaryWriter writer;
        
        public Database(string path)
        {
            filePath = path;

            // create database if it doesnt exist
            if (!File.Exists(path))
            {
                File.Create(path);
            }
            
            // ready filestreams
            readStream = File.OpenRead(filePath);

            if (readStream.Length == 0) // no content; write basic header
            {
                readStream.Close();
                writeStream = File.OpenWrite(filePath);
                writer = new BinaryWriter(writeStream);
                
                // write base header
                var header = new byte[] { 31, 35, 35, 4 };
                
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(header);
                
                writer.Write(header);
                writer.Close();
            }
            else
            {
                // do stuff here soon xd
            }

        }
    }
}