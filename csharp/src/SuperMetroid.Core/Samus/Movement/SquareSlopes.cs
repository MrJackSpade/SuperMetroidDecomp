using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Which quarters of a square slope block are solid, for Samus, her shots and enemies alike (#1461, #1521).</summary>
public static partial class SquareSlopes
{
    /// <summary>Whether a quadrant of a block of each square slope shape is solid.</summary>
    private static IdentityTable<SquareSlopeShape, Func<SquareSlopeQuadrant, bool>> SolidQuadrants => field ??= new(new Dictionary<SquareSlopeShape, Func<SquareSlopeQuadrant, bool>>
    {
        [SquareSlopeShape.BottomHalf] = static quadrant => quadrant.Bottom,
        [SquareSlopeShape.RightHalf] = static quadrant => quadrant.Right,
        [SquareSlopeShape.BottomRightQuarter] = static quadrant => quadrant.Bottom && quadrant.Right,
        [SquareSlopeShape.AllButTopLeft] = static quadrant => quadrant.Bottom || quadrant.Right,
        [SquareSlopeShape.Full] = static quadrant => true,
    });
    /// <summary>Whether <paramref name="quadrant"/> of a block of <paramref name="shape"/> is solid.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The shape is undefined.</exception>
    public static bool IsSolid(SquareSlopeShape shape, SquareSlopeQuadrant quadrant) => SolidQuadrants[shape](quadrant);
}
