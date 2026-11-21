namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>One of the ending's colour resources the import decodes (#1544): what it is, and how many colours it holds.</summary>
/// <param name="Kind">Whether the resource is a whole screen's colours, a run of them, or the logo's crossfade.</param>
/// <param name="ColorCount">The colours the whole resource holds.</param>
internal readonly record struct EndingPaletteDefinition(EndingPaletteKind Kind, int ColorCount);
