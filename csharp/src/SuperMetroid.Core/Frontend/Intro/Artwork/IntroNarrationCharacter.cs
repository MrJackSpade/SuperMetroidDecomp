using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Identifies one compiled character emitted in typewriter order.</summary>
/// <param name="Column">Zero-based tilemap column for the character.</param>
/// <param name="Row">Zero-based tilemap row for the character.</param>
/// <param name="TilemapWord">The background tilemap entry that shows the narration glyph: its character, palette and priority.</param>
/// <param name="IsSpace">Whether the source character is a space.</param>
public readonly record struct IntroNarrationCharacter(
    int Column,
    int Row,
    SnesBgTilemapWord TilemapWord,
    bool IsSpace);
