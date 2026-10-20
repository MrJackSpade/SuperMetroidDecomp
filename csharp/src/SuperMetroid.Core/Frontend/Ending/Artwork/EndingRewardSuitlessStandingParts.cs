using Pose = SuperMetroid.Core.Frontend.Ending.Artwork.EndingRewardSpriteFrame;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Two suitless standing compositions: shared lower-body regions,
/// vertical arm strips and independently positioned head/upper-body grids.</summary>
internal static class EndingRewardSuitlessStandingParts
{
    /// <summary>Both standing variants use twenty-eight OAM pieces despite their different arm placement.</summary>
    private const int PartCount = 28;
    /// <summary>Reusable lower-body tile grid for the straight-arm pose.</summary>
    private static readonly EndingRewardSuitlessGridParts Lower = new(Pose.SuitlessSamusLowerBody);
    /// <summary>The standing composition's parts in OAM order.</summary>
    private static readonly CompiledSpritePart[] Standing = Build(EndingSuitlessArms.Relaxed);
    /// <summary>The straight-arm standing composition's parts in OAM order.</summary>
    private static readonly CompiledSpritePart[] StandingArmsStraight = Build(EndingSuitlessArms.Straight);
    /// <summary>Applies the calculated composition only to the two supported standing frame pointers.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (pointer == EndingRewardSpriteDefinitions.FramePointer(Pose.SuitlessSamusStanding))
            return supplied.CalculateIfMatching(Standing);
        if (pointer == EndingRewardSpriteDefinitions.FramePointer(Pose.SuitlessSamusStandingArmsStraight))
            return supplied.CalculateIfMatching(StandingArmsStraight);
        return supplied;
    }
    /// <summary>Builds a pose's parts in OAM order.</summary>
    private static CompiledSpritePart[] Build(EndingSuitlessArms arms) =>
        [.. Enumerable.Range(0, PartCount).Select(index => Part(arms, index))];
    /// <summary>Calculates the selected pose's OAM tile, size, and offset, reusing lower-body pieces for straight arms.</summary>
    private static CompiledSpritePart Part(EndingSuitlessArms arms, int index)
    {
        if ((uint)index >= PartCount) throw new ArgumentOutOfRangeException(nameof(index));
        int tile, x, y;
        bool large;
        if (arms == EndingSuitlessArms.Straight)
        {
            if (index is >= 5 and < 8) return Lower[index];
            if (index is >= 8 and < 12) return Lower[index - 7];
            if (index is >= 12 and < 18) return Lower[index - 4];
            if (index is >= 1 and < 5 or >= 21 and < 25)
            {
                bool right = index < 5;
                int row = index - (right ? 1 : 21);
                tile = (right ? SnesCharacterGrid.Cell(27, 5) : SnesCharacterGrid.Cell(27, 4)) - 16 * row;
                x = right ? 8 : -13; y = 24 - 8 * row; large = false;
            }
            else
            {
                if (index == 0) tile = EndingRewardSuitlessStandingCellDefinitions.HeadRightCap;
                else if (index < 21)
                {
                    // Left edge, right edge, then center of the head's middle row.
                    int column = index == 18 ? 0 : index == 19 ? 3 : 1;
                    tile = SnesCharacterGrid.Cell(26, 0) + column;
                }
                else if (index < 27) tile = SnesCharacterGrid.Cell(27, 2) - 2 * (index - 25);
                else tile = EndingRewardSuitlessStandingCellDefinitions.HeadTop;
                x = -15 + 8 * (tile & 15); y = -40 + 8 * (tile / 16 - 24);
                large = index is 20 or >= 25;
            }
        }
        else
        {
            if (index == 5)
            {
                tile = EndingRewardSuitlessStandingCellDefinitions.RightArm; x = 8; y = 0; large = true;
            }
            else if (index is >= 6 and < 10)
            {
                tile = SnesCharacterGrid.Cell(21, 13) - 16 * (index - 6); x = -16; y = 24 - 8 * (index - 6); large = false;
            }
            else
            {
                if (index < 3) tile = index switch
                {
                    0 => EndingRewardSuitlessStandingCellDefinitions.RightKneeEdge,
                    1 => EndingRewardSuitlessStandingCellDefinitions.LeftUpperEdge,
                    _ => EndingRewardSuitlessStandingCellDefinitions.LeftFootTip,
                };
                else if (index < 5) tile = SnesCharacterGrid.Cell(0, 3) + 16 * (index - 3);
                else if (index < 14) tile = index switch
                {
                    10 => EndingRewardSuitlessStandingCellDefinitions.RightUpperBody,
                    11 => EndingRewardSuitlessStandingCellDefinitions.RightShinEdge,
                    12 => EndingRewardSuitlessStandingCellDefinitions.RightFootTip,
                    _ => EndingRewardSuitlessStandingCellDefinitions.RightFoot,
                };
                else if (index < 17) tile = SnesCharacterGrid.Cell(10, 2) - 32 * (index - 14);
                else if (index < 20) tile = SnesCharacterGrid.Cell(4, 0) - 16 * (index - 17);
                else if (index == 20) tile = EndingRewardSuitlessStandingCellDefinitions.RightShoulder;
                else tile = SnesCharacterGrid.Cell(12, 1) - 32 * (index - 21);
                x = -16 + 8 * (tile & 15); y = -40 + 8 * (tile / 16);
                large = index is 10 or >= 13 and < 17 or >= 20;
            }
        }
        return new(SnesSpritemapXWord.Create(x, large), SnesSpritemapYByte.Create(y),
            SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
    }
}
