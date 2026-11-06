namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The poses with records that no path of the game gives Samus (#1513). Every write of one of these poses, whether a
/// pose-input list, an animation program's transition, a pose-table fallback, a mirror or a movement handler, runs only
/// while Samus already holds one of them, so from any pose she starts in she never reaches them, and the body never
/// draws them.
/// </summary>
internal static class SamusUnreachablePoseDefinitions
{
    /// <summary>
    /// The unreachable poses, in the game's numbering: the glitch balls and their knockbacks (the knockback is chosen only
    /// for a glitch ball's movement), the posture transitions to them and the instant morphing transitions, the
    /// running-aimed-straight-up records, the compact airborne records, the grapple airborne records and the Draygon-held
    /// records. No pose-input list, no-input pose, program transition or outside path writes the running-aimed-straight-up
    /// records, whose tables pair the standing aim-up top and tops facing other ways with running legs. The records of
    /// the poses no path reaches in any way have no pose at all (#1517): these keep theirs because their programs are laid
    /// out after reachable ones, and an animation read past a program's end reads them.
    /// </summary>
    internal static readonly SamusPoseId[] Poses =
    [
        SamusPoseId.RunningAimUpRightPose,
        SamusPoseId.RunningAimUpLeftPose,
        SamusPoseId.GlitchBallRightPose,
        SamusPoseId.UnusedKnockbackRightPose,
        SamusPoseId.UnusedKnockbackLeftPose,
        SamusPoseId.GlitchBallMorphingRightPose,
        SamusPoseId.GlitchBallMorphingLeftPose,
        SamusPoseId.InstantMorphingRightPose,
        SamusPoseId.InstantMorphingLeftPose,
        SamusPoseId.GlitchBallLeftPose,
        SamusPoseId.CompactAirborneRightPose,
        SamusPoseId.CompactAirborneLeftPose,
        SamusPoseId.CompactAirborneFallingRightPose,
        SamusPoseId.CompactAirborneFallingLeftPose,
        SamusPoseId.GrappleAirborneGunExtendedRightPose,
        SamusPoseId.GrappleAirborneAimDownRightPose,
        SamusPoseId.GrappleAirborneAimDiagonalDownRightPose,
        SamusPoseId.DraygonGrabbingLeftPose,
        SamusPoseId.CompactMorphingRightPose,
        SamusPoseId.DraygonHeldMorphingLeftPose,
        SamusPoseId.CompactStandingUpRightPose,
        SamusPoseId.DraygonHeldStandingUpLeftPose,
        SamusPoseId.DraygonHeldBallLeftPose,
    ];
}
