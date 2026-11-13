namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Chooses a Tripper's or Kamer's animation for its motion and direction.</summary>
internal static class PlatformAnimationChoices
{
    /// <summary>
    /// The animation for the zero/nonzero direction branches: vertical motion selects the moving
    /// animations, and every nonzero direction value selects the right-moving animation.
    /// </summary>
    internal static PlatformSegment SelectAnimation(bool isKamer, Axis axis, PlatformHorizontalMovement direction) =>
        // The test is zero against nonzero: any direction value other than left's moves right.
        PlatformAnimationDefinitions.Animations[new PlatformPose(isKamer, axis,
            direction == PlatformHorizontalMovement.Left ? PlatformHorizontalMovement.Left : PlatformHorizontalMovement.Right)];
}
