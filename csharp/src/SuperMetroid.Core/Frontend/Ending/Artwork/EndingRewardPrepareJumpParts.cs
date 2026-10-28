using SuperMetroid.Core.Frontend.Intro;
using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>$8C:9A82: suited jump-preparation pose. All pieces occupy one atlas
/// grid; draw order traverses edge strips before the interior rows.</summary>
internal sealed class EndingRewardPrepareJumpParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Uses calculated jump-preparation geometry only for the matching stock frame; supplied edits and other reward poses keep their parts.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied) =>
        shown == EndingRewardSpritePoses.FramePose(EndingRewardSpriteFrame.LargeSamusFromEndingPreparingToJump)
            ? supplied.CalculateIfMatching(new EndingRewardPrepareJumpParts()) : supplied;

    /// <summary>The suited preparation pose has 22 parts spanning boundary strips and interior rows.</summary>
    public int Count => 22;
    /// <summary>Derives one part's atlas tile, centered offset, and size in the native edge-first draw order.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int tile;
            if (index < 2) tile = CharacterSheetGrid.Cell(16, 0) + 16 * index; // outer left edge, top to bottom
            else if (index < 4) tile = CharacterSheetGrid.Cell(15, 7) - (index - 2); // upper-right cap
            else if (index < 7) tile = CharacterSheetGrid.Cell(15, 3) - (index - 4); // upper-left cap
            else if (index == 7) tile = EndingRewardPrepareJumpCellDefinitions.LeftSideEdge;
            else if (index == 8) tile = EndingRewardPrepareJumpCellDefinitions.RightSideEdge;
            else if (index == 9) tile = EndingRewardPrepareJumpCellDefinitions.LowerEndpoint;
            else if (index < 18)
            {
                int cell = index - 10;
                tile = CharacterSheetGrid.Cell(24, 5) - 32 * (cell / 2) - 2 * (cell % 2); // interior rows, bottom-right first
            }
            else tile = CharacterSheetGrid.Cell(16, 7) - 2 * (index - 18); // upper interior, right to left
            int x = 8 * (tile & 15) - 38;
            int y = 8 * (tile / 16 - 16) - 35;
            bool large = index is 1 or >= 8;
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of(x, y), large,
                tile, 0, 3, 0), true);
        }
    }
    /// <summary>Yields the complete jump-preparation pose in native OAM order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same ordered pose pieces to non-generic collection consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
