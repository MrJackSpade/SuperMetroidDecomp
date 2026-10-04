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
             new("csharp/src/SuperMetroid.Core/Assets/SamusFullBodyCycleColorCatalog.cs", "D972C0B97676960E77959B059054FD1A06E1E28C14448EE42A6B3CE1C3E7981F"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "75AD4BE101D4CF0C9230325B23C505015933CFAF99B6D40135050635A5AB4CD9"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteColorDefinitions.cs", "22521B690AF6FFFD1C89A4271C2F3BF97253890355253E559849B210A4D5D697")],
            "Private construction follows validation of all four families, each with three suits and four sixteen-color shades. The loader validates complete rows at calculated allocation indices and rejects collisions, then stores unique color inputs and differing overrides while calculating opaque base-row, transparent-entry and cross-suit aliases plus stored-shine quarter-white interpolation and Speed Booster base tints and dim-endpoint brightening with shared blue channels, and active-shinespark warm tints and gold ramps, plus Screw Attack green/blue ramps and Power suit-ink ramps. Seven Speed Booster endpoints store differing channels only; two Gravity base inks share Power channels. Active-shine and Screw Attack endpoints likewise retain only differing channels; Varia Screw slot2 interpolates red between supplied endpoints. Apply/Resolve require one of48 aligned native palette identities and bounded color indices; palette clocks and restored caller state are not certified."),
        new("SuperMetroid.Core.Assets.SamusSuitColorCatalog", "samus-suit-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusSuitColorCatalog.cs", "A1E47A88B24A4E21282174E417E5C0C1A5CBF36FBB58F46F6891C8FC0C5D7DD4")],
            "The sole private-constructor loader requires three sixteen-color RGB5 suit inputs, stores the Power colors and only differing Varia/Gravity slots, preserving independent edits through explicit overrides. Apply/Resolve accept only even suit offsets zero/two/four and bounded color indices. Equipment priority and suit selection remain gameplay responsibilities."),
        new("SuperMetroid.Core.Assets.SamusChargeColorCatalog", "samus-charge-complete-palettes", ["ApplyCharge", "ApplyHyper", "ResolveCharge", "ResolveHyper"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusChargeColorCatalog.cs", "0BEC6A9DF5B461E7199C7A97D519BA9CB841F4B679AF610B282C657AC7DC5A16"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusFullBodyCycleColorCatalog.cs", "D972C0B97676960E77959B059054FD1A06E1E28C14448EE42A6B3CE1C3E7981F"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusPaletteFade.cs", "4CBE656DC00C6CF33257B12449042328AB3C97DA6DD1901816F82B138BBC7C23"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs", "4283B5E2CED3D1D750598A5C3028CB3E2B20BF240750CC31A3E20F28EFB97521"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "75AD4BE101D4CF0C9230325B23C505015933CFAF99B6D40135050635A5AB4CD9")],
            "Private construction requires charged and pseudo-Screw families with three suits/six phases each plus ten Hyper-shot frames, all sixteen colors. Charge-family selectors guard the complete supplied domains; repeated phases and suits share inputs, charge shades calculate eighth whitening, and pseudo-Screw bright shades reuse full-body tint operations with independently supplied channel overrides. Hyper-shot rows use an independently loaded shared Hyper Beam calculation with reversed frame indices; the common loader validates all rows and channels. Both boolean branches are covered; charging and shot timing are not executed."),
        new("SuperMetroid.Core.Assets.SamusHurtColorCatalog", "samus-hurt-complete-palettes", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusHurtColorCatalog.cs", "F3E86B43D0F0631D9AAB5C5C059B26D6554820CBF1F002E78726198D99D31C42")],
            "Private construction requires both hurt/intro sixteen-color arrays and compiles independent words. Resolve guards the exact variant enum and color bounds. Damage counters, knockback and restoration timing are not certified."),
        new("SuperMetroid.Core.Assets.SamusHyperBeamColorCatalog", "samus-hyper-beam-complete-palettes", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs", "4283B5E2CED3D1D750598A5C3028CB3E2B20BF240750CC31A3E20F28EFB97521"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "75AD4BE101D4CF0C9230325B23C505015933CFAF99B6D40135050635A5AB4CD9")],
            "The private-constructor loader requires all ten sixteen-color frames and stores canonical input colors plus differing overrides, calculating repeated ink and transparent aliases plus the green-to-yellow hue transform and regular midpoint interpolation across the original hue endpoints, including the cycle boundary. Intermediate colors store only independently supplied differing components; matching channels are calculated on resolution. Nine inks share a source ink with uniform RGB brightening throughout the cycle, with only differing channels kept as inputs. Overflowing source edits keep the independently supplied target explicit. Magenta shade overrides also share blue with resolved red. Magenta/green endpoints share blue with red and the magenta minimum comes from green-frame red; red endpoints share blue with green and red with the green endpoint maximum, preserving differing edits. Resolve guards frame/color bounds. Full-body cycle timing is outside this resource-domain proof."),
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
            [new("csharp/src/SuperMetroid.Core/Assets/HyperBeamFxColorCatalog.cs", "2C6A1C6C8E24311CA7586ACA9104F80FE7DB6B93C0629C6A6D63C14861AD5C95"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs", "4283B5E2CED3D1D750598A5C3028CB3E2B20BF240750CC31A3E20F28EFB97521"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "75AD4BE101D4CF0C9230325B23C505015933CFAF99B6D40135050635A5AB4CD9")],
            "The sole private-constructor loader requires ten eight-color frames and stores even hue endpoints, a shared neutral first-color intensity and differing supplied channel overrides. Odd hues calculate the upward-rounded RGB5 midpoint, including the cycle wrap. Yellow highlights calculate from green highlights by raising red to green. Saturated green/magenta endpoints transform the red endpoint channels. Paired and other endpoint colors share white/red extrema while keeping varying shade channels independent, including zero-valued edits. Red highlights interpolate red/white endpoints at fifth steps with nearest-channel rounding. Middle shades interpolate adjacent inks; only differing supplied components are retained as inputs. Red, green and magenta endpoint inks derive their equal blue channels from the corresponding red/green input, preserving independent edits. Apply guards frame bounds. The CGRAM destination is placement, not a resource identity; palette-FX instructions and timing remain outside this proof."),
    ];
}
