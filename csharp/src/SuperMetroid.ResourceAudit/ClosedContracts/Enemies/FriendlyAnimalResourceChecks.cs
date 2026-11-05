using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Confirms each specifically identified friendly-animal artwork omission.</summary>
internal static class FriendlyAnimalResourceChecks
{
    /// <summary>Selects each friendly-animal family's bank, the poses its typed programs show (#641), and appended schema range for composition checks.</summary>
    public static void Run(string family)
    {
        int previousCount = EnemySpritemapDefinitions.PreFriendlyAnimalFrameCount;
        int escapeStart = previousCount + FriendlyAnimalVisualDefinitions.EtecoonFrameCount + FriendlyAnimalVisualDefinitions.DachoraFrameCount;
        switch (family)
        {
            case "Etecoon":
                EnemyCompositionResourceChecks.RunSpritemaps(family, FriendlyAnimalVisualDefinitions.NormalBank,
                    EtecoonAnimationsTooling.Spritemaps(Enum.GetValues<EtecoonProgram>()),
                    FriendlyAnimalVisualDefinitions.EtecoonFrameCount, previousCount, previousCount);
                return;
            case "Dachora":
                EnemyCompositionResourceChecks.RunSpritemaps(family, FriendlyAnimalVisualDefinitions.NormalBank,
                    DachoraAnimationsTooling.Spritemaps(Enum.GetValues<DachoraProgram>()),
                    FriendlyAnimalVisualDefinitions.DachoraFrameCount, previousCount,
                    previousCount + FriendlyAnimalVisualDefinitions.EtecoonFrameCount);
                return;
            case "EscapeEtecoon":
                EnemyCompositionResourceChecks.RunSpritemaps(family, FriendlyAnimalVisualDefinitions.EscapeBank,
                    EscapeEtecoonAnimationsTooling.Spritemaps(Enum.GetValues<EscapeEtecoonProgram>()),
                    FriendlyAnimalVisualDefinitions.EscapeEtecoonFrameCount, previousCount, escapeStart);
                return;
            case "EscapeDachora":
                EnemyCompositionResourceChecks.RunSpritemaps(family, FriendlyAnimalVisualDefinitions.EscapeBank,
                    EscapeDachoraAnimationsTooling.Spritemaps(Enum.GetValues<EscapeDachoraProgram>()),
                    FriendlyAnimalVisualDefinitions.EscapeDachoraFrameCount, previousCount,
                    escapeStart + FriendlyAnimalVisualDefinitions.EscapeEtecoonFrameCount);
                return;
            default:
                throw new ArgumentException("Expected Etecoon, Dachora, EscapeEtecoon or EscapeDachora.", nameof(family));
        }
    }
}
