namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Schema, alphabet, and fixed layout limits for opening narration.</summary>
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
    /// <summary>Five cinematic background-object updates between character records, including spaces; independent of host elapsed time.</summary>
    public const ushort CharacterDelayFrames = 5;
    /// <summary>One-update duration of the no-op marker at the beginning of each native page stream.</summary>
    public const ushort InitialMarkerDelayFrames = 1;
    /// <summary>128 cinematic background-object updates held after the final page starts caret blinking and before its finish callback.</summary>
    public const ushort FinalPageHoldFrames = 128;
    /// <summary>Every character the narration supports.</summary>
    public const string Alphabet = " ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.,'!";
}
