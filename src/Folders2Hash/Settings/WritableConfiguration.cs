using EvilBaschdi.Core;
using EvilBaschdi.Core.Settings.ByMachineAndUser;
using Folders2Hash.Models;

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
    public WritableConfiguration(IAppSettingByKey appSettingByKey)
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
        ArgumentNullException.ThrowIfNull(configuration);

        _appSettingByKey.RunFor("HashTypes", configuration.HashTypes);
        _appSettingByKey.RunFor("KeepFileExtension", configuration.KeepFileExtension);
        _appSettingByKey.RunFor("LoggingPath", configuration.LoggingPath);
        _appSettingByKey.RunFor("PathsToScan", configuration.PathsToScan);
    }
}