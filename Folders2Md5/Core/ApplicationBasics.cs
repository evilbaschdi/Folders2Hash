using System.Windows.Forms;

namespace Folders2Md5.Core
{
    public class ApplicationBasics : IApplicationBasics
    {
        public void BrowseFolder()
        {
            var folderDialog = new FolderBrowserDialog
            {
                SelectedPath = GetInitialDirectory()
            };

            var result = folderDialog.ShowDialog();
            if(result.ToString() != "OK")
            {
                return;
            }

            Properties.Settings.Default.InitialDirectory = folderDialog.SelectedPath;
            Properties.Settings.Default.Save();
        }

        public void BrowseLoggingFolder()
        {
            var folderDialog = new FolderBrowserDialog
            {
                SelectedPath = GetInitialDirectory()
            };

            var result = folderDialog.ShowDialog();
            if(result.ToString() != "OK")
            {
                return;
            }

            Properties.Settings.Default.LoggingPath = folderDialog.SelectedPath;
            Properties.Settings.Default.Save();
        }

        public string GetInitialDirectory()
        {
            return string.IsNullOrWhiteSpace(Properties.Settings.Default.InitialDirectory)
                ? ""
                : Properties.Settings.Default.InitialDirectory;
        }

        public string GetLoggingPath()
        {
            return string.IsNullOrWhiteSpace(Properties.Settings.Default.LoggingPath)
                ? ""
                : Properties.Settings.Default.LoggingPath;
        }
    }
}