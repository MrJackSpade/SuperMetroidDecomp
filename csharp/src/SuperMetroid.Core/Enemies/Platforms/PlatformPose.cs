namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Which platform, the axis it is moving along, and its horizontal direction: what selects its program.</summary>
/// <param name="Kamer">Whether the platform is a Kamer rather than a Tripper.</param>
/// <param name="Motion">The axis the platform moves along: vertical selects the moving programs, horizontal the still ones.</param>
/// <param name="Direction">The way the platform moves or faces horizontally.</param>
internal readonly record struct PlatformPose(bool Kamer, Axis Motion, PlatformHorizontalMovement Direction);
