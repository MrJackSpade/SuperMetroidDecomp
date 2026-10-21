using SuperMetroid.Core.Frontend.Intro;
using System.Collections;
using Pose = SuperMetroid.Core.Frontend.Ending.Artwork.EndingRewardSpriteFrame;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Suitless lower-body and jump compositions use atlas grids, with
/// separately positioned arm strips in the jumping pose.</summary>
internal sealed class EndingRewardSuitlessGridParts(Pose pose) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Recalculates only the three suitless frame compositions represented by atlas grids.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied)
    {
        if (shown == EndingRewardSpriteDefinitions.FramePose(Pose.SuitlessSamusLowerBody))
            return supplied.CalculateIfMatching(new EndingRewardSuitlessGridParts(Pose.SuitlessSamusLowerBody));
        if (shown == EndingRewardSpriteDefinitions.FramePose(Pose.SuitlessSamusPreparingToJump))
            return supplied.CalculateIfMatching(new EndingRewardSuitlessGridParts(Pose.SuitlessSamusPreparingToJump));
        if (shown == EndingRewardSpriteDefinitions.FramePose(Pose.SuitlessSamusJumping))
            return supplied.CalculateIfMatching(new EndingRewardSuitlessGridParts(Pose.SuitlessSamusJumping));
        return supplied;
    }
    /// <summary>Counts the grid and arm-strip parts in the selected suitless pose.</summary>
    public int Count => pose switch
    {
        Pose.SuitlessSamusLowerBody => 14,
        Pose.SuitlessSamusPreparingToJump => 20,
        Pose.SuitlessSamusJumping => 19,
        Pose.LargeSamusFromEndingStanding or
            Pose.LargeSamusFromEndingPreparingToJump or
            Pose.LargeSamusFromEndingJumping or
            Pose.LargeSamusHelmetFromEndingFrame1 or
            Pose.LargeSamusHelmetFromEndingFrame2 or
            Pose.JumpingSamusHeadFromEnding or
            Pose.SamusArmFromEndingFrame1 or
            Pose.SamusArmFromEndingFrame2 or
            Pose.SamusArmFromEndingFrame3 or
            Pose.SamusArmFromEndingFrame4 or
            Pose.SamusArmFromEndingFrame5 or
            Pose.SamusArmFromEndingFrame6 or
            Pose.SamusArmFromEndingFrame7 or
            Pose.SamusArmFromEndingFrame8 or
            Pose.SamusHeadFromEndingFrame1 or
            Pose.SamusHeadFromEndingFrame2 or
            Pose.SamusHeadFromEndingFrame3 or
            Pose.SamusHeadFromEndingFrame4 or
            Pose.SamusHeadWithHelmetFromEnding or
            Pose.HeadlessArmlessSuitedSamus or
            Pose.SamusFalling or
            Pose.SamusLanding or
            Pose.SamusLanded or
            Pose.SamusShooting or
            Pose.SuitlessSamusStanding or
            Pose.SuitlessSamusStandingArmsStraight or
            Pose.SuitlessSamusOpeningHairFrame1 or
            Pose.SuitlessSamusOpeningHairFrame2 or
            Pose.SuitlessSamusOpeningHairFrame3 or
            Pose.SuitlessSamusOpeningHairFrame4 or
            Pose.SuitlessSamusOpeningHairFrame5 or
            Pose.SuitlessSamusOpeningHairFrame6 or
            Pose.SuitlessSamusOpeningHairFrame7 or
            Pose.SuitlessSamusOpeningHairFrame8 => throw new ArgumentOutOfRangeException(nameof(pose)),
        _ => throw new ArgumentOutOfRangeException(nameof(pose)),
    };

    /// <summary>Selects an atlas tile and its native size and offset; jumping arm strips precede the body grid.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int tile, x, y;
            bool large;
            if (pose == Pose.SuitlessSamusLowerBody)
            {
                if (index == 0) tile = EndingRewardSuitlessCellDefinitions.InnerHipEdge;
                else if (index < 3) tile = SnesCharacterGrid.Cell(13, 0) + 5 * (index - 1); // outer foot tips, left then right
                else if (index < 5) tile = SnesCharacterGrid.Cell(12, 1) + 2 * (index - 3); // foot pair, left then right
                else if (index < 8) tile = SnesCharacterGrid.Cell(8, 3) - 16 * (index - 5); // inner edge, bottom to top
                else if (index < 10) tile = SnesCharacterGrid.Cell(11, 3) - 32 * (index - 8); // right leg
                else tile = SnesCharacterGrid.Cell(11, 1) - 32 * (index - 10); // left leg and hip
                x = 8 * (tile & 15) - 16; y = 8 * (tile / 16) - 40;
                large = index is 3 or 4 or >= 8;
            }
            else if (pose == Pose.SuitlessSamusPreparingToJump)
            {
                if (index < 5) tile = SnesCharacterGrid.Cell(5, 6) - 16 * index; // left edge, bottom to top
                else if (index == 5) tile = EndingRewardSuitlessCellDefinitions.CrouchRightCap;
                else if (index == 6) tile = EndingRewardSuitlessCellDefinitions.CrouchLeftLegEdge;
                else if (index < 10) tile = SnesCharacterGrid.Cell(12, 8) - 32 * (index - 7); // right leg
                else if (index < 12) tile = SnesCharacterGrid.Cell(6, 9) - 4 * (index - 10); // outer hips, right then left
                else if (index < 14) tile = SnesCharacterGrid.Cell(4, 9) - 32 * (index - 12); // right upper edge
                else tile = SnesCharacterGrid.Cell(10, 7) - 32 * (index - 14); // central column, bottom to top
                x = 8 * ((tile & 15) - 6) - 16; y = 8 * (tile / 16) - 32;
                large = index >= 7;
            }
            else if (index < 7)
            {
                bool right = index < 4;
                int row = right ? index : index - 4;
                tile = (right ? SnesCharacterGrid.Cell(11, 11) : SnesCharacterGrid.Cell(2, 11)) - 16 * row;
                x = right ? 16 : -24; y = (right ? -16 : -24) - 8 * row;
                large = false;
            }
            else
            {
                if (index < 9) tile = SnesCharacterGrid.Cell(12, 14) - 32 * (index - 7); // lower right leg
                else
                {
                    int cell = index - 9;
                    tile = SnesCharacterGrid.Cell(8, 14) - 32 * (cell / 2) - 2 * (cell % 2); // paired body rows
                }
                x = 8 * ((tile & 15) - 12) - 16; y = 8 * (tile / 16) - 40;
                large = true;
            }
            return new(SnesSpritemapXWord.Create(x, large), SnesSpritemapYByte.Create(y),
                SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
        }
    }
    /// <summary>Visits the pose's parts in the order used by its compiled spritemap.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same native part ordering to non-generic callers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
