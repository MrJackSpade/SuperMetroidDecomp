using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals.Artwork;

/// <summary>
/// The presentation boundary of the escaping Dachora's programs (#641): the bank-$B3 spritemap each
/// <see cref="EscapeDachoraFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class EscapeDachoraFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(EscapeDachoraFrame frame) => frame.Pose switch
    {
        0 => 0xeb1b,
        1 => 0xeb4a,
        2 => 0xeb79,
        3 => 0xebb2,
        4 => 0xebe1,
        5 => 0xec10,
        6 => 0xec49,
        7 => 0xec78,
        8 => 0xeca7,
        9 => 0xece0,
        10 => 0xed0f,
        11 => 0xed3e,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined escape Dachora pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(EscapeDachoraFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.EscapeDachora00,
        { Pose: 1 } => EnemyFrameId.EscapeDachora01,
        { Pose: 10 } => EnemyFrameId.EscapeDachora10,
        { Pose: 11 } => EnemyFrameId.EscapeDachora11,
        { Pose: 2 } => EnemyFrameId.EscapeDachora02,
        { Pose: 3 } => EnemyFrameId.EscapeDachora03,
        { Pose: 4 } => EnemyFrameId.EscapeDachora04,
        { Pose: 5 } => EnemyFrameId.EscapeDachora05,
        { Pose: 6 } => EnemyFrameId.EscapeDachora06,
        { Pose: 7 } => EnemyFrameId.EscapeDachora07,
        { Pose: 8 } => EnemyFrameId.EscapeDachora08,
        { Pose: 9 } => EnemyFrameId.EscapeDachora09,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the EscapeDachoraFrame programs show."),
    };
}
