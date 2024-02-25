using System.Collections.Frozen;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.VisualTree;

namespace Laminar.Avalonia.ToolbarPanel.AdjustableStackPanel;

[TemplatePart("PART_Move", typeof(Control))]
[TemplatePart("PART_Shrink", typeof(Control))]
[TemplatePart("PART_Grow", typeof(Control))]
public class ResizeWidget : TemplatedControl
{
    public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<ResizeWidget, Orientation>(nameof(Orientation));

    public static readonly DirectProperty<ResizeWidget, double> SizeProperty = AvaloniaProperty.RegisterDirect<ResizeWidget, double>(nameof(Size), r => r._size, (r, v) => r._size = v);

    public static readonly DirectProperty<ResizeWidget, ResizerMode> ModeProperty = AvaloniaProperty.RegisterDirect<ResizeWidget, ResizerMode>(nameof(Mode), r => r._mode, (r, v) => r._mode = v);

    public static readonly AttachedProperty<ResizeWidget?> ResizeWidgetProperty = AvaloniaProperty.RegisterAttached<ToolbarDock, Control, ResizeWidget?>("ResizeWidget");
    public static ResizeWidget? GetResizeWidget(Control control) => control.GetValue(ResizeWidgetProperty);
    public static void SetResizeWidget(Control control, ResizeWidget? resizeWidget) => control.SetValue(ResizeWidgetProperty, resizeWidget);

    private static readonly FrozenDictionary<ResizerMode, string> ModePseudoClasses = ResizerModeExtensions.AllModes().ToFrozenDictionary(x => x, x => ":" + x.ToString());
    private ResizerMode _mode;
    private double _size;
    private Point? _lastMousePoint = null;

    static ResizeWidget()
    {
        // Load the custom styles manually
        StyleInclude newStyles = new(new Uri("resm:Styles?assembly=Laminar.Avalonia.ToolbarPanel"))
        {
            Source = new Uri("avares://Laminar.Avalonia.ToolbarPanel/ToolbarStyles.axaml"),
        };
        Application.Current!.Styles.Add(newStyles);
    }

    public Func<ResizerMode, bool>? ModeAccessibleCheck = null;

    public double Size
    {
        get => _size;
        set => SetAndRaise(SizeProperty, ref _size, value);

    }

    public ResizerMode Mode
    {
        get => _mode;
        set => SetAndRaise(ModeProperty, ref _mode, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        _lastMousePoint = e.GetPosition(this.GetVisualParent());
        e.Handled = true;
        base.OnPointerPressed(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterModeSwitchOnChildHover(e, "PART_ResizeZoneBefore", ResizerMode.ArrowBefore);
        RegisterModeSwitchOnChildHover(e, "PART_ResizeZoneAfter", ResizerMode.ArrowAfter);
        RegisterModeSwitchOnChildHover(e, "PART_DefaultResizeZone", ResizerMode.Default);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        ShowAccessibleModes();
        base.OnPointerEntered(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        HideAccessibleModes();
        Mode = ResizerMode.None;
        base.OnPointerExited(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!_lastMousePoint.HasValue)
        {
            base.OnPointerMoved(e);
            return;
        }

        Point currentMousePoint = e.GetPosition(this.GetVisualParent());

        Point deltaXY = currentMousePoint - _lastMousePoint.Value;
        Size = Orientation switch
        {
            Orientation.Vertical => Size + deltaXY.Y,
            Orientation.Horizontal => Size + deltaXY.X,
            _ => throw new InvalidOperationException()
        };

        _lastMousePoint = currentMousePoint;
        e.Handled = true;
        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (_lastMousePoint is null)
        {
            base.OnPointerReleased(e);
            return;
        }

        _lastMousePoint = null;

        e.Handled = true;
        base.OnPointerReleased(e);
    }

    public void ShowAccessibleModes()
    {
        foreach (ResizerMode mode in GetAccessibleModes())
        {
            PseudoClasses.Add(ModePseudoClasses[mode]);
        }
    }

    public void HideAccessibleModes()
    {
        foreach (ResizerMode mode in ResizerModeExtensions.AllModes())
        {
            PseudoClasses.Remove(ModePseudoClasses[mode]);
        }
    }

    public static ResizeWidget GetOrCreateResizer(Control control)
    {
        if (GetResizeWidget(control) is ResizeWidget widget)
        {
            return widget;
        }

        ResizeWidget newResizer = new();
        SetResizeWidget(control, newResizer);
        return newResizer;
    }

    private void RegisterModeSwitchOnChildHover(TemplateAppliedEventArgs e, string childName, ResizerMode mode)
    {
        Control? child = e.NameScope.Find<Control>(childName);

        if (child is not null)
        {
            child.PointerEntered += (_, _) =>
            {
                if (ModeAccessibleCheck is null || ModeAccessibleCheck(mode))
                {
                    Mode = mode;
                }
            };
        }
    }

    private IEnumerable<ResizerMode> GetAccessibleModes() => ModeAccessibleCheck is null ? ResizerModeExtensions.AllModes() : ResizerModeExtensions.AllModes().Where(mode => ModeAccessibleCheck(mode));
}
