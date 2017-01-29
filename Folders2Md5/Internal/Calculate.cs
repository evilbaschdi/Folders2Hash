using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Folders2Md5.Internal
{
    /// <summary>
    /// </summary>
    public class Calculate : ICalculate
    {
        /// <summary>
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public string Hash(string filename, string type)
        {
            switch (type.ToLower())
            {
                case "md5":
                    var md5 = MD5.Create();
                    return Hash(filename, md5);
                case "sha1":
                    var sha1 = SHA1.Create();
                    return Hash(filename, sha1);
                case "sha256":
                    var sha256 = SHA256.Create();
                    return Hash(filename, sha256);
                case "sha384":
                    var sha384 = SHA384.Create();
                    return Hash(filename, sha384);
                case "sha512":
                    var sha512 = SHA512.Create();
                    return Hash(filename, sha512);
            }
            return "Algorithm not supported";
        }


        /// <summary>
        /// </summary>
        /// <param name="file"></param>
        /// <param name="type"></param>
        /// <param name="keepFileExtension"></param>
        /// <returns></returns>
        public string HashFileName(string file, string type, bool keepFileExtension)
        {
            return keepFileExtension
                ? $@"{Path.GetDirectoryName(file)}\{Path.GetFileName(file)}.{type}"
                : $@"{Path.GetDirectoryName(file)}\{Path.GetFileNameWithoutExtension(file)}.{type}";
        }

        private string Hash(string filename, HashAlgorithm hashAlgorithm)
        {
            try
            {
                using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    var hash = hashAlgorithm.ComputeHash(fs);

                    var sb = new StringBuilder();
                    foreach (var t in hash)
                    {
                        sb.Append(t.ToString("X2"));
                    }
                    return sb.ToString();
                }
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
    }
}