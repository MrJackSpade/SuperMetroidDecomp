using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the eight head compositions statically identified as omitted.</summary>
internal static class NuclearWaffleResourceChecks
{
    /// <summary>Checks the statically identified Nuclear Waffle/Puromi head compositions.</summary>
    internal static void Run() => EnemyCompositionResourceChecks.Run("Nuclear Waffle/Puromi",
        NuclearWaffleVisualDefinitions.Bank, NuclearWaffleVisualDefinitions.Operands(),
        NuclearWaffleVisualDefinitions.FrameCount, EnemySpritemapDefinitions.PreNuclearWaffleVersion,
        EnemySpritemapDefinitions.PreNuclearWaffleFrameCount);
}
