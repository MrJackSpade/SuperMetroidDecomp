using SuperMetroid.Core.Frontend.Intro;
using System.Collections;
using Pose = SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingRewardSpriteFrame;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Standing suited body and its headless/armless variant share a
/// thirty-part atlas grid. The full body prepends the four-tile arm strip.</summary>
internal sealed class EndingRewardStandingParts(int armParts) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Replaces matching stock standing poses with calculated parts while preserving edited compositions.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied)
    {
        if (shown == EndingRewardSpritePoses.FramePose(Pose.LargeSamusFromEndingStanding))
            return supplied.CalculateIfMatching(new EndingRewardStandingParts(armParts: 4));
        if (shown == EndingRewardSpritePoses.FramePose(Pose.HeadlessArmlessSuitedSamus))
            return supplied.CalculateIfMatching(new EndingRewardStandingParts(armParts: 0));
        return supplied;
    }

    /// <summary>Counts thirty shared body parts and four optional arm-strip parts.</summary>
    public int Count => armParts + 30;
    /// <summary>Calculates an indexed body or arm part from its native atlas tile and screen offset.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int x, y, tile;
            bool large;
            if (index < armParts)
            {
                tile = CharacterSheetGrid.Cell(11, 14) - 32 * index;
                x = 16; y = 8 - 16 * index; large = true;
            }
            else
            {
                int piece = index - armParts;
                if (piece < 2) tile = CharacterSheetGrid.Cell(14, 6) - 16 * piece; // outer foot edge, bottom to top
                else if (piece == 2) tile = EndingRewardStandingCellDefinitions.RightFoot;
                else if (piece < 6) tile = CharacterSheetGrid.Cell(2, 6) - 16 * (piece - 3); // outer shoulder edge
                else if (piece < 8) tile = CharacterSheetGrid.Cell(9, 1) - 16 * (piece - 6); // inner left hip edge
                else if (piece == 8) tile = EndingRewardStandingCellDefinitions.OuterLeftHip;
                else if (piece == 9) tile = EndingRewardStandingCellDefinitions.LeftLegEdge;
                else if (piece < 12) tile = CharacterSheetGrid.Cell(13, 2) - 2 * (piece - 10); // remaining feet, right to left
                else if (piece < 18)
                {
                    int cell = piece - 12;
                    tile = CharacterSheetGrid.Cell(12, 4) - 32 * (cell / 2) - 2 * (cell % 2); // two-column lower body
                }
                else
                {
                    int cell = piece - 18;
                    tile = CharacterSheetGrid.Cell(6, 4) - 32 * (cell / 3) - 2 * (cell % 3); // three-column upper body
                }
                x = 8 * (tile & 15) - 24;
                y = 8 * (tile / 16) - 56;
                large = piece is 2 or >= 10;
            }
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of(x, y), large,
                tile, 0, 3, 0), true);
        }
    }
    /// <summary>Enumerates standing parts in the OAM composition order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same standing-part order through the non-generic collection interface.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
