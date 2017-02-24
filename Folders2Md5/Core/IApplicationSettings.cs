using System.Collections.Specialized;

namespace Folders2Hash.Core
{
    /// <summary>
    /// </summary>
    public interface IApplicationSettings
    {
        /// <summary>
        /// </summary>
        string InitialDirectory { get; set; }

        /// <summary>
        /// </summary>
        string LoggingPath { get; set; }

        /// <summary>
        /// </summary>
        bool KeepFileExtension { get; set; }

        /// <summary>
        /// </summary>
        StringCollection CurrentHashAlgorithms { get; set; }
    }
}