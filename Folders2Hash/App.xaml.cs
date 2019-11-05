using System.ComponentModel;
using System.Linq;
using System.Windows;
using EvilBaschdi.CoreExtended.Metro;
using Folders2Hash.Core;
using Folders2Hash.Internal;

namespace Folders2Hash
{
    /// <inheritdoc />
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class App : Application
        // ReSharper restore RedundantExtendsListEntry
    {
        private MainWindow _mainWindow;

        /// <inheritdoc />
        /// <exception cref="T:System.OverflowException">
        ///     The dictionary already contains the maximum number of elements (
        ///     <see cref="F:System.Int32.MaxValue" />).
        /// </exception>
        protected override void OnStartup(StartupEventArgs e)
        {
            var themeManagerHelper = new ThemeManagerHelper();
            themeManagerHelper.RegisterSystemColorTheme();
            _mainWindow = new MainWindow();

            ISilentConfigurationPath silentConfigurationPath = new SilentConfigurationPath();
            ISilentConfiguration silentConfiguration = new SilentConfiguration(silentConfigurationPath);
            IHashAlgorithmDictionary hashAlgorithmDictionary = new HashAlgorithmDictionary();

            if (e?.Args != null && e.Args.Any())
            {
                var firstArg = e.Args.First().ToLower();
                if (firstArg.Equals("-silent") && silentConfiguration.Value != null)
                {
                    _mainWindow.CurrentHiddenInstance = _mainWindow;
                    _mainWindow.RunPreconfiguredHashCalculation(silentConfiguration.Value);
                }
                else
                {
                    foreach (var dictionaryValue in hashAlgorithmDictionary.Value.Values)
                    {
                        if (!firstArg.EndsWith(dictionaryValue))
                        {
                            continue;
                        }

                        var hashEvaluationDialog = new HashEvaluationDialog
                                                   {
                                                       HashFile = firstArg,
                                                       HashType = dictionaryValue
                                                   };
                        hashEvaluationDialog.Closing += HashEvaluationDialogClosing;
                        hashEvaluationDialog.Show();
                        break;
                    }
                }
            }
            else
            {
                _mainWindow.ShowInTaskbar = true;
                _mainWindow.Visibility = Visibility.Visible;
            }

            base.OnStartup(e);
        }

        private void HashEvaluationDialogClosing(object sender, CancelEventArgs e)
        {
            _mainWindow.Close();
        }
    }
}