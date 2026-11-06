using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>One quarter of a square slope block, in the slope shape's own unmirrored frame: its column half and row half.</summary>
public readonly record struct SquareSlopeQuadrant
{
    /// <summary>Names a quarter by its halves.</summary>
    /// <param name="right">Whether it is in the right column half.</param>
    /// <param name="bottom">Whether it is in the bottom row half.</param>
    public SquareSlopeQuadrant(bool right, bool bottom)
    {
        Right = right;
        Bottom = bottom;
    }

    /// <summary>Whether the quarter is in the shape's right column half; otherwise the left.</summary>
    public bool Right { get; }

    /// <summary>Whether the quarter is in the shape's bottom row half; otherwise the top.</summary>
    public bool Bottom { get; }

    /// <summary>The quarter beside this one in the same row.</summary>
    public SquareSlopeQuadrant OtherColumn => new(!Right, Bottom);

    /// <summary>The quarter above or below this one in the same column.</summary>
    public SquareSlopeQuadrant OtherRow => new(Right, !Bottom);

    /// <summary>
    /// The quarter of <paramref name="behavior"/>'s slope shape that a block's halves show: the block's horizontal and
    /// vertical reflections swap the halves they mirror.
    /// </summary>
    /// <param name="behavior">The square slope block's behaviour, which holds its reflections.</param>
    /// <param name="rightHalf">Whether the point lies in the block's right half.</param>
    /// <param name="bottomHalf">Whether the point lies in the block's bottom half.</param>
    public static SquareSlopeQuadrant Showing(RoomBlockBehavior behavior, bool rightHalf, bool bottomHalf) =>
        new(rightHalf != behavior.SlopeFlipsHorizontally, bottomHalf != behavior.SlopeFlipsVertically);
}
