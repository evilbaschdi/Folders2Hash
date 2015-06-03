using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Folders2Md5.Internal
{
    public class Calculate : ICalculate
    {
        public string Md5Hash(string filename)
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

        public string Sha1Hash(string filename)
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
    }
}