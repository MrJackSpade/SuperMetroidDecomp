namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Mutually exclusive glyph layouts used by the fixed staff-credit roles.</summary>
public enum CreditsLineStyle
{
    /// <summary>An 8-pixel-high line of 8-by-8-pixel glyphs, used for section headings; authored text accepts spaces and uppercase letters.</summary>
    Small,
    /// <summary>A 16-pixel-high line of 8-by-16-pixel glyphs, used for names and other credits; authored text additionally accepts ampersand and period.</summary>
    Large,
}
