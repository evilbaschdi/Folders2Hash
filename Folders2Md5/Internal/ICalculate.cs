namespace Folders2Md5.Internal
{
    public interface ICalculate
    {
        string Hash(string filename, string type);

        string HashFileName(string file, string type, bool keepFileExtension);
    }
}