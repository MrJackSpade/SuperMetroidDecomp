using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Builds one ordinary Spazer seed as a four-cell diagonal or two-cell axial strip.</summary>
internal readonly struct SpazerSeedParts(int pose) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>The seed direction each seed glyph phase draws: the four diagonals, then the four axes.</summary>
    private static readonly SpazerSeedDirection[] PhaseDirections =
    [
        SpazerSeedDirection.UpRight, SpazerSeedDirection.DownRight, SpazerSeedDirection.DownLeft, SpazerSeedDirection.UpLeft,
        SpazerSeedDirection.Down, SpazerSeedDirection.Left, SpazerSeedDirection.Up, SpazerSeedDirection.Right,
    ];

    /// <summary>Diagonal seeds use four cells; axial seeds use two.</summary>
    public int Count => pose < 4 ? 4 : 2;
    /// <summary>Places one native seed glyph along the selected diagonal or axial footprint.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int size = SpazerCompositionGeometryDefinitions.TileSize;
            SpazerSeedDirection direction = PhaseDirections[pose];
            TileFlip flips = SpazerCompositionGeometryDefinitions.SeedFlips[direction];
            int x, y, tile;
            if (pose < 4)
            {
                x = SpazerCompositionGeometryDefinitions.DiagonalFirstPairOriginX + (index / 2 + index % 2) * size;
                y = SpazerCompositionGeometryDefinitions.DiagonalFirstPairOriginY - (index / 2) * size;
                if (!flips.HasFlag(TileFlip.Horizontal)) x = -size - x;
                if (flips.HasFlag(TileFlip.Vertical)) y = -size - y;
                tile = ProjectileSpritePartDefinitions.SpazerDiagonalGlyph - index % 2;
            }
            else
            {
                bool vertical = direction is not (SpazerSeedDirection.Left or SpazerSeedDirection.Right);
                bool reverse = direction is SpazerSeedDirection.Down or SpazerSeedDirection.Right;
                int along = (reverse ? index - 1 : -index) * size;
                x = vertical ? -size / 2 : along;
                y = vertical ? along : SpazerCompositionGeometryDefinitions.HorizontalStripOriginY;
                tile = vertical ? ProjectileSpritePartDefinitions.SpazerVerticalGlyph : ProjectileSpritePartDefinitions.SpazerHorizontalGlyph;
            }
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
