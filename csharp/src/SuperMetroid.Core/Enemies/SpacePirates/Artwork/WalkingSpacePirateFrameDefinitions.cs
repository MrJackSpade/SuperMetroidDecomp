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

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(WalkingSpacePirateFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.WalkingPirateWalkLeft0,
        { Pose: 1 } => EnemyFrameId.WalkingPirateWalkLeft1,
        { Pose: 10 } => EnemyFrameId.WalkingPirateWalkRight2,
        { Pose: 11 } => EnemyFrameId.WalkingPirateWalkRight3,
        { Pose: 12 } => EnemyFrameId.WalkingPirateWalkRight4,
        { Pose: 13 } => EnemyFrameId.WalkingPirateWalkRight5,
        { Pose: 14 } => EnemyFrameId.WalkingPirateWalkRight6,
        { Pose: 15 } => EnemyFrameId.WalkingPirateWalkRight7,
        { Pose: 16 } => EnemyFrameId.WalkingPirateFireLeft0,
        { Pose: 17 } => EnemyFrameId.WalkingPirateFireLeft1,
        { Pose: 18 } => EnemyFrameId.WalkingPirateFireLeft2,
        { Pose: 19 } => EnemyFrameId.WalkingPirateFireLeft3,
        { Pose: 2 } => EnemyFrameId.WalkingPirateWalkLeft2,
        { Pose: 20 } => EnemyFrameId.WalkingPirateFireLeft4,
        { Pose: 21 } => EnemyFrameId.WalkingPirateFireLeft5,
        { Pose: 22 } => EnemyFrameId.WalkingPirateFireRight0,
        { Pose: 23 } => EnemyFrameId.WalkingPirateFireRight1,
        { Pose: 24 } => EnemyFrameId.WalkingPirateFireRight2,
        { Pose: 25 } => EnemyFrameId.WalkingPirateFireRight3,
        { Pose: 26 } => EnemyFrameId.WalkingPirateFireRight4,
        { Pose: 27 } => EnemyFrameId.WalkingPirateFireRight5,
        { Pose: 28 } => EnemyFrameId.WalkingPirateLookLeft0,
        { Pose: 29 } => EnemyFrameId.WalkingPirateLookLeft1,
        { Pose: 3 } => EnemyFrameId.WalkingPirateWalkLeft3,
        { Pose: 30 } => EnemyFrameId.WalkingPirateLookLeft2,
        { Pose: 31 } => EnemyFrameId.WalkingPirateLookRight0,
        { Pose: 32 } => EnemyFrameId.WalkingPirateLookRight1,
        { Pose: 33 } => EnemyFrameId.WalkingPirateLookRight2,
        { Pose: 34 } => EnemyFrameId.WalkingPirateLookShared,
        { Pose: 35 } => EnemyFrameId.WalkingPirateFlinchLeft,
        { Pose: 36 } => EnemyFrameId.WalkingPirateFlinchRight,
        { Pose: 4 } => EnemyFrameId.WalkingPirateWalkLeft4,
        { Pose: 5 } => EnemyFrameId.WalkingPirateWalkLeft5,
        { Pose: 6 } => EnemyFrameId.WalkingPirateWalkLeft6,
        { Pose: 7 } => EnemyFrameId.WalkingPirateWalkLeft7,
        { Pose: 8 } => EnemyFrameId.WalkingPirateWalkRight0,
        { Pose: 9 } => EnemyFrameId.WalkingPirateWalkRight1,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the WalkingSpacePirateFrame programs show."),
    };
}
