using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms only the six statically identified ordinary-composition omissions.</summary>
internal static class OrdinaryEnemyResourceChecks
{
    public static void Run(string family)
    {
        int previousCount = EnemySpritemapDefinitions.PreAuditOrdinaryFrameCount;
        int previousVersion = EnemySpritemapDefinitions.PreAuditOrdinaryVersion;
        (string Name, byte Bank, ushort[] Operands, int Count)[] families =
        [
            ("Hibashi", HibashiVisualDefinitions.Bank, HibashiVisualDefinitions.Operands(), HibashiVisualDefinitions.FrameCount),
            ("Zebetite", ZebetiteVisualDefinitions.Bank, ZebetiteVisualDefinitions.Operands(), ZebetiteVisualDefinitions.FrameCount),
            ("WreckedShipGhost", WreckedShipGhostVisualDefinitions.Bank, WreckedShipGhostVisualDefinitions.Operands(), WreckedShipGhostVisualDefinitions.FrameCount),
            ("Powamp", PowampVisualDefinitions.Bank, PowampVisualDefinitions.Operands(), PowampVisualDefinitions.FrameCount),
            ("Spark", SparkVisualDefinitions.Bank, SparkVisualDefinitions.Operands(), SparkVisualDefinitions.FrameCount),
            ("Shitroid", ShitroidVisualDefinitions.Bank, ShitroidVisualDefinitions.Operands(), ShitroidVisualDefinitions.FrameCount),
        ];
        int offset = 0;
        foreach (var definition in families)
        {
            if (definition.Name == family)
            {
                EnemyCompositionResourceChecks.Run(family, definition.Bank, definition.Operands,
                    definition.Count, previousVersion, previousCount, previousCount + offset);
                return;
            }
            offset += definition.Count;
        }
        throw new ArgumentException("Expected " + string.Join(", ", families.Select(item => item.Name)), nameof(family));
    }
}
