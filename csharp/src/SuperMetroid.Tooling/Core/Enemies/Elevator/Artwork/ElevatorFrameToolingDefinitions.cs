namespace SuperMetroid.Core.Enemies.Elevator.Artwork;

/// <summary>Development-tool members of <see cref="ElevatorFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class ElevatorFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa3;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(ElevatorFrame frame) => frame.Pose switch
    {
        0 => 0x962f,
        1 => 0x9645,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined elevator pose."),
    };
}
