namespace SuperMetroid.ResourceAudit;

/// <summary>Complete loader-owned player color domains; no palette clocks or gameplay paths are executed.</summary>
internal static class SamusColorClosedContractDefinitions
{
    private static readonly ReviewedSource PaletteDefinitions = new(
        "csharp/src/SuperMetroid.Core/Game/SamusPaletteRomData.cs", "063EA177518DE0CF5C8EA5BD064C2C20E19E1A9D796A632384DDC7F6113EAE2E");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.SamusFullBodyCycleColorCatalog", "samus-body-cycle-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusFullBodyCycleColorCatalog.cs", "D5427688F0F468E4EFAD98CA5B7EB000F0D3914FEB86C80F241A5E86C7239B9D"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "398E98A457128DB634036693BD38C41E1D670D5B0357ADE3D37012F3346343E6"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteColorDefinitions.cs", "E9F9C58A433A1FB0F402A9040FB15A25EF7556234290E312A5DE80EC4E499F94")]),
        new("SuperMetroid.Core.Assets.SamusSuitColorCatalog", "samus-suit-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusSuitColorCatalog.cs", "67DD657CE31F20408474861935158FEFBAA77BED8A1185C744C667DA1E71484E")]),
        new("SuperMetroid.Core.Assets.SamusChargeColorCatalog", "samus-charge-complete-palettes", ["ApplyCharge", "ApplyHyper", "ResolveCharge", "ResolveHyper"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusChargeColorCatalog.cs", "9737A7BB9E87BE3D2BE7F7AC85B0B168ABFC5146CE6307B2382590E093C37AC5"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusFullBodyCycleColorCatalog.cs", "D5427688F0F468E4EFAD98CA5B7EB000F0D3914FEB86C80F241A5E86C7239B9D"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusPaletteFade.cs", "84583825D0B2866162DEB582F4C4972B0BE3D3F2384D8E73A4762DB0A3FB08ED"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs", "93565C7F96FC162C2E63A770F9457B1078D34510E58B02A3E1C81B1EC0B056A8"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "398E98A457128DB634036693BD38C41E1D670D5B0357ADE3D37012F3346343E6")]),
        new("SuperMetroid.Core.Assets.SamusHurtColorCatalog", "samus-hurt-complete-palettes", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusHurtColorCatalog.cs", "171E26408846DDFCE9CB2E5B99258E0FE31DEE4D905C8321397DA003793E2B0B"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusHurtColorDefinitions.cs", "E9D30E2FB36424EBFAE0A012E3281124E7409E0A3547322AB93794A2DB9A4F18")]),
        new("SuperMetroid.Core.Assets.SamusHyperBeamColorCatalog", "samus-hyper-beam-complete-palettes", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs", "93565C7F96FC162C2E63A770F9457B1078D34510E58B02A3E1C81B1EC0B056A8"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "398E98A457128DB634036693BD38C41E1D670D5B0357ADE3D37012F3346343E6")]),
        new("SuperMetroid.Core.Assets.SamusVisorColorCatalog", "samus-visor-complete-colors", ["Resolve", "TryResolveByteOffset"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusVisorColorCatalog.cs", "3A9D8513A9B4554C751597D474E99BDEC550D0E6830B58688A9C2766A8C1A24F"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusVisorColorDefinitions.cs", "2CA6FF83D94F33B96103F99203FC73EF53AD3683EBEDFE62D6A841E3AEE8098A")]),
        new("SuperMetroid.Core.Assets.CrystalFlashColorCatalog", "crystal-flash-complete-color-streams", ["ApplyBody", "ApplyBubble", "ResolveBody", "ResolveBubble"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/CrystalFlashColorCatalog.cs", "C987D71AA85C8EB674CCC7EC4C06ED19C128FABDF6FCD5E40CBE832360FDDBC7")]),
        new("SuperMetroid.Core.Assets.PowerBombFixedColorCatalog", "power-bomb-complete-fixed-colors", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/PowerBombFixedColorCatalog.cs", "5BCA7D4FEDE9EC3C42DE61DC9FF9D954E0C5C5CFC927F5B5903E7F500A1A6516")]),
        new("SuperMetroid.Core.Assets.HyperBeamFxColorCatalog", "hyper-beam-fx-complete-color-frames", ["Apply"],
            [new("csharp/src/SuperMetroid.Core/Assets/HyperBeamFxColorCatalog.cs", "AD957CDFCDF99815402CA4F62598A56C8904F983C6C857B83D2DF212DAD73DB3"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs", "93565C7F96FC162C2E63A770F9457B1078D34510E58B02A3E1C81B1EC0B056A8"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "398E98A457128DB634036693BD38C41E1D670D5B0357ADE3D37012F3346343E6")]),
    ];
}
