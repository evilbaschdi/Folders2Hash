using System;
using System.IO;
using Folders2Hash.Models;
using Newtonsoft.Json;

namespace Folders2Hash.Core
{
    /// <inheritdoc />
    public class SilentConfiguration : ISilentConfiguration
    {
        private readonly ISilentConfigurationPath _silentConfigurationPath;
        private Configuration _configuration;

        /// <summary>
        ///     Constructor of the class
        /// </summary>
        /// <param name="silentConfigurationPath"></param>
        public SilentConfiguration(ISilentConfigurationPath silentConfigurationPath
        )
        {
            _silentConfigurationPath = silentConfigurationPath ?? throw new ArgumentNullException(nameof(silentConfigurationPath));
        }

        /// <inheritdoc />
        public Configuration Value
        {
            get
            {
                if (_configuration == null)
                {
                    if (File.Exists(_silentConfigurationPath.Value))
                    {
                        var config = File.ReadAllText(_silentConfigurationPath.Value);
                        _configuration = JsonConvert.DeserializeObject<Configuration>(config);
                        _configuration.CloseHiddenInstancesOnFinish = true;
                    }
                    else
                    {
                        _configuration = null;
                    }
                }
                return _configuration;
            }
        }
    }
}