namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The mutually exclusive suit palette loaded around Samus.</summary>
public enum SamusLoadingSuitPaletteFxProgramOwner
{
    /// <summary>Power-suit colors.</summary>
    /// <remarks>
    /// Issue #856 / #625: all 144 BGR555 words in ROM program <c>$8D:DB62</c>
    /// match existing authored Samus palettes in bank <c>$9B</c>. Records
    /// 0, 2, 4, 6, 8 equal normal Power Suit <c>$9B:9400</c>; records 1 and 3
    /// equal speed-boost shade <c>$9B:9B80</c>; records 5 and 7 equal
    /// <c>$9B:9B60</c> and <c>$9B:9B40</c>. Each comparison covers all sixteen
    /// colors, independently rechecked under #1165. LoadingPaletteColorDefinitions
    /// owns calculated sharing/shades and the specific base-ink disposition;
    /// LoadingPaletteInputView documents independent tint components. Production
    /// indexes only records 0..8 and colors 0..15 through <c>ColorPointer</c>.
    /// </remarks>
    PowerSuit,
    /// <summary>Varia-suit colors.</summary>
    /// <remarks>
    /// Issue #857 / #625: all 144 BGR555 words in ROM program <c>$8D:DCC8</c>
    /// match existing authored Samus palettes in bank <c>$9B</c>. Records
    /// 0, 2, 4, 6, 8 equal normal Varia Suit <c>$9B:9520</c>; records 1 and 3
    /// equal speed-boost shade <c>$9B:9D80</c>; records 5 and 7 equal
    /// <c>$9B:9D60</c> and <c>$9B:9D40</c>. Each comparison covers all sixteen
    /// colors, independently rechecked under #1165. LoadingPaletteColorDefinitions
    /// owns calculated sharing/shades and the specific base-ink disposition;
    /// LoadingPaletteInputView documents independent tint components. Production
    /// indexes only records 0..8 and colors 0..15 through <c>ColorPointer</c>.
    /// </remarks>
    VariaSuit,
    /// <summary>Gravity-suit colors.</summary>
    /// <remarks>
    /// Issue #858 / #625: all 144 BGR555 words in ROM program <c>$8D:DE2E</c>
    /// match existing authored Samus palettes in bank <c>$9B</c>. Records
    /// 0, 2, 4, 6, 8 equal normal Gravity Suit <c>$9B:9800</c>; records 1 and 3
    /// equal speed-boost shade <c>$9B:9F80</c>; records 5 and 7 equal
    /// <c>$9B:9F60</c> and <c>$9B:9F40</c>. Each comparison covers all sixteen
    /// colors, independently rechecked under #1165. LoadingPaletteColorDefinitions
    /// owns calculated sharing/shades and the specific base-ink disposition;
    /// LoadingPaletteInputView documents independent tint components. Production
    /// indexes only records 0..8 and colors 0..15 through <c>ColorPointer</c>.
    /// </remarks>
    GravitySuit,
}
