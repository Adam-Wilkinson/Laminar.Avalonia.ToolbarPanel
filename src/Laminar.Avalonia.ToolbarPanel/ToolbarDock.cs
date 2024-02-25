using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Laminar.Avalonia.ToolbarPanel.AdjustableStackPanel;

namespace Laminar.Avalonia.ToolbarPanel;

public class ToolbarDock : StackPanel
{
    public static readonly AttachedProperty<ResizeWidget> LayerResizeWidgetProperty = AvaloniaProperty.RegisterAttached<ToolbarDock, Control, ResizeWidget>("ControlResize");
    public static ResizeWidget GetLayerResizeWidget(Control control) => control.GetValue(LayerResizeWidgetProperty);
    public static void SetLayerResizeWidget(Control control, ResizeWidget resizeWidget) => control.SetValue(LayerResizeWidgetProperty, resizeWidget);

    readonly Dock _location;
    readonly Orientation _orientation;

    public ToolbarDock(Dock location)
    {
        _location = location;
        _orientation = location switch
        {
            Dock.Left or Dock.Right => Orientation.Vertical,
            Dock.Top or Dock.Bottom => Orientation.Horizontal,
            _ => default
        };
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var children = Children;
        bool fHorizontal = (Orientation == Orientation.Horizontal);
        Rect rcChild = new Rect(finalSize);
        double previousChildSize = 0.0;
        var spacing = Spacing;

        //
        // Arrange and Position Children.
        //
        for (int i = 0, count = children.Count; i < count; ++i)
        {
            var child = children[i];

            if (child == null || !child.IsVisible)
            { continue; }

            ResizeWidget layerResizer = GetOrCreateResizer(child);

            if (fHorizontal)
            {
                rcChild = rcChild.WithX(rcChild.X + previousChildSize);
                previousChildSize = layerResizer.Size;
                rcChild = rcChild.WithWidth(previousChildSize);
                rcChild = rcChild.WithHeight(Math.Max(finalSize.Height, child.DesiredSize.Height));
                previousChildSize += spacing;
            }
            else
            {
                rcChild = rcChild.WithY(rcChild.Y + previousChildSize);
                previousChildSize = layerResizer.Size;
                rcChild = rcChild.WithHeight(previousChildSize);
                rcChild = rcChild.WithWidth(Math.Max(finalSize.Width, child.DesiredSize.Width));
                previousChildSize += spacing;
            }

            child.Arrange(rcChild);
        }

        return finalSize;
    }

    private ResizeWidget GetOrCreateResizer(Control child)
    {
        if (GetLayerResizeWidget(child) is ResizeWidget widget)
        {
            return widget;
        }

        ResizeWidget newResizer = new();
        SetLayerResizeWidget(child, newResizer);
        return newResizer;
    }

    public void AddToolbar(Toolbar toolbar)
    {
        FindLayerAtLevel(ToolbarDecorator.GetLevel(toolbar)).Children.Add(toolbar);
    }

    public void RemoveToolbar(Toolbar toolbar)
    {
        FindLayerAtLevel(ToolbarDecorator.GetLevel(toolbar)).Children.Remove(toolbar);
    }

    private void ArrangeChild(Layoutable child, Rect localPosition, Size availableSize)
    {
        child.Arrange(ConvertDockToPanelCoords(localPosition, availableSize));
    }

    private Rect ConvertDockToPanelCoords(Rect rect, Size availableSize) => _location switch
    {
        Dock.Left => new Rect(rect.Top, rect.Left, rect.Height, rect.Width),
        Dock.Bottom => new Rect(rect.Left, availableSize.Height - rect.Bottom, rect.Width, rect.Height),
        Dock.Right => new Rect(availableSize.Width - rect.Bottom, rect.Left, rect.Height, rect.Width),
        Dock.Top => rect,
        _ => throw new NotImplementedException(),
    };

    private ToolbarDockLayer FindLayerAtLevel(int level)
    {
        int index = 0;
        foreach (Control child in Children)
        {
            if (child is not ToolbarDockLayer layer)
            {
                continue;
            }

            if (ToolbarDecorator.GetLevel(child) == level)
            {
                return layer;
            }

            if (ToolbarDecorator.GetLevel(child) > level)
            {
                ToolbarDockLayer newLayer = new() { Level = level };
                Children.Insert(index, newLayer);
                return newLayer;
            }

            index++;
        }

        ToolbarDockLayer newLayerAtEnd = new() { Level = level };
        Children.Add(newLayerAtEnd);
        return newLayerAtEnd;
    }
}