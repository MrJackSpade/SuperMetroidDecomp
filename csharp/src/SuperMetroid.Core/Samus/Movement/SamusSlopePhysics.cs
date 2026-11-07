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
    /// The slope block's behaviour. Bits 0–4 choose the profile; bit 7 marks a ceiling slope.
    /// </param>
    /// <param name="displacement">Signed 16.16 horizontal displacement.</param>
    /// <param name="verticalSpeed">
    /// Unsigned high/low pair of <c>samus_y_speed</c>/<c>samus_y_subspeed</c>. Any nonzero
    /// value suppresses grounded slope scaling exactly as the routine does.
    /// </param>
    public static int ScaleGroundedHorizontalDisplacement(
        int behavior,
        int displacement,
        uint verticalSpeed)
        => ScaleGroundedHorizontalDisplacement(
            new RoomBlockBehavior(behavior),
            displacement,
            verticalSpeed);

    /// <summary>Typed behaviour overload used by live room collision.</summary>
    public static int ScaleGroundedHorizontalDisplacement(
        RoomBlockBehavior behavior,
        int displacement,
        uint verticalSpeed)
    {

        // Ceiling slopes and airborne Samus leave the incoming amount untouched. This is
        // the routine's complete early-return condition, not a host-side policy choice.
        if (behavior.SlopeFlipsVertically || verticalSpeed != 0)
            return displacement;

        int multiplier = SlopeSpeeds.HorizontalMultiplier(behavior.SlopeShape);

        // The game discards the displacement's lowest eight fractional bits before the
        // unsigned 16×16 multiply. For negative values it negates that truncated 16-bit
        // magnitude, multiplies, and negates the 32-bit result afterward.
        if (displacement >= 0)
        {
            int truncated = CoordinateAxis.Wrap(EightEight.WholeOf(displacement));
            return unchecked((int)((uint)truncated * multiplier));
        }
        else
        {
            int magnitude = -EightEight.WholeOf(displacement);
            return unchecked(-(int)((uint)magnitude * multiplier));
        }
    }

    /// <summary>Typed behaviour overload used by live room collision.</summary>
    public static int ReadAlignmentHeight(
        RoomBlockBehavior behavior,
        int xPosition)
    {

        // the behaviour's horizontal-flip bit mirrors the profile horizontally. XORing the complete coordinate by
        // 15 is equivalent to flipping its low nibble while retaining sixteen-bit word math.
        int sampledX = behavior.SlopeFlipsHorizontally
            ? CoordinateAxis.Wrap(xPosition ^ 15)
            : xPosition;
        return SlopeHeights.Read(behavior.SlopeShape, sampledX & 15);
    }

    /// <summary>
    /// Aligns Samus's Y position to a slope: the post-horizontal correction
    /// for non-square floor and ceiling slopes.
    /// </summary>
    /// <remarks>
    /// This routine does not perform the preceding horizontal collision scan. It consumes
    /// the position that scan accepted and adjusts only whole-pixel Y, exactly like the
    /// original call immediately after a move without solid-enemy collision.
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
        // type 1, shapes >=5, and the behaviour's ceiling bit clear participate as floor slopes.
        int bottomPosition = CoordinateAxis.Wrap(yPosition + yRadius - 1);
        if (TryGetBlockAtPixel(level, Position16<RoomPixels>.Of(xPosition, bottomPosition), out RoomCollisionBlock bottom) &&
            bottom.CollisionType == RoomCollisionType.Slope &&
            bottom.Behavior.IsNonSquareSlope)
        {
            if (!bottom.Behavior.SlopeFlipsVertically)
            {
                int height = ReadAlignmentHeight(bottom.Behavior, xPosition);
                int bottomNibble = bottomPosition & 15;
                int correction = CoordinateAxis.Difference(height - bottomNibble, 1);
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
            top.Behavior.IsNonSquareSlope)
        {
            if (top.Behavior.SlopeFlipsVertically)
            {
                int height = ReadAlignmentHeight(top.Behavior, xPosition);
                int invertedTopNibble = (topPosition & 15) ^ 15;
                int correction = CoordinateAxis.Difference(height - invertedTopNibble, 1);
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
        int blockX = pixel.X >> 4;
        int blockY = pixel.Y >> 4;
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
