using System.Runtime.Serialization;

namespace Folders2Hash.Models;

/// <summary>
/// </summary>
[DataContract]
public class Configuration
{
    /// <summary>
    /// </summary>
    [DataMember]
    public bool CloseHiddenInstancesOnFinish { get; set; }

    /// <summary>
    /// </summary>
    [DataMember]
    public List<string> HashTypes { get; set; }

    /// <summary>
    /// </summary>
    [DataMember]
    public bool KeepFileExtension { get; set; }

    /// <summary>
    /// </summary>
    [DataMember]
    public string LoggingPath { get; set; }

    /// <summary>
    /// </summary>
    [DataMember]
    public List<string> PathsToScan { get; set; }
}