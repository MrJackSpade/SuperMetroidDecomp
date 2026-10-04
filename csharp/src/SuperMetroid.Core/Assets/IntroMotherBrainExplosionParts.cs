using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Centered flashes followed by mirrored explosion quadrants in native OAM order.</summary>
internal sealed class IntroMotherBrainExplosionParts(bool big, int frame) : IReadOnlyList<CompiledSpritePart>
{
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int index = 0; index < 12; index++)
            if (IntroMotherBrainExplosionSpriteDefinitions.FramePointer(index >= 6, index % 6) == pointer)
                return supplied.CalculateIfMatching(new IntroMotherBrainExplosionParts(index >= 6, index % 6));
        return supplied;
    }
    public int Count => frame < (big ? 1 : 2) ? 1 : 4;
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            bool large = big && frame >= 2;
            int tile;
            int x, y;
            SnesTileFlipFlags flips = 0;
            if (Count == 1)
            {
                tile = big ? IntroMotherBrainExplosionAtlas.BigFlash
                    : frame == 0 ? IntroMotherBrainExplosionAtlas.SmallFlash
                    : IntroMotherBrainExplosionAtlas.SmallExpandedFlash;
                x = y = -4;
            }
            else
            {
                int column = big ? index % 2 : index / 2;
                int row = big ? index / 2 : index % 2;
                int size = large ? 16 : 8;
                x = -size * column;
                y = -size * row;
                flips = (column == 0 ? SnesTileFlipFlags.Horizontal : 0)
                    | (row == 0 ? SnesTileFlipFlags.Vertical : 0);
                tile = big
                    ? frame == 1 ? IntroMotherBrainExplosionAtlas.BigInitialQuadrant
                        : IntroMotherBrainExplosionAtlas.BigQuadrantStart + 2 * (frame - 2)
                    : IntroMotherBrainExplosionAtlas.SmallQuadrantStart + frame - 2;
            }
            return new(SnesSpritemapXWord.Create(x, large), unchecked((byte)y),
                SnesObjAttributeWord.Create(tile, 0, 3, flips), true);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal static class IntroMotherBrainExplosionAtlas
{
    /// <summary>Tile$53, initial centered small explosion flash in8C:97F7.</summary>
    internal const int SmallFlash = 0x53;
    /// <summary>Tile$51, second centered small explosion flash in8C:97FE.</summary>
    internal const int SmallExpandedFlash = 0x51;
    /// <summary>Tile$5F, initial centered big explosion flash in8C:985D.</summary>
    internal const int BigFlash = 0x5f;
    /// <summary>Tile$8A, eight-pixel quadrant before the big explosion expands.</summary>
    internal const int BigInitialQuadrant = 0x8a;
    /// <summary>Tile$60, first of four consecutive small explosion quadrants.</summary>
    internal const int SmallQuadrantStart = 0x60;
    /// <summary>Tile$90, first of four adjacent sixteen-pixel big explosion quadrants.</summary>
    internal const int BigQuadrantStart = 0x90;
}
