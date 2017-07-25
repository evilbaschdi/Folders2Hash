using System.Collections.ObjectModel;
using EvilBaschdi.Core.DotNetExtensions;
using Folders2Hash.Models;

namespace Folders2Hash.Internal
{
    /// <inheritdoc />
    /// <summary>
    ///     Processes file calculation by file path
    /// </summary>
    public interface IFileListCalculationProcessor : IValueFor<Configuration, ObservableCollection<LogEntry>>
    {
    }
}