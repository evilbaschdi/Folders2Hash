namespace Folders2Md5.Core
{
    public interface IApplicationBasics
    {
        void BrowseFolder();

        void BrowseLoggingFolder();

        string GetInitialDirectory();

        string GetLoggingPath();
    }
}