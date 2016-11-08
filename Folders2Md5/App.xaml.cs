using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Windows;
using Folders2Md5.Internal;

namespace Folders2Md5
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    // ReSharper disable RedundantExtendsListEntry
    public partial class App : Application
        // ReSharper restore RedundantExtendsListEntry
    {
        private readonly ConcurrentDictionary<string, bool> _pathsToScan = new ConcurrentDictionary<string, bool>();


        /// <exception cref="OverflowException">
        ///     The dictionary already contains the maximum number of elements (
        ///     <see cref="F:System.Int32.MaxValue" />).
        /// </exception>
        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = new MainWindow();


            if (e.Args.Any())
            {
                //Folders2Md5.exe g 'F:\Setup\Images' l 'C:\temp'
                var argumentsWithKeepFalse = ArgumentsWithKeepFalse(e.Args);
                //Folders2Md5.exe g 'F:\Setup\Images' l 'C:\temp' k
                var argumentsWithKeepTrue = ArgumentsWithKeepTrue(e.Args);

                if (argumentsWithKeepFalse || argumentsWithKeepTrue)
                {
                    var keep = argumentsWithKeepTrue && !argumentsWithKeepFalse;

                    mainWindow.CurrentHiddenInstance = mainWindow;
                    _pathsToScan.TryAdd(e.Args[1].Replace("'", ""), true);
                    var configuration = new Configuration
                                        {
                                            //HashType.
                                            HashType = "md5",
                                            //Application has to be closed if triggered through command line.
                                            CloseHiddenInstancesOnFinish = true,
                                            PathsToScan = _pathsToScan,
                                            LoggingPath = e.Args[3].Replace("'", ""),
                                            KeepFileExtension = keep
                                        };

                    mainWindow.RunPreconfiguredHashCalculation(configuration);
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

        private bool ArgumentsWithKeepTrue(string[] args)
        {
            return args.Length == 5 &&
                   (args.Contains("logging") || args.Contains("l")) &&
                   (args.Contains("generate") || args.Contains("g")) &&
                   (args.Contains("keep") || args.Contains("k"))
                ;
        }

        private bool ArgumentsWithKeepFalse(string[] args)
        {
            return args.Length == 4 &&
                   (args.Contains("logging") || args.Contains("l")) &&
                   (args.Contains("generate") || args.Contains("g"));
        }
    }
}