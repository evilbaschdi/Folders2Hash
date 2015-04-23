using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            Generate.IsEnabled = !string.IsNullOrWhiteSpace(Properties.Settings.Default.InitialDirectory) &&
                                 Directory.Exists(Properties.Settings.Default.InitialDirectory);
        }

        private void GenerateHashsOnClick(object sender, RoutedEventArgs e)
        {
            GenerateHashs();
        }

        public void GenerateHashs()
        {
            GenerateHashs(_initialDirectory);
        }

        public List<string> Types()
        {
            var list = new List<string>();

            list.Add("md5");
            //list.Add("sha1");

            return list;
        }

        public void GenerateHashs(string initialDirectory)
        {
            var outputList = new List<string>();
            Output.Text = "";

            var filePath = new FilePath();
            var fileList = filePath.GetFileList(initialDirectory);

            Parallel.ForEach(Types(), type => Parallel.ForEach(fileList, file =>
            {
                var output = "";
                var fileExtension = Path.GetExtension(file);
                if(string.IsNullOrWhiteSpace(fileExtension) || !fileExtension.Contains(type))
                {
                    var calculate = new Calculate();
                    var fileName = filePath.HashFileName(file, type);

                    if(!File.Exists(fileName))
                    {
                        var hashSum = "";
                        switch(type)
                        {
                            case "md5":
                                hashSum = calculate.Md5Hash(file);
                                break;

                            case "sha1":
                                hashSum = calculate.Sha1Hash(file);
                                break;
                        }

                        output += string.Format("file: '{1}'{0}", Environment.NewLine, file);

                        output += string.Format("{0}: {1}{2}", type.ToUpper(), hashSum, Environment.NewLine);

                        File.AppendAllText(fileName, hashSum);
                        output += string.Format("generated: {1}{0}", Environment.NewLine, fileName);
                    }
                    else
                    {
                        output += string.Format("already existing: {1}{0}", Environment.NewLine, fileName);
                    }

                    output += Environment.NewLine;
                }
                outputList.Add(output);
            }))
                ;
            outputList.ForEach(o => Output.Text += o);

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