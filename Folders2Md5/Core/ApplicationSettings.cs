namespace Folders2Md5.Core
{
    public class ApplicationSettings : IApplicationSettings
    {
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

        public bool KeepFileExtension
        {
            get { return Properties.Settings.Default.KeepFileExtension; }
            set
            {
                Properties.Settings.Default.KeepFileExtension = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}