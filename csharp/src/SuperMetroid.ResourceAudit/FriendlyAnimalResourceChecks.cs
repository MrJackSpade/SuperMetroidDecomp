using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms each specifically identified friendly-animal artwork omission.</summary>
internal static class FriendlyAnimalResourceChecks
{
    /// <summary>Checks the native composition slices for the selected friendly-animal family.</summary>
    /// <param name="family">One of the four supported normal or escape animal families.</param>
    public static void Run(string family)
    {
        int previousCount = EnemySpritemapDefinitions.PreFriendlyAnimalFrameCount;
        int previousVersion = EnemySpritemapDefinitions.PreFriendlyAnimalVersion;
        (byte bank, ushort[] operands, int count, int offset) = family switch
        {
            "Etecoon" => (FriendlyAnimalVisualDefinitions.NormalBank, FriendlyAnimalVisualDefinitions.EtecoonOperands(),
                FriendlyAnimalVisualDefinitions.EtecoonFrameCount, 0),
            "Dachora" => (FriendlyAnimalVisualDefinitions.NormalBank, FriendlyAnimalVisualDefinitions.DachoraOperands(),
                FriendlyAnimalVisualDefinitions.DachoraFrameCount, FriendlyAnimalVisualDefinitions.EtecoonFrameCount),
            "EscapeEtecoon" => (FriendlyAnimalVisualDefinitions.EscapeBank, FriendlyAnimalVisualDefinitions.EscapeEtecoonOperands(),
                FriendlyAnimalVisualDefinitions.EscapeEtecoonFrameCount, FriendlyAnimalVisualDefinitions.EtecoonFrameCount +
                    FriendlyAnimalVisualDefinitions.DachoraFrameCount),
            "EscapeDachora" => (FriendlyAnimalVisualDefinitions.EscapeBank, FriendlyAnimalVisualDefinitions.EscapeDachoraOperands(),
                FriendlyAnimalVisualDefinitions.EscapeDachoraFrameCount, FriendlyAnimalVisualDefinitions.EtecoonFrameCount +
                    FriendlyAnimalVisualDefinitions.DachoraFrameCount + FriendlyAnimalVisualDefinitions.EscapeEtecoonFrameCount),
            _ => throw new ArgumentException("Expected Etecoon, Dachora, EscapeEtecoon or EscapeDachora.", nameof(family)),
        };
        EnemyCompositionResourceChecks.Run(family, bank, operands, count,
            previousVersion, previousCount, previousCount + offset);
    }
}
