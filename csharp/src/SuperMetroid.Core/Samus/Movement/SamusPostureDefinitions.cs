namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Authored displacement values for posture transitions.</summary>
public static class SamusPostureDefinitions
{
    /// <summary>
    /// The pose-change downward distance for <see cref="Poses.SamusPoseId.MorphingTransitionRightPose"/> and
    /// <see cref="Poses.SamusPoseId.MorphingTransitionLeftPose"/>. The posture change
    /// requests nine pixels downward with the new radius, irrespective of the
    /// source radius, and clips that request through downward block collision.
    /// </summary>
    public const int MorphEntryDownwardPixels = 9;

    /// <summary>
    /// For <see cref="Poses.SamusPoseId.CrouchingTransitionRightPose"/> and
    /// <see cref="Poses.SamusPoseId.CrouchingTransitionLeftPose"/>, and the literal five for aimed
    /// crouching transitions. The posture change requests five pixels downward with the
    /// new radius 16 and clips that request through downward block collision.
    /// </summary>
    public const int CrouchEntryDownwardPixels = 5;
}
