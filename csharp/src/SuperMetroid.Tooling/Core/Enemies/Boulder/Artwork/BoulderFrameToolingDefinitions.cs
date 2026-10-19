using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Enemies.Boulder.Artwork;

/// <summary>Development-tool members of <see cref="BoulderFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class BoulderFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(BoulderFrame frame) => frame.Pose switch
    {
        0 => 0x8a59,
        1 => 0x8a6f,
        2 => 0x8a85,
        3 => 0x8a9b,
        4 => 0x8ab1,
        5 => 0x8ac7,
        6 => 0x8add,
        7 => 0x8af3,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Boulder pose."),
    };
}
