using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>$8C:9A82: suited jump-preparation pose. All pieces occupy one atlas
/// grid; draw order traverses edge strips before the interior rows.</summary>
internal sealed class EndingRewardPrepareJumpParts : IReadOnlyList<CompiledSpritePart>
{
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied) =>
        pointer == EndingRewardSpriteDefinitions.FramePointer(EndingRewardSpriteFrame.LargeSamusFromEndingPreparingToJump)
            ? supplied.CalculateIfMatching(new EndingRewardPrepareJumpParts()) : supplied;

    public int Count => 22;
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int tile;
            if (index < 2) tile = 0x100 + 16 * index; // outer left edge, top to bottom
            else if (index < 4) tile = 0xf7 - (index - 2); // upper-right cap
            else if (index < 7) tile = 0xf3 - (index - 4); // upper-left cap
            else if (index == 7) tile = EndingRewardPrepareJumpAtlas.LeftSideEdge;
            else if (index == 8) tile = EndingRewardPrepareJumpAtlas.RightSideEdge;
            else if (index == 9) tile = EndingRewardPrepareJumpAtlas.LowerEndpoint;
            else if (index < 18)
            {
                int cell = index - 10;
                tile = 0x185 - 32 * (cell / 2) - 2 * (cell % 2); // interior rows, bottom-right first
            }
            else tile = 0x107 - 2 * (index - 18); // upper interior, right to left
            int x = 8 * (tile & 15) - 38;
            int y = 8 * (tile / 16 - 16) - 35;
            bool large = index is 1 or >= 8;
            return new(SnesSpritemapXWord.Create(x, large), unchecked((byte)y),
                SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Individually ordered boundary pieces of the suited crouching pose.</summary>
internal static class EndingRewardPrepareJumpAtlas
{
    /// <summary>Tile$122, small left edge preceding the interior rows.</summary>
    internal const int LeftSideEdge = 0x122;
    /// <summary>Tile$127, large right edge preceding the interior rows.</summary>
    internal const int RightSideEdge = 0x127;
    /// <summary>Tile$1A5, large bottom endpoint beneath the paired interior rows.</summary>
    internal const int LowerEndpoint = 0x1a5;
}
