using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Folders2Md5.Models
{
    /// <summary>
    /// </summary>
    public class Configuration : IConfiguration
    {
        /// <summary>
        /// </summary>
        public bool CloseHiddenInstancesOnFinish { get; set; }

        /// <summary>
        /// </summary>
        public bool KeepFileExtension { get; set; }

        /// <summary>
        /// </summary>
        public ConcurrentDictionary<string, bool> PathsToScan { get; set; }

        /// <summary>
        /// </summary>
        public string LoggingPath { get; set; }

        /// <summary>
        /// </summary>
        public List<string> HashTypes { get; set; }
        
    }
}