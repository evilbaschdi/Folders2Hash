using System.Text;

namespace Folders2Hash.Internal;

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
        ArgumentNullException.ThrowIfNull(file);

        ArgumentNullException.ThrowIfNull(type);

        return keepFileExtension
            ? $@"{Path.GetDirectoryName(file)}\{Path.GetFileName(file)}.{type}"
            : $@"{Path.GetDirectoryName(file)}\{Path.GetFileNameWithoutExtension(file)}.{type}";
    }

    /// <inheritdoc />
    public IEnumerable<KeyValuePair<string, string>> Hashes(string filename, Dictionary<string, string> hashAlgorithmTypes)
    {
        ArgumentNullException.ThrowIfNull(filename);

        ArgumentNullException.ThrowIfNull(hashAlgorithmTypes);

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

                list.Add(new(hashAlgorithmType, sb.ToString()));
            }

            fileStream.Close();
        }
        catch (Exception exception)
        {
            list.Add(new("", exception.Message));
        }

        return list;
    }
}