using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Metroids.Artwork;

/// <summary>
/// The presentation boundary of Mochtroid's programs (#641): the bank-$A3 spritemap each
/// <see cref="MochtroidFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class MochtroidFrameDefinitions
{

    /// <summary>The installed frame each <see cref="MochtroidFrame"/> shows.</summary>
    internal static EnemyFrameTable<MochtroidFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.MochtroidFlight0 },
        { new(Pose: 1), EnemyFrameId.MochtroidFlight1 },
        { new(Pose: 2), EnemyFrameId.MochtroidFlight2 },
        { new(Pose: 3), EnemyFrameId.MochtroidAttached0 },
        { new(Pose: 4), EnemyFrameId.MochtroidAttached1 },
        { new(Pose: 5), EnemyFrameId.MochtroidAttached2 },
    };
}
