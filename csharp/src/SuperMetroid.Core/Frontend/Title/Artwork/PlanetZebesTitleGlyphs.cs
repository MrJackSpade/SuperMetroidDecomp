namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>
/// Selects the PLANET ZEBES title's glyph tiles (<see cref="PlanetZebesTitleGlyphDefinitions"/>).
/// </summary>
internal static class PlanetZebesTitleGlyphs
{
    /// <summary>Named glyph selection in the Ceres OBJ atlas: P at8F and
    /// the remaining distinct title letters L,A,N,E,T,Z,B,S at97..9E.
    /// Repeated E glyphs use the same tile. This is a character-to-glyph dispatch.</summary>
    internal static int Tile(char glyph) =>
        Ensure.Listed(PlanetZebesTitleGlyphDefinitions.Tiles, glyph);
}
