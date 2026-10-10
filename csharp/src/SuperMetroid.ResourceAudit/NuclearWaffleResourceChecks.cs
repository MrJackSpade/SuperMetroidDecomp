using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the eight head compositions statically identified as omitted.</summary>
internal static class NuclearWaffleResourceChecks
{
    internal static void Run() => EnemyCompositionResourceChecks.Run("Nuclear Waffle/Puromi",
        NuclearWaffleVisualDefinitions.Bank, NuclearWaffleVisualDefinitions.Operands(),
        NuclearWaffleVisualDefinitions.FrameCount, (int)EnemySpritemapSchema.PreNuclearWaffle,
        EnemySpritemapDefinitions.PreNuclearWaffleFrameCount);
}
