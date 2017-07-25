using System.Collections.Specialized;
using Folders2Hash.Properties;

namespace Folders2Hash.Core
{
    /// <inheritdoc />
    public class ApplicationSettings : IApplicationSettings
    {
        /// <inheritdoc />
        public string InitialDirectory
        {
            get => string.IsNullOrWhiteSpace(Settings.Default.InitialDirectory)
                ? ""
                : Settings.Default.InitialDirectory;
            set
            {
                Settings.Default.InitialDirectory = value;
                Settings.Default.Save();
            }
        }

        /// <inheritdoc />
        public string LoggingPath
        {
            get => string.IsNullOrWhiteSpace(Settings.Default.LoggingPath)
                ? ""
                : Settings.Default.LoggingPath;
            set
            {
                Settings.Default.LoggingPath = value;
                Settings.Default.Save();
            }
        }

        /// <inheritdoc />
        public bool KeepFileExtension
        {
            get => Settings.Default.KeepFileExtension;
            set
            {
                Settings.Default.KeepFileExtension = value;
                Settings.Default.Save();
            }
        }

        /// <inheritdoc />
        public StringCollection CurrentHashAlgorithms
        {
            get
            {
                if (Settings.Default.CurrentHashAlgorithms != null && Settings.Default.CurrentHashAlgorithms.Count > 0)
                {
                    return Settings.Default.CurrentHashAlgorithms;
                }
                return new StringCollection
                       {
                           "md5"
                       };
            }
            set
            {
                Settings.Default.CurrentHashAlgorithms = value;
                Settings.Default.Save();
            }
        }
    }
}