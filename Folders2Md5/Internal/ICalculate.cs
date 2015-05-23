namespace Folders2Md5.Internal
{
    public interface ICalculate
    {
        string Md5Hash(string filename);
        string Sha1Hash(string filename);
    }
}