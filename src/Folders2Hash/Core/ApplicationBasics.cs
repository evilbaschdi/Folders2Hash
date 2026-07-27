namespace Folders2Hash.Core;

/// <inheritdoc />
public class ApplicationBasics : IApplicationBasics
{
    /// <inheritdoc />
    public void BrowseFolder()
        => throw new NotSupportedException("Use the Avalonia storage provider from the window.");

    /// <inheritdoc />
    public void BrowseLoggingFolder()
        => throw new NotSupportedException("Use the Avalonia storage provider from the window.");
}