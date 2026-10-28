namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Schema, glyph encoding, and fixed layout limits for opening narration.</summary>
public static class IntroNarrationDefinitions
{
    /// <summary>Required schema revision of the six-page editable English narration document.</summary>
    public const int Version = 1;
    /// <summary>JSON presentation-resource filename containing named pages and their ordered UTF-8 text lines.</summary>
    public const string FileName = "intro-narration.json";
    /// <summary>Zero-based BG tilemap column at which every narration line begins: column one.</summary>
    public const int FirstColumn = 1;
    /// <summary>Maximum glyph count per line, 29, occupying tile columns one through 29 including spaces.</summary>
    public const int MaximumColumns = 29;
    /// <summary>First permitted text row in BG tile cells, four; presentation lines use even rows with two-row spacing.</summary>
    public const int FirstTextRow = 4;
    /// <summary>Last permitted text row in BG tile cells, sixteen, inclusive.</summary>
    public const int LastTextRow = 16;
    /// <summary>Five cinematic background-object updates between character records, including spaces; independent of hardware lag or host elapsed time.</summary>
    public const ushort CharacterDelayFrames = 5;
    /// <summary>One-update duration of the no-op marker at the beginning of each native page stream.</summary>
    public const ushort InitialMarkerDelayFrames = 1;
    /// <summary>128 cinematic background-object updates held after the final page starts caret blinking and before its finish callback.</summary>
    public const ushort FinalPageHoldFrames = 128;
    /// <summary>Tile of uppercase A, from <c>IndirectInstructions_IntroText_A</c> at <c>$8C:D685</c>; A-Z use consecutive tiles in the upper priority tier.</summary>
    public const int UppercaseATile = 0x000;
    /// <summary>Tile of digit zero, from the glyph record at <c>$8C:D721</c>; zero through nine use consecutive upper-priority tiles.</summary>
    public const int DigitZeroTile = 0x01a;
    /// <summary>Blank-space tile from <c>IndirectInstructions_IntroText_Space</c> at <c>$8C:D67D</c>, without the nonblank glyphs' priority.</summary>
    internal const int BlankTile = 0x02f;
    /// <summary>Period tile from <c>IndirectInstructions_IntroText_Period</c> at <c>$8C:D75D</c>, distinct from the unused decimal-point glyph.</summary>
    internal const int PeriodTile = 0x024;
    /// <summary>Comma tile from <c>IndirectInstructions_IntroText_Comma</c> at <c>$8C:D763</c>.</summary>
    internal const int CommaTile = 0x025;
    /// <summary>Apostrophe tile from <c>IndirectInstructions_IntroText_Apostrophe</c> at <c>$8C:D76F</c>.</summary>
    internal const int ApostropheTile = 0x027;
    /// <summary>Exclamation tile from <c>IndirectInstructions_IntroText_ExclamationPoint</c> at <c>$8C:D77B</c>.</summary>
    internal const int ExclamationTile = 0x02a;

    /// <summary>Every character the narration supports, which <see cref="IntroNarrationGlyphs.CompileGlyph"/> accepts.</summary>
    internal const string Alphabet = " ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.,'!";
}
