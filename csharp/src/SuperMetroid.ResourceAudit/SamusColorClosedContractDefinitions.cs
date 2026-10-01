namespace SuperMetroid.ResourceAudit;

/// <summary>Complete loader-owned player color domains; no palette clocks or gameplay paths are executed.</summary>
internal static class SamusColorClosedContractDefinitions
{
    private static readonly ReviewedSource PaletteDefinitions = new(
        "csharp/src/SuperMetroid.Core/Game/SamusPaletteRomData.cs",
        "8063D11FFF1947241535DB52D3372C904AB7FE5BFD291B4EDECC1D567C7BAE1C");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.SamusFullBodyCycleColorCatalog", "samus-body-cycle-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusFullBodyCycleColorCatalog.cs", "532C6A8B89094D982E3B4A9AE845BCE5E92FDE35FFA30432E2136379075AA15C")],
            "Private construction follows validation of all four families, each with three suits and four sixteen-color shades. The loader compiles independent arrays at every guarded native selector pointer and rejects collisions. Apply/Resolve guard that complete sparse pointer set and color bounds; palette clocks and restored caller state are not certified."),
        new("SuperMetroid.Core.Assets.SamusSuitColorCatalog", "samus-suit-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusSuitColorCatalog.cs", "A6BDFA9818BD551DE2DC0E2BE5886C943755053BE08905DA89487908D3B75FE7")],
            "The sole private-constructor loader requires three sixteen-color RGB5 suit arrays and compiles independent words. Apply/Resolve accept only even suit offsets zero/two/four and bounded color indices. Equipment priority and suit selection remain gameplay responsibilities."),
        new("SuperMetroid.Core.Assets.SamusChargeColorCatalog", "samus-charge-complete-palettes", ["ApplyCharge", "ApplyHyper", "ResolveCharge", "ResolveHyper"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusChargeColorCatalog.cs", "7660DDF98B73CF8A04FC46A80D6E64ED28B1DC2BC61257C39C2EA4F2781EEC70")],
            "Private construction requires charged and pseudo-Screw families with three suits/six phases each plus ten Hyper-shot frames, all sixteen colors. Reviewed selectors guard the complete independent arrays. Both boolean branches are covered; charging and shot timing are not executed."),
        new("SuperMetroid.Core.Assets.SamusHurtColorCatalog", "samus-hurt-complete-palettes", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusHurtColorCatalog.cs", "F3E86B43D0F0631D9AAB5C5C059B26D6554820CBF1F002E78726198D99D31C42")],
            "Private construction requires both hurt/intro sixteen-color arrays and compiles independent words. Resolve guards the exact variant enum and color bounds. Damage counters, knockback and restoration timing are not certified."),
        new("SuperMetroid.Core.Assets.SamusHyperBeamColorCatalog", "samus-hyper-beam-complete-palettes", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs", "76BF9ADFF6B137D5EFD7F04F6DC37C80EF109D6AB0B7D8C250659B595DE40B8A")],
            "The private-constructor loader requires all ten sixteen-color frames and compiles independent arrays. Resolve guards frame/color bounds. Full-body cycle timing is outside this resource-domain proof."),
        new("SuperMetroid.Core.Assets.SamusVisorColorCatalog", "samus-visor-complete-colors", ["Resolve", "TryResolveByteOffset"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusVisorColorCatalog.cs", "96F815C0A4F327203015F75D71EF176CE59331CF03B27035B72C8DE72934D307")],
            "The private-constructor loader requires all six RGB5 colors and compiles independent words. Resolve guards its index. TryResolveByteOffset is a membership query: unsupported/odd offsets return false, not a missing-resource demand. Caller fallback behavior and X-ray clocks are not certified."),
        new("SuperMetroid.Core.Assets.CrystalFlashColorCatalog", "crystal-flash-complete-color-streams", ["ApplyBody", "ApplyBubble", "ResolveBody", "ResolveBubble"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/CrystalFlashColorCatalog.cs", "B57D161FF1784072D15FE5720C43AFA9A49208F44E16E27490CCB29551DE9445")],
            "Private construction requires ten ten-color body rows and six six-color bubble rows, compiled independently. Each reviewed method guards its own frame/color bounds rather than the other stream's larger domain. Timers, radius and healing are outside this proof."),
        new("SuperMetroid.Core.Assets.PowerBombFixedColorCatalog", "power-bomb-complete-fixed-colors", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/PowerBombFixedColorCatalog.cs", "B9FE2A0BCC2ED71399E0F7E4C41C599BFF089DDC7459BDCF65930673D8772892")],
            "Private construction requires sixteen pre-explosion and thirty-two explosion RGB5 triplets and compiles independent arrays. Resolve guards the exact sequence enum and that sequence's index. Known pre-explosion selectors cannot borrow explosion capacity. HDMA radius and phase timing are not certified."),
        new("SuperMetroid.Core.Assets.HyperBeamFxColorCatalog", "hyper-beam-fx-complete-color-frames", ["Apply"],
            [new("csharp/src/SuperMetroid.Core/Assets/HyperBeamFxColorCatalog.cs", "4A1E4425A87B558DA8D9EEB36F8B0089BE63464A905ED9AC12631DDCFCF5D903")],
            "The sole private-constructor loader requires ten eight-color frames and compiles independent arrays. Apply guards frame bounds. The CGRAM destination is placement, not a resource identity; palette-FX instructions and timing remain outside this proof."),
    ];
}
