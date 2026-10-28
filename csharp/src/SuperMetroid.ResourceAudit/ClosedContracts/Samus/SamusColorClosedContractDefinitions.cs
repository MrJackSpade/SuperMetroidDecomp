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
        new("SuperMetroid.Core.Samus.Artwork.InstalledSamusFullBodyCycleColors", "samus-body-cycle-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions, FullBodyCyclePalettes,
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledSamusFullBodyCycleColors.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/LoadingPaletteInputView.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/LoadingPaletteColorDefinitions.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.InstalledSamusSuitColors", "samus-suit-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions, FullBodyCyclePalettes,
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledSamusSuitColors.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.InstalledSamusChargeColors", "samus-charge-complete-palettes", ["ApplyCharge", "ApplyHyper", "ResolveCharge", "ResolveHyper"],
            [PaletteDefinitions, FullBodyCyclePalettes,
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledSamusChargeColors.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledSamusFullBodyCycleColors.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusPaletteFade.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledSamusHyperBeamColors.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/LoadingPaletteInputView.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.InstalledSamusHurtColors", "samus-hurt-complete-palettes", ["Resolve"],
            [PaletteDefinitions, FullBodyCyclePalettes,
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledSamusHurtColors.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusHurtColorDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusHurtColorLevels.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.InstalledSamusHyperBeamColors", "samus-hyper-beam-complete-palettes", ["Resolve"],
            [PaletteDefinitions, FullBodyCyclePalettes,
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledSamusHyperBeamColors.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/LoadingPaletteInputView.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.InstalledSamusVisorColors", "samus-visor-complete-colors", ["Resolve", "TryResolveByteOffset"],
            [PaletteDefinitions, FullBodyCyclePalettes,
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledSamusVisorColors.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusVisorColorDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusVisorColorSteps.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.InstalledCrystalFlashColors", "crystal-flash-complete-color-streams", ["ApplyBody", "ApplyBubble", "ResolveBody", "ResolveBubble"],
            [PaletteDefinitions, FullBodyCyclePalettes,
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledCrystalFlashColors.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.InstalledPowerBombFixedColors", "power-bomb-complete-fixed-colors", ["Resolve"],
            [PaletteDefinitions, FullBodyCyclePalettes,
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledPowerBombFixedColors.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.InstalledHyperBeamFxColors", "hyper-beam-fx-complete-color-frames", ["Apply"],
            [new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledHyperBeamFxColors.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledSamusHyperBeamColors.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/LoadingPaletteInputView.cs")]),
    ];
}
