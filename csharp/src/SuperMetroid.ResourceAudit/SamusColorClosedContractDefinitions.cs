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
             new("csharp/src/SuperMetroid.Core/Assets/SamusFullBodyCycleColorCatalog.cs", "118F6C23E61DA6D26B726CA75CE5CA3469A76FC86E6E6A0826EC08E8D3C124B9"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "398E98A457128DB634036693BD38C41E1D670D5B0357ADE3D37012F3346343E6"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteColorDefinitions.cs", "E9F9C58A433A1FB0F402A9040FB15A25EF7556234290E312A5DE80EC4E499F94")]),
        new("SuperMetroid.Core.Assets.SamusSuitColorCatalog", "samus-suit-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusSuitColorCatalog.cs", "A8AF3469BBE0C508E92D9836F77E1307D8A4E9353C0B05850D092A9A95627CBD")]),
        new("SuperMetroid.Core.Assets.SamusChargeColorCatalog", "samus-charge-complete-palettes", ["ApplyCharge", "ApplyHyper", "ResolveCharge", "ResolveHyper"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusChargeColorCatalog.cs", "CDB76AC830F5C819E72F6C4C69EFA092FF576E069F0153DC87A62D2E73BF8D9A"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusFullBodyCycleColorCatalog.cs", "118F6C23E61DA6D26B726CA75CE5CA3469A76FC86E6E6A0826EC08E8D3C124B9"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusPaletteFade.cs", "84583825D0B2866162DEB582F4C4972B0BE3D3F2384D8E73A4762DB0A3FB08ED"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs", "76825820A49F541FD09CE8B64FE43E71F4549D178752F72731B0C9B2149DC8E4"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "398E98A457128DB634036693BD38C41E1D670D5B0357ADE3D37012F3346343E6")]),
        new("SuperMetroid.Core.Assets.SamusHurtColorCatalog", "samus-hurt-complete-palettes", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusHurtColorCatalog.cs", "171E26408846DDFCE9CB2E5B99258E0FE31DEE4D905C8321397DA003793E2B0B"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusHurtColorDefinitions.cs", "E9D30E2FB36424EBFAE0A012E3281124E7409E0A3547322AB93794A2DB9A4F18")]),
        new("SuperMetroid.Core.Assets.SamusHyperBeamColorCatalog", "samus-hyper-beam-complete-palettes", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs", "76825820A49F541FD09CE8B64FE43E71F4549D178752F72731B0C9B2149DC8E4"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "398E98A457128DB634036693BD38C41E1D670D5B0357ADE3D37012F3346343E6")]),
        new("SuperMetroid.Core.Assets.SamusVisorColorCatalog", "samus-visor-complete-colors", ["Resolve", "TryResolveByteOffset"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusVisorColorCatalog.cs", "3A9D8513A9B4554C751597D474E99BDEC550D0E6830B58688A9C2766A8C1A24F"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusVisorColorDefinitions.cs", "2CA6FF83D94F33B96103F99203FC73EF53AD3683EBEDFE62D6A841E3AEE8098A")]),
        new("SuperMetroid.Core.Assets.CrystalFlashColorCatalog", "crystal-flash-complete-color-streams", ["ApplyBody", "ApplyBubble", "ResolveBody", "ResolveBubble"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/CrystalFlashColorCatalog.cs", "B2257FD24107D42CF2E6F01E8C5C3A583328EE399D4F8FB988CA0EFAC29CC308")]),
        new("SuperMetroid.Core.Assets.PowerBombFixedColorCatalog", "power-bomb-complete-fixed-colors", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/PowerBombFixedColorCatalog.cs", "5BCA7D4FEDE9EC3C42DE61DC9FF9D954E0C5C5CFC927F5B5903E7F500A1A6516")]),
        new("SuperMetroid.Core.Assets.HyperBeamFxColorCatalog", "hyper-beam-fx-complete-color-frames", ["Apply"],
            [new("csharp/src/SuperMetroid.Core/Assets/HyperBeamFxColorCatalog.cs", "4F2AD51F69432A91AF8746A55D0B1E9823EAFCFCFDCE87BB5279947458ECE5B9"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs", "76825820A49F541FD09CE8B64FE43E71F4549D178752F72731B0C9B2149DC8E4"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "398E98A457128DB634036693BD38C41E1D670D5B0357ADE3D37012F3346343E6")]),
    ];
}
