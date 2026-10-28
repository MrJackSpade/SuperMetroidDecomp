namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Which platform, whether it is moving, and whether it faces left: what selects its program.</summary>
/// <param name="Kamer">Whether the platform is a Kamer rather than a Tripper.</param>
/// <param name="Moving">Whether the platform is moving vertically rather than still.</param>
/// <param name="FacingLeft">Whether the platform moves or faces left.</param>
internal readonly record struct PlatformPose(bool Kamer, bool Moving, bool FacingLeft);
