using System.Linq;
using System.Windows;

namespace Folders2Md5
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
// ReSharper disable RedundantExtendsListEntry
    public partial class App : Application
// ReSharper restore RedundantExtendsListEntry
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = new MainWindow();

            if(e.Args.Any())
            {
                if((e.Args.Contains("-generate") || e.Args.Contains("-g")) && e.Args.Count() == 1)
                {
                    mainWindow.CurrentHiddenInstance = mainWindow;
                    mainWindow.CloseHiddenInstancesOnFinish = true;

                    mainWindow.GenerateHashs();
                }
                if((e.Args.Contains("-generate") || e.Args.Contains("-g")) &&
                   (e.Args.Contains("-path") || e.Args.Contains("-p")) && e.Args.Count() == 3)
                {
                    mainWindow.CurrentHiddenInstance = mainWindow;
                    mainWindow.CloseHiddenInstancesOnFinish = true;
                    mainWindow.GenerateHashs(e.Args[2].Replace("'", ""));
                }
            }
            else
            {
                mainWindow.ShowInTaskbar = true;
                mainWindow.Visibility = Visibility.Visible;
            }
        }
    }
}