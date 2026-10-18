using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Elevator.Artwork;

/// <summary>
/// The presentation boundary of the elevator's programs (#641): the bank-$A3 spritemap each
/// <see cref="ElevatorFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class ElevatorFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(ElevatorFrame frame) => frame.Pose switch
    {
        0 => 0x962f,
        1 => 0x9645,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined elevator pose."),
    };

    /// <summary>The installed frame each <see cref="ElevatorFrame"/> shows.</summary>
    internal static EnemyFrameTable<ElevatorFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.ElevatorPlatform0 },
        { new(Pose: 1), EnemyFrameId.ElevatorPlatform1 },
    };
}
