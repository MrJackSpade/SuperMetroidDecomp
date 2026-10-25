using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Three-cell L-shaped missile or Super Missile artwork for one diagonal travel pose.</summary>
internal readonly struct DiagonalMissileParts(int pose, bool super) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>The native diagonal missile footprint uses three cells.</summary>
    public int Count => 3;
    /// <summary>Places one corner of the L-shaped missile or Super Missile sprite.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= 3) throw new ArgumentOutOfRangeException(nameof(index));
            int corner = (index + (super ? ProjectileSpritePartDefinitions.SuperMissileFirstCorner : ProjectileSpritePartDefinitions.MissileFirstCorner)) % 3;
            int x = (super ? ProjectileSpritePartDefinitions.SuperMissileDiagonalX : ProjectileSpritePartDefinitions.MissileDiagonalX) + (corner == 0 ? 8 : 0);
            int y = (super ? ProjectileSpritePartDefinitions.SuperMissileDiagonalY : ProjectileSpritePartDefinitions.MissileDiagonalY) + (corner == 2 ? 0 : 8);
            bool flipX = pose is 1 or 2;
            bool flipY = pose >= 2;
            if (flipX) x = -8 - x;
            if (flipY) y = -8 - y;
            int tile = (super ? ProjectileSpritePartDefinitions.SuperMissileDiagonalGlyph : ProjectileSpritePartDefinitions.MissileDiagonalGlyph) + 2 - corner;
            var flips = (flipX ? TileFlip.Horizontal : 0) | (flipY ? TileFlip.Vertical : 0);
            return new(SnesSpritemapXWord.Create(x, false), SnesSpritemapYByte.Create(y),
                SnesObjAttributeWord.Create(tile, ProjectileSpritePartDefinitions.FixedProjectilePalette, ProjectileSpritePartDefinitions.SuperMissilePriority, flips), false);
        }
    }
    /// <summary>Yields the three diagonal missile corners in their cartridge order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the three diagonal missile corners to non-generic OAM readers.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
