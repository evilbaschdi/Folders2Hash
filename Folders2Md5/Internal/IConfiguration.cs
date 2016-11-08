using System.Collections.Concurrent;

namespace Folders2Md5.Internal
{
    public interface IConfiguration
    {
        bool CloseHiddenInstancesOnFinish { get; set; }

        bool KeepFileExtension { get; set; }

        ConcurrentDictionary<string, bool> PathsToScan { get; set; }

        string LoggingPath { get; set; }

        string HashType { get; set; }
    }
}