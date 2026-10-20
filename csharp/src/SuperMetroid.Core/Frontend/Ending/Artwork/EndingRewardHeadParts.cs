using System.Collections;
using Pose = SuperMetroid.Core.Frontend.Ending.Artwork.EndingRewardSpriteFrame;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Reward head compositions: atlas rows, shared upper head and packed
/// jump/helmet caps. Native draw order is preserved independently of tile order.</summary>
internal sealed class EndingRewardHeadParts(Pose pose) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Limits calculated compositions to the large helmet, jumping head, and four face frames.</summary>
    private static bool IsHead(Pose pose) => pose is >= Pose.LargeSamusHelmetFromEndingFrame1 and <= Pose.JumpingSamusHeadFromEnding
        or >= Pose.SamusHeadFromEndingFrame1 and <= Pose.SamusHeadWithHelmetFromEnding;

    /// <summary>Recalculates a reward head only when its native frame pointer belongs to this head family.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        foreach (Pose pose in Enum.GetValues<Pose>())
            if (IsHead(pose) && EndingRewardSpriteDefinitions.FramePointer(pose) == pointer)
                return supplied.CalculateIfMatching(new EndingRewardHeadParts(pose));
        return supplied;
    }

    /// <summary>Face frames use two pieces; helmet and jump poses need their own cap and head part counts.</summary>
    public int Count => pose switch
    {
        Pose.LargeSamusHelmetFromEndingFrame1 or Pose.LargeSamusHelmetFromEndingFrame2 => 5,
        Pose.JumpingSamusHeadFromEnding => 3,
        Pose.SamusHeadWithHelmetFromEnding => 4,
        Pose.SamusHeadFromEndingFrame1 or
            Pose.SamusHeadFromEndingFrame2 or
            Pose.SamusHeadFromEndingFrame3 or
            Pose.SamusHeadFromEndingFrame4 => 2,
        Pose.LargeSamusFromEndingStanding or
            Pose.LargeSamusFromEndingPreparingToJump or
            Pose.LargeSamusFromEndingJumping or
            Pose.SamusArmFromEndingFrame1 or
            Pose.SamusArmFromEndingFrame2 or
            Pose.SamusArmFromEndingFrame3 or
            Pose.SamusArmFromEndingFrame4 or
            Pose.SamusArmFromEndingFrame5 or
            Pose.SamusArmFromEndingFrame6 or
            Pose.SamusArmFromEndingFrame7 or
            Pose.SamusArmFromEndingFrame8 or
            Pose.HeadlessArmlessSuitedSamus or
            Pose.SamusFalling or
            Pose.SamusLanding or
            Pose.SamusLanded or
            Pose.SamusShooting or
            Pose.SuitlessSamusStanding or
            Pose.SuitlessSamusPreparingToJump or
            Pose.SuitlessSamusJumping or
            Pose.SuitlessSamusStandingArmsStraight or
            Pose.SuitlessSamusOpeningHairFrame1 or
            Pose.SuitlessSamusOpeningHairFrame2 or
            Pose.SuitlessSamusOpeningHairFrame3 or
            Pose.SuitlessSamusOpeningHairFrame4 or
            Pose.SuitlessSamusOpeningHairFrame5 or
            Pose.SuitlessSamusOpeningHairFrame6 or
            Pose.SuitlessSamusOpeningHairFrame7 or
            Pose.SuitlessSamusOpeningHairFrame8 or
            Pose.SuitlessSamusLowerBody => throw new ArgumentOutOfRangeException(nameof(pose)),
        _ => throw new ArgumentOutOfRangeException(nameof(pose)),
    };

    /// <summary>Builds the authored head or helmet piece at its draw-order index using the shared atlas rows.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int x, y, tile;
            bool large;
            if (pose is >= Pose.SamusHeadFromEndingFrame1 and <= Pose.SamusHeadFromEndingFrame4)
            {
                x = -8; y = index == 0 ? -8 : -16; large = true;
                tile = index == 0 ? SnesCharacterGrid.Cell(24, 8) + 2 * (pose - Pose.SamusHeadFromEndingFrame1) : SnesCharacterGrid.Cell(23, 8); // magic-number-audit: allow(PoseOrMovement) - tile atlas indices and strides for cinematic sprite composition.
            }
            else if (pose == Pose.SamusHeadWithHelmetFromEnding)
            {
                int column = 1 - index % 2, row = index / 2;
                x = -12 + 8 * column; y = 4 - 16 * row; large = row == 1;
                tile = SnesCharacterGrid.Cell(28, 8) + column - 32 * row;
            }
            else if (pose == Pose.JumpingSamusHeadFromEnding)
            {
                large = index == 2;
                int column = large ? 0 : 1 - index;
                x = -8 + 8 * column; y = large ? -6 : -14;
                tile = (large ? SnesCharacterGrid.Cell(21, 7) : SnesCharacterGrid.Cell(20, 7)) + column;
            }
            else if (pose == Pose.LargeSamusHelmetFromEndingFrame1)
            {
                large = index >= 3;
                int column = large ? index - 3 : 2 - index;
                x = -12 + 8 * column; y = large ? -13 : 3;
                tile = (large ? SnesCharacterGrid.Cell(7, 6) : SnesCharacterGrid.Cell(9, 6)) + column;
            }
            else
            {
                // Right large piece precedes the three small cap tiles, then left large piece.
                large = index is 0 or 4;
                int column = large ? (index == 0 ? 1 : 0) : 3 - index;
                x = -12 + 8 * column; y = large ? -6 : -14;
                tile = (large ? SnesCharacterGrid.Cell(5, 6) : SnesCharacterGrid.Cell(4, 6)) + column;
            }
            return new(SnesSpritemapXWord.Create(x, large), SnesSpritemapYByte.Create(y),
                SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
        }
    }
    /// <summary>Enumerates reward head pieces in native sprite draw order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Uses the same draw order for non-generic collection consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
