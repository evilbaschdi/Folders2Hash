using System;
using System.IO;
using System.Security.Cryptography;

namespace Folders2Md5.Internal
{
    public class Calculate : ICalculate
    {
        public string Hash(string filename, string type)
        {
            using(var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var hash = ((HashAlgorithm) CryptoConfig.CreateFromName(type.ToUpper())).ComputeHash(fs);
                var converted = BitConverter.ToString(hash).Replace("-", string.Empty);

                return converted;
            }
        }
    }
}