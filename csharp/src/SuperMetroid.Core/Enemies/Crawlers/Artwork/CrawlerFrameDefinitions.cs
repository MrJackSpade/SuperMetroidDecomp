using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Crawlers.Artwork;

/// <summary>
/// The presentation boundary of the crawlers' programs (#641): the bank-$A3 spritemap each
/// <see cref="CrawlerFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class CrawlerFrameDefinitions
{

    /// <summary>The installed frame each <see cref="CrawlerFrame"/> shows.</summary>
    internal static EnemyFrameTable<CrawlerFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.SciserUpsideUp0 },
        { new(Pose: 1), EnemyFrameId.SciserUpsideUp1 },
        { new(Pose: 10), EnemyFrameId.SciserUpsideLeft1 },
        { new(Pose: 11), EnemyFrameId.SciserUpsideLeft2 },
        { new(Pose: 12), EnemyFrameId.ZeroCrawler12 },
        { new(Pose: 13), EnemyFrameId.ZeroCrawler13 },
        { new(Pose: 14), EnemyFrameId.ZeroCrawler14 },
        { new(Pose: 15), EnemyFrameId.ZeroCrawler15 },
        { new(Pose: 16), EnemyFrameId.ZeroCrawler04 },
        { new(Pose: 17), EnemyFrameId.ZeroCrawler05 },
        { new(Pose: 18), EnemyFrameId.ZeroCrawler06 },
        { new(Pose: 19), EnemyFrameId.ZeroCrawler07 },
        { new(Pose: 2), EnemyFrameId.SciserUpsideUp2 },
        { new(Pose: 20), EnemyFrameId.ZeroCrawler08 },
        { new(Pose: 21), EnemyFrameId.ZeroCrawler09 },
        { new(Pose: 22), EnemyFrameId.ZeroCrawler10 },
        { new(Pose: 23), EnemyFrameId.ZeroCrawler11 },
        { new(Pose: 24), EnemyFrameId.ZeroCrawler00 },
        { new(Pose: 25), EnemyFrameId.ZeroCrawler01 },
        { new(Pose: 26), EnemyFrameId.ZeroCrawler02 },
        { new(Pose: 27), EnemyFrameId.ZeroCrawler03 },
        { new(Pose: 28), EnemyFrameId.ViolaSpin0 },
        { new(Pose: 29), EnemyFrameId.ViolaSpin1 },
        { new(Pose: 3), EnemyFrameId.SciserUpsideRight0 },
        { new(Pose: 30), EnemyFrameId.ViolaSpin2 },
        { new(Pose: 31), EnemyFrameId.ViolaSpin3 },
        { new(Pose: 32), EnemyFrameId.ViolaSpin4 },
        { new(Pose: 33), EnemyFrameId.ViolaSpin5 },
        { new(Pose: 34), EnemyFrameId.ViolaSpin6 },
        { new(Pose: 35), EnemyFrameId.ViolaSpin7 },
        { new(Pose: 36), EnemyFrameId.HzoomerUpsideUp0 },
        { new(Pose: 37), EnemyFrameId.HzoomerUpsideUp1 },
        { new(Pose: 38), EnemyFrameId.HzoomerUpsideUp2 },
        { new(Pose: 39), EnemyFrameId.HzoomerUpsideUp3 },
        { new(Pose: 4), EnemyFrameId.SciserUpsideRight1 },
        { new(Pose: 40), EnemyFrameId.HzoomerUpsideUp4 },
        { new(Pose: 41), EnemyFrameId.HzoomerUpsideLeft0 },
        { new(Pose: 42), EnemyFrameId.HzoomerUpsideLeft1 },
        { new(Pose: 43), EnemyFrameId.HzoomerUpsideLeft2 },
        { new(Pose: 44), EnemyFrameId.HzoomerUpsideLeft3 },
        { new(Pose: 45), EnemyFrameId.HzoomerUpsideLeft4 },
        { new(Pose: 46), EnemyFrameId.HzoomerUpsideDown0 },
        { new(Pose: 47), EnemyFrameId.HzoomerUpsideDown1 },
        { new(Pose: 48), EnemyFrameId.HzoomerUpsideDown2 },
        { new(Pose: 49), EnemyFrameId.HzoomerUpsideDown3 },
        { new(Pose: 5), EnemyFrameId.SciserUpsideRight2 },
        { new(Pose: 50), EnemyFrameId.HzoomerUpsideDown4 },
        { new(Pose: 51), EnemyFrameId.HzoomerUpsideRight0 },
        { new(Pose: 52), EnemyFrameId.HzoomerUpsideRight1 },
        { new(Pose: 53), EnemyFrameId.HzoomerUpsideRight2 },
        { new(Pose: 54), EnemyFrameId.HzoomerUpsideRight3 },
        { new(Pose: 55), EnemyFrameId.HzoomerUpsideRight4 },
        { new(Pose: 6), EnemyFrameId.SciserUpsideDown0 },
        { new(Pose: 7), EnemyFrameId.SciserUpsideDown1 },
        { new(Pose: 8), EnemyFrameId.SciserUpsideDown2 },
        { new(Pose: 9), EnemyFrameId.SciserUpsideLeft0 },
    };
}
