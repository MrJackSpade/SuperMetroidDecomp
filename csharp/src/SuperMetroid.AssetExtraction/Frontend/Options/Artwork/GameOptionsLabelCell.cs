namespace SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

/// <summary>One cell of a controller action's label (#1467): the page cell it lies on and its place in the label's glyph.</summary>
/// <param name="PageCell">The page cell, row by row.</param>
/// <param name="GlyphCell">The cell's place in the glyph, row by row.</param>
internal readonly record struct GameOptionsLabelCell(int PageCell, int GlyphCell);
