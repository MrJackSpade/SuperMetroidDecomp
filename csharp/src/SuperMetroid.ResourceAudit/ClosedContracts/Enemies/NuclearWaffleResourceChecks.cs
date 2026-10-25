using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.World.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Confirms the eight head compositions statically identified as omitted.</summary>
internal static class NuclearWaffleResourceChecks
{
    /// <summary>Checks the omitted head compositions against the compiled visual operands and pre-Nuclear-Waffle schema boundary.</summary>
    internal static void Run() => EnemyCompositionResourceChecks.RunSpritemaps("Nuclear Waffle/Puromi",
        NuclearWaffleVisualDefinitions.Bank, NuclearWaffleVisualDefinitions.Spritemaps(),
        NuclearWaffleVisualDefinitions.FrameCount, EnemySpritemapSchema.PreNuclearWaffle.Revision,
        EnemySpritemapDefinitions.PreNuclearWaffleFrameCount);
}
