namespace SuperMetroid.Core.Assets;

/// <summary>One primary-page language text region, recolored by replacing only its BG palette bits.</summary>
public sealed record GameOptionsLanguageRegionDocument
{
    /// <summary>Nonempty caller-owned array of row-major tile cell indexes 0-1023, unique across all four language regions.</summary>
    public required int[] Cells { get; init; }
    /// <summary>Whether this region receives the selected palette when Japanese is chosen; false instead highlights it when English is chosen.</summary>
    public required bool HighlightWhenJapanese { get; init; }
}
