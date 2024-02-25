using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Reactive;
using Laminar.Avalonia.ToolbarPanel.AdjustableStackPanel;

namespace Laminar.Avalonia.ToolbarPanel;

internal record class ToolbarDragContext(Point StartLocation, Vector ClickOffset, Toolbar Toolbar)
{
    public Point CurrentLocation { get; set; } = StartLocation;
}

public class ToolbarPanel : Panel
{
    public static readonly DirectProperty<ToolbarPanel, Thickness> ToolbarThicknessProperty = AvaloniaProperty.RegisterDirect<ToolbarPanel, Thickness>(nameof(ToolbarThickness), t => t.ToolbarThickness);

    private readonly Dictionary<Toolbar, List<IDisposable>> _toolbarObservables = new();

    private ToolbarDragContext? _dragContext;

    private readonly AdjustableStackPanel.AdjustableStackPanel _topDock = new() { Orientation = Orientation.Vertical, VerticalAlignment = VerticalAlignment.Top };
    private readonly AdjustableStackPanel.AdjustableStackPanel _leftDock = new() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Left };
    private readonly AdjustableStackPanel.AdjustableStackPanel _rightDock = new() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
    private readonly AdjustableStackPanel.AdjustableStackPanel _bottomDock = new() { Orientation = Orientation.Vertical, VerticalAlignment = VerticalAlignment.Bottom };

    private Thickness _toolbarThickess;
    public Thickness ToolbarThickness 
    { 
        get => _toolbarThickess;
        private set => SetAndRaise(ToolbarThicknessProperty, ref _toolbarThickess, value);
    }

    public void AddToolbar(Toolbar toolbar)
    {
        _toolbarObservables.Add(toolbar, new List<IDisposable>
        {
            toolbar.ChildControl.GetObservable(ToolbarDecorator.LocationProperty).Subscribe(new AnonymousObserver<Dock>(x =>
            {
                FloatToolbar(toolbar);
                DockToolbar(toolbar);
            })),

            toolbar.ChildControl.GetObservable(ToolbarDecorator.LevelProperty).Subscribe(new AnonymousObserver<int>(x =>
            {
                FloatToolbar(toolbar);
                DockToolbar(toolbar);
            })),
        });

        toolbar.MoveInitiated += InitiateMove;

        Children.Add(toolbar);
    }

    public void RemoveToolbar(Toolbar toolbar)
    {
        toolbar.MoveInitiated -= InitiateMove;
        Children.Remove(toolbar);
        _toolbarObservables[toolbar].ForEach(x => x.Dispose());
        _toolbarObservables.Remove(toolbar);
    }

    public void InitiateMove(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Toolbar toolbarSender)
        {
            return;
        }

        FloatToolbar(toolbarSender);
        _dragContext = new(e.GetPosition(this), e.GetPosition(this) - toolbarSender.Bounds.TopLeft, toolbarSender);
    }

    public void FloatToolbar(Toolbar toolbar)
    {
        GetDockAt(ToolbarDecorator.GetLocation(toolbar)).Children.Add(toolbar);
    }

    public void DockToolbar(Toolbar toolbar)
    {
        GetDockAt(ToolbarDecorator.GetLocation(toolbar)).Children.Add(toolbar);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Rect arrangeRect = new(finalSize);

        _topDock.Arrange(arrangeRect);
        _bottomDock.Arrange(arrangeRect);
        _leftDock.Arrange(arrangeRect);
        _rightDock.Arrange(arrangeRect);

        _dragContext?.Toolbar.Arrange(new Rect(_dragContext.CurrentLocation - _dragContext.ClickOffset, _dragContext.Toolbar.DesiredSize));

        ToolbarThickness = new Thickness(_leftDock.Width, _topDock.Height, _rightDock.Width, _bottomDock.Height);

        return finalSize;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return base.MeasureOverride(availableSize);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_dragContext is null)
        {
            return;
        }

        _dragContext.CurrentLocation = e.GetPosition(this);
        InvalidateArrange();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _dragContext = null;
    }

    private AdjustableStackPanel.AdjustableStackPanel GetDockAt(Dock dock) => dock switch
    {
        Dock.Top => _topDock,
        Dock.Bottom => _bottomDock,
        Dock.Left => _leftDock,
        Dock.Right => _rightDock,
        _ => throw new ArgumentException($"Invalid dock location {dock}", nameof(dock)),
    };
}