using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Folders2Md5.Internal
{
    public class Calculate
    {
        public string Md5Hash(string filename)
        {
            var md5 = MD5.Create();
            using(var fs = new FileStream(filename, FileMode.Open))
            {
                var hash = md5.ComputeHash(fs);

                var sb = new StringBuilder();
                foreach(var t in hash)
                {
                    sb.Append(t.ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}