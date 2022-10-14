namespace Folders2Hash.Core;

/// <inheritdoc />
public class SilentConfigurationPath : IConfigurationPath
{
    /// <inheritdoc />
    public string Value { get; } = $@"{AppDomain.CurrentDomain.BaseDirectory}SilentConfiguration.json";
}