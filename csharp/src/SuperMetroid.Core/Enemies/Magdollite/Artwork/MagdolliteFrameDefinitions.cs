using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Magdollite.Artwork;

/// <summary>
/// The presentation boundary of the Magdollite's programs (#641): the bank-$A8 spritemap each
/// <see cref="MagdolliteFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class MagdolliteFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(MagdolliteFrame frame) => frame.Pose switch
    {
        0 => 0xb448,
        1 => 0xb459,
        2 => 0xb46a,
        3 => 0xb47b,
        4 => 0xb48c,
        5 => 0xb49d,
        6 => 0xb4a9,
        7 => 0xb4b5,
        8 => 0xb4c1,
        9 => 0xb4cf,
        10 => 0xb4e0,
        11 => 0xb4f1,
        12 => 0xb502,
        13 => 0xb513,
        14 => 0xb524,
        15 => 0xb535,
        16 => 0xb546,
        17 => 0xb552,
        18 => 0xb56a,
        19 => 0xb578,
        20 => 0xb589,
        21 => 0xb59a,
        22 => 0xb5a1,
        23 => 0xb5ad,
        24 => 0xb5be,
        25 => 0xb5d4,
        26 => 0xb5ef,
        27 => 0xb60f,
        28 => 0xb634,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Magdollite pose."),
    };

    /// <summary>The installed frame each <see cref="MagdolliteFrame"/> shows.</summary>
    internal static EnemyFrameTable<MagdolliteFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.MagdolliteLeftIdle0 },
        { new(Pose: 1), EnemyFrameId.MagdolliteLeftIdle1 },
        { new(Pose: 10), EnemyFrameId.MagdolliteLeftSubmerge2 },
        { new(Pose: 11), EnemyFrameId.MagdolliteRightIdle0 },
        { new(Pose: 12), EnemyFrameId.MagdolliteRightIdle1 },
        { new(Pose: 13), EnemyFrameId.MagdolliteRightIdle2 },
        { new(Pose: 14), EnemyFrameId.MagdolliteRightThrow0 },
        { new(Pose: 15), EnemyFrameId.MagdolliteRightThrow1 },
        { new(Pose: 16), EnemyFrameId.MagdolliteRightThrow2 },
        { new(Pose: 17), EnemyFrameId.MagdolliteRightThrow3 },
        { new(Pose: 18), EnemyFrameId.MagdolliteRightSubmerge0 },
        { new(Pose: 19), EnemyFrameId.MagdolliteRightSubmerge1 },
        { new(Pose: 2), EnemyFrameId.MagdolliteLeftIdle2 },
        { new(Pose: 20), EnemyFrameId.MagdolliteRightSubmerge2 },
        { new(Pose: 21), EnemyFrameId.MagdollitePillarPhase0 },
        { new(Pose: 22), EnemyFrameId.MagdollitePillarPhase1 },
        { new(Pose: 23), EnemyFrameId.MagdollitePillarPhase2 },
        { new(Pose: 24), EnemyFrameId.MagdollitePillarPhase3 },
        { new(Pose: 25), EnemyFrameId.MagdollitePillarPhase4 },
        { new(Pose: 26), EnemyFrameId.MagdollitePillarPhase5 },
        { new(Pose: 27), EnemyFrameId.MagdollitePillarPhase6 },
        { new(Pose: 28), EnemyFrameId.MagdollitePillarPhase7 },
        { new(Pose: 3), EnemyFrameId.MagdolliteLeftThrow0 },
        { new(Pose: 4), EnemyFrameId.MagdolliteLeftThrow1 },
        { new(Pose: 5), EnemyFrameId.MagdolliteLeftThrow2 },
        { new(Pose: 6), EnemyFrameId.MagdolliteLeftThrow3 },
        { new(Pose: 7), EnemyFrameId.MagdollitePillarCap },
        { new(Pose: 8), EnemyFrameId.MagdolliteLeftSubmerge0 },
        { new(Pose: 9), EnemyFrameId.MagdolliteLeftSubmerge1 },
    };
}
