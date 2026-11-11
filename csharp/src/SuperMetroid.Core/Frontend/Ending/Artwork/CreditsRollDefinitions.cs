namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>The credits roll's layout in pixels: each line's height, the space between the staff roles and the roll's length (#1467, #1601).</summary>
public static class CreditsRollDefinitions
{
    /// <summary>The height of a section heading's line, in pixels.</summary>
    public const int SmallLineHeight = 8;
    /// <summary>The height of a name's line, in pixels.</summary>
    public const int LargeLineHeight = 16;
    /// <summary>The blank space before the first staff heading, in pixels; a distance, not a frame delay.</summary>
    public const int InitialSpace = 64;
    /// <summary>The blank space before a new section heading, in pixels, except the initial heading and paired sound-effects heading.</summary>
    public const int SectionSpace = 128;
    /// <summary>The blank space before each non-heading line, in pixels, separating large names within a section.</summary>
    public const int InterLineSpace = 8;
    /// <summary>The blank space following the final general-manager name, in pixels, retaining the roll's scrolling tail.</summary>
    public const int TrailingSpace = 280;
    /// <summary>The roll's whole length, in pixels: every line, the space before each and the space after the last.</summary>
    public const int Length = 4160;
    /// <summary>How far the roll scrolls on past its end before the credits finish, in pixels.</summary>
    public const int ScrollPastEnd = 8;
}
