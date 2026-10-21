namespace SuperMetroid.Core.Enemies.WreckedShip.Artwork;

/// <summary>Development-tool members of <see cref="WreckedShipGhostFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class WreckedShipGhostFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa8;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(WreckedShipGhostFrame frame) => frame.Pose switch
    {
        0 => 0x9e46,
        1 => 0x9e5c,
        2 => 0x9e72,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Wrecked Ship ghost pose."),
    };
}
