using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios.Artwork;

/// <summary>
/// The presentation boundary of the Lower Norfair Rio's programs (#641): the bank-$A2 spritemap each
/// <see cref="LowerNorfairRioFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class LowerNorfairRioFrameDefinitions
{

    /// <summary>The installed frame each <see cref="LowerNorfairRioFrame"/> shows.</summary>
    internal static EnemyFrameTable<LowerNorfairRioFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.LowerNorfairRioC8bd },
        { new(Pose: 1), EnemyFrameId.LowerNorfairRioC8d3 },
        { new(Pose: 10), EnemyFrameId.LowerNorfairRioC999 },
        { new(Pose: 11), EnemyFrameId.LowerNorfairRioC9af },
        { new(Pose: 12), EnemyFrameId.LowerNorfairRioC9c5 },
        { new(Pose: 13), EnemyFrameId.LowerNorfairRioC9db },
        { new(Pose: 14), EnemyFrameId.LowerNorfairRioC9f1 },
        { new(Pose: 15), EnemyFrameId.LowerNorfairRioCa07 },
        { new(Pose: 16), EnemyFrameId.LowerNorfairRioCa13 },
        { new(Pose: 17), EnemyFrameId.LowerNorfairRioCa1f },
        { new(Pose: 2), EnemyFrameId.LowerNorfairRioC8e9 },
        { new(Pose: 3), EnemyFrameId.LowerNorfairRioC8ff },
        { new(Pose: 4), EnemyFrameId.LowerNorfairRioC915 },
        { new(Pose: 5), EnemyFrameId.LowerNorfairRioC92b },
        { new(Pose: 6), EnemyFrameId.LowerNorfairRioC941 },
        { new(Pose: 7), EnemyFrameId.LowerNorfairRioC957 },
        { new(Pose: 8), EnemyFrameId.LowerNorfairRioC96d },
        { new(Pose: 9), EnemyFrameId.LowerNorfairRioC983 },
    };
}
