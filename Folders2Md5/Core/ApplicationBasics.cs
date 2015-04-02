using System.Windows.Forms;

namespace Folders2Md5.Core
{
    public class ApplicationBasics
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

            Properties.Settings.Default.InitialDirectory = GetInitialDirectory();
            Properties.Settings.Default.Save();
        }

        public string GetInitialDirectory()
        {
            return string.IsNullOrWhiteSpace(Properties.Settings.Default.InitialDirectory)
                ? ""
                : Properties.Settings.Default.InitialDirectory;
        }
    }
}