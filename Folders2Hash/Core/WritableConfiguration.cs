using System.IO;
using System.Windows;
using EvilBaschdi.Core;
using Folders2Hash.Models;
using Newtonsoft.Json;

namespace Folders2Hash.Core;

/// <inheritdoc cref="IWritableConfiguration" />
/// <inheritdoc cref="CachedValue{T}" />
public class WritableConfiguration : CachedWritableValue<Configuration>, IWritableConfiguration
{
    private readonly IConfigurationPath _configurationPath;

    /// <summary>
    ///     Constructor of the class
    /// </summary>
    /// <param name="configurationPath"></param>
    public WritableConfiguration(IConfigurationPath configurationPath
    )
    {
        _configurationPath = configurationPath ?? throw new ArgumentNullException(nameof(configurationPath));
    }

    /// <inheritdoc />
    protected override Configuration NonCachedValue
    {
        get
        {
            Configuration configuration = new();
            if (!File.Exists(_configurationPath.Value))
            {
                return configuration;
            }

            var json = File.ReadAllText(_configurationPath.Value);
            configuration = JsonConvert.DeserializeObject<Configuration>(json) ?? new Configuration();
            configuration.CloseHiddenInstancesOnFinish = true;
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

        if (_configurationPath.Value.Contains("Silent"))
        {
            MessageBox.Show("you cannot change silent Configuration through GUI");
            return;
        }

        var json = JsonConvert.SerializeObject(configuration);
        File.WriteAllText(_configurationPath.Value, json);
    }
}