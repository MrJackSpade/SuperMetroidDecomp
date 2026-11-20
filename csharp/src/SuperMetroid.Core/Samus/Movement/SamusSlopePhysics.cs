using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Primitives used by Samus's non-square slope collision path.
/// </summary>
/// <remarks>
/// Super Metroid does not reduce slopes to a line equation. A block's slope shape selects one of 32
/// sixteen-sample height profiles, while a separate multiplier scales grounded horizontal
/// displacement. The profiles and multipliers give the exact discrete geometry and whole-pixel
/// movement.
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
    /// <param name="verticalSpeed">Samus's vertical speed; any vertical motion leaves the displacement
    /// unscaled.</param>
    public static SignedSixteenSixteen ScaleGroundedHorizontalDisplacement(
        RoomBlockBehavior behavior,
        SignedSixteenSixteen displacement,
        UnsignedSixteenSixteen verticalSpeed)
    {
        // Ceiling slopes and an airborne Samus keep their horizontal displacement unscaled.
        if (behavior.Slope.FlipsVertically || !verticalSpeed.IsZero)
            return displacement;

        // A slope scales leftward and rightward movement by the same rule (SignedSixteenSixteen.ScaledBy).
        return displacement.ScaledBy(behavior.Slope.Shape.GroundedSpeedShare);
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
    /// the position that scan accepted and adjusts only whole-pixel Y, as the game does
    /// straight after a move without solid-enemy collision.
    /// </remarks>
    public static SlopeAlignmentResult AlignYPosition(
        RoomLevelData level,
        Position16<RoomPixels> position,
        int yRadius,
        bool horizontalSlopeCollisionEnabled)
    {
        Ensure.NotNull(level);
        int xPosition = position.X;
        int yPosition = position.Y;

        if (!horizontalSlopeCollisionEnabled)
            return new SlopeAlignmentResult(position, Adjusted: false);

        bool adjusted = false;

        // First probes the center-X block containing Samus's bottom pixel. Only
        // slope blocks with a non-square shape that is not flipped vertically participate as floor slopes.
        int bottomPosition = TerrainScanEdges.Edge(yPosition + yRadius - 1);
        if (TryGetBlockAtPixel(level, Position16<RoomPixels>.Of(xPosition, bottomPosition), out RoomCollisionBlock bottom) &&
            bottom.CollisionType == RoomCollisionType.Slope &&
            bottom.Behavior.Slope.IsNonSquare)
        {
            if (!bottom.Behavior.Slope.FlipsVertically)
            {
                int height = ReadAlignmentHeight(bottom.Behavior, xPosition);
                int bottomDepth = RoomGridConversions.OffsetWithinBlock(bottomPosition);
                int correction = height - bottomDepth - 1;
                if (correction < 0)
                {
                    yPosition += correction;
                    adjusted = true;
                }
            }
        }

        // Recompute the top probe after any floor correction. Vertically flipped slopes are ceilings and reverse the same signed
        // correction.
        int topPosition = TerrainScanEdges.Edge(yPosition - yRadius);
        if (TryGetBlockAtPixel(level, Position16<RoomPixels>.Of(xPosition, topPosition), out RoomCollisionBlock top) &&
            top.CollisionType == RoomCollisionType.Slope &&
            top.Behavior.Slope.IsNonSquare)
        {
            if (top.Behavior.Slope.FlipsVertically)
            {
                int height = ReadAlignmentHeight(top.Behavior, xPosition);
                int topDepth = RoomGridConversions.OffsetFromBlockEnd(topPosition);
                int correction = height - topDepth - 1;
                if (correction <= 0)
                {
                    yPosition -= correction;
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
        Position16<RoomBlocks> blockAt = pixel.ToRoomBlock();
        if (!level.Size.Contains(blockAt))
        {
            block = default;
            return false;
        }

        block = level.GetCollisionBlock(blockAt);
        return true;
    }
}
