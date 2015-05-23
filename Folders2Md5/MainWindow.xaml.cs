using Folders2Md5.Core;
using Folders2Md5.Internal;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Folders2Md5
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    // ReSharper disable RedundantExtendsListEntry
    public partial class MainWindow : MetroWindow
        // ReSharper restore RedundantExtendsListEntry
    {
        public bool CloseHiddenInstancesOnFinish { get; set; }

        public MainWindow CurrentHiddenInstance { get; set; }

        private readonly IApplicationStyle _style;
        private readonly IApplicationBasics _basics;
        private readonly ICalculate _calculate;
        private string _initialDirectory;
        private string _loggingPath;

        public MainWindow()
        {
            _style = new ApplicationStyle(this);
            _basics = new ApplicationBasics();
            InitializeComponent();
            _style.Load();
            ValidateForm();
            _calculate = new Calculate();
        }

        private void ValidateForm()
        {
            Generate.IsEnabled = !string.IsNullOrWhiteSpace(Properties.Settings.Default.InitialDirectory) &&
                                 Directory.Exists(Properties.Settings.Default.InitialDirectory);

            KeepFileExtension.IsChecked = Properties.Settings.Default.KeepFileExtension;

            _initialDirectory = _basics.GetInitialDirectory();
            InitialDirectory.Text = _initialDirectory;

            _loggingPath = _basics.GetLoggingPath();
            LoggingPath.Text = _loggingPath;
        }

        private void GenerateHashsOnClick(object sender, RoutedEventArgs e)
        {
            GenerateHashs();
        }

        public void GenerateHashs()
        {
            GenerateHashs(_initialDirectory);
        }

        public void GenerateHashs(string initialDirectory)
        {
            var type = "md5";
            var outputList = new List<string>();
            var outputText = string.Format("Start: {0}{1}{1}", DateTime.Now, Environment.NewLine);

            var filePath = new FilePath();
            var fileList = filePath.GetFileList(initialDirectory).Distinct();

            Parallel.ForEach(fileList, file =>
            {
                var output = string.Empty;

                var fileName = filePath.HashFileName(file, type);

                if(!File.Exists(fileName))
                {
                    var hashSum = "";
                    switch(type)
                    {
                        case "md5":
                            hashSum = _calculate.Md5Hash(file);
                            break;

                        case "sha1":
                            hashSum = _calculate.Sha1Hash(file);
                            break;
                    }

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
            outputText += string.Format("End: {0}{1}{1}", DateTime.Now, Environment.NewLine);
            Output.Text = outputText;

            File.AppendAllText(
                $@"{_loggingPath}\Folders2Md5_Log_{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.txt",
                outputText);

            if(CloseHiddenInstancesOnFinish)
            {
                CurrentHiddenInstance.Close();
            }
        }

        #region Initial Directory

        private void BrowseClick(object sender, RoutedEventArgs e)
        {
            _basics.BrowseFolder();
            InitialDirectory.Text = Properties.Settings.Default.InitialDirectory;
            _initialDirectory = Properties.Settings.Default.InitialDirectory;
            ValidateForm();
        }

        private void InitialDirectoryOnLostFocus(object sender, RoutedEventArgs e)
        {
            if(Directory.Exists(InitialDirectory.Text))
            {
                Properties.Settings.Default.InitialDirectory = InitialDirectory.Text;
                Properties.Settings.Default.Save();
                _initialDirectory = Properties.Settings.Default.InitialDirectory;
            }
            ValidateForm();
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

            if(activeFlyout.IsOpen && stayOpen)
            {
                activeFlyout.IsOpen = true;
            }
            else
            {
                activeFlyout.IsOpen = !activeFlyout.IsOpen;
            }
        }

        #endregion Flyout

        #region Style

        private void SaveStyleClick(object sender, RoutedEventArgs e)
        {
            _style.SaveStyle();
        }

        private void Theme(object sender, RoutedEventArgs e)
        {
            _style.SetTheme(sender, e);
        }

        private void AccentOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _style.SetAccent(sender, e);
        }

        #endregion Style

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
                Properties.Settings.Default.KeepFileExtension = checkBox.IsChecked.Value;
            }
            Properties.Settings.Default.Save();
        }

        private void BrowseLoggingPathClick(object sender, RoutedEventArgs e)
        {
            _basics.BrowseLoggingFolder();
            LoggingPath.Text = Properties.Settings.Default.LoggingPath;
            _loggingPath = Properties.Settings.Default.LoggingPath;
            ValidateForm();
        }

        private void LoggingPathOnLostFocus(object sender, RoutedEventArgs e)
        {
            if(Directory.Exists(LoggingPath.Text))
            {
                Properties.Settings.Default.InitialDirectory = LoggingPath.Text;
                Properties.Settings.Default.Save();
                _loggingPath = Properties.Settings.Default.LoggingPath;
            }
            ValidateForm();
        }

        #endregion GenerationSettings
    }
}