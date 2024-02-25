using System;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Reactive;

namespace Laminar.Avalonia.ToolbarPanel;

public class ToolbarDecorator : Decorator
{
    public static readonly AttachedProperty<Dock> LocationProperty = AvaloniaProperty.RegisterAttached<ToolbarDecorator, AvaloniaObject, Dock>("Location", Dock.Top);
    public static Dock GetLocation(AvaloniaObject obj) => obj.GetValue(LocationProperty);
    public static void SetLocation(AvaloniaObject obj, Dock tl) => obj.SetValue(LocationProperty, tl);
    
    public static readonly AttachedProperty<int> LevelProperty = AvaloniaProperty.RegisterAttached<ToolbarDecorator, AvaloniaObject, int>("Level");
    public static int GetLevel(AvaloniaObject obj) => obj.GetValue(LevelProperty);
    public static void SetLevel(AvaloniaObject obj, int level) => obj.SetValue(LevelProperty, level);

    public static readonly AttachedProperty<Func<Control, Control>> CloneFunctionProperty = AvaloniaProperty.RegisterAttached<ToolbarDecorator, AvaloniaObject, Func<Control, Control>>("CloneFunction");
    public static Func<Control, Control> GetCloneFunction(AvaloniaObject obj) => obj.GetValue(CloneFunctionProperty);
    public static void SetCloneFunction(AvaloniaObject obj, Func<Control, Control> cloneFunc) => obj.SetValue(CloneFunctionProperty, cloneFunc);

    public static readonly AttachedProperty<Orientation> OrientationProperty = AvaloniaProperty.RegisterAttached<ToolbarDecorator, AvaloniaObject, Orientation>("Orientation", inherits: true, defaultBindingMode: BindingMode.OneWay);
    public static Orientation GetOrientation(AvaloniaObject obj) => obj.GetValue(OrientationProperty);
    public static void SetOrientation(AvaloniaObject obj, Orientation orientation) => throw new InvalidOperationException("Orientation is a read only attached property");

    public Controls Toolbars { get; } = new();

    private readonly Dictionary<Control, Toolbar> _toolbarsDictionary = new();
    private readonly ToolbarPanel _toolbarPanel = new();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (AdornerLayer.GetAdornerLayer(this) is AdornerLayer adornerLayer)
        {
            adornerLayer.Children.Add(_toolbarPanel);
        }

        base.OnAttachedToVisualTree(e);
    }

    public ToolbarDecorator()
    {
        Toolbars.CollectionChanged += ToolbarsCollectionChanged;
        _toolbarPanel.GetObservable(ToolbarPanel.ToolbarThicknessProperty).Subscribe(new AnonymousObserver<Thickness>(_ => { InvalidateMeasure(); }));
    }

    private void ToolbarsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Move)
        {
            return;
        }

        if (e.OldItems is not null)
        {
            foreach (object item in e.OldItems)
            {
                if (item is Control removedToolbar)
                {
                    ToolbarRemoved(removedToolbar);
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (object item in e.NewItems)
            {
                if (item is Control newToolbar)
                {
                    ToolbarAdded(newToolbar);
                }
            }
        }
    }

    private void ToolbarAdded(Control newToolbar)
    {
        Toolbar newToolbarControl = new()
        {
            ChildControl = newToolbar
        };

        _toolbarPanel.AddToolbar(newToolbarControl);
        _toolbarsDictionary.Add(newToolbar, newToolbarControl);
    }

    private void ToolbarRemoved(Control removedToolbar)
    {
        _toolbarPanel.RemoveToolbar(_toolbarsDictionary[removedToolbar]);
        _toolbarsDictionary.Remove(removedToolbar);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return LayoutHelper.MeasureChild(Child, availableSize, Padding, _toolbarPanel.ToolbarThickness);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return LayoutHelper.ArrangeChild(Child, finalSize, Padding, _toolbarPanel.ToolbarThickness);
    }
}
