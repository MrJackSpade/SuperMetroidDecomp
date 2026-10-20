namespace SuperMetroid.Core.Enemies.Corpses.Artwork;

/// <summary>Development-tool members of <see cref="DeadTorizoFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class DeadTorizoFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa9;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(DeadTorizoFrame frame) => frame.Pose switch
    {
        0 => 0xd6e2,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Dead Torizo pose."),
    };
}
