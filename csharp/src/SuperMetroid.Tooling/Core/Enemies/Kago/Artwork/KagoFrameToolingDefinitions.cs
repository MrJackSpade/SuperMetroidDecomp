namespace SuperMetroid.Core.Enemies.Kago.Artwork;

/// <summary>Development-tool members of <see cref="KagoFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class KagoFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa8;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(KagoFrame frame) => frame.Pose switch
    {
        0 => 0xabda,
        1 => 0xabf0,
        2 => 0xac06,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Kago pose."),
    };
}
