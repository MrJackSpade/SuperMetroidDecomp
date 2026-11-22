using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The runs of Samus's animation programs that no segment of her animations names (#2395). The cartridge lays them out
/// between two named segments, so the import counts their frames in the layout it decodes; no read reaches them, so the
/// game holds nothing of them.
/// </summary>
internal static partial class SamusAnimationUnnamedRunDefinitions
{
    /// <summary>
    /// Every unnamed run, in layout order: the knockback program's slow recoil after the knockback's own frames and before
    /// its recoil hold, four delays closed by a restart and then two delays closed by a restart.
    /// </summary>
    internal static IReadOnlyList<SamusAnimationUnnamedRun> All { get; } =
    [
        new(SamusAnimationName.Knockback, SamusAnimationSegmentName.Recoil, Delays: 4, new SamusAnimationRestart()),
        new(SamusAnimationName.Knockback, SamusAnimationSegmentName.Recoil, Delays: 2, new SamusAnimationRestart()),
    ];
}
