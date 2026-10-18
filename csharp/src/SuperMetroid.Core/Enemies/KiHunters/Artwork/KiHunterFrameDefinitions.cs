using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.KiHunters.Artwork;

/// <summary>
/// The presentation boundary of the Ki-Hunter's programs (#641): the bank-$A8 spritemap each
/// <see cref="KiHunterFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class KiHunterFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(KiHunterFrame frame) => frame.Pose switch
    {
        0 => 0xeb2e,
        1 => 0xeb58,
        2 => 0xeb82,
        3 => 0xebac,
        4 => 0xebc2,
        5 => 0xebce,
        6 => 0xebf7,
        7 => 0xec21,
        8 => 0xec4b,
        9 => 0xec75,
        10 => 0xec8b,
        11 => 0xec97,
        12 => 0xeca8,
        13 => 0xecc0,
        14 => 0xecea,
        15 => 0xed14,
        16 => 0xed3e,
        17 => 0xed68,
        18 => 0xed92,
        19 => 0xedbc,
        20 => 0xede6,
        21 => 0xee10,
        22 => 0xee3a,
        23 => 0xee64,
        24 => 0xee8e,
        25 => 0xeeb8,
        26 => 0xeee2,
        27 => 0xef0c,
        28 => 0xef36,
        29 => 0xef60,
        30 => 0xef8a,
        31 => 0xefb4,
        32 => 0xefde,
        33 => 0xf00d,
        34 => 0xf03c,
        35 => 0xf06b,
        36 => 0xf09a,
        37 => 0xf0c4,
        38 => 0xf0f3,
        39 => 0xf122,
        40 => 0xf151,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Ki-Hunter pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(KiHunterFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.KiHunterA8Eb2e,
        { Pose: 1 } => EnemyFrameId.KiHunterA8Eb58,
        { Pose: 10 } => EnemyFrameId.KiHunterA8Ec8b,
        { Pose: 11 } => EnemyFrameId.KiHunterA8Ec97,
        { Pose: 12 } => EnemyFrameId.KiHunterA8Eca8,
        { Pose: 13 } => EnemyFrameId.KiHunterA8Ecc0,
        { Pose: 14 } => EnemyFrameId.KiHunterA8Ecea,
        { Pose: 15 } => EnemyFrameId.KiHunterA8Ed14,
        { Pose: 16 } => EnemyFrameId.KiHunterA8Ed3e,
        { Pose: 17 } => EnemyFrameId.KiHunterA8Ed68,
        { Pose: 18 } => EnemyFrameId.KiHunterA8Ed92,
        { Pose: 19 } => EnemyFrameId.KiHunterA8Edbc,
        { Pose: 2 } => EnemyFrameId.KiHunterA8Eb82,
        { Pose: 20 } => EnemyFrameId.KiHunterA8Ede6,
        { Pose: 21 } => EnemyFrameId.KiHunterA8Ee10,
        { Pose: 22 } => EnemyFrameId.KiHunterA8Ee3a,
        { Pose: 23 } => EnemyFrameId.KiHunterA8Ee64,
        { Pose: 24 } => EnemyFrameId.KiHunterA8Ee8e,
        { Pose: 25 } => EnemyFrameId.KiHunterA8Eeb8,
        { Pose: 26 } => EnemyFrameId.KiHunterA8Eee2,
        { Pose: 27 } => EnemyFrameId.KiHunterA8Ef0c,
        { Pose: 28 } => EnemyFrameId.KiHunterA8Ef36,
        { Pose: 29 } => EnemyFrameId.KiHunterA8Ef60,
        { Pose: 3 } => EnemyFrameId.KiHunterA8Ebac,
        { Pose: 30 } => EnemyFrameId.KiHunterA8Ef8a,
        { Pose: 31 } => EnemyFrameId.KiHunterA8Efb4,
        { Pose: 32 } => EnemyFrameId.KiHunterA8Efde,
        { Pose: 33 } => EnemyFrameId.KiHunterA8F00d,
        { Pose: 34 } => EnemyFrameId.KiHunterA8F03c,
        { Pose: 35 } => EnemyFrameId.KiHunterA8F06b,
        { Pose: 36 } => EnemyFrameId.KiHunterA8F09a,
        { Pose: 37 } => EnemyFrameId.KiHunterA8F0c4,
        { Pose: 38 } => EnemyFrameId.KiHunterA8F0f3,
        { Pose: 39 } => EnemyFrameId.KiHunterA8F122,
        { Pose: 4 } => EnemyFrameId.KiHunterA8Ebc2,
        { Pose: 40 } => EnemyFrameId.KiHunterA8F151,
        { Pose: 5 } => EnemyFrameId.KiHunterA8Ebce,
        { Pose: 6 } => EnemyFrameId.KiHunterA8Ebf7,
        { Pose: 7 } => EnemyFrameId.KiHunterA8Ec21,
        { Pose: 8 } => EnemyFrameId.KiHunterA8Ec4b,
        { Pose: 9 } => EnemyFrameId.KiHunterA8Ec75,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the KiHunterFrame programs show."),
    };
}
