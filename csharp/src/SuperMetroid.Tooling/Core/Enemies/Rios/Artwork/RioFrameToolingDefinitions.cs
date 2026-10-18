using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Enemies.Rios.Artwork;

/// <summary>Development-tool members of <see cref="RioFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class RioFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(RioFrame frame) => frame.Pose switch
    {
        0 => 0xbd6c,
        1 => 0xbd82,
        2 => 0xbd98,
        3 => 0xbdae,
        4 => 0xbdc4,
        5 => 0xbdda,
        6 => 0xbdf0,
        7 => 0xbe06,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Rio pose."),
    };
}
