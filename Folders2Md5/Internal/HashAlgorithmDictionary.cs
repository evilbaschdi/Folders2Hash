using System.Collections.Generic;

namespace Folders2Hash.Internal
{
    /// <summary>
    /// </summary>
    public class HashAlgorithmDictionary : IHashAlgorithmDictionary
    {
        /// <summary>
        /// </summary>
        public Dictionary<string, string> Value => new Dictionary<string, string>
                                                   {
                                                       { "MD5", "md5" },
                                                       { "SHA-1", "sha1" },
                                                       { "SHA-256", "sha256" },
                                                       { "SHA-384", "sha384" },
                                                       { "SHA-512", "sha512" }
                                                   };
    }
}