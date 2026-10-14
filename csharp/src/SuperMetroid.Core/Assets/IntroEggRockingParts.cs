using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>Intact egg and two opposite row-sheared rocking poses.</summary>
internal sealed class IntroEggRockingParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Recalculates an egg frame only when its native sprite pointer selects one of the three rocking poses.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 3; frame++)
            if (IntroDiscoveryActorSpriteDefinitions.EggFramePointer(frame) == pointer)
                return supplied.CalculateIfMatching(new IntroEggRockingParts(frame));
        return supplied;
    }
    /// <summary>The intact egg uses a large lower-left part; either sheared pose splits it into four small parts.</summary>
    public int Count => frame == 0 ? 6 : 9;
    /// <summary>Places an egg atlas part on its grid, shearing each rocking row according to the selected pose.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int row, column, tile;
            bool large = frame == 0 && index == 3;
            if (index < 3)
            {
                row = 0;
                column = 2 - index;
                tile = IntroEggRockingAtlas.TopStrip + column;
            }
            else if (large)
            {
                row = 1;
                column = 0;
                tile = IntroEggRockingAtlas.LowerLeft;
            }
            else
            {
                int piece = index - (frame == 0 ? 4 : 3);
                if (piece < 2)
                {
                    row = 2 - piece;
                    column = 2;
                    tile = IntroEggRockingAtlas.RightColumn + row - 1;
                }
                else
                {
                    int grid = piece - 2;
                    row = 2 - grid / 2;
                    column = 1 - grid % 2;
                    tile = IntroEggRockingAtlas.LowerLeft + 16 * (row - 1) + column;
                }
            }
            int shear = frame == 0 ? 0 : (frame == 1 ? -1 : 1) * (3 - row);
            return new(SnesSpritemapXWord.Create(-12 + 8 * column + shear, large),
                unchecked((byte)(-12 + 8 * row)), SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
        }
    }
    /// <summary>Enumerates egg parts in the cartridge's top-strip then lower-body drawing order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same drawing order to callers using the non-generic collection interface.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
