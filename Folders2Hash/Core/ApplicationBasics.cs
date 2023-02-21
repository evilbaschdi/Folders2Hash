using EvilBaschdi.Core.Wpf.Browsers;
using Folders2Hash.Models;
using Folders2Hash.Settings;
using JetBrains.Annotations;

namespace Folders2Hash.Core;

/// <inheritdoc />
public class ApplicationBasics : IApplicationBasics
{
    private readonly IFolderBrowser _folderBrowser;
    private readonly IWritableConfiguration _writableConfiguration;
    private Configuration _configuration = new();

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="folderBrowser"></param>
    /// <param name="writableConfiguration"></param>
    public ApplicationBasics(IFolderBrowser folderBrowser, [NotNull] IWritableConfiguration writableConfiguration)
    {
        _folderBrowser = folderBrowser ?? throw new ArgumentNullException(nameof(folderBrowser));
        _writableConfiguration = writableConfiguration ?? throw new ArgumentNullException(nameof(writableConfiguration));
    }

    /// <inheritdoc />
    public void BrowseFolder()
    {
        _configuration = _writableConfiguration.Value;
        _folderBrowser.SelectedPath = _configuration.PathsToScan.First();
        _folderBrowser.ShowDialog();
        var dic = new List<string>
                  {
                      _folderBrowser.SelectedPath
                  };
        _configuration.PathsToScan = dic;
        _writableConfiguration.Value = _configuration;
    }

    /// <inheritdoc />
    public void BrowseLoggingFolder()
    {
        _configuration = _writableConfiguration.Value;
        _folderBrowser.SelectedPath = _configuration.LoggingPath;
        _folderBrowser.ShowDialog();
        _configuration.LoggingPath = _folderBrowser.SelectedPath;
        _writableConfiguration.Value = _configuration;
    }
}