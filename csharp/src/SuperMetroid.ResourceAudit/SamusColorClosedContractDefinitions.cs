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
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteColorDefinitions.cs", "22521B690AF6FFFD1C89A4271C2F3BF97253890355253E559849B210A4D5D697")],
            "Private construction follows validation of all four families, each with three suits and four sixteen-color shades. The loader validates complete rows at calculated allocation indices and rejects collisions, then stores unique color inputs and differing overrides while calculating opaque base-row, transparent-entry and cross-suit aliases plus stored-shine quarter-white interpolation and Speed Booster base tints and dim-endpoint brightening with shared blue channels, and active-shinespark warm tints and gold ramps, plus Screw Attack green/blue ramps and Power suit-ink ramps. Seven Speed Booster endpoints store differing channels only; two Gravity base inks share Power channels. Active-shine and Screw Attack endpoints likewise retain only differing channels; Varia Screw slot2 interpolates red between supplied endpoints. Apply/Resolve require one of48 aligned native palette identities and bounded color indices; palette clocks and restored caller state are not certified."),
        new("SuperMetroid.Core.Assets.SamusSuitColorCatalog", "samus-suit-complete-palettes", ["Apply", "Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusSuitColorCatalog.cs", "F4FC4C4FB0445CED4A7190B442390148D3398A6964293DE80202316C01204561")],
            "The sole private-constructor loader requires three sixteen-color RGB5 suit inputs, stores the Power colors and only differing Varia/Gravity slots, preserving independent edits through explicit overrides. Apply/Resolve accept only even suit offsets zero/two/four and bounded color indices. Equipment priority and suit selection remain gameplay responsibilities."),
        new("SuperMetroid.Core.Assets.SamusChargeColorCatalog", "samus-charge-complete-palettes", ["ApplyCharge", "ApplyHyper", "ResolveCharge", "ResolveHyper"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusChargeColorCatalog.cs", "4DABF8D704DE81CF4B5E673090203DFA04B8CD82F45DE578E2B7EB085DBC400D"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusFullBodyCycleColorCatalog.cs", "D169D74BA1093A2C2019A7B68E3228E6AE862813725F56B8118950C469F63274"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusPaletteFade.cs", "4CBE656DC00C6CF33257B12449042328AB3C97DA6DD1901816F82B138BBC7C23"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs", "326C289DAFB66BAAFFFA316DC42C6F10C9DE76AA492DB9407C6FF88E911B626E"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "75AD4BE101D4CF0C9230325B23C505015933CFAF99B6D40135050635A5AB4CD9")],
            "Private construction requires charged and pseudo-Screw families with three suits/six phases each plus ten Hyper-shot frames, all sixteen colors. Charge-family selectors guard the complete supplied domains; repeated phases and suits share inputs, charge shades calculate eighth whitening, and pseudo-Screw bright shades reuse full-body tint operations with independently supplied channel overrides. Hyper-shot rows use an independently loaded shared Hyper Beam calculation with reversed frame indices; the common loader validates all rows and channels. Both boolean branches are covered; charging and shot timing are not executed."),
        new("SuperMetroid.Core.Assets.SamusHurtColorCatalog", "samus-hurt-complete-palettes", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusHurtColorCatalog.cs", "7C63BFCF04EE45B5D2A32A64AE95E7F57EA6D4FA7B092FB686ADDA906CCBA526"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusHurtColorDefinitions.cs", "165E73692E14F525887B2FB656DA759090870547FD3DC44D8D616F077860D5B2")],
            "Private construction requires both hurt/intro sixteen-color arrays and calculates shared intro channels and hurt whitening from required source levels with independent overrides. Resolve guards the exact variant enum and color bounds. Damage counters, knockback and restoration timing are not certified."),
        new("SuperMetroid.Core.Assets.SamusHyperBeamColorCatalog", "samus-hyper-beam-complete-palettes", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs", "326C289DAFB66BAAFFFA316DC42C6F10C9DE76AA492DB9407C6FF88E911B626E"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "75AD4BE101D4CF0C9230325B23C505015933CFAF99B6D40135050635A5AB4CD9")],
            "The private-constructor loader requires all ten sixteen-color frames and stores canonical input colors plus differing overrides, calculating repeated ink and transparent aliases plus the green-to-yellow hue transform and regular midpoint interpolation across the original hue endpoints, including the cycle boundary. Intermediate colors store only independently supplied differing components; matching channels are calculated on resolution. Nine inks share a source ink with uniform RGB brightening throughout the cycle, with only differing channels kept as inputs. Overflowing source edits keep the independently supplied target explicit. Magenta shade overrides also share blue with resolved red. Magenta/green endpoints share blue with red and the magenta minimum comes from green-frame red; red endpoints share blue with green and red with the green endpoint maximum, preserving differing edits. Resolve guards frame/color bounds. Full-body cycle timing is outside this resource-domain proof."),
        new("SuperMetroid.Core.Assets.SamusVisorColorCatalog", "samus-visor-complete-colors", ["Resolve", "TryResolveByteOffset"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/SamusVisorColorCatalog.cs", "63BCEEA7315F048ECEC9CA273004F109035CF29E7C1E16062A70AEEB2505512B"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusVisorColorDefinitions.cs", "E92F240DA2A5B53C4E93CD60DCC02A0A405686D5D421B316A3799B7991E630D6")],
            "The private-constructor loader requires all six RGB5 colors, calculates widening/darkening phases and stores independently supplied differing words. Resolve guards its index. TryResolveByteOffset is a membership query: unsupported/odd offsets return false, not a missing-resource demand. Caller fallback behavior and X-ray clocks are not certified."),
        new("SuperMetroid.Core.Assets.CrystalFlashColorCatalog", "crystal-flash-complete-color-streams", ["ApplyBody", "ApplyBubble", "ResolveBody", "ResolveBubble"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/CrystalFlashColorCatalog.cs", "CCE87067115FF9C77EA5005114B366200CB806E50D77DCF5C0CAB9355E28E232")],
            "Private construction requires ten ten-color body rows and six six-color bubble rows, compiled independently. Each reviewed method guards its own frame/color bounds rather than the other stream's larger domain. Timers, radius and healing are outside this proof."),
        new("SuperMetroid.Core.Assets.PowerBombFixedColorCatalog", "power-bomb-complete-fixed-colors", ["Resolve"],
            [PaletteDefinitions,
             new("csharp/src/SuperMetroid.Core/Assets/PowerBombFixedColorCatalog.cs", "B9F212EAF9FAD35FAF7F1BC826C390D4F94F8397F10A21ECB1588836D1277926")],
            "Private construction requires sixteen pre-explosion and thirty-two explosion RGB5 triplets and compiles independent arrays. Resolve guards the exact sequence enum and that sequence's index. Known pre-explosion selectors cannot borrow explosion capacity. HDMA radius and phase timing are not certified."),
        new("SuperMetroid.Core.Assets.HyperBeamFxColorCatalog", "hyper-beam-fx-complete-color-frames", ["Apply"],
            [new("csharp/src/SuperMetroid.Core/Assets/HyperBeamFxColorCatalog.cs", "E6965F7D27C8AFEBE9F5A6D8303FCA8D5D5D76E1E2A54A62E263FFC27886B31B"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusHyperBeamColorCatalog.cs", "326C289DAFB66BAAFFFA316DC42C6F10C9DE76AA492DB9407C6FF88E911B626E"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "75AD4BE101D4CF0C9230325B23C505015933CFAF99B6D40135050635A5AB4CD9")],
            "The sole private-constructor loader requires ten eight-color frames and stores even hue endpoints, a shared neutral first-color intensity and differing supplied channel overrides. Odd hues calculate the upward-rounded RGB5 midpoint, including the cycle wrap. Yellow highlights calculate from green highlights by raising red to green. Saturated green/magenta endpoints transform the red endpoint channels. Paired and other endpoint colors share white/red extrema while keeping varying shade channels independent, including zero-valued edits. Red highlights interpolate red/white endpoints at fifth steps with nearest-channel rounding. Middle shades interpolate adjacent inks; only differing supplied components are retained as inputs. Red, green and magenta endpoint inks derive their equal blue channels from the corresponding red/green input, preserving independent edits. Apply guards frame bounds. The CGRAM destination is placement, not a resource identity; palette-FX instructions and timing remain outside this proof."),
    ];
}
