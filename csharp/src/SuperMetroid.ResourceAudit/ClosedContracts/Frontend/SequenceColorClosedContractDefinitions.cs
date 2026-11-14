namespace SuperMetroid.ResourceAudit.ClosedContracts.Frontend;

/// <summary>Complete imported color sequences; no battle, death or palette animation is executed.</summary>
internal static class SequenceColorClosedContractDefinitions
{
    /// <summary>Imported palette sequences and fixed color sets reviewed as
    /// complete presentation domains, with their source dependencies.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainRainbowBeamColours", "mother-brain-rainbow-v3-complete-sequences",
            ["BeamColor"],
            [new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainRainbowBeamColours.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainBeamDefinitions.cs")]),
        new("SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork.MotherBrainDeathColorCatalog", "mother-brain-death-v1-complete-color-rows",
            ["BodyColor", "LegColor", "CorpseColor"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Bosses/MotherBrain/Artwork/MotherBrainDeathColorCatalog.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/MotherBrain/MotherBrainDeathColorFile.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/MotherBrain/Artwork/MotherBrainDeathColorFormat.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Artwork/InstalledColourRows.cs")]),
        new("SuperMetroid.AssetExtraction.Enemies.Artwork.ChozoAndTubeColorCatalog", "chozo-tube-v1-complete-fixed-palettes",
            ["ResolveTubeCracks", "ResolveStatue"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Enemies/Artwork/ChozoAndTubeColorCatalog.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/Artwork/ChozoAndTubeColorDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Artwork/InstalledColourRows.cs")]),
        new("SuperMetroid.AssetExtraction.Samus.Artwork.InstalledSamusDeathPalettes", "samus-death-complete-cloned-color-sequences",
            ["SuitedColor", "SuitlessColor", "WhiteoutColor", "ExplosionPaletteIndex"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Samus/Artwork/InstalledSamusDeathPalettes.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Samus/Artwork/SamusColourSets.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Palettes/SamusPaletteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Palettes/SamusFullBodyCyclePalettes.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Death/SamusDeathExplosionTimingDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Death/SamusDeathExplosionTiming.cs")]),
    ];
}
