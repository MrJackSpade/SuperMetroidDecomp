namespace SuperMetroid.Core.Enemies.Owtch.Artwork;

/// <summary>Development-tool members of <see cref="OwtchFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class OwtchFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(OwtchFrame frame) => frame.Pose switch
    {
        0 => 0xa589,
        1 => 0xa590,
        2 => 0xa597,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Owtch pose."),
    };
}
