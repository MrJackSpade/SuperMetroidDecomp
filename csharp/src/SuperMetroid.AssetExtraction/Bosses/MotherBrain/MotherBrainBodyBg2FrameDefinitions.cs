using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>
/// The installed document of Mother Brain's body-layer poses (#1466): its schema, file name and the name each pose is
/// stored under. Native roots interleave sprite limbs and BG2 body streams; their hitboxes never enter the document.
/// </summary>
internal static class MotherBrainBodyBg2FrameDefinitions
{
    /// <summary>Largest native root: ten components, including two BG2 streams.</summary>
    internal const int MaximumComponents = EnemyBg2FrameLayout.MaximumComponents;
    /// <summary>Schema revision of the selected BG2 body-frame document; 3 names each frame for the animation that first shows it (#1623).</summary>
    internal const int Version = 3;
    /// <summary>Installed JSON filename for Mother Brain's selected BG2 body frames.</summary>
    internal const string FileName = "mother-brain-body-bg2-frames.json";
    /// <summary>
    /// The frames whose pictures lie further right on her body's area than the cells the cartridge writes them to, by the
    /// pixels they lie further right (#1949). Her body's area lies 38 pixels left of her, where her leaning and uncrouching
    /// poses lie on their cells; her walking, standing and hand-beam poses lie 4 pixels further right, 34 left of her, and
    /// her crouch 6 further right, 32 left of her. Evidence: the cartridge placed her body's area against her at each step:
    /// 34 pixels left of her after every walking step, and on each posture change 34 left shifted by that change's own
    /// amount, which leaves each of these poses where it lies here as standing up from the crouch shows them, 63 pixels above
    /// her. Three transitional placements are not kept, so each pose lies in one place against her: her uncrouching pose as
    /// she crouches lay 6 pixels further right, her standing pose right after standing up 2 pixels further right until her
    /// next step, and her crouch while she rises from the floor 1 pixel lower.
    /// </summary>
    internal static IReadOnlyDictionary<EnemyFrameId, int> FramesShiftedRight { get; } = new Dictionary<EnemyFrameId, int>
    {
        [EnemyFrameId.MotherBrainBodyStanding] = 4,
        [EnemyFrameId.MotherBrainBodyWalkingNearLegLifting] = 4,
        [EnemyFrameId.MotherBrainBodyWalkingNearLegReaching] = 4,
        [EnemyFrameId.MotherBrainBodyWalkingNearLegPlanting] = 4,
        [EnemyFrameId.MotherBrainBodyWalkingLegsApart] = 4,
        [EnemyFrameId.MotherBrainBodyWalkingFarLegLifting] = 4,
        [EnemyFrameId.MotherBrainBodyWalkingFarLegPassing] = 4,
        [EnemyFrameId.MotherBrainBodyWalkingFarLegPlanting] = 4,
        [EnemyFrameId.MotherBrainBodyWalkingLegsTogether] = 4,
        [EnemyFrameId.MotherBrainBodyHandCurling] = 4,
        [EnemyFrameId.MotherBrainBodyArmDrawnBack] = 4,
        [EnemyFrameId.MotherBrainBodyArmFlungOut] = 4,
        [EnemyFrameId.MotherBrainBodyArmReturning] = 4,
        [EnemyFrameId.MotherBrainBodyCrouched] = 6,
    };

    /// <summary>Every body frame with a BG2 stream.</summary>
    internal static ImmutableArray<EnemyFrameName> Frames { get; } =
    [
        new(EnemyFrameId.MotherBrainBodyStanding, "mother_brain_body_bg2_standing"),
        new(EnemyFrameId.MotherBrainBodyWalkingNearLegLifting, "mother_brain_body_bg2_walking_near_leg_lifting"),
        new(EnemyFrameId.MotherBrainBodyWalkingNearLegReaching, "mother_brain_body_bg2_walking_near_leg_reaching"),
        new(EnemyFrameId.MotherBrainBodyWalkingNearLegPlanting, "mother_brain_body_bg2_walking_near_leg_planting"),
        new(EnemyFrameId.MotherBrainBodyWalkingLegsApart, "mother_brain_body_bg2_walking_legs_apart"),
        new(EnemyFrameId.MotherBrainBodyWalkingFarLegLifting, "mother_brain_body_bg2_walking_far_leg_lifting"),
        new(EnemyFrameId.MotherBrainBodyWalkingFarLegPassing, "mother_brain_body_bg2_walking_far_leg_passing"),
        new(EnemyFrameId.MotherBrainBodyWalkingFarLegPlanting, "mother_brain_body_bg2_walking_far_leg_planting"),
        new(EnemyFrameId.MotherBrainBodyWalkingLegsTogether, "mother_brain_body_bg2_walking_legs_together"),
        new(EnemyFrameId.MotherBrainBodyCrouched, "mother_brain_body_bg2_crouched"),
        new(EnemyFrameId.MotherBrainBodyUncrouching, "mother_brain_body_bg2_uncrouching"),
        new(EnemyFrameId.MotherBrainBodyLeaningDown, "mother_brain_body_bg2_leaning_down"),
        new(EnemyFrameId.MotherBrainBodyHandCurling, "mother_brain_body_bg2_hand_curling"),
        new(EnemyFrameId.MotherBrainBodyArmDrawnBack, "mother_brain_body_bg2_arm_drawn_back"),
        new(EnemyFrameId.MotherBrainBodyArmFlungOut, "mother_brain_body_bg2_arm_flung_out"),
        new(EnemyFrameId.MotherBrainBodyArmReturning, "mother_brain_body_bg2_arm_returning"),
    ];
}
