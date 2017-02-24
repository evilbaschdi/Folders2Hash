using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EvilBaschdi.Core.DirectoryExtensions;
using EvilBaschdi.Core.DotNetExtensions;
using Folders2Md5.Models;

namespace Folders2Md5.Internal
{
    /// <summary>
    ///     Processes file calculation by file path
    /// </summary>
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
            if (calculate == null)
            {
                throw new ArgumentNullException(nameof(calculate));
            }
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            if (logging == null)
            {
                throw new ArgumentNullException(nameof(logging));
            }
            _calculate = calculate;
            _filePath = filePath;
            _logging = logging;
        }

        /// <inheritdoc />
        public ObservableCollection<Folders2Md5LogEntry> ValueFor(Configuration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            var folders2Md5LogEntries = new ConcurrentBag<Folders2Md5LogEntry>();
            var result = new ObservableCollection<Folders2Md5LogEntry>();
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
                                          "folders2md5_log_"
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
                            var folders2Md5LogEntry = new Folders2Md5LogEntry
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

                                folders2Md5LogEntry.HashSum = hashSum;
                                folders2Md5LogEntry.AlreadyExisting = false;
                                File.AppendAllText(hashFileName, hashSum);
                            }
                            else
                            {
                                folders2Md5LogEntry.HashSum = File.ReadAllText(hashFileName).Trim();
                                folders2Md5LogEntry.AlreadyExisting = true;
                            }
                            folders2Md5LogEntries.Add(folders2Md5LogEntry);
                        }
                    );
                }
            );

            if (folders2Md5LogEntries.Any())
            {
                if (folders2Md5LogEntries.Any(item => item != null && item.AlreadyExisting == false))
                {
                    result = new ObservableCollection<Folders2Md5LogEntry>(folders2Md5LogEntries.Where(item => item != null && item.AlreadyExisting == false));


                    _logging.Run(folders2Md5LogEntries, configuration);
                }
            }
            return result;
        }
    }
}