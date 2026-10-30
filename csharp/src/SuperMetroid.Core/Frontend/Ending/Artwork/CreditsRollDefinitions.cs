namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>The credits roll's row layout: the blank rows between the staff roles and the roll's length (#1467).</summary>
public static class CreditsRollDefinitions
{
    /// <summary>Eight blank tile rows preceding the first staff heading; this is a row count, not a frame delay.</summary>
    public const int InitialBlankRows = 8;
    /// <summary>Sixteen blank tile rows before a new section heading, except the initial heading and paired sound-effects heading.</summary>
    public const int SectionBlankRows = 16;
    /// <summary>One blank tile row before each non-heading line, separating large names within a section.</summary>
    public const int InterLineBlankRows = 1;
    /// <summary>Thirty-five blank tile rows following the final general-manager name, retaining the native scrolling tail.</summary>
    public const int TrailingBlankRows = 35;
    /// <summary>Total expanded scrolling stream length, 520 tile rows including headings, two-row names, and blank spacing; not the 128-row source tilemap size.</summary>
    public const int ExpectedCompiledRows = 520;
}
