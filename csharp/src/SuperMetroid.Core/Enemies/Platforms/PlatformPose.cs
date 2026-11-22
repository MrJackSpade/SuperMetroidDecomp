namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Which platform, the art it shows for its vertical movement, and its horizontal direction: what selects its animation.</summary>
/// <param name="Kind">Which platform it is, the Tripper or the suspensor platform.</param>
/// <param name="Art">The art it shows: moving while it rises or sinks, still at rest (#2536).</param>
/// <param name="Direction">The way the platform moves or faces horizontally.</param>
internal readonly partial record struct PlatformPose(PlatformKind Kind, PlatformVerticalArt Art, HorizontalDirection Direction);
