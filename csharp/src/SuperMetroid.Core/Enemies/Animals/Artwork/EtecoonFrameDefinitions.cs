using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals.Artwork;

/// <summary>
/// The presentation boundary of the Etecoon's programs (#641): the bank-$A7 spritemap each
/// <see cref="EtecoonFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class EtecoonFrameDefinitions
{

    /// <summary>The installed frame each <see cref="EtecoonFrame"/> shows.</summary>
    internal static EnemyFrameTable<EtecoonFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.Etecoon02 },
        { new(Pose: 1), EnemyFrameId.Etecoon03 },
        { new(Pose: 10), EnemyFrameId.Etecoon12 },
        { new(Pose: 11), EnemyFrameId.Etecoon13 },
        { new(Pose: 12), EnemyFrameId.Etecoon00 },
        { new(Pose: 13), EnemyFrameId.Etecoon01 },
        { new(Pose: 14), EnemyFrameId.Etecoon14 },
        { new(Pose: 15), EnemyFrameId.Etecoon17 },
        { new(Pose: 16), EnemyFrameId.Etecoon18 },
        { new(Pose: 17), EnemyFrameId.Etecoon19 },
        { new(Pose: 18), EnemyFrameId.Etecoon21 },
        { new(Pose: 19), EnemyFrameId.Etecoon22 },
        { new(Pose: 2), EnemyFrameId.Etecoon04 },
        { new(Pose: 20), EnemyFrameId.Etecoon23 },
        { new(Pose: 21), EnemyFrameId.Etecoon24 },
        { new(Pose: 22), EnemyFrameId.Etecoon20 },
        { new(Pose: 23), EnemyFrameId.Etecoon25 },
        { new(Pose: 24), EnemyFrameId.Etecoon26 },
        { new(Pose: 25), EnemyFrameId.Etecoon27 },
        { new(Pose: 26), EnemyFrameId.Etecoon28 },
        { new(Pose: 27), EnemyFrameId.Etecoon30 },
        { new(Pose: 28), EnemyFrameId.Etecoon16 },
        { new(Pose: 29), EnemyFrameId.Etecoon15 },
        { new(Pose: 3), EnemyFrameId.Etecoon06 },
        { new(Pose: 30), EnemyFrameId.Etecoon29 },
        { new(Pose: 4), EnemyFrameId.Etecoon07 },
        { new(Pose: 5), EnemyFrameId.Etecoon08 },
        { new(Pose: 6), EnemyFrameId.Etecoon09 },
        { new(Pose: 7), EnemyFrameId.Etecoon05 },
        { new(Pose: 8), EnemyFrameId.Etecoon10 },
        { new(Pose: 9), EnemyFrameId.Etecoon11 },
    };
}
