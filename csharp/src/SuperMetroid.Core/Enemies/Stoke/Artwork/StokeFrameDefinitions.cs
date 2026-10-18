using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Stoke.Artwork;

/// <summary>
/// The presentation boundary of Stoke's programs (#641): the bank-$A2 spritemap each
/// <see cref="StokeFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class StokeFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(StokeFrame frame) => frame.Pose switch
    {
        0 => 0x8aca,
        1 => 0x8ad6,
        2 => 0x8ae7,
        3 => 0x8af3,
        4 => 0x8aff,
        5 => 0x8b15,
        6 => 0x8b21,
        7 => 0x8b32,
        8 => 0x8b3e,
        9 => 0x8b4a,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Stoke pose."),
    };

    /// <summary>The installed frame each <see cref="StokeFrame"/> shows.</summary>
    internal static EnemyFrameTable<StokeFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.StokeWalkLeft0 },
        { new(Pose: 1), EnemyFrameId.StokeWalkLeft1 },
        { new(Pose: 2), EnemyFrameId.StokeWalkLeft2 },
        { new(Pose: 3), EnemyFrameId.StokeWalkLeft3 },
        { new(Pose: 4), EnemyFrameId.StokeAttackLeft },
        { new(Pose: 5), EnemyFrameId.StokeWalkRight0 },
        { new(Pose: 6), EnemyFrameId.StokeWalkRight1 },
        { new(Pose: 7), EnemyFrameId.StokeWalkRight2 },
        { new(Pose: 8), EnemyFrameId.StokeWalkRight3 },
        { new(Pose: 9), EnemyFrameId.StokeAttackRight },
    };
}
