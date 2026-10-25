using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>One centered endcap pair, then two rotated pairs. Footprint, glyph/endcap selection and native traversal remain REQUIRED; cell adjacency/reflection calculate.</summary>
internal readonly struct AlternateDiagonalStartupParts(bool reflected, int phase) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Alternate diagonal startup uses one pair, then an expanded two-pair footprint.</summary>
    public int Count => phase == 0 ? 2 : 4;
    /// <summary>Places an inner or endcap glyph with optional diagonal reflection.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int size = SpazerCompositionGeometryDefinitions.TileSize;
            int x, y, tile;
            bool rotated;
            if (phase == 0)
            {
                x = -size / 2;
                y = (index - 1) * size;
                tile = ProjectileSpritePartDefinitions.SpazerDiagonalEndcapGlyph;
                rotated = index != 0;
            }
            else
            {
                int cell = index < 2 ? index : 3 - index;
                x = 0;
                y = -size / 2 + cell * size;
                tile = ProjectileSpritePartDefinitions.SpazerDiagonalInnerGlyph + cell;
                rotated = index < 2;
                if (!rotated) { x = -size - x; y = -size - y; }
            }
            if (reflected) x = -size - x;
            var flips = (rotated != reflected ? TileFlip.Horizontal : 0) |
                (rotated ? TileFlip.Vertical : 0);
            return new(SnesSpritemapXWord.Create(x, false), SnesSpritemapYByte.Create(y),
                SnesObjAttributeWord.Create(tile, ProjectileSpritePartDefinitions.PowerPalette, ProjectileSpritePartDefinitions.PowerPriority, flips), false);
        }
    }
    /// <summary>Yields the alternate diagonal startup's native pair order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the reflected or ordinary diagonal startup pair order.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
