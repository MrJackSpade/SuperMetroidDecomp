namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>The opening narration's alphabet.</summary>
public static class IntroNarrationGlyphs
{
    /// <summary>Every character the narration supports.</summary>
    public const string Alphabet = " ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.,'!";

    /// <summary>Checks whether a character can be typed without changing case or substituting punctuation.</summary>
    /// <param name="character">The UTF-16 character from an editable narration line.</param>
    /// <returns>True only for the space, A-Z, 0-9, period, comma, straight-apostrophe, and exclamation alphabet.</returns>
    public static bool IsSupportedGlyph(char character) => Alphabet.Contains(character, StringComparison.Ordinal);
}
