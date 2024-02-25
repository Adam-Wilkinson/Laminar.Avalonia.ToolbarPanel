using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Styling;

namespace Laminar.Avalonia.ToolbarPanel;

[TemplatePart("PART_MoveWidget", typeof(Control))]
public class Toolbar : TemplatedControl
{
    public static readonly StyledProperty<double> DepthProperty = AvaloniaProperty.Register<Toolbar, double>(nameof(Depth));

    public static readonly StyledProperty<Control> ChildControlProperty = AvaloniaProperty.Register<Toolbar, Control>(nameof(ChildControl));

    public static readonly DirectProperty<Toolbar, Orientation> OrientationProperty = AvaloniaProperty.RegisterDirect<Toolbar, Orientation>(nameof(Orientation), o => o.Orientation, (o, e) => o.Orientation = e);

    public static readonly DirectProperty<Toolbar, double> SizeWeightingProperty = AvaloniaProperty.RegisterDirect<Toolbar, double>(nameof(SizeWeight), o => o.SizeWeight, (o, e) => o.SizeWeight = e);

    public static readonly StyledProperty<Dock> MoveWidgetLocationProperty = AvaloniaProperty.Register<Toolbar, Dock>(nameof(MoveWidgetLocation));

    private Orientation _orientation = Orientation.Horizontal;
    private double _sizeWeight = 1.0;



    public Toolbar()
    {
        this[!OrientationProperty] = LocationBindingWithConverter<Orientation?>(dock => dock switch
        {
            Dock.Left or Dock.Right => Orientation.Vertical,
            Dock.Top or Dock.Bottom => Orientation.Horizontal,
            _ => null,
        });

        this[!MoveWidgetLocationProperty] = LocationBindingWithConverter<Dock?>(dock => dock switch
        {
            Dock.Left or Dock.Right => Dock.Top,
            Dock.Bottom or Dock.Top => Dock.Left,
            _ => null,
        });
    }

    public event EventHandler<PointerPressedEventArgs>? MoveInitiated;

    public double Depth
    {
        get => GetValue(DepthProperty);
        set => SetValue(DepthProperty, value);
    }

    public double SizeWeight
    {
        get => _sizeWeight;
        set => SetAndRaise(SizeWeightingProperty, ref _sizeWeight, value);
    }

    public Orientation Orientation
    {
        get => _orientation;
        set => SetAndRaise(OrientationProperty, ref _orientation, value);
    }

    public Control ChildControl
    {
        get => GetValue(ChildControlProperty);
        set => SetValue(ChildControlProperty, value);
    }

    public Dock MoveWidgetLocation
    {
        get => GetValue(MoveWidgetLocationProperty);
        set => SetValue(MoveWidgetLocationProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        Control moveWidget = e.NameScope.Find<Control>("PART_MoveWidget");

        if (moveWidget is not null)
        {
            moveWidget.PointerPressed += MoveWidget_PointerPressed;
        }
    }

    private void MoveWidget_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        MoveInitiated?.Invoke(this, e);
    }

    private Binding LocationBindingWithConverter<T>(Func<Dock, T> mapper) => new("ChildControl.(t:ToolbarDecorator.Location)")
    {
        Source = this,
        Converter = new FuncValueConverter<Dock, T>(mapper),
        TypeResolver = (s1, s2) => typeof(ToolbarDecorator)
    };
}
