using System;
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

        public IEnumerable<string> GetSubdirectoriesContainingOnlyFiles(string path)
        {
            return from subdirectory in Directory.GetDirectories(path, "*", SearchOption.AllDirectories)
                where Directory.GetDirectories(subdirectory).Length == 0
                select subdirectory;
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

        public string Md5FileName(string file, string md5Hash, bool keepOriginalFileName)
        {
            return keepOriginalFileName
                ? String.Format(@"{0}\{1}.md5", Path.GetDirectoryName(file),
                    Path.GetFileNameWithoutExtension(file))
                : String.Format(@"{0}\{1}.md5_{2}", Path.GetDirectoryName(file), md5Hash,
                    CleanFileName(file));
        }
    }
}