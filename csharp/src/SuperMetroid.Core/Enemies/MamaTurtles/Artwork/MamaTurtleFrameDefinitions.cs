using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MamaTurtles.Artwork;

/// <summary>
/// The presentation boundary of the turtles' programs (#641): the bank-$A2 spritemap each
/// <see cref="MamaTurtleFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class MamaTurtleFrameDefinitions
{

    /// <summary>The installed frame each <see cref="MamaTurtleFrame"/> shows.</summary>
    internal static EnemyFrameTable<MamaTurtleFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.Tatori00 },
        { new(Pose: 1), EnemyFrameId.Tatori01 },
        { new(Pose: 10), EnemyFrameId.Tatori10 },
        { new(Pose: 11), EnemyFrameId.Tatori11 },
        { new(Pose: 12), EnemyFrameId.Tatori16 },
        { new(Pose: 13), EnemyFrameId.Tatori15 },
        { new(Pose: 14), EnemyFrameId.Tatori14 },
        { new(Pose: 15), EnemyFrameId.Tatori13 },
        { new(Pose: 16), EnemyFrameId.Tatori12 },
        { new(Pose: 17), EnemyFrameId.Tatori17 },
        { new(Pose: 18), EnemyFrameId.Tatori18 },
        { new(Pose: 19), EnemyFrameId.Tatori19 },
        { new(Pose: 2), EnemyFrameId.Tatori02 },
        { new(Pose: 20), EnemyFrameId.Tatori20 },
        { new(Pose: 21), EnemyFrameId.Tatori23 },
        { new(Pose: 22), EnemyFrameId.Tatori21 },
        { new(Pose: 23), EnemyFrameId.Tatori22 },
        { new(Pose: 24), EnemyFrameId.Tatori28 },
        { new(Pose: 25), EnemyFrameId.Tatori27 },
        { new(Pose: 26), EnemyFrameId.Tatori26 },
        { new(Pose: 27), EnemyFrameId.Tatori25 },
        { new(Pose: 28), EnemyFrameId.Tatori24 },
        { new(Pose: 3), EnemyFrameId.Tatori03 },
        { new(Pose: 4), EnemyFrameId.Tatori06 },
        { new(Pose: 5), EnemyFrameId.Tatori04 },
        { new(Pose: 6), EnemyFrameId.Tatori05 },
        { new(Pose: 7), EnemyFrameId.Tatori07 },
        { new(Pose: 8), EnemyFrameId.Tatori08 },
        { new(Pose: 9), EnemyFrameId.Tatori09 },
    };
}
