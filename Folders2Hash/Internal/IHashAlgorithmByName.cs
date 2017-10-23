using System.Security.Cryptography;
using EvilBaschdi.Core.DotNetExtensions;

namespace Folders2Hash.Internal
{
    /// <inheritdoc />
    public interface IHashAlgorithmByName : IValueFor<string, HashAlgorithm>
    {
    }
}