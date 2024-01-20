using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XGPSaveExtractor
{
    class FileInfo
    {
        public Guid Guid { get; }
        public string Path { get; }
        public string Name { get; }

        public FileInfo(string name, Guid guid, string path)
        {
            Name = name;
            Guid = guid;
            Path = path;
        }
    }
}
