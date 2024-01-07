using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using EvilBaschdi.Core.Wpf;
using Folders2Hash.Internal;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Folders2Hash;

/// <inheritdoc cref="MetroWindow" />
/// <summary>
///     Interaction logic for HashEvaluationDialog.xaml
/// </summary>
// ReSharper disable once RedundantExtendsListEntry
public partial class HashEvaluationDialog : MetroWindow
{
    private readonly ICalculate _calculate;
    private ProgressDialogController _controller;
    private string _hashFileContent;
    private string _sourceFileHash;
    private string _sourceFileName;
    private Task<bool> _task;
    private bool _windowShown;

    /// <inheritdoc />
    public HashEvaluationDialog()
    {
        IHashAlgorithmByName hashAlgorithmByName = new HashAlgorithmByName();
        _calculate = new Calculate(hashAlgorithmByName);
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        InitializeComponent();

        IApplicationStyle applicationStyle = new ApplicationStyle();
        IApplicationLayout applicationLayout = new ApplicationLayout();
        applicationStyle.Run();
        applicationLayout.RunFor((true, false));
    }

    /// <summary>
    /// </summary>
    public string HashFile { private get; init; }

    /// <summary>
    /// </summary>
    public string HashType { private get; init; }

    /// <inheritdoc />
    /// <summary>
    ///     Executing code when window is shown.
    /// </summary>
    /// <param name="e"></param>
    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        if (_windowShown)
        {
            return;
        }

        _windowShown = true;

        await ConfigureController();
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    private async Task ConfigureController()
    {
        TaskbarItemInfo.SetCurrentValue(TaskbarItemInfo.ProgressStateProperty, TaskbarItemProgressState.Indeterminate);

        SetCurrentValue(CursorProperty, Cursors.Wait);

        var options = new MetroDialogSettings
                      {
                          ColorScheme = MetroDialogColorScheme.Accented
                      };

        SetCurrentValue(MetroDialogOptionsProperty, options);
        _controller = await this.ShowProgressAsync("Loading...", "evaluation is running", true, options);
        _controller.SetIndeterminate();
        _controller.Canceled += ControllerCanceled;

        _task = Task<bool>.Factory.StartNew(IsHashValid);
        await _task;
        _task.GetAwaiter().OnCompleted(TaskCompleted);
    }

    private bool IsHashValid()
    {
        if (HashFile.StartsWith("checksums"))
        {
            return false;
        }

        _sourceFileName = HashFile[..^(HashType.Length + 1)];
        _hashFileContent = File.ReadAllLines(HashFile).FirstOrDefault(l => !l.StartsWith("#"))?.Split(" *").FirstOrDefault();

        if (!File.Exists(_sourceFileName))
        {
            return false;
        }

        var dic = new Dictionary<string, string>
                  {
                      { HashType, HashFile }
                  };
        var sourceFileHashes = _calculate.Hashes(_sourceFileName, dic);
        _sourceFileHash = sourceFileHashes.First(x => x.Key.Equals(HashType, StringComparison.CurrentCultureIgnoreCase)).Value;
        return _sourceFileHash.Trim().Equals(_hashFileContent?.Trim(), StringComparison.InvariantCultureIgnoreCase);
    }

    private void TaskCompleted()
    {
        HashFileName.SetCurrentValue(System.Windows.Controls.HeaderedContentControl.HeaderProperty, $"Hash File: '{HashFile}'");
        SourceFileName.SetCurrentValue(System.Windows.Controls.HeaderedContentControl.HeaderProperty, $"Original Source File: '{_sourceFileName}'");
        HashFileContent.SetCurrentValue(System.Windows.Controls.TextBox.TextProperty, _hashFileContent);
        SourceFileHash.SetCurrentValue(System.Windows.Controls.TextBox.TextProperty, _sourceFileHash);

        if (_task.Result)
        {
            HashFileContent.SetCurrentValue(BackgroundProperty, Brushes.GreenYellow);
            HashFileContent.SetCurrentValue(ForegroundProperty, Brushes.Black);
            SourceFileHash.SetCurrentValue(BackgroundProperty, Brushes.GreenYellow);
            SourceFileHash.SetCurrentValue(ForegroundProperty, Brushes.Black);
        }
        else
        {
            HashFileContent.SetCurrentValue(BackgroundProperty, Brushes.DarkRed);
            HashFileContent.SetCurrentValue(ForegroundProperty, Brushes.White);
            SourceFileHash.SetCurrentValue(BackgroundProperty, Brushes.DarkRed);
            SourceFileHash.SetCurrentValue(ForegroundProperty, Brushes.White);
        }

        _controller.CloseAsync();
        _controller.Closed += ControllerClosed;
    }

    private void ControllerClosed(object sender, EventArgs e)
    {
        TaskbarItemInfo.SetCurrentValue(TaskbarItemInfo.ProgressStateProperty, TaskbarItemProgressState.Normal);
        TaskbarItemInfo.SetCurrentValue(TaskbarItemInfo.ProgressValueProperty, (double)1);
        SetCurrentValue(CursorProperty, Cursors.Arrow);
    }

    private void ControllerCanceled(object sender, EventArgs e)
    {
        _controller.CloseAsync();
        _controller.Closed += ControllerClosed;
    }
}