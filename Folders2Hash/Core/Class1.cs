using System;
using System.Collections.Specialized;
using System.Configuration;

namespace Folders2Hash.Core
{
    /// <summary>
    ///     Interface for classes to get values from or set values in ApplicationSettingsBase
    /// </summary>
    public interface IExtendedSettings
    {
        /// <summary>
        ///     Get value of type T
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="fallback"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Get<T>(string setting, T fallback = default(T));

        /// <summary>
        ///     Set value of type T
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="value"></param>
        void Set(string setting, object value);
    }

    /// <inheritdoc />
    /// <summary>
    ///     Classes to get values from or set values in ApplicationSettingsBase
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExtendedSettingsByApplicationSettingsBase : IExtendedSettings
    {
        private readonly ApplicationSettingsBase _settingsBase;

        /// <summary>
        /// </summary>
        /// <param name="settingsBase"></param>
        public ExtendedSettingsByApplicationSettingsBase(ApplicationSettingsBase settingsBase)
        {
            _settingsBase = settingsBase ?? throw new ArgumentNullException(nameof(settingsBase));
        }

        /// <inheritdoc />
        /// <summary>
        ///     Get value of type T
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="fallback"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(string setting, T fallback = default(T))
        {
            var value = (T) _settingsBase[setting];
            if (fallback != null)
            {
                if (IsValueEmpty(value))
                {
                    return fallback;
                }
            }
            return value;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Set value of type T
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="value"></param>
        public void Set(string setting, object value)
        {
            _settingsBase[setting] = value;
            _settingsBase.Save();
        }

        private bool IsValueEmpty<T>(T value)
        {
            if (value == null)
            {
                return true;
            }
            if (value is string)
            {
                if (string.IsNullOrWhiteSpace(value as string))
                {
                    return true;
                }
            }
            if (value is StringCollection)
            {
                var collection = value as StringCollection;
                if (collection.Count == 0)
                {
                    return true;
                }
            }
            else
            {
                if (value.Equals(default(T)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}