namespace SuperMetroid.Core.Enemies;

/// <summary>Which side of a slope block an enemy is aligned against.</summary>
internal enum SlopeSurface
{
    /// <summary>The slope's upper surface, under a floor walker.</summary>
    Top,

    /// <summary>The slope's underside, above a ceiling walker.</summary>
    Underside,
}
