using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates.Artwork;

/// <summary>
/// The presentation boundary of the walking Space Pirates' programs (#641): the bank-$B2 spritemap each
/// <see cref="WalkingSpacePirateFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class WalkingSpacePirateFrameDefinitions
{

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
