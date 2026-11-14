using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Which quarters of a square slope block are solid, for Samus, her shots and enemies alike (#1461, #1521).</summary>
public static class SquareSlopes
{
    /// <summary>The solid quarters of each square slope shape.</summary>
    private static readonly Dictionary<SlopeShape, SquareSlopeShape> Quarters = new()
    {
        [SlopeShapeDefinitions.SquareBottomHalf] = SquareSlopeShape.BottomHalf,
        [SlopeShapeDefinitions.SquareRightHalf] = SquareSlopeShape.RightHalf,
        [SlopeShapeDefinitions.SquareBottomRightQuarter] = SquareSlopeShape.BottomRightQuarter,
        [SlopeShapeDefinitions.SquareAllButTopLeft] = SquareSlopeShape.AllButTopLeft,
        [SlopeShapeDefinitions.SquareFull] = SquareSlopeShape.Full,
    };

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

    /// <summary>The square slope shape a slope block's behaviour selects.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The behaviour selects a non-square slope shape.</exception>
    public static SquareSlopeShape ShapeOf(RoomBlockBehavior behavior) =>
        Quarters.TryGetValue(behavior.Slope.Shape, out SquareSlopeShape quarters)
            ? quarters
            : throw new ArgumentOutOfRangeException(nameof(behavior), behavior.Slope.Shape, "The behaviour selects a non-square slope shape.");

    /// <summary>Whether the quarter of a square slope block that <paramref name="quadrant"/> names is solid.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The behaviour selects a non-square slope shape.</exception>
    public static bool IsSolid(RoomBlockBehavior behavior, SquareSlopeQuadrant quadrant) => IsSolid(ShapeOf(behavior), quadrant);
}
