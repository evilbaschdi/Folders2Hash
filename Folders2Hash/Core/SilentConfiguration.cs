using System;
using System.IO;
using EvilBaschdi.Core;
using Folders2Hash.Models;
using Newtonsoft.Json;

namespace Folders2Hash.Core
{
    /// <inheritdoc cref="ISilentConfiguration" />
    /// <inheritdoc cref="CachedValue{T}" />
    public class SilentConfiguration : CachedValue<Configuration>, ISilentConfiguration
    {
        private readonly ISilentConfigurationPath _silentConfigurationPath;
        private Configuration _configuration = new Configuration();

        /// <summary>
        ///     Constructor of the class
        /// </summary>
        /// <param name="silentConfigurationPath"></param>
        public SilentConfiguration(ISilentConfigurationPath silentConfigurationPath
        )
        {
            _silentConfigurationPath = silentConfigurationPath ?? throw new ArgumentNullException(nameof(silentConfigurationPath));
        }

        protected override Configuration NonCachedValue
        {
            get
            {
                if (!File.Exists(_silentConfigurationPath.Value))
                {
                    return _configuration;
                }

                var config = File.ReadAllText(_silentConfigurationPath.Value);
                _configuration = JsonConvert.DeserializeObject<Configuration>(config);
                _configuration.CloseHiddenInstancesOnFinish = true;
                return _configuration;
            }
        }
    }
}