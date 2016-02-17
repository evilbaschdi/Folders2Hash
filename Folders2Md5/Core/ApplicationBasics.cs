using System;
using EvilBaschdi.Core.Browsers;

namespace Folders2Md5.Core
{
    public class ApplicationBasics : IApplicationBasics
    {
        private readonly IFolderBrowser _folderBrowser;
        private readonly IApplicationSettings _applicationSettings;

        /// <summary>
        ///     Initialisiert eine neue Instanz der <see cref="T:System.Object" />-Klasse.
        /// </summary>
        public ApplicationBasics(IFolderBrowser folderBrowser, IApplicationSettings applicationSettings)
        {
            if (folderBrowser == null)
            {
                throw new ArgumentNullException(nameof(folderBrowser));
            }
            if (applicationSettings == null)
            {
                throw new ArgumentNullException(nameof(applicationSettings));
            }
            _folderBrowser = folderBrowser;
            _applicationSettings = applicationSettings;
        }

        public void BrowseFolder()
        {
            _folderBrowser.SelectedPath = _applicationSettings.InitialDirectory;
            _folderBrowser.ShowDialog();
            _applicationSettings.InitialDirectory = _folderBrowser.SelectedPath;
        }

        public void BrowseLoggingFolder()
        {
            _folderBrowser.SelectedPath = _applicationSettings.LoggingPath;
            _folderBrowser.ShowDialog();
            _applicationSettings.LoggingPath = _folderBrowser.SelectedPath;
        }
    }
}