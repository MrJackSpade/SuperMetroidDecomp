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

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(MagdolliteFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.MagdolliteLeftIdle0,
        { Pose: 1 } => EnemyFrameId.MagdolliteLeftIdle1,
        { Pose: 10 } => EnemyFrameId.MagdolliteLeftSubmerge2,
        { Pose: 11 } => EnemyFrameId.MagdolliteRightIdle0,
        { Pose: 12 } => EnemyFrameId.MagdolliteRightIdle1,
        { Pose: 13 } => EnemyFrameId.MagdolliteRightIdle2,
        { Pose: 14 } => EnemyFrameId.MagdolliteRightThrow0,
        { Pose: 15 } => EnemyFrameId.MagdolliteRightThrow1,
        { Pose: 16 } => EnemyFrameId.MagdolliteRightThrow2,
        { Pose: 17 } => EnemyFrameId.MagdolliteRightThrow3,
        { Pose: 18 } => EnemyFrameId.MagdolliteRightSubmerge0,
        { Pose: 19 } => EnemyFrameId.MagdolliteRightSubmerge1,
        { Pose: 2 } => EnemyFrameId.MagdolliteLeftIdle2,
        { Pose: 20 } => EnemyFrameId.MagdolliteRightSubmerge2,
        { Pose: 21 } => EnemyFrameId.MagdollitePillarPhase0,
        { Pose: 22 } => EnemyFrameId.MagdollitePillarPhase1,
        { Pose: 23 } => EnemyFrameId.MagdollitePillarPhase2,
        { Pose: 24 } => EnemyFrameId.MagdollitePillarPhase3,
        { Pose: 25 } => EnemyFrameId.MagdollitePillarPhase4,
        { Pose: 26 } => EnemyFrameId.MagdollitePillarPhase5,
        { Pose: 27 } => EnemyFrameId.MagdollitePillarPhase6,
        { Pose: 28 } => EnemyFrameId.MagdollitePillarPhase7,
        { Pose: 3 } => EnemyFrameId.MagdolliteLeftThrow0,
        { Pose: 4 } => EnemyFrameId.MagdolliteLeftThrow1,
        { Pose: 5 } => EnemyFrameId.MagdolliteLeftThrow2,
        { Pose: 6 } => EnemyFrameId.MagdolliteLeftThrow3,
        { Pose: 7 } => EnemyFrameId.MagdollitePillarCap,
        { Pose: 8 } => EnemyFrameId.MagdolliteLeftSubmerge0,
        { Pose: 9 } => EnemyFrameId.MagdolliteLeftSubmerge1,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the MagdolliteFrame programs show."),
    };
}
