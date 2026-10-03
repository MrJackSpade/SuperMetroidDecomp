namespace SuperMetroid.ResourceAudit;

/// <summary>Complete loader-owned player color domains; no palette clocks or gameplay paths are executed.</summary>
internal static class SamusColorClosedContractDefinitions
{
    private static readonly ReviewedSource PaletteDefinitions = new(
        "csharp/src/SuperMetroid.Core/Game/SamusPaletteRomData.cs",
        "14630DCAAE73E472CCA4518AD4A16E27CCF3BC2EA40A718D40CBE98DC7560F18");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.SamusFullBodyCycleColorCatalog", "samus-body-cycle-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusFullBodyCycleColorCatalog.cs", "FD8836939C09098A2A87B31C32DFCD4F87DDF45AC88764083C5B7ED1FF596E1C"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "FA6859028BA9EBA2CF0B7C04C30233386959FCF4A01CD5EA62C3D58B076D4155"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteColorDefinitions.cs", "22521B690AF6FFFD1C89A4271C2F3BF97253890355253E559849B210A4D5D697")],
            "Private construction follows validation of all four families, each with three suits and four sixteen-color shades. The loader validates complete rows at calculated allocation indices and rejects collisions, then stores unique color inputs and differing overrides while calculating opaque base-row, transparent-entry and cross-suit aliases plus stored-shine quarter-white interpolation and Speed Booster base tints and dim-endpoint brightening with shared blue channels, and active-shinespark warm tints and gold ramps, plus Screw Attack green/blue ramps and Power suit-ink ramps. Seven Speed Booster endpoints store differing channels only; two Gravity base inks share Power channels. Apply/Resolve require one of48 aligned native palette identities and bounded color indices; palette clocks and restored caller state are not certified."),
        new("SuperMetroid.Core.Assets.SamusSuitColorCatalog", "samus-suit-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusSuitColorCatalog.cs", "A1E47A88B24A4E21282174E417E5C0C1A5CBF36FBB58F46F6891C8FC0C5D7DD4")],
            "The sole private-constructor loader requires three sixteen-color RGB5 suit inputs, stores the Power colors and only differing Varia/Gravity slots, preserving independent edits through explicit overrides. Apply/Resolve accept only even suit offsets zero/two/four and bounded color indices. Equipment priority and suit selection remain gameplay responsibilities."),
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
