using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Folders2Md5.Internal
{
    public class FilePath : IFilePath
    {
        public string CleanFileName(string path)
        {
            var fileName = Path.GetFileName(path);
            return fileName?.Replace(".", "_").Replace(" ", "_") ?? string.Empty;
        }

        private IEnumerable<string> GetSubdirectoriesContainingOnlyFiles(string path)
        {
            return Directory.GetDirectories(path, "*", SearchOption.AllDirectories).ToList();
        }

        public List<string> GetFileList(string initialDirectory)
        {
            var fileList = new List<string>();
            var initialDirectoryFileList = Directory.GetFiles(initialDirectory).ToList();

            foreach(var file in initialDirectoryFileList.Where(file => IsValidFileName(file, fileList)))
            {
                fileList.Add(file);
            }

            var initialDirectorySubdirectoriesFileList =
                GetSubdirectoriesContainingOnlyFiles(initialDirectory).SelectMany(Directory.GetFiles);

            foreach(var file in initialDirectorySubdirectoriesFileList.Where(file => IsValidFileName(file, fileList)))
            {
                fileList.Add(file);
            }

            return fileList;
        }

        private bool IsValidFileName(string file, ICollection<string> fileList)
        {
            var type = "md5";
            var fileExtension = Path.GetExtension(file);
            return !fileList.Contains(file) && !file.ToLower().Contains("folders2md5_log_") &&
                   !string.IsNullOrWhiteSpace(fileExtension) &&
                   !fileExtension.ToLower().Contains(type) && !fileExtension.ToLower().Equals(".ini") &&
                   !fileExtension.ToLower().Equals(".db");
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

        public string HashFileName(string file, string type, bool keepFileExtension)
        {
            return keepFileExtension
                ? $@"{Path.GetDirectoryName(file)}\{Path.GetFileName(file)}.{type}"
                : $@"{Path.GetDirectoryName(file)}\{Path.GetFileNameWithoutExtension(file)}.{type}";
        }
    }
}