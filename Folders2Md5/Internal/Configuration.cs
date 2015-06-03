namespace Folders2Md5.Internal
{
    public class Configuration : IConfiguration
    {
        public bool CloseHiddenInstancesOnFinish { get; set; }

        public bool KeepFileExtension { get; set; }

        public string InitialDirectory { get; set; }

        public string LoggingPath { get; set; }

        public string HashType { get; set; }
    }
}