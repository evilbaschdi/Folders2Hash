using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Folders2Md5.Models
{
    /// <summary>
    /// 
    /// </summary>
    public interface IConfiguration
    {/// <summary>
    /// 
    /// </summary>
        bool CloseHiddenInstancesOnFinish { get; set; }
        /// <summary>
        /// 
        /// </summary>

        bool KeepFileExtension { get; set; }
        /// <summary>
        /// 
        /// </summary>
        ConcurrentDictionary<string, bool> PathsToScan { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string LoggingPath { get; set; }
        /// <summary>
        /// 
        /// </summary>
        List<string> HashTypes { get; set; }
    }
}