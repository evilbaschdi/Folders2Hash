using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EvilBaschdi.Core.Avalonia.DependencyInjection;
using EvilBaschdi.Core.Avalonia.Lifetime;
using EvilBaschdi.Core.Settings.ByMachineAndUser;
using Folders2Hash.Internal;
using Folders2Hash.Settings;

namespace Folders2Hash;

internal class App : ApplicationWithSplash
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    protected override bool ResizeWithBorder400 => false;

    protected override void PreMainWindowCreation()
    {
        ApplicationServices.AppName = Current?.Name;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var args = desktop.Args ?? [];
            if (args.Length != 0 && args.First().Equals("-silent", StringComparison.OrdinalIgnoreCase))
            {
                var writableConfiguration = CreateWritableConfiguration();
                if (writableConfiguration.Value is { } configuration)
                {
                    var mainWindow = ApplicationServices.GetRequiredService<MainWindow>();
                    mainWindow.RunPreconfiguredHashCalculation(configuration);
                }

                desktop.Shutdown();
                return;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    protected override Window CreateMainWindow()
    {
        var mainWindow = ApplicationServices.GetRequiredService<MainWindow>();
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return mainWindow;
        }

        var firstArg = desktop.Args?.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(firstArg) || firstArg.Equals("-silent", StringComparison.OrdinalIgnoreCase))
        {
            return mainWindow;
        }

        var lowerArg = firstArg.ToLowerInvariant();
        var hashAlgorithmDictionary = ApplicationServices.GetRequiredService<IHashAlgorithmDictionary>();
        var hashType = hashAlgorithmDictionary.Value.Values.FirstOrDefault(dictionaryValue
                                                                               => lowerArg.EndsWith(dictionaryValue, StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(hashType))
        {
            return mainWindow;
        }

        var hashEvaluationDialog = ApplicationServices.GetRequiredService<HashEvaluationDialog>();
        hashEvaluationDialog.HashFile = firstArg;
        hashEvaluationDialog.HashType = hashType;
        return hashEvaluationDialog;
    }

    private static IWritableConfiguration CreateWritableConfiguration()
    {
        IAppSettingsFromJsonFile appSettingsFromJsonFile = new AppSettingsFromJsonFile();
        IAppSettingsFromJsonFileByMachineAndUser silentAppSettingsFromJsonFileByMachineAndUser = new SilentAppSettingsFromJsonFileByMachineAndUser();
        IAppSettingByKey appSettingByKey = new AppSettingByKey(appSettingsFromJsonFile, silentAppSettingsFromJsonFileByMachineAndUser);
        return new WritableConfiguration(appSettingByKey);
    }
}