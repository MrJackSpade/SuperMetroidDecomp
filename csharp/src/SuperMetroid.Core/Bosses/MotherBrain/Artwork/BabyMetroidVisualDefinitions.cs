using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The presentation boundary of the Baby Metroid (#641): the bank-$A9 spritemaps of its pulse, shared by the Tourian Shitroid and the cutscene Baby.</summary>
internal static class BabyMetroidVisualDefinitions
{

    /// <summary>The installed frame each <see cref="BabyMetroidFrame"/> shows.</summary>
    internal static EnemyFrameTable<BabyMetroidFrame> ShownFrames { get; } = new()
    {
        { new(Index: 0), EnemyFrameId.Shitroid02 },
        { new(Index: 1), EnemyFrameId.Shitroid01 },
        { new(Index: 2), EnemyFrameId.Shitroid00 },
    };
}
