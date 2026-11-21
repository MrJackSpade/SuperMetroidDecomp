using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Which quarters of a square slope block are solid, for Samus, her shots and enemies alike (#1461, #1521).</summary>
public static class SquareSlopes
{
    /// <summary>Whether <paramref name="quadrant"/> of a block of <paramref name="shape"/> is solid.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The shape is undefined.</exception>
    public static bool IsSolid(SquareSlopeShape shape, SquareSlopeQuadrant quadrant) => shape switch
    {
        SquareSlopeShape.BottomHalf => quadrant.Bottom,
        SquareSlopeShape.RightHalf => quadrant.Right,
        SquareSlopeShape.BottomRightQuarter => quadrant.Bottom && quadrant.Right,
        SquareSlopeShape.AllButTopLeft => quadrant.Bottom || quadrant.Right,
        SquareSlopeShape.Full => true,
        _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, "Undefined square slope shape."),
    };
}
