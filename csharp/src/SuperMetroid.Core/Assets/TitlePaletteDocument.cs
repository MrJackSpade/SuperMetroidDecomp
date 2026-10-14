namespace SuperMetroid.Core.Assets;

/// <summary>Editable RGB5 schema for the initial title palette, ambient color frames and copyright replacements; nested arrays remain caller-mutable until compilation.</summary>
public sealed record TitlePaletteDocument
{
    /// <summary>Schema revision; loading requires <see cref="TitlePaletteFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 256 nonnull colors in CGRAM index order, corresponding to Palettes_TitleScreen at $8C:E1E9; every channel is 0..31.</summary>
    public required PaletteRgb5[] Colors { get; init; }
    /// <summary>Eight ordered four-color RGB5 rows for $8D:C7FE's tube-light loop, targeting CGRAM 42..45; each row's ten-update duration remains compiled mechanics.</summary>
    public required PaletteRgb5[][] BabyMetroidTubeLight { get; init; }
    /// <summary>Two ordered two-color RGB5 rows for $8D:C866's display-flicker loop, targeting CGRAM 46..47; alternating one-update durations remain compiled mechanics.</summary>
    public required PaletteRgb5[][] FlickeringDisplays { get; init; }
    /// <summary>Nonnull RGB5 copyright highlight restored at CGRAM 201 on fast skip, independently editable from the initial palette's same slot.</summary>
    public required PaletteRgb5 SkipCopyrightWhite { get; init; }
    /// <summary>Nonnull RGB5 copyright tint restored at CGRAM 202 on fast skip, independently editable from the initial palette; Red is a legacy role name rather than a color constraint.</summary>
    public required PaletteRgb5 SkipCopyrightRed { get; init; }
}
