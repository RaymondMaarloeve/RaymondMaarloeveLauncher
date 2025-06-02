using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using RaymondMaarloeveLauncher.ViewModels;
using RaymondMaarloeveLauncher.Views;

namespace RaymondMaarloeveLauncher;

/// <summary>
/// The main application class for RaymondMaarloeveLauncher.
/// Responsible for initializing Avalonia, setting up themes, and managing application lifetime events.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes Avalonia XAML resources for the application.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Called when the framework initialization is completed.
    /// Sets up the main window, applies the selected theme, and disables duplicate data annotation validation.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var theme = ActualThemeVariant ?? ThemeVariant.Light;

            ResourceDictionary themeDict = theme == ThemeVariant.Dark
                ? new Assets.Themes.DarkTheme()
                : new Assets.Themes.LightTheme();

            Resources.MergedDictionaries.Add(themeDict);
            
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Disables Avalonia's built-in data annotation validation to avoid duplicate validations with CommunityToolkit.
    /// </summary>
    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}