using Avalonia;
using EvilBaschdi.About.Avalonia.DependencyInjection;
using EvilBaschdi.Core.Avalonia.AppBuilderImplementations;

namespace Folders2Hash;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => new AppBuilderImplementationToUseReactiveUIWithMicrosoftDependencyResolver<App>()
           .ValueFor(serviceCollection => { serviceCollection.AddAboutServices(); })
#if DEBUG
           .WithDeveloperTools()
#endif
    ;
}