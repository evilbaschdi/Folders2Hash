using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Folders2Hash.Internal
{
    /// <inheritdoc />
    public class Calculate : ICalculate
    {
        private readonly IHashAlgorithmByName _hashAlgorithmByName;

        /// <summary>
        ///     Constructor of the class
        /// </summary>
        /// <param name="hashAlgorithmByName"></param>
        public Calculate(IHashAlgorithmByName hashAlgorithmByName)
        {
            _hashAlgorithmByName = hashAlgorithmByName ?? throw new ArgumentNullException(nameof(hashAlgorithmByName));
        }

        /// <inheritdoc />
        public string HashFileName(string file, string type, bool keepFileExtension)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return keepFileExtension
                ? $@"{Path.GetDirectoryName(file)}\{Path.GetFileName(file)}.{type}"
                : $@"{Path.GetDirectoryName(file)}\{Path.GetFileNameWithoutExtension(file)}.{type}";
        }

        /// <inheritdoc />
        public List<KeyValuePair<string, string>> Hashes(string filename, Dictionary<string, string> hashAlgorithmTypes)
        {
            if (filename == null)
            {
                throw new ArgumentNullException(nameof(filename));
            }

            if (hashAlgorithmTypes == null)
            {
                throw new ArgumentNullException(nameof(hashAlgorithmTypes));
            }

            var list = new List<KeyValuePair<string, string>>();
            try
            {
                var fileInfo = new FileInfo(filename);
                var fileStream = fileInfo.Open(FileMode.Open);
                foreach (var hashAlgorithmType in hashAlgorithmTypes.Keys)
                {
                    fileStream.Position = 0;
                    var hashAlgorithm = _hashAlgorithmByName.ValueFor(hashAlgorithmType);
                    var hash = hashAlgorithm.ComputeHash(fileStream);

                    var sb = new StringBuilder();
                    foreach (var t in hash)
                    {
                        sb.Append(t.ToString("X2"));
                    }

                    list.Add(new KeyValuePair<string, string>(hashAlgorithmType, sb.ToString()));
                }

                fileStream.Close();
            }
            catch (Exception exception)
            {
                list.Add(new KeyValuePair<string, string>("", exception.Message));
            }

            return list;
        }
    }
}