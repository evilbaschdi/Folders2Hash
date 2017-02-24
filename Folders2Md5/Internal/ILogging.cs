using System.Collections.Concurrent;
using EvilBaschdi.Core.DotNetExtensions;
using Folders2Md5.Models;

namespace Folders2Md5.Internal
{
    /// <summary>
    ///     Is generating a logging file.
    /// </summary>
    public interface ILogging : IRunFor2<ConcurrentBag<Folders2Md5LogEntry>, Configuration>
    {
    }
}