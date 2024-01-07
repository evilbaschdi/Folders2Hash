using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;
using EvilBaschdi.About.Core;
using EvilBaschdi.About.Core.Models;
using EvilBaschdi.About.Wpf;
using EvilBaschdi.Core;
using EvilBaschdi.Core.Internal;
using EvilBaschdi.Core.Logging;
using EvilBaschdi.Core.Wpf;
using EvilBaschdi.Core.Wpf.Browsers;
using EvilBaschdi.Core.Wpf.FlyOut;
using EvilBaschdi.Settings.ByMachineAndUser;
using Folders2Hash.Core;
using Folders2Hash.Internal;
using Folders2Hash.Models;
using Folders2Hash.Settings;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Folders2Hash;

/// <inheritdoc cref="MetroWindow" />
public partial class MainWindow
{
    /// <summary>
    ///     Current hidden instance for use with instances through command line.
    /// </summary>
    public MainWindow CurrentHiddenInstance { private get; set; }

    private readonly IApplicationBasics _basics;
    private readonly IHashAlgorithmDictionary _hashAlgorithmDictionary;
    private readonly IToggleFlyOut _toggleFlyOut;

    private Configuration _configuration;
    private ObservableCollection<LogEntry> _logEntries;
    private ObservableCollection<SelectableObject<HashAlgorithmModel>> _observableCollection;
    private Task<ObservableCollection<LogEntry>> _task;
    private ProgressDialogController _controller;

    //true == directory, false == file
    private readonly ConcurrentDictionary<string, bool> _pathsToScan = new();

    private string _loggingPath;
    private readonly IWritableConfiguration _writableConfiguration;
    private readonly ICurrentFlyOuts _currentFlyOuts;

    /// <inheritdoc />
    public MainWindow()
    {
        InitializeComponent();
        _hashAlgorithmDictionary = new HashAlgorithmDictionary();
        IFolderBrowser folderBrowser = new ExplorerFolderBrowser();

        IAppSettingsFromJsonFile appSettingsFromJsonFile = new AppSettingsFromJsonFile();
        IAppSettingsFromJsonFileByMachineAndUser appSettingsFromJsonFileByMachineAndUser = new AppSettingsFromJsonFileByMachineAndUser();
        IAppSettingByKey appSettingByKey = new AppSettingByKey(appSettingsFromJsonFile, appSettingsFromJsonFileByMachineAndUser);

        _writableConfiguration = new WritableConfiguration(appSettingByKey);
        _basics = new ApplicationBasics(folderBrowser, _writableConfiguration);
        _configuration = _writableConfiguration.Value;
        TaskbarItemInfo = new();

        IApplicationStyle applicationStyle = new ApplicationStyle();
        IApplicationLayout applicationLayout = new ApplicationLayout();
        applicationStyle.Run();
        applicationLayout.RunFor((true, false));
        _currentFlyOuts = new CurrentFlyOuts();
        _toggleFlyOut = new ToggleFlyOut();
        Load();
    }

    private void Load()
    {
        GetHashAlgorithms();
        HashAlgorithms.SetCurrentValue(ItemsControl.ItemsSourceProperty, _observableCollection);
        Generate.SetCurrentValue(IsEnabledProperty,
            !string.IsNullOrWhiteSpace(_configuration.PathsToScan.FirstOrDefault()) && Directory.Exists(_configuration.PathsToScan.FirstOrDefault()));

        KeepFileExtension.SetCurrentValue(ToggleSwitch.IsOnProperty, _configuration.KeepFileExtension);
        InitialDirectory.SetCurrentValue(TextBox.TextProperty, _configuration.PathsToScan.FirstOrDefault() ?? string.Empty);

        _loggingPath = !string.IsNullOrWhiteSpace(_configuration.LoggingPath)
            ? _configuration.LoggingPath
            : _configuration.PathsToScan.FirstOrDefault();
        LoggingPath.SetCurrentValue(TextBox.TextProperty, _loggingPath ?? string.Empty);
    }

    private void AboutWindowClick(object sender, RoutedEventArgs e)
    {
        ICurrentAssembly currentAssembly = new CurrentAssembly();
        IAboutContent aboutContent = new AboutContent(currentAssembly);
        IAboutViewModel aboutModel = new AboutViewModel(aboutContent);
        IApplyMicaBrush applyMicaBrush = new ApplyMicaBrush();
        IApplicationLayout applicationLayout = new ApplicationLayout();
        var aboutWindow = new AboutWindow(aboutModel, applicationLayout, applyMicaBrush);

        aboutWindow.ShowDialog();
    }

    // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

    #region Fly-out

    private void ToggleSettingsFlyoutClick(object sender, RoutedEventArgs e)
    {
        var currentFlyOutsModel = _currentFlyOuts.ValueFor(Flyouts, 0);
        _toggleFlyOut.RunFor(currentFlyOutsModel);
    }

    #endregion Fly-out

    #region Process Controller

    private async void GenerateHashesOnClick(object sender, RoutedEventArgs e)
    {
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
        _controller = await this.ShowProgressAsync("Please wait...", "Hashes are getting generated.", true, options);
        _controller.SetIndeterminate();
        _controller.Canceled += ControllerCanceled;
        _task = Task<ObservableCollection<LogEntry>>.Factory.StartNew(RunHashCalculation);
        await _task;
        _task.GetAwaiter().OnCompleted(TaskCompleted);
        _logEntries = _task.Result;
    }

    private void TaskCompleted()
    {
        _controller.CloseAsync();
        _controller.Closed += ControllerClosed;
    }

    private void ControllerClosed(object sender, EventArgs e)
    {
        ResultGrid.SetCurrentValue(ItemsControl.ItemsSourceProperty, _logEntries);
        var message = $"Check Sums were generated. {Environment.NewLine}You can find the logging file at '{_loggingPath}'.";
        this.ShowMessageAsync("Completed", message);
        TaskbarItemInfo.SetCurrentValue(TaskbarItemInfo.ProgressStateProperty, TaskbarItemProgressState.Normal);
        TaskbarItemInfo.SetCurrentValue(TaskbarItemInfo.ProgressValueProperty, (double)1);
        SetCurrentValue(CursorProperty, Cursors.Arrow);
        _pathsToScan.Clear();
    }

    private void ControllerCanceled(object sender, EventArgs e)
    {
        _task.Dispose();
    }

    #endregion Process Controller

    #region Calculation

    /// <summary>
    /// </summary>
    /// <param name="configuration"></param>
    public void RunPreconfiguredHashCalculation(Configuration configuration)
    {
        _configuration = configuration;
        RunHashCalculation();
    }

    private ObservableCollection<LogEntry> RunHashCalculation()
    {
        IHashAlgorithmByName hashAlgorithmByName = new HashAlgorithmByName();
        ICalculate calculate = new Calculate(hashAlgorithmByName);
        IFileListFromPath filePath = new FileListFromPath();
        IAppendAllTextWithHeadline appendAllTextWithHeadline = new AppendAllTextWithHeadline();
        ILogging logging = new Logging(appendAllTextWithHeadline);
        IFileListCalculationProcessor fileListCalculationProcessor = new FileListCalculationProcessor(calculate, filePath, logging);

        var result = fileListCalculationProcessor.ValueFor(_configuration);

        if (_configuration.CloseHiddenInstancesOnFinish)
        {
            CurrentHiddenInstance?.Close();
        }

        return result;
    }

    #endregion Calculation

    #region GenerationSettings

    private void FileExtension(object sender, RoutedEventArgs e)
    {
        var keepFileExtension = false;
        if (sender is ToggleSwitch toggleSwitch)
        {
            keepFileExtension = toggleSwitch.IsOn;
        }

        _configuration.KeepFileExtension = keepFileExtension;
        _writableConfiguration.Value = _configuration;
    }

    private void BrowseLoggingPathClick(object sender, RoutedEventArgs e)
    {
        _basics.BrowseLoggingFolder();
        LoggingPath.SetCurrentValue(TextBox.TextProperty, _configuration.LoggingPath);
        _loggingPath = _configuration.LoggingPath;
        Load();
    }

    private void LoggingPathOnLostFocus(object sender, RoutedEventArgs e)
    {
        if (Directory.Exists(LoggingPath.Text))
        {
            _configuration.LoggingPath = LoggingPath.Text;
            _loggingPath = _configuration.LoggingPath;
            _writableConfiguration.Value = _configuration;
        }

        Load();
    }

    #endregion GenerationSettings

    #region Drag and Drop

