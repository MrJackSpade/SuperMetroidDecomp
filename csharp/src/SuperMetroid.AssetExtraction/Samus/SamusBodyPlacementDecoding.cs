using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.AssetExtraction.Samus.SamusBodyPlacementSourceDefinitions;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Decodes, once, the cartridge's Samus body placement tables into each pose's placement and each of its animation
/// frames' own placement (#1517), so that the installed frames carry where they place the body and drawing places it by
/// pose and frame alone.
/// </summary>
/// <remarks>
/// <para>
/// The game's draw chooses the body origin by the pose's movement type and pose number. A landing pose reads the landing
/// table at four bytes per pose from the right-facing normal landing, plus the animation frame, as an unsigned rise. The
/// first twelve posture transitions, from the right-facing crouching transition, read the posture table at two bytes per
/// pose plus the frame, and the drained crouch reads the drained table by the frame directly; both are signed offsets
/// below the drawn point. The forward-facing bodies' frames from the third, and the drained standing body's from the
/// sixth, are pinned to fixed rises. Every other frame uses the pose's art origin.
/// </para>
/// <para>
/// A frame index that runs into the next pose's row reads that row, as the game does. An index past a table's end has no
/// placement, and the frame is not installed: the game's draw of it is rejected. The drained table's later bytes, which
/// the game lays out among the commands its drained controllers publish, are each a frame's placement like any other;
/// drawing reads only the placements of the frames a pose shows.
/// </para>
/// </remarks>
public static class SamusBodyPlacementDecoding
{
    /// <summary>Decodes every pose's placement from the cartridge's tables.</summary>
    /// <param name="graphicsYOffsets">Each pose's signed art origin, in the cartridge's pose order.</param>
    /// <param name="landingOffsets">The landing table's unsigned rises.</param>
    /// <param name="postureOffsets">The posture-transition table's signed offsets below the drawn point.</param>
    /// <param name="drainedOffsets">The drained-body table's signed offsets below the drawn point.</param>
    /// <returns>Each pose with a record, with its placement.</returns>
    /// <exception cref="InvalidDataException">A table has the wrong length.</exception>
    public static Dictionary<SamusPoseId, SamusBodyPosePlacement> Decode(
        IReadOnlyList<int> graphicsYOffsets, IReadOnlyList<int> landingOffsets, IReadOnlyList<int> postureOffsets,
        IReadOnlyList<int> drainedOffsets)
    {
        Ensure.NotNull(graphicsYOffsets);
        Ensure.NotNull(landingOffsets);
        Ensure.NotNull(postureOffsets);
        Ensure.NotNull(drainedOffsets);
        if (graphicsYOffsets.Count != SamusBodySources.PoseCount || landingOffsets.Count != LandingByteCount ||
            postureOffsets.Count != PostureByteCount || drainedOffsets.Count != DrainedByteCount)
            throw new InvalidDataException("Samus body placement tables have an invalid length.");
        var placements = new Dictionary<SamusPoseId, SamusBodyPosePlacement>();
        for (int number = 0; number < SamusBodySources.PoseCount; number++)
        {
            SamusPoseId pose = SamusPoseNumbering.FromNumber(number);
            int origin = graphicsYOffsets[number];
            placements.Add(pose, new SamusBodyPosePlacement(origin,
                FrameRises(pose, number, origin, landingOffsets, postureOffsets, drainedOffsets)));
        }
        return placements;
    }

    /// <summary>
    /// The rise of each of <paramref name="pose"/>'s animation frames, for a pose whose frames are not all placed by its art
    /// origin; null for a pose whose frames all are.
    /// </summary>
    private static int[]? FrameRises(SamusPoseId pose, int number, int origin, IReadOnlyList<int> landingOffsets,
        IReadOnlyList<int> postureOffsets, IReadOnlyList<int> drainedOffsets)
    {
        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(pose);
        if (movementType == SamusMovementType.Standing &&
            pose is SamusPoseId.ForwardFacingPowerSuitPose or SamusPoseId.ForwardFacingSuitedPose)
            return Pinned(origin, ForwardFacingFirstLaterFrame, ForwardFacingLaterRise);
        int landingRow = number - SamusPoseNumbering.Number(SamusPoseId.NormalLandingRightPose);
        if (movementType == SamusMovementType.Standing && landingRow is >= 0 and < LandingByteCount / LandingPoseByteCount)
            return [.. landingOffsets.Skip(landingRow * LandingPoseByteCount)];
        int postureRow = number - SamusPoseNumbering.Number(SamusPoseId.CrouchingTransitionRightPose);
        if (movementType == SamusMovementType.PostureTransition && postureRow is >= 0 and < PostureByteCount / PosturePoseByteCount)
            return [.. postureOffsets.Skip(postureRow * PosturePoseByteCount).Select(offset => -offset)];
        if (pose is SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose)
            return [.. drainedOffsets.Select(offset => -offset)];
        if (pose is SamusPoseId.DrainedStandingRightPose or SamusPoseId.DrainedStandingLeftPose)
            return Pinned(origin, DrainedStandingFirstLaterFrame, DrainedStandingLaterRise);
        return null;
    }

    /// <summary>
    /// Every animation frame the body sheets cover placed by <paramref name="origin"/> before
    /// <paramref name="firstPinnedFrame"/> and by <paramref name="pinnedRise"/> from it.
    /// </summary>
    private static int[] Pinned(int origin, int firstPinnedFrame, int pinnedRise) =>
        [.. Enumerable.Range(0, SamusBodySheetDefinitions.AnimationFrames).Select(frame => frame >= firstPinnedFrame ? pinnedRise : origin)];
}
