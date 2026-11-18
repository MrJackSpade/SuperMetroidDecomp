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
    /// <summary>Five cinematic updates between one typed character and the next, spaces included; independent of host elapsed time.</summary>
    public const int CharacterDelayFrames = 5;
    /// <summary>
    /// The updates each page waits before it shows its subtitle, and again before its first letter: one.
    /// </summary>
    public const int PageOpeningPauseFrames = 1;
    /// <summary>128 cinematic updates the final page holds once its caret starts blinking, before the narration finishes.</summary>
    public const int FinalPageHoldFrames = 128;
}
