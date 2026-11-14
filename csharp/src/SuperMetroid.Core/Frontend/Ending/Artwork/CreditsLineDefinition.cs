namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>The role, font style and spacing of one line of editable staff-credit content.</summary>
/// <param name="Role">The line's stable role, which keys its editable text and names its picture.</param>
/// <param name="Style">Glyph layout, which sets the line's height (<see cref="CreditsRollDefinitions"/>).</param>
/// <param name="SpaceBefore">The blank space before the line on the roll, in pixels; a scrolling-layout distance, not an update or frame delay.</param>
public readonly record struct CreditsLineDefinition(
    string Role,
    CreditsLineStyle Style,
    int SpaceBefore);
