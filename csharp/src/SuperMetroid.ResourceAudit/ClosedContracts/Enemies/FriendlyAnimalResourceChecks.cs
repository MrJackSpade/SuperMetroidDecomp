using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Confirms each specifically identified friendly-animal artwork omission.</summary>
internal static class FriendlyAnimalResourceChecks
{
    /// <summary>Selects each friendly-animal family's bank and the poses its typed programs show (#641) for composition checks.</summary>
    public static void Run(string family)
    {
        switch (family)
        {
            case "Etecoon":
                EnemyCompositionResourceChecks.RunSpritemaps(family, FriendlyAnimalVisualDefinitions.NormalBank,
                    EtecoonAnimationsTooling.Spritemaps(Enum.GetValues<EtecoonSegment>()),
                    FriendlyAnimalVisualDefinitions.EtecoonFrameCount);
                return;
            case "Dachora":
                EnemyCompositionResourceChecks.RunSpritemaps(family, FriendlyAnimalVisualDefinitions.NormalBank,
                    DachoraAnimationsTooling.Spritemaps(Enum.GetValues<DachoraSegment>()),
                    FriendlyAnimalVisualDefinitions.DachoraFrameCount);
                return;
            case "EscapeEtecoon":
                EnemyCompositionResourceChecks.RunSpritemaps(family, FriendlyAnimalVisualDefinitions.EscapeBank,
                    EscapeEtecoonAnimationsTooling.Spritemaps(Enum.GetValues<EscapeEtecoonSegment>()),
                    FriendlyAnimalVisualDefinitions.EscapeEtecoonFrameCount);
                return;
            case "EscapeDachora":
                EnemyCompositionResourceChecks.RunSpritemaps(family, FriendlyAnimalVisualDefinitions.EscapeBank,
                    EscapeDachoraAnimationsTooling.Spritemaps(Enum.GetValues<EscapeDachoraSegment>()),
                    FriendlyAnimalVisualDefinitions.EscapeDachoraFrameCount);
                return;
            default:
                throw new ArgumentException("Expected Etecoon, Dachora, EscapeEtecoon or EscapeDachora.", nameof(family));
        }
    }
}
