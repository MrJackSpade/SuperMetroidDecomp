using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Confirms only the six statically identified ordinary-composition omissions.</summary>
internal static class OrdinaryEnemyResourceChecks
{
    /// <summary>Selects the reported family before checking installed ordinary compositions.</summary>
    public static void Run(string family)
    {
        // Typed families (#641) name their spritemaps; the rest still resolve compiled operands.
        OrdinaryEnemyResourceFamily[] families =
        [
            new OrdinaryEnemyResourceFamily("Hibashi", HibashiVisualDefinitions.Bank, null, [.. HibashiVisualDefinitionsTooling.Spritemaps()], HibashiVisualDefinitions.FrameCount),
            new OrdinaryEnemyResourceFamily("Zebetite", ZebetiteVisualToolingDefinitions.Bank, null, ZebetiteVisualToolingDefinitions.Spritemaps(), ZebetiteVisualToolingDefinitions.FrameCount),
            new OrdinaryEnemyResourceFamily("WreckedShipGhost", WreckedShipGhostVisualDefinitions.Bank, null, WreckedShipGhostVisualDefinitions.Spritemaps(), WreckedShipGhostVisualDefinitions.FrameCount),
            new OrdinaryEnemyResourceFamily("Powamp", PowampVisualDefinitions.Bank, null, [.. EnemyCompositionSchemaTooling.Family("powamp_").Select(frame => frame.Pointer)], PowampVisualDefinitions.FrameCount),
            new OrdinaryEnemyResourceFamily("Spark", SparkVisualDefinitions.Bank, null, [.. EnemyCompositionSchemaTooling.Family("spark_").Select(frame => frame.Pointer)], SparkVisualDefinitions.FrameCount),
            new OrdinaryEnemyResourceFamily("Shitroid", BabyMetroidSpritemapDefinitions.Bank, null, BabyMetroidSpritemapDefinitions.Spritemaps(), BabyMetroidSpritemapDefinitions.FrameCount),
        ];
        foreach (var definition in families)
        {
            if (definition.Name == family)
            {
                if (definition.Spritemaps is { } spritemaps)
                    EnemyCompositionResourceChecks.RunSpritemaps(family, definition.Bank, spritemaps, definition.Count);
                else
                    EnemyCompositionResourceChecks.Run(family, definition.Bank, definition.Operands, definition.Count);
                return;
            }
        }
        throw new ArgumentException("Expected " + string.Join(", ", families.Select(item => item.Name)), nameof(family));
    }
}
