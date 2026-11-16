namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// The art a platform last chose for its vertical movement (#2145): moving while it rises or sinks, still once it rests.
/// Its values are arbitrary.
/// </summary>
public enum PlatformVerticalArt
{
    /// <summary>The art of a platform rising or sinking.</summary>
    Moving,

    /// <summary>The art of a platform at rest.</summary>
    Still,
}
