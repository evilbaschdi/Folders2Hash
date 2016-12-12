namespace Folders2Md5.Core
{
    /// <summary>
    /// </summary>
    public interface IApplicationSettings
    {
        string InitialDirectory { get; set; }
        string LoggingPath { get; set; }
        bool KeepFileExtension { get; set; }
    }
}