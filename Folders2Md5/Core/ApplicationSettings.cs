using System.Collections.Specialized;

namespace Folders2Hash.Core
{
    /// <summary>
    /// </summary>
    public class ApplicationSettings : IApplicationSettings
    {
        /// <summary>
        /// </summary>
        public string InitialDirectory
        {
            get
            {
                return string.IsNullOrWhiteSpace(Properties.Settings.Default.InitialDirectory)
                    ? ""
                    : Properties.Settings.Default.InitialDirectory;
            }
            set
            {
                Properties.Settings.Default.InitialDirectory = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// </summary>
        public string LoggingPath
        {
            get
            {
                return string.IsNullOrWhiteSpace(Properties.Settings.Default.LoggingPath)
                    ? ""
                    : Properties.Settings.Default.LoggingPath;
            }
            set
            {
                Properties.Settings.Default.LoggingPath = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// </summary>
        public bool KeepFileExtension
        {
            get { return Properties.Settings.Default.KeepFileExtension; }
            set
            {
                Properties.Settings.Default.KeepFileExtension = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// </summary>
        public StringCollection CurrentHashAlgorithms
        {
            get
            {
                if (Properties.Settings.Default.CurrentHashAlgorithms != null && Properties.Settings.Default.CurrentHashAlgorithms.Count > 0)
                {
                    return Properties.Settings.Default.CurrentHashAlgorithms;
                }
                return new StringCollection
                       {
                           "md5"
                       };
            }
            set
            {
                Properties.Settings.Default.CurrentHashAlgorithms = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}