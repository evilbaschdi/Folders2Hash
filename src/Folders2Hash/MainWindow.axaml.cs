using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using EvilBaschdi.About.Avalonia;
using EvilBaschdi.Core.Avalonia.DependencyInjection;
using EvilBaschdi.Core.Avalonia.Themes;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using Folders2Hash.Internal;
using Folders2Hash.Models;
using Folders2Hash.Settings;

namespace Folders2Hash;

internal partial class MainWindow : FAAppWindow
{
    private readonly IHashAlgorithmDictionary _hashAlgorithmDictionary;
    private readonly IWritableConfiguration _writableConfiguration;
    private readonly IFileListCalculationProcessor _fileListCalculationProcessor;
    private Configuration _configuration;
    private ObservableCollection<LogEntry> _logEntries;
    private ObservableCollection<SelectableObject<HashAlgorithmModel>> _observableCollection;

    // ReSharper disable once CollectionNeverUpdated.Local
    private readonly ConcurrentDictionary<string, bool> _pathsToScan = new();
    private string _loggingPath;

    public MainWindow(IHashAlgorithmDictionary hashAlgorithmDictionary,
                      IWritableConfiguration writableConfiguration,
                      IFileListCalculationProcessor fileListCalculationProcessor)
    {
        _hashAlgorithmDictionary = hashAlgorithmDictionary ?? throw new ArgumentNullException(nameof(hashAlgorithmDictionary));
        _writableConfiguration = writableConfiguration ?? throw new ArgumentNullException(nameof(writableConfiguration));
        _fileListCalculationProcessor = fileListCalculationProcessor ?? throw new ArgumentNullException(nameof(fileListCalculationProcessor));
        InitializeComponent();
        ThemeEngine.ApplyThemeToWindow(this, true);
        _configuration = _writableConfiguration.Value;
        _logEntries = [];
        _observableCollection = [];
        _loggingPath = string.Empty;
        Load();
    }

    public void RunPreconfiguredHashCalculation(Configuration configuration)
    {
        _configuration = configuration;
        RunHashCalculation();
    }

    private void Load()
    {
        GetHashAlgorithms();
        HashAlgorithms.ItemsSource = _observableCollection;

        var firstPath = _configuration.PathsToScan.FirstOrDefault();
        Generate.IsEnabled = !string.IsNullOrWhiteSpace(firstPath) && Directory.Exists(firstPath);
        KeepFileExtension.IsChecked = _configuration.KeepFileExtension;
        InitialDirectory.Text = firstPath ?? string.Empty;
        _loggingPath = !string.IsNullOrWhiteSpace(_configuration.LoggingPath) ? _configuration.LoggingPath : firstPath ?? string.Empty;
        LoggingPath.Text = _loggingPath;
    }

    private async void AboutWindowClick(object? sender, RoutedEventArgs e)
    {
        var aboutWindow = ApplicationServices.GetRequiredService<AboutWindow>();
        await aboutWindow.ShowDialog(this);
    }

    private async void GenerateHashesOnClick(object? sender, RoutedEventArgs e)
        => await ConfigureCalculationAndRunAsync();

    private async Task ConfigureCalculationAndRunAsync()
    {
        Generate.IsEnabled = false;
        try
        {
            _logEntries = await Task.Run(RunHashCalculation);
            ResultGrid.ItemsSource = _logEntries;
            var message = $"Check sums were generated.{Environment.NewLine}Logging file location: '{_loggingPath}'.";
            await ShowTaskDialogAsync("Completed", message, FASymbol.CopyFilled);
        }
        finally
        {
            _pathsToScan.Clear();
            Load();
        }
    }

    private ObservableCollection<LogEntry> RunHashCalculation()
    {
        var calculationConfiguration = _configuration;
        if (_pathsToScan.Count != 0)
        {
            calculationConfiguration = new Configuration
                                       {
                                           CloseHiddenInstancesOnFinish = _configuration.CloseHiddenInstancesOnFinish,
                                           HashTypes = [.. _configuration.HashTypes],
                                           KeepFileExtension = _configuration.KeepFileExtension,
                                           LoggingPath = _configuration.LoggingPath,
                                           PathsToScan = [.. _pathsToScan.Keys]
                                       };
        }

        return _fileListCalculationProcessor.ValueFor(calculationConfiguration);
    }

    private void FileExtension(object? sender, RoutedEventArgs e)
    {
        _configuration.KeepFileExtension = KeepFileExtension.IsChecked ?? false;
        _writableConfiguration.Value = _configuration;
    }

    private async void BrowseLoggingPathClick(object? sender, RoutedEventArgs e)
    {
        var selectedFolder = await SelectFolderAsync(_configuration.LoggingPath);
        if (!string.IsNullOrWhiteSpace(selectedFolder))
        {
            _configuration.LoggingPath = selectedFolder;
            _loggingPath = selectedFolder;
            _writableConfiguration.Value = _configuration;
        }

        Load();
    }

