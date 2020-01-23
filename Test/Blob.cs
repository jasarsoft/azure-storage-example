using System.IO;

namespace Test
{
    public class Blob
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public Stream Stream { get; set; }

        public Blob(string name, Stream stream, string type)
        {
            Name = name;
            Stream = stream;
            Type = type;
        }
    }
}