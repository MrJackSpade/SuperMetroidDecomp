using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.FuneNamihe.Artwork;

/// <summary>
/// The presentation boundary of the Fune's and Namihe's programs (#641): the bank-$A8 spritemap each
/// <see cref="FuneNamiheFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class FuneNamiheFrameDefinitions
{

    /// <summary>The installed frame each <see cref="FuneNamiheFrame"/> shows.</summary>
    internal static EnemyFrameTable<FuneNamiheFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.FuneLeftIdle },
        { new(Pose: 1), EnemyFrameId.FuneLeftActive0 },
        { new(Pose: 10), EnemyFrameId.NamiheLeftIdle },
        { new(Pose: 11), EnemyFrameId.NamiheLeftActive0 },
        { new(Pose: 12), EnemyFrameId.NamiheLeftActive1 },
        { new(Pose: 13), EnemyFrameId.NamiheLeftActive2 },
        { new(Pose: 14), EnemyFrameId.NamiheLeftActive3 },
        { new(Pose: 15), EnemyFrameId.NamiheLeftActive4 },
        { new(Pose: 16), EnemyFrameId.NamiheRightIdle },
        { new(Pose: 17), EnemyFrameId.NamiheRightActive0 },
        { new(Pose: 18), EnemyFrameId.NamiheRightActive1 },
        { new(Pose: 19), EnemyFrameId.NamiheRightActive2 },
        { new(Pose: 2), EnemyFrameId.FuneLeftActive1 },
        { new(Pose: 20), EnemyFrameId.NamiheRightActive3 },
        { new(Pose: 21), EnemyFrameId.NamiheRightActive4 },
        { new(Pose: 3), EnemyFrameId.FuneLeftActive2 },
        { new(Pose: 4), EnemyFrameId.FuneLeftActive3 },
        { new(Pose: 5), EnemyFrameId.FuneRightIdle },
        { new(Pose: 6), EnemyFrameId.FuneRightActive0 },
        { new(Pose: 7), EnemyFrameId.FuneRightActive1 },
        { new(Pose: 8), EnemyFrameId.FuneRightActive2 },
        { new(Pose: 9), EnemyFrameId.FuneRightActive3 },
    };
}
