using System;

namespace Folders2Hash.Core
{
    /// <inheritdoc />
    public class SilentConfigurationPath : IConfigurationPath
    {
        /// <inheritdoc />
        public string Value { get; } = $@"{AppDomain.CurrentDomain.BaseDirectory}SilentConfiguration.json";
    }

    /// <inheritdoc />
    public class ConfigurationPath : IConfigurationPath
    {
        /// <inheritdoc />
        public string Value { get; } = $@"{AppDomain.CurrentDomain.BaseDirectory}Configuration.json";
    }
}