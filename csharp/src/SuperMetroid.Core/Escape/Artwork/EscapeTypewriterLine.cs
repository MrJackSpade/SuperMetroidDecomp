namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>One warning line's start and selected text; this value record does not check that the text fits.</summary>
/// <param name="Start">
/// The pixel of the layer, from its top-left, the line's first glyph's top-left corner shows at (#1406, #1514, #1601); the
/// line types rightward from it, one glyph's width per character.
/// </param>
/// <param name="Text">One through 32 supported characters in a loaded message. Spaces advance the typing position without writing a glyph; other characters select the caller's glyph base.</param>
public sealed record EscapeTypewriterLine(Position16<PlanePixels> Start, string Text);
