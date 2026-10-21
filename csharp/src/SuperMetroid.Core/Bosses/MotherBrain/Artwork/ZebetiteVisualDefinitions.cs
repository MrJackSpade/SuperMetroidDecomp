using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The presentation boundary of the Zebetites (#641): the bank-$A6 spritemaps of their health poses.</summary>
internal static class ZebetiteVisualDefinitions
{

    /// <summary>The installed frame each <see cref="ZebetiteFrame"/> shows.</summary>
    internal static EnemyFrameTable<ZebetiteFrame> ShownFrames { get; } = new()
    {
        { new(Index: 0), EnemyFrameId.Zebetite00 },
        { new(Index: 1), EnemyFrameId.Zebetite01 },
        { new(Index: 2), EnemyFrameId.Zebetite02 },
        { new(Index: 3), EnemyFrameId.Zebetite03 },
        { new(Index: 4), EnemyFrameId.Zebetite04 },
        { new(Index: 5), EnemyFrameId.Zebetite05 },
        { new(Index: 6), EnemyFrameId.Zebetite06 },
        { new(Index: 7), EnemyFrameId.Zebetite07 },
        { new(Index: 8), EnemyFrameId.Zebetite08 },
        { new(Index: 9), EnemyFrameId.Zebetite09 },
    };
}
