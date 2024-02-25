﻿namespace Laminar.Avalonia.ToolbarPanel.AdjustableStackPanel;

public enum ResizerMode
{
    None,
    ArrowBefore,
    Default,
    ArrowAfter
}

public static class ResizerModeExtensions
{
    private static readonly ResizerMode[] _resizerModes = typeof(ResizerMode).GetEnumValues().Cast<ResizerMode>().Where(mode => mode != ResizerMode.None).ToArray();

    public static ResizerMode[] AllModes() => _resizerModes;

    public static (ResizeMethod methodBefore, ResizeMethod methodAfter)? GetResizeMethods(this ResizerMode mode) => mode switch
    {
        ResizerMode.ArrowBefore => (ResizeMethod.SqueezeExpand, ResizeMethod.Cascade),
        ResizerMode.Default => (ResizeMethod.Cascade, ResizeMethod.Cascade),
        ResizerMode.ArrowAfter => (ResizeMethod.Cascade, ResizeMethod.SqueezeExpand),
        _ => null,
    };

    public static bool IsAccessible(this ResizerMode mode, int indexInParent, int totalChildren, ResizeFlags currentFlags) => mode switch
    {
        ResizerMode.Default => true,
        ResizerMode.ArrowBefore => indexInParent >= 1,
        ResizerMode.ArrowAfter => indexInParent != totalChildren - 1 && !(currentFlags.HasFlag(ResizeFlags.CanConsumeSpaceAfterStack) && indexInParent == totalChildren - 2),
        _ => false
    };
}