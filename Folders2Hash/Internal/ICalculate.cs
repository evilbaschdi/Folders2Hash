namespace Folders2Hash.Internal;

/// <summary>
/// </summary>
public interface ICalculate
{
    // List<string> All(string filename);
    /// <summary>
    /// </summary>
    /// <param name="file"></param>
    /// <param name="type"></param>
    /// <param name="keepFileExtension"></param>
    /// <returns></returns>
    string HashFileName(string file, string type, bool keepFileExtension);

    /// <summary>
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="hashAlgorithmTypes"></param>
    /// <returns></returns>
    IEnumerable<KeyValuePair<string, string>> Hashes(string filename, Dictionary<string, string> hashAlgorithmTypes);
}