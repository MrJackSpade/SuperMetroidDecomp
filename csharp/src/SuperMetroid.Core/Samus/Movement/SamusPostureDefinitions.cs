namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Authored displacement values for posture transition commands.</summary>
public static class SamusPostureDefinitions
{
    /// <summary>
    /// The pose-change downward distance for <c>MorphingTransitionRightPose</c> and <c>MorphingTransitionLeftPose</c>. Command seven
    /// requests nine pixels downward with the new radius, irrespective of the
    /// source radius, and clips that request through downward block collision.
    /// </summary>
    public const int MorphEntryDownwardPixels = 9;

    /// <summary>
    /// For <c>CrouchingTransitionRightPose</c> and <c>CrouchingTransitionLeftPose</c>, and the literal five for aimed
    /// crouching transitions. Command seven requests five pixels downward with the
    /// new radius 16 and clips that request through downward block collision.
    /// </summary>
    public const int CrouchEntryDownwardPixels = 5;
}
