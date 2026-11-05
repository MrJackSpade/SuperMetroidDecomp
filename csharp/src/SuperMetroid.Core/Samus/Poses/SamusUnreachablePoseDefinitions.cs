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
    /// for a glitch ball's movement), the posture transitions to and from them and their spring-ball item transitions,
    /// the unused running, standing and crouch-transition records, the unused aerial records and their input lists, the
    /// unused grapple drop records and the unused Draygon-held records.
    /// </summary>
    internal static readonly SamusPoseId[] Poses =
    [
        SamusPoseId.UnusedPose20,
        SamusPoseId.UnusedPose21,
        SamusPoseId.UnusedPose22,
        SamusPoseId.UnusedPose23,
        SamusPoseId.UnusedPose24,
        SamusPoseId.UnusedKnockbackRightPose,
        SamusPoseId.UnusedKnockbackLeftPose,
        SamusPoseId.UnusedPose39,
        SamusPoseId.UnusedPose3A,
        SamusPoseId.UnusedPose3F,
        SamusPoseId.UnusedPose40,
        SamusPoseId.UnusedPose42,
        SamusPoseId.UnusedPose45,
        SamusPoseId.UnusedPose46,
        SamusPoseId.UnusedPose47,
        SamusPoseId.UnusedPose48,
        SamusPoseId.UnusedPose5B,
        SamusPoseId.UnusedPose5C,
        SamusPoseId.UnusedPose5D,
        SamusPoseId.UnusedPose5E,
        SamusPoseId.UnusedPose5F,
        SamusPoseId.UnusedPose60,
        SamusPoseId.UnusedPose61,
        SamusPoseId.UnusedPose62,
        SamusPoseId.UnusedPose63,
        SamusPoseId.UnusedPose64,
        SamusPoseId.UnusedPose65,
        SamusPoseId.UnusedPose66,
        SamusPoseId.UnusedPoseAC,
        SamusPoseId.UnusedPoseAD,
        SamusPoseId.UnusedPoseAE,
        SamusPoseId.UnusedPoseAF,
        SamusPoseId.UnusedPoseB0,
        SamusPoseId.UnusedPoseB1,
        SamusPoseId.UnusedPoseC5,
        SamusPoseId.UnusedPoseC6,
        SamusPoseId.UnusedPoseDb,
        SamusPoseId.UnusedPoseDc,
        SamusPoseId.UnusedPoseDd,
        SamusPoseId.UnusedPoseDe,
        SamusPoseId.UnusedPoseDf,
    ];
}
