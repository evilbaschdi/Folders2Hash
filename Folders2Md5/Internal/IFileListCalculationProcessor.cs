using System.Collections.ObjectModel;
using EvilBaschdi.Core.DotNetExtensions;
using Folders2Md5.Models;

namespace Folders2Md5.Internal
{
    /// <summary>
    ///     Processes file calculation by file path
    /// </summary>
    public interface IFileListCalculationProcessor : IValueFor<Configuration, ObservableCollection<Folders2Md5LogEntry>>
    {
    }
}