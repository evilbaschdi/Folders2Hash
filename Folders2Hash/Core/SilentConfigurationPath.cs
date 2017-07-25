using System;

namespace Folders2Hash.Core
{
    /// <inheritdoc />
    public class SilentConfigurationPath : ISilentConfigurationPath
    {
        /// <inheritdoc />
        public string Value { get; } = $@"{AppDomain.CurrentDomain.BaseDirectory}SilentConfiguration.json";
    }
}