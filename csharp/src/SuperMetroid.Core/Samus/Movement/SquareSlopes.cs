using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Which quarters of a square slope block are solid, for Samus, her shots and enemies alike (#1461, #1521).</summary>
public static partial class SquareSlopes
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

    /// <summary>The solid quarters of the square slope shape a slope block's behaviour selects (<see cref="SlopeShape.SquareQuarters"/>).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The behaviour selects a non-square slope shape.</exception>
    public static SquareSlopeShape ShapeOf(RoomBlockBehavior behavior) =>
        behavior.Slope.Shape.SquareQuarters ??
            throw new ArgumentOutOfRangeException(nameof(behavior), behavior.Slope.Shape, "The behaviour selects a non-square slope shape.");

    /// <summary>Whether the quarter of a square slope block that <paramref name="quadrant"/> names is solid.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The behaviour selects a non-square slope shape.</exception>
    public static bool IsSolid(RoomBlockBehavior behavior, SquareSlopeQuadrant quadrant) => IsSolid(ShapeOf(behavior), quadrant);
}
