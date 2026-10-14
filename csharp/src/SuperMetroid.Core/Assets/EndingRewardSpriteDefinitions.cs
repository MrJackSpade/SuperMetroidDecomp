namespace SuperMetroid.Core.Assets;

/// <summary>
/// Cartridge OAM record identities selected by the suitless and suited reward
/// lists at $8B:ED1D..EE5C. Counts describe visuals, not actor timing.
/// </summary>
public static class EndingRewardSpriteDefinitions
{
    /// <summary>$8C:99D6, LargeSamusFromEndingStanding, first of37 consecutive reward OAM records.</summary>
    private const ushort FirstRecord = 0x99d6;
    /// <summary>The native reward list publishes thirty-seven distinct OAM records.</summary>
    private const int FrameCount = 37;
    /// <summary>Gets the 37 published asset roles in installation order.</summary>
    public static IReadOnlyList<EndingRewardSpriteFrameDefinition> Frames { get; } = new FrameView();

    /// <summary>Advances through variable-length native OAM records to locate one reward pose.</summary>
    internal static ushort FramePointer(EndingRewardSpriteFrame frame)
    {
        if ((uint)frame >= FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        int address = FirstRecord;
        for (int preceding = 0; preceding < (int)frame; preceding++)
            address += sizeof(ushort) + 5 * Parts((EndingRewardSpriteFrame)preceding);
        return (ushort)address;
    }

    /// <summary>Counts the stock OAM parts that determine each subsequent record's address.</summary>
    private static int Parts(EndingRewardSpriteFrame frame) => frame switch
    {
        EndingRewardSpriteFrame.SamusHeadFromEndingFrame1 or
            EndingRewardSpriteFrame.SamusHeadFromEndingFrame2 or
            EndingRewardSpriteFrame.SamusHeadFromEndingFrame3 or
            EndingRewardSpriteFrame.SamusHeadFromEndingFrame4 => 2,
        EndingRewardSpriteFrame.JumpingSamusHeadFromEnding => 3,
        EndingRewardSpriteFrame.SamusHeadWithHelmetFromEnding => 4,
        EndingRewardSpriteFrame.LargeSamusHelmetFromEndingFrame1 or
            EndingRewardSpriteFrame.LargeSamusHelmetFromEndingFrame2 or
            EndingRewardSpriteFrame.SamusArmFromEndingFrame1 or
            EndingRewardSpriteFrame.SamusArmFromEndingFrame3 or
            EndingRewardSpriteFrame.SamusArmFromEndingFrame4 or
            EndingRewardSpriteFrame.SamusArmFromEndingFrame5 or
            EndingRewardSpriteFrame.SamusArmFromEndingFrame6 or
            EndingRewardSpriteFrame.SamusArmFromEndingFrame7 or
            EndingRewardSpriteFrame.SamusArmFromEndingFrame8 => 5,
        EndingRewardSpriteFrame.SamusArmFromEndingFrame2 => 6,
        EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame1 or
            EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame5 => 9,
        EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame2 or
            EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame3 or
            EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame4 or
            EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame6 => 10,
        EndingRewardSpriteFrame.SamusLanding or
            EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame8 => 13,
        EndingRewardSpriteFrame.SuitlessSamusLowerBody => 14,
        EndingRewardSpriteFrame.SamusFalling or
            EndingRewardSpriteFrame.SamusShooting or
            EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame7 => 15,
        EndingRewardSpriteFrame.SuitlessSamusJumping => 19,
        EndingRewardSpriteFrame.LargeSamusFromEndingJumping or
            EndingRewardSpriteFrame.SuitlessSamusPreparingToJump => 20,
        EndingRewardSpriteFrame.SamusLanded => 21,
        EndingRewardSpriteFrame.LargeSamusFromEndingPreparingToJump => 22,
        EndingRewardSpriteFrame.SuitlessSamusStanding or
            EndingRewardSpriteFrame.SuitlessSamusStandingArmsStraight => 28,
        EndingRewardSpriteFrame.HeadlessArmlessSuitedSamus => 30,
        EndingRewardSpriteFrame.LargeSamusFromEndingStanding => 34,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };

    /// <summary>Maps a published asset slot to its stable JSON name and native OAM pose.</summary>
    private static EndingRewardSpriteFrameDefinition Get(int index)
    {
        if ((uint)index >= FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index is >= 2 and <= 9)
            return Define("suitless-hair-" + (index - 2 + 1).ToString(System.Globalization.CultureInfo.InvariantCulture),
                EndingRewardSpriteFrameSeries.HairOpening[index - 2]);
        if (index is >= 21 and <= 28)
            return Define("suited-arm-" + (index - 21 + 1).ToString(System.Globalization.CultureInfo.InvariantCulture),
                EndingRewardSpriteFrameSeries.Arm[index - 21]);
        if (index is >= 29 and <= 31)
            return Define("helmetless-head-" + (index - 29 + 2).ToString(System.Globalization.CultureInfo.InvariantCulture),
                EndingRewardSpriteFrameSeries.HelmetlessHead[index - 29 + 1]);
        return (AssetRole)index switch
        {
            AssetRole.SuitlessIdleUpper => Define("suitless-idle-upper", EndingRewardSpriteFrame.SuitlessSamusStandingArmsStraight),
            AssetRole.SuitlessLower => Define("suitless-lower", EndingRewardSpriteFrame.SuitlessSamusLowerBody),
            AssetRole.SuitlessStanding => Define("suitless-standing", EndingRewardSpriteFrame.SuitlessSamusStanding),
            AssetRole.SuitlessPrepareJump => Define("suitless-prepare-jump", EndingRewardSpriteFrame.SuitlessSamusPreparingToJump),
            AssetRole.SuitlessJumping => Define("suitless-jumping", EndingRewardSpriteFrame.SuitlessSamusJumping),
            AssetRole.SamusFalling => Define("samus-falling", EndingRewardSpriteFrame.SamusFalling),
            AssetRole.SamusLanding => Define("samus-landing", EndingRewardSpriteFrame.SamusLanding),
            AssetRole.SamusLanded => Define("samus-landed", EndingRewardSpriteFrame.SamusLanded),
            AssetRole.SamusShooting => Define("samus-shooting", EndingRewardSpriteFrame.SamusShooting),
            AssetRole.SuitedIdleBody => Define("suited-idle-body", EndingRewardSpriteFrame.LargeSamusFromEndingStanding),
            AssetRole.SuitedHelmetHead => Define("suited-helmet-head", EndingRewardSpriteFrame.SamusHeadWithHelmetFromEnding),
            AssetRole.HelmetlessHead1 => Define("helmetless-head-1", EndingRewardSpriteFrame.SamusHeadFromEndingFrame1),
            AssetRole.SuitedHeadlessBody => Define("suited-headless-body", EndingRewardSpriteFrame.HeadlessArmlessSuitedSamus),
            AssetRole.SuitedPrepareJump => Define("suited-prepare-jump", EndingRewardSpriteFrame.LargeSamusFromEndingPreparingToJump),
            AssetRole.SuitedJumping => Define("suited-jumping", EndingRewardSpriteFrame.LargeSamusFromEndingJumping),
            AssetRole.SuitedHelmetJumpHead1 => Define("suited-helmet-jump-head-1", EndingRewardSpriteFrame.LargeSamusHelmetFromEndingFrame1),
            AssetRole.SuitedHelmetJumpHead2 => Define("suited-helmet-jump-head-2", EndingRewardSpriteFrame.LargeSamusHelmetFromEndingFrame2),
            AssetRole.HelmetlessJumpHead => Define("helmetless-jump-head", EndingRewardSpriteFrame.JumpingSamusHeadFromEnding),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
    }

    /// <summary>Indices outside the three contiguous hair, arm, and head frame series.</summary>
    private enum AssetRole
    {
        /// <summary>Upper half of the opening suitless idle composition.</summary>
        SuitlessIdleUpper = 0,
        /// <summary>Lower half shared by suitless reward poses.</summary>
        SuitlessLower = 1,
        /// <summary>Complete standing pose after the hair-opening series.</summary>
        SuitlessStanding = 10,
        /// <summary>Pose immediately before the suitless jump.</summary>
        SuitlessPrepareJump = 11,
        /// <summary>Airborne suitless reward pose.</summary>
        SuitlessJumping = 12,
        /// <summary>Small falling figure in the shooting sequence.</summary>
        SamusFalling = 13,
        /// <summary>Small figure approaching the landing pose.</summary>
        SamusLanding = 14,
        /// <summary>Small figure after the landing completes.</summary>
        SamusLanded = 15,
        /// <summary>Small figure firing toward the screen.</summary>
        SamusShooting = 16,
        /// <summary>Large suited body before head and arm overlays.</summary>
        SuitedIdleBody = 17,
        /// <summary>Helmeted head overlay for the suited body.</summary>
        SuitedHelmetHead = 18,
        /// <summary>First helmetless head overlay before the contiguous head series.</summary>
        HelmetlessHead1 = 19,
        /// <summary>Suited body drawn beneath separate head and arm parts.</summary>
        SuitedHeadlessBody = 20,
        /// <summary>Large suited pose anticipating the jump.</summary>
        SuitedPrepareJump = 32,
        /// <summary>Large suited airborne pose.</summary>
        SuitedJumping = 33,
        /// <summary>First helmet overlay during the suited jump.</summary>
        SuitedHelmetJumpHead1 = 34,
        /// <summary>Second helmet overlay during the suited jump.</summary>
        SuitedHelmetJumpHead2 = 35,
        /// <summary>Unhelmeted head overlay during the jump.</summary>
        HelmetlessJumpHead = 36,
    }

    /// <summary>Combines a stable asset name with its computed native pointer and stock part count.</summary>
    private static EndingRewardSpriteFrameDefinition Define(string name, EndingRewardSpriteFrame frame) =>
        new(name, FramePointer(frame), Parts(frame));

    /// <summary>Exposes the calculated published frame sequence without an eagerly stored table.</summary>
    private sealed class FrameView : IReadOnlyList<EndingRewardSpriteFrameDefinition>
    {
        /// <summary>The thirty-seven published reward asset roles.</summary>
        public int Count => FrameCount;
        /// <summary>Resolves a role index to its named native OAM identity.</summary>
        public EndingRewardSpriteFrameDefinition this[int index] => Get(index);
        /// <summary>Visits all asset roles in installation order.</summary>
        public IEnumerator<EndingRewardSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return Get(index);
        }
        /// <summary>Exposes the same installation order to nongeneric collection consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

}
