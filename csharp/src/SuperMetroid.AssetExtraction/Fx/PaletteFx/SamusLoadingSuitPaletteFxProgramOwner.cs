using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The mutually exclusive suit palette loaded around Samus.</summary>
public enum SamusLoadingSuitPaletteFxProgramOwner
{
    /// <summary>Power-suit colors.</summary>
    /// <remarks>
    /// Issue #856 / #625: all 144 colours in the Power-suit loading program
    /// match existing authored Samus palettes. Records
    /// 0, 2, 4, 6, 8 equal the normal Power Suit palette; records 1 and 3
    /// equal its first speed-boost shade; records 5 and 7 equal
    /// its second and third shades. Each comparison covers all sixteen
    /// colors, independently rechecked under #1165. LoadingPaletteColorDefinitions
    /// owns calculated sharing/shades and the specific base-ink disposition;
    /// LoadingPaletteInputView documents independent tint components. Production
    /// reads only records 0..8 and colors 0..15.
    /// </remarks>
    PowerSuit,
    /// <summary>Varia-suit colors.</summary>
    /// <remarks>
    /// Issue #857 / #625: all 144 colours in the Varia-suit loading program
    /// match existing authored Samus palettes. Records
    /// 0, 2, 4, 6, 8 equal the normal Varia Suit palette; records 1 and 3
    /// equal its first speed-boost shade; records 5 and 7 equal
    /// its second and third shades. Each comparison covers all sixteen
    /// colors, independently rechecked under #1165. LoadingPaletteColorDefinitions
    /// owns calculated sharing/shades and the specific base-ink disposition;
    /// LoadingPaletteInputView documents independent tint components. Production
    /// reads only records 0..8 and colors 0..15.
    /// </remarks>
    VariaSuit,
    /// <summary>Gravity-suit colors.</summary>
    /// <remarks>
    /// Issue #858 / #625: all 144 colours in the Gravity-suit loading program
    /// match existing authored Samus palettes. Records
    /// 0, 2, 4, 6, 8 equal the normal Gravity Suit palette; records 1 and 3
    /// equal its first speed-boost shade; records 5 and 7 equal
    /// its second and third shades. Each comparison covers all sixteen
    /// colors, independently rechecked under #1165. LoadingPaletteColorDefinitions
    /// owns calculated sharing/shades and the specific base-ink disposition;
    /// LoadingPaletteInputView documents independent tint components. Production
    /// reads only records 0..8 and colors 0..15.
    /// </remarks>
    GravitySuit,
}
