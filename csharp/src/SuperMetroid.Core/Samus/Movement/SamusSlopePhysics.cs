using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Primitives used by Samus's non-square slope collision path.
/// </summary>
/// <remarks>
/// Super Metroid does not reduce slopes to a line equation. behaviour bits select one of 32
/// sixteen-sample height profiles, while a separate table scales grounded horizontal
/// displacement. Compiled profiles and multipliers preserve the exact discrete geometry
/// and integer movement calculations without reading game tables at runtime.
/// </remarks>
public static class SamusSlopePhysics
{
    /// <summary>
    /// Reacts to a non-square slope during horizontal block collision.
    /// </summary>
    /// <param name="behavior">
    /// The slope block's behaviour: its profile and whether it is a ceiling slope.
    /// </param>
    /// <param name="displacement">The horizontal displacement.</param>
    /// <param name="verticalSpeed">Samus's vertical speed. Any motion suppresses grounded slope scaling exactly as the game does.</param>
    public static SignedSixteenSixteen ScaleGroundedHorizontalDisplacement(
        RoomBlockBehavior behavior,
        SignedSixteenSixteen displacement,
        UnsignedSixteenSixteen verticalSpeed)
    {

        // Ceiling slopes and airborne Samus leave the incoming amount untouched. This is
        // the game's complete early-return condition, not an invented policy choice.
        if (behavior.Slope.FlipsVertically || !verticalSpeed.IsZero)
            return displacement;

        int multiplier = behavior.Slope.Shape.GroundedSpeedShare;

        // The lowest eight fractional bits are discarded before scaling, rounding toward negative
        // infinity, so a leftward displacement scales by the same rule as a rightward one.
        return displacement.ScaledByTwoFiftySixths(multiplier);
    }

    /// <summary>
    /// The slope's height under the pixel column <paramref name="xPosition"/>: its profile read at the column's offset
    /// within its block, or, for a horizontally flipped slope, at the offset measured from the block's far edge.
    /// </summary>
    public static int ReadAlignmentHeight(
        RoomBlockBehavior behavior,
        int xPosition)
    {
        int column = behavior.Slope.FlipsHorizontally
            ? RoomGridConversions.OffsetFromBlockEnd(xPosition)
            : RoomGridConversions.OffsetWithinBlock(xPosition);
        return behavior.Slope.Shape.HeightAt(column);
    }

    /// <summary>
    /// Aligns Samus's Y position to a slope: the post-horizontal correction
    /// for non-square floor and ceiling slopes.
    /// </summary>
    /// <remarks>
    /// This alignment does not perform the preceding horizontal collision scan. It consumes
    /// the position that scan accepted and adjusts only whole-pixel Y, exactly like the
    /// game's call immediately after a move without solid-enemy collision.
    /// </remarks>
    public static SlopeAlignmentResult AlignYPosition(
        RoomLevelData level,
        Position16<RoomPixels> position,
        int yRadius,
        bool horizontalSlopeCollisionEnabled = true)
    {
        Ensure.NotNull(level);
        int xPosition = position.X;
        int yPosition = position.Y;

        if (!horizontalSlopeCollisionEnabled)
            return new SlopeAlignmentResult(position, Adjusted: false);

        bool adjusted = false;

        // First probes the center-X block containing Samus's bottom pixel. Only
        // slope blocks, shapes >=5, and the behaviour's ceiling bit clear participate as floor slopes.
        int bottomPosition = CoordinateAxis.Wrap(yPosition + yRadius - 1);
        if (TryGetBlockAtPixel(level, Position16<RoomPixels>.Of(xPosition, bottomPosition), out RoomCollisionBlock bottom) &&
            bottom.CollisionType == RoomCollisionType.Slope &&
            bottom.Behavior.Slope.IsNonSquare)
        {
            if (!bottom.Behavior.Slope.FlipsVertically)
            {
                int height = ReadAlignmentHeight(bottom.Behavior, xPosition);
                int bottomDepth = RoomGridConversions.OffsetWithinBlock(bottomPosition);
                int correction = CoordinateAxis.Difference(height - bottomDepth, 1);
                if (correction < 0)
                {
                    yPosition = CoordinateAxis.Wrap(yPosition + correction);
                    adjusted = true;
                }
            }
        }

        // Recompute the top probe after any floor correction. Ceiling slopes use behaviour bit
        // the ceiling bit set and reverse the same signed correction.
        int topPosition = CoordinateAxis.Wrap(yPosition - yRadius);
        if (TryGetBlockAtPixel(level, Position16<RoomPixels>.Of(xPosition, topPosition), out RoomCollisionBlock top) &&
            top.CollisionType == RoomCollisionType.Slope &&
            top.Behavior.Slope.IsNonSquare)
        {
            if (top.Behavior.Slope.FlipsVertically)
            {
                int height = ReadAlignmentHeight(top.Behavior, xPosition);
                int topDepth = RoomGridConversions.OffsetFromBlockEnd(topPosition);
                int correction = CoordinateAxis.Difference(height - topDepth, 1);
                if (correction <= 0)
                {
                    yPosition = CoordinateAxis.Wrap(yPosition - correction);
                    adjusted = true;
                }
            }
        }

        return new SlopeAlignmentResult(position.WithY(yPosition), adjusted);
    }

    /// <summary>Converts a pixel probe to its containing room block; probes
    /// beyond the room bounds return false without reading collision data.</summary>
    private static bool TryGetBlockAtPixel(
        RoomLevelData level,
        Position16<RoomPixels> pixel,
        out RoomCollisionBlock block)
    {
        int blockX = RoomGridConversions.PixelToBlock(pixel.X);
        int blockY = RoomGridConversions.PixelToBlock(pixel.Y);
        if ((uint)blockX >= level.Size.X ||
            (uint)blockY >= level.Size.Y)
        {
            block = default;
            return false;
        }

        block = level.GetCollisionBlock(new(CoordinateAxis.Wrap(blockX), CoordinateAxis.Wrap(blockY)));
        return true;
    }

}
