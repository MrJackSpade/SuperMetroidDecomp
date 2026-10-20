namespace SuperMetroid.Core.Enemies.Platforms.Artwork;

/// <summary>Development-tool members of <see cref="PlatformFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class PlatformFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa3;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(PlatformFrame frame) => frame.Pose switch
    {
        0 => 0x9f29,
        1 => 0x9f3a,
        2 => 0x9f4b,
        3 => 0x9f5c,
        4 => 0x9f6d,
        5 => 0x9f7e,
        6 => 0x9f8f,
        7 => 0x9fa5,
        8 => 0x9fb6,
        9 => 0x9fcc,
        10 => 0x9fe2,
        11 => 0x9ff3,
        12 => 0xa021,
        13 => 0xa02d,
        14 => 0xa039,
        15 => 0xa045,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Tripper or Kamer pose."),
    };
}
