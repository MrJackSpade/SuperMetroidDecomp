using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios.Artwork;

/// <summary>
/// The presentation boundary of the Rio's programs (#641): the bank-$A2 spritemap each
/// <see cref="RioFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class RioFrameDefinitions
{

    /// <summary>The installed frame each <see cref="RioFrame"/> shows.</summary>
    internal static EnemyFrameTable<RioFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.RioPose0 },
        { new(Pose: 1), EnemyFrameId.RioPose1 },
        { new(Pose: 2), EnemyFrameId.RioPose2 },
        { new(Pose: 3), EnemyFrameId.RioPose3 },
        { new(Pose: 4), EnemyFrameId.RioPose4 },
        { new(Pose: 5), EnemyFrameId.RioPose5 },
        { new(Pose: 6), EnemyFrameId.RioPose6 },
        { new(Pose: 7), EnemyFrameId.RioPose7 },
    };
}
