using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Metroids.Artwork;

/// <summary>
/// The presentation boundary of the Metroid's programs (#641): the bank-$A3 spritemap each
/// <see cref="MetroidFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class MetroidFrameDefinitions
{

    /// <summary>The installed frame each <see cref="MetroidFrame"/> shows.</summary>
    internal static EnemyFrameTable<MetroidFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.MetroidBody0 },
        { new(Pose: 1), EnemyFrameId.MetroidBody1 },
        { new(Pose: 2), EnemyFrameId.MetroidBody2 },
        { new(Pose: 3), EnemyFrameId.MetroidBody3 },
    };
}
