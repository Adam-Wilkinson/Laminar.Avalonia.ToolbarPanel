using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;

namespace Laminar.Avalonia.ToolbarPanel.Converters;

public class FlipOrientationConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => (value as Orientation?) switch
    {
        Orientation.Vertical => Orientation.Horizontal,
        Orientation.Horizontal => Orientation.Vertical,
        _ => null
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Convert(value, targetType, parameter, culture);
}
