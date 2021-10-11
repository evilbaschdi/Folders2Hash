using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;
using EvilBaschdi.Core.Internal;
using EvilBaschdi.Core.Logging;
using EvilBaschdi.CoreExtended;
using EvilBaschdi.CoreExtended.Browsers;
using EvilBaschdi.CoreExtended.Controls.About;
using Folders2Hash.Core;
using Folders2Hash.Internal;
using Folders2Hash.Models;
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

        private readonly IApplicationBasics _basics;
        private readonly IHashAlgorithmDictionary _hashAlgorithmDictionary;

        private Configuration _configuration;
        private ObservableCollection<LogEntry> _logEntries;
        private ObservableCollection<SelectableObject<HashAlgorithmModel>> _observableCollection;
        private Task<ObservableCollection<LogEntry>> _task;
        private ProgressDialogController _controller;

        //true == directory, false == file
        private readonly ConcurrentDictionary<string, bool> _pathsToScan = new();

        private string _loggingPath;
        private readonly IWritableConfiguration _writableConfiguration;
        private readonly IRoundCorners _roundCorners;

        /// <inheritdoc />
        public MainWindow()
        {
            InitializeComponent();
            _hashAlgorithmDictionary = new HashAlgorithmDictionary();
            IFolderBrowser folderBrowser = new ExplorerFolderBrowser();
            IConfigurationPath configurationPath = new ConfigurationPath();
            _writableConfiguration = new WritableConfiguration(configurationPath);
            _basics = new ApplicationBasics(folderBrowser, _writableConfiguration);
            _configuration = _writableConfiguration.Value;
            TaskbarItemInfo = new();

            _roundCorners = new RoundCorners();
            IApplicationStyle style = new ApplicationStyle(_roundCorners, true);
            style.Run();
            Load();
        }

        private void Load()
        {
            GetHashAlgorithms();
            HashAlgorithms.ItemsSource = _observableCollection;
            Generate.IsEnabled = !string.IsNullOrWhiteSpace(_configuration.PathsToScan.FirstOrDefault().Key) && Directory.Exists(_configuration.PathsToScan.FirstOrDefault().Key);

            KeepFileExtension.IsOn = _configuration.KeepFileExtension;
            InitialDirectory.Text = _configuration.PathsToScan.FirstOrDefault().Key ?? string.Empty;

            _loggingPath = !string.IsNullOrWhiteSpace(_configuration.LoggingPath)
                ? _configuration.LoggingPath
                : _configuration.PathsToScan.FirstOrDefault().Key;
            LoggingPath.Text = _loggingPath ?? string.Empty;
        }

        private void AboutWindowClick(object sender, RoutedEventArgs e)
        {
            var assembly = typeof(MainWindow).Assembly;
            IAboutContent aboutWindowContent = new AboutContent(assembly, $@"{AppDomain.CurrentDomain.BaseDirectory}\hash512.png");

            var aboutWindow = new AboutWindow
                              {
                                  DataContext = new AboutViewModel(aboutWindowContent, _roundCorners)
                              };

            aboutWindow.ShowDialog();
        }

        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

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
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
            Cursor = Cursors.Wait;

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
            var message = $"Check Sums were generated. {Environment.NewLine}You can find the logging file at '{_loggingPath}'.";
            this.ShowMessageAsync("Completed", message);
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
            LoggingPath.Text = _configuration.LoggingPath;
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
            if (null != e.Data && e.Data.GetDataPresent(DataFormats.FileDrop))
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
            InitialDirectory.Text = _configuration.PathsToScan.FirstOrDefault().Key;
            Load();
        }

        private void InitialDirectoryOnLostFocus(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(InitialDirectory.Text))
            {
                var dic = new ConcurrentDictionary<string, bool>();
                dic.TryAdd(InitialDirectory.Text, true);
                _configuration.PathsToScan = dic;
                _writableConfiguration.Value = _configuration;
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
            var activeFlyout = (Flyout)Flyouts.Items[index];
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
    }
}