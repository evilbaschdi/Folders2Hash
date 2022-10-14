using System.ComponentModel;
using System.Windows;
using Folders2Hash.Core;
using Folders2Hash.Internal;
#if (!DEBUG)
using ControlzEx.Theming;

#endif

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
#if (!DEBUG)
            ThemeManager.Current.SyncTheme(ThemeSyncMode.SyncAll);
#endif
            _mainWindow = new();

            IConfigurationPath configurationPath = new SilentConfigurationPath();
            IWritableConfiguration writableConfiguration = new WritableConfiguration(configurationPath);

            if (e?.Args != null && e.Args.Any())
            {
                var firstArg = e.Args.First().ToLower();
                //hidden
                if (firstArg.Equals("-silent") && writableConfiguration.Value != null)
                {
                    _mainWindow.CurrentHiddenInstance = _mainWindow;
                    _mainWindow.RunPreconfiguredHashCalculation(writableConfiguration.Value);
                }
                //HashEvaluationDialog
                else
                {
                    IHashAlgorithmDictionary hashAlgorithmDictionary = new HashAlgorithmDictionary();
                    foreach (var hashEvaluationDialog in from dictionaryValue in hashAlgorithmDictionary.Value.Values
                                                         where firstArg.EndsWith(dictionaryValue)
                                                         select new HashEvaluationDialog
                                                                {
                                                                    HashFile = firstArg,
                                                                    HashType = dictionaryValue
                                                                })
                    {
                        hashEvaluationDialog.Closing += HashEvaluationDialogClosing;
                        hashEvaluationDialog.Show();
                        break;
                    }
                }
            }
            //Default
            else
            {
                _mainWindow.SetCurrentValue(Window.ShowInTaskbarProperty, true);
                _mainWindow.SetCurrentValue(UIElement.VisibilityProperty, Visibility.Visible);
            }

            base.OnStartup(e);
        }

        private void HashEvaluationDialogClosing(object sender, CancelEventArgs e)
        {
            _mainWindow?.Close();
        }
    }
}