using SuperMetroid.Core.Frontend.Intro;
using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>$8C:9AF2: suited jumping pose, with independently positioned limb
/// strips and upper/lower body atlas regions.</summary>
internal sealed class EndingRewardJumpParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Uses calculated parts only for the suited ending jump frame.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied) =>
        shown == EndingRewardSpriteDefinitions.FramePose(EndingRewardSpriteFrame.LargeSamusFromEndingJumping)
            ? supplied.CalculateIfMatching(new EndingRewardJumpParts()) : supplied;

    /// <summary>Twenty native limb and body sprite strips form the jumping pose.</summary>
    public int Count => 20;
    /// <summary>Places one strip from the suited jumping atlas in native draw order.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int tile, x, y;
            if (index == 0)
            {
                tile = EndingRewardJumpCellDefinitions.UpperBodyCenter; x = -4; y = -53;
            }
            else if (index < 3)
            {
                int row = 2 - index;
                tile = CharacterSheetGrid.Cell(0, 14) + row * 32; x = 19; y = -61 + row * 16;
            }
            else if (index == 3)
            {
                tile = EndingRewardJumpCellDefinitions.LeftLimbCap; x = -27; y = -62;
            }
            else if (index < 6)
            {
                int row = 5 - index;
                tile = CharacterSheetGrid.Cell(1, 7) + row * 16; x = -36; y = -52 + row * 8;
            }
            else if (index < 11)
            {
                // Lower upper-body row right-to-left, then the two upper corners.
                tile = index < 9 ? CharacterSheetGrid.Cell(3, 13) - 2 * (index - 6) : CharacterSheetGrid.Cell(1, 13) - 4 * (index - 9);
                x = -20 + 8 * ((tile & 15) - 9);
                y = -53 + 8 * (tile / 16 - 1);
            }
            else
            {
                if (index < 13) tile = CharacterSheetGrid.Cell(5, 12) - 2 * (index - 11); // waist pair
                else if (index < 16) tile = CharacterSheetGrid.Cell(14, 12) - 32 * (index - 13); // lower leg, bottom to top
                else
                {
                    int cell = index - 16;
                    tile = CharacterSheetGrid.Cell(8, 12) - 32 * (cell / 2) - 2 * (cell % 2); // paired upper-leg rows
                }
                x = -16 + 8 * ((tile & 15) - 10);
                y = -13 + 8 * (tile / 16 - 6);
            }
            return new(SpritePart.Create(x, y, index != 13,
                tile, 0, 3, 0), true);
        }
    }
    /// <summary>Yields all twenty jumping-pose strips in native draw order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same ordered sprite strips to non-generic enumeration.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
