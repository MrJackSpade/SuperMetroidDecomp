using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms only the statically identified foot/lint composition omissions, not the fight.</summary>
internal static class KraidPartResourceChecks
{
    /// <summary>Checks the identified Kraid foot or lint artwork composition gap.</summary>
    /// <param name="part">Either the extended-frame Foot family or spritemap Lint family.</param>
    internal static void Run(string part)
    {
        switch (part)
        {
            case "Foot":
                ExtendedEnemyCompositionResourceChecks.Run("Kraid foot", KraidFootVisualDefinitions.Frames.ToArray(),
                    EnemyExtendedFrameDefinitions.PreKraidFootVersion, EnemyExtendedFrameDefinitions.PreKraidFootFrameCount);
                break;
            case "Lint":
                EnemyCompositionResourceChecks.Run("Kraid lint", KraidLintVisualDefinitions.Bank,
                    KraidLintVisualDefinitions.Operands(), KraidLintVisualDefinitions.FrameCount,
                    EnemySpritemapDefinitions.PreKraidLintVersion, EnemySpritemapDefinitions.PreKraidLintFrameCount);
                break;
            default: throw new ArgumentException("Expected Foot or Lint", nameof(part));
        }
    }
}
