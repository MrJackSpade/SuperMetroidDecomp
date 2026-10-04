using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Native8C:9654 spells PLANET ZEBES on an eight-pixel baseline,
/// emitting non-space glyphs from right to left. Center the twelve-cell text at
/// X=-48 and Y=-8, leaving the space empty; all parts are small and unflipped.</summary>
internal sealed class PlanetZebesTitleParts : IReadOnlyList<CompiledSpritePart>
{
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied) =>
        pointer == CeresDestructionSpriteDefinitions.Title
            ? supplied.CalculateIfMatching(new PlanetZebesTitleParts()) : supplied;
    public int Count => PlanetZebesTitleAtlas.Text.Length - 1;
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int glyph = Count - 1 - index;
            int column = glyph + (glyph >= PlanetZebesTitleAtlas.SpaceColumn ? 1 : 0);
            return new(SnesSpritemapXWord.Create(-4 * PlanetZebesTitleAtlas.Text.Length + 8 * column, false),
                unchecked((byte)-8), SnesObjAttributeWord.Create(PlanetZebesTitleAtlas.Tile(PlanetZebesTitleAtlas.Text[column]), 0, 0, 0), true);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal static class PlanetZebesTitleAtlas
{
    /// <summary>English title selected by native8C:9654; the space occupies column6.</summary>
    internal const string Text = "PLANET ZEBES";
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