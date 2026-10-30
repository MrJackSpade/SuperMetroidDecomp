using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>Editable decimal glyphs and placement for the reserve-supply display, independent of its numeric value.</summary>
public sealed record PauseReserveDigitVisual
{
    /// <summary>Destination of the leftmost supply digit; the next two places occupy consecutive native words.</summary>
    public required PauseGridPoint Anchor { get; init; }
    /// <summary>Exactly ten BG glyph/attribute definitions in decimal-value order, zero through nine; not ten destination digit places.</summary>
    public required PauseBackdropCell[] Cells { get; init; }
}
