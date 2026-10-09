namespace SuperMetroid.ResourceAudit;

/// <summary>Complete loader-owned player color domains; no palette clocks or gameplay paths are executed.</summary>
internal static class SamusColorClosedContractDefinitions
{
    /// <summary>Fingerprint for the native palette layout used by all reviewed Samus color providers.</summary>
    private static readonly ReviewedSource PaletteDefinitions = new(
        "csharp/src/SuperMetroid.Core/Game/SamusPaletteRomData.cs",
        "063EA177518DE0CF5C8EA5BD064C2C20E19E1A9D796A632384DDC7F6113EAE2E");

    /// <summary>Fingerprints closing the loader-owned player color provider domains.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.SamusFullBodyCycleColorCatalog", "samus-body-cycle-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusFullBodyCycleColorCatalog.cs", "F77CA5C2F6C3CB50D758F1CD5E61379520D496D88129B9A74E1D2F9507D68C4F"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "398E98A457128DB634036693BD38C41E1D670D5B0357ADE3D37012F3346343E6"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteColorDefinitions.cs", "DCB60356933F8D5A3C97E5F22DFADEAB5551CF43361986122D252D60FBF0A25C")]),
        new("SuperMetroid.Core.Assets.SamusSuitColorCatalog", "samus-suit-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusSuitColorCatalog.cs", "752788B416FB394CB53DAA3A8E6E1D58CC636B35921B3E12F4F6A14CD9E6F68F")]),
        new("SuperMetroid.Core.Assets.SamusChargeColorCatalog", "samus-charge-complete-palettes", ["ApplyCharge", "ApplyHyper", "ResolveCharge", "ResolveHyper"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusChargeColorCatalog.cs", "C0FE8794F04CEA4824D25C08E109FD54D4CCB97746265CFE4007667BF8C9D61F"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusFullBodyCycleColorCatalog.cs", "F77CA5C2F6C3CB50D758F1CD5E61379520D496D88129B9A74E1D2F9507D68C4F"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusPaletteFade.cs", "E22A292E690FB7D6BE8D1CCAB546358F1E94EBEBD175D9AD4094538A3DD8C2CE"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs", "E525E4FA1EAB6C8BFB9C91811F2EBAED14E4755348FAB16551B594DB2BC34333"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "398E98A457128DB634036693BD38C41E1D670D5B0357ADE3D37012F3346343E6")]),
        new("SuperMetroid.Core.Assets.SamusHurtColorCatalog", "samus-hurt-complete-palettes", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusHurtColorCatalog.cs", "3AC3194A45D422830DDBD4E698E982907EC7370C51C1D04D6B0BC9DFE3E2492F"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusHurtColorDefinitions.cs", "4F6607CCF06683DA02E545191704D80A8F9868B3F4F0C8AC4F84177125F0510E")]),
        new("SuperMetroid.Core.Assets.SamusHyperBeamColorCatalog", "samus-hyper-beam-complete-palettes", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs", "E525E4FA1EAB6C8BFB9C91811F2EBAED14E4755348FAB16551B594DB2BC34333"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "398E98A457128DB634036693BD38C41E1D670D5B0357ADE3D37012F3346343E6")]),
        new("SuperMetroid.Core.Assets.SamusVisorColorCatalog", "samus-visor-complete-colors", ["Resolve", "TryResolveByteOffset"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusVisorColorCatalog.cs", "7C4ADFDCED6C0B8CE8099DF0F2CC9AB35264856C00653DF31CE33152AC9D58C6"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusVisorColorDefinitions.cs", "F04D37E98BD55D0D42BF40AD6AF7BE0E59E3F070F14A1352EB57ABAAF503FE30")]),
        new("SuperMetroid.Core.Assets.CrystalFlashColorCatalog", "crystal-flash-complete-color-streams", ["ApplyBody", "ApplyBubble", "ResolveBody", "ResolveBubble"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/CrystalFlashColorCatalog.cs", "C8972014C2B4E34F4086614436CF2D709D8E00C6262FDAE784B2F44B8493A4C9")]),
        new("SuperMetroid.Core.Assets.PowerBombFixedColorCatalog", "power-bomb-complete-fixed-colors", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/PowerBombFixedColorCatalog.cs", "5BCA7D4FEDE9EC3C42DE61DC9FF9D954E0C5C5CFC927F5B5903E7F500A1A6516")]),
        new("SuperMetroid.Core.Assets.HyperBeamFxColorCatalog", "hyper-beam-fx-complete-color-frames", ["Apply"],
            [new("csharp/src/SuperMetroid.Core/Assets/HyperBeamFxColorCatalog.cs", "C547DD0A80A920C7C2B6678E2D7AF7F19AE7BDAF770D21474BEA8303ABFB81F1"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs", "E525E4FA1EAB6C8BFB9C91811F2EBAED14E4755348FAB16551B594DB2BC34333"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "398E98A457128DB634036693BD38C41E1D670D5B0357ADE3D37012F3346343E6")]),
    ];
}
