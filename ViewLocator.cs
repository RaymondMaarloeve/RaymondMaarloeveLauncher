using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using RaymondMaarloeveLauncher.ViewModels;

namespace RaymondMaarloeveLauncher;

/// <summary>
/// Locates and instantiates views for corresponding view models by naming convention.
/// </summary>
public class ViewLocator : IDataTemplate
{
    /// <summary>
    /// Builds a view for the given view model parameter by replacing 'ViewModel' with 'View' in the type name.
    /// </summary>
    /// <param name="param">The view model instance.</param>
    /// <returns>The corresponding view as a Control, or a TextBlock if not found.</returns>
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    /// <summary>
    /// Determines whether the data object is a ViewModelBase and can be matched to a view.
    /// </summary>
    /// <param name="data">The data object to check.</param>
    /// <returns>True if the data is a ViewModelBase; otherwise, false.</returns>
    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}