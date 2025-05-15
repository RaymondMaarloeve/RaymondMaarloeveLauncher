using System.Collections.Generic;

namespace RaymondMaarloeveLauncher.Models
{
    public class FileInfo
    {
        public string name { get; set; }
        public string path { get; set; }
    }

    public class Root
    {
        public List<FileInfo> files { get; set; }
        public bool success { get; set; }
    }
}
