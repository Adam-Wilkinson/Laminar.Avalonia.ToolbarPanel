using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Laminar.Avalonia.ToolbarPanel.AdjustableStackPanel;

namespace Laminar.Avalonia.ToolbarPanel;

public class ToolbarDockLayer : StackPanel
{
    public required int Level { get; init; }

    public ResizeWidget ResizeWidget { get; } = new();

    protected override Size ArrangeOverride(Size finalSize)
    {
        Controls children = base.Children;
        Rect rect = new(finalSize);
        double num = 0.0;
        double spacing = Spacing;
        double totalWeighting = CalculateTotalWeighting();
        int i = 0;
        for (int count = children.Count; i < count; i++)
        {
            Control control = children[i];
            double currentControlWeighting = FindWeight(control);
            if (control != null && control.IsVisible)
            {
                switch (Orientation)
                {
                    case Orientation.Horizontal:
                        rect = rect.WithX(rect.X + num);
                        num = Math.Max(control.DesiredSize.Width, finalSize.Width * currentControlWeighting / totalWeighting);
                        rect = rect.WithWidth(num).WithHeight(Math.Max(finalSize.Height, control.DesiredSize.Height));
                        num += spacing;
                        break;
                    case Orientation.Vertical:
                        rect = rect.WithY(rect.Y + num);
                        num = Math.Max(control.DesiredSize.Height, finalSize.Height * currentControlWeighting / totalWeighting);
                        rect = rect.WithHeight(num).WithWidth(Math.Max(finalSize.Width, control.DesiredSize.Width));
                        num += spacing;
                        break;
                }
                control.Arrange(rect);
            }
        }

        return finalSize;
    }

    public double CalculateDepth()
    {
        double depth = 0.0;

        foreach (Control child in Children)
        {
            if (child is not Toolbar toolbar) { continue; }

            depth = Math.Max(depth, Orientation == Orientation.Horizontal ? toolbar.DesiredSize.Height : toolbar.DesiredSize.Width);
        }

        return depth;
    }

    public double CalculateTotalWeighting()
    {
        double totalWeighting = 0.0;

        foreach (Control child in Children)
        {
            totalWeighting += FindWeight(child);
        }

        return totalWeighting;
    }

    double FindWeight(Control control)
    {
        if (control is Toolbar toolbar)
        {
            return toolbar.SizeWeight;
        }

        return 1.0;
    }
}
