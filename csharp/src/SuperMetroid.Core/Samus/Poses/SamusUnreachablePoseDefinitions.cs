namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The poses with records that no path of the game gives Samus (#1513, #1535). No pose-input list, no-input pose,
/// animation program transition, pose-table fallback, mirror, movement handler or outside write installs one of them,
/// so Samus never holds them and the body never draws them.
/// </summary>
/// <remarks>
/// The Baby's Hyper Beam freeze and its rainbow stop set the animation frame of a Samus the drain has let go of, whatever
/// pose she holds, and a crouch, morph or stand transition reads that frame past its own short program into the programs
/// laid out after it (<see cref="Compatibility.LaidOutAnimationContinuation{TAnimation, TStep}"/>). Those reads install the glitch
/// balls and the Draygon-held ball, which are therefore held poses and are not listed here (#1535). The transitions whose
/// programs those reads run through are still never held: their programs run under the reading pose, which is the pose
/// drawn, and nothing installs the transitions themselves.
/// </remarks>
internal static class SamusUnreachablePoseDefinitions
{
    /// <summary>
    /// The unreachable poses: the running-aimed-straight-up records, whose tables pair the
    /// standing aim-up top and tops facing other ways with running legs and which nothing writes; the glitch-ball and
    /// instant morphing transitions, the compact airborne records, the grapple airborne records, the Draygon grab and the
    /// compact and Draygon-held transitions. These keep their records because their programs are laid out after reachable
    /// ones and a read past a program's end runs them under the reading pose; nothing installs the poses themselves. The
    /// records of the poses no path reaches in any way, not even by such a read, have no pose at all (#1517).
    /// </summary>
    internal static readonly SamusPoseId[] Poses =
    [
        SamusPoseId.RunningAimUpRightPose,
        SamusPoseId.RunningAimUpLeftPose,
        SamusPoseId.GlitchBallMorphingRightPose,
        SamusPoseId.GlitchBallMorphingLeftPose,
        SamusPoseId.InstantMorphingRightPose,
        SamusPoseId.InstantMorphingLeftPose,
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
    ];
}
