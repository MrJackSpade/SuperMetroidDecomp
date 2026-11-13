namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Fixed layout limits and typing cadence of the opening narration.</summary>
public static class IntroNarrationDefinitions
{
    /// <summary>Zero-based page column at which every narration line begins: column one.</summary>
    public const int FirstColumn = 1;
    /// <summary>Maximum glyph count per line, 29, occupying glyph columns one through 29 including spaces.</summary>
    public const int MaximumColumns = 29;
    /// <summary>First permitted text row in page cells, four; presentation lines use even rows with two-row spacing.</summary>
    public const int FirstTextRow = 4;
    /// <summary>Five cinematic background-object updates between character records, including spaces; independent of host elapsed time.</summary>
    public const int CharacterDelayFrames = 5;
    /// <summary>One-update duration of the no-op marker at the beginning of each page stream.</summary>
    public const int InitialMarkerDelayFrames = 1;
    /// <summary>128 cinematic background-object updates held after the final page starts caret blinking and before its finishing cue.</summary>
    public const int FinalPageHoldFrames = 128;
}
