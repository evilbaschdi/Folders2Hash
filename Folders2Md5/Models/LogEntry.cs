using System;
using System.Runtime.Serialization;

namespace Folders2Hash.Models
{
    /// <summary>
    /// </summary>
    [DataContract]
    public class LogEntry
    {
        /// <summary>
        /// </summary>
        [DataMember]
        public string FileName { get; set; }

        /// <summary>
        /// </summary>
        [DataMember]
        public string ShortFileName { get; set; }

        /// <summary>
        /// </summary>
        [DataMember]
        public string HashFileName { get; set; }

        /// <summary>
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// </summary>
        [DataMember]
        public string HashSum { get; set; }

        /// <summary>
        /// </summary>
        [DataMember]
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// </summary>
        [DataMember]
        public bool AlreadyExisting { get; set; }
    }
}