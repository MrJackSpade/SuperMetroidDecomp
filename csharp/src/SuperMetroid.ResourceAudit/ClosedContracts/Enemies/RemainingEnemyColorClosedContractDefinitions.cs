namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Individually reviewed complete enemy color sources; no AI, battle or palette cadence is exercised.</summary>
internal static class RemainingEnemyColorClosedContractDefinitions
{
    /// <summary>Reviewed complete color-source contracts for ten enemy and cinematic palette families.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.AssetExtraction.Bosses.Botwoon.Artwork.BotwoonColorCatalog", "botwoon-complete-health-colors", ["HealthColor"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Bosses/Botwoon/Artwork/BotwoonColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Botwoon/BotwoonHealthBandDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Artwork/InstalledColourRows.cs")]),
        new("SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork.BabyMetroidCutsceneColorCatalog", "cutscene-baby-complete-color-images", ["InitialColor", "FadeColor"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Bosses/MotherBrain/Artwork/BabyMetroidCutsceneColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/BabyMetroidCutsceneColorDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/MotherBrain/Artwork/BabyMetroidCutsceneColorFormat.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Artwork/InstalledColourRows.cs")]),
        new("SuperMetroid.AssetExtraction.Enemies.Artwork.EnemyAuxiliaryColorCatalog", "enemy-auxiliary-complete-color-images", ["Resolve"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Enemies/Artwork/EnemyAuxiliaryColorCatalog.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemyAuxiliaryColorDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Artwork/InstalledColourRows.cs")]),
        new("SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork.KraidColorCatalog", "kraid-complete-color-sources", ["Resolve"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Bosses/Kraid/Artwork/KraidColorCatalog.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/Kraid/KraidPalettes.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/Kraid/Artwork/KraidColors.cs")]),
        new("SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork.ShitroidColorCatalog", "shitroid-complete-live-color-images", ["NormalColor", "TargetColor"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Bosses/MotherBrain/Artwork/ShitroidColorCatalog.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/MotherBrain/ShitroidColorDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/BabyMetroidCutsceneColorDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/MotherBrain/Artwork/BabyMetroidCutsceneColorFormat.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Artwork/InstalledColourRows.cs")]),
        new("SuperMetroid.AssetExtraction.Enemies.Artwork.DachoraColorCatalog", "dachora-complete-phase-colors", ["Resolve"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Enemies/Artwork/DachoraColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Animals/DachoraColorDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Artwork/InstalledColourRows.cs")]),
    ];
}
