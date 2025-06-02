using Avalonia;
using System;
using Avalonia.ReactiveUI;

namespace RaymondMaarloeveLauncher;

/// <summary>
/// The main entry point for the RaymondMaarloeveLauncher application.
/// Responsible for application startup and Avalonia configuration.
/// </summary>
sealed class Program
{
    /// <summary>
    /// Application initialization code. Do not use any Avalonia, third-party APIs, or SynchronizationContext-reliant code before AppMain is called.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    /// <summary>
    /// Configures Avalonia for the application. Do not remove; also used by the visual designer.
    /// </summary>
    /// <returns>The configured AppBuilder instance.</returns>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();

}