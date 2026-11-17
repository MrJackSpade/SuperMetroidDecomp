namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Chooses a Tripper's or the suspensor platform's animation for its motion and direction.</summary>
internal static class PlatformAnimationChoices
{
    /// <summary>
    /// The animation for the platform's motion and direction: vertical motion selects the moving animations, horizontal
    /// motion the still ones, each facing the way the platform moves.
    /// </summary>
    internal static PlatformSegment SelectAnimation(bool isSuspensorPlatform, Axis axis, HorizontalDirection direction) =>
        PlatformAnimationDefinitions.Animations[new PlatformPose(isSuspensorPlatform, axis, direction)];
}
