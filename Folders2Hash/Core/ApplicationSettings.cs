using System;
using System.Collections.Specialized;
using System.IO;
using EvilBaschdi.CoreExtended.AppHelpers;

namespace Folders2Hash.Core
{
    /// <inheritdoc />
    public class ApplicationSettings : IApplicationSettings
    {
        private readonly IAppSettingsBase _extendedSettings;

        /// <summary>
        ///     Constructor of the class
        /// </summary>
        /// <param name="extendedSettings"></param>
        public ApplicationSettings(IAppSettingsBase extendedSettings)
        {
            _extendedSettings = extendedSettings ?? throw new ArgumentNullException(nameof(extendedSettings));
        }

        /// <inheritdoc />
        public string InitialDirectory
        {
            get => _extendedSettings.Get("InitialDirectory", "");
            set => _extendedSettings.Set("InitialDirectory", value);
        }

        /// <inheritdoc />
        public string LoggingPath
        {
            get => _extendedSettings.Get("LoggingPath", $@"{Path.GetPathRoot(Environment.SystemDirectory)}Temp");
            set => _extendedSettings.Set("LoggingPath", value);
        }

        /// <inheritdoc />
        public bool KeepFileExtension
        {
            get => _extendedSettings.Get<bool>("KeepFileExtension");
            set => _extendedSettings.Set("KeepFileExtension", value);
        }

        /// <inheritdoc />
        public StringCollection CurrentHashAlgorithms
        {
            get => _extendedSettings.Get("CurrentHashAlgorithms", new StringCollection
                                                                  {
                                                                      "md5"
                                                                  });
            set => _extendedSettings.Set("CurrentHashAlgorithms", value);
        }
    }
}