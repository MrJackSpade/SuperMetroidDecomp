namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Animation selection for the Tripper and suspensor platforms.</summary>
internal static class PlatformAnimationDefinitions
{
    /// <summary>
    /// Animation choice for each platform pose; initialization uses the still art. The suspensor platform looks the same moving or
    /// still, so only its heading chooses its animation (#1842).
    /// </summary>
    internal static readonly Dictionary<PlatformPose, PlatformSegment> Animations = new()
    {
        [new(PlatformKind.SuspensorPlatform, PlatformVerticalArt.Moving, HorizontalDirection.Left)] = PlatformSegment.SuspensorPlatformHeadingLeft,
        [new(PlatformKind.SuspensorPlatform, PlatformVerticalArt.Moving, HorizontalDirection.Right)] = PlatformSegment.SuspensorPlatformHeadingRight,
        [new(PlatformKind.SuspensorPlatform, PlatformVerticalArt.Still, HorizontalDirection.Left)] = PlatformSegment.SuspensorPlatformHeadingLeft,
        [new(PlatformKind.SuspensorPlatform, PlatformVerticalArt.Still, HorizontalDirection.Right)] = PlatformSegment.SuspensorPlatformHeadingRight,
        [new(PlatformKind.Tripper, PlatformVerticalArt.Moving, HorizontalDirection.Left)] = PlatformSegment.TripperMovingLeft,
        [new(PlatformKind.Tripper, PlatformVerticalArt.Moving, HorizontalDirection.Right)] = PlatformSegment.TripperMovingRight,
        [new(PlatformKind.Tripper, PlatformVerticalArt.Still, HorizontalDirection.Left)] = PlatformSegment.TripperStillMovingLeft,
        [new(PlatformKind.Tripper, PlatformVerticalArt.Still, HorizontalDirection.Right)] = PlatformSegment.TripperStillMovingRight,
    };
}
