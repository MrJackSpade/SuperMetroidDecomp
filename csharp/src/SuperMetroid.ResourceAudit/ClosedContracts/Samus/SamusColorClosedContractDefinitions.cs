using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Samus;

/// <summary>Complete loader-owned player color domains; no palette clocks or gameplay paths are executed.</summary>
internal static class SamusColorClosedContractDefinitions
{
    /// <summary>Shared native palette definitions required by the player-color providers.</summary>
    private static readonly ReviewedSource PaletteDefinitions = new(
        "csharp/src/SuperMetroid.Core/Samus/Palettes/SamusPaletteDefinitions.cs");

    /// <summary>The full-body cycle palettes the special palette handlers select from the shared definitions.</summary>
    private static readonly ReviewedSource FullBodyCyclePalettes = new(
        "csharp/src/SuperMetroid.Core/Samus/Palettes/SamusFullBodyCyclePalettes.cs");

    /// <summary>Reviewed Samus body, suit, charge, hurt, visor, and special
    /// effect color domains with their complete source dependencies.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.AssetExtraction.Samus.Artwork.SamusFullBodyCycleColours", "samus-body-cycle-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions, FullBodyCyclePalettes,
             new("csharp/src/SuperMetroid.AssetExtraction/Samus/Artwork/SamusFullBodyCycleColours.cs")]),
        new("SuperMetroid.AssetExtraction.Samus.Artwork.InstalledSamusSuitColors", "samus-suit-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions, FullBodyCyclePalettes,
             new("csharp/src/SuperMetroid.AssetExtraction/Samus/Artwork/InstalledSamusSuitColors.cs")]),
        new("SuperMetroid.AssetExtraction.Samus.Artwork.SamusChargeColours", "samus-charge-complete-palettes", ["ApplyCharge", "ApplyHyper", "ResolveHyper"],
            [PaletteDefinitions, FullBodyCyclePalettes,
             new("csharp/src/SuperMetroid.AssetExtraction/Samus/Artwork/SamusChargeColours.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusChargePalette.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Samus/Artwork/SamusHyperBeamColours.cs")]),
        new("SuperMetroid.AssetExtraction.Samus.Artwork.InstalledSamusHurtColors", "samus-hurt-complete-palettes", ["Resolve"],
            [PaletteDefinitions, FullBodyCyclePalettes,
             new("csharp/src/SuperMetroid.AssetExtraction/Samus/Artwork/InstalledSamusHurtColors.cs")]),
        new("SuperMetroid.AssetExtraction.Samus.Artwork.SamusHyperBeamColours", "samus-hyper-beam-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions, FullBodyCyclePalettes,
             new("csharp/src/SuperMetroid.AssetExtraction/Samus/Artwork/SamusHyperBeamColours.cs")]),
        new("SuperMetroid.AssetExtraction.Samus.Artwork.InstalledSamusVisorColors", "samus-visor-complete-colors", ["Resolve"],
            [PaletteDefinitions, FullBodyCyclePalettes,
             new("csharp/src/SuperMetroid.AssetExtraction/Samus/Artwork/InstalledSamusVisorColors.cs")]),
        new("SuperMetroid.AssetExtraction.Samus.Artwork.InstalledCrystalFlashColors", "crystal-flash-complete-color-streams", ["ApplyBody", "ApplyBubble", "ResolveBody", "ResolveBubble"],
            [PaletteDefinitions, FullBodyCyclePalettes,
             new("csharp/src/SuperMetroid.AssetExtraction/Samus/Artwork/InstalledCrystalFlashColors.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.InstalledPowerBombGlowColors", "power-bomb-complete-glow-colors", ["Resolve"],
            [PaletteDefinitions, FullBodyCyclePalettes,
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledPowerBombGlowColors.cs")]),
        new("SuperMetroid.AssetExtraction.Samus.Artwork.HyperBeamFxColours", "hyper-beam-fx-complete-color-frames", ["Apply"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.AssetExtraction/Samus/Artwork/HyperBeamFxColours.cs")]),
    ];
}
