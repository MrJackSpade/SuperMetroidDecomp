namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Compiled role, font style, and spacing for one line of editable staff-credit content.</summary>
/// <param name="Id">Stable case-sensitive role key required at this ordered document position; does not contain the editable displayed wording.</param>
/// <param name="Style">Glyph layout determining whether the line occupies one or two BG tile rows.</param>
/// <param name="BlankRowsBefore">Number of complete 32-cell blank tile rows inserted before the line; a scrolling-layout count, not an update or frame delay.</param>
public readonly record struct CreditsLineDefinition(
    string Id,
    CreditsLineStyle Style,
    int BlankRowsBefore);
