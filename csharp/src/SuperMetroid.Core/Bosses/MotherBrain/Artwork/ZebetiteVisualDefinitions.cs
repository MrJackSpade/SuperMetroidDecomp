using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The presentation boundary of the Zebetites (#641): the spritemaps of their health poses.</summary>
internal static class ZebetiteVisualDefinitions
{

    /// <summary>The installed frame each <see cref="ZebetiteFrame"/> shows.</summary>
    internal static EnemyFrameTable<ZebetiteFrame> ShownFrames { get; } = new()
    {
        { new(Index: 0), EnemyFrameId.ZebetiteFirst },
        { new(Index: 1), EnemyFrameId.ZebetiteSecond },
        { new(Index: 2), EnemyFrameId.ZebetiteThird },
        { new(Index: 3), EnemyFrameId.ZebetiteFourth },
        { new(Index: 4), EnemyFrameId.ZebetiteFifth },
        { new(Index: 5), EnemyFrameId.ZebetiteSixth },
        { new(Index: 6), EnemyFrameId.ZebetiteSeventh },
        { new(Index: 7), EnemyFrameId.ZebetiteEighth },
        { new(Index: 8), EnemyFrameId.ZebetiteNinth },
        { new(Index: 9), EnemyFrameId.ZebetiteTenth },
    };
}
