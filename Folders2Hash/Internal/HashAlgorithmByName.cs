using System.Security.Cryptography;

namespace Folders2Hash.Internal;

/// <inheritdoc />
public class HashAlgorithmByName : IHashAlgorithmByName
{
    /// <inheritdoc />
    /// <param name="type"></param>
    /// <returns></returns>
    public HashAlgorithm ValueFor(string type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        switch (type.ToLower())
        {
            case "md5":
                var md5 = MD5.Create();
                return md5;
            case "sha1":
                var sha1 = SHA1.Create();
                return sha1;
            case "sha256":
                var sha256 = SHA256.Create();
                return sha256;
            case "sha384":
                var sha384 = SHA384.Create();
                return sha384;
            case "sha512":
                var sha512 = SHA512.Create();
                return sha512;
            default:
                return null;
        }
    }
}