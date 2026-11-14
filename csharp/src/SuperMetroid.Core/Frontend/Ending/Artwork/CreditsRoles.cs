using static SuperMetroid.Core.Frontend.Ending.Artwork.CreditsRollDefinitions;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The credits' 67 staff lines in roll order (<see cref="CreditsRollDefinitions"/>): each line's role, which keys its
/// editable text and names its picture, its glyph style and the blank space before it. The wording remains editable
/// content.
/// </summary>
public static class CreditsRoles
{
    /// <summary>Every staff line in roll order: section headings in the small glyphs, names in the large.</summary>
    public static IReadOnlyList<CreditsLineDefinition> Lines { get; } =
    [
        new("staff-heading", CreditsLineStyle.Small, InitialSpace),
        Heading("producer-heading"),
        Name("producer-name"),
        Heading("director-heading"),
        Name("director-name"),
        Heading("background-designers-heading"),
        Name("background-designer-1"),
        Name("background-designer-2"),
        Name("background-designer-3"),
        Heading("object-designers-heading"),
        Name("object-designer-1"),
        Name("object-designer-2"),
        Heading("samus-original-designer-heading"),
        Name("samus-original-designer-name"),
        Heading("samus-designer-heading"),
        Name("samus-designer-name"),
        Heading("sound-program-heading"),
        PairedHeading("sound-effects-heading"),
        Name("sound-programmer-name"),
        Heading("music-composers-heading"),
        Name("music-composer-1"),
        Name("music-composer-2"),
        Heading("program-director-heading"),
        Name("program-director-name"),
        Heading("system-coordinator-heading"),
        Name("system-coordinator-name"),
        Heading("system-programmer-heading"),
        Name("system-programmer-name"),
        Heading("samus-programmer-heading"),
        Name("samus-programmer-name"),
        Heading("event-programmer-heading"),
        Name("event-programmer-name"),
        Heading("enemy-programmer-heading"),
        Name("enemy-programmer-name"),
        Heading("map-programmer-heading"),
        Name("map-programmer-name"),
        Heading("assistant-programmer-heading"),
        Name("assistant-programmer-name"),
        Heading("coordinators-heading"),
        Name("coordinator-1"),
        Name("coordinator-2"),
        Heading("printed-art-work-heading"),
        Name("printed-art-work-1"),
        Name("printed-art-work-2"),
        Name("printed-art-work-3"),
        Name("printed-art-work-4"),
        Name("printed-art-work-5"),
        Name("printed-art-work-6"),
        Heading("special-thanks-heading"),
        Name("special-thanks-01"),
        Name("special-thanks-02"),
        Name("special-thanks-03"),
        Name("special-thanks-04"),
        Name("special-thanks-05"),
        Name("special-thanks-06"),
        Name("special-thanks-07"),
        Name("special-thanks-08"),
        Name("special-thanks-09"),
        Name("special-thanks-10"),
        Name("special-thanks-11"),
        Name("special-thanks-12"),
        Name("special-thanks-13"),
        Name("special-thanks-14"),
        Name("special-thanks-15"),
        Name("special-thanks-r-and-d"),
        Heading("general-manager-heading"),
        Name("general-manager-name"),
    ];

    /// <summary>A section's heading line <paramref name="role"/>, in the small glyphs, a section's space below the line before it.</summary>
    private static CreditsLineDefinition Heading(string role) => new(role, CreditsLineStyle.Small, SectionSpace);

    /// <summary>A heading line <paramref name="role"/> paired with the heading before it, in the small glyphs, directly beneath it.</summary>
    private static CreditsLineDefinition PairedHeading(string role) => new(role, CreditsLineStyle.Small, SpaceBefore: 0);

    /// <summary>A name line <paramref name="role"/>, in the large glyphs, a line's space below the line before it.</summary>
    private static CreditsLineDefinition Name(string role) => new(role, CreditsLineStyle.Large, InterLineSpace);
}
