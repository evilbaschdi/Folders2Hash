using System.Collections.Concurrent;

namespace Folders2Md5.Internal
{
    public class Configuration : IConfiguration
    {
        public bool CloseHiddenInstancesOnFinish { get; set; }

        public bool KeepFileExtension { get; set; }

        public ConcurrentDictionary<string, bool> PathsToScan { get; set; }

        public string LoggingPath { get; set; }

        public string HashType { get; set; }
    }
}