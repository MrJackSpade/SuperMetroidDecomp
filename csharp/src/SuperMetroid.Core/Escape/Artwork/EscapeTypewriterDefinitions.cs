namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Schema and identities for the two escape-warning messages.</summary>
public static class EscapeTypewriterDefinitions
{
    /// <summary>The key click the Zebes typewriter plays.</summary>
    public static readonly SoundEffectId ZebesClick = RoomSound.IntroTypewriter;

    /// <summary>The key click the Ceres typewriter plays for every other glyph.</summary>
    public static readonly SoundEffectId CeresClick = EnemySound.CeresTypewriter;

    /// <summary>
    /// The glyphs whose typing requests the key click (#1619): every second glyph of the count written, on its even place,
    /// from the second glyph on.
    /// </summary>
    internal static readonly UpdateCadence ClickEverySecondGlyph = new(2, 0);

    /// <summary>
    /// The updates the typewriter waits after each character it types once the warning's opening characters are typed, so
    /// those later characters are typed every third update.
    /// </summary>
    public const int CharacterDelayFrames = 2;

    /// <summary>
    /// The characters at the warning's start typed with no wait after them (#2368): the first character is followed by the
    /// second on the next update, and only then does the character delay take effect.
    /// </summary>
    internal const int UndelayedOpeningCharacters = 1;

    /// <summary>The Ceres warning's first line.</summary>
    private const string CeresSequenceWarning = "SELF DESTRUCT SEQUENCE";
    /// <summary>The Ceres warning's second line.</summary>
    private const string CeresEvacuationWarning = "ACTIVATED EVACUATE";
    /// <summary>The Ceres warning's third line.</summary>
    private const string CeresColonyWarning = "COLONY IMMEDIATELY";
    /// <summary>The Zebes warning's first line.</summary>
    private const string ZebesBombWarning = "TIME BOMB SET!";
    /// <summary>The Zebes warning's second line.</summary>
    private const string ZebesEscapeWarning = "ESCAPE IMMEDIATELY!";

    /// <summary>The warning block's left margin: each line's first glyph shows 40 pixels from the layer's left.</summary>
    internal const int FirstLeft = 40;
    /// <summary>The first line's glyphs show 64 pixels from the layer's top.</summary>
    internal const int FirstTop = 64;
    /// <summary>From one line's top to the next line's, in pixels: a glyph's height and a glyph-high blank gap.</summary>
    internal const int LineSpacing = 16;
    /// <summary>How far right the next character is typed after each character, glyph or space, in pixels: one glyph's width.</summary>
    public const int GlyphAdvance = 8;
    /// <summary>The lines the Ceres warning is typed in.</summary>
    private const int CeresLineCount = 3;
    /// <summary>The lines the Zebes warning is typed in.</summary>
    private const int ZebesLineCount = 2;

    /// <summary>Each warning message's line count: Ceres uses three warning lines while Zebes uses two.</summary>
    internal static readonly IReadOnlyDictionary<EscapeTypewriterMessageId, int> LineCounts =
        new Dictionary<EscapeTypewriterMessageId, int>
        {
            [EscapeTypewriterMessageId.Ceres] = CeresLineCount,
            [EscapeTypewriterMessageId.Zebes] = ZebesLineCount,
        };

    /// <summary>The wording of each warning line, by message and line.</summary>
    internal static readonly IReadOnlyDictionary<EscapeTypewriterLineKey, string> LineTexts =
        new Dictionary<EscapeTypewriterLineKey, string>
        {
            [new(EscapeTypewriterMessageId.Ceres, 0)] = CeresSequenceWarning,
            [new(EscapeTypewriterMessageId.Ceres, 1)] = CeresEvacuationWarning,
            [new(EscapeTypewriterMessageId.Ceres, 2)] = CeresColonyWarning,
            [new(EscapeTypewriterMessageId.Zebes, 0)] = ZebesBombWarning,
            [new(EscapeTypewriterMessageId.Zebes, 1)] = ZebesEscapeWarning,
        };
}
