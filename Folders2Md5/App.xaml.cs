using System;
using System.Linq;
using System.Windows;
using Folders2Hash.Core;

namespace Folders2Hash
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    // ReSharper disable RedundantExtendsListEntry
    public partial class App : Application
        // ReSharper restore RedundantExtendsListEntry
    {
        /// <exception cref="OverflowException">
        ///     The dictionary already contains the maximum number of elements (
        ///     <see cref="F:System.Int32.MaxValue" />).
        /// </exception>
        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = new MainWindow();

            ISilentConfigurationPath silentConfigurationPath = new SilentConfigurationPath();
            ISilentConfiguration silentConfiguration = new SilentConfiguration(silentConfigurationPath);

            if (e?.Args != null && e.Args.Any() && e.Args.First().Equals("-silent") && silentConfiguration.Value != null)
            {
                mainWindow.CurrentHiddenInstance = mainWindow;
                mainWindow.RunPreconfiguredHashCalculation(silentConfiguration.Value);
            }
            else
            {
                mainWindow.ShowInTaskbar = true;
                mainWindow.Visibility = Visibility.Visible;
            }
        }
    }
}