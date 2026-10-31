namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Schema and identities for the two escape-warning text programs.</summary>
public static class EscapeTypewriterDefinitions
{
    /// <summary>The key click the Zebes typewriter plays.</summary>
    public static readonly SoundEffectId ZebesClick = Library3Sound.IntroTypewriter;

    /// <summary>The key click the Ceres typewriter plays for every other glyph.</summary>
    public static readonly SoundEffectId CeresClick = Library2Sound.CeresTypewriter;

    /// <summary>The supported editable escape-typewriter JSON schema version.</summary>
    public const int Version = 2;

    /// <summary>The embedded editable escape-warning program asset file name.</summary>
    public const string FileName = "escape-typewriter.json";

    /// <summary>The delay between typed characters; unchanged and outside the installed programs table payload. No timing exemption is claimed.</summary>
    public const int CharacterDelayFrames = 2;
    /// <summary>Maximum authored warning length and BG tilemap row width, in character cells.</summary>
    public const int MaximumLineLength = 32;

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

    /// <summary>Reviewed five-cell warning-block margin; a chosen text-composition input, not derived from game state.</summary>
    internal const int FirstColumn = 5;
    /// <summary>Reviewed first tile row eight; a chosen text-composition input.</summary>
    internal const int FirstRow = 8;
    /// <summary>Reviewed two-row leading; preserves the selected one-blank-row warning typography.</summary>
    internal const int LineRowStep = 2;
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
