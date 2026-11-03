namespace SuperMetroid.ResourceAudit.ClosedContracts.Bosses;

/// <summary>Reviewed palette-domain closure proofs; no boss AI or palette timing is run.</summary>
internal static class BossColorClosedContractDefinitions
{
    /// <summary>Reviewed finite beam and boss palette catalogs with their guarded methods and source files for the resource audit.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Samus.Artwork.InstalledBeamPalettes", "beam-palettes-v1-twelve-selections", ["LoadTo"],
            [new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledBeamPalettes.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/BeamPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/BeamPaint.cs"),
             new("csharp/src/SuperMetroid.Core/Progression/SamusEquipmentFlags.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Samus/SpazerCompositionGeometryDefinitions.cs")]),
        new("SuperMetroid.Core.Bosses.SporeSpawn.Artwork.SporeSpawnColorCatalog", "spore-spawn-v1-complete-scene-palettes",
            ["ResolveSpore", "ResolveHealth", "ResolveDeath"],
            [new("csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/Artwork/SporeSpawnColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/Artwork/SporeSpawnColorSelection.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/Artwork/SporeSpawnColors.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/Artwork/SporeSpawnHealthBands.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/Artwork/SporeSpawnDeathColorDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/Artwork/SporeSpawnDeathFade.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/SporeSpawnColorDefinitions.cs")]),
        new("SuperMetroid.Core.Bosses.Phantoon.Artwork.PhantoonColorCatalog", "phantoon-v1-complete-color-targets",
            ["ResolveHealth", "ResolveFadeOut", "ResolvePowerOn"],
            [new("csharp/src/SuperMetroid.Core/Bosses/Phantoon/Artwork/PhantoonColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Phantoon/Artwork/PhantoonColorSelection.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Phantoon/Artwork/PhantoonColors.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/WreckedShipPowerPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/WreckedShipPowerPaint.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Phantoon/Artwork/PhantoonHealthPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Phantoon/Artwork/PhantoonHealthPaint.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Phantoon/PhantoonColorDefinitions.cs")]),
        new("SuperMetroid.Core.Areas.Tourian.Artwork.TourianStatuePalettes", "tourian-statue-v1-complete-eye-palettes",
            ["ResolveEye", "BaseColor", "StatueColor", "GreyColor"],
            [new("csharp/src/SuperMetroid.Core/Areas/Tourian/Artwork/TourianStatuePalettes.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Tourian/Artwork/TourianStatuePaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Tourian/Artwork/TourianStatuePaint.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Tourian/Artwork/TourianStatueColorDefinitions.cs")]),
    ];
}
