using System.Collections.Generic;
using System.IO;

namespace Folders2Md5.Internal
{
    public interface IFilePath
    {
        string CleanFileName(string path);

        List<string> GetFileList(string initialDirectory);

        byte[] ReadFullFile(Stream stream);

        string HashFileName(string file, string type, bool keepFileExtension);
    }
}