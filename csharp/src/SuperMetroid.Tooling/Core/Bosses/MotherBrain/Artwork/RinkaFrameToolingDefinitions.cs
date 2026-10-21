namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Development-tool members of <see cref="RinkaFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class RinkaFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(RinkaFrame frame) => frame.Pose switch
    {
        0 => 0xba38,
        1 => 0xba3f,
        2 => 0xba46,
        3 => 0xba4d,
        4 => 0xba54,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Rinka pose."),
    };
}
