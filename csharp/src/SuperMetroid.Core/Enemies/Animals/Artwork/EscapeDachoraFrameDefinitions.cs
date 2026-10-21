using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals.Artwork;

/// <summary>
/// The presentation boundary of the escaping Dachora's programs (#641): the bank-$B3 spritemap each
/// <see cref="EscapeDachoraFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class EscapeDachoraFrameDefinitions
{

    /// <summary>The installed frame each <see cref="EscapeDachoraFrame"/> shows.</summary>
    internal static EnemyFrameTable<EscapeDachoraFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.EscapeDachora00 },
        { new(Pose: 1), EnemyFrameId.EscapeDachora01 },
        { new(Pose: 10), EnemyFrameId.EscapeDachora10 },
        { new(Pose: 11), EnemyFrameId.EscapeDachora11 },
        { new(Pose: 2), EnemyFrameId.EscapeDachora02 },
        { new(Pose: 3), EnemyFrameId.EscapeDachora03 },
        { new(Pose: 4), EnemyFrameId.EscapeDachora04 },
        { new(Pose: 5), EnemyFrameId.EscapeDachora05 },
        { new(Pose: 6), EnemyFrameId.EscapeDachora06 },
        { new(Pose: 7), EnemyFrameId.EscapeDachora07 },
        { new(Pose: 8), EnemyFrameId.EscapeDachora08 },
        { new(Pose: 9), EnemyFrameId.EscapeDachora09 },
    };
}
