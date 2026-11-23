using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The presentation boundary of the Baby Metroid (#641): the frames of its pulse, shared by the Tourian Baby Metroid and the cutscene Baby.</summary>
internal static partial class BabyMetroidVisualDefinitions
{

    /// <summary>The installed frame each <see cref="BabyMetroidFrame"/> shows.</summary>
    internal static EnemyFrameTable<BabyMetroidFrame> ShownFrames { get; } = new()
    {
        { BabyMetroidFrame.Closed, EnemyFrameId.BabyMetroidNucleiDark },
        { BabyMetroidFrame.HalfOpen, EnemyFrameId.BabyMetroidNucleiGlowing },
        { BabyMetroidFrame.WideOpen, EnemyFrameId.BabyMetroidNucleiBright },
    };
}
