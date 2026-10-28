namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Mutually exclusive Font-3 glyph layouts used by the fixed staff-credit roles.</summary>
public enum CreditsLineStyle
{
    /// <summary>One 8-pixel-high tile row per glyph, used for section headings; authored text accepts spaces and uppercase letters.</summary>
    Small,
    /// <summary>Two vertically stacked tile rows per 8-by-16-pixel glyph, used for names and other credits; authored text additionally accepts ampersand and period.</summary>
    Large,
}
