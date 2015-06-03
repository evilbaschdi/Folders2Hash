using Folders2Md5.Internal;
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

            if (e.Args.Any())
            {
                //Folders2Md5.exe g 'F:\Setup\Images' l 'C:\temp'
                if (e.Args.Count() == 4 &&
                   (e.Args.Contains("logging") || e.Args.Contains("l")) &&
                   (e.Args.Contains("generate") || e.Args.Contains("g"))
                    )
                {
                    mainWindow.CurrentHiddenInstance = mainWindow;

                    var configuration = new Configuration
                    {
                        //HashType.
                        HashType = "md5",
                        //Application has to be closed if triggered through command line.
                        CloseHiddenInstancesOnFinish = true,
                        InitialDirectory = e.Args[1].Replace("'", ""),
                        LoggingPath = e.Args[3].Replace("'", ""),
                        KeepFileExtension = false
                    };
                    //HashType.
                    //Application has to be closed if triggered through command line.

                    mainWindow.GenerateHashs(configuration);
                }
                //Folders2Md5.exe g 'F:\Setup\Images' l 'C:\temp' k
                if (e.Args.Count() == 5 &&
                   (e.Args.Contains("logging") || e.Args.Contains("l")) &&
                   (e.Args.Contains("generate") || e.Args.Contains("g")) &&
                   (e.Args.Contains("keep") || e.Args.Contains("k"))
                    )
                {
                    mainWindow.CurrentHiddenInstance = mainWindow;

                    var configuration = new Configuration
                    {
                        //HashType.
                        HashType = "md5",
                        //Application has to be closed if triggered through command line.
                        CloseHiddenInstancesOnFinish = true,
                        InitialDirectory = e.Args[1].Replace("'", ""),
                        LoggingPath = e.Args[3].Replace("'", ""),
                        KeepFileExtension = true
                    };
                    //HashType.
                    //Application has to be closed if triggered through command line.

                    mainWindow.GenerateHashs(configuration);
                }
                else
                {
                    mainWindow.Close();
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