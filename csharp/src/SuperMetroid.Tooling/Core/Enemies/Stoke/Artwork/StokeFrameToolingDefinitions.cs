namespace SuperMetroid.Core.Enemies.Stoke.Artwork;

/// <summary>Development-tool members of <see cref="StokeFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class StokeFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(StokeFrame frame) => frame.Pose switch
    {
        0 => 0x8aca,
        1 => 0x8ad6,
        2 => 0x8ae7,
        3 => 0x8af3,
        4 => 0x8aff,
        5 => 0x8b15,
        6 => 0x8b21,
        7 => 0x8b32,
        8 => 0x8b3e,
        9 => 0x8b4a,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Stoke pose."),
    };
}
