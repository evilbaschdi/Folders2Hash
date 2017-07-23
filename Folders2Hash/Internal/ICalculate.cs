namespace Folders2Hash.Internal
{
    /// <summary>
    /// </summary>
    public interface ICalculate
    {
        /// <summary>
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        string Hash(string filename, string type);

        // List<string> All(string filename);
        /// <summary>
        /// </summary>
        /// <param name="file"></param>
        /// <param name="type"></param>
        /// <param name="keepFileExtension"></param>
        /// <returns></returns>
        string HashFileName(string file, string type, bool keepFileExtension);
    }
}