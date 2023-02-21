using EvilBaschdi.Settings.ByMachineAndUser;
using EvilBaschdi.Settings.Writable;

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