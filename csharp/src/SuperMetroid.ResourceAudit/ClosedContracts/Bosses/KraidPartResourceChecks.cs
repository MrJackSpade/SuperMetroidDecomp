using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.ResourceAudit.ClosedContracts.Enemies;
using SuperMetroid.AssetExtraction.World.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Bosses;

/// <summary>Confirms only the statically identified foot/lint composition omissions, not the fight.</summary>
internal static class KraidPartResourceChecks
{
    /// <summary>Audits the selected Kraid foot extended frames or lint OAM frames against their required installed resource counts.</summary>
    internal static void Run(string part)
    {
        switch (part)
        {
            case "Foot":
                ExtendedEnemyCompositionResourceChecks.Run("Kraid foot", [.. EnemyExtendedFrameDefinitions.Frames[EnemyExtendedFrameDefinitions.PreKraidFootFrameCount..]],
                    EnemyExtendedFrameSchema.PreKraidFoot.Revision, EnemyExtendedFrameDefinitions.PreKraidFootFrameCount);
                break;
            case "Lint":
                EnemyCompositionResourceChecks.RunSpritemaps("Kraid lint", KraidLintVisualDefinitions.Bank,
                    KraidLintVisualDefinitions.Spritemaps(), KraidLintVisualDefinitions.FrameCount,
                    EnemySpritemapSchema.PreKraidLint.Revision, EnemySpritemapDefinitions.PreKraidLintFrameCount);
                break;
            default: throw new ArgumentException("Expected Foot or Lint", nameof(part));
        }
    }
}
