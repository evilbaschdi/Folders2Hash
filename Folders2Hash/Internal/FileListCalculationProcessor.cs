using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using EvilBaschdi.Core.Extensions;
using EvilBaschdi.Core.Internal;
using EvilBaschdi.Core.Model;
using Folders2Hash.Models;

namespace Folders2Hash.Internal;

/// <inheritdoc />
public class FileListCalculationProcessor : IFileListCalculationProcessor
{
    private readonly ICalculate _calculate;
    private readonly IFileListFromPath _filePath;
    private readonly ILogging _logging;

    /// <summary>
    ///     Constructor of the class
    /// </summary>
    /// <param name="calculate"></param>
    /// <param name="filePath"></param>
    /// <param name="logging"></param>
    public FileListCalculationProcessor(ICalculate calculate, IFileListFromPath filePath, ILogging logging)
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

        var excludeExtensionList = new List<string>
                                   {
                                       "ini",
                                       "db"
                                   };
        excludeExtensionList.AddRange(configurationHashTypes);

        var excludeFileNameList = new List<string>
                                  {
                                      "folders2hash_log"
                                  };

        var fileList = new ConcurrentBag<string>();
        //true == directory, false == file
        Parallel.ForEach(configuration.PathsToScan,
            item =>
            {
                var filter = new FileListFromPathFilter
                             {
                                 FilterExtensionsNotToEqual = excludeExtensionList,
                                 FilterFileNamesNotToEqual = excludeFileNameList
                             };
                fileList.AddRange(_filePath.ValueFor(item, filter).Distinct());
            }
        );

        var distinctFileList = fileList.Distinct();

        //Parallel.ForEach(distinctFileList,
        //    file => { }
        //);

        foreach (var file in distinctFileList)
        {
            try
            {
                var fileInfo = file.FileInfo();
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
                        var logEntry = new LogEntry(file, fileInfo.Name, hashFileName, type.ToUpper(), DateTime.Now, File.ReadAllText(hashFileName).Trim(), true);
                        hashLogEntries.Add(logEntry);
                    }
                }

                var calculatedHashes = _calculate.Hashes(file, localHashTypesToCalculate);

                foreach (var (type, hashSum) in calculatedHashes)
                {
                    var hashFileName = localHashTypesToCalculate.First(name => name.Key.Equals(type)).Value;
                    var logEntry = new LogEntry(file, fileInfo.Name, hashFileName, type.ToUpper(), DateTime.Now, hashSum, false);

                    File.AppendAllText(hashFileName, hashSum);
                    hashLogEntries.Add(logEntry);
                }
            }
            catch (Exception e)
            {
                _logging.RunFor(file, e.Message, configuration);
            }
        }

        if (!hashLogEntries.Any())
        {
            return result;
        }

        {
            if (hashLogEntries.Any(item => item is { AlreadyExisting: false }))
            {
                result = new(hashLogEntries.Where(item => item is { AlreadyExisting: false }));
            }

            _logging.RunFor(hashLogEntries, configuration);
        }

        return result;
    }
}