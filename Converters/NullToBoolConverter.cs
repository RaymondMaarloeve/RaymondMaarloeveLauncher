using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace RaymondMaarloeveLauncher.Converters;

/// <summary>
/// Converts a null value to a boolean, with optional inversion.
/// </summary>
public class NullToBoolConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets a value indicating whether the result should be inverted.
    /// </summary>
    public bool Invert { get; set; } = false;

    /// <summary>
    /// Converts a value to a boolean indicating whether it is not null, with optional inversion.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>True if the value is not null (or false if Invert is true); otherwise, false (or true if Invert is true).</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool result = value != null;
        return Invert ? !result : result;
    }

    /// <summary>
    /// Not implemented. Throws a NotImplementedException if called.
    /// </summary>
    /// <param name="value">The value that is produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>Nothing. Always throws.</returns>
    /// <exception cref="NotImplementedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}