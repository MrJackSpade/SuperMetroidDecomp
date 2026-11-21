namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Authored displacement values for posture transitions.</summary>
public static partial class SamusPostureDefinitions
{
    /// <summary>
    /// The pose-change downward distance for <see cref="Poses.SamusPoseId.MorphingTransitionRightPose"/> and
    /// <see cref="Poses.SamusPoseId.MorphingTransitionLeftPose"/>. The posture change moves Samus down this far with
    /// her new radius, whatever her radius before, stopping at any block below her.
    /// </summary>
    public const int MorphEntryDownwardPixels = 9;

    /// <summary>
    /// The pose-change downward distance for <see cref="Poses.SamusPoseId.CrouchingTransitionRightPose"/>,
    /// <see cref="Poses.SamusPoseId.CrouchingTransitionLeftPose"/> and the aimed crouching transitions. The posture change
    /// moves Samus down this far with her crouching radius, stopping at any block below her.
    /// </summary>
    public const int CrouchEntryDownwardPixels = 5;
}
