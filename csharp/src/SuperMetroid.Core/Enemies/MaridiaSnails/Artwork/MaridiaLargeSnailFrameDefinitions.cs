using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MaridiaSnails.Artwork;

/// <summary>
/// The presentation boundary of Oum's programs (#641): the bank-$A2 spritemap each
/// <see cref="MaridiaLargeSnailFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class MaridiaLargeSnailFrameDefinitions
{

    /// <summary>The installed frame each <see cref="MaridiaLargeSnailFrame"/> shows.</summary>
    internal static EnemyFrameTable<MaridiaLargeSnailFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.OumOamCB87 },
        { new(Pose: 1), EnemyFrameId.OumOamCB91 },
        { new(Pose: 10), EnemyFrameId.OumOamCBEB },
        { new(Pose: 11), EnemyFrameId.OumOamCBF5 },
        { new(Pose: 12), EnemyFrameId.OumOamCBFF },
        { new(Pose: 13), EnemyFrameId.OumOamCC09 },
        { new(Pose: 14), EnemyFrameId.OumOamCC13 },
        { new(Pose: 15), EnemyFrameId.OumOamCC1D },
        { new(Pose: 16), EnemyFrameId.OumOamCC27 },
        { new(Pose: 17), EnemyFrameId.OumOamCC31 },
        { new(Pose: 18), EnemyFrameId.OumOamCC3B },
        { new(Pose: 19), EnemyFrameId.OumOamCC45 },
        { new(Pose: 2), EnemyFrameId.OumOamCB9B },
        { new(Pose: 20), EnemyFrameId.OumOamCC4F },
        { new(Pose: 21), EnemyFrameId.OumOamCC59 },
        { new(Pose: 22), EnemyFrameId.OumOamCC63 },
        { new(Pose: 23), EnemyFrameId.OumOamCC6D },
        { new(Pose: 24), EnemyFrameId.OumOamCC77 },
        { new(Pose: 25), EnemyFrameId.OumOamCC81 },
        { new(Pose: 26), EnemyFrameId.OumOamCC8B },
        { new(Pose: 27), EnemyFrameId.OumOamCC95 },
        { new(Pose: 28), EnemyFrameId.OumOamCC9F },
        { new(Pose: 29), EnemyFrameId.OumOamCCA9 },
        { new(Pose: 3), EnemyFrameId.OumOamCBA5 },
        { new(Pose: 4), EnemyFrameId.OumOamCBAF },
        { new(Pose: 5), EnemyFrameId.OumOamCBB9 },
        { new(Pose: 6), EnemyFrameId.OumOamCBC3 },
        { new(Pose: 7), EnemyFrameId.OumOamCBCD },
        { new(Pose: 8), EnemyFrameId.OumOamCBD7 },
        { new(Pose: 9), EnemyFrameId.OumOamCBE1 },
    };
}
