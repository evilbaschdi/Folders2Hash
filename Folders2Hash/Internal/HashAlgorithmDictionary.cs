﻿namespace Folders2Hash.Internal;

/// <inheritdoc />
public class HashAlgorithmDictionary : IHashAlgorithmDictionary
{
    /// <inheritdoc />
    /// <summary>
    ///     todo: please also extend RegisterFileTypes\Program.cs
    /// </summary>
    public Dictionary<string, string> Value => new()
                                               {
                                                   { "MD5", "md5" },
                                                   { "SHA-1", "sha1" },
                                                   { "SHA-256", "sha256" },
                                                   { "SHA-384", "sha384" },
                                                   { "SHA-512", "sha512" }
                                               };
}