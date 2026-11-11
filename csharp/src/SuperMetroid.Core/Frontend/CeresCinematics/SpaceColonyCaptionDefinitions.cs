namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// The SPACE COLONY caption: its non-space letters appear left to right, one letter's width apart, and with Japanese text
/// the katakana caption shows below it. The import pictures each letter and the katakana caption.
/// </summary>
public static class SpaceColonyCaptionDefinitions
{
    /// <summary>The English caption; its one space takes a letter's width but shows nothing.</summary>
    public const string Text = "SPACE COLONY";
    /// <summary>Counts the caption's visible letters, its space left out.</summary>
    public static int LetterCount => Text.Length - 1;

    /// <summary>The pixel column of the caption's first letter's left edge.</summary>
    internal const int FirstLetterLeft = 80;

    /// <summary>The width each of the caption's characters takes, in pixels.</summary>
    internal const int LetterAdvance = 8;

    /// <summary>The pixel row of the caption's top edge.</summary>
    public const int Top = 192;

    /// <summary>The pixel column of the katakana caption's left edge, and the pixel row of its top edge.</summary>
    public const int JapaneseLeft = 96, JapaneseTop = 208;
}
