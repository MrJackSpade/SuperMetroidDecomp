using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Intact egg and two opposite row-sheared rocking poses.</summary>
internal sealed class IntroEggRockingParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 3; frame++)
            if (IntroDiscoveryActorSpriteDefinitions.EggFramePointer(frame) == pointer)
                return supplied.CalculateIfMatching(new IntroEggRockingParts(frame));
        return supplied;
    }
    public int Count => frame == 0 ? 6 : 9;
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
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal static class IntroEggRockingAtlas
{
    /// <summary>Tile$122,start of the intact egg three-tile top strip.</summary>
    internal const int TopStrip = 0x122;
    /// <summary>Tile$110,upper-left of the two-by-two lower-left egg patch.</summary>
    internal const int LowerLeft = 0x110;
    /// <summary>Tile$100,upper of two right-column drawings stored consecutively in one atlas row.</summary>
    internal const int RightColumn = 0x100;
}
