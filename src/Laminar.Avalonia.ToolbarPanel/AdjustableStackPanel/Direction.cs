using Avalonia;
using ALayout = Avalonia.Layout;

namespace Laminar.Avalonia.ToolbarPanel.AdjustableStackPanel;

public enum Direction
{
    TopToBottom,
    LeftToRight,
    BottomToTop,
    RightToLeft,
}

public class InvalidDirectionException(Direction direction) : Exception
{
    public override string Message => $"Invalid direction provided: {direction}";

    public static InvalidDirectionException? FindInvalidDirection(params Direction[] directions)
    {
        foreach (Direction direction in directions)
        {
            switch (direction)
            {
                case Direction.TopToBottom | Direction.LeftToRight | Direction.BottomToTop | Direction.RightToLeft:
                    break;
                default:
                    return new InvalidDirectionException(direction);
            }
        }

        return null;
    }
}

public static class DirectionExtensions
{
    /// <summary>
    /// Gets the orientation of the direction, whether it's vertical or horizontal
    /// </summary>
    /// <param name="direction">The direction</param>
    /// <returns>The orientation of <paramref name="direction"/></returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="direction"/> is not a valid direction enum</exception>
    public static ALayout.Orientation Orientation(this Direction direction) => direction switch
    {
        Direction.TopToBottom => ALayout.Orientation.Vertical,
        Direction.LeftToRight => ALayout.Orientation.Horizontal,
        Direction.BottomToTop => ALayout.Orientation.Vertical,
        Direction.RightToLeft => ALayout.Orientation.Horizontal,
        _ => throw InvalidDirectionException.FindInvalidDirection(direction)!
    };

    /// <summary>
    /// <para>Orientates a rectangle to align with a specific direction, effectively rotating around the center of <paramref name="bounds"/> by an amount determined by the angle between <paramref name="from"/> and <paramref name="to"/></para>
    /// </summary>
    /// <param name="from">The current orientation of the rectangle</param>
    /// <param name="to">The desired orientation of the rectangle</param>
    /// <param name="rect">The rectangle that is transformed</param>
    /// <param name="bounds">The bounds that the rectangle is rotated within</param>
    /// <returns>The transformed <see cref="Rect"/></returns>
    public static Rect OrientateRectToDirection(this Direction from, Direction to, Rect rect, Size bounds)
    {
        rect /= new Vector(bounds.Width, bounds.Height);

        return (from, to) switch
        {
            (Direction.TopToBottom, Direction.TopToBottom) or (Direction.LeftToRight, Direction.LeftToRight) or (Direction.BottomToTop, Direction.BottomToTop) or (Direction.RightToLeft, Direction.RightToLeft) => rect,
            (Direction.TopToBottom, Direction.LeftToRight) or (Direction.LeftToRight, Direction.BottomToTop) or (Direction.BottomToTop, Direction.RightToLeft) or (Direction.RightToLeft, Direction.TopToBottom) => new Rect(rect.Top, 1 - rect.Right, rect.Height, rect.Width),
            (Direction.TopToBottom, Direction.BottomToTop) or (Direction.LeftToRight, Direction.RightToLeft) or (Direction.BottomToTop, Direction.TopToBottom) or (Direction.RightToLeft, Direction.LeftToRight) => new Rect(1 - rect.Right, 1 - rect.Bottom, rect.Width, rect.Height),
            (Direction.TopToBottom, Direction.RightToLeft) or (Direction.LeftToRight, Direction.TopToBottom) or (Direction.BottomToTop, Direction.LeftToRight) or (Direction.RightToLeft, Direction.BottomToTop) => new Rect(1 - rect.Bottom, rect.Left, rect.Height, rect.Width),
            _ => throw InvalidDirectionException.FindInvalidDirection(to, from)!
        } * new Vector(bounds.Width, bounds.Height);
    }
}
