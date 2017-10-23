using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EvilBaschdi.Core.DirectoryExtensions;
using EvilBaschdi.Core.DotNetExtensions;
using Folders2Hash.Models;

namespace Folders2Hash.Internal
{
    /// <inheritdoc />
    public class FileListCalculationProcessor : IFileListCalculationProcessor
    {
        private readonly ICalculate _calculate;
        private readonly IFilePath _filePath;
        private readonly ILogging _logging;

        /// <summary>
        ///     Constructor of the class
        /// </summary>
        /// <param name="calculate"></param>
        /// <param name="filePath"></param>
        /// <param name="logging"></param>
        public FileListCalculationProcessor(ICalculate calculate, IFilePath filePath, ILogging logging)
        {
            _calculate = calculate ?? throw new ArgumentNullException(nameof(calculate));
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _logging = logging ?? throw new ArgumentNullException(nameof(logging));
        }

        /// <inheritdoc />
        public ObservableCollection<LogEntry> ValueFor(Configuration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            var hashLogEntries = new ConcurrentBag<LogEntry>();
            var result = new ObservableCollection<LogEntry>();
            var configurationHashTypes = configuration.HashTypes;

            var includeExtensionList = new List<string>();
            var excludeExtensionList = new List<string>
                                       {
                                           "ini",
                                           "db"
                                       };
            excludeExtensionList.AddRange(configurationHashTypes);

            var includeFileNameList = new List<string>();
            var excludeFileNameList = new List<string>
                                      {
                                          "folders2hash_log"
                                      };

            var fileList = new ConcurrentBag<string>();
            //true == directory, false == file
            Parallel.ForEach(configuration.PathsToScan,
                item =>
                {
                    if (item.Value)
                    {
                        fileList.AddRange(_filePath.GetFileList(item.Key, includeExtensionList, excludeExtensionList, includeFileNameList, excludeFileNameList).Distinct());
                    }
                    else
                    {
                        fileList.Add(item.Key);
                    }
                }
            );


            var distinctFileList = fileList.Distinct();

            Parallel.ForEach(distinctFileList,
                file =>
                {
                    var fileInfo = new FileInfo(file);
                    var localHashTypesToCalculate = new Dictionary<string, string>();

                    foreach (var type in configurationHashTypes)
                    {
                        var hashFileName = _calculate.HashFileName(file, type, configuration.KeepFileExtension);

                        if (!File.Exists(hashFileName) || File.GetLastWriteTime(file) > File.GetLastWriteTime(hashFileName))
                        {
                            localHashTypesToCalculate.Add(type, hashFileName);

                            if (File.Exists(hashFileName))
                            {
                                File.Delete(hashFileName);
                            }
                        }
                        else
                        {
                            var logEntry = new LogEntry
                                           {
                                               FileName = file,
                                               ShortFileName = fileInfo.Name,
                                               HashFileName = hashFileName,
                                               Type = type.ToUpper(),
                                               TimeStamp = DateTime.Now,
                                               HashSum = File.ReadAllText(hashFileName).Trim(),
                                               AlreadyExisting = true
                                           };
                            hashLogEntries.Add(logEntry);
                        }
                    }


                    var calculatedHashes = _calculate.Hashes(file, localHashTypesToCalculate);

                    foreach (var calculatedHash in calculatedHashes)
                    {
                        var type = calculatedHash.Key;
                        var hashSum = calculatedHash.Value;
                        var hashFileName = localHashTypesToCalculate.First(name => name.Key.Equals(type)).Value;
                        var logEntry = new LogEntry
                                       {
                                           FileName = file,
                                           ShortFileName = fileInfo.Name,
                                           HashFileName = hashFileName,
                                           Type = type.ToUpper(),
                                           TimeStamp = DateTime.Now,
                                           HashSum = hashSum,
                                           AlreadyExisting = false
                                       };
                        File.AppendAllText(hashFileName, hashSum);
                        hashLogEntries.Add(logEntry);
                    }
                }
            );


            if (hashLogEntries.Any())
            {
                if (hashLogEntries.Any(item => item != null && item.AlreadyExisting == false))
                {
                    result = new ObservableCollection<LogEntry>(hashLogEntries.Where(item => item != null && item.AlreadyExisting == false));
                }

                _logging.RunFor(hashLogEntries, configuration);
            }
            return result;
        }
    }
}