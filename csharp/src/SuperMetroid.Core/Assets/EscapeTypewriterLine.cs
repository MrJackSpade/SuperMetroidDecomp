namespace SuperMetroid.Core.Assets;

/// <summary>One warning line's starting word address and selected text; this value record does not independently validate either field.</summary>
/// <param name="Destination">Starting VRAM word address, not a byte address or screen-pixel coordinate; loaded programs require the complete line to fit below word $8000.</param>
/// <param name="Text">One through 32 supported characters in a loaded program. Spaces advance the destination without writing a tile; other characters select the caller's glyph base.</param>
public sealed record EscapeTypewriterLine(ushort Destination, string Text);
