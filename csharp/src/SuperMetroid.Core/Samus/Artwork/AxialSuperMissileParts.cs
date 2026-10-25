using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Four axial native poses use a centered two-cell strip, reflected with travel direction; independent glyph/style/pixel content remains required.</summary>
internal readonly struct AxialSuperMissileParts(int pose) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Two OBJ cells form the axial Super Missile strip.</summary>
    public int Count => 2;
    /// <summary>Places and optionally reflects one strip cell for the selected compass pose.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= 2) throw new ArgumentOutOfRangeException(nameof(index));
            bool vertical = (pose & 1) != 0;
            bool reflected = pose >= 2;
            int x = vertical ? -8 / 2 : -index * 8;
            int y = vertical ? (index - 1) * 8 : -8 / 2;
            if (reflected)
            {
                if (vertical) y = -8 - y;
                else x = -8 - x;
            }
            int tile = vertical ? ProjectileSpritePartDefinitions.SuperMissileVerticalGlyph + index : ProjectileSpritePartDefinitions.SuperMissileHorizontalGlyph - index;
            var flips = reflected ? (vertical ? TileFlip.Vertical : TileFlip.Horizontal) : 0;
            return new(SnesSpritemapXWord.Create(x, false), SnesSpritemapYByte.Create(y),
                SnesObjAttributeWord.Create(tile, ProjectileSpritePartDefinitions.FixedProjectilePalette, ProjectileSpritePartDefinitions.SuperMissilePriority, flips), false);
        }
    }
    /// <summary>Yields the two axial strip parts in native OAM order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the axial Super Missile's two ordered OAM parts to non-generic consumers.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
