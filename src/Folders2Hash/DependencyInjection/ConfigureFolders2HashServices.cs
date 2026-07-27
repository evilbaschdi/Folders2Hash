using EvilBaschdi.Core.Internal;
using EvilBaschdi.Core.Logging;
using EvilBaschdi.Core.Settings.ByMachineAndUser;
using Folders2Hash.Core;
using Folders2Hash.Internal;
using Folders2Hash.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Folders2Hash.DependencyInjection;

/// <summary>
///     Registers Folders2Hash application services in the dependency injection container.
/// </summary>
public static class ConfigureFolders2HashServices
{
    /// <summary>
    ///     Registers core Folders2Hash services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static void AddFolders2HashServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // EvilBaschdi.Core services
        services.AddSingleton<IFileListFromPath, FileListFromPath>();
        services.AddSingleton<ILogFilePathWithInterval, LogFilePathWithInterval>();
        services.AddSingleton<IAppSettingsFromJsonFile, AppSettingsFromJsonFile>();
        services.AddSingleton<IAppSettingsFromJsonFileByMachineAndUser, AppSettingsFromJsonFileByMachineAndUser>();
        services.AddSingleton<IAppSettingByKey, AppSettingByKey>();

        // Folders2Hash settings
        services.AddSingleton<IWritableConfiguration, WritableConfiguration>();

        // Folders2Hash business services
        services.AddSingleton<IApplicationBasics, ApplicationBasics>();
        services.AddSingleton<IHashAlgorithmDictionary, HashAlgorithmDictionary>();
        services.AddSingleton<IHashAlgorithmByName, HashAlgorithmByName>();
        services.AddSingleton<ICalculate, Calculate>();
        services.AddSingleton<ILogging, Logging>();
        services.AddSingleton<IFileListCalculationProcessor, FileListCalculationProcessor>();
    }
}