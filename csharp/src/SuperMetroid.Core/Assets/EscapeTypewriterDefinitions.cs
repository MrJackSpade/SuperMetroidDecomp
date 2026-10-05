namespace SuperMetroid.Core.Assets;

/// <summary>Schema and native identities for the two escape-warning text programs.</summary>
public static class EscapeTypewriterDefinitions
{
    public const int Version = 1;
    public const string FileName = "escape-typewriter.json";
    public const int CeresSourceAddress = 0xa6c450;
    public const int ZebesSourceAddress = 0xa6c49c;
    public const ushort CharacterDelayFrames = 2;
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

    /// <summary>$A6:C456/$C4A2 destination operands: chosen five-cell left margin; still required.</summary>
    internal const int FirstColumn = 5;
    /// <summary>$A6:C456/$C4A2 destination operands: chosen first tile row eight; still required.</summary>
    internal const int FirstRow = 8;
    /// <summary>$A6:C470/$C486/$C4B4 destination operands: chosen two-row line spacing; still required.</summary>
    internal const int LineRowStep = 2;
    /// <summary>$A6:C454..C49A: chosen three-line Ceres message grouping; still required.</summary>
    private const int CeresLineCount = 3;
    /// <summary>$A6:C4A0..C4C9: chosen two-line Zebes message grouping; still required.</summary>
    private const int ZebesLineCount = 2;
    /// <summary>$A6:C456, TypewriterText_CeresEscapeTimer: BG1 tilemap VRAM identity.</summary>
    private const int CeresTilemapBase = 0x5000;
    /// <summary>$A6:C4A2, TypewriterText_ZebesEscapeTimer: BG2 tilemap VRAM identity.</summary>
    private const int ZebesTilemapBase = 0x4800;

    internal static int LineCount(EscapeTypewriterProgramId id) => id switch
    {
        EscapeTypewriterProgramId.Ceres => CeresLineCount,
        EscapeTypewriterProgramId.Zebes => ZebesLineCount,
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    internal static EscapeTypewriterLine Line(EscapeTypewriterProgramId id, int index)
    {
        if ((uint)index >= LineCount(id)) throw new ArgumentOutOfRangeException(nameof(index));
        string text = (id, index) switch
        {
            (EscapeTypewriterProgramId.Ceres, 0) => CeresSequenceWarning,
            (EscapeTypewriterProgramId.Ceres, 1) => CeresEvacuationWarning,
            (EscapeTypewriterProgramId.Ceres, 2) => CeresColonyWarning,
            (EscapeTypewriterProgramId.Zebes, 0) => ZebesBombWarning,
            (EscapeTypewriterProgramId.Zebes, 1) => ZebesEscapeWarning,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        int tilemap = id == EscapeTypewriterProgramId.Ceres ? CeresTilemapBase : ZebesTilemapBase;
        return new((ushort)(tilemap + (FirstRow + index * LineRowStep) * MaximumLineLength + FirstColumn), text);
    }

    public static int SourceAddress(EscapeTypewriterProgramId id) => id switch
    {
        EscapeTypewriterProgramId.Ceres => CeresSourceAddress,
        EscapeTypewriterProgramId.Zebes => ZebesSourceAddress,
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, "No native escape text source exists."),
    };
}
