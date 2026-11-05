namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>One warning line's first cell and selected text; this value record does not check that the text fits.</summary>
/// <param name="Cell">
/// The cell of the layer the line's first character is typed over (#1406, #1514), its column and row from the layer's
/// top-left; the line types rightward along the row.
/// </param>
/// <param name="Text">One through 32 supported characters in a loaded program. Spaces advance the cell without writing a tile; other characters select the caller's glyph base.</param>
public sealed record EscapeTypewriterLine(Position16<TilemapCells> Cell, string Text);
