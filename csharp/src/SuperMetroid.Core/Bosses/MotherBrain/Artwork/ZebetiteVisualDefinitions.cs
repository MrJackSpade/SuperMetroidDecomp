using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The presentation boundary of the Zebetites (#641): the frames of their health poses.</summary>
internal static class ZebetiteVisualDefinitions
{

    /// <summary>The installed frame each <see cref="ZebetiteFrame"/> shows.</summary>
    internal static EnemyFrameTable<ZebetiteFrame> ShownFrames { get; } = new()
    {
        { ZebetiteFrame.LargeAtLeast800, EnemyFrameId.ZebetiteTallIntact },
        { ZebetiteFrame.LargeAtLeast600, EnemyFrameId.ZebetiteTallDrained },
        { ZebetiteFrame.LargeAtLeast400, EnemyFrameId.ZebetiteTallHalfDrained },
        { ZebetiteFrame.LargeAtLeast200, EnemyFrameId.ZebetiteTallMostlyDrained },
        { ZebetiteFrame.LargeBelow200, EnemyFrameId.ZebetiteTallNearlyDrained },
        { ZebetiteFrame.SplitAtLeast800, EnemyFrameId.ZebetiteShortIntact },
        { ZebetiteFrame.SplitAtLeast600, EnemyFrameId.ZebetiteShortDrained },
        { ZebetiteFrame.SplitAtLeast400, EnemyFrameId.ZebetiteShortHalfDrained },
        { ZebetiteFrame.SplitAtLeast200, EnemyFrameId.ZebetiteShortMostlyDrained },
        { ZebetiteFrame.SplitBelow200, EnemyFrameId.ZebetiteShortNearlyDrained },
    };
}
