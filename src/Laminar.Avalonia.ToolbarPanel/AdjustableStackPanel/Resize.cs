namespace Laminar.Avalonia.ToolbarPanel.AdjustableStackPanel;

public readonly record struct Resize(int IndexOffset, ResizeAmountTransformation ResizeAmountTransformation, ResizerMode ResizerMode)
{
    public readonly double Execute<T>(IList<T> resizeElements, IResizingHarness<T> resizeHarness, double resizeAmount, double spaceToExpandInto, int indexOfCurrentResize, int activeResizerIndex, Span<double> spaceBeforeResizers, ResizeFlags flags)
    {
        double spaceBeforeResizer = indexOfCurrentResize < 0 ? 0 : spaceBeforeResizers[indexOfCurrentResize];
        double spaceAfterResizer = spaceBeforeResizers[^1] - spaceBeforeResizer;

        double currentResizeAmount = ResizeAmountTransformation(resizeAmount, activeResizerIndex < 0 ? 0 : spaceBeforeResizers[activeResizerIndex], spaceBeforeResizer, spaceBeforeResizers[^1]);

        if (ResizerMode.GetResizeMethods() is not (ResizeMethod methodBeforeResizer, ResizeMethod methodAfterResizer))
        {
            return 0;
        }

        if (indexOfCurrentResize == resizeElements.Count - 1)
        {
            methodAfterResizer = ResizeMethod.ChangeStackSize;
        }

        if (indexOfCurrentResize == -1)
        {
            methodBeforeResizer = ResizeMethod.ChangeStackSize;
        }

        Span<ResizeMethod> methodsAfterResizer = flags.HasFlag(ResizeFlags.CanConsumeSpaceAfterStack) ? [ResizeMethod.ChangeStackSize, methodAfterResizer] : [methodAfterResizer];

        Span<ResizeMethod> methodsBeforeResizer = flags.HasFlag(ResizeFlags.CanConsumeSpaceBeforeStack) ? [ResizeMethod.ChangeStackSize, methodBeforeResizer] : [methodBeforeResizer];

        ListSlice<T> elementsBeforeResizer = resizeElements.CreateBackwardsSlice(indexOfCurrentResize);
        ListSlice<T> elementsAfterResizer = resizeElements.CreateForwardsSlice(indexOfCurrentResize + 1);

        if (currentResizeAmount > 0)
        {
            double successfulResizeAmount = -methodsAfterResizer.RunMethods(elementsAfterResizer, resizeHarness, -currentResizeAmount, spaceAfterResizer, spaceToExpandInto);
            return methodsBeforeResizer.RunMethods(elementsBeforeResizer, resizeHarness, successfulResizeAmount, spaceBeforeResizer, spaceToExpandInto);
        }
        else if (currentResizeAmount < 0)
        {
            double successfulResizeAmount = methodsBeforeResizer.RunMethods(elementsBeforeResizer, resizeHarness, currentResizeAmount, spaceBeforeResizer, spaceToExpandInto);
            return methodsAfterResizer.RunMethods(elementsAfterResizer, resizeHarness, -successfulResizeAmount, spaceAfterResizer, spaceToExpandInto);
        }

        return 0;
    }
}