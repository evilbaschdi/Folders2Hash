using System.Collections.Concurrent;
using EvilBaschdi.Core;
using Folders2Hash.Models;

namespace Folders2Hash.Internal
{
    /// <summary>
    ///     Is generating a logging file.
    /// </summary>
    public interface ILogging : IRunFor2<ConcurrentBag<LogEntry>, Configuration>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="message"></param>
        /// <param name="configuration"></param>
        void RunFor(string file, string message, Configuration configuration);
    }
}