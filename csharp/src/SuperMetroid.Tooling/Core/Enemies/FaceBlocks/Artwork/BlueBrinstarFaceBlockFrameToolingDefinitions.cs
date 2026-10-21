namespace SuperMetroid.Core.Enemies.FaceBlocks.Artwork;

/// <summary>Development-tool members of <see cref="BlueBrinstarFaceBlockFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class BlueBrinstarFaceBlockFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa8;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(BlueBrinstarFaceBlockFrame frame) => frame.Pose switch
    {
        0 => 0xe92c,
        1 => 0xe942,
        2 => 0xe958,
        3 => 0xe96e,
        4 => 0xe984,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Blue Brinstar face block pose."),
    };
}
