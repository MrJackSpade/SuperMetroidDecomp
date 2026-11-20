using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The block moves Samus's movements share (#2557): a sideways move that a wall stops, a vertical move in the update's scan
/// order, and the grounding probe a movement with no vertical speed runs. Grounded, knockback, posture and Morph Ball
/// movement each call them with the terrain their update moves through.
/// </summary>
internal static class SamusTerrainMoves
{
    /// <summary>
    /// Moves Samus by <paramref name="requested"/> through <paramref name="terrain"/>; a wall she meets stops her horizontal
    /// momentum at once, before any vertical move, so the next update does not carry her on from the wall.
    /// </summary>
    /// <param name="terrain">The terrain she moves through.</param>
    /// <param name="samus">Samus.</param>
    /// <param name="requested">The sideways move and the direction its collision is checked in.</param>
    internal static BlockMoveResult MoveStoppingAtWalls(SamusTerrain terrain, SamusState samus, SamusHorizontalDisplacement requested)
    {
        BlockMoveResult moved = SamusBlockCollision.MoveHorizontal(terrain, Ensure.NotNull(samus).Kinematics, samus, samus.Pose,
            new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
        if (moved.Collided)
            samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        return moved;
    }

    /// <summary>
    /// Moves Samus vertically by <paramref name="displacement"/> through <paramref name="terrain"/>, her block scan running
    /// left to right on the updates its cadence names: that parity can decide which of two simultaneous block reactions wins.
    /// </summary>
    /// <param name="terrain">The terrain she moves through.</param>
    /// <param name="samus">Samus.</param>
    /// <param name="displacement">The vertical move.</param>
    /// <param name="frameCounter">The update's frame counter, whose parity picks the scan's column order.</param>
    internal static BlockMoveResult MoveVertically(SamusTerrain terrain, SamusState samus, SignedSixteenSixteen displacement, int frameCounter) =>
        SamusBlockCollision.MoveVertical(terrain, Ensure.NotNull(samus).Kinematics, samus, samus.Pose,
            new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });

    /// <summary>
    /// Moves Samus vertically without a speed calculation: by any external vertical displacement, otherwise the grounding
    /// probe's asymmetric positive one-pixel bias over her total horizontal speed.
    /// </summary>
    /// <param name="terrain">The terrain she moves through.</param>
    /// <param name="samus">Samus.</param>
    /// <param name="frameCounter">The update's frame counter, whose parity picks the scan's column order.</param>
    internal static BlockMoveResult ProbeGroundWithoutSpeed(SamusTerrain terrain, SamusState samus, int frameCounter) =>
        MoveVertically(terrain, samus,
            SamusExtraDisplacement.CalculateNoSpeedVerticalDisplacement(Ensure.NotNull(samus).Kinematics, samus.HorizontalSpeed), frameCounter);
}
