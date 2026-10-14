namespace SuperMetroid.Core.Assets;

/// <summary>The opening-narration BG words for space and the four punctuation glyphs.</summary>
public enum IntroNarrationSymbolWord : ushort
{
    /// <summary>Blank-space BG word $002F from <c>IndirectInstructions_IntroText_Space</c> at <c>$8C:D67D</c>, without the nonblank glyphs' priority bit.</summary>
    Blank = 0x002f,
    /// <summary>High-priority period BG word $2024 from <c>IndirectInstructions_IntroText_Period</c> at <c>$8C:D75D</c>, distinct from the unused decimal-point glyph.</summary>
    Period = 0x2024,
    /// <summary>High-priority comma BG word $2025 from <c>IndirectInstructions_IntroText_Comma</c> at <c>$8C:D763</c>.</summary>
    Comma = 0x2025,
    /// <summary>High-priority apostrophe BG word $2027 from <c>IndirectInstructions_IntroText_Apostrophe</c> at <c>$8C:D76F</c>.</summary>
    Apostrophe = 0x2027,
    /// <summary>High-priority exclamation BG word $202A from <c>IndirectInstructions_IntroText_ExclamationPoint</c> at <c>$8C:D77B</c>.</summary>
    Exclamation = 0x202a,
}
