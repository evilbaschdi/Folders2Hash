using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using Folders2Hash.Internal;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Folders2Hash
{
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
        }

        /// <summary>
        /// </summary>
        public string HashFile { get; set; }

        /// <summary>
        /// </summary>
        public string HashType { get; set; }

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
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;

            Cursor = Cursors.Wait;

            var options = new MetroDialogSettings
                          {
                              ColorScheme = MetroDialogColorScheme.Accented
                          };

            MetroDialogOptions = options;
            _controller = await this.ShowProgressAsync("Loading...", "evaluation is running", true, options);
            _controller.SetIndeterminate();
            _controller.Canceled += ControllerCanceled;

            _task = Task<bool>.Factory.StartNew(IsHashValid);
            await _task;
            _task.GetAwaiter().OnCompleted(TaskCompleted);
        }

        private bool IsHashValid()
        {
            _sourceFileName = HashFile.Substring(0, HashFile.Length - (HashType.Length + 1));
            _hashFileContent = File.ReadAllText(HashFile);

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
            return _sourceFileHash.Trim().Equals(_hashFileContent.Trim(), StringComparison.InvariantCultureIgnoreCase);
        }

        private void TaskCompleted()
        {
            HashFileName.Header = $"Hash File: '{HashFile}'";
            SourceFileName.Header = $"Original Source File: '{_sourceFileName}'";
            HashFileContent.Text = _hashFileContent;
            SourceFileHash.Text = _sourceFileHash;

            if (_task.Result)
            {
                HashFileContent.Background = Brushes.GreenYellow;
                HashFileContent.Foreground = Brushes.Black;
                SourceFileHash.Background = Brushes.GreenYellow;
                SourceFileHash.Foreground = Brushes.Black;
            }
            else
            {
                HashFileContent.Background = Brushes.DarkRed;
                HashFileContent.Foreground = Brushes.White;
                SourceFileHash.Background = Brushes.DarkRed;
                SourceFileHash.Foreground = Brushes.White;
            }

            _controller.CloseAsync();
            _controller.Closed += ControllerClosed;
        }

        private void ControllerClosed(object sender, EventArgs e)
        {
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
            TaskbarItemInfo.ProgressValue = 1;
            Cursor = Cursors.Arrow;
        }

        private void ControllerCanceled(object sender, EventArgs e)
        {
            _controller.CloseAsync();
            _controller.Closed += ControllerClosed;
        }
    }
}