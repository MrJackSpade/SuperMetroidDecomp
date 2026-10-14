namespace SuperMetroid.Core.Assets;

/// <summary>Rectangular placement and font style of an ending-text field.</summary>
/// <param name="Row">Zero-based destination tile row.</param>
/// <param name="Column">Zero-based destination tile column.</param>
/// <param name="Width">Field width in glyphs.</param>
/// <param name="Style">Font layout and attributes used by the field.</param>
public readonly record struct EndingTextRegionDefinition(
    int Row, int Column, int Width, EndingTextStyle Style);
