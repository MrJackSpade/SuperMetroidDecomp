namespace SuperMetroid.Core.Assets;

/// <summary>Defines the complete editable Samus full-body cycle palette set.</summary>
public sealed record SamusFullBodyCycleColorDocument
{
    /// <summary>Gets the document schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Suit order: Power, Varia, Gravity; each contains four 16-color shades.</summary>
    public required PaletteRgb5[][][] SpeedBooster { get; init; }
    /// <summary>Gets Power, Varia, and Gravity Screw Attack palettes, each with four 16-color shades.</summary>
    public required PaletteRgb5[][][] ScrewAttack { get; init; }
    /// <summary>Gets Power, Varia, and Gravity stored-shine palettes, each with four 16-color shades.</summary>
    public required PaletteRgb5[][][] StoredShine { get; init; }
    /// <summary>Gets Power, Varia, and Gravity active-shinespark palettes, each with four 16-color shades.</summary>
    public required PaletteRgb5[][][] ActiveShinespark { get; init; }
}
