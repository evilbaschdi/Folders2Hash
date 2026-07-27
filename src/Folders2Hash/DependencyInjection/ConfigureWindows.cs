using Microsoft.Extensions.DependencyInjection;

namespace Folders2Hash.DependencyInjection;

/// <summary>
///     Registers Window classes in the dependency injection container.
/// </summary>
public static class ConfigureWindows
{
    /// <summary>
    ///     Registers application windows.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static void AddWindows(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<MainWindow>();
        services.AddTransient<HashEvaluationDialog>();
    }
}