namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>Title lettering and Ceres OBJ atlas glyph selection.</summary>
internal static class PlanetZebesTitleAtlas
{
    /// <summary>English title selected by native8C:9654; the space occupies column6.</summary>
    internal const string Text = "PLANET ZEBES";
    /// <summary>Column reserved for the undrawn space between PLANET and ZEBES.</summary>
    internal const int SpaceColumn = 6;
    /// <summary>Named glyph selection in the Ceres OBJ atlas: P at8F and
    /// the remaining distinct title letters L,A,N,E,T,Z,B,S at97..9E.
    /// Repeated E glyphs use the same tile. This is a character-to-glyph dispatch.</summary>
    internal static int Tile(char glyph) => glyph switch
    {
        'P' => 0x8f,
        'L' => 0x97,
        'A' => 0x98,
        'N' => 0x99,
        'E' => 0x9a,
        'T' => 0x9b,
        'Z' => 0x9c,
        'B' => 0x9d,
        'S' => 0x9e,
        _ => throw new ArgumentOutOfRangeException(nameof(glyph)),
    };
}
