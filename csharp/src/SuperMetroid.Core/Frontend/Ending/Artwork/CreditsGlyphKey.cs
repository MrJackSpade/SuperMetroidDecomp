
namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>The style, character and half that select a credits glyph.</summary>
/// <param name="Style">The style.</param>
/// <param name="Character">The character.</param>
/// <param name="Bottom">The bottom.</param>
internal readonly record struct CreditsGlyphKey(CreditsLineStyle Style, char Character, bool Bottom);
