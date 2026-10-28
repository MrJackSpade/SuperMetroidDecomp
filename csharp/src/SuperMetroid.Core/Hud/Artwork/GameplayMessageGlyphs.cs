using static SuperMetroid.Core.Hud.Artwork.GameplayMessageTitleDefinitions;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Which characters and tiles the message boxes' text alphabet holds.</summary>
public static class GameplayMessageGlyphs
{
    /// <summary>True when <paramref name="tile"/> is one of the A-Z letter tiles.</summary>
    public static bool IsLetterTile(int tile) => (uint)(tile - FirstLetterTile) < LetterCount;

    /// <summary>Determines whether a character belongs to the supported title glyph alphabet.</summary>
    public static bool IsSupportedGlyph(char character) =>
        character is ' ' or '-' or '.' or '?' or >= 'A' and <= 'Z';
}
