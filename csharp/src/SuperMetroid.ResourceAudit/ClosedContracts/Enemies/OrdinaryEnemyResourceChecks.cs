using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.World.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Confirms only the six statically identified ordinary-composition omissions.</summary>
internal static class OrdinaryEnemyResourceChecks
{
    /// <summary>Selects the reported family and its appended schema range before checking installed ordinary compositions.</summary>
    public static void Run(string family)
    {
        int previousCount = EnemySpritemapDefinitions.PreAuditOrdinaryFrameCount;
        int previousVersion = (int)EnemySpritemapSchema.PreAuditOrdinary;
        // Typed families (#641) name their spritemaps; the rest still resolve compiled operands.
        (string Name, byte Bank, ushort[]? Operands, ushort[]? Spritemaps, int Count)[] families =
        [
            ("Hibashi", HibashiVisualDefinitions.Bank, null, [.. HibashiVisualDefinitionsTooling.Spritemaps()], HibashiVisualDefinitions.FrameCount),
            ("Zebetite", ZebetiteVisualDefinitions.Bank, null, ZebetiteVisualDefinitions.Spritemaps(), ZebetiteVisualDefinitions.FrameCount),
            ("WreckedShipGhost", WreckedShipGhostVisualDefinitions.Bank, null, WreckedShipGhostVisualDefinitions.Spritemaps(), WreckedShipGhostVisualDefinitions.FrameCount),
            ("Powamp", PowampVisualDefinitions.Bank, null, [.. PowampVisualDefinitions.Frames().Select(frame => frame.Pointer)], PowampVisualDefinitions.FrameCount),
            ("Spark", SparkVisualDefinitions.Bank, null, [.. SparkVisualDefinitions.Frames().Select(frame => frame.Pointer)], SparkVisualDefinitions.FrameCount),
            ("Shitroid", ShitroidVisualDefinitions.Bank, null, ShitroidVisualDefinitions.Spritemaps(), ShitroidVisualDefinitions.FrameCount),
        ];
        int offset = 0;
        foreach (var definition in families)
        {
            if (definition.Name == family)
            {
                if (definition.Spritemaps is { } spritemaps)
                    EnemyCompositionResourceChecks.RunSpritemaps(family, definition.Bank, spritemaps,
                        definition.Count, previousVersion, previousCount, previousCount + offset);
                else
                    EnemyCompositionResourceChecks.Run(family, definition.Bank, definition.Operands,
                        definition.Count, previousVersion, previousCount, previousCount + offset);
                return;
            }
            offset += definition.Count;
        }
        throw new ArgumentException("Expected " + string.Join(", ", families.Select(item => item.Name)), nameof(family));
    }
}
