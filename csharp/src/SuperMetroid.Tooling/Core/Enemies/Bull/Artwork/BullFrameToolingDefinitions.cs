namespace SuperMetroid.Core.Enemies.Bull.Artwork;

/// <summary>Development-tool members of <see cref="BullFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class BullFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa8;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(BullFrame frame) => frame.Pose switch
    {
        0 => 0xdb76,
        1 => 0xdb8c,
        2 => 0xdba2,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Bull pose."),
    };
}
