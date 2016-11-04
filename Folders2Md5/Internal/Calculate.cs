using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Folders2Md5.Internal
{
    public class Calculate : ICalculate
    {
        public string Hash(string filename, string type)
        {
            switch (type.ToLower())
            {
                case "md5":
                    return Md5Hash(filename);
                case "sha1":
                    return Sha1Hash(filename);
                case "sha256":
                    return Sha256Hash(filename);
                case "sha384":
                    return Sha384Hash(filename);
                case "sha512":
                    return Sha512Hash(filename);

            }
            return "type not found";
        }

        //public List<string> All(string filename)
        //{
        //    throw new NotImplementedException();
        //}

        public string HashFileName(string file, string type, bool keepFileExtension)
        {
            return keepFileExtension
                ? $@"{Path.GetDirectoryName(file)}\{Path.GetFileName(file)}.{type}"
                : $@"{Path.GetDirectoryName(file)}\{Path.GetFileNameWithoutExtension(file)}.{type}";
        }

        private string Md5Hash(string filename)
        {
            try
            {
                var md5 = MD5.Create();
                using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    var hash = md5.ComputeHash(fs);

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

        private string Sha1Hash(string filename)
        {
            try
            {
                var sha1 = SHA1.Create();
                using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    var hash = sha1.ComputeHash(fs);

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

        private string Sha256Hash(string filename)
        {
            try
            {
                var sha1 = SHA256.Create();
                using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    var hash = sha1.ComputeHash(fs);

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

        private string Sha384Hash(string filename)
        {
            try
            {
                var sha1 = SHA384.Create();
                using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    var hash = sha1.ComputeHash(fs);

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

        private string Sha512Hash(string filename)
        {
            try
            {
                var sha1 = SHA512.Create();
                using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    var hash = sha1.ComputeHash(fs);

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