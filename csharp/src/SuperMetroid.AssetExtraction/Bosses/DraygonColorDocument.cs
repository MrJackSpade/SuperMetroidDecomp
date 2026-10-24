using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Defines editable RGB5 colors for Draygon's opening, combat, flash, and health states.</summary>
public sealed record DraygonColorDocument
{
    /// <summary>Gets the document schema version.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the 25-color opening palette.</summary>
    public required PaletteRgb5[] Intro { get; init; }
    /// <summary>Gets the 16 normal background colors.</summary>
    public required PaletteRgb5[] Background { get; init; }
    /// <summary>Gets the 16 normal sprite colors.</summary>
    public required PaletteRgb5[] Sprite { get; init; }
    /// <summary>Gets the 16 colors used by a white hurt-flash frame.</summary>
    public required PaletteRgb5[] WhiteFlash { get; init; }
    /// <summary>Gets eight health-band rows containing four colors each.</summary>
    public required PaletteRgb5[][] HealthBands { get; init; }
}
