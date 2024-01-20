using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XGPSaveExtractor
{
    class ContainerInfo
    {
        public ContainerInfo(string name, int number, Guid guid, FileInfo[] files)
        {
            Name = name;
            Number = number;
            Guid = guid;
            Files = files;
        }
        public string Name { get; }
        public int Number { get; }
        public Guid Guid { get; }
        public FileInfo[] Files { get; }

    }
}
