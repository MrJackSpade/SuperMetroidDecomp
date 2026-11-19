namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Chooses a Tripper's or the suspensor platform's animation for its art and direction.</summary>
internal static class PlatformAnimationChoices
{
    /// <summary>The animation that shows <paramref name="art"/>, facing the way the platform moves.</summary>
    internal static PlatformSegment SelectAnimation(PlatformKind kind, PlatformVerticalArt art, HorizontalDirection direction) =>
        PlatformAnimationDefinitions.Animations[new PlatformPose(kind, art, direction)];
}
