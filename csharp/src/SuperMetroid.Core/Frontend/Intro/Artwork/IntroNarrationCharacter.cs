namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Identifies one compiled character emitted in typewriter order.</summary>
/// <param name="Column">Zero-based tilemap column for the character.</param>
/// <param name="Row">Zero-based tilemap row for the character.</param>
/// <param name="TilemapWord">Compiled BG tilemap word for the narration glyph.</param>
/// <param name="IsSpace">Whether the source character is a space.</param>
public readonly record struct IntroNarrationCharacter(
    int Column,
    int Row,
    ushort TilemapWord,
    bool IsSpace);
