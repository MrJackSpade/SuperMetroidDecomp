using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms only the statically identified foot/lint composition omissions, not the fight.</summary>
internal static class KraidPartResourceChecks
{
    /// <summary>Audits the selected Kraid foot extended frames or lint OAM frames against their required installed resource counts.</summary>
    internal static void Run(string part)
    {
        switch (part)
        {
            case "Foot":
                ExtendedEnemyCompositionResourceChecks.Run("Kraid foot", KraidFootVisualDefinitions.Frames.ToArray(),
                    (int)EnemyExtendedFrameSchema.PreKraidFoot, EnemyExtendedFrameDefinitions.PreKraidFootFrameCount);
                break;
            case "Lint":
                EnemyCompositionResourceChecks.Run("Kraid lint", KraidLintVisualDefinitions.Bank,
                    KraidLintVisualDefinitions.Operands(), KraidLintVisualDefinitions.FrameCount,
                    (int)EnemySpritemapSchema.PreKraidLint, EnemySpritemapDefinitions.PreKraidLintFrameCount);
                break;
            default: throw new ArgumentException("Expected Foot or Lint", nameof(part));
        }
    }
}
