namespace SuperMetroid.Core.Enemies.Kzan.Artwork;

/// <summary>Development-tool members of <see cref="KzanFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class KzanFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(KzanFrame frame) => frame.Pose switch
    {
        0 => 0x8ce5,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Kzan pose."),
    };
}
