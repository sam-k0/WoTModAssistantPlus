using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoTModAssistant
{
    public class GameVersion
    {
        public string FullPath { get; set; }
        public string Version { get; set; }
        public GameVersion(string fullPath, string version)
        {
            FullPath = fullPath;
            Version = version;
        }

    }
}
