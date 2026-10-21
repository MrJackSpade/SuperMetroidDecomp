using SuperMetroid.Core.Frontend.Intro;
using System.Collections;
using Pose = SuperMetroid.Core.Frontend.Ending.Artwork.EndingRewardSpriteFrame;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Small falling/landing/shooting figures use pose-specific origins
/// on a common atlas grid. Native priority is zero for these four compositions.</summary>
internal sealed class EndingRewardShootingSceneParts(Pose pose) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Uses calculated OAM parts only when a supplied falling, landing, or shooting frame matches its native composition.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied)
    {
        foreach (Pose pose in EndingRewardSpriteFrameSeries.ShootingScene)
            if (shown == EndingRewardSpriteDefinitions.FramePose(pose))
                return supplied.CalculateIfMatching(new EndingRewardShootingSceneParts(pose));
        return supplied;
    }
    /// <summary>Native part count depends on the figure's falling, landing, landed, or shooting pose.</summary>
    public int Count => pose switch
    {
        Pose.SamusFalling or Pose.SamusShooting => 15,
        Pose.SamusLanding => 13,
        Pose.SamusLanded => 21,
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
    /// <summary>Calculates a pose's OAM tile, size, and origin while retaining its authored part order.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int tile, originX, originY;
            bool large;
            switch (pose)
            {
                case Pose.SamusFalling:
                    if (index < 3) tile = SnesCharacterGrid.Cell(5, 4) - 16 * index; // right edge
                    else if (index < 6) tile = SnesCharacterGrid.Cell(8, 3) - 16 * (index - 3); // leg edge
                    else if (index < 10)
                    {
                        int cell = index - 6;
                        tile = SnesCharacterGrid.Cell(2, 1) + 4 * (cell / 2) - cell % 2; // left/right cap pairs
                    }
                    else if (index < 12) tile = 4 * (index - 10); // outer upper pieces
                    else tile = SnesCharacterGrid.Cell(4, 2) - 32 * (index - 12); // central column
                    large = index >= 10; originX = -24; originY = -32;
                    break;
                case Pose.SamusLanding:
                    if (index == 0) tile = EndingRewardShootingCellDefinitions.LandingLeftLegEdge;
                    else if (index < 3) tile = SnesCharacterGrid.Cell(8, 10) - (index - 1); // lower edge
                    else if (index < 5) tile = SnesCharacterGrid.Cell(6, 9) - 32 * (index - 3); // right body
                    else if (index < 7) tile = SnesCharacterGrid.Cell(3, 11) - (index - 5); // right cap
                    else if (index == 7) tile = EndingRewardShootingCellDefinitions.LandingLeftOuter;
                    else if (index == 8) tile = EndingRewardShootingCellDefinitions.LandingLeftCap;
                    else if (index == 9) tile = EndingRewardShootingCellDefinitions.LandingRightUpper;
                    else tile = SnesCharacterGrid.Cell(5, 8) - 32 * (index - 10); // central column
                    large = index is 3 or 4 or 7 or >= 9; originX = -72; originY = -33;
                    break;
                case Pose.SamusLanded:
                    if (index < 2) tile = SnesCharacterGrid.Cell(11, 0) - 16 * index; // left cap
                    else if (index == 2) tile = EndingRewardShootingCellDefinitions.LandedRightCap;
                    else if (index == 3) tile = EndingRewardShootingCellDefinitions.LandedRightUpper;
                    else if (index < 6) tile = SnesCharacterGrid.Cell(18, 2) - (index - 4); // left foot edge
                    else if (index < 9) tile = SnesCharacterGrid.Cell(18, 5) - (index - 6); // right foot edge
                    else if (index < 11) tile = SnesCharacterGrid.Cell(16, 3) - 2 * (index - 9); // feet
                    else if (index < 15) tile = SnesCharacterGrid.Cell(15, 4) - (index - 11); // waist edge
                    else
                    {
                        int cell = index - 15;
                        tile = SnesCharacterGrid.Cell(13, 3) - 32 * (cell / 2) - 2 * (cell % 2); // paired body rows
                    }
                    large = index is 3 or 9 or 10 or >= 15; originX = -24; originY = -112;
                    break;
                case Pose.SamusShooting:
                    if (index == 0) tile = EndingRewardShootingCellDefinitions.ShootingRightFootTip;
                    else if (index < 4) tile = SnesCharacterGrid.Cell(17, 10) - 2 * (index - 1); // lower row
                    else if (index == 4) tile = EndingRewardShootingCellDefinitions.ShootingLeftHipEdge;
                    else if (index < 7) tile = SnesCharacterGrid.Cell(16, 10) - 3 * (index - 5); // outer leg pair
                    else if (index < 9) tile = SnesCharacterGrid.Cell(14, 10) - 2 * (index - 7); // waist pair
                    else if (index == 9) tile = EndingRewardShootingCellDefinitions.ShootingRightUpper;
                    else if (index == 10) tile = EndingRewardShootingCellDefinitions.ShootingRightEdge;
                    else
                    {
                        int cell = index - 11;
                        tile = SnesCharacterGrid.Cell(12, 9) - 32 * (cell / 2) - 2 * (cell % 2); // upper-body pairs
                    }
                    large = index is not (0 or 2 or 4 or 10); originX = -76; originY = -112;
                    break;
                case Pose.LargeSamusFromEndingStanding:
                case Pose.LargeSamusFromEndingPreparingToJump:
                case Pose.LargeSamusFromEndingJumping:
                case Pose.LargeSamusHelmetFromEndingFrame1:
                case Pose.LargeSamusHelmetFromEndingFrame2:
                case Pose.JumpingSamusHeadFromEnding:
                case Pose.SamusArmFromEndingFrame1:
                case Pose.SamusArmFromEndingFrame2:
                case Pose.SamusArmFromEndingFrame3:
                case Pose.SamusArmFromEndingFrame4:
                case Pose.SamusArmFromEndingFrame5:
                case Pose.SamusArmFromEndingFrame6:
                case Pose.SamusArmFromEndingFrame7:
                case Pose.SamusArmFromEndingFrame8:
                case Pose.SamusHeadFromEndingFrame1:
                case Pose.SamusHeadFromEndingFrame2:
                case Pose.SamusHeadFromEndingFrame3:
                case Pose.SamusHeadFromEndingFrame4:
                case Pose.SamusHeadWithHelmetFromEnding:
                case Pose.HeadlessArmlessSuitedSamus:
                case Pose.SuitlessSamusStanding:
                case Pose.SuitlessSamusPreparingToJump:
                case Pose.SuitlessSamusJumping:
                case Pose.SuitlessSamusStandingArmsStraight:
                case Pose.SuitlessSamusOpeningHairFrame1:
                case Pose.SuitlessSamusOpeningHairFrame2:
                case Pose.SuitlessSamusOpeningHairFrame3:
                case Pose.SuitlessSamusOpeningHairFrame4:
                case Pose.SuitlessSamusOpeningHairFrame5:
                case Pose.SuitlessSamusOpeningHairFrame6:
                case Pose.SuitlessSamusOpeningHairFrame7:
                case Pose.SuitlessSamusOpeningHairFrame8:
                case Pose.SuitlessSamusLowerBody:
                default:
                    throw new InvalidOperationException($"{pose} is not a falling, landing or shooting figure.");
            }
            return new(SnesSpritemapXWord.Create(originX + 8 * (tile & 15), large),
                SnesSpritemapYByte.Create(originY + 8 * (tile / 16)), SnesObjAttributeWord.Create(tile, 0, 0, 0), true);
        }
    }
    /// <summary>Traverses calculated OAM pieces in the source composition's drawing order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same source drawing order to non-generic collection consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
