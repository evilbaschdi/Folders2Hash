using Avalonia.Media;
using EvilBaschdi.Core.Avalonia.Themes;
using FluentAvalonia.UI.Windowing;
using Folders2Hash.Internal;

namespace Folders2Hash;

internal partial class HashEvaluationDialog : FAAppWindow
{
    private readonly ICalculate _calculate;
    private string _hashFileContent;
    private string _sourceFileHash;
    private string _sourceFileName;
    private bool _windowShown;

    public HashEvaluationDialog()
    {
        IHashAlgorithmByName hashAlgorithmByName = new HashAlgorithmByName();
        _calculate = new Calculate(hashAlgorithmByName);
        _hashFileContent = string.Empty;
        _sourceFileHash = string.Empty;
        _sourceFileName = string.Empty;
        InitializeComponent();
        ThemeEngine.ApplyThemeToWindow(this, false);
        Opened += OnOpened;
    }

    public string HashFile { private get; init; } = string.Empty;

    public string HashType { private get; init; } = string.Empty;

    private async void OnOpened(object? sender, EventArgs e)
    {
        if (_windowShown)
        {
            return;
        }

        _windowShown = true;
        var isValid = await Task.Run(IsHashValid);
        TaskCompleted(isValid);
    }

    private bool IsHashValid()
    {
        if (HashFile.StartsWith("checksums", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        _sourceFileName = HashFile[..^(HashType.Length + 1)];
        _hashFileContent = File.ReadAllLines(HashFile).FirstOrDefault(line => !line.StartsWith('#'))?.Split(" *").FirstOrDefault() ?? string.Empty;

        if (!File.Exists(_sourceFileName))
        {
            return false;
        }

        var dictionary = new Dictionary<string, string>
                         {
                             { HashType, HashFile }
                         };
        var sourceFileHashes = _calculate.Hashes(_sourceFileName, dictionary);
        _sourceFileHash = sourceFileHashes.First(entry => entry.Key.Equals(HashType, StringComparison.CurrentCultureIgnoreCase)).Value;
        return _sourceFileHash.Trim().Equals(_hashFileContent.Trim(), StringComparison.InvariantCultureIgnoreCase);
    }

    private void TaskCompleted(bool isHashValid)
    {
        HashFileNameText.Text = $"Hash File: '{HashFile}'";
        SourceFileNameText.Text = $"Original Source File: '{_sourceFileName}'";
        HashFileContent.Text = _hashFileContent;
        SourceFileHash.Text = _sourceFileHash;

        if (isHashValid)
        {
            HashFileContent.Background = Brushes.GreenYellow;
            HashFileContent.Foreground = Brushes.Black;
            SourceFileHash.Background = Brushes.GreenYellow;
            SourceFileHash.Foreground = Brushes.Black;
            return;
        }

        HashFileContent.Background = Brushes.DarkRed;
        HashFileContent.Foreground = Brushes.White;
        SourceFileHash.Background = Brushes.DarkRed;
        SourceFileHash.Foreground = Brushes.White;
    }
}