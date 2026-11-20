namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The growing and vertical shutters' poses and Kamer's platform loop as typed animations (#641).
/// Their steps are <see cref="ShutterAnimations"/>.
/// </summary>
internal enum ShutterSegment
{
    /// <summary>The growing shutter holding its first section alone, a quarter grown, as it starts.</summary>
    QuarterGrown,
    /// <summary>The growing shutter holding two of its four sections, half grown.</summary>
    HalfGrown,
    /// <summary>The growing shutter holding three of its four sections, three quarters grown.</summary>
    ThreeQuartersGrown,
    /// <summary>The shutter holding all four sections, fully grown: a growing shutter's last stage and a vertical shutter's pose.</summary>
    FullyGrown,
    /// <summary>Kamer's light brightening from dark to bright, repeating.</summary>
    KamerPlatform,
}
