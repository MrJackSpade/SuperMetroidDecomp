using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Samus;

/// <summary>Complete loader-owned player color domains; no palette clocks or gameplay paths are executed.</summary>
internal static class SamusColorClosedContractDefinitions
{
    /// <summary>Shared native palette definitions required by the player-color providers.</summary>
    private static readonly ReviewedSource PaletteDefinitions = new(
        "csharp/src/SuperMetroid.Core/Samus/Palettes/SamusPaletteRomData.cs");

    /// <summary>Reviewed Samus body, suit, charge, hurt, visor, and special
    /// effect color domains with their complete source dependencies.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Samus.Artwork.SamusFullBodyCycleColorCatalog", "samus-body-cycle-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusFullBodyCycleColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/LoadingPaletteInputView.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/LoadingPaletteColorDefinitions.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.SamusSuitColorCatalog", "samus-suit-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusSuitColorCatalog.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.SamusChargeColorCatalog", "samus-charge-complete-palettes", ["ApplyCharge", "ApplyHyper", "ResolveCharge", "ResolveHyper"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusChargeColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusFullBodyCycleColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusPaletteFade.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusHyperBeamColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/LoadingPaletteInputView.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.SamusHurtColorCatalog", "samus-hurt-complete-palettes", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusHurtColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusHurtColorDefinitions.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.SamusHyperBeamColorCatalog", "samus-hyper-beam-complete-palettes", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusHyperBeamColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/LoadingPaletteInputView.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.SamusVisorColorCatalog", "samus-visor-complete-colors", ["Resolve", "TryResolveByteOffset"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusVisorColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusVisorColorDefinitions.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.CrystalFlashColorCatalog", "crystal-flash-complete-color-streams", ["ApplyBody", "ApplyBubble", "ResolveBody", "ResolveBubble"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/CrystalFlashColorCatalog.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.PowerBombFixedColorCatalog", "power-bomb-complete-fixed-colors", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/PowerBombFixedColorCatalog.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.HyperBeamFxColorCatalog", "hyper-beam-fx-complete-color-frames", ["Apply"],
            [new("csharp/src/SuperMetroid.Core/Samus/Artwork/HyperBeamFxColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusHyperBeamColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/LoadingPaletteInputView.cs")]),
    ];
}
