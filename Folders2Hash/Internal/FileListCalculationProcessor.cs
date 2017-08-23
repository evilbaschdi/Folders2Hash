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
            var currentHashAlgorithms = configuration.HashTypes;

            var includeExtensionList = new List<string>();
            var excludeExtensionList = new List<string>
                                       {
                                           "ini",
                                           "db"
                                       };
            excludeExtensionList.AddRange(currentHashAlgorithms);

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

            Parallel.ForEach(currentHashAlgorithms,
                type =>
                {
                    Parallel.ForEach(fileList.Distinct(),
                        file =>
                        {
                            var hashFileName = _calculate.HashFileName(file, type, configuration.KeepFileExtension);

                            var fileInfo = new FileInfo(file);
                            var logEntry = new LogEntry
                                           {
                                               FileName = file,
                                               ShortFileName = fileInfo.Name,
                                               HashFileName = hashFileName,
                                               Type = type.ToUpper(),
                                               TimeStamp = DateTime.Now
                                           };

                            if (!File.Exists(hashFileName) || File.GetLastWriteTime(file) > File.GetLastWriteTime(hashFileName))
                            {
                                var hashSum = _calculate.Hash(file, type);

                                if (File.Exists(hashFileName))
                                {
                                    File.Delete(hashFileName);
                                }

                                logEntry.HashSum = hashSum;
                                logEntry.AlreadyExisting = false;
                                File.AppendAllText(hashFileName, hashSum);
                            }
                            else
                            {
                                logEntry.HashSum = File.ReadAllText(hashFileName).Trim();
                                logEntry.AlreadyExisting = true;
                            }
                            hashLogEntries.Add(logEntry);
                        }
                    );
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