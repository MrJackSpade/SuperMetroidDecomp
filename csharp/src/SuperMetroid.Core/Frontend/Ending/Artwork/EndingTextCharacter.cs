namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>One typed ending-text character (#1467): where it is placed and which glyph it shows.</summary>
/// <param name="Row">Zero-based destination cell row.</param>
/// <param name="Column">Zero-based destination cell column.</param>
/// <param name="Style">The text style whose glyph it shows; a large style's glyph is two rows tall.</param>
/// <param name="Character">The character: a space, which shows nothing, or a letter.</param>
public readonly record struct EndingTextCharacter(int Row, int Column, EndingTextStyle Style, char Character);
