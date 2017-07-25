using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;
using EvilBaschdi.Core.Application;
using EvilBaschdi.Core.Browsers;
using EvilBaschdi.Core.DirectoryExtensions;
using EvilBaschdi.Core.Threading;
using EvilBaschdi.Core.Wpf;
using Folders2Hash.Core;
using Folders2Hash.Internal;
using Folders2Hash.Models;
using Folders2Hash.Properties;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Folders2Hash
{
    /// <inheritdoc cref="MetroWindow" />
    public partial class MainWindow
    {
        /// <summary>
        ///     Current hidden instance for use with instances through command line.
        /// </summary>
        public MainWindow CurrentHiddenInstance { private get; set; }

        private readonly IApplicationSettings _applicationSettings;
        private readonly IApplicationBasics _basics;
        private readonly IDialogService _dialogService;
        private readonly IHashAlgorithmDictionary _hashAlgorithmDictionary;
        private readonly IMetroStyle _style;
        private Configuration _configuration;
        private ObservableCollection<LogEntry> _logEntries;
        private ObservableCollection<SelectableObject<HashAlgorithmModel>> _observableCollection;
        private Task<ObservableCollection<LogEntry>> _task;
        private ProgressDialogController _controller;

        //true == directory, false == file
        private readonly ConcurrentDictionary<string, bool> _pathsToScan = new ConcurrentDictionary<string, bool>();

        private string _loggingPath;
        private int _overrideProtection;

        /// <inheritdoc />
        public MainWindow()
        {
            InitializeComponent();
            _hashAlgorithmDictionary = new HashAlgorithmDictionary();
            IFolderBrowser folderBrowser = new ExplorerFolderBrowser();
            _applicationSettings = new ApplicationSettings();
            _basics = new ApplicationBasics(folderBrowser, _applicationSettings);
            _dialogService = new DialogService(this);
            TaskbarItemInfo = new TaskbarItemInfo();
            IThemeManagerHelper themeManagerHelper = new ThemeManagerHelper();
            ISettings coreSettings = new CoreSettings(Settings.Default);
            _style = new MetroStyle(this, Accent, ThemeSwitch, coreSettings, themeManagerHelper);

            _style.Load(true);

            var linkerTime = Assembly.GetExecutingAssembly().GetLinkerTime();
            LinkerTime.Content = linkerTime.ToString(CultureInfo.InvariantCulture);
            Load();
        }

        private void Load()
        {
            GetHashAlgorithms();
            HashAlgorithms.ItemsSource = _observableCollection;
            Generate.IsEnabled = !string.IsNullOrWhiteSpace(_applicationSettings.InitialDirectory) && Directory.Exists(_applicationSettings.InitialDirectory);

            KeepFileExtension.IsChecked = _applicationSettings.KeepFileExtension;
            InitialDirectory.Text = _applicationSettings.InitialDirectory;

            _loggingPath = !string.IsNullOrWhiteSpace(_applicationSettings.LoggingPath)
                ? _applicationSettings.LoggingPath
                : _applicationSettings.InitialDirectory;
            LoggingPath.Text = _loggingPath;

            _overrideProtection = 1;
        }


        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        #region Process Controller

        private async void GenerateHashesOnClick(object sender, RoutedEventArgs e)
        {
            _pathsToScan.TryAdd(_applicationSettings.InitialDirectory, true);
            await ConfigureController();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private async Task ConfigureController()
        {
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;

            var configuration = new Configuration
                                {
                                    PathsToScan = _pathsToScan,
                                    HashTypes = _applicationSettings.CurrentHashAlgorithms.Cast<string>().ToList(),
                                    LoggingPath = _loggingPath,
                                    KeepFileExtension = _applicationSettings.KeepFileExtension
                                };
            Cursor = Cursors.Wait;
            _configuration = configuration;
            var options = new MetroDialogSettings
                          {
                              ColorScheme = MetroDialogColorScheme.Accented
                          };

            MetroDialogOptions = options;
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
            ResultGrid.ItemsSource = _logEntries;
            var message = $"Checksums were generated. {Environment.NewLine}You can find the logging file at '{_loggingPath}'.";
            _dialogService.ShowMessage("Completed", message);
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
            TaskbarItemInfo.ProgressValue = 1;
            Cursor = Cursors.Arrow;
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
            ICalculate calculate = new Calculate();
            IMultiThreadingHelper multiThreadingHelper = new MultiThreadingHelper();
            IFilePath filePath = new FilePath(multiThreadingHelper);
            ILogging logging = new Logging();
            IFileListCalculationProcessor fileListCalculationProcessor = new FileListCalculationProcessor(calculate, filePath, logging);

            var result = fileListCalculationProcessor.ValueFor(_configuration);

            if (_configuration.CloseHiddenInstancesOnFinish)
            {
                CurrentHiddenInstance.Close();
            }
            return result;
        }

        #endregion Calculation

        #region GenerationSettings

        private void FileExtension(object sender, EventArgs e)
        {
            HandleFileExtension(sender as ToggleSwitch);
        }

        private void HandleFileExtension(ToggleSwitch toggleSwitch)
        {
            if (toggleSwitch.IsChecked != null)
            {
                _applicationSettings.KeepFileExtension = toggleSwitch.IsChecked.Value;
            }
        }

        private void BrowseLoggingPathClick(object sender, RoutedEventArgs e)
        {
            _basics.BrowseLoggingFolder();
            LoggingPath.Text = _applicationSettings.LoggingPath;
            _loggingPath = _applicationSettings.LoggingPath;
            Load();
        }

        private void LoggingPathOnLostFocus(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(LoggingPath.Text))
            {
                _applicationSettings.LoggingPath = LoggingPath.Text;
                _loggingPath = _applicationSettings.LoggingPath;
            }
            Load();
        }

        #endregion GenerationSettings

        #region Drag and Drop

        private async void GridOnDrop(object sender, DragEventArgs e)
        {
            if (null != e.Data && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var droppedElements = (string[]) e.Data.GetData(DataFormats.FileDrop, true);
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
                var droppedElements = (string[]) e.Data.GetData(DataFormats.FileDrop, true);
                if (droppedElements != null)
                {
                    foreach (var droppedElement in droppedElements)
                    {
                        var fileAttributes = File.GetAttributes(droppedElement);
                        var isDirectory = (fileAttributes & FileAttributes.Directory) == FileAttributes.Directory;
                        if (isDirectory)
                        {
                            if (!Directory.Exists(droppedElement))
                            {
                                isCorrect = false;
                                break;
                            }
                        }
                        else
                        {
                            if (!File.Exists(droppedElement))
                            {
                                isCorrect = false;
                                break;
                            }
                        }
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
            var comboBox = (ComboBox) sender;
            comboBox.SelectedItem = null;
        }

        private void HashAlgorithmsOnCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            var currentHashAlgorithms = new StringCollection();

            foreach (SelectableObject<HashAlgorithmModel> hashType in HashAlgorithms.Items)
            {
                if (hashType.IsSelected)
                {
                    currentHashAlgorithms.Add(hashType.ObjectData.Extension);
                }
            }

            _applicationSettings.CurrentHashAlgorithms = currentHashAlgorithms;
            SetHashAlgorithmsWatermark();
        }

        private void GetHashAlgorithms()
        {
            _observableCollection = new ObservableCollection<SelectableObject<HashAlgorithmModel>>();

            var hashAlgorithmDictionary = _hashAlgorithmDictionary.Value;

            foreach (var item in hashAlgorithmDictionary)
            {
                var hashAlgorithmModel = new HashAlgorithmModel
                                         {
                                             Extension = item.Value,
                                             DisplayName = item.Key,
                                             IsSelected = _applicationSettings.CurrentHashAlgorithms.Contains(item.Value)
                                         };
                var selectableObject = new SelectableObject<HashAlgorithmModel>(hashAlgorithmModel)
                                       {
                                           IsSelected = hashAlgorithmModel.IsSelected
                                       };
                _observableCollection.Add(selectableObject);
            }
            SetHashAlgorithmsWatermark();
        }

        private void SetHashAlgorithmsWatermark()
        {
            var hashAlgorithmDictionary = _hashAlgorithmDictionary.Value;
            var currentHashAlgorithms = _applicationSettings.CurrentHashAlgorithms.Cast<string>().ToList();
            TextBoxHelper.SetWatermark(HashAlgorithms,
                string.Join(", ", (from item in hashAlgorithmDictionary where currentHashAlgorithms.Contains(item.Value) select item.Key).ToList()));
        }

        #endregion HashAlgorithms

        #region Initial Directory

        private void BrowseClick(object sender, RoutedEventArgs e)
        {
            _basics.BrowseFolder();
            InitialDirectory.Text = _applicationSettings.InitialDirectory;
            Load();
        }

        private void InitialDirectoryOnLostFocus(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(InitialDirectory.Text))
            {
                _applicationSettings.InitialDirectory = InitialDirectory.Text;
            }
            Load();
        }

        #endregion Initial Directory

        #region Fly-out

        private void ToggleSettingsFlyoutClick(object sender, RoutedEventArgs e)
        {
            ToggleFlyout(0);
        }

        private void ToggleFlyout(int index, bool stayOpen = false)
        {
            var activeFlyout = (Flyout) Flyouts.Items[index];
            if (activeFlyout == null)
            {
                return;
            }

            foreach (
                var nonactiveFlyout in
                Flyouts.Items.Cast<Flyout>()
                        .Where(nonactiveFlyout => nonactiveFlyout.IsOpen && nonactiveFlyout.Name != activeFlyout.Name))
            {
                nonactiveFlyout.IsOpen = false;
            }

            activeFlyout.IsOpen = activeFlyout.IsOpen && stayOpen || !activeFlyout.IsOpen;
        }

        #endregion Fly-out

        #region MetroStyle

        private void SaveStyleClick(object sender, RoutedEventArgs e)
        {
            if (_overrideProtection == 0)
            {
                return;
            }
            _style.SaveStyle();
        }

        private void Theme(object sender, EventArgs e)
        {
            if (_overrideProtection == 0)
            {
                return;
            }
            _style.SetTheme(sender);
        }

        private void AccentOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_overrideProtection == 0)
            {
                return;
            }
            _style.SetAccent(sender, e);
        }

        #endregion MetroStyle
    }
}