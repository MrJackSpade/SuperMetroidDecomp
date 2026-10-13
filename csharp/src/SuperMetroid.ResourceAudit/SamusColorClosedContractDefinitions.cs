namespace SuperMetroid.ResourceAudit;

/// <summary>Complete loader-owned player color domains; no palette clocks or gameplay paths are executed.</summary>
internal static class SamusColorClosedContractDefinitions
{
    /// <summary>Shared native palette definitions required by the player-color providers.</summary>
    private static readonly ReviewedSource PaletteDefinitions = new(
        "csharp/src/SuperMetroid.Core/Game/SamusPaletteRomData.cs");

    /// <summary>Reviewed Samus body, suit, charge, hurt, visor, and special
    /// effect color domains with their complete source dependencies.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.SamusFullBodyCycleColorCatalog", "samus-body-cycle-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusFullBodyCycleColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteColorDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.SamusSuitColorCatalog", "samus-suit-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusSuitColorCatalog.cs")]),
        new("SuperMetroid.Core.Assets.SamusChargeColorCatalog", "samus-charge-complete-palettes", ["ApplyCharge", "ApplyHyper", "ResolveCharge", "ResolveHyper"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusChargeColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusFullBodyCycleColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusPaletteFade.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs")]),
        new("SuperMetroid.Core.Assets.SamusHurtColorCatalog", "samus-hurt-complete-palettes", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusHurtColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusHurtColorDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.SamusHyperBeamColorCatalog", "samus-hyper-beam-complete-palettes", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs")]),
        new("SuperMetroid.Core.Assets.SamusVisorColorCatalog", "samus-visor-complete-colors", ["Resolve", "TryResolveByteOffset"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusVisorColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusVisorColorDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.CrystalFlashColorCatalog", "crystal-flash-complete-color-streams", ["ApplyBody", "ApplyBubble", "ResolveBody", "ResolveBubble"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/CrystalFlashColorCatalog.cs")]),
        new("SuperMetroid.Core.Assets.PowerBombFixedColorCatalog", "power-bomb-complete-fixed-colors", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/PowerBombFixedColorCatalog.cs")]),
        new("SuperMetroid.Core.Assets.HyperBeamFxColorCatalog", "hyper-beam-fx-complete-color-frames", ["Apply"],
            [new("csharp/src/SuperMetroid.Core/Assets/HyperBeamFxColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs")]),
    ];
}
