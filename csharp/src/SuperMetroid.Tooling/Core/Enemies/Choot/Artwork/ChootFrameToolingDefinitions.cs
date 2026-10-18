using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Enemies.Choot.Artwork;

/// <summary>Development-tool members of <see cref="ChootFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class ChootFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(ChootFrame frame) => frame.Pose switch
    {
        0 => 0xe146,
        1 => 0xe15c,
        2 => 0xe168,
        3 => 0xe16f,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Choot pose."),
    };
}
