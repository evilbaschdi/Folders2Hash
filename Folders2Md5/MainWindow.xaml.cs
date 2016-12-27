using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;
using EvilBaschdi.Core.Application;
using EvilBaschdi.Core.Browsers;
using EvilBaschdi.Core.DirectoryExtensions;
using EvilBaschdi.Core.DotNetExtensions;
using EvilBaschdi.Core.Logging;
using EvilBaschdi.Core.Threading;
using EvilBaschdi.Core.Wpf;
using Folders2Md5.Core;
using Folders2Md5.Internal;
using Folders2Md5.Models;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Folders2Md5
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        /// <summary>
        ///     Set tru to close hidden instances on finish.
        /// </summary>
        public bool CloseHiddenInstancesOnFinish { get; set; }

        /// <summary>
        ///     Current hidden instance for use with instances through command line.
        /// </summary>
        public MainWindow CurrentHiddenInstance { get; set; }

        private readonly IMetroStyle _style;
        private readonly IFolderBrowser _folderBrowser;
        private readonly IApplicationSettings _applicationSettings;
        private readonly IApplicationBasics _basics;
        private readonly ICalculate _calculate;
        private readonly IDialogService _dialogService;
        private readonly ISettings _coreSettings;
        private readonly IThemeManagerHelper _themeManagerHelper;
        private Configuration _configuration;
        private ObservableCollection<Folders2Md5LogEntry> _folders2Md5LogEntries;
        private Task<ObservableCollection<Folders2Md5LogEntry>> _task;
        private ProgressDialogController _controller;
        private readonly ConcurrentDictionary<string, bool> _pathsToScan = new ConcurrentDictionary<string, bool>();
        private string _loggingPath;
        private int _overrideProtection;

        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable
        /// <summary>
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            _folderBrowser = new ExplorerFolderBrowser();
            _applicationSettings = new ApplicationSettings();
            _basics = new ApplicationBasics(_folderBrowser, _applicationSettings);

            _dialogService = new DialogService(this);
            TaskbarItemInfo = new TaskbarItemInfo();
            _themeManagerHelper = new ThemeManagerHelper();
            _coreSettings = new CoreSettings(Properties.Settings.Default);
            _style = new MetroStyle(this, Accent, ThemeSwitch, _coreSettings, _themeManagerHelper);
            _style.Load(true);
            _calculate = new Calculate();
            var linkerTime = Assembly.GetExecutingAssembly().GetLinkerTime();
            LinkerTime.Content = linkerTime.ToString(CultureInfo.InvariantCulture);
            Load();
        }

        private void Load()
        {
            Generate.IsEnabled = !string.IsNullOrWhiteSpace(_applicationSettings.InitialDirectory) &&
                                 Directory.Exists(_applicationSettings.InitialDirectory);

            KeepFileExtension.IsChecked = _applicationSettings.KeepFileExtension;
            InitialDirectory.Text = _applicationSettings.InitialDirectory;

            _loggingPath = !string.IsNullOrWhiteSpace(_applicationSettings.LoggingPath)
                ? _applicationSettings.LoggingPath
                : _applicationSettings.InitialDirectory;
            LoggingPath.Text = _loggingPath;

            _overrideProtection = 1;
        }

        #region Process Controller

        private async void GenerateHashsOnClick(object sender, RoutedEventArgs e)
        {
            _pathsToScan.TryAdd(_applicationSettings.InitialDirectory, true);
            await ConfigureController();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public async Task ConfigureController()
        {
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;

            var configuration = new Configuration
                                {
                                    PathsToScan = _pathsToScan,
                                    LoggingPath = _loggingPath,
                                    HashType = "md5",
                                    KeepFileExtension = _applicationSettings.KeepFileExtension
                                };
            Cursor = Cursors.Wait;
            _configuration = configuration;
            var options = new MetroDialogSettings
                          {
                              ColorScheme = MetroDialogColorScheme.Accented
                          };

            MetroDialogOptions = options;
            _controller = await this.ShowProgressAsync("Please wait...", "Hashs are getting generated.", true, options);
            _controller.SetIndeterminate();
            _controller.Canceled += ControllerCanceled;
            _task = Task<ObservableCollection<Folders2Md5LogEntry>>.Factory.StartNew(RunHashCalculation);
            await _task;
            _task.GetAwaiter().OnCompleted(TaskCompleted);
            _folders2Md5LogEntries = _task.Result;
        }

        private void TaskCompleted()
        {
            _controller.CloseAsync();
            _controller.Closed += ControllerClosed;
        }

        private void ControllerClosed(object sender, EventArgs e)
        {
            ResultGrid.ItemsSource = _folders2Md5LogEntries;
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

        /// <summary>
        /// </summary>
        /// <param name="configuration"></param>
        public void RunPreconfiguredHashCalculation(Configuration configuration)
        {
            _configuration = configuration;
            RunHashCalculation();
        }

        private ObservableCollection<Folders2Md5LogEntry> RunHashCalculation()
        {
            var multiThreadingHelper = new MultiThreadingHelper();
            var filePath = new FilePath(multiThreadingHelper);
            var appendAllTextWithHeadline = new AppendAllTextWithHeadline();
            var folders2Md5LogEntries = new ConcurrentBag<Folders2Md5LogEntry>();
            var result = new ObservableCollection<Folders2Md5LogEntry>();
            var configuration = _configuration;
            var calculateAllHashTypes = false;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var hashTypes = calculateAllHashTypes
                ? new List<string>
                  {
                      "md5",
                      "sha1",
                      "sha256",
                      "sha384",
                      "sha512"
                  }
                : new List<string>
                  {
                      configuration.HashType.ToLower()
                  };

            var stringBuilder = new StringBuilder();

            var includeExtensionList = new List<string>();
            var excludeExtensionList = new List<string>
                                       {
                                           "ini",
                                           "db"
                                       };
            excludeExtensionList.AddRange(hashTypes);

            var includeFileNameList = new List<string>();
            var excludeFileNameList = new List<string>
                                      {
                                          "folders2md5_log_"
                                      };

            var fileList = new ConcurrentBag<string>();

            Parallel.ForEach(_pathsToScan,
                item =>
                {
                    if (item.Value)
                    {
                        fileList.AddRange(filePath.GetFileList(item.Key, includeExtensionList, excludeExtensionList, includeFileNameList, excludeFileNameList).Distinct());
                    }
                    else
                    {
                        fileList.Add(item.Key);
                    }
                }
            );

            Parallel.ForEach(hashTypes,
                type =>
                {
                    Parallel.ForEach(fileList.Distinct(),
                        file =>
                        {
                            var hashFileName = _calculate.HashFileName(file, type, configuration.KeepFileExtension);

                            var fileInfo = new FileInfo(file);
                            var folders2Md5LogEntry = new Folders2Md5LogEntry
                                                      {
                                                          FileName = file,
                                                          ShortFileName = fileInfo.Name,
                                                          HashFileName = hashFileName,
                                                          Type = type.ToUpper(),
                                                          TimeStamp = DateTime.Now
                                                      };

                            if (!File.Exists(hashFileName) || File.GetLastWriteTime(file) > File.GetLastWriteTime(hashFileName))
                            {
                                var hashSum = _calculate.Hash(file, type);

                                if (File.Exists(hashFileName))
                                {
                                    File.Delete(hashFileName);
                                }

                                folders2Md5LogEntry.HashSum = hashSum;
                                folders2Md5LogEntry.AlreadyExisting = false;
                                File.AppendAllText(hashFileName, hashSum);
                            }
                            else
                            {
                                folders2Md5LogEntry.HashSum = File.ReadAllText(hashFileName).Trim();
                                folders2Md5LogEntry.AlreadyExisting = true;
                            }
                            folders2Md5LogEntries.Add(folders2Md5LogEntry);
                        }
                    );
                }
            );


            if (folders2Md5LogEntries.Any())
            {
                //todo: problems with multithreading and string builder
                //Parallel.ForEach(folders2Md5LogEntries,
                //    logEntry =>
                //    {
                //        if (logEntry != null)
                //        {
                //            stringBuilder.Append(
                //                $"file:///{logEntry.FileName.Replace("\\", "/")};{logEntry.AlreadyExisting};{logEntry.Type};{logEntry.HashSum};{Environment.NewLine}");
                //        }
                //    });
                foreach (var logEntry in folders2Md5LogEntries)
                {
                    if (logEntry != null)
                    {
                        stringBuilder.Append(
                            $"{logEntry.FileName};{logEntry.Type};{logEntry.HashSum};{logEntry.AlreadyExisting};{Environment.NewLine}");
                    }
                }

                appendAllTextWithHeadline.For($@"{configuration.LoggingPath}\Folders2Md5_Log_{DateTime.Now:yyyy-MM-dd_HHmm}.csv", stringBuilder,
                    "FileName;Type;HashSum;AlreadyExisting;");

                if (folders2Md5LogEntries.Any(item => item != null && item.AlreadyExisting == false))
                {
                    result = new ObservableCollection<Folders2Md5LogEntry>(folders2Md5LogEntries.Where(item => item != null && item.AlreadyExisting == false));
                }
            }


            if (configuration.CloseHiddenInstancesOnFinish)
            {
                CurrentHiddenInstance.Close();
            }
            return result;
        }

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

        #region Flyout

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

        #endregion Flyout

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
            bool isCorrect = true;

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

        #endregion
    }
}