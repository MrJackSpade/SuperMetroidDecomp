namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>
/// How the escape warnings are typed (#2422): the key clicks, the typing cadence and how far each character moves the typing
/// on. The warnings' wording and where each line starts are installed content, which the import reads from the game.
/// </summary>
public static class EscapeTypewriterDefinitions
{
    /// <summary>The key click the Zebes typewriter plays.</summary>
    public static readonly SoundEffectId ZebesClick = RoomSound.Typewriter;

    /// <summary>The key click the Ceres typewriter plays for every other glyph.</summary>
    public static readonly SoundEffectId CeresClick = RoomSound.Typewriter;

    /// <summary>
    /// The glyphs whose typing requests the key click (#1619): every second glyph of the count written, on its even place,
    /// from the second glyph on.
    /// </summary>
    internal static readonly UpdateCadence GlyphClick = new(2, 0);

    /// <summary>
    /// The updates the typewriter waits after each character it types once the warning's opening characters are typed, so
    /// those later characters are typed after that wait each.
    /// </summary>
    public const int CharacterDelayFrames = 2;

    /// <summary>
    /// The characters at the warning's start typed with no wait after them (#2368): the first character is followed by the
    /// second on the next update, and only then does the character delay take effect.
    /// </summary>
    internal const int UndelayedOpeningCharacters = 1;

    /// <summary>How far right the next character is typed after each character, glyph or space, in pixels: one glyph's width.</summary>
    public const int GlyphAdvance = 8;
}
