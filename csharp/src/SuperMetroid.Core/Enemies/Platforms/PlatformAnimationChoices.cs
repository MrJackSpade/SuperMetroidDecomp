namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Chooses a Tripper's or Kamer's animation for its motion and direction.</summary>
internal static class PlatformAnimationChoices
{
    /// <summary>
    /// The animation for the platform's motion and direction: vertical motion selects the moving animations, horizontal
    /// motion the still ones, each facing the way the platform moves.
    /// </summary>
    internal static PlatformSegment SelectAnimation(bool isKamer, Axis axis, PlatformHorizontalMovement direction) =>
        PlatformAnimationDefinitions.Animations[new PlatformPose(isKamer, axis, direction)];
}
