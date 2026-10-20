namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>A menu heading glyph or border cell's tile position.</summary>
/// <param name="Column">The tile column.</param>
/// <param name="Row">The tile row.</param>
internal readonly record struct MenuHeadingCell(int Column, int Row);
