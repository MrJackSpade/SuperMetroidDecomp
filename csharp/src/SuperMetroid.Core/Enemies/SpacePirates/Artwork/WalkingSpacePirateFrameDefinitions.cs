using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates.Artwork;

/// <summary>
/// The presentation boundary of the walking Space Pirates' programs (#641): the bank-$B2 spritemap each
/// <see cref="WalkingSpacePirateFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class WalkingSpacePirateFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(WalkingSpacePirateFrame frame) => frame.Pose switch
    {
        0 => 0x89c4,
        1 => 0x89d6,
        2 => 0x89e8,
        3 => 0x89fa,
        4 => 0x8a0c,
        5 => 0x8a1e,
        6 => 0x8a30,
        7 => 0x8a42,
        8 => 0x8af6,
        9 => 0x8b08,
        10 => 0x8b1a,
        11 => 0x8b2c,
        12 => 0x8b3e,
        13 => 0x8b50,
        14 => 0x8b62,
        15 => 0x8b74,
        16 => 0x8c28,
        17 => 0x8c3a,
        18 => 0x8c4c,
        19 => 0x8c5e,
        20 => 0x8c70,
        21 => 0x8c82,
        22 => 0x8c94,
        23 => 0x8ca6,
        24 => 0x8cb8,
        25 => 0x8cca,
        26 => 0x8cdc,
        27 => 0x8cee,
        28 => 0x8d00,
        29 => 0x8d1a,
        30 => 0x8d2c,
        31 => 0x8d46,
        32 => 0x8d60,
        33 => 0x8d72,
        34 => 0x8d8c,
        35 => 0x8f92,
        36 => 0x8fa6,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Walking Space Pirate pose."),
    };

    /// <summary>The installed frame each <see cref="WalkingSpacePirateFrame"/> shows.</summary>
    internal static EnemyFrameTable<WalkingSpacePirateFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.WalkingPirateWalkLeft0 },
        { new(Pose: 1), EnemyFrameId.WalkingPirateWalkLeft1 },
        { new(Pose: 10), EnemyFrameId.WalkingPirateWalkRight2 },
        { new(Pose: 11), EnemyFrameId.WalkingPirateWalkRight3 },
        { new(Pose: 12), EnemyFrameId.WalkingPirateWalkRight4 },
        { new(Pose: 13), EnemyFrameId.WalkingPirateWalkRight5 },
        { new(Pose: 14), EnemyFrameId.WalkingPirateWalkRight6 },
        { new(Pose: 15), EnemyFrameId.WalkingPirateWalkRight7 },
        { new(Pose: 16), EnemyFrameId.WalkingPirateFireLeft0 },
        { new(Pose: 17), EnemyFrameId.WalkingPirateFireLeft1 },
        { new(Pose: 18), EnemyFrameId.WalkingPirateFireLeft2 },
        { new(Pose: 19), EnemyFrameId.WalkingPirateFireLeft3 },
        { new(Pose: 2), EnemyFrameId.WalkingPirateWalkLeft2 },
        { new(Pose: 20), EnemyFrameId.WalkingPirateFireLeft4 },
        { new(Pose: 21), EnemyFrameId.WalkingPirateFireLeft5 },
        { new(Pose: 22), EnemyFrameId.WalkingPirateFireRight0 },
        { new(Pose: 23), EnemyFrameId.WalkingPirateFireRight1 },
        { new(Pose: 24), EnemyFrameId.WalkingPirateFireRight2 },
        { new(Pose: 25), EnemyFrameId.WalkingPirateFireRight3 },
        { new(Pose: 26), EnemyFrameId.WalkingPirateFireRight4 },
        { new(Pose: 27), EnemyFrameId.WalkingPirateFireRight5 },
        { new(Pose: 28), EnemyFrameId.WalkingPirateLookLeft0 },
        { new(Pose: 29), EnemyFrameId.WalkingPirateLookLeft1 },
        { new(Pose: 3), EnemyFrameId.WalkingPirateWalkLeft3 },
        { new(Pose: 30), EnemyFrameId.WalkingPirateLookLeft2 },
        { new(Pose: 31), EnemyFrameId.WalkingPirateLookRight0 },
        { new(Pose: 32), EnemyFrameId.WalkingPirateLookRight1 },
        { new(Pose: 33), EnemyFrameId.WalkingPirateLookRight2 },
        { new(Pose: 34), EnemyFrameId.WalkingPirateLookShared },
        { new(Pose: 35), EnemyFrameId.WalkingPirateFlinchLeft },
        { new(Pose: 36), EnemyFrameId.WalkingPirateFlinchRight },
        { new(Pose: 4), EnemyFrameId.WalkingPirateWalkLeft4 },
        { new(Pose: 5), EnemyFrameId.WalkingPirateWalkLeft5 },
        { new(Pose: 6), EnemyFrameId.WalkingPirateWalkLeft6 },
        { new(Pose: 7), EnemyFrameId.WalkingPirateWalkLeft7 },
        { new(Pose: 8), EnemyFrameId.WalkingPirateWalkRight0 },
        { new(Pose: 9), EnemyFrameId.WalkingPirateWalkRight1 },
    };
}
