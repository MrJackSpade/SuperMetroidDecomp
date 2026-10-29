using System.Collections;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>Native8C:9654 spells PLANET ZEBES on an eight-pixel baseline,
/// emitting non-space glyphs from right to left. Center the twelve-cell text at
/// X=-48 and Y=-8, leaving the space empty; all parts are small and unflipped.</summary>
internal sealed class PlanetZebesTitleParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Reconstructs the title lettering only for the Ceres destruction title backdrop.</summary>
    internal static SpriteComposition CalculateIfMatching(CeresDestructionBackdrop? backdrop, SpriteComposition supplied) =>
        backdrop == CeresDestructionBackdrop.Title
            ? supplied.CalculateIfMatching(new PlanetZebesTitleParts()) : supplied;
    /// <summary>All non-space glyphs in the twelve-column title.</summary>
    public int Count => PlanetZebesTitleGlyphDefinitions.Text.Length - 1;
    /// <summary>Places one atlas glyph in reverse native draw order while skipping the word space.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int glyph = Count - 1 - index;
            int column = glyph + (glyph >= PlanetZebesTitleGlyphDefinitions.SpaceColumn ? 1 : 0);
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of(-4 * PlanetZebesTitleGlyphDefinitions.Text.Length + 8 * column, -8), false, PlanetZebesTitleGlyphs.Tile(PlanetZebesTitleGlyphDefinitions.Text[column]), 0, 0, 0), true);
        }
    }
    /// <summary>Enumerates title letters from the rightmost glyph to the leftmost.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Uses the same reverse glyph order for non-generic enumeration.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
