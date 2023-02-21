using EvilBaschdi.Core;
using EvilBaschdi.Settings.ByMachineAndUser;
using Folders2Hash.Models;
using JetBrains.Annotations;

namespace Folders2Hash.Settings;

/// <inheritdoc cref="IWritableConfiguration" />
/// <inheritdoc cref="CachedValue{T}" />
public class WritableConfiguration : CachedWritableValue<Configuration>, IWritableConfiguration
{
    private readonly IAppSettingByKey _appSettingByKey;

    /// <summary>
    ///     Constructor of the class
    /// </summary>
    /// <param name="appSettingByKey"></param>
    public WritableConfiguration([NotNull] IAppSettingByKey appSettingByKey)
    {
        _appSettingByKey = appSettingByKey ?? throw new ArgumentNullException(nameof(appSettingByKey));
    }

    /// <inheritdoc />
    protected override Configuration NonCachedValue
    {
        get
        {
            Configuration configuration = new()
                                          {
                                              CloseHiddenInstancesOnFinish = true,
                                              HashTypes = _appSettingByKey.ValueFor<List<string>>("HashTypes"),
                                              KeepFileExtension = _appSettingByKey.ValueFor<bool>("KeepFileExtension"),
                                              LoggingPath = _appSettingByKey.ValueFor("LoggingPath"),
                                              PathsToScan = _appSettingByKey.ValueFor<List<string>>("PathsToScan")
                                          };

            return configuration;
        }
    }

    /// <inheritdoc />
    protected override void SaveValue(Configuration configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        _appSettingByKey.RunFor("HashTypes", configuration.HashTypes);
        _appSettingByKey.RunFor("KeepFileExtension", configuration.KeepFileExtension);
        _appSettingByKey.RunFor("LoggingPath", configuration.LoggingPath);
        _appSettingByKey.RunFor("PathsToScan", configuration.PathsToScan);
    }
}