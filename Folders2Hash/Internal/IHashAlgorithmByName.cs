using System.Security.Cryptography;
using EvilBaschdi.Core;

namespace Folders2Hash.Internal
{
    /// <inheritdoc />
    public interface IHashAlgorithmByName : IValueFor<string, HashAlgorithm>
    {
    }
}