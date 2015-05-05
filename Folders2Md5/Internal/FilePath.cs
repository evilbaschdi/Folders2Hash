using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Folders2Md5.Internal
{
    public class FilePath
    {
        public string CleanFileName(string path)
        {
            var fileName = Path.GetFileName(path);
            // ReSharper disable PossibleNullReferenceException
            return fileName.Replace(".", "_").Replace(" ", "_");
            // ReSharper restore PossibleNullReferenceException
        }

        private IEnumerable<string> GetSubdirectoriesContainingOnlyFiles(string path)
        {
            return Directory.GetDirectories(path, "*", SearchOption.AllDirectories).ToList();
        }

        public List<string> GetFileList(string initialDirectory)
        {
            var fileList = Directory.GetFiles(initialDirectory).ToList();
            fileList.AddRange(
                GetSubdirectoriesContainingOnlyFiles(initialDirectory).SelectMany(Directory.GetFiles));
            return fileList;
        }

        public byte[] ReadFullFile(Stream stream)
        {
            var buffer = new byte[32768];
            using(var ms = new MemoryStream())
            {
                while(true)
                {
                    var read = stream.Read(buffer, 0, buffer.Length);
                    if(read <= 0)
                    {
                        return ms.ToArray();
                    }
                    ms.Write(buffer, 0, read);
                }
            }
        }

        public string HashFileName(string file, string type)
        {
            return $@"{Path.GetDirectoryName(file)}\{Path.GetFileNameWithoutExtension(file)}.{type}";
        }
    }
}