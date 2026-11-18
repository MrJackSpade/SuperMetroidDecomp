namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Cartridge values and helpers of <see cref="SamusPoseMetadata"/> that only verification reads.</summary>
internal static class SamusPoseMetadataConstants
{
    /// <summary>
    /// True when the ordinary grounded input step may interrupt a landing
    /// with any same-facing normal-jump transition, neutral or aimed.
    /// </summary>
    /// <remarks>
    /// Every landing pose above takes the standing input transitions
    /// (<see cref="SamusPoseInputList.StandingAndLandingRight"/>), the aimed and firing landings as well as the normal
    /// and spin ones. Those transitions keep Up, the diagonal-up aim or the diagonal-down aim held on the fresh
    /// Jump press, so a right diagonal-up landing can take the right
    /// diagonal-up jump transition, not only the neutral one. Every landing of the family admits every one of
    /// these transitions.
    /// </remarks>
    public static bool IsLandingToNormalJumpTransition(SamusPoseId sourcePose, SamusPoseId targetPose) =>
        SamusPoseMetadata.IsRightFacingLandingPose(sourcePose) && targetPose is
            SamusPoseId.NeutralJumpTransitionRightPose or
            SamusPoseId.NormalJumpTransitionAimUpRightPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose ||
        SamusPoseMetadata.IsLeftFacingLandingPose(sourcePose) && targetPose is
            SamusPoseId.NeutralJumpTransitionLeftPose or
            SamusPoseId.NormalJumpTransitionAimUpLeftPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose;
}
