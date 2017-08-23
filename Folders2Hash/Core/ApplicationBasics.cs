using System;
using EvilBaschdi.Core.Browsers;

namespace Folders2Hash.Core
{
    /// <inheritdoc />
    public class ApplicationBasics : IApplicationBasics
    {
        private readonly IApplicationSettings _applicationSettings;
        private readonly IFolderBrowser _folderBrowser;

        /// <summary>
        ///     Initialisiert eine neue Instanz der <see cref="T:System.Object" />-Klasse.
        /// </summary>
        public ApplicationBasics(IFolderBrowser folderBrowser, IApplicationSettings applicationSettings)
        {
            _folderBrowser = folderBrowser ?? throw new ArgumentNullException(nameof(folderBrowser));
            _applicationSettings = applicationSettings ?? throw new ArgumentNullException(nameof(applicationSettings));
        }

        /// <inheritdoc />
        public void BrowseFolder()
        {
            _folderBrowser.SelectedPath = _applicationSettings.InitialDirectory;
            _folderBrowser.ShowDialog();
            _applicationSettings.InitialDirectory = _folderBrowser.SelectedPath;
        }

        /// <inheritdoc />
        public void BrowseLoggingFolder()
        {
            _folderBrowser.SelectedPath = _applicationSettings.LoggingPath;
            _folderBrowser.ShowDialog();
            _applicationSettings.LoggingPath = _folderBrowser.SelectedPath;
        }
    }
}