using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the eight head compositions statically identified as omitted.</summary>
internal static class NuclearWaffleResourceChecks
{
    /// <summary>Checks the omitted head compositions against the compiled visual operands and pre-Nuclear-Waffle schema boundary.</summary>
    internal static void Run() => EnemyCompositionResourceChecks.Run("Nuclear Waffle/Puromi",
        NuclearWaffleVisualDefinitions.Bank, NuclearWaffleVisualDefinitions.Operands(),
        NuclearWaffleVisualDefinitions.FrameCount, (int)EnemySpritemapSchema.PreNuclearWaffle,
        EnemySpritemapDefinitions.PreNuclearWaffleFrameCount);
}
