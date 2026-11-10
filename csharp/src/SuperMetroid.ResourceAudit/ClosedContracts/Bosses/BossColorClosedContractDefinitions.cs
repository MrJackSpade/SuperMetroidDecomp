namespace SuperMetroid.ResourceAudit.ClosedContracts.Bosses;

/// <summary>Reviewed palette-domain closure proofs; no boss AI or palette timing is run.</summary>
internal static class BossColorClosedContractDefinitions
{
    /// <summary>Reviewed finite beam and boss palette catalogs with their guarded methods and source files for the resource audit.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.AssetExtraction.Samus.Artwork.InstalledBeamPalettes", "beam-palettes-v1-twelve-selections", ["Colours"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Samus/Artwork/InstalledBeamPalettes.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Samus/Artwork/BeamPaletteDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Samus/BeamTileAtlasDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Progression/SamusEquipmentFlags.cs")]),
        new("SuperMetroid.AssetExtraction.Bosses.SporeSpawn.Artwork.SporeSpawnColorCatalog", "spore-spawn-v1-complete-scene-palettes",
            ["ResolveSpore", "ResolveHealth"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Bosses/SporeSpawn/Artwork/SporeSpawnColorCatalog.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/SporeSpawn/Artwork/SporeSpawnColorSelection.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/SporeSpawn/Artwork/SporeSpawnColors.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/SporeSpawn/Artwork/SporeSpawnHealthBands.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/SporeSpawn/Artwork/SporeSpawnDeathColorDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/SporeSpawn/Artwork/SporeSpawnDeathFade.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/SporeSpawnColorDefinitions.cs")]),
        new("SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork.PhantoonColorCatalog", "phantoon-v1-complete-color-targets",
            ["ResolveHealth", "ResolveFadeOut", "ResolvePowerOn"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Bosses/Phantoon/Artwork/PhantoonColorCatalog.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/Phantoon/Artwork/PhantoonColorSelection.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/Phantoon/Artwork/PhantoonColors.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/Artwork/WreckedShipPowerPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/Artwork/WreckedShipPowerPaint.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/Phantoon/Artwork/PhantoonHealthPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/Phantoon/Artwork/PhantoonHealthPaint.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Phantoon/PhantoonColorDefinitions.cs")]),
        new("SuperMetroid.AssetExtraction.Areas.Tourian.Artwork.TourianStatuePalettes", "tourian-statue-v1-complete-eye-palettes",
            ["ResolveEye", "BaseColor", "StatueColor", "GreyColor"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Areas/Tourian/Artwork/TourianStatuePalettes.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Areas/Tourian/Artwork/TourianStatuePaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Areas/Tourian/Artwork/TourianStatuePaint.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Areas/Tourian/Artwork/TourianStatueColorDefinitions.cs")]),
    ];
}
