using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios.Artwork;

/// <summary>
/// The presentation boundary of the Norfair Rio's programs (#641): the bank-$A2 spritemap each
/// <see cref="NorfairRioFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class NorfairRioFrameDefinitions
{

    /// <summary>The installed frame each <see cref="NorfairRioFrame"/> shows.</summary>
    internal static EnemyFrameTable<NorfairRioFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.NorfairRioC427 },
        { new(Pose: 1), EnemyFrameId.NorfairRioC442 },
        { new(Pose: 10), EnemyFrameId.NorfairRioC523 },
        { new(Pose: 11), EnemyFrameId.NorfairRioC534 },
        { new(Pose: 12), EnemyFrameId.NorfairRioC54f },
        { new(Pose: 13), EnemyFrameId.NorfairRioC56a },
        { new(Pose: 14), EnemyFrameId.NorfairRioC585 },
        { new(Pose: 15), EnemyFrameId.NorfairRioC5a0 },
        { new(Pose: 16), EnemyFrameId.NorfairRioC5bb },
        { new(Pose: 17), EnemyFrameId.NorfairRioC5d6 },
        { new(Pose: 18), EnemyFrameId.NorfairRioC5e2 },
        { new(Pose: 19), EnemyFrameId.NorfairRioC5ee },
        { new(Pose: 2), EnemyFrameId.NorfairRioC45d },
        { new(Pose: 3), EnemyFrameId.NorfairRioC493 },
        { new(Pose: 4), EnemyFrameId.NorfairRioC49f },
        { new(Pose: 5), EnemyFrameId.NorfairRioC4ab },
        { new(Pose: 6), EnemyFrameId.NorfairRioC4b7 },
        { new(Pose: 7), EnemyFrameId.NorfairRioC4d2 },
        { new(Pose: 8), EnemyFrameId.NorfairRioC4ed },
        { new(Pose: 9), EnemyFrameId.NorfairRioC508 },
    };
}
