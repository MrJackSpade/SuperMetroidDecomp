namespace SuperMetroid.Core.Enemies.Polyp.Artwork;

/// <summary>Development-tool members of <see cref="PolypFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class PolypFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(PolypFrame frame) => frame.Pose switch
    {
        0 => 0xb5fb,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Polyp pose."),
    };
}
