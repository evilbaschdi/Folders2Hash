using System.Runtime.Serialization;

namespace Folders2Hash.Models;

/// <summary>
/// </summary>
[DataContract]
public record LogEntry(string FileName, string ShortFileName, string HashFileName, string Type, DateTime TimeStamp, string HashSum, bool AlreadyExisting);