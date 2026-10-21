using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Hibashi.Artwork;

/// <summary>
/// The presentation boundary of the Hibashi's programs (#641): the bank-$A6 spritemap each
/// <see cref="HibashiFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class HibashiFrameDefinitions
{

    /// <summary>The installed frame each <see cref="HibashiFrame"/> shows.</summary>
    internal static EnemyFrameTable<HibashiFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.Hibashi00 },
        { new(Pose: 1), EnemyFrameId.Hibashi01 },
        { new(Pose: 10), EnemyFrameId.Hibashi10 },
        { new(Pose: 11), EnemyFrameId.Hibashi11 },
        { new(Pose: 12), EnemyFrameId.Hibashi12 },
        { new(Pose: 13), EnemyFrameId.Hibashi13 },
        { new(Pose: 14), EnemyFrameId.Hibashi14 },
        { new(Pose: 15), EnemyFrameId.Hibashi15 },
        { new(Pose: 16), EnemyFrameId.Hibashi16 },
        { new(Pose: 17), EnemyFrameId.Hibashi17 },
        { new(Pose: 18), EnemyFrameId.Hibashi18 },
        { new(Pose: 19), EnemyFrameId.Hibashi19 },
        { new(Pose: 2), EnemyFrameId.Hibashi02 },
        { new(Pose: 20), EnemyFrameId.Hibashi20 },
        { new(Pose: 21), EnemyFrameId.Hibashi21 },
        { new(Pose: 22), EnemyFrameId.Hibashi22 },
        { new(Pose: 3), EnemyFrameId.Hibashi03 },
        { new(Pose: 4), EnemyFrameId.Hibashi04 },
        { new(Pose: 5), EnemyFrameId.Hibashi05 },
        { new(Pose: 6), EnemyFrameId.Hibashi06 },
        { new(Pose: 7), EnemyFrameId.Hibashi07 },
        { new(Pose: 8), EnemyFrameId.Hibashi08 },
        { new(Pose: 9), EnemyFrameId.Hibashi09 },
    };
}
