namespace SuperMetroid.ResourceAudit;

/// <summary>Complete loader-owned player color domains; no palette clocks or gameplay paths are executed.</summary>
internal static class SamusColorClosedContractDefinitions
{
    private static readonly ReviewedSource PaletteDefinitions = new(
        "csharp/src/SuperMetroid.Core/Game/SamusPaletteRomData.cs",
        "E3ABE232AE6901DE0656576DF10C612529875E594E4831DD1F4E26E642C468D4");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.SamusFullBodyCycleColorCatalog", "samus-body-cycle-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusFullBodyCycleColorCatalog.cs", "D169D74BA1093A2C2019A7B68E3228E6AE862813725F56B8118950C469F63274"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "75AD4BE101D4CF0C9230325B23C505015933CFAF99B6D40135050635A5AB4CD9"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteColorDefinitions.cs", "22521B690AF6FFFD1C89A4271C2F3BF97253890355253E559849B210A4D5D697")]),
        new("SuperMetroid.Core.Assets.SamusSuitColorCatalog", "samus-suit-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusSuitColorCatalog.cs", "F4FC4C4FB0445CED4A7190B442390148D3398A6964293DE80202316C01204561")]),
        new("SuperMetroid.Core.Assets.SamusChargeColorCatalog", "samus-charge-complete-palettes", ["ApplyCharge", "ApplyHyper", "ResolveCharge", "ResolveHyper"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusChargeColorCatalog.cs", "4DE824EA89186A4C5873430483984EA7BE8AB5027263EAC272D991D9F42EE9BD"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusFullBodyCycleColorCatalog.cs", "D169D74BA1093A2C2019A7B68E3228E6AE862813725F56B8118950C469F63274"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusPaletteFade.cs", "4CBE656DC00C6CF33257B12449042328AB3C97DA6DD1901816F82B138BBC7C23"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs", "326C289DAFB66BAAFFFA316DC42C6F10C9DE76AA492DB9407C6FF88E911B626E"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "75AD4BE101D4CF0C9230325B23C505015933CFAF99B6D40135050635A5AB4CD9")]),
        new("SuperMetroid.Core.Assets.SamusHurtColorCatalog", "samus-hurt-complete-palettes", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusHurtColorCatalog.cs", "7C63BFCF04EE45B5D2A32A64AE95E7F57EA6D4FA7B092FB686ADDA906CCBA526"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusHurtColorDefinitions.cs", "165E73692E14F525887B2FB656DA759090870547FD3DC44D8D616F077860D5B2")]),
        new("SuperMetroid.Core.Assets.SamusHyperBeamColorCatalog", "samus-hyper-beam-complete-palettes", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs", "326C289DAFB66BAAFFFA316DC42C6F10C9DE76AA492DB9407C6FF88E911B626E"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "75AD4BE101D4CF0C9230325B23C505015933CFAF99B6D40135050635A5AB4CD9")]),
        new("SuperMetroid.Core.Assets.SamusVisorColorCatalog", "samus-visor-complete-colors", ["Resolve", "TryResolveByteOffset"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusVisorColorCatalog.cs", "63BCEEA7315F048ECEC9CA273004F109035CF29E7C1E16062A70AEEB2505512B"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusVisorColorDefinitions.cs", "E92F240DA2A5B53C4E93CD60DCC02A0A405686D5D421B316A3799B7991E630D6")]),
        new("SuperMetroid.Core.Assets.CrystalFlashColorCatalog", "crystal-flash-complete-color-streams", ["ApplyBody", "ApplyBubble", "ResolveBody", "ResolveBubble"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/CrystalFlashColorCatalog.cs", "CCE87067115FF9C77EA5005114B366200CB806E50D77DCF5C0CAB9355E28E232")]),
        new("SuperMetroid.Core.Assets.PowerBombFixedColorCatalog", "power-bomb-complete-fixed-colors", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/PowerBombFixedColorCatalog.cs", "B9F212EAF9FAD35FAF7F1BC826C390D4F94F8397F10A21ECB1588836D1277926")]),
        new("SuperMetroid.Core.Assets.HyperBeamFxColorCatalog", "hyper-beam-fx-complete-color-frames", ["Apply"],
            [new("csharp/src/SuperMetroid.Core/Assets/HyperBeamFxColorCatalog.cs", "E6965F7D27C8AFEBE9F5A6D8303FCA8D5D5D76E1E2A54A62E263FFC27886B31B"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs", "326C289DAFB66BAAFFFA316DC42C6F10C9DE76AA492DB9407C6FF88E911B626E"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "75AD4BE101D4CF0C9230325B23C505015933CFAF99B6D40135050635A5AB4CD9")]),
    ];
}
