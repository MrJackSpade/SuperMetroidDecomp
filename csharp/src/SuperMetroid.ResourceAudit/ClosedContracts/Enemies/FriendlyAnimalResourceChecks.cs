using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Confirms each specifically identified friendly-animal artwork omission.</summary>
internal static class FriendlyAnimalResourceChecks
{
    /// <summary>Selects each friendly-animal family's bank, poses, and appended schema range for composition checks.</summary>
    public static void Run(string family)
    {
        int previousCount = EnemySpritemapDefinitions.PreFriendlyAnimalFrameCount;
        int previousVersion = (int)EnemySpritemapSchema.PreFriendlyAnimal;
        int escapeStart = previousCount + FriendlyAnimalVisualDefinitions.EtecoonFrameCount + FriendlyAnimalVisualDefinitions.DachoraFrameCount;
        switch (family)
        {
            case "Etecoon":
                EnemyCompositionResourceChecks.Run(family, FriendlyAnimalVisualDefinitions.NormalBank, FriendlyAnimalVisualDefinitions.EtecoonOperands(),
                    FriendlyAnimalVisualDefinitions.EtecoonFrameCount, previousVersion, previousCount, previousCount);
                return;
            case "Dachora":
                EnemyCompositionResourceChecks.Run(family, FriendlyAnimalVisualDefinitions.NormalBank, FriendlyAnimalVisualDefinitions.DachoraOperands(),
                    FriendlyAnimalVisualDefinitions.DachoraFrameCount, previousVersion, previousCount,
                    previousCount + FriendlyAnimalVisualDefinitions.EtecoonFrameCount);
                return;
            // The rescue-sequence animals are typed programs (#641); their poses come from the programs.
            case "EscapeEtecoon":
                EnemyCompositionResourceChecks.RunSpritemaps(family, FriendlyAnimalVisualDefinitions.EscapeBank,
                    EscapeEtecoonAnimations.Spritemaps(Enum.GetValues<EscapeEtecoonProgram>()),
                    FriendlyAnimalVisualDefinitions.EscapeEtecoonFrameCount, previousVersion, previousCount, escapeStart);
                return;
            case "EscapeDachora":
                EnemyCompositionResourceChecks.RunSpritemaps(family, FriendlyAnimalVisualDefinitions.EscapeBank,
                    EscapeDachoraAnimations.Spritemaps(Enum.GetValues<EscapeDachoraProgram>()),
                    FriendlyAnimalVisualDefinitions.EscapeDachoraFrameCount, previousVersion, previousCount,
                    escapeStart + FriendlyAnimalVisualDefinitions.EscapeEtecoonFrameCount);
                return;
            default:
                throw new ArgumentException("Expected Etecoon, Dachora, EscapeEtecoon or EscapeDachora.", nameof(family));
        }
    }
}
