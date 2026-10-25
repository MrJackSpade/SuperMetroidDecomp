using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Ordinary diagonal startup strips keep the required two-pair origin and shift by half a cell when shortened to one pair. Pair adjacency/reflection calculate; selected footprint lengths, origin, glyphs and order remain REQUIRED.</summary>
internal readonly struct SpazerDiagonalStartupParts(int group, int pairs) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>One or two diagonal tile pairs occupy two or four OAM entries.</summary>
    public int Count => pairs * 2;
    /// <summary>Places one half of the selected diagonal startup pair.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int size = SpazerCompositionGeometryDefinitions.TileSize;
            int x = SpazerCompositionGeometryDefinitions.DiagonalFirstPairOriginX + (2 - pairs) * size / 2 +
                (index / 2 + index % 2) * size;
            int y = SpazerCompositionGeometryDefinitions.DiagonalFirstPairOriginY + (pairs - 2) * size / 2 - index / 2 * size;
            bool flipX = group != 0, flipY = group == 2;
            if (!flipX) x = -size - x;
            if (flipY) y = -size - y;
            var flips = (flipX ? TileFlip.Horizontal : 0) | (flipY ? TileFlip.Vertical : 0);
            return new(SpritePart.Create(x, y, false,
                ProjectileSpritePartDefinitions.SpazerDiagonalGlyph - index % 2, ProjectileSpritePartDefinitions.PowerPalette, ProjectileSpritePartDefinitions.PowerPriority, flips), false);
        }
    }
    /// <summary>Yields the selected diagonal startup pairs in OAM order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes startup pairs in their native diagonal OAM order.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
