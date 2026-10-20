using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Builds one ordinary Spazer seed as a four-cell diagonal or two-cell axial strip.</summary>
internal readonly struct SpazerSeedParts(int pose) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Diagonal seeds use four cells; axial seeds use two.</summary>
    public int Count => pose < 4 ? 4 : 2;
    /// <summary>Places one native seed glyph along the selected diagonal or axial footprint.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int size = SpazerCompositionGeometryDefinitions.TileSize;
            SpazerSeedDirection direction = (SpazerSeedDirection)pose;
            int x, y, tile;
            bool flipX, flipY;
            if (pose < 4)
            {
                x = SpazerCompositionGeometryDefinitions.DiagonalFirstPairOriginX + (index / 2 + index % 2) * size;
                y = SpazerCompositionGeometryDefinitions.DiagonalFirstPairOriginY - (index / 2) * size;
                flipX = direction is SpazerSeedDirection.UpRight or SpazerSeedDirection.DownRight;
                flipY = direction is SpazerSeedDirection.DownRight or SpazerSeedDirection.DownLeft;
                if (!flipX) x = -size - x;
                if (flipY) y = -size - y;
                tile = ProjectileSpritePartDefinitions.SpazerDiagonalGlyph - index % 2;
            }
            else
            {
                bool vertical = direction is SpazerSeedDirection.Up or SpazerSeedDirection.Down;
                bool reverse = direction is SpazerSeedDirection.Down or SpazerSeedDirection.Right;
                int along = (reverse ? index - 1 : -index) * size;
                x = vertical ? -size / 2 : along;
                y = vertical ? along : SpazerCompositionGeometryDefinitions.HorizontalStripOriginY;
                flipX = direction == SpazerSeedDirection.Right;
                flipY = direction == SpazerSeedDirection.Down;
                tile = vertical ? ProjectileSpritePartDefinitions.SpazerVerticalGlyph : ProjectileSpritePartDefinitions.SpazerHorizontalGlyph;
            }
            var flips = (flipX ? SnesTileFlipFlags.Horizontal : 0) | (flipY ? SnesTileFlipFlags.Vertical : 0);
            return new(SnesSpritemapXWord.Create(x, false), SnesSpritemapYByte.Create(y),
                SnesObjAttributeWord.Create(tile, ProjectileSpritePartDefinitions.PowerPalette, ProjectileSpritePartDefinitions.PowerPriority, flips), false);
        }
    }
    /// <summary>Yields the seed's OAM cells in the chosen travel order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the Spazer seed cells in their selected travel direction.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
