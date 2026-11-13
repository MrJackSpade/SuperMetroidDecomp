namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The frames an enemy shows: every installed enemy frame, and the empty frame every enemy family shares. Each is named for
/// its owner and its place among that owner's frames (the pose, index or part its frame table selects it by); each cites
/// the name the installed artwork gives it (<see cref="EnemyFrameNameDefinitions"/>), which also gives its installed sheet
/// key (<see cref="EnemyFrameSheetKeys"/>), so a frame's identity never reaches a file.
/// </summary>
public enum EnemyFrameId
{
    /// <summary>The frame the installed artwork names <c>boyon_idle_0</c>.</summary>
    BoyonIdleLoopFirst = 1,
    /// <summary>The frame the installed artwork names <c>boyon_idle_1</c>.</summary>
    BoyonIdleLoopSecond,
    /// <summary>The frame the installed artwork names <c>boyon_idle_2</c>.</summary>
    BoyonIdleLoopThird,
    /// <summary>The frame the installed artwork names <c>boyon_bounce_0</c>.</summary>
    BoyonBouncingLoopFirst,
    /// <summary>The frame the installed artwork names <c>boyon_bounce_1</c>.</summary>
    BoyonBouncingLoopSecond,
    /// <summary>The frame the installed artwork names <c>boyon_bounce_2</c>.</summary>
    BoyonBouncingLoopThird,
    /// <summary>The frame the installed artwork names <c>boyon_bounce_3</c>.</summary>
    BoyonBouncingLoopFourth,
    /// <summary>The frame the installed artwork names <c>cacatac_upright_idle_0</c>.</summary>
    CacatacUpsideUpIdleFirst,
    /// <summary>The frame the installed artwork names <c>cacatac_upright_idle_1</c>.</summary>
    CacatacUpsideUpIdleSecond,
    /// <summary>The frame the installed artwork names <c>cacatac_upright_idle_2</c>.</summary>
    CacatacUpsideUpIdleThird,
    /// <summary>The frame the installed artwork names <c>cacatac_upright_idle_3</c>.</summary>
    CacatacUpsideUpIdleFourth,
    /// <summary>The frame the installed artwork names <c>cacatac_upright_idle_4</c>.</summary>
    CacatacUpsideUpIdleFifth,
    /// <summary>The frame the installed artwork names <c>cacatac_upright_idle_5</c>.</summary>
    CacatacUpsideUpIdleSixth,
    /// <summary>The frame the installed artwork names <c>cacatac_upright_idle_6</c>.</summary>
    CacatacUpsideUpIdleSeventh,
    /// <summary>The frame the installed artwork names <c>cacatac_upright_idle_7</c>.</summary>
    CacatacUpsideUpIdleEighth,
    /// <summary>The frame the installed artwork names <c>cacatac_upright_attack_1</c>.</summary>
    CacatacUpsideUpAttackSecond,
    /// <summary>The frame the installed artwork names <c>cacatac_upright_attack_2</c>.</summary>
    CacatacUpsideUpAttackThird,
    /// <summary>The frame the installed artwork names <c>cacatac_inverted_idle_0</c>.</summary>
    CacatacUpsideDownIdleLoopFirst,
    /// <summary>The frame the installed artwork names <c>cacatac_inverted_idle_1</c>.</summary>
    CacatacUpsideDownIdleLoopSecond,
    /// <summary>The frame the installed artwork names <c>cacatac_inverted_idle_2</c>.</summary>
    CacatacUpsideDownIdleLoopThird,
    /// <summary>The frame the installed artwork names <c>cacatac_inverted_idle_3</c>.</summary>
    CacatacUpsideDownIdleLoopFourth,
    /// <summary>The frame the installed artwork names <c>cacatac_inverted_idle_4</c>.</summary>
    CacatacUpsideDownIdleLoopFifth,
    /// <summary>The frame the installed artwork names <c>cacatac_inverted_idle_5</c>.</summary>
    CacatacUpsideDownIdleLoopSixth,
    /// <summary>The frame the installed artwork names <c>cacatac_inverted_idle_6</c>.</summary>
    CacatacUpsideDownIdleLoopSeventh,
    /// <summary>The frame the installed artwork names <c>cacatac_inverted_idle_7</c>.</summary>
    CacatacUpsideDownIdleLoopEighth,
    /// <summary>The frame the installed artwork names <c>cacatac_inverted_attack_1</c>.</summary>
    CacatacUpsideDownAttackSecond,
    /// <summary>The frame the installed artwork names <c>cacatac_inverted_attack_2</c>.</summary>
    CacatacUpsideDownAttackThird,
    /// <summary>The frame the installed artwork names <c>boulder_roll_0</c>.</summary>
    BoulderLeftFirst,
    /// <summary>The frame the installed artwork names <c>boulder_roll_1</c>.</summary>
    BoulderLeftSecond,
    /// <summary>The frame the installed artwork names <c>boulder_roll_2</c>.</summary>
    BoulderLeftThird,
    /// <summary>The frame the installed artwork names <c>boulder_roll_3</c>.</summary>
    BoulderLeftFourth,
    /// <summary>The frame the installed artwork names <c>boulder_roll_4</c>.</summary>
    BoulderLeftFifth,
    /// <summary>The frame the installed artwork names <c>boulder_roll_5</c>.</summary>
    BoulderLeftSixth,
    /// <summary>The frame the installed artwork names <c>boulder_roll_6</c>.</summary>
    BoulderLeftSeventh,
    /// <summary>The frame the installed artwork names <c>boulder_roll_7</c>.</summary>
    BoulderLeftEighth,
    /// <summary>The frame the installed artwork names <c>atomic_up_right_0</c>.</summary>
    AtomicUpRightFirst,
    /// <summary>The frame the installed artwork names <c>atomic_up_right_1</c>.</summary>
    AtomicUpRightSecond,
    /// <summary>The frame the installed artwork names <c>atomic_up_right_2</c>.</summary>
    AtomicUpRightThird,
    /// <summary>The frame the installed artwork names <c>atomic_up_right_3</c>.</summary>
    AtomicUpRightFourth,
    /// <summary>The frame the installed artwork names <c>atomic_up_right_4</c>.</summary>
    AtomicUpRightFifth,
    /// <summary>The frame the installed artwork names <c>atomic_up_right_5</c>.</summary>
    AtomicUpRightSixth,
    /// <summary>The frame the installed artwork names <c>atomic_up_left_0</c>.</summary>
    AtomicUpLeftFirst,
    /// <summary>The frame the installed artwork names <c>atomic_up_left_1</c>.</summary>
    AtomicUpLeftSecond,
    /// <summary>The frame the installed artwork names <c>atomic_up_left_2</c>.</summary>
    AtomicUpLeftThird,
    /// <summary>The frame the installed artwork names <c>atomic_up_left_3</c>.</summary>
    AtomicUpLeftFourth,
    /// <summary>The frame the installed artwork names <c>atomic_up_left_4</c>.</summary>
    AtomicUpLeftFifth,
    /// <summary>The frame the installed artwork names <c>atomic_up_left_5</c>.</summary>
    AtomicUpLeftSixth,
    /// <summary>The frame the installed artwork names <c>skultera_swim_left_0</c>.</summary>
    SkulteraSwimmingLeftLoopFirst,
    /// <summary>The frame the installed artwork names <c>skultera_swim_left_1</c>.</summary>
    SkulteraSwimmingLeftLoopSecond,
    /// <summary>The frame the installed artwork names <c>skultera_swim_left_2</c>.</summary>
    SkulteraSwimmingLeftLoopThird,
    /// <summary>The frame the installed artwork names <c>skultera_turn_right_0</c>.</summary>
    SkulteraTurningRightFirst,
    /// <summary>The frame the installed artwork names <c>skultera_turn_right_1</c>.</summary>
    SkulteraTurningRightSecond,
    /// <summary>The frame the installed artwork names <c>skultera_turn_right_2</c>.</summary>
    SkulteraTurningRightThird,
    /// <summary>The frame the installed artwork names <c>skultera_turn_right_3</c>.</summary>
    SkulteraTurningRightFourth,
    /// <summary>The frame the installed artwork names <c>skultera_turn_right_4</c>.</summary>
    SkulteraTurningRightFifth,
    /// <summary>The frame the installed artwork names <c>skultera_turn_right_5</c>.</summary>
    SkulteraTurningRightSixth,
    /// <summary>The frame the installed artwork names <c>skultera_turn_right_6</c>.</summary>
    SkulteraTurningRightSeventh,
    /// <summary>The frame the installed artwork names <c>skultera_turn_right_7</c>.</summary>
    SkulteraTurningRightEighth,
    /// <summary>The frame the installed artwork names <c>skultera_swim_right_0</c>.</summary>
    SkulteraSwimmingRightLoopFirst,
    /// <summary>The frame the installed artwork names <c>skultera_swim_right_1</c>.</summary>
    SkulteraSwimmingRightLoopSecond,
    /// <summary>The frame the installed artwork names <c>skultera_swim_right_2</c>.</summary>
    SkulteraSwimmingRightLoopThird,
    /// <summary>The frame the installed artwork names <c>skultera_turn_left_0</c>.</summary>
    SkulteraTurningLeftFirst,
    /// <summary>The frame the installed artwork names <c>skultera_turn_left_1</c>.</summary>
    SkulteraTurningLeftSecond,
    /// <summary>The frame the installed artwork names <c>skultera_turn_left_2</c>.</summary>
    SkulteraTurningLeftThird,
    /// <summary>The frame the installed artwork names <c>skultera_turn_left_3</c>.</summary>
    SkulteraTurningLeftFourth,
    /// <summary>The frame the installed artwork names <c>skultera_turn_left_4</c>.</summary>
    SkulteraTurningLeftFifth,
    /// <summary>The frame the installed artwork names <c>skultera_turn_left_5</c>.</summary>
    SkulteraTurningLeftSixth,
    /// <summary>The frame the installed artwork names <c>skultera_turn_left_6</c>.</summary>
    SkulteraTurningLeftSeventh,
    /// <summary>The frame the installed artwork names <c>skultera_turn_left_7</c>.</summary>
    SkulteraTurningLeftEighth,
    /// <summary>The frame the installed artwork names <c>waver_steady_left</c>.</summary>
    WaverSteadyLeft,
    /// <summary>The frame the installed artwork names <c>waver_steady_right</c>.</summary>
    WaverSteadyRight,
    /// <summary>The frame the installed artwork names <c>waver_spin_left_0</c>.</summary>
    WaverSpinningFacingLeftFirst,
    /// <summary>The frame the installed artwork names <c>waver_spin_left_1</c>.</summary>
    WaverSpinningFacingLeftSecond,
    /// <summary>The frame the installed artwork names <c>waver_spin_left_2</c>.</summary>
    WaverSpinningFacingLeftThird,
    /// <summary>The frame the installed artwork names <c>waver_spin_left_3</c>.</summary>
    WaverSpinningFacingLeftFourth,
    /// <summary>The frame the installed artwork names <c>waver_spin_right_0</c>.</summary>
    WaverSpinningFacingRightFirst,
    /// <summary>The frame the installed artwork names <c>waver_spin_right_1</c>.</summary>
    WaverSpinningFacingRightSecond,
    /// <summary>The frame the installed artwork names <c>waver_spin_right_2</c>.</summary>
    WaverSpinningFacingRightThird,
    /// <summary>The frame the installed artwork names <c>waver_spin_right_3</c>.</summary>
    WaverSpinningFacingRightFourth,
    /// <summary>The frame the installed artwork names <c>metaree_idle_0</c>.</summary>
    MetareeIdlingFirst,
    /// <summary>The frame the installed artwork names <c>metaree_idle_1</c>.</summary>
    MetareeIdlingSecond,
    /// <summary>The frame the installed artwork names <c>metaree_idle_2</c>.</summary>
    MetareeIdlingThird,
    /// <summary>The frame the installed artwork names <c>metaree_idle_3</c>.</summary>
    MetareeIdlingFourth,
    /// <summary>The frame the installed artwork names <c>metaree_prepare_1</c>.</summary>
    MetareePreparingAttackSecond,
    /// <summary>The frame the installed artwork names <c>skree_idle_0</c>.</summary>
    SkreeIdlingFirst,
    /// <summary>The frame the installed artwork names <c>skree_idle_1</c>.</summary>
    SkreeIdlingSecond,
    /// <summary>The frame the installed artwork names <c>skree_idle_2</c>.</summary>
    SkreeIdlingThird,
    /// <summary>The frame the installed artwork names <c>skree_idle_3</c>.</summary>
    SkreeIdlingFourth,
    /// <summary>The frame the installed artwork names <c>skree_prepare_1</c>.</summary>
    SkreePreparingAttackSecond,
    /// <summary>The frame the installed artwork names <c>zoa_shoot_left_0</c>.</summary>
    ZoaFacingLeftShootingFirst,
    /// <summary>The frame the installed artwork names <c>zoa_shoot_left_1</c>.</summary>
    ZoaFacingLeftShootingSecond,
    /// <summary>The frame the installed artwork names <c>zoa_shoot_left_2</c>.</summary>
    ZoaFacingLeftShootingThird,
    /// <summary>The frame the installed artwork names <c>zoa_rise_left_0</c>.</summary>
    ZoaFacingLeftRisingFirst,
    /// <summary>The frame the installed artwork names <c>zoa_rise_left_1</c>.</summary>
    ZoaFacingLeftRisingSecond,
    /// <summary>The frame the installed artwork names <c>zoa_rise_left_2</c>.</summary>
    ZoaFacingLeftRisingThird,
    /// <summary>The frame the installed artwork names <c>zoa_shoot_right_0</c>.</summary>
    ZoaFacingRightShootingFirst,
    /// <summary>The frame the installed artwork names <c>zoa_shoot_right_1</c>.</summary>
    ZoaFacingRightShootingSecond,
    /// <summary>The frame the installed artwork names <c>zoa_shoot_right_2</c>.</summary>
    ZoaFacingRightShootingThird,
    /// <summary>The frame the installed artwork names <c>zoa_rise_right_0</c>.</summary>
    ZoaFacingRightRisingFirst,
    /// <summary>The frame the installed artwork names <c>zoa_rise_right_1</c>.</summary>
    ZoaFacingRightRisingSecond,
    /// <summary>The frame the installed artwork names <c>zoa_rise_right_2</c>.</summary>
    ZoaFacingRightRisingThird,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_normal_left_0</c>.</summary>
    ZebRisingLeftFirst,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_normal_left_1</c>.</summary>
    ZebRisingLeftSecond,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_normal_left_2</c>.</summary>
    ZebRisingLeftThird,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_normal_left_3</c>.</summary>
    ZebRisingLeftFourth,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_normal_left_4</c>.</summary>
    ZebRisingLeftFifth,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_normal_right_0</c>.</summary>
    ZebRisingRightFirst,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_normal_right_1</c>.</summary>
    ZebRisingRightSecond,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_normal_right_2</c>.</summary>
    ZebRisingRightThird,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_normal_right_3</c>.</summary>
    ZebRisingRightFourth,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_normal_right_4</c>.</summary>
    ZebRisingRightFifth,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_shoot_left_0</c>.</summary>
    ZebboShootingLeftFirst,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_shoot_left_1</c>.</summary>
    ZebboShootingLeftSecond,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_shoot_left_2</c>.</summary>
    ZebboShootingLeftThird,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_rise_left_0</c>.</summary>
    ZebboRisingLeftFirst,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_rise_left_1</c>.</summary>
    ZebboRisingLeftSecond,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_rise_left_2</c>.</summary>
    ZebboRisingLeftThird,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_shoot_right_0</c>.</summary>
    ZebboShootingRightFirst,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_shoot_right_1</c>.</summary>
    ZebboShootingRightSecond,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_shoot_right_2</c>.</summary>
    ZebboShootingRightThird,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_rise_right_0</c>.</summary>
    ZebboRisingRightFirst,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_rise_right_1</c>.</summary>
    ZebboRisingRightSecond,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_rise_right_2</c>.</summary>
    ZebboRisingRightThird,
    /// <summary>The frame the installed artwork names <c>pipe_norfair_left_0</c>.</summary>
    GametRisingLeftFirst,
    /// <summary>The frame the installed artwork names <c>pipe_norfair_left_1</c>.</summary>
    GametRisingLeftSecond,
    /// <summary>The frame the installed artwork names <c>pipe_norfair_left_2</c>.</summary>
    GametRisingLeftThird,
    /// <summary>The frame the installed artwork names <c>pipe_norfair_left_3</c>.</summary>
    GametRisingLeftFourth,
    /// <summary>The frame the installed artwork names <c>pipe_norfair_left_4</c>.</summary>
    GametRisingLeftFifth,
    /// <summary>The frame the installed artwork names <c>pipe_norfair_right_0</c>.</summary>
    GametRisingRightFirst,
    /// <summary>The frame the installed artwork names <c>pipe_norfair_right_1</c>.</summary>
    GametRisingRightSecond,
    /// <summary>The frame the installed artwork names <c>pipe_norfair_right_2</c>.</summary>
    GametRisingRightThird,
    /// <summary>The frame the installed artwork names <c>pipe_norfair_right_3</c>.</summary>
    GametRisingRightFourth,
    /// <summary>The frame the installed artwork names <c>pipe_norfair_right_4</c>.</summary>
    GametRisingRightFifth,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_fly_left_0</c>.</summary>
    GeegaFlyingLeftFirst,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_fly_left_1</c>.</summary>
    GeegaFlyingLeftSecond,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_fly_left_2</c>.</summary>
    GeegaFlyingLeftThird,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_arc_left_0</c>.</summary>
    GeegaArcingLeftFirst,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_arc_left_1</c>.</summary>
    GeegaArcingLeftSecond,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_arc_left_2</c>.</summary>
    GeegaArcingLeftThird,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_fly_right_0</c>.</summary>
    GeegaFlyingRightFirst,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_fly_right_1</c>.</summary>
    GeegaFlyingRightSecond,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_fly_right_2</c>.</summary>
    GeegaFlyingRightThird,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_arc_right_0</c>.</summary>
    GeegaArcingRightFirst,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_arc_right_1</c>.</summary>
    GeegaArcingRightSecond,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_arc_right_2</c>.</summary>
    GeegaArcingRightThird,
    /// <summary>The frame the installed artwork names <c>fake_kraid_walk_left_0</c>.</summary>
    FakeKraidStepForwardsFacingLeftFirst,
    /// <summary>The frame the installed artwork names <c>fake_kraid_walk_left_1</c>.</summary>
    FakeKraidStepForwardsFacingLeftSecond,
    /// <summary>The frame the installed artwork names <c>fake_kraid_walk_left_2</c>.</summary>
    FakeKraidStepForwardsFacingLeftThird,
    /// <summary>The frame the installed artwork names <c>fake_kraid_walk_left_3</c>.</summary>
    FakeKraidStepForwardsFacingLeftFourth,
    /// <summary>The frame the installed artwork names <c>fake_kraid_spit_left_0</c>.</summary>
    FakeKraidFireSpitFacingLeftFirst,
    /// <summary>The frame the installed artwork names <c>fake_kraid_spit_left_1</c>.</summary>
    FakeKraidFireSpitFacingLeftSecond,
    /// <summary>The frame the installed artwork names <c>fake_kraid_spit_left_2</c>.</summary>
    FakeKraidFireSpitFacingLeftThird,
    /// <summary>The frame the installed artwork names <c>fake_kraid_walk_right_0</c>.</summary>
    FakeKraidStepForwardsFacingRightFirst,
    /// <summary>The frame the installed artwork names <c>fake_kraid_walk_right_1</c>.</summary>
    FakeKraidStepForwardsFacingRightSecond,
    /// <summary>The frame the installed artwork names <c>fake_kraid_walk_right_2</c>.</summary>
    FakeKraidStepForwardsFacingRightThird,
    /// <summary>The frame the installed artwork names <c>fake_kraid_walk_right_3</c>.</summary>
    FakeKraidStepForwardsFacingRightFourth,
    /// <summary>The frame the installed artwork names <c>fake_kraid_spit_right_0</c>.</summary>
    FakeKraidFireSpitFacingRightFirst,
    /// <summary>The frame the installed artwork names <c>fake_kraid_spit_right_1</c>.</summary>
    FakeKraidFireSpitFacingRightSecond,
    /// <summary>The frame the installed artwork names <c>fake_kraid_spit_right_2</c>.</summary>
    FakeKraidFireSpitFacingRightThird,
    /// <summary>The frame the installed artwork names <c>kraid_nail_0</c>.</summary>
    KraidNailLoopFirst,
    /// <summary>The frame the installed artwork names <c>kraid_nail_1</c>.</summary>
    KraidNailLoopSecond,
    /// <summary>The frame the installed artwork names <c>kraid_nail_2</c>.</summary>
    KraidNailLoopThird,
    /// <summary>The frame the installed artwork names <c>kraid_nail_3</c>.</summary>
    KraidNailLoopFourth,
    /// <summary>The frame the installed artwork names <c>kraid_nail_4</c>.</summary>
    KraidNailLoopFifth,
    /// <summary>The frame the installed artwork names <c>kraid_nail_5</c>.</summary>
    KraidNailLoopSixth,
    /// <summary>The frame the installed artwork names <c>kraid_nail_6</c>.</summary>
    KraidNailLoopSeventh,
    /// <summary>The frame the installed artwork names <c>kraid_nail_7</c>.</summary>
    KraidNailLoopEighth,
    /// <summary>The frame the installed artwork names <c>owtch_left_0</c>.</summary>
    OwtchMovingLeftLoopFirst,
    /// <summary>The frame the installed artwork names <c>owtch_left_1</c>.</summary>
    OwtchMovingLeftLoopSecond,
    /// <summary>The frame the installed artwork names <c>owtch_left_2</c>.</summary>
    OwtchMovingLeftLoopThird,
    /// <summary>The frame the installed artwork names <c>stoke_walk_left_0</c>.</summary>
    StokeMovingLeftLoopFirst,
    /// <summary>The frame the installed artwork names <c>stoke_walk_left_1</c>.</summary>
    StokeMovingLeftLoopSecond,
    /// <summary>The frame the installed artwork names <c>stoke_walk_left_2</c>.</summary>
    StokeMovingLeftLoopThird,
    /// <summary>The frame the installed artwork names <c>stoke_walk_left_3</c>.</summary>
    StokeMovingLeftLoopFourth,
    /// <summary>The frame the installed artwork names <c>stoke_attack_left</c>.</summary>
    StokeAttackLeft,
    /// <summary>The frame the installed artwork names <c>stoke_walk_right_0</c>.</summary>
    StokeMovingRightLoopFirst,
    /// <summary>The frame the installed artwork names <c>stoke_walk_right_1</c>.</summary>
    StokeMovingRightLoopSecond,
    /// <summary>The frame the installed artwork names <c>stoke_walk_right_2</c>.</summary>
    StokeMovingRightLoopThird,
    /// <summary>The frame the installed artwork names <c>stoke_walk_right_3</c>.</summary>
    StokeMovingRightLoopFourth,
    /// <summary>The frame the installed artwork names <c>stoke_attack_right</c>.</summary>
    StokeAttackRight,
    /// <summary>The frame the installed artwork names <c>ripper_shared_left_0</c>.</summary>
    GRipperMovingLeftFirst,
    /// <summary>The frame the installed artwork names <c>ripper_shared_left_1</c>.</summary>
    GRipperMovingLeftSecond,
    /// <summary>The frame the installed artwork names <c>ripper_shared_left_2</c>.</summary>
    GRipperMovingLeftThird,
    /// <summary>The frame the installed artwork names <c>ripper_shared_right_0</c>.</summary>
    GRipperMovingRightFirst,
    /// <summary>The frame the installed artwork names <c>ripper_shared_right_1</c>.</summary>
    GRipperMovingRightSecond,
    /// <summary>The frame the installed artwork names <c>ripper_shared_right_2</c>.</summary>
    GRipperMovingRightThird,
    /// <summary>The frame the installed artwork names <c>ripper_shared_frozen_left</c>.</summary>
    RipperSharedFrozenLeft,
    /// <summary>The frame the installed artwork names <c>ripper_shared_frozen_right</c>.</summary>
    RipperSharedFrozenRight,
    /// <summary>The frame the installed artwork names <c>ripper_left_0</c>.</summary>
    RipperMovingLeftFirst,
    /// <summary>The frame the installed artwork names <c>ripper_left_1</c>.</summary>
    RipperMovingLeftSecond,
    /// <summary>The frame the installed artwork names <c>ripper_left_2</c>.</summary>
    RipperMovingLeftThird,
    /// <summary>The frame the installed artwork names <c>ripper_right_0</c>.</summary>
    RipperMovingRightFirst,
    /// <summary>The frame the installed artwork names <c>ripper_right_1</c>.</summary>
    RipperMovingRightSecond,
    /// <summary>The frame the installed artwork names <c>ripper_right_2</c>.</summary>
    RipperMovingRightThird,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_0</c>.</summary>
    FirefleaLoopFirst,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_1</c>.</summary>
    FirefleaLoopSecond,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_2</c>.</summary>
    FirefleaLoopThird,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_3</c>.</summary>
    FirefleaLoopFourth,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_4</c>.</summary>
    FirefleaLoopFifth,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_5</c>.</summary>
    FirefleaLoopSixth,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_6</c>.</summary>
    FirefleaLoopSeventh,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_7</c>.</summary>
    FirefleaLoopEighth,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_8</c>.</summary>
    FirefleaLoopNinth,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_9</c>.</summary>
    FirefleaLoopTenth,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_10</c>.</summary>
    FirefleaLoopEleventh,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_11</c>.</summary>
    FirefleaLoopTwelfth,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_12</c>.</summary>
    FirefleaLoopThirteenth,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_13</c>.</summary>
    FirefleaLoopFourteenth,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_14</c>.</summary>
    FirefleaLoopFifteenth,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_15</c>.</summary>
    FirefleaLoopSixteenth,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_16</c>.</summary>
    FirefleaLoopSeventeenth,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_17</c>.</summary>
    FirefleaLoopEighteenth,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_18</c>.</summary>
    FirefleaLoopNineteenth,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_19</c>.</summary>
    FirefleaLoopTwentieth,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_20</c>.</summary>
    FirefleaLoopTwentyFirst,
    /// <summary>The frame the installed artwork names <c>magdollite_left_idle_0</c>.</summary>
    MagdolliteLeftIdleFirst,
    /// <summary>The frame the installed artwork names <c>magdollite_left_idle_1</c>.</summary>
    MagdolliteLeftIdleSecond,
    /// <summary>The frame the installed artwork names <c>magdollite_left_idle_2</c>.</summary>
    MagdolliteLeftIdleThird,
    /// <summary>The frame the installed artwork names <c>magdollite_left_throw_0</c>.</summary>
    MagdolliteLeftThrowFirst,
    /// <summary>The frame the installed artwork names <c>magdollite_left_throw_1</c>.</summary>
    MagdolliteLeftThrowSecond,
    /// <summary>The frame the installed artwork names <c>magdollite_left_throw_2</c>.</summary>
    MagdolliteLeftThrowThird,
    /// <summary>The frame the installed artwork names <c>magdollite_left_throw_3</c>.</summary>
    MagdolliteLeftThrowFourth,
    /// <summary>The frame the installed artwork names <c>magdollite_pillar_cap</c>.</summary>
    MagdollitePillarCap,
    /// <summary>The frame the installed artwork names <c>magdollite_left_submerge_0</c>.</summary>
    MagdolliteLeftRaisePillar,
    /// <summary>The frame the installed artwork names <c>magdollite_left_submerge_1</c>.</summary>
    MagdolliteLeftSubmergeSecond,
    /// <summary>The frame the installed artwork names <c>magdollite_left_submerge_2</c>.</summary>
    MagdolliteLeftSubmergeThird,
    /// <summary>The frame the installed artwork names <c>magdollite_right_idle_0</c>.</summary>
    MagdolliteRightIdleFirst,
    /// <summary>The frame the installed artwork names <c>magdollite_right_idle_1</c>.</summary>
    MagdolliteRightIdleSecond,
    /// <summary>The frame the installed artwork names <c>magdollite_right_idle_2</c>.</summary>
    MagdolliteRightIdleThird,
    /// <summary>The frame the installed artwork names <c>magdollite_right_throw_0</c>.</summary>
    MagdolliteRightThrowFirst,
    /// <summary>The frame the installed artwork names <c>magdollite_right_throw_1</c>.</summary>
    MagdolliteRightThrowSecond,
    /// <summary>The frame the installed artwork names <c>magdollite_right_throw_2</c>.</summary>
    MagdolliteRightThrowThird,
    /// <summary>The frame the installed artwork names <c>magdollite_right_throw_3</c>.</summary>
    MagdolliteRightThrowFourth,
    /// <summary>The frame the installed artwork names <c>magdollite_right_submerge_0</c>.</summary>
    MagdolliteRightRaisePillar,
    /// <summary>The frame the installed artwork names <c>magdollite_right_submerge_1</c>.</summary>
    MagdolliteRightSubmergeSecond,
    /// <summary>The frame the installed artwork names <c>magdollite_right_submerge_2</c>.</summary>
    MagdolliteRightSubmergeThird,
    /// <summary>The frame the installed artwork names <c>magdollite_pillar_phase_0</c>.</summary>
    MagdollitePillarPhaseFirst,
    /// <summary>The frame the installed artwork names <c>magdollite_pillar_phase_1</c>.</summary>
    MagdollitePillarPhaseSecond,
    /// <summary>The frame the installed artwork names <c>magdollite_pillar_phase_2</c>.</summary>
    MagdollitePillarPhaseThird,
    /// <summary>The frame the installed artwork names <c>magdollite_pillar_phase_3</c>.</summary>
    MagdollitePillarPhaseFourth,
    /// <summary>The frame the installed artwork names <c>magdollite_pillar_phase_4</c>.</summary>
    MagdollitePillarPhaseFifth,
    /// <summary>The frame the installed artwork names <c>magdollite_pillar_phase_5</c>.</summary>
    MagdollitePillarPhaseSixth,
    /// <summary>The frame the installed artwork names <c>magdollite_pillar_phase_6</c>.</summary>
    MagdollitePillarPhaseSeventh,
    /// <summary>The frame the installed artwork names <c>magdollite_pillar_phase_7</c>.</summary>
    MagdollitePillarPhaseEighth,
    /// <summary>The frame the installed artwork names <c>ceres_door_rotating_overlay</c>.</summary>
    CeresDoorRotatingOverlay,
    /// <summary>The frame the installed artwork names <c>ceres_door_left_hold</c>.</summary>
    CeresDoorLeftHold,
    /// <summary>The frame the installed artwork names <c>ceres_door_left_transition_0</c>.</summary>
    CeresDoorCloseFacingLeftFourth,
    /// <summary>The frame the installed artwork names <c>ceres_door_left_transition_1</c>.</summary>
    CeresDoorCloseFacingLeftThird,
    /// <summary>The frame the installed artwork names <c>ceres_door_left_transition_2</c>.</summary>
    CeresDoorCloseFacingLeftSecond,
    /// <summary>The frame the installed artwork names <c>ceres_door_left_transition_3</c>.</summary>
    CeresDoorCloseFacingLeftFirst,
    /// <summary>The frame the installed artwork names <c>ceres_door_right_hold</c>.</summary>
    CeresDoorRightHold,
    /// <summary>The frame the installed artwork names <c>ceres_door_right_transition_0</c>.</summary>
    CeresDoorRidleyRoomFacingRightFourth,
    /// <summary>The frame the installed artwork names <c>ceres_door_right_transition_1</c>.</summary>
    CeresDoorRidleyRoomFacingRightThird,
    /// <summary>The frame the installed artwork names <c>ceres_door_right_transition_2</c>.</summary>
    CeresDoorRidleyRoomFacingRightSecond,
    /// <summary>The frame the installed artwork names <c>ceres_door_right_transition_3</c>.</summary>
    CeresDoorRidleyRoomFacingRightFirst,
    /// <summary>The frame the installed artwork names <c>ceres_door_initial</c>.</summary>
    CeresDoorInitial,
    /// <summary>The frame the installed artwork names <c>ceres_door_mode7_left_wall</c>.</summary>
    CeresDoorTurningFloorLeftWall,
    /// <summary>The frame the installed artwork names <c>ceres_door_mode7_right_wall</c>.</summary>
    CeresDoorTurningFloorRightWall,
    /// <summary>The frame the installed artwork names <c>ceres_door_ridley_private_overlay</c>.</summary>
    CeresDoorRidleyPrivateOverlay,
    /// <summary>The frame the installed artwork names <c>ceres_baby_horizontal</c>.</summary>
    CeresBabyHorizontal,
    /// <summary>The frame the installed artwork names <c>ceres_baby_round</c>.</summary>
    CeresBabyRound,
    /// <summary>The frame the installed artwork names <c>ceres_baby_vertical</c>.</summary>
    CeresBabyVertical,
    /// <summary>The frame the installed artwork names <c>rio_pose_0</c>.</summary>
    RioIdleFirst,
    /// <summary>The frame the installed artwork names <c>rio_pose_1</c>.</summary>
    RioIdleSecond,
    /// <summary>The frame the installed artwork names <c>rio_pose_2</c>.</summary>
    RioPostSwoopIdleThird,
    /// <summary>The frame the installed artwork names <c>rio_pose_3</c>.</summary>
    RioPostSwoopIdleFourth,
    /// <summary>The frame the installed artwork names <c>rio_pose_4</c>.</summary>
    RioSwoopingPartOneThird,
    /// <summary>The frame the installed artwork names <c>rio_pose_5</c>.</summary>
    RioSwoopingPartOneFourth,
    /// <summary>The frame the installed artwork names <c>rio_pose_6</c>.</summary>
    RioSwoopingPartOneFifth,
    /// <summary>The frame the installed artwork names <c>rio_pose_7</c>.</summary>
    RioSwoopingPartTwoFirst,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_0</c>.</summary>
    LowerNorfairRioIdleLoopFirst,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_1</c>.</summary>
    LowerNorfairRioIdleLoopSecond,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_2</c>.</summary>
    LowerNorfairRioIdleLoopThird,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_3</c>.</summary>
    LowerNorfairRioPrepareToSwoopSecond,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_4</c>.</summary>
    LowerNorfairRioPrepareToSwoopThird,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_5</c>.</summary>
    LowerNorfairRioPrepareToSwoopFourth,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_6</c>.</summary>
    LowerNorfairRioPrepareToSwoopFifth,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_7</c>.</summary>
    LowerNorfairRioPrepareToSwoopSixth,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_8</c>.</summary>
    LowerNorfairRioPrepareToSwoopSeventh,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_9</c>.</summary>
    LowerNorfairRioPrepareToSwoopEighth,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_10</c>.</summary>
    LowerNorfairRioPrepareToSwoopNinth,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_11</c>.</summary>
    LowerNorfairRioAscendingPartOneThird,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_12</c>.</summary>
    LowerNorfairRioAscendingPartTwoLoopSecond,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_13</c>.</summary>
    LowerNorfairRioAscendingPartTwoLoopThird,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_14</c>.</summary>
    LowerNorfairRioAscendingPartTwoLoopFirst,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_15</c>.</summary>
    LowerNorfairRioFlamesFirst,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_16</c>.</summary>
    LowerNorfairRioFlamesSecond,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_17</c>.</summary>
    LowerNorfairRioFlamesThird,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_0</c>.</summary>
    NorfairRioIdleSecond,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_1</c>.</summary>
    NorfairRioIdleFirst,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_2</c>.</summary>
    NorfairRioIdleThird,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_3</c>.</summary>
    NorfairRioFlamesAscendingFirst,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_4</c>.</summary>
    NorfairRioFlamesAscendingSecond,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_5</c>.</summary>
    NorfairRioFlamesAscendingThird,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_6</c>.</summary>
    NorfairRioDescendingFirst,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_7</c>.</summary>
    NorfairRioDescendingSecond,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_8</c>.</summary>
    NorfairRioDescendingThird,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_9</c>.</summary>
    NorfairRioStartDescendingSecond,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_10</c>.</summary>
    NorfairRioStartDescendingThird,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_11</c>.</summary>
    NorfairRioStartDescendingFourth,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_12</c>.</summary>
    NorfairRioStartDescendingFifth,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_13</c>.</summary>
    NorfairRioStartDescendingSixth,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_14</c>.</summary>
    NorfairRioAscendingFirst,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_15</c>.</summary>
    NorfairRioAscendingSecond,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_16</c>.</summary>
    NorfairRioAscendingThird,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_17</c>.</summary>
    NorfairRioFlamesDescendingFirst,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_18</c>.</summary>
    NorfairRioFlamesDescendingSecond,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_19</c>.</summary>
    NorfairRioFlamesDescendingThird,
    /// <summary>The frame the installed artwork names <c>puyo_ground_0</c>.</summary>
    PuyoGroundedFastFirst,
    /// <summary>The frame the installed artwork names <c>puyo_ground_1</c>.</summary>
    PuyoGroundedFastSecond,
    /// <summary>The frame the installed artwork names <c>puyo_ground_2</c>.</summary>
    PuyoGroundedFastThird,
    /// <summary>The frame the installed artwork names <c>puyo_air_0</c>.</summary>
    PuyoAirborneFirst,
    /// <summary>The frame the installed artwork names <c>puyo_air_1</c>.</summary>
    PuyoAirborneSecond,
    /// <summary>The frame the installed artwork names <c>puyo_air_2</c>.</summary>
    PuyoAirborneThird,
    /// <summary>The frame the installed artwork names <c>puyo_air_3</c>.</summary>
    PuyoAirborneFourth,
    /// <summary>The frame the installed artwork names <c>puyo_air_4</c>.</summary>
    PuyoAirborneFifth,
    /// <summary>The frame the installed artwork names <c>bull_idle_0</c>.</summary>
    BullNormalFirst,
    /// <summary>The frame the installed artwork names <c>bull_idle_1</c>.</summary>
    BullNormalSecond,
    /// <summary>The frame the installed artwork names <c>bull_idle_2</c>.</summary>
    BullNormalThird,
    /// <summary>The frame the installed artwork names <c>alcoon_left_walk_0</c>.</summary>
    AlcoonWalkingLeftLoopFirst,
    /// <summary>The frame the installed artwork names <c>alcoon_left_walk_1</c>.</summary>
    AlcoonWalkingLeftLoopSecond,
    /// <summary>The frame the installed artwork names <c>alcoon_left_walk_2</c>.</summary>
    AlcoonWalkingLeftLoopThird,
    /// <summary>The frame the installed artwork names <c>alcoon_left_walk_3</c>.</summary>
    AlcoonWalkingLeftLoopFourth,
    /// <summary>The frame the installed artwork names <c>alcoon_left_fire_0</c>.</summary>
    AlcoonFireLeftFirst,
    /// <summary>The frame the installed artwork names <c>alcoon_left_fire_1</c>.</summary>
    AlcoonFireLeftSecond,
    /// <summary>The frame the installed artwork names <c>alcoon_left_fire_2</c>.</summary>
    AlcoonFireLeftThird,
    /// <summary>The frame the installed artwork names <c>alcoon_left_fire_3</c>.</summary>
    AlcoonFireLeftFourth,
    /// <summary>The frame the installed artwork names <c>alcoon_left_air_up</c>.</summary>
    AlcoonLeftAirUp,
    /// <summary>The frame the installed artwork names <c>alcoon_right_walk_0</c>.</summary>
    AlcoonWalkingRightLoopFirst,
    /// <summary>The frame the installed artwork names <c>alcoon_right_walk_1</c>.</summary>
    AlcoonWalkingRightLoopSecond,
    /// <summary>The frame the installed artwork names <c>alcoon_right_walk_2</c>.</summary>
    AlcoonWalkingRightLoopThird,
    /// <summary>The frame the installed artwork names <c>alcoon_right_walk_3</c>.</summary>
    AlcoonWalkingRightLoopFourth,
    /// <summary>The frame the installed artwork names <c>alcoon_right_fire_0</c>.</summary>
    AlcoonFireRightFirst,
    /// <summary>The frame the installed artwork names <c>alcoon_right_fire_1</c>.</summary>
    AlcoonFireRightSecond,
    /// <summary>The frame the installed artwork names <c>alcoon_right_fire_2</c>.</summary>
    AlcoonFireRightThird,
    /// <summary>The frame the installed artwork names <c>alcoon_right_fire_3</c>.</summary>
    AlcoonFireRightFourth,
    /// <summary>The frame the installed artwork names <c>alcoon_right_air_up</c>.</summary>
    AlcoonRightAirUp,
    /// <summary>The frame the installed artwork names <c>beetom_left_crawl_0</c>.</summary>
    BeetomCrawlingLeftLoopFirst,
    /// <summary>The frame the installed artwork names <c>beetom_left_crawl_1</c>.</summary>
    BeetomCrawlingLeftLoopSecond,
    /// <summary>The frame the installed artwork names <c>beetom_left_crawl_2</c>.</summary>
    BeetomCrawlingLeftLoopThird,
    /// <summary>The frame the installed artwork names <c>beetom_left_hop_0</c>.</summary>
    BeetomHopLeftFirst,
    /// <summary>The frame the installed artwork names <c>beetom_left_hop_1</c>.</summary>
    BeetomHopLeftSecond,
    /// <summary>The frame the installed artwork names <c>beetom_left_drain_0</c>.</summary>
    BeetomDrainingLeftFirst,
    /// <summary>The frame the installed artwork names <c>beetom_left_drain_1</c>.</summary>
    BeetomDrainingLeftSecond,
    /// <summary>The frame the installed artwork names <c>beetom_left_drain_2</c>.</summary>
    BeetomDrainingLeftThird,
    /// <summary>The frame the installed artwork names <c>beetom_left_drain_3</c>.</summary>
    BeetomDrainingLeftLoopFirst,
    /// <summary>The frame the installed artwork names <c>beetom_left_drain_4</c>.</summary>
    BeetomDrainingLeftLoopSecond,
    /// <summary>The frame the installed artwork names <c>beetom_left_drain_5</c>.</summary>
    BeetomDrainingLeftLoopThird,
    /// <summary>The frame the installed artwork names <c>beetom_right_crawl_0</c>.</summary>
    BeetomCrawlingRightLoopFirst,
    /// <summary>The frame the installed artwork names <c>beetom_right_crawl_1</c>.</summary>
    BeetomCrawlingRightLoopSecond,
    /// <summary>The frame the installed artwork names <c>beetom_right_crawl_2</c>.</summary>
    BeetomCrawlingRightLoopThird,
    /// <summary>The frame the installed artwork names <c>beetom_right_hop_0</c>.</summary>
    BeetomHopRightFirst,
    /// <summary>The frame the installed artwork names <c>beetom_right_hop_1</c>.</summary>
    BeetomHopRightSecond,
    /// <summary>The frame the installed artwork names <c>beetom_right_drain_0</c>.</summary>
    BeetomDrainingRightFirst,
    /// <summary>The frame the installed artwork names <c>beetom_right_drain_1</c>.</summary>
    BeetomDrainingRightSecond,
    /// <summary>The frame the installed artwork names <c>beetom_right_drain_2</c>.</summary>
    BeetomDrainingRightThird,
    /// <summary>The frame the installed artwork names <c>beetom_right_drain_3</c>.</summary>
    BeetomDrainingRightLoopFirst,
    /// <summary>The frame the installed artwork names <c>beetom_right_drain_4</c>.</summary>
    BeetomDrainingRightLoopSecond,
    /// <summary>The frame the installed artwork names <c>beetom_right_drain_5</c>.</summary>
    BeetomDrainingRightLoopThird,
    /// <summary>The frame the installed artwork names <c>sidehopper_jump_floor</c>.</summary>
    SidehopperJumpFloor,
    /// <summary>The frame the installed artwork names <c>sidehopper_land_floor_0</c>.</summary>
    SidehopperFloorLandedFirst,
    /// <summary>The frame the installed artwork names <c>sidehopper_land_floor_1</c>.</summary>
    SidehopperFloorLandedSecond,
    /// <summary>The frame the installed artwork names <c>sidehopper_jump_ceiling</c>.</summary>
    SidehopperJumpCeiling,
    /// <summary>The frame the installed artwork names <c>sidehopper_land_ceiling_0</c>.</summary>
    SidehopperCeilingLandedFirst,
    /// <summary>The frame the installed artwork names <c>sidehopper_land_ceiling_1</c>.</summary>
    SidehopperCeilingLandedSecond,
    /// <summary>The frame the installed artwork names <c>dessgeega_jump_floor</c>.</summary>
    DessgeegaJumpFloor,
    /// <summary>The frame the installed artwork names <c>dessgeega_land_floor_0</c>.</summary>
    DessgeegaFloorLandedFirst,
    /// <summary>The frame the installed artwork names <c>dessgeega_land_floor_1</c>.</summary>
    DessgeegaFloorLandedSecond,
    /// <summary>The frame the installed artwork names <c>dessgeega_jump_ceiling</c>.</summary>
    DessgeegaJumpCeiling,
    /// <summary>The frame the installed artwork names <c>dessgeega_land_ceiling_0</c>.</summary>
    DessgeegaCeilingLandedFirst,
    /// <summary>The frame the installed artwork names <c>dessgeega_land_ceiling_1</c>.</summary>
    DessgeegaCeilingLandedSecond,
    /// <summary>The frame the installed artwork names <c>large_sidehopper_jump_floor</c>.</summary>
    LargeSidehopperJumpFloor,
    /// <summary>The frame the installed artwork names <c>large_sidehopper_land_floor_0</c>.</summary>
    LargeSidehopperFloorLandedFirst,
    /// <summary>The frame the installed artwork names <c>large_sidehopper_land_floor_1</c>.</summary>
    LargeSidehopperFloorLandedSecond,
    /// <summary>The frame the installed artwork names <c>large_sidehopper_jump_ceiling</c>.</summary>
    LargeSidehopperJumpCeiling,
    /// <summary>The frame the installed artwork names <c>large_sidehopper_land_ceiling_0</c>.</summary>
    LargeSidehopperCeilingLandedFirst,
    /// <summary>The frame the installed artwork names <c>large_sidehopper_land_ceiling_1</c>.</summary>
    LargeSidehopperCeilingLandedSecond,
    /// <summary>The frame the installed artwork names <c>large_dessgeega_jump_floor</c>.</summary>
    LargeDessgeegaJumpFloor,
    /// <summary>The frame the installed artwork names <c>large_dessgeega_land_floor_0</c>.</summary>
    LargeDessgeegaFloorLandedFirst,
    /// <summary>The frame the installed artwork names <c>large_dessgeega_land_floor_1</c>.</summary>
    LargeDessgeegaFloorLandedSecond,
    /// <summary>The frame the installed artwork names <c>large_dessgeega_jump_ceiling</c>.</summary>
    LargeDessgeegaJumpCeiling,
    /// <summary>The frame the installed artwork names <c>large_dessgeega_land_ceiling_0</c>.</summary>
    LargeDessgeegaCeilingLandedFirst,
    /// <summary>The frame the installed artwork names <c>large_dessgeega_land_ceiling_1</c>.</summary>
    LargeDessgeegaCeilingLandedSecond,
    /// <summary>The frame the installed artwork names <c>choot_idle</c>.</summary>
    ChootIdle,
    /// <summary>The frame the installed artwork names <c>choot_jump</c>.</summary>
    ChootJump,
    /// <summary>The frame the installed artwork names <c>choot_jump_apex</c>.</summary>
    ChootJumpApex,
    /// <summary>The frame the installed artwork names <c>choot_fall_end</c>.</summary>
    ChootFallEnd,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_right_0</c>.</summary>
    HZoomerUpsideRightLoopFirst,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_right_1</c>.</summary>
    HZoomerUpsideRightLoopSecond,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_right_2</c>.</summary>
    HZoomerUpsideRightLoopThird,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_right_3</c>.</summary>
    HZoomerUpsideRightLoopFourth,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_right_4</c>.</summary>
    HZoomerUpsideRightLoopFifth,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_left_0</c>.</summary>
    HZoomerUpsideLeftLoopFirst,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_left_1</c>.</summary>
    HZoomerUpsideLeftLoopSecond,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_left_2</c>.</summary>
    HZoomerUpsideLeftLoopThird,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_left_3</c>.</summary>
    HZoomerUpsideLeftLoopFourth,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_left_4</c>.</summary>
    HZoomerUpsideLeftLoopFifth,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_down_0</c>.</summary>
    HZoomerUpsideDownLoopFirst,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_down_1</c>.</summary>
    HZoomerUpsideDownLoopSecond,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_down_2</c>.</summary>
    HZoomerUpsideDownLoopThird,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_down_3</c>.</summary>
    HZoomerUpsideDownLoopFourth,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_down_4</c>.</summary>
    HZoomerUpsideDownLoopFifth,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_up_0</c>.</summary>
    HZoomerUpsideUpLoopFirst,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_up_1</c>.</summary>
    HZoomerUpsideUpLoopSecond,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_up_2</c>.</summary>
    HZoomerUpsideUpLoopThird,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_up_3</c>.</summary>
    HZoomerUpsideUpLoopFourth,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_up_4</c>.</summary>
    HZoomerUpsideUpLoopFifth,
    /// <summary>The frame the installed artwork names <c>sbug_right_0</c>.</summary>
    SbugRightFirst,
    /// <summary>The frame the installed artwork names <c>sbug_right_1</c>.</summary>
    SbugRightSecond,
    /// <summary>The frame the installed artwork names <c>sbug_right_2</c>.</summary>
    SbugRightThird,
    /// <summary>The frame the installed artwork names <c>sbug_up_right_0</c>.</summary>
    SbugUpRightFirst,
    /// <summary>The frame the installed artwork names <c>sbug_up_right_1</c>.</summary>
    SbugUpRightSecond,
    /// <summary>The frame the installed artwork names <c>sbug_up_right_2</c>.</summary>
    SbugUpRightThird,
    /// <summary>The frame the installed artwork names <c>sbug_up_0</c>.</summary>
    SbugUpFirst,
    /// <summary>The frame the installed artwork names <c>sbug_up_1</c>.</summary>
    SbugUpSecond,
    /// <summary>The frame the installed artwork names <c>sbug_up_2</c>.</summary>
    SbugUpThird,
    /// <summary>The frame the installed artwork names <c>sbug_up_left_0</c>.</summary>
    SbugUpLeftFirst,
    /// <summary>The frame the installed artwork names <c>sbug_up_left_1</c>.</summary>
    SbugUpLeftSecond,
    /// <summary>The frame the installed artwork names <c>sbug_up_left_2</c>.</summary>
    SbugUpLeftThird,
    /// <summary>The frame the installed artwork names <c>sbug_left_0</c>.</summary>
    SbugLeftFirst,
    /// <summary>The frame the installed artwork names <c>sbug_left_1</c>.</summary>
    SbugLeftSecond,
    /// <summary>The frame the installed artwork names <c>sbug_left_2</c>.</summary>
    SbugLeftThird,
    /// <summary>The frame the installed artwork names <c>sbug_down_left_0</c>.</summary>
    SbugDownLeftFirst,
    /// <summary>The frame the installed artwork names <c>sbug_down_left_1</c>.</summary>
    SbugDownLeftSecond,
    /// <summary>The frame the installed artwork names <c>sbug_down_left_2</c>.</summary>
    SbugDownLeftThird,
    /// <summary>The frame the installed artwork names <c>sbug_down_0</c>.</summary>
    SbugDownFirst,
    /// <summary>The frame the installed artwork names <c>sbug_down_1</c>.</summary>
    SbugDownSecond,
    /// <summary>The frame the installed artwork names <c>sbug_down_2</c>.</summary>
    SbugDownThird,
    /// <summary>The frame the installed artwork names <c>sbug_down_right_0</c>.</summary>
    SbugDownRightFirst,
    /// <summary>The frame the installed artwork names <c>sbug_down_right_1</c>.</summary>
    SbugDownRightSecond,
    /// <summary>The frame the installed artwork names <c>sbug_down_right_2</c>.</summary>
    SbugDownRightThird,
    /// <summary>The frame the installed artwork names <c>fune_left_idle</c>.</summary>
    FuneLeftIdle,
    /// <summary>The frame the installed artwork names <c>fune_left_active_0</c>.</summary>
    FuneActiveLeftSecond,
    /// <summary>The frame the installed artwork names <c>fune_left_active_1</c>.</summary>
    FuneActiveLeftThird,
    /// <summary>The frame the installed artwork names <c>fune_left_active_2</c>.</summary>
    FuneActiveLeftFourth,
    /// <summary>The frame the installed artwork names <c>fune_left_active_3</c>.</summary>
    FuneActiveLeftFifth,
    /// <summary>The frame the installed artwork names <c>fune_right_idle</c>.</summary>
    FuneRightIdle,
    /// <summary>The frame the installed artwork names <c>fune_right_active_0</c>.</summary>
    FuneActiveRightSecond,
    /// <summary>The frame the installed artwork names <c>fune_right_active_1</c>.</summary>
    FuneActiveRightThird,
    /// <summary>The frame the installed artwork names <c>fune_right_active_2</c>.</summary>
    FuneActiveRightFourth,
    /// <summary>The frame the installed artwork names <c>fune_right_active_3</c>.</summary>
    FuneActiveRightFifth,
    /// <summary>The frame the installed artwork names <c>namihe_left_idle</c>.</summary>
    NamiheLeftIdle,
    /// <summary>The frame the installed artwork names <c>namihe_left_active_0</c>.</summary>
    NamiheActiveLeftSecond,
    /// <summary>The frame the installed artwork names <c>namihe_left_active_1</c>.</summary>
    NamiheActiveLeftThird,
    /// <summary>The frame the installed artwork names <c>namihe_left_active_2</c>.</summary>
    NamiheActiveLeftFourth,
    /// <summary>The frame the installed artwork names <c>namihe_left_active_3</c>.</summary>
    NamiheActiveLeftFifth,
    /// <summary>The frame the installed artwork names <c>namihe_left_active_4</c>.</summary>
    NamiheActiveLeftSixth,
    /// <summary>The frame the installed artwork names <c>namihe_right_idle</c>.</summary>
    NamiheRightIdle,
    /// <summary>The frame the installed artwork names <c>namihe_right_active_0</c>.</summary>
    NamiheActiveRightSecond,
    /// <summary>The frame the installed artwork names <c>namihe_right_active_1</c>.</summary>
    NamiheActiveRightThird,
    /// <summary>The frame the installed artwork names <c>namihe_right_active_2</c>.</summary>
    NamiheActiveRightFourth,
    /// <summary>The frame the installed artwork names <c>namihe_right_active_3</c>.</summary>
    NamiheActiveRightFifth,
    /// <summary>The frame the installed artwork names <c>namihe_right_active_4</c>.</summary>
    NamiheActiveRightSixth,
    /// <summary>The frame the installed artwork names <c>kamer_platform_0</c>.</summary>
    KamerPlatformFirst,
    /// <summary>The frame the installed artwork names <c>kamer_platform_1</c>.</summary>
    KamerPlatformSecond,
    /// <summary>The frame the installed artwork names <c>kamer_platform_2</c>.</summary>
    KamerPlatformThird,
    /// <summary>The frame the installed artwork names <c>kamer_platform_3</c>.</summary>
    KamerPlatformFourth,
    /// <summary>The frame the installed artwork names <c>elevator_platform_0</c>.</summary>
    ElevatorLoopFirst,
    /// <summary>The frame the installed artwork names <c>elevator_platform_1</c>.</summary>
    ElevatorLoopSecond,
    /// <summary>The frame the installed artwork names <c>draygon_intro_evir_0</c>.</summary>
    SpriteObjectEvirFacingLeftFirst,
    /// <summary>The frame the installed artwork names <c>draygon_intro_evir_1</c>.</summary>
    SpriteObjectEvirFacingLeftSecond,
    /// <summary>The frame the installed artwork names <c>draygon_intro_evir_2</c>.</summary>
    SpriteObjectEvirFacingLeftThird,
    /// <summary>The frame the installed artwork names <c>draygon_intro_evir_3</c>.</summary>
    SpriteObjectEvirFacingLeftFourth,
    /// <summary>The frame the installed artwork names <c>draygon_breath_bubble_0</c>.</summary>
    SpriteObjectShortDraygonBreathBubblesFirst,
    /// <summary>The frame the installed artwork names <c>draygon_breath_bubble_1</c>.</summary>
    SpriteObjectShortDraygonBreathBubblesSecond,
    /// <summary>The frame the installed artwork names <c>draygon_breath_bubble_2</c>.</summary>
    SpriteObjectShortDraygonBreathBubblesThird,
    /// <summary>The frame the installed artwork names <c>draygon_breath_bubble_3</c>.</summary>
    SpriteObjectShortDraygonBreathBubblesFourth,
    /// <summary>The frame the installed artwork names <c>draygon_breath_bubble_4</c>.</summary>
    SpriteObjectShortDraygonBreathBubblesFifth,
    /// <summary>The frame the installed artwork names <c>draygon_breath_bubble_5</c>.</summary>
    SpriteObjectShortDraygonBreathBubblesSixth,
    /// <summary>The frame the installed artwork names <c>draygon_breath_bubble_6</c>.</summary>
    SpriteObjectShortDraygonBreathBubblesSeventh,
    /// <summary>The frame the installed artwork names <c>draygon_breath_bubble_7</c>.</summary>
    SpriteObjectShortDraygonBreathBubblesEighth,
    /// <summary>The frame the installed artwork names <c>draygon_breath_bubble_8</c>.</summary>
    SpriteObjectShortDraygonBreathBubblesNinth,
    /// <summary>The frame the installed artwork names <c>sprite_object_empty</c>.</summary>
    SpriteObjectEmpty,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_dust_cloud_0</c>.</summary>
    SpriteObjectShortBigDustCloudFirst,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_dust_cloud_1</c>.</summary>
    SpriteObjectShortBigDustCloudSecond,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_dust_cloud_2</c>.</summary>
    SpriteObjectShortBigDustCloudThird,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_dust_cloud_3</c>.</summary>
    SpriteObjectShortBigDustCloudFourth,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_dust_cloud_4</c>.</summary>
    SpriteObjectBigDustCloudFifth,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_dust_cloud_5</c>.</summary>
    SpriteObjectBigDustCloudSixth,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_dust_cloud_6</c>.</summary>
    SpriteObjectBigDustCloudSeventh,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_dust_cloud_7</c>.</summary>
    SpriteObjectBigDustCloudEighth,
    /// <summary>The frame the installed artwork names <c>sprite_object_small_explosion_0</c>.</summary>
    SpriteObjectSmallExplosionFirst,
    /// <summary>The frame the installed artwork names <c>sprite_object_small_explosion_1</c>.</summary>
    SpriteObjectSmallExplosionSecond,
    /// <summary>The frame the installed artwork names <c>sprite_object_small_explosion_2</c>.</summary>
    SpriteObjectSmallExplosionThird,
    /// <summary>The frame the installed artwork names <c>sprite_object_small_explosion_3</c>.</summary>
    SpriteObjectSmallExplosionFourth,
    /// <summary>The frame the installed artwork names <c>sprite_object_small_explosion_4</c>.</summary>
    SpriteObjectSmallExplosionFifth,
    /// <summary>The frame the installed artwork names <c>sprite_object_small_explosion_5</c>.</summary>
    SpriteObjectSmallExplosionSixth,
    /// <summary>The frame the installed artwork names <c>sprite_object_dud_shot_0</c>.</summary>
    SpriteObjectDudShotFirst,
    /// <summary>The frame the installed artwork names <c>sprite_object_dud_shot_1</c>.</summary>
    SpriteObjectDudShotSecond,
    /// <summary>The frame the installed artwork names <c>sprite_object_dud_shot_2</c>.</summary>
    SpriteObjectDudShotThird,
    /// <summary>The frame the installed artwork names <c>sprite_object_dud_shot_3</c>.</summary>
    SpriteObjectDudShotFourth,
    /// <summary>The frame the installed artwork names <c>sprite_object_dud_shot_4</c>.</summary>
    SpriteObjectDudShotFifth,
    /// <summary>The frame the installed artwork names <c>sprite_object_dud_shot_5</c>.</summary>
    SpriteObjectDudShotSixth,
    /// <summary>The frame the installed artwork names <c>sprite_object_small_dust_cloud_0</c>.</summary>
    SpriteObjectSmallDudShotSecond,
    /// <summary>The frame the installed artwork names <c>sprite_object_small_dust_cloud_1</c>.</summary>
    SpriteObjectSmallDudShotThird,
    /// <summary>The frame the installed artwork names <c>sprite_object_small_dust_cloud_2</c>.</summary>
    SpriteObjectSmallDudShotFourth,
    /// <summary>The frame the installed artwork names <c>sprite_object_small_dust_cloud_3</c>.</summary>
    SpriteObjectSmallDudShotFifth,
    /// <summary>The frame the installed artwork names <c>sprite_object_pirate_landing_dust_cloud_0</c>.</summary>
    SpriteObjectSmallDudShotFirst,
    /// <summary>The frame the installed artwork names <c>sprite_object_pirate_landing_dust_cloud_1</c>.</summary>
    SpriteObjectSpacePirateLandingDustCloudSecond,
    /// <summary>The frame the installed artwork names <c>sprite_object_pirate_landing_dust_cloud_2</c>.</summary>
    SpriteObjectSpacePirateLandingDustCloudThird,
    /// <summary>The frame the installed artwork names <c>sprite_object_pirate_landing_dust_cloud_3</c>.</summary>
    SpriteObjectSpacePirateLandingDustCloudFourth,
    /// <summary>The frame the installed artwork names <c>sprite_object_smoke_0</c>.</summary>
    SpriteObjectSmokeFirst,
    /// <summary>The frame the installed artwork names <c>sprite_object_smoke_1</c>.</summary>
    SpriteObjectSmokeSecond,
    /// <summary>The frame the installed artwork names <c>sprite_object_smoke_2</c>.</summary>
    SpriteObjectSmokeThird,
    /// <summary>The frame the installed artwork names <c>sprite_object_smoke_3</c>.</summary>
    SpriteObjectSmokeFourth,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_explosion_0</c>.</summary>
    SpriteObjectBigExplosionFirst,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_explosion_1</c>.</summary>
    SpriteObjectBigExplosionSecond,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_explosion_2</c>.</summary>
    SpriteObjectBigExplosionThird,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_explosion_3</c>.</summary>
    SpriteObjectBigExplosionFourth,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_explosion_4</c>.</summary>
    SpriteObjectBigExplosionFifth,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_explosion_5</c>.</summary>
    SpriteObjectBigExplosionSixth,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_0</c>.</summary>
    SpriteObjectPuromiBodyFirst,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_1</c>.</summary>
    SpriteObjectPuromiBodySecond,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_2</c>.</summary>
    SpriteObjectPuromiBodyFourth,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_3</c>.</summary>
    SpriteObjectPuromiBodyFifth,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_4</c>.</summary>
    SpriteObjectPuromiBodyThird,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_5</c>.</summary>
    SpriteObjectPuromiBodySixth,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_6</c>.</summary>
    SpriteObjectPuromiBodySeventh,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_7</c>.</summary>
    SpriteObjectPuromiBodyEighth,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_right_explosion_0</c>.</summary>
    SpriteObjectPuromiRightExplosionFirst,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_right_explosion_1</c>.</summary>
    SpriteObjectPuromiRightExplosionSecond,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_right_explosion_2</c>.</summary>
    SpriteObjectPuromiRightExplosionThird,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_right_explosion_3</c>.</summary>
    SpriteObjectPuromiRightExplosionFourth,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_right_explosion_4</c>.</summary>
    SpriteObjectPuromiRightExplosionFifth,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_right_explosion_5</c>.</summary>
    SpriteObjectPuromiRightExplosionSixth,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_right_explosion_6</c>.</summary>
    SpriteObjectPuromiRightExplosionSeventh,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_left_explosion_0</c>.</summary>
    SpriteObjectPuromiLeftExplosionFirst,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_left_explosion_1</c>.</summary>
    SpriteObjectPuromiLeftExplosionSecond,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_left_explosion_2</c>.</summary>
    SpriteObjectPuromiLeftExplosionThird,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_left_explosion_3</c>.</summary>
    SpriteObjectPuromiLeftExplosionFourth,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_left_explosion_4</c>.</summary>
    SpriteObjectPuromiLeftExplosionFifth,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_left_explosion_5</c>.</summary>
    SpriteObjectPuromiLeftExplosionSixth,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_left_explosion_6</c>.</summary>
    SpriteObjectPuromiLeftExplosionSeventh,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_splash_0</c>.</summary>
    SpriteObjectPuromiSplashFirst,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_splash_1</c>.</summary>
    SpriteObjectPuromiSplashSecond,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_splash_2</c>.</summary>
    SpriteObjectPuromiSplashThird,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_splash_4</c>.</summary>
    SpriteObjectPuromiSplashFourth,
    /// <summary>The frame the installed artwork names <c>sprite_object_falling_spark_trail_0</c>.</summary>
    SpriteObjectFallingSparkTrailFirst,
    /// <summary>The frame the installed artwork names <c>sprite_object_falling_spark_trail_1</c>.</summary>
    SpriteObjectFallingSparkTrailSecond,
    /// <summary>The frame the installed artwork names <c>sprite_object_falling_spark_trail_2</c>.</summary>
    SpriteObjectFallingSparkTrailThird,
    /// <summary>The frame the installed artwork names <c>sprite_object_falling_spark_trail_3</c>.</summary>
    SpriteObjectFallingSparkTrailFourth,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_shell_0</c>.</summary>
    SpriteObjectMetroidShellFirst,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_shell_1</c>.</summary>
    SpriteObjectMetroidShellThird,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_shell_2</c>.</summary>
    SpriteObjectMetroidShellLoopThird,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_0</c>.</summary>
    SpriteObjectMetroidElectricityFirst,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_1</c>.</summary>
    SpriteObjectMetroidElectricitySecond,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_2</c>.</summary>
    SpriteObjectMetroidElectricityThird,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_3</c>.</summary>
    SpriteObjectMetroidElectricityFifth,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_4</c>.</summary>
    SpriteObjectMetroidElectricitySixth,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_5</c>.</summary>
    SpriteObjectMetroidElectricitySeventh,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_6</c>.</summary>
    SpriteObjectMetroidElectricityLoopFirst,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_7</c>.</summary>
    SpriteObjectMetroidElectricityLoopSecond,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_8</c>.</summary>
    SpriteObjectMetroidElectricityLoopThird,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_9</c>.</summary>
    SpriteObjectMetroidElectricityEleventh,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_10</c>.</summary>
    SpriteObjectMetroidElectricityTwelfth,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_11</c>.</summary>
    SpriteObjectMetroidElectricityThirteenth,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_12</c>.</summary>
    SpriteObjectMetroidElectricityEighth,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_13</c>.</summary>
    SpriteObjectMetroidElectricityNinth,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_14</c>.</summary>
    SpriteObjectMetroidElectricityTenth,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_15</c>.</summary>
    SpriteObjectMetroidElectricityFourteenth,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_16</c>.</summary>
    SpriteObjectMetroidElectricityFifteenth,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_17</c>.</summary>
    SpriteObjectMetroidElectricitySixteenth,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_18</c>.</summary>
    SpriteObjectMetroidElectricitySeventeenth,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_19</c>.</summary>
    SpriteObjectMetroidElectricityEighteenth,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_20</c>.</summary>
    SpriteObjectMetroidElectricityNineteenth,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_21</c>.</summary>
    SpriteObjectMetroidElectricityLoopFifth,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_22</c>.</summary>
    SpriteObjectMetroidElectricityLoopSixth,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_23</c>.</summary>
    SpriteObjectMetroidElectricityLoopSeventh,
    /// <summary>The frame the installed artwork names <c>sprite_object_yapping_maw_base_facing_down</c>.</summary>
    SpriteObjectYappingMawBaseFacingDown,
    /// <summary>The frame the installed artwork names <c>sprite_object_yapping_maw_base_facing_up</c>.</summary>
    SpriteObjectYappingMawBaseFacingUp,
    /// <summary>The frame the installed artwork names <c>sprite_object_evir_facing_right_0</c>.</summary>
    SpriteObjectEvirFacingRightFirst,
    /// <summary>The frame the installed artwork names <c>sprite_object_evir_facing_right_1</c>.</summary>
    SpriteObjectEvirFacingRightSecond,
    /// <summary>The frame the installed artwork names <c>sprite_object_evir_facing_right_2</c>.</summary>
    SpriteObjectEvirFacingRightThird,
    /// <summary>The frame the installed artwork names <c>sprite_object_evir_facing_right_3</c>.</summary>
    SpriteObjectEvirFacingRightFourth,
    /// <summary>The frame the installed artwork names <c>sprite_object_draygon_foaming_at_the_mouth_0</c>.</summary>
    SpriteObjectDraygonFoamingAtTheMouthFirst,
    /// <summary>The frame the installed artwork names <c>sprite_object_draygon_foaming_at_the_mouth_1</c>.</summary>
    SpriteObjectDraygonFoamingAtTheMouthSecond,
    /// <summary>The frame the installed artwork names <c>sprite_object_draygon_foaming_at_the_mouth_2</c>.</summary>
    SpriteObjectDraygonFoamingAtTheMouthThird,
    /// <summary>The frame the installed artwork names <c>sprite_object_draygon_foaming_at_the_mouth_3</c>.</summary>
    SpriteObjectDraygonFoamingAtTheMouthFourth,
    /// <summary>The frame the installed artwork names <c>sprite_object_draygon_foaming_at_the_mouth_4</c>.</summary>
    SpriteObjectDraygonFoamingAtTheMouthFifth,
    /// <summary>The frame the installed artwork names <c>sprite_object_draygon_foaming_at_the_mouth_5</c>.</summary>
    SpriteObjectDraygonFoamingAtTheMouthSixth,
    /// <summary>The frame the installed artwork names <c>sprite_object_draygon_foaming_at_the_mouth_6</c>.</summary>
    SpriteObjectDraygonFoamingAtTheMouthSeventh,
    /// <summary>The frame the installed artwork names <c>sprite_object_draygon_foaming_at_the_mouth_7</c>.</summary>
    SpriteObjectDraygonFoamingAtTheMouthEighth,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_0</c>.</summary>
    YappingMawAttackingUpFirst,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_1</c>.</summary>
    YappingMawAttackingUpSecond,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_2</c>.</summary>
    YappingMawAttackingUpThird,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_3</c>.</summary>
    YappingMawAttackingUpRightFirst,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_4</c>.</summary>
    YappingMawAttackingUpRightSecond,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_5</c>.</summary>
    YappingMawAttackingUpRightThird,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_6</c>.</summary>
    YappingMawAttackingRightFirst,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_7</c>.</summary>
    YappingMawAttackingRightSecond,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_8</c>.</summary>
    YappingMawAttackingRightThird,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_9</c>.</summary>
    YappingMawAttackingDownRightFirst,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_10</c>.</summary>
    YappingMawAttackingDownRightSecond,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_11</c>.</summary>
    YappingMawAttackingDownRightThird,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_12</c>.</summary>
    YappingMawAttackingDownFirst,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_13</c>.</summary>
    YappingMawAttackingDownSecond,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_14</c>.</summary>
    YappingMawAttackingDownThird,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_15</c>.</summary>
    YappingMawAttackingDownLeftFirst,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_16</c>.</summary>
    YappingMawAttackingDownLeftSecond,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_17</c>.</summary>
    YappingMawAttackingDownLeftThird,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_18</c>.</summary>
    YappingMawAttackingLeftFirst,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_19</c>.</summary>
    YappingMawAttackingLeftSecond,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_20</c>.</summary>
    YappingMawAttackingLeftThird,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_21</c>.</summary>
    YappingMawAttackingUpLeftFirst,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_22</c>.</summary>
    YappingMawAttackingUpLeftSecond,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_23</c>.</summary>
    YappingMawAttackingUpLeftThird,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_0</c>.</summary>
    KiHunterFlyingLeftFirst,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_1</c>.</summary>
    KiHunterFlyingLeftSecond,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_2</c>.</summary>
    KiHunterFlyingLeftThird,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_3</c>.</summary>
    KiHunterWingsLeftFirst,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_4</c>.</summary>
    KiHunterWingsLeftSecond,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_5</c>.</summary>
    KiHunterWingsLeftThird,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_6</c>.</summary>
    KiHunterFlyingRightFirst,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_7</c>.</summary>
    KiHunterFlyingRightSecond,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_8</c>.</summary>
    KiHunterFlyingRightThird,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_9</c>.</summary>
    KiHunterWingsRightFirst,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_10</c>.</summary>
    KiHunterWingsRightSecond,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_11</c>.</summary>
    KiHunterWingsRightThird,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_12</c>.</summary>
    KiHunterDetachedWings,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_13</c>.</summary>
    KiHunterSwoopLeftThird,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_14</c>.</summary>
    KiHunterSwoopLeftFirst,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_15</c>.</summary>
    KiHunterSwoopLeftSecond,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_16</c>.</summary>
    KiHunterSwoopLeftFourth,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_17</c>.</summary>
    KiHunterSwoopLeftFifth,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_18</c>.</summary>
    KiHunterSwoopRightThird,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_19</c>.</summary>
    KiHunterSwoopRightFirst,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_20</c>.</summary>
    KiHunterSwoopRightSecond,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_21</c>.</summary>
    KiHunterSwoopRightFourth,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_22</c>.</summary>
    KiHunterSwoopRightFifth,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_23</c>.</summary>
    KiHunterJumpLeftFirst,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_24</c>.</summary>
    KiHunterJumpLeftSecond,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_25</c>.</summary>
    KiHunterJumpLeftThird,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_26</c>.</summary>
    KiHunterJumpLeftFourth,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_27</c>.</summary>
    KiHunterJumpRightFirst,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_28</c>.</summary>
    KiHunterJumpRightSecond,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_29</c>.</summary>
    KiHunterJumpRightThird,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_30</c>.</summary>
    KiHunterJumpRightFourth,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_31</c>.</summary>
    KiHunterSpitLeftFirst,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_32</c>.</summary>
    KiHunterSpitLeftSecond,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_33</c>.</summary>
    KiHunterSpitLeftThird,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_34</c>.</summary>
    KiHunterSpitLeftFourth,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_35</c>.</summary>
    KiHunterSpitLeftFifth,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_36</c>.</summary>
    KiHunterSpitRightFirst,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_37</c>.</summary>
    KiHunterSpitRightSecond,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_38</c>.</summary>
    KiHunterSpitRightThird,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_39</c>.</summary>
    KiHunterSpitRightFourth,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_40</c>.</summary>
    KiHunterSpitRightFifth,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_0</c>.</summary>
    MotherBrainHeadInitial,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_1</c>.</summary>
    MotherBrainHeadNeutralPhaseTwoSecond,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_2</c>.</summary>
    MotherBrainHeadStretchingPhaseTwoFirst,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_3</c>.</summary>
    MotherBrainHeadStretchingPhaseTwoSecond,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_4</c>.</summary>
    MotherBrainHeadStretchingPhaseTwoThird,
    /// <summary>The frame the installed artwork names <c>mother_brain_neck_joint</c>.</summary>
    MotherBrainNeckJoint,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_6</c>.</summary>
    MotherBrainHeadDecapitatedLoop,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_7</c>.</summary>
    MotherBrainHeadDecapitatedSecond,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_8</c>.</summary>
    MotherBrainHeadStretchingPhaseThreeFirst,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_9</c>.</summary>
    MotherBrainHeadStretchingPhaseThreeSecond,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_10</c>.</summary>
    MotherBrainHeadStretchingPhaseThreeThird,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_11</c>.</summary>
    MotherBrainHeadCorpseFourth,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_12</c>.</summary>
    MotherBrainHeadCorpseLoop,
    /// <summary>The frame the installed artwork names <c>mother_brain_tube_0</c>.</summary>
    MotherBrainTubeFirst,
    /// <summary>The frame the installed artwork names <c>mother_brain_tube_1</c>.</summary>
    MotherBrainTubeSecond,
    /// <summary>The frame the installed artwork names <c>mother_brain_tube_2</c>.</summary>
    MotherBrainTubeThird,
    /// <summary>The frame the installed artwork names <c>mother_brain_tube_3</c>.</summary>
    MotherBrainTubeFourth,
    /// <summary>The frame the installed artwork names <c>mother_brain_tube_4</c>.</summary>
    MotherBrainTubeFifth,
    /// <summary>The frame the installed artwork names <c>dead_torizo_corpse_hook</c>.</summary>
    DeadTorizoCorpseHook,
    /// <summary>The frame the installed artwork names <c>ridley_tail_segment_small</c>.</summary>
    RidleyTailSegmentSmall,
    /// <summary>The frame the installed artwork names <c>ridley_tail_segment_medium</c>.</summary>
    RidleyTailSegmentMedium,
    /// <summary>The frame the installed artwork names <c>ridley_tail_segment_large</c>.</summary>
    RidleyTailSegmentLarge,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_12</c>.</summary>
    RidleyBreakupTailTipThirteenth,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_11</c>.</summary>
    RidleyBreakupTailTipTwelfth,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_10</c>.</summary>
    RidleyBreakupTailTipEleventh,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_9</c>.</summary>
    RidleyBreakupTailTipTenth,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_8</c>.</summary>
    RidleyBreakupTailTipNinth,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_7</c>.</summary>
    RidleyBreakupTailTipEighth,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_6</c>.</summary>
    RidleyBreakupTailTipSeventh,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_5</c>.</summary>
    RidleyBreakupTailTipSixth,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_4</c>.</summary>
    RidleyBreakupTailTipFifth,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_3</c>.</summary>
    RidleyBreakupTailTipFourth,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_2</c>.</summary>
    RidleyBreakupTailTipThird,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_1</c>.</summary>
    RidleyBreakupTailTipSecond,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_0</c>.</summary>
    RidleyBreakupTailTipFirst,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_15</c>.</summary>
    RidleyBreakupTailTipSixteenth,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_14</c>.</summary>
    RidleyBreakupTailTipFifteenth,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_13</c>.</summary>
    RidleyBreakupTailTipFourteenth,
    /// <summary>The frame the installed artwork names <c>ridley_wing_left_0</c>.</summary>
    RidleyWingFacingLeftFirst,
    /// <summary>The frame the installed artwork names <c>ridley_wing_left_1</c>.</summary>
    RidleyWingFacingLeftSecond,
    /// <summary>The frame the installed artwork names <c>ridley_wing_left_2</c>.</summary>
    RidleyWingFacingLeftThird,
    /// <summary>The frame the installed artwork names <c>ridley_wing_left_3</c>.</summary>
    RidleyWingFacingLeftFourth,
    /// <summary>The frame the installed artwork names <c>ridley_wing_left_4</c>.</summary>
    RidleyWingFacingLeftFifth,
    /// <summary>The frame the installed artwork names <c>ridley_wing_left_5</c>.</summary>
    RidleyWingFacingLeftSixth,
    /// <summary>The frame the installed artwork names <c>ridley_wing_right_0</c>.</summary>
    RidleyWingFacingRightFirst,
    /// <summary>The frame the installed artwork names <c>ridley_wing_right_1</c>.</summary>
    RidleyWingFacingRightSecond,
    /// <summary>The frame the installed artwork names <c>ridley_wing_right_2</c>.</summary>
    RidleyWingFacingRightThird,
    /// <summary>The frame the installed artwork names <c>ridley_wing_right_3</c>.</summary>
    RidleyWingFacingRightFourth,
    /// <summary>The frame the installed artwork names <c>ridley_wing_right_4</c>.</summary>
    RidleyWingFacingRightFifth,
    /// <summary>The frame the installed artwork names <c>ridley_wing_right_5</c>.</summary>
    RidleyWingFacingRightSixth,
    /// <summary>The frame the installed artwork names <c>sciser_upside_right_0</c>.</summary>
    SciserUpsideRightLoopFirst,
    /// <summary>The frame the installed artwork names <c>sciser_upside_right_1</c>.</summary>
    SciserUpsideRightLoopSecond,
    /// <summary>The frame the installed artwork names <c>sciser_upside_right_2</c>.</summary>
    SciserUpsideRightLoopThird,
    /// <summary>The frame the installed artwork names <c>sciser_upside_left_0</c>.</summary>
    SciserUpsideLeftLoopFirst,
    /// <summary>The frame the installed artwork names <c>sciser_upside_left_1</c>.</summary>
    SciserUpsideLeftLoopSecond,
    /// <summary>The frame the installed artwork names <c>sciser_upside_left_2</c>.</summary>
    SciserUpsideLeftLoopThird,
    /// <summary>The frame the installed artwork names <c>sciser_upside_down_0</c>.</summary>
    SciserUpsideDownLoopFirst,
    /// <summary>The frame the installed artwork names <c>sciser_upside_down_1</c>.</summary>
    SciserUpsideDownLoopSecond,
    /// <summary>The frame the installed artwork names <c>sciser_upside_down_2</c>.</summary>
    SciserUpsideDownLoopThird,
    /// <summary>The frame the installed artwork names <c>sciser_upside_up_0</c>.</summary>
    SciserUpsideUpLoopFirst,
    /// <summary>The frame the installed artwork names <c>sciser_upside_up_1</c>.</summary>
    SciserUpsideUpLoopSecond,
    /// <summary>The frame the installed artwork names <c>sciser_upside_up_2</c>.</summary>
    SciserUpsideUpLoopThird,
    /// <summary>The frame the installed artwork names <c>fly_shared_0</c>.</summary>
    FlyFlightFirst,
    /// <summary>The frame the installed artwork names <c>fly_shared_1</c>.</summary>
    FlyFlightSecond,
    /// <summary>The frame the installed artwork names <c>fly_shared_2</c>.</summary>
    FlyFlightThird,
    /// <summary>The frame the installed artwork names <c>fly_shared_3</c>.</summary>
    FlyFlightFourth,
    /// <summary>The frame the installed artwork names <c>kago_cycle_0</c>.</summary>
    KagoSlowFirst,
    /// <summary>The frame the installed artwork names <c>kago_cycle_1</c>.</summary>
    KagoSlowSecond,
    /// <summary>The frame the installed artwork names <c>kago_cycle_2</c>.</summary>
    KagoSlowThird,
    /// <summary>The frame the installed artwork names <c>face_block_neutral</c>.</summary>
    FaceBlockNeutral,
    /// <summary>The frame the installed artwork names <c>face_block_samus_left_1</c>.</summary>
    BlueBrinstarFaceBlockSamusLeftSecond,
    /// <summary>The frame the installed artwork names <c>face_block_samus_left_2</c>.</summary>
    BlueBrinstarFaceBlockSamusLeftThird,
    /// <summary>The frame the installed artwork names <c>face_block_samus_right_1</c>.</summary>
    BlueBrinstarFaceBlockSamusRightSecond,
    /// <summary>The frame the installed artwork names <c>face_block_samus_right_2</c>.</summary>
    BlueBrinstarFaceBlockSamusRightThird,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_0</c>.</summary>
    MorphBallEyeActiveFirst,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_1</c>.</summary>
    MorphBallEyeActiveSecond,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_2</c>.</summary>
    MorphBallEyeActiveThird,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_3</c>.</summary>
    MorphBallEyeActiveFourth,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_4</c>.</summary>
    MorphBallEyeActiveFifth,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_5</c>.</summary>
    MorphBallEyeActiveSixth,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_6</c>.</summary>
    MorphBallEyeActiveSeventh,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_7</c>.</summary>
    MorphBallEyeActiveEighth,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_8</c>.</summary>
    MorphBallEyeActiveNinth,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_9</c>.</summary>
    MorphBallEyeActiveTenth,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_10</c>.</summary>
    MorphBallEyeActiveEleventh,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_11</c>.</summary>
    MorphBallEyeActiveTwelfth,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_12</c>.</summary>
    MorphBallEyeActiveThirteenth,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_13</c>.</summary>
    MorphBallEyeActiveFourteenth,
    /// <summary>The frame the installed artwork names <c>morph_eye_right_transition</c>.</summary>
    MorphEyeRightTransition,
    /// <summary>The frame the installed artwork names <c>morph_eye_right_closed</c>.</summary>
    MorphEyeRightClosed,
    /// <summary>The frame the installed artwork names <c>morph_eye_left_transition</c>.</summary>
    MorphEyeLeftTransition,
    /// <summary>The frame the installed artwork names <c>morph_eye_left_closed</c>.</summary>
    MorphEyeLeftClosed,
    /// <summary>The frame the installed artwork names <c>morph_eye_mount_right</c>.</summary>
    MorphEyeMountRight,
    /// <summary>The frame the installed artwork names <c>morph_eye_mount_down</c>.</summary>
    MorphEyeMountDown,
    /// <summary>The frame the installed artwork names <c>morph_eye_mount_left</c>.</summary>
    MorphEyeMountLeft,
    /// <summary>The frame the installed artwork names <c>morph_eye_mount_up</c>.</summary>
    MorphEyeMountUp,
    /// <summary>The frame the installed artwork names <c>shutter_growing_10px</c>.</summary>
    ShutterGrowingTenPixels,
    /// <summary>The frame the installed artwork names <c>shutter_growing_20px</c>.</summary>
    ShutterGrowingTwentyPixels,
    /// <summary>The frame the installed artwork names <c>shutter_growing_30px</c>.</summary>
    ShutterGrowingThirtyPixels,
    /// <summary>The frame the installed artwork names <c>shutter_vertical_40px</c>.</summary>
    ShutterFortyPixels,
    /// <summary>The frame the installed artwork names <c>shutter_horizontal</c>.</summary>
    ShutterHorizontal,
    /// <summary>The frame the installed artwork names <c>metroid_body_0</c>.</summary>
    MetroidChasingSamusFirst,
    /// <summary>The frame the installed artwork names <c>metroid_body_1</c>.</summary>
    MetroidChasingSamusSecond,
    /// <summary>The frame the installed artwork names <c>metroid_body_2</c>.</summary>
    MetroidChasingSamusThird,
    /// <summary>The frame the installed artwork names <c>metroid_body_3</c>.</summary>
    MetroidChasingSamusFourth,
    /// <summary>The frame the installed artwork names <c>shaktool_final_saw_0</c>.</summary>
    ShaktoolSawHandFinalPieceFirst,
    /// <summary>The frame the installed artwork names <c>shaktool_final_saw_1</c>.</summary>
    ShaktoolSawHandFinalPieceSecond,
    /// <summary>The frame the installed artwork names <c>shaktool_final_saw_2</c>.</summary>
    ShaktoolSawHandFinalPieceThird,
    /// <summary>The frame the installed artwork names <c>shaktool_arm</c>.</summary>
    ShaktoolArm,
    /// <summary>The frame the installed artwork names <c>shaktool_head_left</c>.</summary>
    ShaktoolHeadLeft,
    /// <summary>The frame the installed artwork names <c>shaktool_head_up_left</c>.</summary>
    ShaktoolHeadUpLeft,
    /// <summary>The frame the installed artwork names <c>shaktool_head_up</c>.</summary>
    ShaktoolHeadUp,
    /// <summary>The frame the installed artwork names <c>shaktool_head_up_right</c>.</summary>
    ShaktoolHeadUpRight,
    /// <summary>The frame the installed artwork names <c>shaktool_head_right</c>.</summary>
    ShaktoolHeadRight,
    /// <summary>The frame the installed artwork names <c>shaktool_head_down_right</c>.</summary>
    ShaktoolHeadDownRight,
    /// <summary>The frame the installed artwork names <c>shaktool_head_down</c>.</summary>
    ShaktoolHeadDown,
    /// <summary>The frame the installed artwork names <c>shaktool_head_down_left</c>.</summary>
    ShaktoolHeadDownLeft,
    /// <summary>The frame the installed artwork names <c>shaktool_primary_saw_0</c>.</summary>
    ShaktoolSawHandPrimaryPieceFirst,
    /// <summary>The frame the installed artwork names <c>shaktool_primary_saw_1</c>.</summary>
    ShaktoolSawHandPrimaryPieceSecond,
    /// <summary>The frame the installed artwork names <c>shaktool_primary_saw_2</c>.</summary>
    ShaktoolSawHandPrimaryPieceThird,
    /// <summary>The frame the installed artwork names <c>tripper_moving_left_0</c>.</summary>
    TripperMovingLeftLoopFirst,
    /// <summary>The frame the installed artwork names <c>tripper_moving_left_1</c>.</summary>
    TripperMovingLeftLoopSecond,
    /// <summary>The frame the installed artwork names <c>tripper_moving_left_2</c>.</summary>
    TripperMovingLeftLoopThird,
    /// <summary>The frame the installed artwork names <c>tripper_moving_right_0</c>.</summary>
    TripperMovingRightLoopFirst,
    /// <summary>The frame the installed artwork names <c>tripper_moving_right_1</c>.</summary>
    TripperMovingRightLoopSecond,
    /// <summary>The frame the installed artwork names <c>tripper_moving_right_2</c>.</summary>
    TripperMovingRightLoopThird,
    /// <summary>The frame the installed artwork names <c>tripper_still_moving_left_0</c>.</summary>
    TripperStillMovingLeftLoopFirst,
    /// <summary>The frame the installed artwork names <c>tripper_still_moving_left_1</c>.</summary>
    TripperStillMovingLeftLoopSecond,
    /// <summary>The frame the installed artwork names <c>tripper_still_moving_left_2</c>.</summary>
    TripperStillMovingLeftLoopThird,
    /// <summary>The frame the installed artwork names <c>tripper_still_moving_right_0</c>.</summary>
    TripperStillMovingRightLoopFirst,
    /// <summary>The frame the installed artwork names <c>tripper_still_moving_right_1</c>.</summary>
    TripperStillMovingRightLoopSecond,
    /// <summary>The frame the installed artwork names <c>tripper_still_moving_right_2</c>.</summary>
    TripperStillMovingRightLoopThird,
    /// <summary>The frame the installed artwork names <c>tripper_frozen_moving_left</c>.</summary>
    TripperFrozenMovingLeft,
    /// <summary>The frame the installed artwork names <c>tripper_frozen_moving_right</c>.</summary>
    TripperFrozenMovingRight,
    /// <summary>The frame the installed artwork names <c>tripper_kamer_platform_0</c>.</summary>
    KamerMovingLeftLoopFirst,
    /// <summary>The frame the installed artwork names <c>tripper_kamer_platform_1</c>.</summary>
    KamerMovingLeftLoopSecond,
    /// <summary>The frame the installed artwork names <c>tripper_kamer_platform_2</c>.</summary>
    KamerMovingLeftLoopThird,
    /// <summary>The frame the installed artwork names <c>tripper_kamer_platform_3</c>.</summary>
    KamerMovingLeftLoopFourth,
    /// <summary>The frame the installed artwork names <c>dragon_body_idle_left</c>.</summary>
    DragonBodyIdleLeft,
    /// <summary>The frame the installed artwork names <c>dragon_wing_left_0</c>.</summary>
    DragonWingsFacingLeftFirst,
    /// <summary>The frame the installed artwork names <c>dragon_wing_left_1</c>.</summary>
    DragonWingsFacingLeftSecond,
    /// <summary>The frame the installed artwork names <c>dragon_body_idle_right</c>.</summary>
    DragonBodyIdleRight,
    /// <summary>The frame the installed artwork names <c>dragon_wing_right_0</c>.</summary>
    DragonWingsFacingRightFirst,
    /// <summary>The frame the installed artwork names <c>dragon_wing_right_1</c>.</summary>
    DragonWingsFacingRightSecond,
    /// <summary>The frame the installed artwork names <c>dragon_body_attack_left_0</c>.</summary>
    DragonAttackingFacingLeftFirst,
    /// <summary>The frame the installed artwork names <c>dragon_body_attack_left_1</c>.</summary>
    DragonAttackingFacingLeftSecond,
    /// <summary>The frame the installed artwork names <c>dragon_body_attack_left_2</c>.</summary>
    DragonAttackingFacingLeftThird,
    /// <summary>The frame the installed artwork names <c>dragon_body_attack_right_0</c>.</summary>
    DragonAttackingFacingRightFirst,
    /// <summary>The frame the installed artwork names <c>dragon_body_attack_right_1</c>.</summary>
    DragonAttackingFacingRightSecond,
    /// <summary>The frame the installed artwork names <c>dragon_body_attack_right_2</c>.</summary>
    DragonAttackingFacingRightThird,
    /// <summary>The frame the installed artwork names <c>multiviola_spin_0</c>.</summary>
    MultiviolaFlyingFirst,
    /// <summary>The frame the installed artwork names <c>multiviola_spin_1</c>.</summary>
    MultiviolaFlyingSecond,
    /// <summary>The frame the installed artwork names <c>multiviola_spin_2</c>.</summary>
    MultiviolaFlyingThird,
    /// <summary>The frame the installed artwork names <c>multiviola_spin_3</c>.</summary>
    MultiviolaFlyingFourth,
    /// <summary>The frame the installed artwork names <c>multiviola_spin_4</c>.</summary>
    MultiviolaFlyingFifth,
    /// <summary>The frame the installed artwork names <c>multiviola_spin_5</c>.</summary>
    MultiviolaFlyingSixth,
    /// <summary>The frame the installed artwork names <c>multiviola_spin_6</c>.</summary>
    MultiviolaFlyingSeventh,
    /// <summary>The frame the installed artwork names <c>multiviola_spin_7</c>.</summary>
    MultiviolaFlyingEighth,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_1</c>.</summary>
    NorfairLavaJumperHidden,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_2</c>.</summary>
    NorfairLavaJumperJumpSecond,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_3</c>.</summary>
    NorfairLavaJumperJumpThird,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_4</c>.</summary>
    NorfairLavaJumperJumpFourth,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_5</c>.</summary>
    NorfairLavaJumperJumpFifth,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_6</c>.</summary>
    NorfairLavaJumperJumpSixth,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_7</c>.</summary>
    NorfairLavaJumperJumpSeventh,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_8</c>.</summary>
    NorfairLavaJumperFollowerFirst,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_0</c>.</summary>
    NorfairLavaJumperFollowerSecond,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_10</c>.</summary>
    NorfairLavaJumperFollowerLoopFirst,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_9</c>.</summary>
    NorfairLavaJumperFollowerLoopThird,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_17</c>.</summary>
    ChozoStatueLowerNorfairInitial,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_18</c>.</summary>
    ChozoStatueLowerNorfairActivatedSecond,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_19</c>.</summary>
    ChozoStatueLowerNorfairActivatedThird,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_20</c>.</summary>
    ChozoStatueLowerNorfairActivatedFourth,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_21</c>.</summary>
    ChozoStatueLowerNorfairActivatedFifth,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_22</c>.</summary>
    ChozoStatueLowerNorfairActivatedSixth,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_23</c>.</summary>
    ChozoStatueLowerNorfairActivatedSeventh,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_24</c>.</summary>
    ChozoStatueLowerNorfairActivatedEighth,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_25</c>.</summary>
    ChozoStatueLowerNorfairActivatedNinth,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_0</c>.</summary>
    ChozoStatueWreckedShipInitial,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_1</c>.</summary>
    ChozoStatueWreckedShipActivatedSecond,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_2</c>.</summary>
    ChozoStatueWreckedShipActivatedThird,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_3</c>.</summary>
    ChozoStatueWreckedShipActivatedFourth,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_12</c>.</summary>
    ChozoStatueWreckedShipActivatedFifth,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_13</c>.</summary>
    ChozoStatueWreckedShipActivatedSixth,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_14</c>.</summary>
    ChozoStatueWreckedShipActivatedSeventh,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_15</c>.</summary>
    ChozoStatueWreckedShipActivatedEighth,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_16</c>.</summary>
    ChozoStatueWreckedShipActivatedNinth,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_11</c>.</summary>
    ChozoStatueWreckedShipActivatedLoopTwoFirst,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_4</c>.</summary>
    ChozoStatueWreckedShipActivatedLoopTwoSecond,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_5</c>.</summary>
    ChozoStatueWreckedShipActivatedLoopTwoThird,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_6</c>.</summary>
    ChozoStatueWreckedShipActivatedLoopTwoFourth,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_7</c>.</summary>
    ChozoStatueWreckedShipActivatedLoopTwoFifth,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_8</c>.</summary>
    ChozoStatueWreckedShipActivatedLoopTwoSixth,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_9</c>.</summary>
    ChozoStatueWreckedShipActivatedLoopTwoSeventh,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_10</c>.</summary>
    ChozoStatueWreckedShipActivatedLoopTwoEighth,
    /// <summary>The frame the installed artwork names <c>viola_spin_0</c>.</summary>
    ViolaNormalFirst,
    /// <summary>The frame the installed artwork names <c>viola_spin_1</c>.</summary>
    ViolaNormalSecond,
    /// <summary>The frame the installed artwork names <c>viola_spin_2</c>.</summary>
    ViolaNormalThird,
    /// <summary>The frame the installed artwork names <c>viola_spin_3</c>.</summary>
    ViolaNormalFourth,
    /// <summary>The frame the installed artwork names <c>viola_spin_4</c>.</summary>
    ViolaNormalFifth,
    /// <summary>The frame the installed artwork names <c>viola_spin_5</c>.</summary>
    ViolaNormalSixth,
    /// <summary>The frame the installed artwork names <c>viola_spin_6</c>.</summary>
    ViolaNormalSeventh,
    /// <summary>The frame the installed artwork names <c>viola_spin_7</c>.</summary>
    ViolaNormalEighth,
    /// <summary>The frame the installed artwork names <c>rinka_spin_0</c>.</summary>
    RinkaOrdinary,
    /// <summary>The frame the installed artwork names <c>rinka_spin_1</c>.</summary>
    RinkaOrdinaryLoopSecond,
    /// <summary>The frame the installed artwork names <c>rinka_spin_2</c>.</summary>
    RinkaOrdinaryLoopThird,
    /// <summary>The frame the installed artwork names <c>rinka_spin_3</c>.</summary>
    RinkaOrdinaryLoopFourth,
    /// <summary>The frame the installed artwork names <c>rinka_spin_4</c>.</summary>
    RinkaOrdinaryLoopFifth,
    /// <summary>The frame the installed artwork names <c>dead_torizo_stationary</c>.</summary>
    DeadTorizoStationary,
    /// <summary>The frame the installed artwork names <c>dead_zoomer_corpse_0</c>.</summary>
    DeadZoomerCorpseFirst,
    /// <summary>The frame the installed artwork names <c>dead_zoomer_corpse_2</c>.</summary>
    DeadZoomerCorpseSecond,
    /// <summary>The frame the installed artwork names <c>dead_zoomer_corpse_4</c>.</summary>
    DeadZoomerCorpseThird,
    /// <summary>The frame the installed artwork names <c>dead_ripper_corpse_0</c>.</summary>
    DeadRipperCorpseFirst,
    /// <summary>The frame the installed artwork names <c>dead_ripper_corpse_2</c>.</summary>
    DeadRipperCorpseSecond,
    /// <summary>The frame the installed artwork names <c>dead_skree_corpse_0</c>.</summary>
    DeadSkreeCorpseFirst,
    /// <summary>The frame the installed artwork names <c>dead_skree_corpse_2</c>.</summary>
    DeadSkreeCorpseSecond,
    /// <summary>The frame the installed artwork names <c>dead_skree_corpse_4</c>.</summary>
    DeadSkreeCorpseThird,
    /// <summary>The frame the installed artwork names <c>dead_sidehopper_hop_body</c>.</summary>
    DeadSidehopperHopBody,
    /// <summary>The frame the installed artwork names <c>dead_sidehopper_hop_alt</c>.</summary>
    DeadSidehopperHopAlt,
    /// <summary>The frame the installed artwork names <c>dead_sidehopper_idle</c>.</summary>
    DeadSidehopperIdle,
    /// <summary>The frame the installed artwork names <c>dead_sidehopper_corpse</c>.</summary>
    DeadSidehopperCorpse,
    /// <summary>The frame the installed artwork names <c>dead_sidehopper_initially_dead</c>.</summary>
    DeadSidehopperInitiallyDead,
    /// <summary>The frame the installed artwork names <c>mochtroid_flight_0</c>.</summary>
    MochtroidFreeFlightFirst,
    /// <summary>The frame the installed artwork names <c>mochtroid_flight_1</c>.</summary>
    MochtroidFreeFlightSecond,
    /// <summary>The frame the installed artwork names <c>mochtroid_flight_2</c>.</summary>
    MochtroidFreeFlightThird,
    /// <summary>The frame the installed artwork names <c>mochtroid_attached_0</c>.</summary>
    MochtroidAttachedFirst,
    /// <summary>The frame the installed artwork names <c>mochtroid_attached_1</c>.</summary>
    MochtroidAttachedSecond,
    /// <summary>The frame the installed artwork names <c>mochtroid_attached_2</c>.</summary>
    MochtroidAttachedThird,
    /// <summary>The frame the installed artwork names <c>evir_body_left_0</c>.</summary>
    EvirBodyFacingLeftFirst,
    /// <summary>The frame the installed artwork names <c>evir_body_left_1</c>.</summary>
    EvirBodyFacingLeftSecond,
    /// <summary>The frame the installed artwork names <c>evir_body_left_2</c>.</summary>
    EvirBodyFacingLeftThird,
    /// <summary>The frame the installed artwork names <c>evir_body_left_3</c>.</summary>
    EvirBodyFacingLeftFourth,
    /// <summary>The frame the installed artwork names <c>evir_body_left_4</c>.</summary>
    EvirBodyFacingLeftFifth,
    /// <summary>The frame the installed artwork names <c>evir_body_left_5</c>.</summary>
    EvirBodyFacingLeftSixth,
    /// <summary>The frame the installed artwork names <c>evir_arms_left_0</c>.</summary>
    EvirArmsFacingLeftFirst,
    /// <summary>The frame the installed artwork names <c>evir_arms_left_1</c>.</summary>
    EvirArmsFacingLeftSecond,
    /// <summary>The frame the installed artwork names <c>evir_arms_left_2</c>.</summary>
    EvirArmsFacingLeftThird,
    /// <summary>The frame the installed artwork names <c>evir_arms_left_3</c>.</summary>
    EvirArmsFacingLeftFourth,
    /// <summary>The frame the installed artwork names <c>evir_arms_left_4</c>.</summary>
    EvirArmsFacingLeftFifth,
    /// <summary>The frame the installed artwork names <c>evir_body_right_0</c>.</summary>
    EvirBodyFacingRightFirst,
    /// <summary>The frame the installed artwork names <c>evir_body_right_1</c>.</summary>
    EvirBodyFacingRightSecond,
    /// <summary>The frame the installed artwork names <c>evir_body_right_2</c>.</summary>
    EvirBodyFacingRightThird,
    /// <summary>The frame the installed artwork names <c>evir_body_right_3</c>.</summary>
    EvirBodyFacingRightFourth,
    /// <summary>The frame the installed artwork names <c>evir_body_right_4</c>.</summary>
    EvirBodyFacingRightFifth,
    /// <summary>The frame the installed artwork names <c>evir_body_right_5</c>.</summary>
    EvirBodyFacingRightSixth,
    /// <summary>The frame the installed artwork names <c>evir_arms_right_0</c>.</summary>
    EvirArmsFacingRightFirst,
    /// <summary>The frame the installed artwork names <c>evir_arms_right_1</c>.</summary>
    EvirArmsFacingRightSecond,
    /// <summary>The frame the installed artwork names <c>evir_arms_right_2</c>.</summary>
    EvirArmsFacingRightThird,
    /// <summary>The frame the installed artwork names <c>evir_arms_right_3</c>.</summary>
    EvirArmsFacingRightFourth,
    /// <summary>The frame the installed artwork names <c>evir_arms_right_4</c>.</summary>
    EvirArmsFacingRightFifth,
    /// <summary>The frame the installed artwork names <c>evir_projectile_flight</c>.</summary>
    EvirProjectileFlight,
    /// <summary>The frame the installed artwork names <c>evir_projectile_regenerating</c>.</summary>
    EvirProjectileRegenerating,
    /// <summary>The frame the installed artwork names <c>work_robot_left_00</c>.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopTwelfth,
    /// <summary>The frame the installed artwork names <c>work_robot_left_01</c>.</summary>
    WorkRobotInitial,
    /// <summary>The frame the installed artwork names <c>work_robot_left_02</c>.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopSecond,
    /// <summary>The frame the installed artwork names <c>work_robot_left_03</c>.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopThird,
    /// <summary>The frame the installed artwork names <c>work_robot_left_04</c>.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopFourth,
    /// <summary>The frame the installed artwork names <c>work_robot_left_05</c>.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopFifth,
    /// <summary>The frame the installed artwork names <c>work_robot_left_06</c>.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopSixth,
    /// <summary>The frame the installed artwork names <c>work_robot_left_07</c>.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopSeventh,
    /// <summary>The frame the installed artwork names <c>work_robot_left_08</c>.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopEighth,
    /// <summary>The frame the installed artwork names <c>work_robot_left_09</c>.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopNinth,
    /// <summary>The frame the installed artwork names <c>work_robot_left_10</c>.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopTenth,
    /// <summary>The frame the installed artwork names <c>work_robot_left_11</c>.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopEleventh,
    /// <summary>The frame the installed artwork names <c>work_robot_right_00</c>.</summary>
    WorkRobotApproachingFallRightFirst,
    /// <summary>The frame the installed artwork names <c>work_robot_right_01</c>.</summary>
    WorkRobotApproachingFallRightSecond,
    /// <summary>The frame the installed artwork names <c>work_robot_right_02</c>.</summary>
    WorkRobotApproachingFallRightThird,
    /// <summary>The frame the installed artwork names <c>work_robot_right_03</c>.</summary>
    WorkRobotFacingRightWalkingForwardsLoopFourth,
    /// <summary>The frame the installed artwork names <c>work_robot_right_04</c>.</summary>
    WorkRobotFacingRightWalkingForwardsLoopFifth,
    /// <summary>The frame the installed artwork names <c>work_robot_right_05</c>.</summary>
    WorkRobotFacingRightWalkingForwardsLoopSixth,
    /// <summary>The frame the installed artwork names <c>work_robot_right_06</c>.</summary>
    WorkRobotFacingRightWalkingForwardsLoopSeventh,
    /// <summary>The frame the installed artwork names <c>work_robot_right_07</c>.</summary>
    WorkRobotFacingRightWalkingForwardsLoopEighth,
    /// <summary>The frame the installed artwork names <c>work_robot_right_08</c>.</summary>
    WorkRobotFacingRightWalkingForwardsLoopNinth,
    /// <summary>The frame the installed artwork names <c>work_robot_right_09</c>.</summary>
    WorkRobotFacingRightWalkingForwardsLoopTenth,
    /// <summary>The frame the installed artwork names <c>work_robot_right_10</c>.</summary>
    WorkRobotFacingRightWalkingForwardsLoopEleventh,
    /// <summary>The frame the installed artwork names <c>work_robot_right_11</c>.</summary>
    WorkRobotFacingRightWalkingForwardsLoopTwelfth,
    /// <summary>The frame the installed artwork names <c>work_robot_unpowered_neutral</c>.</summary>
    WorkRobotUnpoweredNeutral,
    /// <summary>The frame the installed artwork names <c>work_robot_unpowered_left</c>.</summary>
    WorkRobotUnpoweredLeft,
    /// <summary>The frame the installed artwork names <c>work_robot_unpowered_right</c>.</summary>
    WorkRobotUnpoweredRight,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_right_up_00</c>.</summary>
    YardOutsideTurnUpsideRightMovingUpFirst,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_right_up_01</c>.</summary>
    YardOutsideTurnUpsideRightMovingUpSecond,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_right_up_02</c>.</summary>
    YardOutsideTurnUpsideRightMovingUpThird,
    /// <summary>The frame the installed artwork names <c>yard_crawl_top_left_00</c>.</summary>
    YardCrawlingUpsideUpMovingLeftFirst,
    /// <summary>The frame the installed artwork names <c>yard_crawl_top_left_01</c>.</summary>
    YardCrawlingUpsideUpMovingLeftSecond,
    /// <summary>The frame the installed artwork names <c>yard_crawl_top_left_02</c>.</summary>
    YardCrawlingUpsideUpMovingLeftThird,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_top_left_00</c>.</summary>
    YardOutsideTurnUpsideUpMovingLeftFirst,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_top_left_01</c>.</summary>
    YardOutsideTurnUpsideUpMovingLeftSecond,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_top_left_02</c>.</summary>
    YardOutsideTurnUpsideUpMovingLeftThird,
    /// <summary>The frame the installed artwork names <c>yard_crawl_left_down_00</c>.</summary>
    YardCrawlingUpsideLeftMovingDownFirst,
    /// <summary>The frame the installed artwork names <c>yard_crawl_left_down_01</c>.</summary>
    YardCrawlingUpsideLeftMovingDownSecond,
    /// <summary>The frame the installed artwork names <c>yard_crawl_left_down_02</c>.</summary>
    YardCrawlingUpsideLeftMovingDownThird,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_left_down_00</c>.</summary>
    YardOutsideTurnUpsideLeftMovingDownFirst,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_left_down_01</c>.</summary>
    YardOutsideTurnUpsideLeftMovingDownSecond,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_left_down_02</c>.</summary>
    YardOutsideTurnUpsideLeftMovingDownThird,
    /// <summary>The frame the installed artwork names <c>yard_crawl_bottom_right_00</c>.</summary>
    YardCrawlingUpsideDownMovingRightFirst,
    /// <summary>The frame the installed artwork names <c>yard_crawl_bottom_right_01</c>.</summary>
    YardCrawlingUpsideDownMovingRightSecond,
    /// <summary>The frame the installed artwork names <c>yard_crawl_bottom_right_02</c>.</summary>
    YardCrawlingUpsideDownMovingRightThird,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_bottom_right_00</c>.</summary>
    YardOutsideTurnUpsideDownMovingRightFirst,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_bottom_right_01</c>.</summary>
    YardOutsideTurnUpsideDownMovingRightSecond,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_bottom_right_02</c>.</summary>
    YardOutsideTurnUpsideDownMovingRightThird,
    /// <summary>The frame the installed artwork names <c>yard_crawl_right_up_00</c>.</summary>
    YardCrawlingUpsideRightMovingUpFirst,
    /// <summary>The frame the installed artwork names <c>yard_crawl_right_up_01</c>.</summary>
    YardCrawlingUpsideRightMovingUpSecond,
    /// <summary>The frame the installed artwork names <c>yard_crawl_right_up_02</c>.</summary>
    YardCrawlingUpsideRightMovingUpThird,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_left_up_00</c>.</summary>
    YardOutsideTurnUpsideLeftMovingUpFirst,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_left_up_01</c>.</summary>
    YardOutsideTurnUpsideLeftMovingUpSecond,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_left_up_02</c>.</summary>
    YardOutsideTurnUpsideLeftMovingUpThird,
    /// <summary>The frame the installed artwork names <c>yard_crawl_top_right_00</c>.</summary>
    YardCrawlingUpsideUpMovingRightFirst,
    /// <summary>The frame the installed artwork names <c>yard_crawl_top_right_01</c>.</summary>
    YardCrawlingUpsideUpMovingRightSecond,
    /// <summary>The frame the installed artwork names <c>yard_crawl_top_right_02</c>.</summary>
    YardCrawlingUpsideUpMovingRightThird,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_top_right_00</c>.</summary>
    YardOutsideTurnUpsideUpMovingRightFirst,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_top_right_01</c>.</summary>
    YardOutsideTurnUpsideUpMovingRightSecond,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_top_right_02</c>.</summary>
    YardOutsideTurnUpsideUpMovingRightThird,
    /// <summary>The frame the installed artwork names <c>yard_crawl_right_down_00</c>.</summary>
    YardCrawlingUpsideRightMovingDownFirst,
    /// <summary>The frame the installed artwork names <c>yard_crawl_right_down_01</c>.</summary>
    YardCrawlingUpsideRightMovingDownSecond,
    /// <summary>The frame the installed artwork names <c>yard_crawl_right_down_02</c>.</summary>
    YardCrawlingUpsideRightMovingDownThird,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_right_down_00</c>.</summary>
    YardOutsideTurnUpsideRightMovingDownFirst,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_right_down_01</c>.</summary>
    YardOutsideTurnUpsideRightMovingDownSecond,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_right_down_02</c>.</summary>
    YardOutsideTurnUpsideRightMovingDownThird,
    /// <summary>The frame the installed artwork names <c>yard_crawl_bottom_left_00</c>.</summary>
    YardCrawlingUpsideDownMovingLeftFirst,
    /// <summary>The frame the installed artwork names <c>yard_crawl_bottom_left_01</c>.</summary>
    YardCrawlingUpsideDownMovingLeftSecond,
    /// <summary>The frame the installed artwork names <c>yard_crawl_bottom_left_02</c>.</summary>
    YardCrawlingUpsideDownMovingLeftThird,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_bottom_left_00</c>.</summary>
    YardOutsideTurnUpsideDownMovingLeftFirst,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_bottom_left_01</c>.</summary>
    YardOutsideTurnUpsideDownMovingLeftSecond,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_bottom_left_02</c>.</summary>
    YardOutsideTurnUpsideDownMovingLeftThird,
    /// <summary>The frame the installed artwork names <c>yard_crawl_left_up_00</c>.</summary>
    YardCrawlingUpsideLeftMovingUpFirst,
    /// <summary>The frame the installed artwork names <c>yard_crawl_left_up_01</c>.</summary>
    YardCrawlingUpsideLeftMovingUpSecond,
    /// <summary>The frame the installed artwork names <c>yard_crawl_left_up_02</c>.</summary>
    YardCrawlingUpsideLeftMovingUpThird,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_top_left_00</c>.</summary>
    YardInsideTurnUpsideUpMovingLeftFirst,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_top_left_01</c>.</summary>
    YardInsideTurnUpsideUpMovingLeftSecond,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_top_left_02</c>.</summary>
    YardInsideTurnUpsideUpMovingLeftThird,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_right_up_00</c>.</summary>
    YardInsideTurnUpsideRightMovingUpFirst,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_right_up_01</c>.</summary>
    YardInsideTurnUpsideRightMovingUpSecond,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_right_up_02</c>.</summary>
    YardInsideTurnUpsideRightMovingUpThird,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_bottom_right_00</c>.</summary>
    YardInsideTurnUpsideDownMovingRightFirst,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_bottom_right_01</c>.</summary>
    YardInsideTurnUpsideDownMovingRightSecond,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_bottom_right_02</c>.</summary>
    YardInsideTurnUpsideDownMovingRightThird,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_left_down_00</c>.</summary>
    YardInsideTurnUpsideLeftMovingDownFirst,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_left_down_01</c>.</summary>
    YardInsideTurnUpsideLeftMovingDownSecond,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_left_down_02</c>.</summary>
    YardInsideTurnUpsideLeftMovingDownThird,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_top_right_00</c>.</summary>
    YardInsideTurnUpsideUpMovingRightFirst,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_top_right_01</c>.</summary>
    YardInsideTurnUpsideUpMovingRightSecond,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_top_right_02</c>.</summary>
    YardInsideTurnUpsideUpMovingRightThird,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_left_up_00</c>.</summary>
    YardInsideTurnUpsideLeftMovingUpFirst,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_left_up_01</c>.</summary>
    YardInsideTurnUpsideLeftMovingUpSecond,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_left_up_02</c>.</summary>
    YardInsideTurnUpsideLeftMovingUpThird,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_bottom_left_00</c>.</summary>
    YardInsideTurnUpsideDownMovingLeftFirst,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_bottom_left_01</c>.</summary>
    YardInsideTurnUpsideDownMovingLeftSecond,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_bottom_left_02</c>.</summary>
    YardInsideTurnUpsideDownMovingLeftThird,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_right_down_00</c>.</summary>
    YardInsideTurnUpsideRightMovingDownFirst,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_right_down_01</c>.</summary>
    YardInsideTurnUpsideRightMovingDownSecond,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_right_down_02</c>.</summary>
    YardInsideTurnUpsideRightMovingDownThird,
    /// <summary>The frame the installed artwork names <c>yard_hiding_top_left_00</c>.</summary>
    YardHidingUpsideUpMovingLeftFirst,
    /// <summary>The frame the installed artwork names <c>yard_hiding_top_left_01</c>.</summary>
    YardHidingUpsideUpMovingLeftSecond,
    /// <summary>The frame the installed artwork names <c>yard_hidden_top_left_00</c>.</summary>
    YardHiddenUpsideUpMovingLeftFirst,
    /// <summary>The frame the installed artwork names <c>yard_hidden_top_left_01</c>.</summary>
    YardHiddenUpsideUpMovingLeftSecond,
    /// <summary>The frame the installed artwork names <c>yard_hiding_bottom_left_00</c>.</summary>
    YardHidingUpsideDownMovingLeftFirst,
    /// <summary>The frame the installed artwork names <c>yard_hiding_bottom_left_01</c>.</summary>
    YardHidingUpsideDownMovingLeftSecond,
    /// <summary>The frame the installed artwork names <c>yard_hiding_bottom_left_02</c>.</summary>
    YardHidingUpsideDownMovingLeftThird,
    /// <summary>The frame the installed artwork names <c>yard_hiding_bottom_left_03</c>.</summary>
    YardHidingUpsideDownMovingLeftFourth,
    /// <summary>The frame the installed artwork names <c>yard_hiding_bottom_right_00</c>.</summary>
    YardHidingUpsideDownMovingRightFirst,
    /// <summary>The frame the installed artwork names <c>yard_hiding_bottom_right_01</c>.</summary>
    YardHidingUpsideDownMovingRightSecond,
    /// <summary>The frame the installed artwork names <c>yard_hiding_bottom_right_02</c>.</summary>
    YardHidingUpsideDownMovingRightThird,
    /// <summary>The frame the installed artwork names <c>yard_hiding_bottom_right_03</c>.</summary>
    YardHidingUpsideDownMovingRightFourth,
    /// <summary>The frame the installed artwork names <c>yard_hiding_top_right_00</c>.</summary>
    YardHidingUpsideUpMovingRightFirst,
    /// <summary>The frame the installed artwork names <c>yard_hiding_top_right_01</c>.</summary>
    YardHidingUpsideUpMovingRightSecond,
    /// <summary>The frame the installed artwork names <c>yard_hidden_top_right_00</c>.</summary>
    YardHiddenUpsideUpMovingRightFirst,
    /// <summary>The frame the installed artwork names <c>yard_hidden_top_right_01</c>.</summary>
    YardHiddenUpsideUpMovingRightSecond,
    /// <summary>The frame the installed artwork names <c>yard_hiding_right_up_00</c>.</summary>
    YardHidingUpsideRightMovingUpFirst,
    /// <summary>The frame the installed artwork names <c>yard_hiding_right_up_01</c>.</summary>
    YardHidingUpsideRightMovingUpSecond,
    /// <summary>The frame the installed artwork names <c>yard_hiding_right_up_02</c>.</summary>
    YardHidingUpsideRightMovingUpThird,
    /// <summary>The frame the installed artwork names <c>yard_hiding_right_up_03</c>.</summary>
    YardHidingUpsideRightMovingUpFourth,
    /// <summary>The frame the installed artwork names <c>yard_hiding_left_up_00</c>.</summary>
    YardHidingUpsideLeftMovingUpFirst,
    /// <summary>The frame the installed artwork names <c>yard_hiding_left_up_01</c>.</summary>
    YardHidingUpsideLeftMovingUpSecond,
    /// <summary>The frame the installed artwork names <c>yard_hiding_left_up_02</c>.</summary>
    YardHidingUpsideLeftMovingUpThird,
    /// <summary>The frame the installed artwork names <c>yard_hiding_left_up_03</c>.</summary>
    YardHidingUpsideLeftMovingUpFourth,
    /// <summary>The frame the installed artwork names <c>yard_hiding_left_down_00</c>.</summary>
    YardHidingUpsideLeftMovingDownFirst,
    /// <summary>The frame the installed artwork names <c>yard_hiding_left_down_01</c>.</summary>
    YardHidingUpsideLeftMovingDownSecond,
    /// <summary>The frame the installed artwork names <c>yard_hiding_left_down_02</c>.</summary>
    YardHidingUpsideLeftMovingDownThird,
    /// <summary>The frame the installed artwork names <c>yard_hiding_left_down_03</c>.</summary>
    YardHidingUpsideLeftMovingDownFourth,
    /// <summary>The frame the installed artwork names <c>yard_hiding_right_down_00</c>.</summary>
    YardHidingUpsideRightMovingDownFirst,
    /// <summary>The frame the installed artwork names <c>yard_hiding_right_down_01</c>.</summary>
    YardHidingUpsideRightMovingDownSecond,
    /// <summary>The frame the installed artwork names <c>yard_hiding_right_down_02</c>.</summary>
    YardHidingUpsideRightMovingDownThird,
    /// <summary>The frame the installed artwork names <c>yard_hiding_right_down_03</c>.</summary>
    YardHidingUpsideRightMovingDownFourth,
    /// <summary>The frame the installed artwork names <c>botwoon_head_00</c>.</summary>
    BotwoonHeadClosedUpLeft,
    /// <summary>The frame the installed artwork names <c>botwoon_head_01</c>.</summary>
    BotwoonHeadClosedLeft,
    /// <summary>The frame the installed artwork names <c>botwoon_head_02</c>.</summary>
    BotwoonHeadClosedDownLeft,
    /// <summary>The frame the installed artwork names <c>botwoon_head_03</c>.</summary>
    BotwoonHeadClosedDown,
    /// <summary>The frame the installed artwork names <c>botwoon_head_04</c>.</summary>
    BotwoonHeadClosedDownRight,
    /// <summary>The frame the installed artwork names <c>botwoon_head_05</c>.</summary>
    BotwoonHeadClosedRight,
    /// <summary>The frame the installed artwork names <c>botwoon_head_06</c>.</summary>
    BotwoonHeadClosedUpRight,
    /// <summary>The frame the installed artwork names <c>botwoon_head_07</c>.</summary>
    BotwoonHeadClosedUp,
    /// <summary>The frame the installed artwork names <c>botwoon_head_08</c>.</summary>
    BotwoonHeadOpenUpLeft,
    /// <summary>The frame the installed artwork names <c>botwoon_head_09</c>.</summary>
    BotwoonHeadOpenLeft,
    /// <summary>The frame the installed artwork names <c>botwoon_head_10</c>.</summary>
    BotwoonHeadOpenDownLeft,
    /// <summary>The frame the installed artwork names <c>botwoon_head_11</c>.</summary>
    BotwoonHeadOpenDown,
    /// <summary>The frame the installed artwork names <c>botwoon_head_12</c>.</summary>
    BotwoonHeadOpenDownRight,
    /// <summary>The frame the installed artwork names <c>botwoon_head_13</c>.</summary>
    BotwoonHeadOpenRight,
    /// <summary>The frame the installed artwork names <c>botwoon_head_14</c>.</summary>
    BotwoonHeadOpenUpRight,
    /// <summary>The frame the installed artwork names <c>botwoon_head_15</c>.</summary>
    BotwoonHeadOpenUp,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_00</c>.</summary>
    GunshipEntrancePadOpeningFirst,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_01</c>.</summary>
    GunshipEntrancePadOpeningSecond,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_02</c>.</summary>
    GunshipEntrancePadOpeningThird,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_03</c>.</summary>
    GunshipEntrancePadOpeningFourth,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_04</c>.</summary>
    GunshipEntrancePadOpeningFifth,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_05</c>.</summary>
    GunshipEntrancePadOpeningSixth,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_06</c>.</summary>
    GunshipEntrancePadOpeningSeventh,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_07</c>.</summary>
    GunshipEntrancePadOpeningEighth,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_08</c>.</summary>
    GunshipEntrancePadOpeningNinth,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_09</c>.</summary>
    GunshipEntrancePadOpeningTenth,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_10</c>.</summary>
    GunshipEntrancePadOpen,
    /// <summary>The frame the installed artwork names <c>gunship_top_hull</c>.</summary>
    GunshipTopHull,
    /// <summary>The frame the installed artwork names <c>gunship_bottom_hull</c>.</summary>
    GunshipBottomHull,
    /// <summary>The frame the installed artwork names <c>tatori_00</c>.</summary>
    TurtleBabyCrawlingLeftFirst,
    /// <summary>The frame the installed artwork names <c>tatori_01</c>.</summary>
    TurtleBabyCrawlingLeftSecond,
    /// <summary>The frame the installed artwork names <c>tatori_02</c>.</summary>
    TurtleBabyCrawlingLeftThird,
    /// <summary>The frame the installed artwork names <c>tatori_03</c>.</summary>
    TurtleBabyCrawlingLeftFourth,
    /// <summary>The frame the installed artwork names <c>tatori_04</c>.</summary>
    TurtleBabyCrawlingLeftFifth,
    /// <summary>The frame the installed artwork names <c>tatori_05</c>.</summary>
    TurtleBabyCrawlingLeftSixth,
    /// <summary>The frame the installed artwork names <c>tatori_06</c>.</summary>
    TurtleBabyCrawlingLeftSeventh,
    /// <summary>The frame the installed artwork names <c>tatori_07</c>.</summary>
    TurtleBabySpinningFirst,
    /// <summary>The frame the installed artwork names <c>tatori_08</c>.</summary>
    TurtleBabySpinningSecond,
    /// <summary>The frame the installed artwork names <c>tatori_09</c>.</summary>
    TurtleBabySpinningThird,
    /// <summary>The frame the installed artwork names <c>tatori_10</c>.</summary>
    TurtleBabySpinningFourth,
    /// <summary>The frame the installed artwork names <c>tatori_11</c>.</summary>
    TurtleMamaSpinningFirst,
    /// <summary>The frame the installed artwork names <c>tatori_12</c>.</summary>
    TurtleMamaSpinningSecond,
    /// <summary>The frame the installed artwork names <c>tatori_13</c>.</summary>
    TurtleMamaSpinningThird,
    /// <summary>The frame the installed artwork names <c>tatori_14</c>.</summary>
    TurtleMamaEnterShellLeftFirst,
    /// <summary>The frame the installed artwork names <c>tatori_15</c>.</summary>
    TurtleMamaEnterShellLeftSecond,
    /// <summary>The frame the installed artwork names <c>tatori_16</c>.</summary>
    TurtleMamaEnterShellLeftThird,
    /// <summary>The frame the installed artwork names <c>tatori_17</c>.</summary>
    TurtleBabyCrawlingRightFirst,
    /// <summary>The frame the installed artwork names <c>tatori_18</c>.</summary>
    TurtleBabyCrawlingRightSecond,
    /// <summary>The frame the installed artwork names <c>tatori_19</c>.</summary>
    TurtleBabyCrawlingRightThird,
    /// <summary>The frame the installed artwork names <c>tatori_20</c>.</summary>
    TurtleBabyCrawlingRightFourth,
    /// <summary>The frame the installed artwork names <c>tatori_21</c>.</summary>
    TurtleBabyCrawlingRightFifth,
    /// <summary>The frame the installed artwork names <c>tatori_22</c>.</summary>
    TurtleBabyCrawlingRightSixth,
    /// <summary>The frame the installed artwork names <c>tatori_23</c>.</summary>
    TurtleBabyCrawlingRightSeventh,
    /// <summary>The frame the installed artwork names <c>tatori_24</c>.</summary>
    TurtleMamaEnterShellRightFirst,
    /// <summary>The frame the installed artwork names <c>tatori_25</c>.</summary>
    TurtleMamaEnterShellRightSecond,
    /// <summary>The frame the installed artwork names <c>tatori_26</c>.</summary>
    TurtleMamaEnterShellRightThird,
    /// <summary>The frame the installed artwork names <c>tatori_27</c>.</summary>
    TurtleMamaEnterShellRightFourth,
    /// <summary>The frame the installed artwork names <c>tatori_28</c>.</summary>
    TurtleBabyHidingRightThird,
    /// <summary>The frame the installed artwork names <c>zero_crawler_00</c>.</summary>
    ZeroUpsideRightLoopFirst,
    /// <summary>The frame the installed artwork names <c>zero_crawler_01</c>.</summary>
    ZeroUpsideRightLoopSecond,
    /// <summary>The frame the installed artwork names <c>zero_crawler_02</c>.</summary>
    ZeroUpsideRightLoopThird,
    /// <summary>The frame the installed artwork names <c>zero_crawler_03</c>.</summary>
    ZeroUpsideRightLoopFourth,
    /// <summary>The frame the installed artwork names <c>zero_crawler_04</c>.</summary>
    ZeroUpsideLeftLoopFirst,
    /// <summary>The frame the installed artwork names <c>zero_crawler_05</c>.</summary>
    ZeroUpsideLeftLoopSecond,
    /// <summary>The frame the installed artwork names <c>zero_crawler_06</c>.</summary>
    ZeroUpsideLeftLoopThird,
    /// <summary>The frame the installed artwork names <c>zero_crawler_07</c>.</summary>
    ZeroUpsideLeftLoopFourth,
    /// <summary>The frame the installed artwork names <c>zero_crawler_08</c>.</summary>
    ZeroUpsideDownLoopFirst,
    /// <summary>The frame the installed artwork names <c>zero_crawler_09</c>.</summary>
    ZeroUpsideDownLoopSecond,
    /// <summary>The frame the installed artwork names <c>zero_crawler_10</c>.</summary>
    ZeroUpsideDownLoopThird,
    /// <summary>The frame the installed artwork names <c>zero_crawler_11</c>.</summary>
    ZeroUpsideDownLoopFourth,
    /// <summary>The frame the installed artwork names <c>zero_crawler_12</c>.</summary>
    ZeroUpsideUpLoopFirst,
    /// <summary>The frame the installed artwork names <c>zero_crawler_13</c>.</summary>
    ZeroUpsideUpLoopSecond,
    /// <summary>The frame the installed artwork names <c>zero_crawler_14</c>.</summary>
    ZeroUpsideUpLoopThird,
    /// <summary>The frame the installed artwork names <c>zero_crawler_15</c>.</summary>
    ZeroUpsideUpLoopFourth,
    /// <summary>The frame the installed artwork names <c>etecoon_00</c>.</summary>
    EtecoonLookRightAtSamusAndRunLeft,
    /// <summary>The frame the installed artwork names <c>etecoon_01</c>.</summary>
    EtecoonBeginRunningLeft,
    /// <summary>The frame the installed artwork names <c>etecoon_02</c>.</summary>
    EtecoonRunningLeftFirst,
    /// <summary>The frame the installed artwork names <c>etecoon_03</c>.</summary>
    EtecoonRunningLeftSecond,
    /// <summary>The frame the installed artwork names <c>etecoon_04</c>.</summary>
    EtecoonRunningLeftThird,
    /// <summary>The frame the installed artwork names <c>etecoon_05</c>.</summary>
    EtecoonWallJumpLeft,
    /// <summary>The frame the installed artwork names <c>etecoon_06</c>.</summary>
    EtecoonWallJumpLeftLoopFirst,
    /// <summary>The frame the installed artwork names <c>etecoon_07</c>.</summary>
    EtecoonWallJumpLeftLoopSecond,
    /// <summary>The frame the installed artwork names <c>etecoon_08</c>.</summary>
    EtecoonWallJumpLeftLoopThird,
    /// <summary>The frame the installed artwork names <c>etecoon_09</c>.</summary>
    EtecoonWallJumpLeftLoopFourth,
    /// <summary>The frame the installed artwork names <c>etecoon_10</c>.</summary>
    EtecoonHoppingFacingLeft,
    /// <summary>The frame the installed artwork names <c>etecoon_11</c>.</summary>
    EtecoonContinueHoppingFacingLeftFirst,
    /// <summary>The frame the installed artwork names <c>etecoon_12</c>.</summary>
    EtecoonContinueHoppingFacingLeftSecond,
    /// <summary>The frame the installed artwork names <c>etecoon_13</c>.</summary>
    EtecoonHitCeilingFirst,
    /// <summary>The frame the installed artwork names <c>etecoon_14</c>.</summary>
    EtecoonWallJumpLeftEligible,
    /// <summary>The frame the installed artwork names <c>etecoon_15</c>.</summary>
    EtecoonLookLeftAtSamusAndRunRight,
    /// <summary>The frame the installed artwork names <c>etecoon_16</c>.</summary>
    EtecoonBeginRunningRight,
    /// <summary>The frame the installed artwork names <c>etecoon_17</c>.</summary>
    EtecoonRunningRightFirst,
    /// <summary>The frame the installed artwork names <c>etecoon_18</c>.</summary>
    EtecoonRunningRightSecond,
    /// <summary>The frame the installed artwork names <c>etecoon_19</c>.</summary>
    EtecoonRunningRightThird,
    /// <summary>The frame the installed artwork names <c>etecoon_20</c>.</summary>
    EtecoonWallJumpRight,
    /// <summary>The frame the installed artwork names <c>etecoon_21</c>.</summary>
    EtecoonJumpingRightFirst,
    /// <summary>The frame the installed artwork names <c>etecoon_22</c>.</summary>
    EtecoonJumpingRightSecond,
    /// <summary>The frame the installed artwork names <c>etecoon_23</c>.</summary>
    EtecoonJumpingRightThird,
    /// <summary>The frame the installed artwork names <c>etecoon_24</c>.</summary>
    EtecoonJumpingRightFourth,
    /// <summary>The frame the installed artwork names <c>etecoon_25</c>.</summary>
    EtecoonHoppingFacingRight,
    /// <summary>The frame the installed artwork names <c>etecoon_26</c>.</summary>
    EtecoonContinueHoppingFacingRightFirst,
    /// <summary>The frame the installed artwork names <c>etecoon_27</c>.</summary>
    EtecoonContinueHoppingFacingRightSecond,
    /// <summary>The frame the installed artwork names <c>etecoon_28</c>.</summary>
    EtecoonContinueHoppingFacingRightThird,
    /// <summary>The frame the installed artwork names <c>etecoon_29</c>.</summary>
    EtecoonWallJumpRightEligible,
    /// <summary>The frame the installed artwork names <c>etecoon_30</c>.</summary>
    EtecoonFlexingLoopFifth,
    /// <summary>The frame the installed artwork names <c>dachora_00</c>.</summary>
    DachoraRunningLeftFirst,
    /// <summary>The frame the installed artwork names <c>dachora_01</c>.</summary>
    DachoraRunningLeftSecond,
    /// <summary>The frame the installed artwork names <c>dachora_02</c>.</summary>
    DachoraRunningLeftThird,
    /// <summary>The frame the installed artwork names <c>dachora_03</c>.</summary>
    DachoraRunningLeftFourth,
    /// <summary>The frame the installed artwork names <c>dachora_04</c>.</summary>
    DachoraRunningLeftFifth,
    /// <summary>The frame the installed artwork names <c>dachora_05</c>.</summary>
    DachoraRunningLeftSixth,
    /// <summary>The frame the installed artwork names <c>dachora_06</c>.</summary>
    DachoraIdleLeftFirst,
    /// <summary>The frame the installed artwork names <c>dachora_07</c>.</summary>
    DachoraIdleLeftSecond,
    /// <summary>The frame the installed artwork names <c>dachora_08</c>.</summary>
    DachoraIdleLeftThird,
    /// <summary>The frame the installed artwork names <c>dachora_09</c>.</summary>
    DachoraIdleLeftFourth,
    /// <summary>The frame the installed artwork names <c>dachora_10</c>.</summary>
    DachoraIdleLeftFifth,
    /// <summary>The frame the installed artwork names <c>dachora_11</c>.</summary>
    DachoraBlinkLeftSecond,
    /// <summary>The frame the installed artwork names <c>dachora_12</c>.</summary>
    DachoraBlinkLeftThird,
    /// <summary>The frame the installed artwork names <c>dachora_13</c>.</summary>
    DachoraEchoLeft,
    /// <summary>The frame the installed artwork names <c>dachora_14</c>.</summary>
    DachoraRunningRightFirst,
    /// <summary>The frame the installed artwork names <c>dachora_15</c>.</summary>
    DachoraRunningRightSecond,
    /// <summary>The frame the installed artwork names <c>dachora_16</c>.</summary>
    DachoraRunningRightThird,
    /// <summary>The frame the installed artwork names <c>dachora_17</c>.</summary>
    DachoraRunningRightFourth,
    /// <summary>The frame the installed artwork names <c>dachora_18</c>.</summary>
    DachoraRunningRightFifth,
    /// <summary>The frame the installed artwork names <c>dachora_19</c>.</summary>
    DachoraRunningRightSixth,
    /// <summary>The frame the installed artwork names <c>dachora_20</c>.</summary>
    DachoraIdleRightFirst,
    /// <summary>The frame the installed artwork names <c>dachora_21</c>.</summary>
    DachoraIdleRightSecond,
    /// <summary>The frame the installed artwork names <c>dachora_22</c>.</summary>
    DachoraIdleRightThird,
    /// <summary>The frame the installed artwork names <c>dachora_23</c>.</summary>
    DachoraIdleRightFourth,
    /// <summary>The frame the installed artwork names <c>dachora_24</c>.</summary>
    DachoraIdleRightFifth,
    /// <summary>The frame the installed artwork names <c>dachora_25</c>.</summary>
    DachoraBlinkRightSecond,
    /// <summary>The frame the installed artwork names <c>dachora_26</c>.</summary>
    DachoraBlinkRightThird,
    /// <summary>The frame the installed artwork names <c>dachora_27</c>.</summary>
    DachoraChargeRight,
    /// <summary>The frame the installed artwork names <c>dachora_28</c>.</summary>
    DachoraEchoRight,
    /// <summary>The frame the installed artwork names <c>escape_etecoon_00</c>.</summary>
    EscapeEtecoonRunningLeftLowTideLoopFirst,
    /// <summary>The frame the installed artwork names <c>escape_etecoon_01</c>.</summary>
    EscapeEtecoonRunningLeftLowTideLoopSecond,
    /// <summary>The frame the installed artwork names <c>escape_etecoon_02</c>.</summary>
    EscapeEtecoonRunningLeftLowTideLoopThird,
    /// <summary>The frame the installed artwork names <c>escape_etecoon_03</c>.</summary>
    EscapeEtecoonRunningRightLowTideLoopFirst,
    /// <summary>The frame the installed artwork names <c>escape_etecoon_04</c>.</summary>
    EscapeEtecoonRunningRightLowTideLoopSecond,
    /// <summary>The frame the installed artwork names <c>escape_etecoon_05</c>.</summary>
    EscapeEtecoonRunningRightLowTideLoopThird,
    /// <summary>The frame the installed artwork names <c>escape_etecoon_06</c>.</summary>
    EscapeEtecoonStationaryFirst,
    /// <summary>The frame the installed artwork names <c>escape_etecoon_07</c>.</summary>
    EscapeEtecoonStationarySecond,
    /// <summary>The frame the installed artwork names <c>escape_etecoon_08</c>.</summary>
    EscapeEtecoonStationaryThird,
    /// <summary>The frame the installed artwork names <c>escape_etecoon_09</c>.</summary>
    EscapeEtecoonStationaryFourth,
    /// <summary>The frame the installed artwork names <c>escape_dachora_00</c>.</summary>
    EscapeDachoraRunningAroundLowTideLeftFirst,
    /// <summary>The frame the installed artwork names <c>escape_dachora_01</c>.</summary>
    EscapeDachoraRunningAroundLowTideLeftSecond,
    /// <summary>The frame the installed artwork names <c>escape_dachora_02</c>.</summary>
    EscapeDachoraRunningAroundLowTideLeftThird,
    /// <summary>The frame the installed artwork names <c>escape_dachora_03</c>.</summary>
    EscapeDachoraRunningAroundLowTideLeftFourth,
    /// <summary>The frame the installed artwork names <c>escape_dachora_04</c>.</summary>
    EscapeDachoraRunningAroundLowTideLeftFifth,
    /// <summary>The frame the installed artwork names <c>escape_dachora_05</c>.</summary>
    EscapeDachoraRunningAroundLowTideLeftSixth,
    /// <summary>The frame the installed artwork names <c>escape_dachora_06</c>.</summary>
    EscapeDachoraRunningAroundLowTideRightFirst,
    /// <summary>The frame the installed artwork names <c>escape_dachora_07</c>.</summary>
    EscapeDachoraRunningAroundLowTideRightSecond,
    /// <summary>The frame the installed artwork names <c>escape_dachora_08</c>.</summary>
    EscapeDachoraRunningAroundLowTideRightThird,
    /// <summary>The frame the installed artwork names <c>escape_dachora_09</c>.</summary>
    EscapeDachoraRunningAroundLowTideRightFourth,
    /// <summary>The frame the installed artwork names <c>escape_dachora_10</c>.</summary>
    EscapeDachoraRunningAroundLowTideRightFifth,
    /// <summary>The frame the installed artwork names <c>escape_dachora_11</c>.</summary>
    EscapeDachoraRunningAroundLowTideRightSixth,
    /// <summary>The frame the installed artwork names <c>hibashi_00</c>.</summary>
    HibashiGraphicsFirst,
    /// <summary>The frame the installed artwork names <c>hibashi_01</c>.</summary>
    HibashiGraphicsSecond,
    /// <summary>The frame the installed artwork names <c>hibashi_02</c>.</summary>
    HibashiGraphicsThird,
    /// <summary>The frame the installed artwork names <c>hibashi_03</c>.</summary>
    HibashiGraphicsFourth,
    /// <summary>The frame the installed artwork names <c>hibashi_04</c>.</summary>
    HibashiGraphicsFifth,
    /// <summary>The frame the installed artwork names <c>hibashi_05</c>.</summary>
    HibashiGraphicsSixth,
    /// <summary>The frame the installed artwork names <c>hibashi_06</c>.</summary>
    HibashiGraphicsSeventh,
    /// <summary>The frame the installed artwork names <c>hibashi_07</c>.</summary>
    HibashiGraphicsEighth,
    /// <summary>The frame the installed artwork names <c>hibashi_08</c>.</summary>
    HibashiGraphicsNinth,
    /// <summary>The frame the installed artwork names <c>hibashi_09</c>.</summary>
    HibashiGraphicsTenth,
    /// <summary>The frame the installed artwork names <c>hibashi_10</c>.</summary>
    HibashiGraphicsEleventh,
    /// <summary>The frame the installed artwork names <c>hibashi_11</c>.</summary>
    HibashiGraphicsTwelfth,
    /// <summary>The frame the installed artwork names <c>hibashi_12</c>.</summary>
    HibashiGraphicsThirteenth,
    /// <summary>The frame the installed artwork names <c>hibashi_13</c>.</summary>
    HibashiGraphicsFourteenth,
    /// <summary>The frame the installed artwork names <c>hibashi_14</c>.</summary>
    HibashiGraphicsFifteenth,
    /// <summary>The frame the installed artwork names <c>hibashi_15</c>.</summary>
    HibashiGraphicsSixteenth,
    /// <summary>The frame the installed artwork names <c>hibashi_16</c>.</summary>
    HibashiGraphicsSeventeenth,
    /// <summary>The frame the installed artwork names <c>hibashi_17</c>.</summary>
    HibashiGraphicsEighteenth,
    /// <summary>The frame the installed artwork names <c>hibashi_18</c>.</summary>
    HibashiGraphicsNineteenth,
    /// <summary>The frame the installed artwork names <c>hibashi_19</c>.</summary>
    HibashiGraphicsTwentieth,
    /// <summary>The frame the installed artwork names <c>hibashi_20</c>.</summary>
    HibashiGraphicsTwentyFirst,
    /// <summary>The frame the installed artwork names <c>hibashi_21</c>.</summary>
    HibashiGraphicsTwentySecond,
    /// <summary>The frame the installed artwork names <c>hibashi_22</c>.</summary>
    HibashiGraphicsTwentyThird,
    /// <summary>The frame the installed artwork names <c>zebetite_00</c>.</summary>
    ZebetiteFirst,
    /// <summary>The frame the installed artwork names <c>zebetite_01</c>.</summary>
    ZebetiteSecond,
    /// <summary>The frame the installed artwork names <c>zebetite_02</c>.</summary>
    ZebetiteThird,
    /// <summary>The frame the installed artwork names <c>zebetite_03</c>.</summary>
    ZebetiteFourth,
    /// <summary>The frame the installed artwork names <c>zebetite_04</c>.</summary>
    ZebetiteFifth,
    /// <summary>The frame the installed artwork names <c>zebetite_05</c>.</summary>
    ZebetiteSixth,
    /// <summary>The frame the installed artwork names <c>zebetite_06</c>.</summary>
    ZebetiteSeventh,
    /// <summary>The frame the installed artwork names <c>zebetite_07</c>.</summary>
    ZebetiteEighth,
    /// <summary>The frame the installed artwork names <c>zebetite_08</c>.</summary>
    ZebetiteNinth,
    /// <summary>The frame the installed artwork names <c>zebetite_09</c>.</summary>
    ZebetiteTenth,
    /// <summary>The frame the installed artwork names <c>wrecked_ship_ghost_00</c>.</summary>
    WreckedShipGhostFloatingFirst,
    /// <summary>The frame the installed artwork names <c>wrecked_ship_ghost_01</c>.</summary>
    WreckedShipGhostFloatingSecond,
    /// <summary>The frame the installed artwork names <c>wrecked_ship_ghost_02</c>.</summary>
    WreckedShipGhostFloatingThird,
    /// <summary>The frame the installed artwork names <c>powamp_00</c>.</summary>
    PowampBodyFastFirst,
    /// <summary>The frame the installed artwork names <c>powamp_01</c>.</summary>
    PowampBodyFastSecond,
    /// <summary>The frame the installed artwork names <c>powamp_02</c>.</summary>
    PowampBodyFastThird,
    /// <summary>The frame the installed artwork names <c>powamp_03</c>.</summary>
    PowampBalloonInflateFirst,
    /// <summary>The frame the installed artwork names <c>powamp_04</c>.</summary>
    PowampBalloonInflateSecond,
    /// <summary>The frame the installed artwork names <c>powamp_05</c>.</summary>
    PowampBalloonInflateThird,
    /// <summary>The frame the installed artwork names <c>spark_00</c>.</summary>
    SparkFlickerOnFirst,
    /// <summary>The frame the installed artwork names <c>spark_01</c>.</summary>
    SparkFlickerOnThird,
    /// <summary>The frame the installed artwork names <c>spark_02</c>.</summary>
    SparkFlickerOnFourth,
    /// <summary>The frame the installed artwork names <c>spark_03</c>.</summary>
    SparkFlickerOnFifth,
    /// <summary>The frame the installed artwork names <c>spark_04</c>.</summary>
    SparkActiveFirst,
    /// <summary>The frame the installed artwork names <c>spark_05</c>.</summary>
    SparkActiveSecond,
    /// <summary>The frame the installed artwork names <c>spark_06</c>.</summary>
    SparkActiveThird,
    /// <summary>The frame the installed artwork names <c>spark_07</c>.</summary>
    SparkActiveFourth,
    /// <summary>The frame the installed artwork names <c>shitroid_00</c>.</summary>
    BabyMetroidInitialThird,
    /// <summary>The frame the installed artwork names <c>shitroid_01</c>.</summary>
    BabyMetroidInitialSecond,
    /// <summary>The frame the installed artwork names <c>shitroid_02</c>.</summary>
    BabyMetroidInitialFirst,
    /// <summary>The frame the installed artwork names <c>kraid_lint_00</c>.</summary>
    KraidLintInitial,
    /// <summary>The frame the installed artwork names <c>kraid_lint_01</c>.</summary>
    KraidLintPostGrowth,
    /// <summary>The frame the installed artwork names <c>nuclear_waffle_00</c>.</summary>
    NuclearWaffleBodyLoopFirst,
    /// <summary>The frame the installed artwork names <c>nuclear_waffle_01</c>.</summary>
    NuclearWaffleBodyLoopSecond,
    /// <summary>The frame the installed artwork names <c>nuclear_waffle_02</c>.</summary>
    NuclearWaffleBodyLoopThird,
    /// <summary>The frame the installed artwork names <c>nuclear_waffle_03</c>.</summary>
    NuclearWaffleBodyLoopFourth,
    /// <summary>The frame the installed artwork names <c>nuclear_waffle_04</c>.</summary>
    NuclearWaffleBodyLoopFifth,
    /// <summary>The frame the installed artwork names <c>nuclear_waffle_05</c>.</summary>
    NuclearWaffleBodyLoopSixth,
    /// <summary>The frame the installed artwork names <c>nuclear_waffle_06</c>.</summary>
    NuclearWaffleBodyLoopSeventh,
    /// <summary>The frame the installed artwork names <c>nuclear_waffle_07</c>.</summary>
    NuclearWaffleBodyLoopEighth,
    /// <summary>The frame the installed artwork names <c>kzan_00</c>.</summary>
    KzanIdle,
    /// <summary>The frame the installed artwork names <c>polyp_00</c>.</summary>
    PolypStationary,
    /// <summary>The frame the installed artwork names <c>ridley_breakup_legs_left</c>.</summary>
    RidleyBreakupLegsLeft,
    /// <summary>The frame the installed artwork names <c>ridley_breakup_legs_right</c>.</summary>
    RidleyBreakupLegsRight,
    /// <summary>The frame the installed artwork names <c>ridley_breakup_torso_left</c>.</summary>
    RidleyBreakupTorsoLeft,
    /// <summary>The frame the installed artwork names <c>ridley_breakup_torso_right</c>.</summary>
    RidleyBreakupTorsoRight,
    /// <summary>The frame the installed artwork names <c>ridley_breakup_head_left</c>.</summary>
    RidleyBreakupHeadLeft,
    /// <summary>The frame the installed artwork names <c>ridley_breakup_head_right</c>.</summary>
    RidleyBreakupHeadRight,
    /// <summary>The frame the installed artwork names <c>ridley_breakup_claw_left</c>.</summary>
    RidleyBreakupClawLeft,
    /// <summary>The frame the installed artwork names <c>ridley_breakup_claw_right</c>.</summary>
    RidleyBreakupClawRight,
    /// <summary>The frame the installed artwork names <c>walking_pirate_flinch_left</c>.</summary>
    WalkingPirateFlinchLeft,
    /// <summary>The frame the installed artwork names <c>walking_pirate_flinch_right</c>.</summary>
    WalkingPirateFlinchRight,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_left_0</c>.</summary>
    WalkingPirateWalkingLeftLoopFirst,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_left_1</c>.</summary>
    WalkingPirateWalkingLeftLoopSecond,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_left_2</c>.</summary>
    WalkingPirateWalkingLeftLoopThird,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_left_3</c>.</summary>
    WalkingPirateWalkingLeftLoopFourth,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_left_4</c>.</summary>
    WalkingPirateWalkingLeftLoopFifth,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_left_5</c>.</summary>
    WalkingPirateWalkingLeftLoopSixth,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_left_6</c>.</summary>
    WalkingPirateWalkingLeftLoopSeventh,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_left_7</c>.</summary>
    WalkingPirateWalkingLeftLoopEighth,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_left_0</c>.</summary>
    WalkingPirateFireLasersLeftFirst,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_left_1</c>.</summary>
    WalkingPirateFireLasersLeftSecond,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_left_2</c>.</summary>
    WalkingPirateFireLasersLeftThird,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_left_3</c>.</summary>
    WalkingPirateFireLasersLeftFourth,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_left_4</c>.</summary>
    WalkingPirateFireLasersLeftFifth,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_left_5</c>.</summary>
    WalkingPirateFireLasersLeftSixth,
    /// <summary>The frame the installed artwork names <c>walking_pirate_look_left_0</c>.</summary>
    WalkingPirateLookingFacingLeftFirst,
    /// <summary>The frame the installed artwork names <c>walking_pirate_look_left_1</c>.</summary>
    WalkingPirateLookingFacingLeftSecond,
    /// <summary>The frame the installed artwork names <c>walking_pirate_look_left_2</c>.</summary>
    WalkingPirateLookingFacingLeftThird,
    /// <summary>The frame the installed artwork names <c>walking_pirate_look_shared</c>.</summary>
    WalkingPirateLookShared,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_right_0</c>.</summary>
    WalkingPirateWalkingRightLoopFirst,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_right_1</c>.</summary>
    WalkingPirateWalkingRightLoopSecond,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_right_2</c>.</summary>
    WalkingPirateWalkingRightLoopThird,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_right_3</c>.</summary>
    WalkingPirateWalkingRightLoopFourth,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_right_4</c>.</summary>
    WalkingPirateWalkingRightLoopFifth,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_right_5</c>.</summary>
    WalkingPirateWalkingRightLoopSixth,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_right_6</c>.</summary>
    WalkingPirateWalkingRightLoopSeventh,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_right_7</c>.</summary>
    WalkingPirateWalkingRightLoopEighth,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_right_0</c>.</summary>
    WalkingPirateFireLasersRightFirst,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_right_1</c>.</summary>
    WalkingPirateFireLasersRightSecond,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_right_2</c>.</summary>
    WalkingPirateFireLasersRightThird,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_right_3</c>.</summary>
    WalkingPirateFireLasersRightFourth,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_right_4</c>.</summary>
    WalkingPirateFireLasersRightFifth,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_right_5</c>.</summary>
    WalkingPirateFireLasersRightSixth,
    /// <summary>The frame the installed artwork names <c>walking_pirate_look_right_0</c>.</summary>
    WalkingPirateLookingFacingRightFirst,
    /// <summary>The frame the installed artwork names <c>walking_pirate_look_right_1</c>.</summary>
    WalkingPirateLookingFacingRightSecond,
    /// <summary>The frame the installed artwork names <c>walking_pirate_look_right_2</c>.</summary>
    WalkingPirateLookingFacingRightThird,
    /// <summary>The frame the installed artwork names <c>wall_pirate_fire_jump_left_0</c>.</summary>
    WallPirateFireAndJumpLeftFirst,
    /// <summary>The frame the installed artwork names <c>wall_pirate_fire_jump_left_1</c>.</summary>
    WallPirateFireAndJumpLeftSecond,
    /// <summary>The frame the installed artwork names <c>wall_pirate_fire_jump_left_2</c>.</summary>
    WallPirateFireAndJumpLeftThird,
    /// <summary>The frame the installed artwork names <c>wall_pirate_fire_jump_left_3</c>.</summary>
    WallPirateFireAndJumpLeftFourth,
    /// <summary>The frame the installed artwork names <c>wall_pirate_climb_left_0</c>.</summary>
    WallPirateMovingUpLeftWallLoopFirst,
    /// <summary>The frame the installed artwork names <c>wall_pirate_climb_left_1</c>.</summary>
    WallPirateMovingUpLeftWallLoopSecond,
    /// <summary>The frame the installed artwork names <c>wall_pirate_climb_left_2</c>.</summary>
    WallPirateMovingUpLeftWallLoopThird,
    /// <summary>The frame the installed artwork names <c>wall_pirate_climb_left_3</c>.</summary>
    WallPirateMovingUpLeftWallLoopFourth,
    /// <summary>The frame the installed artwork names <c>wall_pirate_climb_left_4</c>.</summary>
    WallPirateMovingUpLeftWallLoopFifth,
    /// <summary>The frame the installed artwork names <c>wall_pirate_fire_jump_right_0</c>.</summary>
    WallPirateFireAndJumpRightFirst,
    /// <summary>The frame the installed artwork names <c>wall_pirate_fire_jump_right_1</c>.</summary>
    WallPirateFireAndJumpRightSecond,
    /// <summary>The frame the installed artwork names <c>wall_pirate_fire_jump_right_2</c>.</summary>
    WallPirateFireAndJumpRightThird,
    /// <summary>The frame the installed artwork names <c>wall_pirate_fire_jump_right_3</c>.</summary>
    WallPirateFireAndJumpRightFourth,
    /// <summary>The frame the installed artwork names <c>wall_pirate_climb_right_0</c>.</summary>
    WallPirateMovingDownRightWallLoopFirst,
    /// <summary>The frame the installed artwork names <c>wall_pirate_climb_right_1</c>.</summary>
    WallPirateMovingDownRightWallLoopSecond,
    /// <summary>The frame the installed artwork names <c>wall_pirate_climb_right_2</c>.</summary>
    WallPirateMovingDownRightWallLoopThird,
    /// <summary>The frame the installed artwork names <c>wall_pirate_climb_right_3</c>.</summary>
    WallPirateMovingDownRightWallLoopFourth,
    /// <summary>The frame the installed artwork names <c>wall_pirate_climb_right_4</c>.</summary>
    WallPirateMovingDownRightWallLoopFifth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_0</c>.</summary>
    NinjaPirateClawAttackLeftFirst,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_1</c>.</summary>
    NinjaPirateClawAttackLeftSecond,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_2</c>.</summary>
    NinjaPirateClawAttackLeftThird,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_3</c>.</summary>
    NinjaPirateClawAttackLeftFourth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_4</c>.</summary>
    NinjaPirateClawAttackLeftFifth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_5</c>.</summary>
    NinjaPirateClawAttackLeftSixth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_6</c>.</summary>
    NinjaPirateClawAttackLeftSeventh,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_7</c>.</summary>
    NinjaPirateClawAttackLeftEighth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_8</c>.</summary>
    NinjaPirateClawAttackLeftNinth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_17</c>.</summary>
    NinjaPirateSpinJumpLeftLoopFirst,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_18</c>.</summary>
    NinjaPirateSpinJumpLeftLoopSecond,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_19</c>.</summary>
    NinjaPirateSpinJumpLeftLoopThird,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_20</c>.</summary>
    NinjaPirateSpinJumpLeftLoopFourth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_21</c>.</summary>
    NinjaPirateSpinJumpLeftLoopFifth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_22</c>.</summary>
    NinjaPirateSpinJumpLeftLoopSixth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_23</c>.</summary>
    NinjaPirateSpinJumpLeftLoopSeventh,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_24</c>.</summary>
    NinjaPirateSpinJumpLeftLoopEighth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_49</c>.</summary>
    NinjaPirateActiveFacingLeftLoopFirst,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_50</c>.</summary>
    NinjaPirateActiveFacingLeftLoopSecond,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_51</c>.</summary>
    NinjaPirateActiveFacingLeftLoopThird,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_55</c>.</summary>
    NinjaPirateFlinchFacingLeft,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_65</c>.</summary>
    NinjaPirateDivekickLeftJump,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_57</c>.</summary>
    NinjaPirateDivekickLeftJumpLoop,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_58</c>.</summary>
    NinjaPirateDivekickLeftDive,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_33</c>.</summary>
    NinjaPirateWalkToLeftPostLoopFirst,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_34</c>.</summary>
    NinjaPirateWalkToLeftPostLoopSecond,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_35</c>.</summary>
    NinjaPirateWalkToLeftPostLoopThird,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_36</c>.</summary>
    NinjaPirateWalkToLeftPostLoopFourth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_37</c>.</summary>
    NinjaPirateWalkToLeftPostLoopFifth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_38</c>.</summary>
    NinjaPirateWalkToLeftPostLoopSixth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_39</c>.</summary>
    NinjaPirateWalkToLeftPostLoopSeventh,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_40</c>.</summary>
    NinjaPirateWalkToLeftPostLoopEighth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_69</c>.</summary>
    NinjaPirateInitialFacingLeftLoopFirst,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_70</c>.</summary>
    NinjaPirateInitialFacingLeftLoopSecond,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_71</c>.</summary>
    NinjaPirateInitialFacingLeftLoopThird,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_75</c>.</summary>
    NinjaPirateLandFacingLeftThird,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_73</c>.</summary>
    NinjaPirateLandFacingLeftFourth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_52</c>.</summary>
    NinjaPirateLandFacingLeftLoopFirst,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_53</c>.</summary>
    NinjaPirateLandFacingLeftLoopSecond,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_54</c>.</summary>
    NinjaPirateLandFacingLeftLoopThird,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_61</c>.</summary>
    NinjaPirateKickFacingLeftFirst,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_67</c>.</summary>
    NinjaPirateKickFacingLeftSecond,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_62</c>.</summary>
    NinjaPirateKickFacingLeftThird,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_9</c>.</summary>
    NinjaPirateClawAttackRightFirst,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_10</c>.</summary>
    NinjaPirateClawAttackRightSecond,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_11</c>.</summary>
    NinjaPirateClawAttackRightThird,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_12</c>.</summary>
    NinjaPirateClawAttackRightFourth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_13</c>.</summary>
    NinjaPirateClawAttackRightFifth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_14</c>.</summary>
    NinjaPirateClawAttackRightSixth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_15</c>.</summary>
    NinjaPirateClawAttackRightSeventh,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_16</c>.</summary>
    NinjaPirateClawAttackRightEighth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_25</c>.</summary>
    NinjaPirateSpinJumpRightLoopFirst,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_26</c>.</summary>
    NinjaPirateSpinJumpRightLoopSecond,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_27</c>.</summary>
    NinjaPirateSpinJumpRightLoopThird,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_28</c>.</summary>
    NinjaPirateSpinJumpRightLoopFourth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_29</c>.</summary>
    NinjaPirateSpinJumpRightLoopFifth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_30</c>.</summary>
    NinjaPirateSpinJumpRightLoopSixth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_31</c>.</summary>
    NinjaPirateSpinJumpRightLoopSeventh,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_32</c>.</summary>
    NinjaPirateSpinJumpRightLoopEighth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_56</c>.</summary>
    NinjaPirateFlinchFacingRight,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_66</c>.</summary>
    NinjaPirateDivekickRightJump,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_59</c>.</summary>
    NinjaPirateDivekickRightJumpLoop,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_60</c>.</summary>
    NinjaPirateDivekickRightDive,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_41</c>.</summary>
    NinjaPirateWalkToRightPostLoopFirst,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_42</c>.</summary>
    NinjaPirateWalkToRightPostLoopSecond,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_43</c>.</summary>
    NinjaPirateWalkToRightPostLoopThird,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_44</c>.</summary>
    NinjaPirateWalkToRightPostLoopFourth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_45</c>.</summary>
    NinjaPirateWalkToRightPostLoopFifth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_46</c>.</summary>
    NinjaPirateWalkToRightPostLoopSixth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_47</c>.</summary>
    NinjaPirateWalkToRightPostLoopSeventh,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_48</c>.</summary>
    NinjaPirateWalkToRightPostLoopEighth,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_72</c>.</summary>
    NinjaPirateInitialFacingRightLoopFirst,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_74</c>.</summary>
    NinjaPirateInitialFacingRightLoopThird,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_63</c>.</summary>
    NinjaPirateKickFacingRightFirst,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_68</c>.</summary>
    NinjaPirateKickFacingRightSecond,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_64</c>.</summary>
    NinjaPirateKickFacingRightThird,
    /// <summary>The frame the installed artwork names <c>ridley_body_left</c>.</summary>
    RidleyBodyLeft,
    /// <summary>The frame the installed artwork names <c>ridley_body_right</c>.</summary>
    RidleyBodyRight,
    /// <summary>The frame the installed artwork names <c>ridley_body_left_mouth_half_open</c>.</summary>
    RidleyBodyLeftMouthHalfOpen,
    /// <summary>The frame the installed artwork names <c>ridley_body_left_mouth_open</c>.</summary>
    RidleyBodyLeftMouthOpen,
    /// <summary>The frame the installed artwork names <c>ridley_body_right_mouth_half_open</c>.</summary>
    RidleyBodyRightMouthHalfOpen,
    /// <summary>The frame the installed artwork names <c>ridley_body_right_mouth_open</c>.</summary>
    RidleyBodyRightMouthOpen,
    /// <summary>The frame the installed artwork names <c>ridley_body_left_legs_half_extended</c>.</summary>
    RidleyBodyLeftLegsHalfExtended,
    /// <summary>The frame the installed artwork names <c>ridley_body_left_legs_extended</c>.</summary>
    RidleyBodyLeftLegsExtended,
    /// <summary>The frame the installed artwork names <c>ridley_body_right_legs_half_extended</c>.</summary>
    RidleyBodyRightLegsHalfExtended,
    /// <summary>The frame the installed artwork names <c>ridley_body_right_legs_extended</c>.</summary>
    RidleyBodyRightLegsExtended,
    /// <summary>The frame the installed artwork names <c>ridley_body_forward</c>.</summary>
    RidleyBodyForward,
    /// <summary>The frame the installed artwork names <c>draygon_frame_0</c>.</summary>
    DraygonArmsFacingLeftIdleFirst,
    /// <summary>The frame the installed artwork names <c>draygon_frame_1</c>.</summary>
    DraygonArmsFacingLeftIdleSecond,
    /// <summary>The frame the installed artwork names <c>draygon_frame_2</c>.</summary>
    DraygonArmsFacingLeftIdleThird,
    /// <summary>The frame the installed artwork names <c>draygon_frame_3</c>.</summary>
    DraygonArmsFacingLeftIdleFourth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_4</c>.</summary>
    DraygonArmsFacingLeftIdleFifth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_5</c>.</summary>
    DraygonArmsFacingLeftIdleSixth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_23</c>.</summary>
    DraygonArmsFacingLeftNearSwoopApexFirst,
    /// <summary>The frame the installed artwork names <c>draygon_frame_24</c>.</summary>
    DraygonArmsFacingLeftNearSwoopApexSecond,
    /// <summary>The frame the installed artwork names <c>draygon_frame_25</c>.</summary>
    DraygonArmsFacingLeftNearSwoopApexThird,
    /// <summary>The frame the installed artwork names <c>draygon_frame_26</c>.</summary>
    DraygonArmsFacingLeftNearSwoopApexFourth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_27</c>.</summary>
    DraygonTailFacingLeftIdleFirst,
    /// <summary>The frame the installed artwork names <c>draygon_frame_28</c>.</summary>
    DraygonTailFacingLeftIdleSecond,
    /// <summary>The frame the installed artwork names <c>draygon_frame_29</c>.</summary>
    DraygonTailFacingLeftIdleThird,
    /// <summary>The frame the installed artwork names <c>draygon_frame_30</c>.</summary>
    DraygonTailFacingLeftIdleFourth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_31</c>.</summary>
    DraygonTailFacingLeftIdleFifth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_32</c>.</summary>
    DraygonTailFacingLeftIdleSixth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_33</c>.</summary>
    DraygonTailFacingLeftIdleSeventh,
    /// <summary>The frame the installed artwork names <c>draygon_frame_34</c>.</summary>
    DraygonTailFacingLeftInitialFakeWhipSecond,
    /// <summary>The frame the installed artwork names <c>draygon_frame_35</c>.</summary>
    DraygonTailFacingLeftInitialFakeWhipThird,
    /// <summary>The frame the installed artwork names <c>draygon_frame_36</c>.</summary>
    DraygonTailFacingLeftInitialFakeWhipFourth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_37</c>.</summary>
    DraygonTailFacingLeftInitialFakeWhipFifth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_38</c>.</summary>
    DraygonTailFacingLeftInitialFakeWhipSixth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_39</c>.</summary>
    DraygonTailFacingLeftInitialFakeWhipSeventh,
    /// <summary>The frame the installed artwork names <c>draygon_frame_40</c>.</summary>
    DraygonTailFacingLeftInitialFakeWhipEighth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_41</c>.</summary>
    DraygonArmsFacingRightIdleFirst,
    /// <summary>The frame the installed artwork names <c>draygon_frame_42</c>.</summary>
    DraygonArmsFacingRightIdleSecond,
    /// <summary>The frame the installed artwork names <c>draygon_frame_43</c>.</summary>
    DraygonArmsFacingRightIdleThird,
    /// <summary>The frame the installed artwork names <c>draygon_frame_44</c>.</summary>
    DraygonArmsFacingRightIdleFourth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_45</c>.</summary>
    DraygonArmsFacingRightIdleFifth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_46</c>.</summary>
    DraygonArmsFacingRightIdleSixth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_64</c>.</summary>
    DraygonArmsFacingRightNearSwoopApexFirst,
    /// <summary>The frame the installed artwork names <c>draygon_frame_65</c>.</summary>
    DraygonArmsFacingRightNearSwoopApexSecond,
    /// <summary>The frame the installed artwork names <c>draygon_frame_66</c>.</summary>
    DraygonArmsFacingRightNearSwoopApexThird,
    /// <summary>The frame the installed artwork names <c>draygon_frame_67</c>.</summary>
    DraygonArmsFacingRightNearSwoopApexFourth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_68</c>.</summary>
    DraygonTailFacingRightIdleFirst,
    /// <summary>The frame the installed artwork names <c>draygon_frame_69</c>.</summary>
    DraygonTailFacingRightIdleSecond,
    /// <summary>The frame the installed artwork names <c>draygon_frame_70</c>.</summary>
    DraygonTailFacingRightIdleThird,
    /// <summary>The frame the installed artwork names <c>draygon_frame_71</c>.</summary>
    DraygonTailFacingRightIdleFourth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_72</c>.</summary>
    DraygonTailFacingRightIdleFifth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_73</c>.</summary>
    DraygonTailFacingRightIdleSixth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_74</c>.</summary>
    DraygonTailFacingRightIdleSeventh,
    /// <summary>The frame the installed artwork names <c>draygon_frame_75</c>.</summary>
    DraygonTailFacingRightFinalWhipsLoopSecond,
    /// <summary>The frame the installed artwork names <c>draygon_frame_76</c>.</summary>
    DraygonTailFacingRightFinalWhipsLoopThird,
    /// <summary>The frame the installed artwork names <c>draygon_frame_77</c>.</summary>
    DraygonTailFacingRightFinalWhipsLoopFourth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_78</c>.</summary>
    DraygonTailFacingRightFinalWhipsLoopFifth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_79</c>.</summary>
    DraygonTailFacingRightFinalWhipsLoopSixth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_80</c>.</summary>
    DraygonTailFacingRightFinalWhipsLoopSeventh,
    /// <summary>The frame the installed artwork names <c>draygon_frame_81</c>.</summary>
    DraygonTailFacingRightFinalWhipsLoopEighth,
    /// <summary>The frame the installed artwork names <c>spore_spawn_dead_0</c>.</summary>
    SporeSpawnInitialDead,
    /// <summary>The frame the installed artwork names <c>spore_spawn_closed_0</c>.</summary>
    SporeSpawnInitialAlive,
    /// <summary>The frame the installed artwork names <c>spore_spawn_opening_0</c>.</summary>
    SporeSpawnOpeningFirst,
    /// <summary>The frame the installed artwork names <c>spore_spawn_opening_1</c>.</summary>
    SporeSpawnOpeningSecond,
    /// <summary>The frame the installed artwork names <c>spore_spawn_opening_2</c>.</summary>
    SporeSpawnOpeningThird,
    /// <summary>The frame the installed artwork names <c>spore_spawn_opening_3</c>.</summary>
    SporeSpawnOpeningFourth,
    /// <summary>The frame the installed artwork names <c>spore_spawn_opening_4</c>.</summary>
    SporeSpawnOpeningFifth,
    /// <summary>The frame the installed artwork names <c>spore_spawn_opening_5</c>.</summary>
    SporeSpawnDeath,
    /// <summary>The frame the installed artwork names <c>spore_spawn_opening_6</c>.</summary>
    SporeSpawnOpeningSeventh,
    /// <summary>The frame the installed artwork names <c>spore_spawn_open_0</c>.</summary>
    SporeSpawnOpenPulseFirst,
    /// <summary>The frame the installed artwork names <c>spore_spawn_open_1</c>.</summary>
    SporeSpawnOpenPulseSecond,
    /// <summary>The frame the installed artwork names <c>spore_spawn_open_2</c>.</summary>
    SporeSpawnOpenPulseThird,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_0</c>.</summary>
    CeresSteamUp,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_1</c>.</summary>
    CeresSteamUpLoopTwoSecond,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_2</c>.</summary>
    CeresSteamUpLoopTwoThird,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_3</c>.</summary>
    CeresSteamUpLoopTwoFourth,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_4</c>.</summary>
    CeresSteamUpLoopTwoFifth,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_5</c>.</summary>
    CeresSteamUpLoopTwoSixth,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_6</c>.</summary>
    CeresSteamUpLoopTwoSeventh,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_7</c>.</summary>
    CeresSteamLeft,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_8</c>.</summary>
    CeresSteamLeftLoopTwoSecond,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_9</c>.</summary>
    CeresSteamLeftLoopTwoThird,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_10</c>.</summary>
    CeresSteamLeftLoopTwoFourth,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_11</c>.</summary>
    CeresSteamLeftLoopTwoFifth,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_12</c>.</summary>
    CeresSteamLeftLoopTwoSixth,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_13</c>.</summary>
    CeresSteamLeftLoopTwoSeventh,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_14</c>.</summary>
    CeresSteamDown,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_15</c>.</summary>
    CeresSteamDownLoopTwoSecond,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_16</c>.</summary>
    CeresSteamDownLoopTwoThird,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_17</c>.</summary>
    CeresSteamDownLoopTwoFourth,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_18</c>.</summary>
    CeresSteamDownLoopTwoFifth,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_19</c>.</summary>
    CeresSteamDownLoopTwoSixth,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_20</c>.</summary>
    CeresSteamDownLoopTwoSeventh,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_21</c>.</summary>
    CeresSteamRight,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_22</c>.</summary>
    CeresSteamRightLoopTwoSecond,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_23</c>.</summary>
    CeresSteamRightLoopTwoThird,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_24</c>.</summary>
    CeresSteamRightLoopTwoFourth,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_25</c>.</summary>
    CeresSteamRightLoopTwoFifth,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_26</c>.</summary>
    CeresSteamRightLoopTwoSixth,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_27</c>.</summary>
    CeresSteamRightLoopTwoSeventh,
    /// <summary>The frame the installed artwork names <c>oum_pose_0</c>.</summary>
    OumFacingLeftIdle,
    /// <summary>The frame the installed artwork names <c>oum_pose_1</c>.</summary>
    OumFacingLeftAttackingSecond,
    /// <summary>The frame the installed artwork names <c>oum_pose_2</c>.</summary>
    OumFacingLeftAttackingThird,
    /// <summary>The frame the installed artwork names <c>oum_pose_3</c>.</summary>
    OumFacingLeftAttackingFourth,
    /// <summary>The frame the installed artwork names <c>oum_pose_4</c>.</summary>
    OumFacingLeftAttackingFifth,
    /// <summary>The frame the installed artwork names <c>oum_pose_5</c>.</summary>
    OumFacingLeftAttackingSixth,
    /// <summary>The frame the installed artwork names <c>oum_pose_6</c>.</summary>
    OumFacingLeftAttackingSeventh,
    /// <summary>The frame the installed artwork names <c>oum_pose_7</c>.</summary>
    OumFacingLeftAttackingEighth,
    /// <summary>The frame the installed artwork names <c>oum_pose_8</c>.</summary>
    OumFacingLeftRollingForwardsSecond,
    /// <summary>The frame the installed artwork names <c>oum_pose_9</c>.</summary>
    OumFacingLeftRollingForwardsThird,
    /// <summary>The frame the installed artwork names <c>oum_pose_10</c>.</summary>
    OumFacingLeftRollingForwardsFourth,
    /// <summary>The frame the installed artwork names <c>oum_pose_11</c>.</summary>
    OumFacingLeftRollingForwardsFifth,
    /// <summary>The frame the installed artwork names <c>oum_pose_12</c>.</summary>
    OumFacingLeftRollingForwardsSixth,
    /// <summary>The frame the installed artwork names <c>oum_pose_13</c>.</summary>
    OumFacingLeftRollingForwardsSeventh,
    /// <summary>The frame the installed artwork names <c>oum_pose_14</c>.</summary>
    OumFacingLeftRollingForwardsEighth,
    /// <summary>The frame the installed artwork names <c>oum_pose_15</c>.</summary>
    OumFacingRightIdle,
    /// <summary>The frame the installed artwork names <c>oum_pose_16</c>.</summary>
    OumFacingRightAttackingSecond,
    /// <summary>The frame the installed artwork names <c>oum_pose_17</c>.</summary>
    OumFacingRightAttackingThird,
    /// <summary>The frame the installed artwork names <c>oum_pose_18</c>.</summary>
    OumFacingRightAttackingFourth,
    /// <summary>The frame the installed artwork names <c>oum_pose_19</c>.</summary>
    OumFacingRightAttackingFifth,
    /// <summary>The frame the installed artwork names <c>oum_pose_20</c>.</summary>
    OumFacingRightAttackingSixth,
    /// <summary>The frame the installed artwork names <c>oum_pose_21</c>.</summary>
    OumFacingRightAttackingSeventh,
    /// <summary>The frame the installed artwork names <c>oum_pose_22</c>.</summary>
    OumFacingRightAttackingEighth,
    /// <summary>The frame the installed artwork names <c>oum_pose_23</c>.</summary>
    OumFacingRightRollingForwardsSecond,
    /// <summary>The frame the installed artwork names <c>oum_pose_24</c>.</summary>
    OumFacingRightRollingForwardsThird,
    /// <summary>The frame the installed artwork names <c>oum_pose_25</c>.</summary>
    OumFacingRightRollingForwardsFourth,
    /// <summary>The frame the installed artwork names <c>oum_pose_26</c>.</summary>
    OumFacingRightRollingForwardsFifth,
    /// <summary>The frame the installed artwork names <c>oum_pose_27</c>.</summary>
    OumFacingRightRollingForwardsSixth,
    /// <summary>The frame the installed artwork names <c>oum_pose_28</c>.</summary>
    OumFacingRightRollingForwardsSeventh,
    /// <summary>The frame the installed artwork names <c>oum_pose_29</c>.</summary>
    OumFacingRightRollingForwardsEighth,
    /// <summary>The frame the installed artwork names <c>crocomire_tongue_fight_0</c>.</summary>
    CrocomireTongueFightFirst,
    /// <summary>The frame the installed artwork names <c>crocomire_tongue_fight_1</c>.</summary>
    CrocomireTongueFightSecond,
    /// <summary>The frame the installed artwork names <c>crocomire_tongue_fight_2</c>.</summary>
    CrocomireTongueFightThird,
    /// <summary>The frame the installed artwork names <c>crocomire_tongue_fight_3</c>.</summary>
    CrocomireTongueFightFourth,
    /// <summary>The frame the installed artwork names <c>crocomire_tongue_melting_pose_0</c>.</summary>
    CrocomireTongueMeltingPoseFirst,
    /// <summary>The frame the installed artwork names <c>crocomire_tongue_melting_pose_1</c>.</summary>
    CrocomireTongueMeltingPoseSecond,
    /// <summary>The frame the installed artwork names <c>crocomire_tongue_melting_pose_2</c>.</summary>
    CrocomireTongueMeltingPoseThird,
    /// <summary>The frame the installed artwork names <c>crocomire_tongue_melting_pose_3</c>.</summary>
    CrocomireTongueMeltingPoseFourth,
    /// <summary>The frame the installed artwork names <c>crocomire_tongue_melting_pose_4</c>.</summary>
    CrocomireTongueMeltingPoseFifth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_0</c>.</summary>
    CrocomireBodyPowerBombReactionMouthNotOpenLoopFirst,
    /// <summary>The frame the installed artwork names <c>crocomire_body_1</c>.</summary>
    CrocomireBodyPowerBombReactionMouthNotOpenLoopSecond,
    /// <summary>The frame the installed artwork names <c>crocomire_body_2</c>.</summary>
    CrocomireBodyPowerBombReactionMouthNotOpenLoopThird,
    /// <summary>The frame the installed artwork names <c>crocomire_body_3</c>.</summary>
    CrocomireBodyPowerBombReactionMouthNotOpenLoopFourth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_4</c>.</summary>
    CrocomireBodyPowerBombReactionMouthNotOpenLoopFifth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_5</c>.</summary>
    CrocomireBodyPowerBombReactionMouthNotOpenLoopSixth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_6</c>.</summary>
    CrocomireBodyPowerBombReactionMouthNotOpenLoopSeventh,
    /// <summary>The frame the installed artwork names <c>crocomire_body_7</c>.</summary>
    CrocomireBodySteppingBackFifth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_8</c>.</summary>
    CrocomireBodySteppingBackFourth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_9</c>.</summary>
    CrocomireBodySteppingBackThird,
    /// <summary>The frame the installed artwork names <c>crocomire_body_10</c>.</summary>
    CrocomireBodySteppingBackSecond,
    /// <summary>The frame the installed artwork names <c>crocomire_body_11</c>.</summary>
    CrocomireBodySteppingBackFirst,
    /// <summary>The frame the installed artwork names <c>crocomire_body_12</c>.</summary>
    CrocomireBodyInitial,
    /// <summary>The frame the installed artwork names <c>crocomire_body_13</c>.</summary>
    CrocomireBodyWaitForFirstSecondDamageSixth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_14</c>.</summary>
    CrocomireBodyWaitForFirstSecondDamageFifth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_15</c>.</summary>
    CrocomireBodyWaitForFirstSecondDamageFourth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_16</c>.</summary>
    CrocomireBodyWaitForFirstSecondDamageThird,
    /// <summary>The frame the installed artwork names <c>crocomire_body_17</c>.</summary>
    CrocomireBodyWaitForFirstSecondDamageSecond,
    /// <summary>The frame the installed artwork names <c>crocomire_body_18</c>.</summary>
    CrocomireBodyWaitForFirstSecondDamageFirst,
    /// <summary>The frame the installed artwork names <c>crocomire_body_19</c>.</summary>
    CrocomireBodyProjectileAttackFirst,
    /// <summary>The frame the installed artwork names <c>crocomire_body_20</c>.</summary>
    CrocomireBodyProjectileAttackSecond,
    /// <summary>The frame the installed artwork names <c>crocomire_body_21</c>.</summary>
    CrocomireBodyProjectileAttackThird,
    /// <summary>The frame the installed artwork names <c>crocomire_body_22</c>.</summary>
    CrocomireBodyProjectileAttackFourth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_23</c>.</summary>
    CrocomireBodyProjectileAttackFifth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_24</c>.</summary>
    CrocomireBodyProjectileAttackSixth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_25</c>.</summary>
    CrocomireBodyProjectileAttackSeventh,
    /// <summary>The frame the installed artwork names <c>crocomire_body_26</c>.</summary>
    CrocomireBodyProjectileAttackEighth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_27</c>.</summary>
    CrocomireBodyStepForwardAfterDelay,
    /// <summary>The frame the installed artwork names <c>crocomire_body_28</c>.</summary>
    CrocomireBodyStepForwardFourth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_29</c>.</summary>
    CrocomireBodyStepForwardFifth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_30</c>.</summary>
    CrocomireBodyStepForwardSixth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_31</c>.</summary>
    CrocomireBodyStepForwardSeventh,
    /// <summary>The frame the installed artwork names <c>crocomire_body_32</c>.</summary>
    CrocomireBodyStepForwardEighth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_33</c>.</summary>
    CrocomireBodyStepForwardNinth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_34</c>.</summary>
    CrocomireBodyStepForwardTenth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_35</c>.</summary>
    CrocomireBodyStepForwardEleventh,
    /// <summary>The frame the installed artwork names <c>crocomire_body_36</c>.</summary>
    CrocomireBodyStepForwardTwelfth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_37</c>.</summary>
    CrocomireBodyProjectileAttackLoopFirst,
    /// <summary>The frame the installed artwork names <c>crocomire_body_38</c>.</summary>
    CrocomireBodyProjectileAttackLoopSecond,
    /// <summary>The frame the installed artwork names <c>crocomire_body_39</c>.</summary>
    CrocomireBodyProjectileAttackLoopThird,
    /// <summary>The frame the installed artwork names <c>crocomire_body_40</c>.</summary>
    CrocomireBodyProjectileAttackLoopFourth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_41</c>.</summary>
    CrocomireBodyProjectileAttackLoopFifth,
    /// <summary>The frame the installed artwork names <c>crocomire_body_42</c>.</summary>
    CrocomireBodyMeltingOneTopRow,
    /// <summary>The frame the installed artwork names <c>crocomire_body_43</c>.</summary>
    CrocomireBodyMeltingOneTopTwoRows,
    /// <summary>The frame the installed artwork names <c>crocomire_body_44</c>.</summary>
    CrocomireBodyMeltingOneTopThreeRows,
    /// <summary>The frame the installed artwork names <c>crocomire_body_45</c>.</summary>
    CrocomireBodyMeltingOneTopFourRows,
    /// <summary>The frame the installed artwork names <c>crocomire_body_46</c>.</summary>
    CrocomireBodyMeltingTwoTopRow,
    /// <summary>The frame the installed artwork names <c>crocomire_body_47</c>.</summary>
    CrocomireBodyMeltingTwoTopTwoRows,
    /// <summary>The frame the installed artwork names <c>crocomire_body_48</c>.</summary>
    CrocomireBodyMeltingTwoTopThreeRows,
    /// <summary>The frame the installed artwork names <c>crocomire_body_49</c>.</summary>
    CrocomireBodyMeltingTwoTopFourRows,
    /// <summary>The frame the installed artwork names <c>bomb_torizo_dormant</c>.</summary>
    BombTorizoDormant,
    /// <summary>The frame the installed artwork names <c>golden_torizo_initial</c>.</summary>
    GoldenTorizoInitial,
    /// <summary>The frame the installed artwork names <c>kraid_arm_0</c>.</summary>
    KraidArmNormalFirst,
    /// <summary>The frame the installed artwork names <c>kraid_arm_1</c>.</summary>
    KraidArmNormalSecond,
    /// <summary>The frame the installed artwork names <c>kraid_arm_2</c>.</summary>
    KraidArmNormalThird,
    /// <summary>The frame the installed artwork names <c>kraid_arm_3</c>.</summary>
    KraidArmNormalFourth,
    /// <summary>The frame the installed artwork names <c>kraid_arm_4</c>.</summary>
    KraidArmNormalFifth,
    /// <summary>The frame the installed artwork names <c>kraid_arm_5</c>.</summary>
    KraidArmNormalSixth,
    /// <summary>The frame the installed artwork names <c>kraid_arm_6</c>.</summary>
    KraidArmNormalSeventh,
    /// <summary>The frame the installed artwork names <c>kraid_arm_7</c>.</summary>
    KraidArmNormalEighth,
    /// <summary>The frame the installed artwork names <c>kraid_arm_8</c>.</summary>
    KraidArmNormalNinth,
    /// <summary>The frame the installed artwork names <c>kraid_arm_9</c>.</summary>
    KraidArmNormalTenth,
    /// <summary>The frame the installed artwork names <c>kraid_arm_10</c>.</summary>
    KraidArmRisingOrSinkingFirst,
    /// <summary>The frame the installed artwork names <c>kraid_arm_11</c>.</summary>
    KraidArmRisingOrSinkingSecond,
    /// <summary>The frame the installed artwork names <c>kraid_arm_12</c>.</summary>
    KraidArmRisingOrSinkingThird,
    /// <summary>The frame the installed artwork names <c>kraid_arm_13</c>.</summary>
    KraidArmRisingOrSinkingFourth,
    /// <summary>The frame the installed artwork names <c>kraid_arm_14</c>.</summary>
    KraidArmRisingOrSinkingFifth,
    /// <summary>The frame the installed artwork names <c>kraid_arm_15</c>.</summary>
    KraidArmRisingOrSinkingSixth,
    /// <summary>The frame the installed artwork names <c>kraid_arm_16</c>.</summary>
    KraidArmRisingOrSinkingSeventh,
    /// <summary>The frame the installed artwork names <c>kraid_arm_17</c>.</summary>
    KraidArmRisingOrSinkingEighth,
    /// <summary>The frame the installed artwork names <c>kraid_arm_18</c>.</summary>
    KraidArmRisingOrSinkingNinth,
    /// <summary>The frame the installed artwork names <c>kraid_arm_19</c>.</summary>
    KraidArmRisingOrSinkingTenth,
    /// <summary>The frame the installed artwork names <c>kraid_arm_20</c>.</summary>
    KraidArmDyingOrPreparingToLungeSecond,
    /// <summary>The frame the installed artwork names <c>kraid_arm_21</c>.</summary>
    KraidArmDyingOrPreparingToLungeThird,
    /// <summary>The frame the installed artwork names <c>golden_torizo_awake_0</c>.</summary>
    BombTorizoInitialFourth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_awake_1</c>.</summary>
    BombTorizoInitialThird,
    /// <summary>The frame the installed artwork names <c>golden_torizo_awake_2</c>.</summary>
    BombTorizoInitialResumed,
    /// <summary>The frame the installed artwork names <c>golden_torizo_awake_3</c>.</summary>
    BombTorizoInitialSixth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_awake_4</c>.</summary>
    BombTorizoInitialSeventh,
    /// <summary>The frame the installed artwork names <c>golden_torizo_awake_5</c>.</summary>
    BombTorizoInitialEighth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_walk_left_0</c>.</summary>
    TorizoFacingLeftWalkingRightLegMovingFirst,
    /// <summary>The frame the installed artwork names <c>golden_torizo_walk_left_1</c>.</summary>
    TorizoFacingLeftWalkingRightLegMovingSecond,
    /// <summary>The frame the installed artwork names <c>golden_torizo_walk_left_2</c>.</summary>
    TorizoFacingLeftWalkingRightLegMovingThird,
    /// <summary>The frame the installed artwork names <c>golden_torizo_walk_left_3</c>.</summary>
    TorizoFacingLeftWalkingRightLegMovingFourth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_walk_left_4</c>.</summary>
    TorizoFacingLeftWalkingRightLegMovingFifth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_walk_left_5</c>.</summary>
    TorizoFacingLeftWalkingLeftLegMovingFirst,
    /// <summary>The frame the installed artwork names <c>golden_torizo_walk_left_6</c>.</summary>
    TorizoFacingLeftWalkingLeftLegMovingSecond,
    /// <summary>The frame the installed artwork names <c>golden_torizo_walk_left_7</c>.</summary>
    TorizoFacingLeftWalkingLeftLegMovingThird,
    /// <summary>The frame the installed artwork names <c>golden_torizo_walk_left_8</c>.</summary>
    TorizoFacingLeftWalkingLeftLegMovingFourth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_walk_left_9</c>.</summary>
    TorizoFacingLeftWalkingLeftLegMovingFifth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_0</c>.</summary>
    TorizoFacingLeftTurningLeft,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_1</c>.</summary>
    TorizoFacingRightWalkingLeftLegMovingFirst,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_2</c>.</summary>
    TorizoFacingRightWalkingLeftLegMovingSecond,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_3</c>.</summary>
    TorizoFacingRightWalkingLeftLegMovingThird,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_4</c>.</summary>
    TorizoFacingRightWalkingLeftLegMovingFourth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_5</c>.</summary>
    TorizoFacingRightWalkingLeftLegMovingFifth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_6</c>.</summary>
    TorizoFacingRightWalkingRightLegMovingFirst,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_7</c>.</summary>
    TorizoFacingRightWalkingRightLegMovingSecond,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_8</c>.</summary>
    TorizoFacingRightWalkingRightLegMovingThird,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_9</c>.</summary>
    TorizoFacingRightWalkingRightLegMovingFourth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_10</c>.</summary>
    TorizoFacingRightWalkingRightLegMovingFifth,
    /// <summary>The frame the installed artwork names <c>torizo_jump_back_0</c>.</summary>
    TorizoFacingRightJumpingForwardsFirst,
    /// <summary>The frame the installed artwork names <c>torizo_jump_back_1</c>.</summary>
    TorizoFacingRightJumpingForwardsSecond,
    /// <summary>The frame the installed artwork names <c>torizo_jump_back_2</c>.</summary>
    TorizoFacingRightJumpingForwardsThird,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_orb_0</c>.</summary>
    TorizoFacingRightSpewingChozoOrbsRightFootFwdFirst,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_orb_1</c>.</summary>
    TorizoFacingRightSpewingChozoOrbsRightFootFwdSecond,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_orb_2</c>.</summary>
    TorizoFacingRightSpewingChozoOrbsRightFootFwdThird,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_orb_3</c>.</summary>
    TorizoFacingRightSpewingChozoOrbsRightFootFwdFourth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_orb_4</c>.</summary>
    TorizoFacingRightSpewingChozoOrbsRightFootFwdFifth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_orb_5</c>.</summary>
    TorizoFacingRightSpewingChozoOrbsRightFootFwdSixth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_0</c>.</summary>
    TorizoFacingRightSpewingChozoOrbsLeftFootFwdFirst,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_1</c>.</summary>
    TorizoFacingRightSonicBoomsLeftFootForwardOneSecond,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_2</c>.</summary>
    TorizoFacingRightSonicBoomsLeftFootForwardOneThird,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_3</c>.</summary>
    TorizoFacingRightSonicBoomsLeftFootForwardOneFourth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_4</c>.</summary>
    TorizoFacingRightSonicBoomsLeftFootForwardOneFifth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_5</c>.</summary>
    TorizoFacingRightSonicBoomsLeftFootForwardOneSixth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_6</c>.</summary>
    TorizoFacingRightSonicBoomsLeftFootForwardOneSeventh,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_7</c>.</summary>
    TorizoFacingRightSonicBoomsLeftFootForwardOneEighth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_8</c>.</summary>
    TorizoFacingRightSonicBoomsLeftFootForwardOneNinth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_9</c>.</summary>
    TorizoFacingRightSonicBoomsLeftFootForwardOneTenth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_10</c>.</summary>
    TorizoFacingRightSonicBoomsLeftFootForwardOneEleventh,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_11</c>.</summary>
    TorizoFacingRightSonicBoomsRightFootForwardOneSecond,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_12</c>.</summary>
    TorizoFacingRightSonicBoomsRightFootForwardOneThird,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_13</c>.</summary>
    TorizoFacingRightSonicBoomsRightFootForwardOneFourth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_14</c>.</summary>
    TorizoFacingRightSonicBoomsRightFootForwardOneFifth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_15</c>.</summary>
    TorizoFacingRightSonicBoomsRightFootForwardOneSixth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_16</c>.</summary>
    TorizoFacingRightSonicBoomsRightFootForwardOneSeventh,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_17</c>.</summary>
    TorizoFacingRightSonicBoomsRightFootForwardOneEighth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_18</c>.</summary>
    TorizoFacingRightSonicBoomsRightFootForwardOneNinth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_19</c>.</summary>
    TorizoFacingRightSonicBoomsRightFootForwardOneTenth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_20</c>.</summary>
    TorizoFacingRightSonicBoomsRightFootForwardOneEleventh,
    /// <summary>The frame the installed artwork names <c>torizo_falling_left</c>.</summary>
    TorizoFallingLeft,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_foot_orb_0</c>.</summary>
    TorizoFacingRightSpewingChozoOrbsLeftFootFwdSecond,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_foot_orb_1</c>.</summary>
    TorizoFacingRightSpewingChozoOrbsLeftFootFwdThird,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_foot_orb_2</c>.</summary>
    TorizoFacingRightSpewingChozoOrbsLeftFootFwdFourth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_foot_orb_3</c>.</summary>
    TorizoFacingRightSpewingChozoOrbsLeftFootFwdFifth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_foot_orb_4</c>.</summary>
    TorizoFacingRightSpewingChozoOrbsLeftFootFwdSixth,
    /// <summary>The frame the installed artwork names <c>torizo_jump_back_left_0</c>.</summary>
    TorizoFacingLeftJumpingForwardsFirst,
    /// <summary>The frame the installed artwork names <c>torizo_jump_back_left_1</c>.</summary>
    TorizoFacingLeftJumpingForwardsThird,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_0</c>.</summary>
    TorizoFacingLeftSpewingChozoOrbsRightFootFwdFirst,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_1</c>.</summary>
    TorizoFacingLeftSpewingChozoOrbsRightFootFwdSecond,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_2</c>.</summary>
    TorizoFacingLeftSpewingChozoOrbsRightFootFwdThird,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_3</c>.</summary>
    TorizoFacingLeftSpewingChozoOrbsRightFootFwdFourth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_4</c>.</summary>
    TorizoFacingLeftSpewingChozoOrbsRightFootFwdFifth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_5</c>.</summary>
    TorizoFacingLeftSpewingChozoOrbsRightFootFwdSixth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_6</c>.</summary>
    TorizoFacingLeftSpewingChozoOrbsLeftFootFwdFirst,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_7</c>.</summary>
    TorizoFacingLeftSpewingChozoOrbsLeftFootFwdSecond,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_8</c>.</summary>
    TorizoFacingLeftSpewingChozoOrbsLeftFootFwdThird,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_9</c>.</summary>
    TorizoFacingLeftSpewingChozoOrbsLeftFootFwdFourth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_10</c>.</summary>
    TorizoFacingLeftSpewingChozoOrbsLeftFootFwdFifth,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_11</c>.</summary>
    TorizoFacingLeftSpewingChozoOrbsLeftFootFwdSixth,
    /// <summary>The frame the installed artwork names <c>torizo_combat_0</c>.</summary>
    TorizoFacingLeftSonicBoomsRightFootForwardOneSecond,
    /// <summary>The frame the installed artwork names <c>torizo_combat_1</c>.</summary>
    TorizoFacingLeftSonicBoomsRightFootForwardOneThird,
    /// <summary>The frame the installed artwork names <c>torizo_combat_2</c>.</summary>
    TorizoFacingLeftSonicBoomsRightFootForwardOneFourth,
    /// <summary>The frame the installed artwork names <c>torizo_combat_3</c>.</summary>
    TorizoFacingLeftSonicBoomsRightFootForwardOneFifth,
    /// <summary>The frame the installed artwork names <c>torizo_combat_4</c>.</summary>
    TorizoFacingLeftSonicBoomsRightFootForwardOneSixth,
    /// <summary>The frame the installed artwork names <c>torizo_combat_5</c>.</summary>
    TorizoFacingLeftSonicBoomsRightFootForwardOneSeventh,
    /// <summary>The frame the installed artwork names <c>torizo_combat_6</c>.</summary>
    TorizoFacingLeftSonicBoomsRightFootForwardOneEighth,
    /// <summary>The frame the installed artwork names <c>torizo_combat_7</c>.</summary>
    TorizoFacingLeftSonicBoomsRightFootForwardOneNinth,
    /// <summary>The frame the installed artwork names <c>torizo_combat_8</c>.</summary>
    TorizoFacingLeftSonicBoomsRightFootForwardOneTenth,
    /// <summary>The frame the installed artwork names <c>torizo_combat_9</c>.</summary>
    TorizoFacingLeftSonicBoomsRightFootForwardOneEleventh,
    /// <summary>The frame the installed artwork names <c>torizo_combat_10</c>.</summary>
    TorizoFacingLeftSonicBoomsLeftFootForwardOneSecond,
    /// <summary>The frame the installed artwork names <c>torizo_combat_11</c>.</summary>
    TorizoFacingLeftSonicBoomsLeftFootForwardOneThird,
    /// <summary>The frame the installed artwork names <c>torizo_combat_12</c>.</summary>
    TorizoFacingLeftSonicBoomsLeftFootForwardOneFourth,
    /// <summary>The frame the installed artwork names <c>torizo_combat_13</c>.</summary>
    TorizoFacingLeftSonicBoomsLeftFootForwardOneFifth,
    /// <summary>The frame the installed artwork names <c>torizo_combat_14</c>.</summary>
    TorizoFacingLeftSonicBoomsLeftFootForwardOneSixth,
    /// <summary>The frame the installed artwork names <c>torizo_combat_15</c>.</summary>
    TorizoFacingLeftSonicBoomsLeftFootForwardOneSeventh,
    /// <summary>The frame the installed artwork names <c>torizo_combat_16</c>.</summary>
    TorizoFacingLeftSonicBoomsLeftFootForwardOneEighth,
    /// <summary>The frame the installed artwork names <c>torizo_combat_17</c>.</summary>
    TorizoFacingLeftSonicBoomsLeftFootForwardOneNinth,
    /// <summary>The frame the installed artwork names <c>torizo_combat_18</c>.</summary>
    TorizoFacingLeftSonicBoomsLeftFootForwardOneTenth,
    /// <summary>The frame the installed artwork names <c>torizo_combat_19</c>.</summary>
    TorizoFacingLeftSonicBoomsLeftFootForwardOneEleventh,
    /// <summary>The frame the installed artwork names <c>torizo_combat_20</c>.</summary>
    TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedFirst,
    /// <summary>The frame the installed artwork names <c>torizo_combat_21</c>.</summary>
    TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedSecond,
    /// <summary>The frame the installed artwork names <c>torizo_combat_22</c>.</summary>
    TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedThird,
    /// <summary>The frame the installed artwork names <c>torizo_combat_23</c>.</summary>
    TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedFourth,
    /// <summary>The frame the installed artwork names <c>torizo_combat_24</c>.</summary>
    TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedFifth,
    /// <summary>The frame the installed artwork names <c>torizo_combat_25</c>.</summary>
    TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedSixth,
    /// <summary>The frame the installed artwork names <c>torizo_combat_26</c>.</summary>
    TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedSeventh,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_standing</c>.</summary>
    MotherBrainBodyStanding,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_walking_0</c>.</summary>
    MotherBrainBodyWalkingFirst,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_walking_1</c>.</summary>
    MotherBrainBodyWalkingSecond,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_walking_2</c>.</summary>
    MotherBrainBodyWalkingThird,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_walking_3</c>.</summary>
    MotherBrainBodyWalkingFourth,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_walking_4</c>.</summary>
    MotherBrainBodyWalkingFifth,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_walking_5</c>.</summary>
    MotherBrainBodyWalkingSixth,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_walking_6</c>.</summary>
    MotherBrainBodyWalkingSeventh,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_walking_7</c>.</summary>
    MotherBrainBodyWalkingEighth,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_crouched</c>.</summary>
    MotherBrainBodyCrouched,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_uncrouching</c>.</summary>
    MotherBrainBodyUncrouching,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_leaning_down</c>.</summary>
    MotherBrainBodyLeaningDown,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_initial_dummy</c>.</summary>
    MotherBrainBodyInitialDummy,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_death_beam_0</c>.</summary>
    MotherBrainBodyHandBeamSecond,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_death_beam_1</c>.</summary>
    MotherBrainBodyHandBeamThird,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_death_beam_2</c>.</summary>
    MotherBrainBodyHandBeamFourth,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_death_beam_3</c>.</summary>
    MotherBrainBodyHandBeamFifth,
    /// <summary>The frame the installed artwork names <c>phantoon_invulnerable_body_0</c>.</summary>
    PhantoonInvulnerableBody,
    /// <summary>The frame the installed artwork names <c>phantoon_full_hitbox_body_0</c>.</summary>
    PhantoonFullHitboxBody,
    /// <summary>The frame the installed artwork names <c>phantoon_eye_hitbox_body_0</c>.</summary>
    PhantoonEyeHitboxBody,
    /// <summary>The frame the installed artwork names <c>phantoon_closed_eye_0</c>.</summary>
    PhantoonEyeClosed,
    /// <summary>The frame the installed artwork names <c>phantoon_opening_eye_0</c>.</summary>
    PhantoonEyeOpenFirst,
    /// <summary>The frame the installed artwork names <c>phantoon_opening_eye_1</c>.</summary>
    PhantoonEyeOpenSecond,
    /// <summary>The frame the installed artwork names <c>phantoon_opening_eye_2</c>.</summary>
    PhantoonEyeOpenThird,
    /// <summary>The frame the installed artwork names <c>phantoon_centered_eye_0</c>.</summary>
    PhantoonEyeballCentered,
    /// <summary>The frame the installed artwork names <c>phantoon_gaze_0</c>.</summary>
    PhantoonEyeLookingUp,
    /// <summary>The frame the installed artwork names <c>phantoon_gaze_4</c>.</summary>
    PhantoonEyeLookingDown,
    /// <summary>The frame the installed artwork names <c>phantoon_gaze_6</c>.</summary>
    PhantoonEyeLookingLeft,
    /// <summary>The frame the installed artwork names <c>phantoon_gaze_2</c>.</summary>
    PhantoonEyeLookingRight,
    /// <summary>The frame the installed artwork names <c>phantoon_gaze_5</c>.</summary>
    PhantoonEyeLookingDownLeft,
    /// <summary>The frame the installed artwork names <c>phantoon_gaze_3</c>.</summary>
    PhantoonEyeLookingDownRight,
    /// <summary>The frame the installed artwork names <c>phantoon_gaze_7</c>.</summary>
    PhantoonEyeLookingUpLeft,
    /// <summary>The frame the installed artwork names <c>phantoon_gaze_1</c>.</summary>
    PhantoonEyeLookingUpRight,
    /// <summary>The frame the installed artwork names <c>phantoon_tentacle_0</c>.</summary>
    PhantoonTentaclesFirst,
    /// <summary>The frame the installed artwork names <c>phantoon_tentacle_1</c>.</summary>
    PhantoonTentaclesSecond,
    /// <summary>The frame the installed artwork names <c>phantoon_tentacle_2</c>.</summary>
    PhantoonTentaclesThird,
    /// <summary>The frame the installed artwork names <c>phantoon_mouth_0</c>.</summary>
    PhantoonMouthInitial,
    /// <summary>The frame the installed artwork names <c>phantoon_mouth_1</c>.</summary>
    PhantoonMouthSpawnFlameSecond,
    /// <summary>The frame the installed artwork names <c>phantoon_mouth_2</c>.</summary>
    PhantoonMouthSpawnFlameFirst,
    /// <summary>The frame the installed artwork names <c>draygon_frame_6</c>.</summary>
    DraygonArmsFacingLeftDyingFirst,
    /// <summary>The frame the installed artwork names <c>draygon_frame_7</c>.</summary>
    DraygonArmsFacingLeftDyingSecond,
    /// <summary>The frame the installed artwork names <c>draygon_frame_8</c>.</summary>
    DraygonArmsFacingLeftDyingThird,
    /// <summary>The frame the installed artwork names <c>draygon_frame_9</c>.</summary>
    DraygonArmsFacingLeftDyingFourth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_10</c>.</summary>
    DraygonBodyFacingLeftFireGoopFirst,
    /// <summary>The frame the installed artwork names <c>draygon_frame_11</c>.</summary>
    DraygonBodyFacingLeftFireGoopSecond,
    /// <summary>The frame the installed artwork names <c>draygon_frame_12</c>.</summary>
    DraygonBodyFacingLeftFireGoopThird,
    /// <summary>The frame the installed artwork names <c>draygon_frame_13</c>.</summary>
    DraygonBodyFacingLeftFireGoopFourth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_14</c>.</summary>
    DraygonEyeFacingLeftIdleFirst,
    /// <summary>The frame the installed artwork names <c>draygon_frame_15</c>.</summary>
    DraygonEyeFacingLeftIdleSecond,
    /// <summary>The frame the installed artwork names <c>draygon_frame_16</c>.</summary>
    DraygonEyeFacingLeftIdleThird,
    /// <summary>The frame the installed artwork names <c>draygon_frame_17</c>.</summary>
    DraygonEyeFacingLeftDyingLoopFifth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_18</c>.</summary>
    DraygonEyeFacingLeftIdleFourth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_19</c>.</summary>
    DraygonEyeFacingLeftDyingLoopThird,
    /// <summary>The frame the installed artwork names <c>draygon_frame_20</c>.</summary>
    DraygonEyeFacingLeftIdleFifth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_21</c>.</summary>
    DraygonEyeFacingLeftIdleSixth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_22</c>.</summary>
    DraygonBodyFacingLeftReset,
    /// <summary>The frame the installed artwork names <c>draygon_frame_47</c>.</summary>
    DraygonArmsFacingRightDyingFirst,
    /// <summary>The frame the installed artwork names <c>draygon_frame_48</c>.</summary>
    DraygonArmsFacingRightDyingSecond,
    /// <summary>The frame the installed artwork names <c>draygon_frame_49</c>.</summary>
    DraygonArmsFacingRightDyingThird,
    /// <summary>The frame the installed artwork names <c>draygon_frame_50</c>.</summary>
    DraygonArmsFacingRightDyingFourth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_51</c>.</summary>
    DraygonBodyFacingRightFireGoopFirst,
    /// <summary>The frame the installed artwork names <c>draygon_frame_52</c>.</summary>
    DraygonBodyFacingRightFireGoopSecond,
    /// <summary>The frame the installed artwork names <c>draygon_frame_53</c>.</summary>
    DraygonBodyFacingRightFireGoopThird,
    /// <summary>The frame the installed artwork names <c>draygon_frame_54</c>.</summary>
    DraygonBodyFacingRightFireGoopFourth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_55</c>.</summary>
    DraygonEyeFacingRightIdleFirst,
    /// <summary>The frame the installed artwork names <c>draygon_frame_56</c>.</summary>
    DraygonEyeFacingRightIdleSecond,
    /// <summary>The frame the installed artwork names <c>draygon_frame_57</c>.</summary>
    DraygonEyeFacingRightIdleThird,
    /// <summary>The frame the installed artwork names <c>draygon_frame_58</c>.</summary>
    DraygonEyeFacingRightDyingLoopFifth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_59</c>.</summary>
    DraygonEyeFacingRightIdleFourth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_60</c>.</summary>
    DraygonEyeFacingRightDyingLoopThird,
    /// <summary>The frame the installed artwork names <c>draygon_frame_61</c>.</summary>
    DraygonEyeFacingRightIdleFifth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_62</c>.</summary>
    DraygonEyeFacingRightIdleSixth,
    /// <summary>The frame the installed artwork names <c>draygon_frame_63</c>.</summary>
    DraygonBodyFacingRightReset,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_0</c>.</summary>
    CrocomireBodySkeletonFallingFirst,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_1</c>.</summary>
    CrocomireBodySkeletonFallingSecond,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_2</c>.</summary>
    CrocomireBodySkeletonFallingThird,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_3</c>.</summary>
    CrocomireBodySkeletonFallsApartFirst,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_4</c>.</summary>
    CrocomireBodySkeletonFallsApartSecond,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_5</c>.</summary>
    CrocomireBodySkeletonFallsApartThird,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_6</c>.</summary>
    CrocomireBodySkeletonFallsApartFourth,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_7</c>.</summary>
    CrocomireBodySkeletonFallsApartFifth,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_8</c>.</summary>
    CrocomireBodySkeletonFallsApartSixth,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_9</c>.</summary>
    CrocomireBodySkeletonFallsApartSeventh,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_10</c>.</summary>
    CrocomireBodySkeletonFallsApartEighth,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_11</c>.</summary>
    CrocomireBodySkeletonFallsApartNinth,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_12</c>.</summary>
    CrocomireBodySkeletonFallsApartTenth,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_13</c>.</summary>
    CrocomireBodySkeletonFallsApartEleventh,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_14</c>.</summary>
    CrocomireBodySkeletonFallsApartTwelfth,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_15</c>.</summary>
    CrocomireBodySkeletonFallsApartThirteenth,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_16</c>.</summary>
    CrocomireBodySkeletonFallsApartFourteenth,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_17</c>.</summary>
    CrocomireBodySkeletonFallsApartFifteenth,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_18</c>.</summary>
    CrocomireBodySkeletonFallsApartSixteenth,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_19</c>.</summary>
    CrocomireBodySkeletonFallsApartSeventeenth,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_20</c>.</summary>
    CrocomireBodySkeletonFallsApartEighteenth,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_21</c>.</summary>
    CrocomireBodySkeletonStable,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_22</c>.</summary>
    CrocomireBodyDead,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_23</c>.</summary>
    CrocomireBodySkeletonFlowingDownRiverFirst,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_24</c>.</summary>
    CrocomireBodySkeletonFlowingDownRiverSecond,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_25</c>.</summary>
    CrocomireBodySkeletonFlowingDownRiverThird,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_26</c>.</summary>
    CrocomireBodySkeletonFlowingDownRiverFourth,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_27</c>.</summary>
    CrocomireBodySkeletonFlowingDownRiverFifth,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_28</c>.</summary>
    CrocomireBodySkeletonFlowingDownRiverSixth,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_29</c>.</summary>
    CrocomireBodySkeletonFlowingDownRiverSeventh,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_30</c>.</summary>
    CrocomireBodySkeletonFlowingDownRiverEighth,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_31</c>.</summary>
    CrocomireBodySkeletonFlowingDownRiverNinth,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_32</c>.</summary>
    CrocomireBodySkeletonFlowingDownRiverTenth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_0</c>.</summary>
    KraidFootNeutral,
    /// <summary>The frame the installed artwork names <c>kraid_foot_1</c>.</summary>
    KraidFootWalkingForwardSecond,
    /// <summary>The frame the installed artwork names <c>kraid_foot_2</c>.</summary>
    KraidFootWalkingForwardThird,
    /// <summary>The frame the installed artwork names <c>kraid_foot_3</c>.</summary>
    KraidFootWalkingForwardFourth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_4</c>.</summary>
    KraidFootWalkingForwardFifth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_5</c>.</summary>
    KraidFootWalkingForwardSixth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_6</c>.</summary>
    KraidFootWalkingForwardSeventh,
    /// <summary>The frame the installed artwork names <c>kraid_foot_7</c>.</summary>
    KraidFootWalkingForwardEighth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_8</c>.</summary>
    KraidFootWalkingForwardNinth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_9</c>.</summary>
    KraidFootWalkingForwardTenth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_10</c>.</summary>
    KraidFootWalkingForwardEleventh,
    /// <summary>The frame the installed artwork names <c>kraid_foot_11</c>.</summary>
    KraidFootWalkingForwardTwelfth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_12</c>.</summary>
    KraidFootWalkingForwardThirteenth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_13</c>.</summary>
    KraidFootWalkingForwardFourteenth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_14</c>.</summary>
    KraidFootWalkingForwardFifteenth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_15</c>.</summary>
    KraidFootWalkingForwardSixteenth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_16</c>.</summary>
    KraidFootWalkingForwardSeventeenth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_17</c>.</summary>
    KraidFootWalkingForwardEighteenth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_18</c>.</summary>
    KraidFootWalkingForwardNineteenth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_19</c>.</summary>
    KraidFootWalkingForwardTwentieth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_20</c>.</summary>
    KraidFootWalkingBackwardThirteenth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_21</c>.</summary>
    KraidFootWalkingForwardTwentyFirst,
    /// <summary>The frame the installed artwork names <c>kraid_foot_22</c>.</summary>
    KraidFootWalkingForwardTwentySecond,
    /// <summary>The frame the installed artwork names <c>kraid_foot_23</c>.</summary>
    KraidFootWalkingForwardTwentyThird,
    /// <summary>The frame the installed artwork names <c>kraid_foot_24</c>.</summary>
    KraidFootWalkingForwardTwentyFourth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_25</c>.</summary>
    KraidFootWalkingForwardTwentyFifth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_26</c>.</summary>
    KraidFootWalkingForwardTwentySixth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_27</c>.</summary>
    KraidFootWalkingForwardTwentySeventh,
    /// <summary>The frame the installed artwork names <c>kraid_foot_28</c>.</summary>
    KraidFootWalkingForwardTwentyEighth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_29</c>.</summary>
    KraidFootWalkingForwardTwentyNinth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_30</c>.</summary>
    KraidFootWalkingForwardThirtieth,
    /// <summary>The frame the installed artwork names <c>kraid_foot_31</c>.</summary>
    KraidFootWalkingForwardThirtyFirst,
    /// <summary>The frame the installed artwork names <c>kraid_foot_32</c>.</summary>
    KraidFootWalkingBackwardSecond,
    /// <summary>The frame the installed artwork names <c>kraid_foot_33</c>.</summary>
    KraidFootWalkingBackwardFirst,
    /// <summary>The frame the installed artwork names <c>kraid_foot_standing</c>.</summary>
    KraidFootStanding,
    /// <summary>
    /// The empty frame every enemy family shares: an enemy showing it draws nothing, and one that collides by its frames'
    /// hitboxes collides at the single point at its origin.
    /// </summary>
    Empty,
}
