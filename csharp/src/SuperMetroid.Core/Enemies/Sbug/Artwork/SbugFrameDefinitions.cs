using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Sbug.Artwork;

/// <summary>
/// The presentation boundary of Sbug's programs (#641): the bank-$A3 spritemap each
/// <see cref="SbugFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class SbugFrameDefinitions
{

    /// <summary>The installed frame each <see cref="SbugFrame"/> shows.</summary>
    internal static EnemyFrameTable<SbugFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.SbugRight0 },
        { new(Pose: 1), EnemyFrameId.SbugRight1 },
        { new(Pose: 10), EnemyFrameId.SbugUpLeft1 },
        { new(Pose: 11), EnemyFrameId.SbugUpLeft2 },
        { new(Pose: 12), EnemyFrameId.SbugLeft0 },
        { new(Pose: 13), EnemyFrameId.SbugLeft1 },
        { new(Pose: 14), EnemyFrameId.SbugLeft2 },
        { new(Pose: 15), EnemyFrameId.SbugDownLeft0 },
        { new(Pose: 16), EnemyFrameId.SbugDownLeft1 },
        { new(Pose: 17), EnemyFrameId.SbugDownLeft2 },
        { new(Pose: 18), EnemyFrameId.SbugDown0 },
        { new(Pose: 19), EnemyFrameId.SbugDown1 },
        { new(Pose: 2), EnemyFrameId.SbugRight2 },
        { new(Pose: 20), EnemyFrameId.SbugDown2 },
        { new(Pose: 21), EnemyFrameId.SbugDownRight0 },
        { new(Pose: 22), EnemyFrameId.SbugDownRight1 },
        { new(Pose: 23), EnemyFrameId.SbugDownRight2 },
        { new(Pose: 3), EnemyFrameId.SbugUpRight0 },
        { new(Pose: 4), EnemyFrameId.SbugUpRight1 },
        { new(Pose: 5), EnemyFrameId.SbugUpRight2 },
        { new(Pose: 6), EnemyFrameId.SbugUp0 },
        { new(Pose: 7), EnemyFrameId.SbugUp1 },
        { new(Pose: 8), EnemyFrameId.SbugUp2 },
        { new(Pose: 9), EnemyFrameId.SbugUpLeft0 },
    };
}
