using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The presentation boundary of the Zebetites (#641): the frames of their health poses.</summary>
internal static class ZebetiteVisualDefinitions
{

    /// <summary>The installed frame each <see cref="ZebetiteFrame"/> shows.</summary>
    internal static EnemyFrameTable<ZebetiteFrame> ShownFrames { get; } = new()
    {
        { ZebetiteFrame.LargeAtLeast800, EnemyFrameId.ZebetiteFirst },
        { ZebetiteFrame.LargeAtLeast600, EnemyFrameId.ZebetiteSecond },
        { ZebetiteFrame.LargeAtLeast400, EnemyFrameId.ZebetiteThird },
        { ZebetiteFrame.LargeAtLeast200, EnemyFrameId.ZebetiteFourth },
        { ZebetiteFrame.LargeBelow200, EnemyFrameId.ZebetiteFifth },
        { ZebetiteFrame.SplitAtLeast800, EnemyFrameId.ZebetiteSixth },
        { ZebetiteFrame.SplitAtLeast600, EnemyFrameId.ZebetiteSeventh },
        { ZebetiteFrame.SplitAtLeast400, EnemyFrameId.ZebetiteEighth },
        { ZebetiteFrame.SplitAtLeast200, EnemyFrameId.ZebetiteNinth },
        { ZebetiteFrame.SplitBelow200, EnemyFrameId.ZebetiteTenth },
    };
}
