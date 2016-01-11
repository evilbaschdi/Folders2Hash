namespace Folders2Md5.Core
{
    public interface IApplicationSettings
    {
        string InitialDirectory { get; set; }
        string LoggingPath { get; set; }
        bool KeepFileExtension { get; set; }
    }
}