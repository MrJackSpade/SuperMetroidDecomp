using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals.Artwork;

/// <summary>
/// The presentation boundary of the Dachora's programs (#641): the bank-$A7 spritemap each
/// <see cref="DachoraFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class DachoraFrameDefinitions
{

    /// <summary>The installed frame each <see cref="DachoraFrame"/> shows.</summary>
    internal static EnemyFrameTable<DachoraFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.Dachora00 },
        { new(Pose: 1), EnemyFrameId.Dachora01 },
        { new(Pose: 10), EnemyFrameId.Dachora07 },
        { new(Pose: 11), EnemyFrameId.Dachora08 },
        { new(Pose: 12), EnemyFrameId.Dachora09 },
        { new(Pose: 13), EnemyFrameId.Dachora10 },
        { new(Pose: 14), EnemyFrameId.Dachora14 },
        { new(Pose: 15), EnemyFrameId.Dachora15 },
        { new(Pose: 16), EnemyFrameId.Dachora16 },
        { new(Pose: 17), EnemyFrameId.Dachora17 },
        { new(Pose: 18), EnemyFrameId.Dachora18 },
        { new(Pose: 19), EnemyFrameId.Dachora19 },
        { new(Pose: 2), EnemyFrameId.Dachora02 },
        { new(Pose: 20), EnemyFrameId.Dachora27 },
        { new(Pose: 21), EnemyFrameId.Dachora28 },
        { new(Pose: 22), EnemyFrameId.Dachora20 },
        { new(Pose: 23), EnemyFrameId.Dachora25 },
        { new(Pose: 24), EnemyFrameId.Dachora26 },
        { new(Pose: 25), EnemyFrameId.Dachora21 },
        { new(Pose: 26), EnemyFrameId.Dachora22 },
        { new(Pose: 27), EnemyFrameId.Dachora23 },
        { new(Pose: 28), EnemyFrameId.Dachora24 },
        { new(Pose: 3), EnemyFrameId.Dachora03 },
        { new(Pose: 4), EnemyFrameId.Dachora04 },
        { new(Pose: 5), EnemyFrameId.Dachora05 },
        { new(Pose: 6), EnemyFrameId.Dachora13 },
        { new(Pose: 7), EnemyFrameId.Dachora06 },
        { new(Pose: 8), EnemyFrameId.Dachora11 },
        { new(Pose: 9), EnemyFrameId.Dachora12 },
    };
}
