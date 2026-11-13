namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Schema and identities for the two escape-warning text programs.</summary>
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

    /// <summary>The delay between typed characters; unchanged and outside the installed programs table payload. No timing exemption is claimed.</summary>
    public const int CharacterDelayFrames = 2;

    /// <summary>Chosen English warning wording, retained as lexical content.</summary>
    private const string CeresSequenceWarning = "SELF DESTRUCT SEQUENCE";
    /// <summary>Chosen English warning wording, retained as lexical content.</summary>
    private const string CeresEvacuationWarning = "ACTIVATED EVACUATE";
    /// <summary>Chosen English warning wording, retained as lexical content.</summary>
    private const string CeresColonyWarning = "COLONY IMMEDIATELY";
    /// <summary>Chosen English warning wording, retained as lexical content.</summary>
    private const string ZebesBombWarning = "TIME BOMB SET!";
    /// <summary>Chosen English warning wording, retained as lexical content.</summary>
    private const string ZebesEscapeWarning = "ESCAPE IMMEDIATELY!";

    /// <summary>The warning block's left margin: each line's first glyph shows 40 pixels from the layer's left.</summary>
    internal const int FirstLeft = 40;
    /// <summary>The first line's glyphs show 64 pixels from the layer's top.</summary>
    internal const int FirstTop = 64;
    /// <summary>From one line's top to the next line's, in pixels: a glyph's height and a glyph-high blank gap.</summary>
    internal const int LineSpacing = 16;
    /// <summary>How far right the next character is typed after each character, glyph or space, in pixels: one glyph's width.</summary>
    public const int GlyphAdvance = 8;
    /// <summary>Reviewed three-line grouping of the approved Ceres wording; selected line breaks are lexical composition.</summary>
    private const int CeresLineCount = 3;
    /// <summary>Reviewed two-line grouping of the approved Zebes wording; selected line breaks are lexical composition.</summary>
    private const int ZebesLineCount = 2;

    /// <summary>Each warning program's line count: Ceres uses three warning lines while Zebes uses two.</summary>
    internal static readonly IReadOnlyDictionary<EscapeTypewriterProgramId, int> LineCounts =
        new Dictionary<EscapeTypewriterProgramId, int>
        {
            [EscapeTypewriterProgramId.Ceres] = CeresLineCount,
            [EscapeTypewriterProgramId.Zebes] = ZebesLineCount,
        };

    /// <summary>The reviewed wording of each warning line, by program and line.</summary>
    internal static readonly IReadOnlyDictionary<EscapeTypewriterLineKey, string> LineTexts =
        new Dictionary<EscapeTypewriterLineKey, string>
        {
            [new(EscapeTypewriterProgramId.Ceres, 0)] = CeresSequenceWarning,
            [new(EscapeTypewriterProgramId.Ceres, 1)] = CeresEvacuationWarning,
            [new(EscapeTypewriterProgramId.Ceres, 2)] = CeresColonyWarning,
            [new(EscapeTypewriterProgramId.Zebes, 0)] = ZebesBombWarning,
            [new(EscapeTypewriterProgramId.Zebes, 1)] = ZebesEscapeWarning,
        };
}
