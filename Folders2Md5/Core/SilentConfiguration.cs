using System;
using System.IO;
using Folders2Md5.Models;
using Newtonsoft.Json;

namespace Folders2Md5.Core
{
    /// <summary>
    ///     Provides the configuration for silent run
    /// </summary>
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
            if (silentConfigurationPath == null)
            {
                throw new ArgumentNullException(nameof(silentConfigurationPath));
            }
            _silentConfigurationPath = silentConfigurationPath;
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