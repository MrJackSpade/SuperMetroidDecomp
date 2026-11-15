namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Which platform, the axis it is moving along, and its horizontal direction: what selects its animation.</summary>
/// <param name="SuspensorPlatform">Whether the platform is the suspensor platform rather than a Tripper.</param>
/// <param name="Motion">The axis the platform moves along: vertical selects the moving animations, horizontal the still ones.</param>
/// <param name="Direction">The way the platform moves or faces horizontally.</param>
internal readonly record struct PlatformPose(bool SuspensorPlatform, Axis Motion, PlatformHorizontalMovement Direction);
