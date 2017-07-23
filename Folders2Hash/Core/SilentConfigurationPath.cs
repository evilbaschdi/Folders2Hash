using System;

namespace Folders2Hash.Core
{
    /// <summary>
    ///     Path of the silent configuration file
    /// </summary>
    public class SilentConfigurationPath : ISilentConfigurationPath
    {
        /// <inheritdoc />
        public string Value { get; } = $@"{AppDomain.CurrentDomain.BaseDirectory}SilentConfiguration.json";
    }
}