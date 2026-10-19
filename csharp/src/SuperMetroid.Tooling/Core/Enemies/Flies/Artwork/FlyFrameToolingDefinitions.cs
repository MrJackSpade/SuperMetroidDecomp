using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Enemies.Flies.Artwork;

/// <summary>Development-tool members of <see cref="FlyFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class FlyFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(FlyFrame frame) => frame.Pose switch
    {
        0 => 0xb1e8,
        1 => 0xb1ef,
        2 => 0xb1f6,
        3 => 0xb1fd,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined fly-family pose."),
    };
}