    private async void GridOnDrop(object sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            var droppedElements = (string[])e.Data.GetData(DataFormats.FileDrop, true);
            if (droppedElements != null)
            {
                foreach (var droppedElement in droppedElements)
                {
                    try
                    {
                        var fileAttributes = File.GetAttributes(droppedElement);
                        var isDirectory = (fileAttributes & FileAttributes.Directory) == FileAttributes.Directory;
                        if (!_pathsToScan.ContainsKey(droppedElement))
                        {
                            _pathsToScan.TryAdd(droppedElement, isDirectory);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null)
                        {
                            MessageBox.Show($"{ex.InnerException.Message} - {ex.InnerException.StackTrace}");
                        }

                        MessageBox.Show($"{ex.Message} - {ex.StackTrace}");
                        // ReSharper disable once ThrowingSystemException
                        throw;
                    }
                }

                await ConfigureController();
            }
        }

        e.Handled = true;
    }

    private void GridOnDragOver(object sender, DragEventArgs e)
    {
        var isCorrect = true;

        if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
        {
            var droppedElements = (string[])e.Data.GetData(DataFormats.FileDrop, true);
            if (droppedElements != null)
            {
                foreach (var droppedElement in droppedElements)
                {
                    var fileAttributes = File.GetAttributes(droppedElement);
                    var isDirectory = (fileAttributes & FileAttributes.Directory) == FileAttributes.Directory;
                    if (isDirectory)
                    {
                        if (Directory.Exists(droppedElement))
                        {
                            continue;
                        }

                        isCorrect = false;
                        break;
                    }

                    if (File.Exists(droppedElement))
                    {
                        continue;
                    }

                    isCorrect = false;
                    break;
                }
            }
        }

        e.Effects = isCorrect ? DragDropEffects.All : DragDropEffects.None;
        e.Handled = true;
    }

    #endregion Drag and Drop

    #region HashAlgorithms

    private void HashAlgorithmsOnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var comboBox = (ComboBox)sender;
        comboBox.SelectedItem = null;
    }

    private void HashAlgorithmsOnCheckBoxChecked(object sender, RoutedEventArgs e)
    {
        var currentHashAlgorithms = (from SelectableObject<HashAlgorithmModel> hashType in HashAlgorithms.Items where hashType.IsSelected select hashType.ObjectData.Extension)
            .ToList();

        _configuration.HashTypes = currentHashAlgorithms;
        _writableConfiguration.Value = _configuration;
        SetHashAlgorithmsWatermark();
    }

    private void GetHashAlgorithms()
    {
        _observableCollection = new();

        var hashAlgorithmDictionary = _hashAlgorithmDictionary.Value;

        foreach (var selectableObject in hashAlgorithmDictionary
                                         .Select(item
                                                     => new HashAlgorithmModel(item.Value, item.Key, _configuration.HashTypes.Contains(item.Value))
                                         ).Select(hashAlgorithmModel
                                                      => new SelectableObject<HashAlgorithmModel>(hashAlgorithmModel)
                                                         {
                                                             IsSelected = hashAlgorithmModel.IsSelected
                                                         }))
        {
            _observableCollection.Add(selectableObject);
        }

        SetHashAlgorithmsWatermark();
    }

    private void SetHashAlgorithmsWatermark()
    {
        var hashAlgorithmDictionary = _hashAlgorithmDictionary.Value;
        var currentHashAlgorithms = _configuration.HashTypes.ToList();
        TextBoxHelper.SetWatermark(HashAlgorithms,
            string.Join(", ", (from item in hashAlgorithmDictionary where currentHashAlgorithms.Contains(item.Value) select item.Key).ToList()));
    }

    #endregion HashAlgorithms

    #region Initial Directory

    private void BrowseClick(object sender, RoutedEventArgs e)
    {
        _basics.BrowseFolder();
        InitialDirectory.SetCurrentValue(TextBox.TextProperty, _configuration.PathsToScan.FirstOrDefault());
        Load();
    }

    private void InitialDirectoryOnLostFocus(object sender, RoutedEventArgs e)
    {
        if (Directory.Exists(InitialDirectory.Text))
        {
            var dic = new List<string>
                      {
                          InitialDirectory.Text
                      };
            _configuration.PathsToScan = dic;
            _writableConfiguration.Value = _configuration;
        }

        Load();
    }

    #endregion Initial Directory
}