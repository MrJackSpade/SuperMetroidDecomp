namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Compiled font style and spacing for one line of editable staff-credit content.</summary>
/// <param name="Style">Glyph layout, which sets the line's height (<see cref="CreditsRollDefinitions"/>).</param>
/// <param name="SpaceBefore">The blank space before the line on the roll, in pixels; a scrolling-layout distance, not an update or frame delay.</param>
public readonly record struct CreditsLineDefinition(
    CreditsLineStyle Style,
    int SpaceBefore);
