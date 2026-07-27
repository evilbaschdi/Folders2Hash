using EvilBaschdi.Core.Settings.ByMachineAndUser;
using EvilBaschdi.Core.Settings.Writable;

namespace Folders2Hash.Settings;

/// <inheritdoc cref="WritableSettingsFromJsonFile" />
public class SilentAppSettingsFromJsonFileByMachineAndUser : WritableSettingsFromJsonFile, IAppSettingsFromJsonFileByMachineAndUser
{
    /// <summary>
    ///     Constructor
    /// </summary>
    public SilentAppSettingsFromJsonFileByMachineAndUser()
        : base($"Settings/App.{Environment.MachineName}.{Environment.UserName}.Silent.json", true)
    {
    }
}