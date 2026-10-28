namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Calculated pose-origin and selected-glyph support relationships for Samus landing, posture and drained transitions.</summary>
internal static class SamusBodyPlacementDefinitions
{
    /// <summary>$90:8D28..8D37: normal/spin landing, each with right/left four-byte rows.</summary>
    internal const int LandingPoseBytes = 4;
    /// <summary>$90:8D28..8D37: sixteen true landing bytes; $90:8D38 PLB is the separate narrowly retained observed byte.</summary>
    internal const int LandingDataBytes = 4 * LandingPoseBytes;
    /// <summary>$90:8D80..8D97: twelve transition poses with two bytes each, paired by facing.</summary>
    internal const int PosturePoseBytes = 2;
    /// <summary>$90:8D80..8D97: six right/left transition pairs, including two unused pairs.</summary>
    internal const int PostureDataBytes = 12 * PosturePoseBytes;
    /// <summary>The twelve $35-$40 posture poses whose rows $90:8D80..8D97 hold, in table order.</summary>
    internal static readonly SuperMetroid.Core.Samus.Poses.SamusPoseId[] PostureBlockPoses =
    [
        SuperMetroid.Core.Samus.Poses.SamusPoseId.CrouchingTransitionRightPose, SuperMetroid.Core.Samus.Poses.SamusPoseId.CrouchingTransitionLeftPose,
        SuperMetroid.Core.Samus.Poses.SamusPoseId.MorphingTransitionRightPose, SuperMetroid.Core.Samus.Poses.SamusPoseId.MorphingTransitionLeftPose,
        SuperMetroid.Core.Samus.Poses.SamusPoseId.UnusedPose39, SuperMetroid.Core.Samus.Poses.SamusPoseId.UnusedPose3A,
        SuperMetroid.Core.Samus.Poses.SamusPoseId.StandingTransitionRightPose, SuperMetroid.Core.Samus.Poses.SamusPoseId.StandingTransitionLeftPose,
        SuperMetroid.Core.Samus.Poses.SamusPoseId.UnmorphingTransitionRightPose, SuperMetroid.Core.Samus.Poses.SamusPoseId.UnmorphingTransitionLeftPose,
        SuperMetroid.Core.Samus.Poses.SamusPoseId.UnusedPose3F, SuperMetroid.Core.Samus.Poses.SamusPoseId.UnusedPose40,
    ];

    /// <summary>$90:8D38: exact PLB instruction encoding observed as the high byte of the final admitted unaligned word.</summary>
    /// <remarks>Only this one byte is retained as unrelated native instruction content; no memory beyond the17-byte window is modeled.</remarks>
    internal const byte AdjacentLandingPlb = 0xAB;

    /// <summary>$90:8D8C: selected standing-transition join one pixel above its destination support; narrowly retained visual composition.</summary>
    internal const int StandingSupportJoin = -1;
    /// <summary>$90:8D91: selected second-unmorph join one pixel below crouching support; narrowly retained visual composition.</summary>
    internal const int UnmorphSupportJoin = 1;

    /// <summary>$91:B378: eight ordinary Morph Ball rolling frames before the loop command; both facing sequences share this domain.</summary>
    internal const int RollingSupportFrames = 8;
    /// <summary>$90:8DEF/8DF0: narrowly retained common support of the two upper-half-only compact drained poses; selected scene composition.</summary>
    internal const int DrainedCompactSupport = 17;
    /// <summary>$90:8DF6: narrowly retained impact support before the kneeling cycle; selected scene composition.</summary>
    internal const int DrainedImpactSupport = 18;

    /// <summary>One of the two transfers a body tile definition's characters arrive in.</summary>
    internal enum BodyTilePart
    {
        /// <summary>The first transfer, filling a half row.</summary>
        First,

        /// <summary>The second transfer, filling the half row one full row below.</summary>
        Second,
    }
}
