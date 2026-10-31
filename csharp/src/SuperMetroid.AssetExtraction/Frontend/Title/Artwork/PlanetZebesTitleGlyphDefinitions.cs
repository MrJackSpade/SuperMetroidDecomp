namespace SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>Title lettering and Ceres OBJ atlas glyph selection.</summary>
internal static class PlanetZebesTitleGlyphDefinitions
{
    /// <summary>English title selected by native8C:9654; the space occupies column6.</summary>
    internal const string Text = "PLANET ZEBES";
    /// <summary>Column reserved for the undrawn space between PLANET and ZEBES.</summary>
    internal const int SpaceColumn = 6;
    /// <summary>Named glyph selection in the Ceres OBJ atlas: P at8F and
    /// the remaining distinct title letters L,A,N,E,T,Z,B,S at97..9E.
    /// Repeated E glyphs use the same tile. This is a character-to-glyph dispatch.</summary>
    internal static readonly IReadOnlyDictionary<char, int> Tiles =
        new Dictionary<char, int>
        {
            ['P'] = 143,
            ['L'] = 151,
            ['A'] = 152,
            ['N'] = 153,
            ['E'] = 154,
            ['T'] = 155,
            ['Z'] = 156,
            ['B'] = 157,
            ['S'] = 158,
        };
}
