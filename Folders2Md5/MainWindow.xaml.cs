using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Folders2Md5.Core;
using Folders2Md5.Internal;
using MahApps.Metro.Controls;

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

        private readonly ApplicationStyle _style;
        private readonly ApplicationBasics _basics;
        private string _initialDirectory;
        private string _fileNameFormat;

        public MainWindow()
        {
            _style = new ApplicationStyle(this);
            _basics = new ApplicationBasics();
            InitializeComponent();
            _style.Load();
            ValidateForm();
            _initialDirectory = _basics.GetInitialDirectory();
            InitialDirectory.Text = _initialDirectory;
        }

        private void ValidateForm()
        {
            Generate.IsEnabled = !string.IsNullOrWhiteSpace(Properties.Settings.Default.InitialDirectory);
            if(!string.IsNullOrWhiteSpace(Properties.Settings.Default.FileNameFormat))
            {
                var format = Properties.Settings.Default.FileNameFormat;
                OriginalFileName.IsChecked = format == OriginalFileName.Name;
                HashInFileName.IsChecked = format == HashInFileName.Name;
            }
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
            var filePath = new FilePath();
            var fileList = Directory.GetFiles(initialDirectory).ToList();
            fileList.AddRange(
                filePath.GetSubdirectoriesContainingOnlyFiles(initialDirectory).SelectMany(Directory.GetFiles));
            Output.Text = "";
            foreach(var file in fileList)
            {
                var fileExtension = Path.GetExtension(file);
                if(!string.IsNullOrWhiteSpace(fileExtension) && fileExtension.Contains("md5"))
                {
                    continue;
                }
                Output.Text += string.Format("file: '{1}'{0}", Environment.NewLine, file);
                var calculate = new Calculate();
                var md5Hash = calculate.Md5Hash(file);
                var keepOriginalFileName = _fileNameFormat == "OriginalFileName";
                var md5FileName = filePath.Md5FileName(file, md5Hash, keepOriginalFileName);

                Output.Text += string.Format("MD5: {1}{0}", Environment.NewLine, md5Hash);

                if(!File.Exists(md5FileName))
                {
                    File.AppendAllText(md5FileName, md5Hash);
                    Output.Text += string.Format("generated: {1}{0}", Environment.NewLine, md5FileName);
                }
                else
                {
                    Output.Text += string.Format("already existing: {1}{0}", Environment.NewLine, md5FileName);
                }
                Output.Text += Environment.NewLine;
            }

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

        #endregion Initial Directory

        #region Settings

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

        private void FileNameFormat(object sender, RoutedEventArgs e)
        {
            var radiobutton = (RadioButton) sender;
            _fileNameFormat = radiobutton.Name;
        }

        private void SaveFileNameFormatClick(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.FileNameFormat = _fileNameFormat;
            Properties.Settings.Default.Save();
        }

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

        #endregion Settings
    }
}