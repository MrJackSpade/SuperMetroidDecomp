using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Schema and native identities for the two escape-warning text programs.</summary>
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

    /// <summary>$A6:C450/$C49C runtime delay reset; unchanged and outside the installed programs table payload. No timing exemption is claimed.</summary>
    public const ushort CharacterDelayFrames = 2;
    /// <summary>Maximum authored warning length and BG tilemap row width, in character cells.</summary>
    public const int MaximumLineLength = 32;

    /// <summary>$A6:C458, TypewriterText_CeresEscapeTimer: chosen English warning wording, retained as lexical content.</summary>
    private const string CeresSequenceWarning = "SELF DESTRUCT SEQUENCE";
    /// <summary>$A6:C472, TypewriterText_CeresEscapeTimer: chosen English warning wording, retained as lexical content.</summary>
    private const string CeresEvacuationWarning = "ACTIVATED EVACUATE";
    /// <summary>$A6:C488, TypewriterText_CeresEscapeTimer: chosen English warning wording, retained as lexical content.</summary>
    private const string CeresColonyWarning = "COLONY IMMEDIATELY";
    /// <summary>$A6:C4A4, TypewriterText_ZebesEscapeTimer: chosen English warning wording, retained as lexical content.</summary>
    private const string ZebesBombWarning = "TIME BOMB SET!";
    /// <summary>$A6:C4B6, TypewriterText_ZebesEscapeTimer: chosen English warning wording, retained as lexical content.</summary>
    private const string ZebesEscapeWarning = "ESCAPE IMMEDIATELY!";

    /// <summary>$A6:C456/$C4A2 destination operands: reviewed five-cell warning-block margin; a chosen text-composition input, not derived from game state.</summary>
    internal const int FirstColumn = 5;
    /// <summary>$A6:C456/$C4A2 destination operands: reviewed first tile row eight; a chosen text-composition input.</summary>
    internal const int FirstRow = 8;
    /// <summary>$A6:C470/$C486/$C4B4 destination operands: reviewed two-row leading; preserves the selected one-blank-row warning typography.</summary>
    internal const int LineRowStep = 2;
    /// <summary>$A6:C454..C49A: reviewed three-line grouping of the approved Ceres wording; selected line breaks are lexical composition.</summary>
    private const int CeresLineCount = 3;
    /// <summary>$A6:C4A0..C4C9: reviewed two-line grouping of the approved Zebes wording; selected line breaks are lexical composition.</summary>
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