    private void LoggingPathOnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(LoggingPath.Text) && Directory.Exists(LoggingPath.Text))
        {
            _configuration.LoggingPath = LoggingPath.Text;
            _loggingPath = _configuration.LoggingPath;
            _writableConfiguration.Value = _configuration;
        }

        Load();
    }

    public async void GridOnDrop(object? sender, DragEventArgs e)
    {
        var droppedElements = e.DataTransfer.TryGetFiles();
        if (droppedElements is not null)
        {
            foreach (var droppedElement in droppedElements)
            {
                var droppedPath = droppedElement.Path.LocalPath;
                if (string.IsNullOrWhiteSpace(droppedPath))
                {
                    continue;
                }

                var isDirectory = droppedElement is IStorageFolder || Directory.Exists(droppedPath);
                if (!isDirectory && !File.Exists(droppedPath))
                {
                    continue;
                }

                _pathsToScan.TryAdd(droppedPath, isDirectory);
            }

            if (_pathsToScan.Count != 0)
            {
                await ConfigureCalculationAndRunAsync();
            }
        }

        e.Handled = true;
    }

    public void GridOnDragOver(object? sender, DragEventArgs e)
    {
        var isCorrect = true;
        var droppedElements = e.DataTransfer.TryGetFiles();
        if (droppedElements is null || !droppedElements.Any())
        {
            isCorrect = false;
        }
        else
        {
            foreach (var droppedElement in droppedElements)
            {
                var droppedPath = droppedElement.Path.LocalPath;
                var isDirectory = droppedElement is IStorageFolder || Directory.Exists(droppedPath);
                if (isDirectory && Directory.Exists(droppedPath))
                {
                    continue;
                }

                if (File.Exists(droppedPath))
                {
                    continue;
                }

                isCorrect = false;
                break;
            }
        }

        e.DragEffects = isCorrect ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void HashAlgorithmsOnCheckBoxChecked(object? sender, RoutedEventArgs e)
    {
        if (sender is CheckBox { DataContext: SelectableObject<HashAlgorithmModel> selectableObject } checkBox)
        {
            selectableObject.IsSelected = checkBox.IsChecked ?? false;
        }

        _configuration.HashTypes = [.. _observableCollection.Where(hashType => hashType.IsSelected).Select(hashType => hashType.ObjectData.Extension)];
        _writableConfiguration.Value = _configuration;
        SetHashAlgorithmsWatermark();
    }

    private void GetHashAlgorithms()
    {
        _observableCollection = [];
        var hashAlgorithmDictionary = _hashAlgorithmDictionary.Value;
        foreach (var selectableObject in hashAlgorithmDictionary
                                         .Select(item => new HashAlgorithmModel(item.Key, item.Value, _configuration.HashTypes.Contains(item.Value)))
                                         .Select(hashAlgorithmModel => new SelectableObject<HashAlgorithmModel>(hashAlgorithmModel, hashAlgorithmModel.IsSelected)))
        {
            _observableCollection.Add(selectableObject);
        }

        SetHashAlgorithmsWatermark();
    }

    private void SetHashAlgorithmsWatermark()
    {
        var hashAlgorithmDictionary = _hashAlgorithmDictionary.Value;
        var currentHashAlgorithms = _configuration.HashTypes.ToList();
        HashAlgorithmsWatermark.Text = string.Join(", ", (from item in hashAlgorithmDictionary
                                                          where currentHashAlgorithms.Contains(item.Value)
                                                          select item.Key).ToList());
    }

    private async void BrowseClick(object? sender, RoutedEventArgs e)
    {
        var selectedFolder = await SelectFolderAsync(_configuration.PathsToScan.FirstOrDefault());
        if (!string.IsNullOrWhiteSpace(selectedFolder))
        {
            _configuration.PathsToScan = [selectedFolder];
            _writableConfiguration.Value = _configuration;
        }

        Load();
    }

    private void InitialDirectoryOnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(InitialDirectory.Text) && Directory.Exists(InitialDirectory.Text))
        {
            _configuration.PathsToScan = [InitialDirectory.Text];
            _writableConfiguration.Value = _configuration;
        }

        Load();
    }

    private async Task<string?> SelectFolderAsync(string? selectedPath)
    {
        IStorageFolder? suggestedStartLocation = null;
        if (!string.IsNullOrWhiteSpace(selectedPath) && Directory.Exists(selectedPath))
        {
            suggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(selectedPath);
        }

        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                                                                  {
                                                                      AllowMultiple = false,
                                                                      SuggestedStartLocation = suggestedStartLocation
                                                                  });

        return folders.FirstOrDefault()?.Path.LocalPath;
    }

    private async Task ShowTaskDialogAsync(string title, string message, FASymbol symbol)
    {
        var taskDialog = new FATaskDialog
                         {
                             Title = title,
                             Content = message,
                             IconSource = new FASymbolIconSource { Symbol = symbol },
                             Buttons =
                             {
                                 FATaskDialogButton.OKButton
                             },
                             XamlRoot = this
                         };
        await taskDialog.ShowAsync();
    }
}