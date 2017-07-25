using System.Collections.Concurrent;
using EvilBaschdi.Core.DotNetExtensions;
using Folders2Hash.Models;

namespace Folders2Hash.Internal
{
    /// <inheritdoc />
    /// <summary>
    ///     Is generating a logging file.
    /// </summary>
    public interface ILogging : IRunFor2<ConcurrentBag<LogEntry>, Configuration>
    {
    }
}