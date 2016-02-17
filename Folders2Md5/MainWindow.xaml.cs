using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shell;
using EvilBaschdi.Core.Application;
using EvilBaschdi.Core.Browsers;
using EvilBaschdi.Core.Wpf;
using Folders2Md5.Core;
using Folders2Md5.Internal;
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
        public bool CloseHiddenInstancesOnFinish { get; set; }
        public MainWindow CurrentHiddenInstance { get; set; }

        private readonly BackgroundWorker _bw;
        private Configuration _configuration;
        private string _result;
        private readonly IMetroStyle _style;
        private readonly IFolderBrowser _folderBrowser;
        private readonly IApplicationSettings _applicationSettings;
        private readonly IApplicationBasics _basics;
        private readonly ICalculate _calculate;
        private readonly IToast _toast;
        private readonly ISettings _coreSettings;
        private string _initialDirectory;
        private string _loggingPath;
        private int _overrideProtection;
        private int _executionCount;

        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        public MainWindow()
        {
            _coreSettings = new CoreSettings();
            _folderBrowser = new ExplorerFolderBrower();
            _applicationSettings = new ApplicationSettings();
            _basics = new ApplicationBasics(_folderBrowser, _applicationSettings);
            InitializeComponent();
            _bw = new BackgroundWorker();
            TaskbarItemInfo = new TaskbarItemInfo();
            _style = new MetroStyle(this, Accent, Dark, Light, _coreSettings);
            _style.Load(true, false);
            _calculate = new Calculate();
            _toast = new Toast("md5.png");
            Load();
        }

        private void Load()
        {
            Generate.IsEnabled = !string.IsNullOrWhiteSpace(_applicationSettings.InitialDirectory) &&
                                 Directory.Exists(_applicationSettings.InitialDirectory);

            KeepFileExtension.IsChecked = _applicationSettings.KeepFileExtension;

            _initialDirectory = _applicationSettings.InitialDirectory;
            InitialDirectory.Text = _initialDirectory;

            _loggingPath = !string.IsNullOrWhiteSpace(_applicationSettings.LoggingPath)
                ? _applicationSettings.LoggingPath
                : _applicationSettings.InitialDirectory;
            LoggingPath.Text = _loggingPath;

            _overrideProtection = 1;
        }

        private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Output.Text = _result;
            var message =
                $"Checksums for path '{_initialDirectory}' were generated." +
                $"{Environment.NewLine}You can find the logging file at '{_loggingPath}'.";

            ShowMessage("Completed", message);
            _toast.Show("Completed", message);

            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
            Cursor = Cursors.Arrow;
        }

        private void GenerateHashsOnClick(object sender, RoutedEventArgs e)
        {
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
            ConfigureGenerateHashs();
        }

        public void ConfigureGenerateHashs()
        {
            _executionCount++;
            var configuration = new Configuration
            {
                InitialDirectory = _initialDirectory,
                LoggingPath = _loggingPath,
                HashType = "md5",
                KeepFileExtension = _applicationSettings.KeepFileExtension
            };
            Cursor = Cursors.Wait;
            _configuration = configuration;
            if(_executionCount == 1)
            {
                _bw.DoWork += (o, args) => RunHashCalculation();
                _bw.WorkerReportsProgress = true;
                _bw.RunWorkerCompleted += BackgroundWorkerRunWorkerCompleted;
            }
            _bw.RunWorkerAsync();
        }

        public void RunPreconfiguredHashCalculation(Configuration configuration)
        {
            _configuration = configuration;
        }

        private void RunHashCalculation()
        {
            var configuration = _configuration;
            var type = configuration.HashType;
            var outputList = new List<string>();
            var outputText = $"Start: {DateTime.Now}{Environment.NewLine}{Environment.NewLine}";

            var filePath = new FilePath();
            var fileList = filePath.GetFileList(configuration.InitialDirectory).Distinct();

            Parallel.ForEach(fileList, file =>
            {
                var output = string.Empty;

                var fileName = filePath.HashFileName(file, type, configuration.KeepFileExtension);

                if(!File.Exists(fileName))
                {
                    var hashSum = _calculate.Hash(file, type);

                    output += $"file: '{file}'{Environment.NewLine}";

                    output += $"{type.ToUpper()}: {hashSum}{Environment.NewLine}";

                    File.AppendAllText(fileName, hashSum);
                    output += $"generated: '{fileName}'{Environment.NewLine}";
                }
                else
                {
                    output += $"already existing: '{fileName}'{Environment.NewLine}";
                }

                output += Environment.NewLine;

                outputList.Add(output);
            });
            outputList.ForEach(o => outputText += o);
            outputText += $"End: {DateTime.Now}{Environment.NewLine}{Environment.NewLine}";
            _result = outputText;

            File.AppendAllText(
                $@"{configuration.LoggingPath}\Folders2Md5_Log_{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.txt",
                outputText);

            if(configuration.CloseHiddenInstancesOnFinish)
            {
                CurrentHiddenInstance.Close();
            }
        }

        public async void ShowMessage(string title, string message)
        {
            var options = new MetroDialogSettings
            {
                ColorScheme = MetroDialogColorScheme.Theme
            };

            MetroDialogOptions = options;
            await this.ShowMessageAsync(title, message);
        }

        #region Initial Directory

        private void BrowseClick(object sender, RoutedEventArgs e)
        {
            _basics.BrowseFolder();
            InitialDirectory.Text = _applicationSettings.InitialDirectory;
            _initialDirectory = _applicationSettings.InitialDirectory;
            Load();
        }

        private void InitialDirectoryOnLostFocus(object sender, RoutedEventArgs e)
        {
            if(Directory.Exists(InitialDirectory.Text))
            {
                _applicationSettings.InitialDirectory = InitialDirectory.Text;
                _initialDirectory = _applicationSettings.InitialDirectory;
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
            if(activeFlyout == null)
            {
                return;
            }

            foreach(
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
            if(_overrideProtection == 0)
            {
                return;
            }
            _style.SaveStyle();
        }

        private void Theme(object sender, RoutedEventArgs e)
        {
            if(_overrideProtection == 0)
            {
                return;
            }
            _style.SetTheme(sender, e);
        }

        private void AccentOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(_overrideProtection == 0)
            {
                return;
            }
            _style.SetAccent(sender, e);
        }

        #endregion MetroStyle

        #region GenerationSettings

        private void KeepFileExtensionChecked(object sender, RoutedEventArgs e)
        {
            Handle(sender as CheckBox);
        }

        private void KeepFileExtensionUnchecked(object sender, RoutedEventArgs e)
        {
            Handle(sender as CheckBox);
        }

        private void Handle(ToggleButton checkBox)
        {
            if(checkBox.IsChecked != null)
            {
                _applicationSettings.KeepFileExtension = checkBox.IsChecked.Value;
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
            if(Directory.Exists(LoggingPath.Text))
            {
                _applicationSettings.LoggingPath = LoggingPath.Text;
                _loggingPath = _applicationSettings.LoggingPath;
            }
            Load();
        }

        #endregion GenerationSettings
    }
}