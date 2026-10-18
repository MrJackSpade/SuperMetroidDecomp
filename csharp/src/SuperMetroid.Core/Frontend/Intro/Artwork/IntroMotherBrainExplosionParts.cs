using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Centered flashes followed by mirrored explosion quadrants in native OAM order.</summary>
internal sealed class IntroMotherBrainExplosionParts(bool big, int frame) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Replaces a recognized small or large intro explosion pointer with its reviewed flash or quadrant composition.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int index = 0; index < 12; index++)
            if (IntroMotherBrainExplosionSpriteDefinitions.FramePointer(index >= 6, index % 6) == pointer)
                return supplied.CalculateIfMatching(new IntroMotherBrainExplosionParts(index >= 6, index % 6));
        return supplied;
    }
    /// <summary>Early flash frames use one centered part; later frames use four mirrored quadrants.</summary>
    public int Count => frame < (big ? 1 : 2) ? 1 : 4;
    /// <summary>Selects a centered flash or one mirrored quadrant at the native tile and offset.</summary>
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
                tile = big ? IntroMotherBrainExplosionCellDefinitions.BigFlash
                    : frame == 0 ? IntroMotherBrainExplosionCellDefinitions.SmallFlash
                    : IntroMotherBrainExplosionCellDefinitions.SmallExpandedFlash;
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
                    ? frame == 1 ? IntroMotherBrainExplosionCellDefinitions.BigInitialQuadrant
                        : IntroMotherBrainExplosionCellDefinitions.BigQuadrantStart + 2 * (frame - 2)
                    : IntroMotherBrainExplosionCellDefinitions.SmallQuadrantStart + frame - 2;
            }
            return new(SnesSpritemapXWord.Create(x, large), SnesSpritemapYByte.Create(y),
                SnesObjAttributeWord.Create(tile, 0, 3, flips), true);
        }
    }
    /// <summary>Yields the selected explosion's parts in native OAM order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same OAM parts to non-generic collection consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

