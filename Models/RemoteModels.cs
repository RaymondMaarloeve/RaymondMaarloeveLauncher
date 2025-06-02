using System.Collections.Generic;

namespace RaymondMaarloeveLauncher.Models
{
    /// <summary>
    /// Represents information about a remote file, including its name and path.
    /// </summary>
    public class FileInfo
    {
        /// <summary>
        /// The name of the file.
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// The path to the file.
        /// </summary>
        public string path { get; set; }
    }

    /// <summary>
    /// Represents the root object for a remote file listing response, including the list of files and the success status.
    /// </summary>
    public class Root
    {
        /// <summary>
        /// The list of files returned by the server.
        /// </summary>
        public List<FileInfo> files { get; set; }
        /// <summary>
        /// Indicates whether the request was successful.
        /// </summary>
        public bool success { get; set; }
    }
}
