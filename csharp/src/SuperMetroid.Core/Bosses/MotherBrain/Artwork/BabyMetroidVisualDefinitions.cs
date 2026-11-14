using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The presentation boundary of the Baby Metroid (#641): the frames of its pulse, shared by the Tourian Shitroid and the cutscene Baby.</summary>
internal static class BabyMetroidVisualDefinitions
{

    /// <summary>The installed frame each <see cref="BabyMetroidFrame"/> shows.</summary>
    internal static EnemyFrameTable<BabyMetroidFrame> ShownFrames { get; } = new()
    {
        { BabyMetroidFrame.Closed, EnemyFrameId.BabyMetroidInitialFirst },
        { BabyMetroidFrame.HalfOpen, EnemyFrameId.BabyMetroidInitialSecond },
        { BabyMetroidFrame.WideOpen, EnemyFrameId.BabyMetroidInitialThird },
    };
}
