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
    BoyonIdle0 = 1,
    /// <summary>The frame the installed artwork names <c>boyon_idle_1</c>.</summary>
    BoyonIdle1,
    /// <summary>The frame the installed artwork names <c>boyon_idle_2</c>.</summary>
    BoyonIdle2,
    /// <summary>The frame the installed artwork names <c>boyon_bounce_0</c>.</summary>
    BoyonBounce0,
    /// <summary>The frame the installed artwork names <c>boyon_bounce_1</c>.</summary>
    BoyonBounce1,
    /// <summary>The frame the installed artwork names <c>boyon_bounce_2</c>.</summary>
    BoyonBounce2,
    /// <summary>The frame the installed artwork names <c>boyon_bounce_3</c>.</summary>
    BoyonBounce3,
    /// <summary>The frame the installed artwork names <c>cacatac_upright_idle_0</c>.</summary>
    CacatacUprightIdle0,
    /// <summary>The frame the installed artwork names <c>cacatac_upright_idle_1</c>.</summary>
    CacatacUprightIdle1,
    /// <summary>The frame the installed artwork names <c>cacatac_upright_idle_2</c>.</summary>
    CacatacUprightIdle2,
    /// <summary>The frame the installed artwork names <c>cacatac_upright_idle_3</c>.</summary>
    CacatacUprightIdle3,
    /// <summary>The frame the installed artwork names <c>cacatac_upright_idle_4</c>.</summary>
    CacatacUprightIdle4,
    /// <summary>The frame the installed artwork names <c>cacatac_upright_idle_5</c>.</summary>
    CacatacUprightIdle5,
    /// <summary>The frame the installed artwork names <c>cacatac_upright_idle_6</c>.</summary>
    CacatacUprightIdle6,
    /// <summary>The frame the installed artwork names <c>cacatac_upright_idle_7</c>.</summary>
    CacatacUprightIdle7,
    /// <summary>The frame the installed artwork names <c>cacatac_upright_attack_1</c>.</summary>
    CacatacUprightAttack1,
    /// <summary>The frame the installed artwork names <c>cacatac_upright_attack_2</c>.</summary>
    CacatacUprightAttack2,
    /// <summary>The frame the installed artwork names <c>cacatac_inverted_idle_0</c>.</summary>
    CacatacInvertedIdle0,
    /// <summary>The frame the installed artwork names <c>cacatac_inverted_idle_1</c>.</summary>
    CacatacInvertedIdle1,
    /// <summary>The frame the installed artwork names <c>cacatac_inverted_idle_2</c>.</summary>
    CacatacInvertedIdle2,
    /// <summary>The frame the installed artwork names <c>cacatac_inverted_idle_3</c>.</summary>
    CacatacInvertedIdle3,
    /// <summary>The frame the installed artwork names <c>cacatac_inverted_idle_4</c>.</summary>
    CacatacInvertedIdle4,
    /// <summary>The frame the installed artwork names <c>cacatac_inverted_idle_5</c>.</summary>
    CacatacInvertedIdle5,
    /// <summary>The frame the installed artwork names <c>cacatac_inverted_idle_6</c>.</summary>
    CacatacInvertedIdle6,
    /// <summary>The frame the installed artwork names <c>cacatac_inverted_idle_7</c>.</summary>
    CacatacInvertedIdle7,
    /// <summary>The frame the installed artwork names <c>cacatac_inverted_attack_1</c>.</summary>
    CacatacInvertedAttack1,
    /// <summary>The frame the installed artwork names <c>cacatac_inverted_attack_2</c>.</summary>
    CacatacInvertedAttack2,
    /// <summary>The frame the installed artwork names <c>boulder_roll_0</c>.</summary>
    BoulderRoll0,
    /// <summary>The frame the installed artwork names <c>boulder_roll_1</c>.</summary>
    BoulderRoll1,
    /// <summary>The frame the installed artwork names <c>boulder_roll_2</c>.</summary>
    BoulderRoll2,
    /// <summary>The frame the installed artwork names <c>boulder_roll_3</c>.</summary>
    BoulderRoll3,
    /// <summary>The frame the installed artwork names <c>boulder_roll_4</c>.</summary>
    BoulderRoll4,
    /// <summary>The frame the installed artwork names <c>boulder_roll_5</c>.</summary>
    BoulderRoll5,
    /// <summary>The frame the installed artwork names <c>boulder_roll_6</c>.</summary>
    BoulderRoll6,
    /// <summary>The frame the installed artwork names <c>boulder_roll_7</c>.</summary>
    BoulderRoll7,
    /// <summary>The frame the installed artwork names <c>atomic_up_right_0</c>.</summary>
    AtomicUpRight0,
    /// <summary>The frame the installed artwork names <c>atomic_up_right_1</c>.</summary>
    AtomicUpRight1,
    /// <summary>The frame the installed artwork names <c>atomic_up_right_2</c>.</summary>
    AtomicUpRight2,
    /// <summary>The frame the installed artwork names <c>atomic_up_right_3</c>.</summary>
    AtomicUpRight3,
    /// <summary>The frame the installed artwork names <c>atomic_up_right_4</c>.</summary>
    AtomicUpRight4,
    /// <summary>The frame the installed artwork names <c>atomic_up_right_5</c>.</summary>
    AtomicUpRight5,
    /// <summary>The frame the installed artwork names <c>atomic_up_left_0</c>.</summary>
    AtomicUpLeft0,
    /// <summary>The frame the installed artwork names <c>atomic_up_left_1</c>.</summary>
    AtomicUpLeft1,
    /// <summary>The frame the installed artwork names <c>atomic_up_left_2</c>.</summary>
    AtomicUpLeft2,
    /// <summary>The frame the installed artwork names <c>atomic_up_left_3</c>.</summary>
    AtomicUpLeft3,
    /// <summary>The frame the installed artwork names <c>atomic_up_left_4</c>.</summary>
    AtomicUpLeft4,
    /// <summary>The frame the installed artwork names <c>atomic_up_left_5</c>.</summary>
    AtomicUpLeft5,
    /// <summary>The frame the installed artwork names <c>skultera_swim_left_0</c>.</summary>
    SkulteraSwimLeft0,
    /// <summary>The frame the installed artwork names <c>skultera_swim_left_1</c>.</summary>
    SkulteraSwimLeft1,
    /// <summary>The frame the installed artwork names <c>skultera_swim_left_2</c>.</summary>
    SkulteraSwimLeft2,
    /// <summary>The frame the installed artwork names <c>skultera_turn_right_0</c>.</summary>
    SkulteraTurnRight0,
    /// <summary>The frame the installed artwork names <c>skultera_turn_right_1</c>.</summary>
    SkulteraTurnRight1,
    /// <summary>The frame the installed artwork names <c>skultera_turn_right_2</c>.</summary>
    SkulteraTurnRight2,
    /// <summary>The frame the installed artwork names <c>skultera_turn_right_3</c>.</summary>
    SkulteraTurnRight3,
    /// <summary>The frame the installed artwork names <c>skultera_turn_right_4</c>.</summary>
    SkulteraTurnRight4,
    /// <summary>The frame the installed artwork names <c>skultera_turn_right_5</c>.</summary>
    SkulteraTurnRight5,
    /// <summary>The frame the installed artwork names <c>skultera_turn_right_6</c>.</summary>
    SkulteraTurnRight6,
    /// <summary>The frame the installed artwork names <c>skultera_turn_right_7</c>.</summary>
    SkulteraTurnRight7,
    /// <summary>The frame the installed artwork names <c>skultera_swim_right_0</c>.</summary>
    SkulteraSwimRight0,
    /// <summary>The frame the installed artwork names <c>skultera_swim_right_1</c>.</summary>
    SkulteraSwimRight1,
    /// <summary>The frame the installed artwork names <c>skultera_swim_right_2</c>.</summary>
    SkulteraSwimRight2,
    /// <summary>The frame the installed artwork names <c>skultera_turn_left_0</c>.</summary>
    SkulteraTurnLeft0,
    /// <summary>The frame the installed artwork names <c>skultera_turn_left_1</c>.</summary>
    SkulteraTurnLeft1,
    /// <summary>The frame the installed artwork names <c>skultera_turn_left_2</c>.</summary>
    SkulteraTurnLeft2,
    /// <summary>The frame the installed artwork names <c>skultera_turn_left_3</c>.</summary>
    SkulteraTurnLeft3,
    /// <summary>The frame the installed artwork names <c>skultera_turn_left_4</c>.</summary>
    SkulteraTurnLeft4,
    /// <summary>The frame the installed artwork names <c>skultera_turn_left_5</c>.</summary>
    SkulteraTurnLeft5,
    /// <summary>The frame the installed artwork names <c>skultera_turn_left_6</c>.</summary>
    SkulteraTurnLeft6,
    /// <summary>The frame the installed artwork names <c>skultera_turn_left_7</c>.</summary>
    SkulteraTurnLeft7,
    /// <summary>The frame the installed artwork names <c>waver_steady_left</c>.</summary>
    WaverSteadyLeft,
    /// <summary>The frame the installed artwork names <c>waver_steady_right</c>.</summary>
    WaverSteadyRight,
    /// <summary>The frame the installed artwork names <c>waver_spin_left_0</c>.</summary>
    WaverSpinLeft0,
    /// <summary>The frame the installed artwork names <c>waver_spin_left_1</c>.</summary>
    WaverSpinLeft1,
    /// <summary>The frame the installed artwork names <c>waver_spin_left_2</c>.</summary>
    WaverSpinLeft2,
    /// <summary>The frame the installed artwork names <c>waver_spin_left_3</c>.</summary>
    WaverSpinLeft3,
    /// <summary>The frame the installed artwork names <c>waver_spin_right_0</c>.</summary>
    WaverSpinRight0,
    /// <summary>The frame the installed artwork names <c>waver_spin_right_1</c>.</summary>
    WaverSpinRight1,
    /// <summary>The frame the installed artwork names <c>waver_spin_right_2</c>.</summary>
    WaverSpinRight2,
    /// <summary>The frame the installed artwork names <c>waver_spin_right_3</c>.</summary>
    WaverSpinRight3,
    /// <summary>The frame the installed artwork names <c>metaree_idle_0</c>.</summary>
    MetareeIdle0,
    /// <summary>The frame the installed artwork names <c>metaree_idle_1</c>.</summary>
    MetareeIdle1,
    /// <summary>The frame the installed artwork names <c>metaree_idle_2</c>.</summary>
    MetareeIdle2,
    /// <summary>The frame the installed artwork names <c>metaree_idle_3</c>.</summary>
    MetareeIdle3,
    /// <summary>The frame the installed artwork names <c>metaree_prepare_1</c>.</summary>
    MetareePrepare1,
    /// <summary>The frame the installed artwork names <c>skree_idle_0</c>.</summary>
    SkreeIdle0,
    /// <summary>The frame the installed artwork names <c>skree_idle_1</c>.</summary>
    SkreeIdle1,
    /// <summary>The frame the installed artwork names <c>skree_idle_2</c>.</summary>
    SkreeIdle2,
    /// <summary>The frame the installed artwork names <c>skree_idle_3</c>.</summary>
    SkreeIdle3,
    /// <summary>The frame the installed artwork names <c>skree_prepare_1</c>.</summary>
    SkreePrepare1,
    /// <summary>The frame the installed artwork names <c>zoa_shoot_left_0</c>.</summary>
    ZoaShootLeft0,
    /// <summary>The frame the installed artwork names <c>zoa_shoot_left_1</c>.</summary>
    ZoaShootLeft1,
    /// <summary>The frame the installed artwork names <c>zoa_shoot_left_2</c>.</summary>
    ZoaShootLeft2,
    /// <summary>The frame the installed artwork names <c>zoa_rise_left_0</c>.</summary>
    ZoaRiseLeft0,
    /// <summary>The frame the installed artwork names <c>zoa_rise_left_1</c>.</summary>
    ZoaRiseLeft1,
    /// <summary>The frame the installed artwork names <c>zoa_rise_left_2</c>.</summary>
    ZoaRiseLeft2,
    /// <summary>The frame the installed artwork names <c>zoa_shoot_right_0</c>.</summary>
    ZoaShootRight0,
    /// <summary>The frame the installed artwork names <c>zoa_shoot_right_1</c>.</summary>
    ZoaShootRight1,
    /// <summary>The frame the installed artwork names <c>zoa_shoot_right_2</c>.</summary>
    ZoaShootRight2,
    /// <summary>The frame the installed artwork names <c>zoa_rise_right_0</c>.</summary>
    ZoaRiseRight0,
    /// <summary>The frame the installed artwork names <c>zoa_rise_right_1</c>.</summary>
    ZoaRiseRight1,
    /// <summary>The frame the installed artwork names <c>zoa_rise_right_2</c>.</summary>
    ZoaRiseRight2,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_normal_left_0</c>.</summary>
    PipeBrinstarNormalLeft0,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_normal_left_1</c>.</summary>
    PipeBrinstarNormalLeft1,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_normal_left_2</c>.</summary>
    PipeBrinstarNormalLeft2,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_normal_left_3</c>.</summary>
    PipeBrinstarNormalLeft3,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_normal_left_4</c>.</summary>
    PipeBrinstarNormalLeft4,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_normal_right_0</c>.</summary>
    PipeBrinstarNormalRight0,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_normal_right_1</c>.</summary>
    PipeBrinstarNormalRight1,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_normal_right_2</c>.</summary>
    PipeBrinstarNormalRight2,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_normal_right_3</c>.</summary>
    PipeBrinstarNormalRight3,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_normal_right_4</c>.</summary>
    PipeBrinstarNormalRight4,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_shoot_left_0</c>.</summary>
    PipeBrinstarStrongShootLeft0,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_shoot_left_1</c>.</summary>
    PipeBrinstarStrongShootLeft1,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_shoot_left_2</c>.</summary>
    PipeBrinstarStrongShootLeft2,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_rise_left_0</c>.</summary>
    PipeBrinstarStrongRiseLeft0,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_rise_left_1</c>.</summary>
    PipeBrinstarStrongRiseLeft1,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_rise_left_2</c>.</summary>
    PipeBrinstarStrongRiseLeft2,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_shoot_right_0</c>.</summary>
    PipeBrinstarStrongShootRight0,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_shoot_right_1</c>.</summary>
    PipeBrinstarStrongShootRight1,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_shoot_right_2</c>.</summary>
    PipeBrinstarStrongShootRight2,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_rise_right_0</c>.</summary>
    PipeBrinstarStrongRiseRight0,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_rise_right_1</c>.</summary>
    PipeBrinstarStrongRiseRight1,
    /// <summary>The frame the installed artwork names <c>pipe_brinstar_strong_rise_right_2</c>.</summary>
    PipeBrinstarStrongRiseRight2,
    /// <summary>The frame the installed artwork names <c>pipe_norfair_left_0</c>.</summary>
    PipeNorfairLeft0,
    /// <summary>The frame the installed artwork names <c>pipe_norfair_left_1</c>.</summary>
    PipeNorfairLeft1,
    /// <summary>The frame the installed artwork names <c>pipe_norfair_left_2</c>.</summary>
    PipeNorfairLeft2,
    /// <summary>The frame the installed artwork names <c>pipe_norfair_left_3</c>.</summary>
    PipeNorfairLeft3,
    /// <summary>The frame the installed artwork names <c>pipe_norfair_left_4</c>.</summary>
    PipeNorfairLeft4,
    /// <summary>The frame the installed artwork names <c>pipe_norfair_right_0</c>.</summary>
    PipeNorfairRight0,
    /// <summary>The frame the installed artwork names <c>pipe_norfair_right_1</c>.</summary>
    PipeNorfairRight1,
    /// <summary>The frame the installed artwork names <c>pipe_norfair_right_2</c>.</summary>
    PipeNorfairRight2,
    /// <summary>The frame the installed artwork names <c>pipe_norfair_right_3</c>.</summary>
    PipeNorfairRight3,
    /// <summary>The frame the installed artwork names <c>pipe_norfair_right_4</c>.</summary>
    PipeNorfairRight4,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_fly_left_0</c>.</summary>
    PipeYellowFlyLeft0,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_fly_left_1</c>.</summary>
    PipeYellowFlyLeft1,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_fly_left_2</c>.</summary>
    PipeYellowFlyLeft2,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_arc_left_0</c>.</summary>
    PipeYellowArcLeft0,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_arc_left_1</c>.</summary>
    PipeYellowArcLeft1,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_arc_left_2</c>.</summary>
    PipeYellowArcLeft2,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_fly_right_0</c>.</summary>
    PipeYellowFlyRight0,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_fly_right_1</c>.</summary>
    PipeYellowFlyRight1,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_fly_right_2</c>.</summary>
    PipeYellowFlyRight2,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_arc_right_0</c>.</summary>
    PipeYellowArcRight0,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_arc_right_1</c>.</summary>
    PipeYellowArcRight1,
    /// <summary>The frame the installed artwork names <c>pipe_yellow_arc_right_2</c>.</summary>
    PipeYellowArcRight2,
    /// <summary>The frame the installed artwork names <c>fake_kraid_walk_left_0</c>.</summary>
    FakeKraidWalkLeft0,
    /// <summary>The frame the installed artwork names <c>fake_kraid_walk_left_1</c>.</summary>
    FakeKraidWalkLeft1,
    /// <summary>The frame the installed artwork names <c>fake_kraid_walk_left_2</c>.</summary>
    FakeKraidWalkLeft2,
    /// <summary>The frame the installed artwork names <c>fake_kraid_walk_left_3</c>.</summary>
    FakeKraidWalkLeft3,
    /// <summary>The frame the installed artwork names <c>fake_kraid_spit_left_0</c>.</summary>
    FakeKraidSpitLeft0,
    /// <summary>The frame the installed artwork names <c>fake_kraid_spit_left_1</c>.</summary>
    FakeKraidSpitLeft1,
    /// <summary>The frame the installed artwork names <c>fake_kraid_spit_left_2</c>.</summary>
    FakeKraidSpitLeft2,
    /// <summary>The frame the installed artwork names <c>fake_kraid_walk_right_0</c>.</summary>
    FakeKraidWalkRight0,
    /// <summary>The frame the installed artwork names <c>fake_kraid_walk_right_1</c>.</summary>
    FakeKraidWalkRight1,
    /// <summary>The frame the installed artwork names <c>fake_kraid_walk_right_2</c>.</summary>
    FakeKraidWalkRight2,
    /// <summary>The frame the installed artwork names <c>fake_kraid_walk_right_3</c>.</summary>
    FakeKraidWalkRight3,
    /// <summary>The frame the installed artwork names <c>fake_kraid_spit_right_0</c>.</summary>
    FakeKraidSpitRight0,
    /// <summary>The frame the installed artwork names <c>fake_kraid_spit_right_1</c>.</summary>
    FakeKraidSpitRight1,
    /// <summary>The frame the installed artwork names <c>fake_kraid_spit_right_2</c>.</summary>
    FakeKraidSpitRight2,
    /// <summary>The frame the installed artwork names <c>kraid_nail_0</c>.</summary>
    KraidNail0,
    /// <summary>The frame the installed artwork names <c>kraid_nail_1</c>.</summary>
    KraidNail1,
    /// <summary>The frame the installed artwork names <c>kraid_nail_2</c>.</summary>
    KraidNail2,
    /// <summary>The frame the installed artwork names <c>kraid_nail_3</c>.</summary>
    KraidNail3,
    /// <summary>The frame the installed artwork names <c>kraid_nail_4</c>.</summary>
    KraidNail4,
    /// <summary>The frame the installed artwork names <c>kraid_nail_5</c>.</summary>
    KraidNail5,
    /// <summary>The frame the installed artwork names <c>kraid_nail_6</c>.</summary>
    KraidNail6,
    /// <summary>The frame the installed artwork names <c>kraid_nail_7</c>.</summary>
    KraidNail7,
    /// <summary>The frame the installed artwork names <c>owtch_left_0</c>.</summary>
    OwtchLeft0,
    /// <summary>The frame the installed artwork names <c>owtch_left_1</c>.</summary>
    OwtchLeft1,
    /// <summary>The frame the installed artwork names <c>owtch_left_2</c>.</summary>
    OwtchLeft2,
    /// <summary>The frame the installed artwork names <c>stoke_walk_left_0</c>.</summary>
    StokeWalkLeft0,
    /// <summary>The frame the installed artwork names <c>stoke_walk_left_1</c>.</summary>
    StokeWalkLeft1,
    /// <summary>The frame the installed artwork names <c>stoke_walk_left_2</c>.</summary>
    StokeWalkLeft2,
    /// <summary>The frame the installed artwork names <c>stoke_walk_left_3</c>.</summary>
    StokeWalkLeft3,
    /// <summary>The frame the installed artwork names <c>stoke_attack_left</c>.</summary>
    StokeAttackLeft,
    /// <summary>The frame the installed artwork names <c>stoke_walk_right_0</c>.</summary>
    StokeWalkRight0,
    /// <summary>The frame the installed artwork names <c>stoke_walk_right_1</c>.</summary>
    StokeWalkRight1,
    /// <summary>The frame the installed artwork names <c>stoke_walk_right_2</c>.</summary>
    StokeWalkRight2,
    /// <summary>The frame the installed artwork names <c>stoke_walk_right_3</c>.</summary>
    StokeWalkRight3,
    /// <summary>The frame the installed artwork names <c>stoke_attack_right</c>.</summary>
    StokeAttackRight,
    /// <summary>The frame the installed artwork names <c>ripper_shared_left_0</c>.</summary>
    RipperSharedLeft0,
    /// <summary>The frame the installed artwork names <c>ripper_shared_left_1</c>.</summary>
    RipperSharedLeft1,
    /// <summary>The frame the installed artwork names <c>ripper_shared_left_2</c>.</summary>
    RipperSharedLeft2,
    /// <summary>The frame the installed artwork names <c>ripper_shared_right_0</c>.</summary>
    RipperSharedRight0,
    /// <summary>The frame the installed artwork names <c>ripper_shared_right_1</c>.</summary>
    RipperSharedRight1,
    /// <summary>The frame the installed artwork names <c>ripper_shared_right_2</c>.</summary>
    RipperSharedRight2,
    /// <summary>The frame the installed artwork names <c>ripper_shared_frozen_left</c>.</summary>
    RipperSharedFrozenLeft,
    /// <summary>The frame the installed artwork names <c>ripper_shared_frozen_right</c>.</summary>
    RipperSharedFrozenRight,
    /// <summary>The frame the installed artwork names <c>ripper_left_0</c>.</summary>
    RipperLeft0,
    /// <summary>The frame the installed artwork names <c>ripper_left_1</c>.</summary>
    RipperLeft1,
    /// <summary>The frame the installed artwork names <c>ripper_left_2</c>.</summary>
    RipperLeft2,
    /// <summary>The frame the installed artwork names <c>ripper_right_0</c>.</summary>
    RipperRight0,
    /// <summary>The frame the installed artwork names <c>ripper_right_1</c>.</summary>
    RipperRight1,
    /// <summary>The frame the installed artwork names <c>ripper_right_2</c>.</summary>
    RipperRight2,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_0</c>.</summary>
    FirefleaCycle0,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_1</c>.</summary>
    FirefleaCycle1,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_2</c>.</summary>
    FirefleaCycle2,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_3</c>.</summary>
    FirefleaCycle3,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_4</c>.</summary>
    FirefleaCycle4,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_5</c>.</summary>
    FirefleaCycle5,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_6</c>.</summary>
    FirefleaCycle6,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_7</c>.</summary>
    FirefleaCycle7,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_8</c>.</summary>
    FirefleaCycle8,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_9</c>.</summary>
    FirefleaCycle9,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_10</c>.</summary>
    FirefleaCycle10,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_11</c>.</summary>
    FirefleaCycle11,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_12</c>.</summary>
    FirefleaCycle12,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_13</c>.</summary>
    FirefleaCycle13,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_14</c>.</summary>
    FirefleaCycle14,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_15</c>.</summary>
    FirefleaCycle15,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_16</c>.</summary>
    FirefleaCycle16,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_17</c>.</summary>
    FirefleaCycle17,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_18</c>.</summary>
    FirefleaCycle18,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_19</c>.</summary>
    FirefleaCycle19,
    /// <summary>The frame the installed artwork names <c>fireflea_cycle_20</c>.</summary>
    FirefleaCycle20,
    /// <summary>The frame the installed artwork names <c>magdollite_left_idle_0</c>.</summary>
    MagdolliteLeftIdle0,
    /// <summary>The frame the installed artwork names <c>magdollite_left_idle_1</c>.</summary>
    MagdolliteLeftIdle1,
    /// <summary>The frame the installed artwork names <c>magdollite_left_idle_2</c>.</summary>
    MagdolliteLeftIdle2,
    /// <summary>The frame the installed artwork names <c>magdollite_left_throw_0</c>.</summary>
    MagdolliteLeftThrow0,
    /// <summary>The frame the installed artwork names <c>magdollite_left_throw_1</c>.</summary>
    MagdolliteLeftThrow1,
    /// <summary>The frame the installed artwork names <c>magdollite_left_throw_2</c>.</summary>
    MagdolliteLeftThrow2,
    /// <summary>The frame the installed artwork names <c>magdollite_left_throw_3</c>.</summary>
    MagdolliteLeftThrow3,
    /// <summary>The frame the installed artwork names <c>magdollite_pillar_cap</c>.</summary>
    MagdollitePillarCap,
    /// <summary>The frame the installed artwork names <c>magdollite_left_submerge_0</c>.</summary>
    MagdolliteLeftSubmerge0,
    /// <summary>The frame the installed artwork names <c>magdollite_left_submerge_1</c>.</summary>
    MagdolliteLeftSubmerge1,
    /// <summary>The frame the installed artwork names <c>magdollite_left_submerge_2</c>.</summary>
    MagdolliteLeftSubmerge2,
    /// <summary>The frame the installed artwork names <c>magdollite_right_idle_0</c>.</summary>
    MagdolliteRightIdle0,
    /// <summary>The frame the installed artwork names <c>magdollite_right_idle_1</c>.</summary>
    MagdolliteRightIdle1,
    /// <summary>The frame the installed artwork names <c>magdollite_right_idle_2</c>.</summary>
    MagdolliteRightIdle2,
    /// <summary>The frame the installed artwork names <c>magdollite_right_throw_0</c>.</summary>
    MagdolliteRightThrow0,
    /// <summary>The frame the installed artwork names <c>magdollite_right_throw_1</c>.</summary>
    MagdolliteRightThrow1,
    /// <summary>The frame the installed artwork names <c>magdollite_right_throw_2</c>.</summary>
    MagdolliteRightThrow2,
    /// <summary>The frame the installed artwork names <c>magdollite_right_throw_3</c>.</summary>
    MagdolliteRightThrow3,
    /// <summary>The frame the installed artwork names <c>magdollite_right_submerge_0</c>.</summary>
    MagdolliteRightSubmerge0,
    /// <summary>The frame the installed artwork names <c>magdollite_right_submerge_1</c>.</summary>
    MagdolliteRightSubmerge1,
    /// <summary>The frame the installed artwork names <c>magdollite_right_submerge_2</c>.</summary>
    MagdolliteRightSubmerge2,
    /// <summary>The frame the installed artwork names <c>magdollite_pillar_phase_0</c>.</summary>
    MagdollitePillarPhase0,
    /// <summary>The frame the installed artwork names <c>magdollite_pillar_phase_1</c>.</summary>
    MagdollitePillarPhase1,
    /// <summary>The frame the installed artwork names <c>magdollite_pillar_phase_2</c>.</summary>
    MagdollitePillarPhase2,
    /// <summary>The frame the installed artwork names <c>magdollite_pillar_phase_3</c>.</summary>
    MagdollitePillarPhase3,
    /// <summary>The frame the installed artwork names <c>magdollite_pillar_phase_4</c>.</summary>
    MagdollitePillarPhase4,
    /// <summary>The frame the installed artwork names <c>magdollite_pillar_phase_5</c>.</summary>
    MagdollitePillarPhase5,
    /// <summary>The frame the installed artwork names <c>magdollite_pillar_phase_6</c>.</summary>
    MagdollitePillarPhase6,
    /// <summary>The frame the installed artwork names <c>magdollite_pillar_phase_7</c>.</summary>
    MagdollitePillarPhase7,
    /// <summary>The frame the installed artwork names <c>ceres_door_rotating_overlay</c>.</summary>
    CeresDoorRotatingOverlay,
    /// <summary>The frame the installed artwork names <c>ceres_door_left_hold</c>.</summary>
    CeresDoorLeftHold,
    /// <summary>The frame the installed artwork names <c>ceres_door_left_transition_0</c>.</summary>
    CeresDoorLeftTransition0,
    /// <summary>The frame the installed artwork names <c>ceres_door_left_transition_1</c>.</summary>
    CeresDoorLeftTransition1,
    /// <summary>The frame the installed artwork names <c>ceres_door_left_transition_2</c>.</summary>
    CeresDoorLeftTransition2,
    /// <summary>The frame the installed artwork names <c>ceres_door_left_transition_3</c>.</summary>
    CeresDoorLeftTransition3,
    /// <summary>The frame the installed artwork names <c>ceres_door_right_hold</c>.</summary>
    CeresDoorRightHold,
    /// <summary>The frame the installed artwork names <c>ceres_door_right_transition_0</c>.</summary>
    CeresDoorRightTransition0,
    /// <summary>The frame the installed artwork names <c>ceres_door_right_transition_1</c>.</summary>
    CeresDoorRightTransition1,
    /// <summary>The frame the installed artwork names <c>ceres_door_right_transition_2</c>.</summary>
    CeresDoorRightTransition2,
    /// <summary>The frame the installed artwork names <c>ceres_door_right_transition_3</c>.</summary>
    CeresDoorRightTransition3,
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
    RioPose0,
    /// <summary>The frame the installed artwork names <c>rio_pose_1</c>.</summary>
    RioPose1,
    /// <summary>The frame the installed artwork names <c>rio_pose_2</c>.</summary>
    RioPose2,
    /// <summary>The frame the installed artwork names <c>rio_pose_3</c>.</summary>
    RioPose3,
    /// <summary>The frame the installed artwork names <c>rio_pose_4</c>.</summary>
    RioPose4,
    /// <summary>The frame the installed artwork names <c>rio_pose_5</c>.</summary>
    RioPose5,
    /// <summary>The frame the installed artwork names <c>rio_pose_6</c>.</summary>
    RioPose6,
    /// <summary>The frame the installed artwork names <c>rio_pose_7</c>.</summary>
    RioPose7,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_0</c>.</summary>
    LowerNorfairRioPose0,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_1</c>.</summary>
    LowerNorfairRioPose1,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_2</c>.</summary>
    LowerNorfairRioPose2,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_3</c>.</summary>
    LowerNorfairRioPose3,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_4</c>.</summary>
    LowerNorfairRioPose4,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_5</c>.</summary>
    LowerNorfairRioPose5,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_6</c>.</summary>
    LowerNorfairRioPose6,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_7</c>.</summary>
    LowerNorfairRioPose7,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_8</c>.</summary>
    LowerNorfairRioPose8,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_9</c>.</summary>
    LowerNorfairRioPose9,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_10</c>.</summary>
    LowerNorfairRioPose10,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_11</c>.</summary>
    LowerNorfairRioPose11,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_12</c>.</summary>
    LowerNorfairRioPose12,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_13</c>.</summary>
    LowerNorfairRioPose13,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_14</c>.</summary>
    LowerNorfairRioPose14,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_15</c>.</summary>
    LowerNorfairRioPose15,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_16</c>.</summary>
    LowerNorfairRioPose16,
    /// <summary>The frame the installed artwork names <c>lower_norfair_rio_pose_17</c>.</summary>
    LowerNorfairRioPose17,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_0</c>.</summary>
    NorfairRioPose0,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_1</c>.</summary>
    NorfairRioPose1,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_2</c>.</summary>
    NorfairRioPose2,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_3</c>.</summary>
    NorfairRioPose3,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_4</c>.</summary>
    NorfairRioPose4,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_5</c>.</summary>
    NorfairRioPose5,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_6</c>.</summary>
    NorfairRioPose6,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_7</c>.</summary>
    NorfairRioPose7,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_8</c>.</summary>
    NorfairRioPose8,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_9</c>.</summary>
    NorfairRioPose9,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_10</c>.</summary>
    NorfairRioPose10,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_11</c>.</summary>
    NorfairRioPose11,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_12</c>.</summary>
    NorfairRioPose12,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_13</c>.</summary>
    NorfairRioPose13,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_14</c>.</summary>
    NorfairRioPose14,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_15</c>.</summary>
    NorfairRioPose15,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_16</c>.</summary>
    NorfairRioPose16,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_17</c>.</summary>
    NorfairRioPose17,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_18</c>.</summary>
    NorfairRioPose18,
    /// <summary>The frame the installed artwork names <c>norfair_rio_pose_19</c>.</summary>
    NorfairRioPose19,
    /// <summary>The frame the installed artwork names <c>puyo_ground_0</c>.</summary>
    PuyoGround0,
    /// <summary>The frame the installed artwork names <c>puyo_ground_1</c>.</summary>
    PuyoGround1,
    /// <summary>The frame the installed artwork names <c>puyo_ground_2</c>.</summary>
    PuyoGround2,
    /// <summary>The frame the installed artwork names <c>puyo_air_0</c>.</summary>
    PuyoAir0,
    /// <summary>The frame the installed artwork names <c>puyo_air_1</c>.</summary>
    PuyoAir1,
    /// <summary>The frame the installed artwork names <c>puyo_air_2</c>.</summary>
    PuyoAir2,
    /// <summary>The frame the installed artwork names <c>puyo_air_3</c>.</summary>
    PuyoAir3,
    /// <summary>The frame the installed artwork names <c>puyo_air_4</c>.</summary>
    PuyoAir4,
    /// <summary>The frame the installed artwork names <c>bull_idle_0</c>.</summary>
    BullIdle0,
    /// <summary>The frame the installed artwork names <c>bull_idle_1</c>.</summary>
    BullIdle1,
    /// <summary>The frame the installed artwork names <c>bull_idle_2</c>.</summary>
    BullIdle2,
    /// <summary>The frame the installed artwork names <c>alcoon_left_walk_0</c>.</summary>
    AlcoonLeftWalk0,
    /// <summary>The frame the installed artwork names <c>alcoon_left_walk_1</c>.</summary>
    AlcoonLeftWalk1,
    /// <summary>The frame the installed artwork names <c>alcoon_left_walk_2</c>.</summary>
    AlcoonLeftWalk2,
    /// <summary>The frame the installed artwork names <c>alcoon_left_walk_3</c>.</summary>
    AlcoonLeftWalk3,
    /// <summary>The frame the installed artwork names <c>alcoon_left_fire_0</c>.</summary>
    AlcoonLeftFire0,
    /// <summary>The frame the installed artwork names <c>alcoon_left_fire_1</c>.</summary>
    AlcoonLeftFire1,
    /// <summary>The frame the installed artwork names <c>alcoon_left_fire_2</c>.</summary>
    AlcoonLeftFire2,
    /// <summary>The frame the installed artwork names <c>alcoon_left_fire_3</c>.</summary>
    AlcoonLeftFire3,
    /// <summary>The frame the installed artwork names <c>alcoon_left_air_up</c>.</summary>
    AlcoonLeftAirUp,
    /// <summary>The frame the installed artwork names <c>alcoon_right_walk_0</c>.</summary>
    AlcoonRightWalk0,
    /// <summary>The frame the installed artwork names <c>alcoon_right_walk_1</c>.</summary>
    AlcoonRightWalk1,
    /// <summary>The frame the installed artwork names <c>alcoon_right_walk_2</c>.</summary>
    AlcoonRightWalk2,
    /// <summary>The frame the installed artwork names <c>alcoon_right_walk_3</c>.</summary>
    AlcoonRightWalk3,
    /// <summary>The frame the installed artwork names <c>alcoon_right_fire_0</c>.</summary>
    AlcoonRightFire0,
    /// <summary>The frame the installed artwork names <c>alcoon_right_fire_1</c>.</summary>
    AlcoonRightFire1,
    /// <summary>The frame the installed artwork names <c>alcoon_right_fire_2</c>.</summary>
    AlcoonRightFire2,
    /// <summary>The frame the installed artwork names <c>alcoon_right_fire_3</c>.</summary>
    AlcoonRightFire3,
    /// <summary>The frame the installed artwork names <c>alcoon_right_air_up</c>.</summary>
    AlcoonRightAirUp,
    /// <summary>The frame the installed artwork names <c>beetom_left_crawl_0</c>.</summary>
    BeetomLeftCrawl0,
    /// <summary>The frame the installed artwork names <c>beetom_left_crawl_1</c>.</summary>
    BeetomLeftCrawl1,
    /// <summary>The frame the installed artwork names <c>beetom_left_crawl_2</c>.</summary>
    BeetomLeftCrawl2,
    /// <summary>The frame the installed artwork names <c>beetom_left_hop_0</c>.</summary>
    BeetomLeftHop0,
    /// <summary>The frame the installed artwork names <c>beetom_left_hop_1</c>.</summary>
    BeetomLeftHop1,
    /// <summary>The frame the installed artwork names <c>beetom_left_drain_0</c>.</summary>
    BeetomLeftDrain0,
    /// <summary>The frame the installed artwork names <c>beetom_left_drain_1</c>.</summary>
    BeetomLeftDrain1,
    /// <summary>The frame the installed artwork names <c>beetom_left_drain_2</c>.</summary>
    BeetomLeftDrain2,
    /// <summary>The frame the installed artwork names <c>beetom_left_drain_3</c>.</summary>
    BeetomLeftDrain3,
    /// <summary>The frame the installed artwork names <c>beetom_left_drain_4</c>.</summary>
    BeetomLeftDrain4,
    /// <summary>The frame the installed artwork names <c>beetom_left_drain_5</c>.</summary>
    BeetomLeftDrain5,
    /// <summary>The frame the installed artwork names <c>beetom_right_crawl_0</c>.</summary>
    BeetomRightCrawl0,
    /// <summary>The frame the installed artwork names <c>beetom_right_crawl_1</c>.</summary>
    BeetomRightCrawl1,
    /// <summary>The frame the installed artwork names <c>beetom_right_crawl_2</c>.</summary>
    BeetomRightCrawl2,
    /// <summary>The frame the installed artwork names <c>beetom_right_hop_0</c>.</summary>
    BeetomRightHop0,
    /// <summary>The frame the installed artwork names <c>beetom_right_hop_1</c>.</summary>
    BeetomRightHop1,
    /// <summary>The frame the installed artwork names <c>beetom_right_drain_0</c>.</summary>
    BeetomRightDrain0,
    /// <summary>The frame the installed artwork names <c>beetom_right_drain_1</c>.</summary>
    BeetomRightDrain1,
    /// <summary>The frame the installed artwork names <c>beetom_right_drain_2</c>.</summary>
    BeetomRightDrain2,
    /// <summary>The frame the installed artwork names <c>beetom_right_drain_3</c>.</summary>
    BeetomRightDrain3,
    /// <summary>The frame the installed artwork names <c>beetom_right_drain_4</c>.</summary>
    BeetomRightDrain4,
    /// <summary>The frame the installed artwork names <c>beetom_right_drain_5</c>.</summary>
    BeetomRightDrain5,
    /// <summary>The frame the installed artwork names <c>sidehopper_jump_floor</c>.</summary>
    SidehopperJumpFloor,
    /// <summary>The frame the installed artwork names <c>sidehopper_land_floor_0</c>.</summary>
    SidehopperLandFloor0,
    /// <summary>The frame the installed artwork names <c>sidehopper_land_floor_1</c>.</summary>
    SidehopperLandFloor1,
    /// <summary>The frame the installed artwork names <c>sidehopper_jump_ceiling</c>.</summary>
    SidehopperJumpCeiling,
    /// <summary>The frame the installed artwork names <c>sidehopper_land_ceiling_0</c>.</summary>
    SidehopperLandCeiling0,
    /// <summary>The frame the installed artwork names <c>sidehopper_land_ceiling_1</c>.</summary>
    SidehopperLandCeiling1,
    /// <summary>The frame the installed artwork names <c>dessgeega_jump_floor</c>.</summary>
    DessgeegaJumpFloor,
    /// <summary>The frame the installed artwork names <c>dessgeega_land_floor_0</c>.</summary>
    DessgeegaLandFloor0,
    /// <summary>The frame the installed artwork names <c>dessgeega_land_floor_1</c>.</summary>
    DessgeegaLandFloor1,
    /// <summary>The frame the installed artwork names <c>dessgeega_jump_ceiling</c>.</summary>
    DessgeegaJumpCeiling,
    /// <summary>The frame the installed artwork names <c>dessgeega_land_ceiling_0</c>.</summary>
    DessgeegaLandCeiling0,
    /// <summary>The frame the installed artwork names <c>dessgeega_land_ceiling_1</c>.</summary>
    DessgeegaLandCeiling1,
    /// <summary>The frame the installed artwork names <c>large_sidehopper_jump_floor</c>.</summary>
    LargeSidehopperJumpFloor,
    /// <summary>The frame the installed artwork names <c>large_sidehopper_land_floor_0</c>.</summary>
    LargeSidehopperLandFloor0,
    /// <summary>The frame the installed artwork names <c>large_sidehopper_land_floor_1</c>.</summary>
    LargeSidehopperLandFloor1,
    /// <summary>The frame the installed artwork names <c>large_sidehopper_jump_ceiling</c>.</summary>
    LargeSidehopperJumpCeiling,
    /// <summary>The frame the installed artwork names <c>large_sidehopper_land_ceiling_0</c>.</summary>
    LargeSidehopperLandCeiling0,
    /// <summary>The frame the installed artwork names <c>large_sidehopper_land_ceiling_1</c>.</summary>
    LargeSidehopperLandCeiling1,
    /// <summary>The frame the installed artwork names <c>large_dessgeega_jump_floor</c>.</summary>
    LargeDessgeegaJumpFloor,
    /// <summary>The frame the installed artwork names <c>large_dessgeega_land_floor_0</c>.</summary>
    LargeDessgeegaLandFloor0,
    /// <summary>The frame the installed artwork names <c>large_dessgeega_land_floor_1</c>.</summary>
    LargeDessgeegaLandFloor1,
    /// <summary>The frame the installed artwork names <c>large_dessgeega_jump_ceiling</c>.</summary>
    LargeDessgeegaJumpCeiling,
    /// <summary>The frame the installed artwork names <c>large_dessgeega_land_ceiling_0</c>.</summary>
    LargeDessgeegaLandCeiling0,
    /// <summary>The frame the installed artwork names <c>large_dessgeega_land_ceiling_1</c>.</summary>
    LargeDessgeegaLandCeiling1,
    /// <summary>The frame the installed artwork names <c>choot_idle</c>.</summary>
    ChootIdle,
    /// <summary>The frame the installed artwork names <c>choot_jump</c>.</summary>
    ChootJump,
    /// <summary>The frame the installed artwork names <c>choot_jump_apex</c>.</summary>
    ChootJumpApex,
    /// <summary>The frame the installed artwork names <c>choot_fall_end</c>.</summary>
    ChootFallEnd,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_right_0</c>.</summary>
    HzoomerUpsideRight0,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_right_1</c>.</summary>
    HzoomerUpsideRight1,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_right_2</c>.</summary>
    HzoomerUpsideRight2,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_right_3</c>.</summary>
    HzoomerUpsideRight3,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_right_4</c>.</summary>
    HzoomerUpsideRight4,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_left_0</c>.</summary>
    HzoomerUpsideLeft0,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_left_1</c>.</summary>
    HzoomerUpsideLeft1,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_left_2</c>.</summary>
    HzoomerUpsideLeft2,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_left_3</c>.</summary>
    HzoomerUpsideLeft3,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_left_4</c>.</summary>
    HzoomerUpsideLeft4,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_down_0</c>.</summary>
    HzoomerUpsideDown0,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_down_1</c>.</summary>
    HzoomerUpsideDown1,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_down_2</c>.</summary>
    HzoomerUpsideDown2,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_down_3</c>.</summary>
    HzoomerUpsideDown3,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_down_4</c>.</summary>
    HzoomerUpsideDown4,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_up_0</c>.</summary>
    HzoomerUpsideUp0,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_up_1</c>.</summary>
    HzoomerUpsideUp1,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_up_2</c>.</summary>
    HzoomerUpsideUp2,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_up_3</c>.</summary>
    HzoomerUpsideUp3,
    /// <summary>The frame the installed artwork names <c>hzoomer_upside_up_4</c>.</summary>
    HzoomerUpsideUp4,
    /// <summary>The frame the installed artwork names <c>sbug_right_0</c>.</summary>
    SbugRight0,
    /// <summary>The frame the installed artwork names <c>sbug_right_1</c>.</summary>
    SbugRight1,
    /// <summary>The frame the installed artwork names <c>sbug_right_2</c>.</summary>
    SbugRight2,
    /// <summary>The frame the installed artwork names <c>sbug_up_right_0</c>.</summary>
    SbugUpRight0,
    /// <summary>The frame the installed artwork names <c>sbug_up_right_1</c>.</summary>
    SbugUpRight1,
    /// <summary>The frame the installed artwork names <c>sbug_up_right_2</c>.</summary>
    SbugUpRight2,
    /// <summary>The frame the installed artwork names <c>sbug_up_0</c>.</summary>
    SbugUp0,
    /// <summary>The frame the installed artwork names <c>sbug_up_1</c>.</summary>
    SbugUp1,
    /// <summary>The frame the installed artwork names <c>sbug_up_2</c>.</summary>
    SbugUp2,
    /// <summary>The frame the installed artwork names <c>sbug_up_left_0</c>.</summary>
    SbugUpLeft0,
    /// <summary>The frame the installed artwork names <c>sbug_up_left_1</c>.</summary>
    SbugUpLeft1,
    /// <summary>The frame the installed artwork names <c>sbug_up_left_2</c>.</summary>
    SbugUpLeft2,
    /// <summary>The frame the installed artwork names <c>sbug_left_0</c>.</summary>
    SbugLeft0,
    /// <summary>The frame the installed artwork names <c>sbug_left_1</c>.</summary>
    SbugLeft1,
    /// <summary>The frame the installed artwork names <c>sbug_left_2</c>.</summary>
    SbugLeft2,
    /// <summary>The frame the installed artwork names <c>sbug_down_left_0</c>.</summary>
    SbugDownLeft0,
    /// <summary>The frame the installed artwork names <c>sbug_down_left_1</c>.</summary>
    SbugDownLeft1,
    /// <summary>The frame the installed artwork names <c>sbug_down_left_2</c>.</summary>
    SbugDownLeft2,
    /// <summary>The frame the installed artwork names <c>sbug_down_0</c>.</summary>
    SbugDown0,
    /// <summary>The frame the installed artwork names <c>sbug_down_1</c>.</summary>
    SbugDown1,
    /// <summary>The frame the installed artwork names <c>sbug_down_2</c>.</summary>
    SbugDown2,
    /// <summary>The frame the installed artwork names <c>sbug_down_right_0</c>.</summary>
    SbugDownRight0,
    /// <summary>The frame the installed artwork names <c>sbug_down_right_1</c>.</summary>
    SbugDownRight1,
    /// <summary>The frame the installed artwork names <c>sbug_down_right_2</c>.</summary>
    SbugDownRight2,
    /// <summary>The frame the installed artwork names <c>fune_left_idle</c>.</summary>
    FuneLeftIdle,
    /// <summary>The frame the installed artwork names <c>fune_left_active_0</c>.</summary>
    FuneLeftActive0,
    /// <summary>The frame the installed artwork names <c>fune_left_active_1</c>.</summary>
    FuneLeftActive1,
    /// <summary>The frame the installed artwork names <c>fune_left_active_2</c>.</summary>
    FuneLeftActive2,
    /// <summary>The frame the installed artwork names <c>fune_left_active_3</c>.</summary>
    FuneLeftActive3,
    /// <summary>The frame the installed artwork names <c>fune_right_idle</c>.</summary>
    FuneRightIdle,
    /// <summary>The frame the installed artwork names <c>fune_right_active_0</c>.</summary>
    FuneRightActive0,
    /// <summary>The frame the installed artwork names <c>fune_right_active_1</c>.</summary>
    FuneRightActive1,
    /// <summary>The frame the installed artwork names <c>fune_right_active_2</c>.</summary>
    FuneRightActive2,
    /// <summary>The frame the installed artwork names <c>fune_right_active_3</c>.</summary>
    FuneRightActive3,
    /// <summary>The frame the installed artwork names <c>namihe_left_idle</c>.</summary>
    NamiheLeftIdle,
    /// <summary>The frame the installed artwork names <c>namihe_left_active_0</c>.</summary>
    NamiheLeftActive0,
    /// <summary>The frame the installed artwork names <c>namihe_left_active_1</c>.</summary>
    NamiheLeftActive1,
    /// <summary>The frame the installed artwork names <c>namihe_left_active_2</c>.</summary>
    NamiheLeftActive2,
    /// <summary>The frame the installed artwork names <c>namihe_left_active_3</c>.</summary>
    NamiheLeftActive3,
    /// <summary>The frame the installed artwork names <c>namihe_left_active_4</c>.</summary>
    NamiheLeftActive4,
    /// <summary>The frame the installed artwork names <c>namihe_right_idle</c>.</summary>
    NamiheRightIdle,
    /// <summary>The frame the installed artwork names <c>namihe_right_active_0</c>.</summary>
    NamiheRightActive0,
    /// <summary>The frame the installed artwork names <c>namihe_right_active_1</c>.</summary>
    NamiheRightActive1,
    /// <summary>The frame the installed artwork names <c>namihe_right_active_2</c>.</summary>
    NamiheRightActive2,
    /// <summary>The frame the installed artwork names <c>namihe_right_active_3</c>.</summary>
    NamiheRightActive3,
    /// <summary>The frame the installed artwork names <c>namihe_right_active_4</c>.</summary>
    NamiheRightActive4,
    /// <summary>The frame the installed artwork names <c>kamer_platform_0</c>.</summary>
    KamerPlatform0,
    /// <summary>The frame the installed artwork names <c>kamer_platform_1</c>.</summary>
    KamerPlatform1,
    /// <summary>The frame the installed artwork names <c>kamer_platform_2</c>.</summary>
    KamerPlatform2,
    /// <summary>The frame the installed artwork names <c>kamer_platform_3</c>.</summary>
    KamerPlatform3,
    /// <summary>The frame the installed artwork names <c>elevator_platform_0</c>.</summary>
    ElevatorPlatform0,
    /// <summary>The frame the installed artwork names <c>elevator_platform_1</c>.</summary>
    ElevatorPlatform1,
    /// <summary>The frame the installed artwork names <c>draygon_intro_evir_0</c>.</summary>
    DraygonIntroEvir0,
    /// <summary>The frame the installed artwork names <c>draygon_intro_evir_1</c>.</summary>
    DraygonIntroEvir1,
    /// <summary>The frame the installed artwork names <c>draygon_intro_evir_2</c>.</summary>
    DraygonIntroEvir2,
    /// <summary>The frame the installed artwork names <c>draygon_intro_evir_3</c>.</summary>
    DraygonIntroEvir3,
    /// <summary>The frame the installed artwork names <c>draygon_breath_bubble_0</c>.</summary>
    DraygonBreathBubble0,
    /// <summary>The frame the installed artwork names <c>draygon_breath_bubble_1</c>.</summary>
    DraygonBreathBubble1,
    /// <summary>The frame the installed artwork names <c>draygon_breath_bubble_2</c>.</summary>
    DraygonBreathBubble2,
    /// <summary>The frame the installed artwork names <c>draygon_breath_bubble_3</c>.</summary>
    DraygonBreathBubble3,
    /// <summary>The frame the installed artwork names <c>draygon_breath_bubble_4</c>.</summary>
    DraygonBreathBubble4,
    /// <summary>The frame the installed artwork names <c>draygon_breath_bubble_5</c>.</summary>
    DraygonBreathBubble5,
    /// <summary>The frame the installed artwork names <c>draygon_breath_bubble_6</c>.</summary>
    DraygonBreathBubble6,
    /// <summary>The frame the installed artwork names <c>draygon_breath_bubble_7</c>.</summary>
    DraygonBreathBubble7,
    /// <summary>The frame the installed artwork names <c>draygon_breath_bubble_8</c>.</summary>
    DraygonBreathBubble8,
    /// <summary>The frame the installed artwork names <c>sprite_object_empty</c>.</summary>
    SpriteObjectEmpty,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_dust_cloud_0</c>.</summary>
    SpriteObjectBigDustCloud0,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_dust_cloud_1</c>.</summary>
    SpriteObjectBigDustCloud1,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_dust_cloud_2</c>.</summary>
    SpriteObjectBigDustCloud2,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_dust_cloud_3</c>.</summary>
    SpriteObjectBigDustCloud3,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_dust_cloud_4</c>.</summary>
    SpriteObjectBigDustCloud4,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_dust_cloud_5</c>.</summary>
    SpriteObjectBigDustCloud5,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_dust_cloud_6</c>.</summary>
    SpriteObjectBigDustCloud6,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_dust_cloud_7</c>.</summary>
    SpriteObjectBigDustCloud7,
    /// <summary>The frame the installed artwork names <c>sprite_object_small_explosion_0</c>.</summary>
    SpriteObjectSmallExplosion0,
    /// <summary>The frame the installed artwork names <c>sprite_object_small_explosion_1</c>.</summary>
    SpriteObjectSmallExplosion1,
    /// <summary>The frame the installed artwork names <c>sprite_object_small_explosion_2</c>.</summary>
    SpriteObjectSmallExplosion2,
    /// <summary>The frame the installed artwork names <c>sprite_object_small_explosion_3</c>.</summary>
    SpriteObjectSmallExplosion3,
    /// <summary>The frame the installed artwork names <c>sprite_object_small_explosion_4</c>.</summary>
    SpriteObjectSmallExplosion4,
    /// <summary>The frame the installed artwork names <c>sprite_object_small_explosion_5</c>.</summary>
    SpriteObjectSmallExplosion5,
    /// <summary>The frame the installed artwork names <c>sprite_object_dud_shot_0</c>.</summary>
    SpriteObjectDudShot0,
    /// <summary>The frame the installed artwork names <c>sprite_object_dud_shot_1</c>.</summary>
    SpriteObjectDudShot1,
    /// <summary>The frame the installed artwork names <c>sprite_object_dud_shot_2</c>.</summary>
    SpriteObjectDudShot2,
    /// <summary>The frame the installed artwork names <c>sprite_object_dud_shot_3</c>.</summary>
    SpriteObjectDudShot3,
    /// <summary>The frame the installed artwork names <c>sprite_object_dud_shot_4</c>.</summary>
    SpriteObjectDudShot4,
    /// <summary>The frame the installed artwork names <c>sprite_object_dud_shot_5</c>.</summary>
    SpriteObjectDudShot5,
    /// <summary>The frame the installed artwork names <c>sprite_object_small_dust_cloud_0</c>.</summary>
    SpriteObjectSmallDustCloud0,
    /// <summary>The frame the installed artwork names <c>sprite_object_small_dust_cloud_1</c>.</summary>
    SpriteObjectSmallDustCloud1,
    /// <summary>The frame the installed artwork names <c>sprite_object_small_dust_cloud_2</c>.</summary>
    SpriteObjectSmallDustCloud2,
    /// <summary>The frame the installed artwork names <c>sprite_object_small_dust_cloud_3</c>.</summary>
    SpriteObjectSmallDustCloud3,
    /// <summary>The frame the installed artwork names <c>sprite_object_pirate_landing_dust_cloud_0</c>.</summary>
    SpriteObjectPirateLandingDustCloud0,
    /// <summary>The frame the installed artwork names <c>sprite_object_pirate_landing_dust_cloud_1</c>.</summary>
    SpriteObjectPirateLandingDustCloud1,
    /// <summary>The frame the installed artwork names <c>sprite_object_pirate_landing_dust_cloud_2</c>.</summary>
    SpriteObjectPirateLandingDustCloud2,
    /// <summary>The frame the installed artwork names <c>sprite_object_pirate_landing_dust_cloud_3</c>.</summary>
    SpriteObjectPirateLandingDustCloud3,
    /// <summary>The frame the installed artwork names <c>sprite_object_smoke_0</c>.</summary>
    SpriteObjectSmoke0,
    /// <summary>The frame the installed artwork names <c>sprite_object_smoke_1</c>.</summary>
    SpriteObjectSmoke1,
    /// <summary>The frame the installed artwork names <c>sprite_object_smoke_2</c>.</summary>
    SpriteObjectSmoke2,
    /// <summary>The frame the installed artwork names <c>sprite_object_smoke_3</c>.</summary>
    SpriteObjectSmoke3,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_explosion_0</c>.</summary>
    SpriteObjectBigExplosion0,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_explosion_1</c>.</summary>
    SpriteObjectBigExplosion1,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_explosion_2</c>.</summary>
    SpriteObjectBigExplosion2,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_explosion_3</c>.</summary>
    SpriteObjectBigExplosion3,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_explosion_4</c>.</summary>
    SpriteObjectBigExplosion4,
    /// <summary>The frame the installed artwork names <c>sprite_object_big_explosion_5</c>.</summary>
    SpriteObjectBigExplosion5,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_0</c>.</summary>
    SpriteObjectPuromi0,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_1</c>.</summary>
    SpriteObjectPuromi1,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_2</c>.</summary>
    SpriteObjectPuromi2,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_3</c>.</summary>
    SpriteObjectPuromi3,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_4</c>.</summary>
    SpriteObjectPuromi4,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_5</c>.</summary>
    SpriteObjectPuromi5,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_6</c>.</summary>
    SpriteObjectPuromi6,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_7</c>.</summary>
    SpriteObjectPuromi7,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_right_explosion_0</c>.</summary>
    SpriteObjectPuromiRightExplosion0,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_right_explosion_1</c>.</summary>
    SpriteObjectPuromiRightExplosion1,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_right_explosion_2</c>.</summary>
    SpriteObjectPuromiRightExplosion2,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_right_explosion_3</c>.</summary>
    SpriteObjectPuromiRightExplosion3,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_right_explosion_4</c>.</summary>
    SpriteObjectPuromiRightExplosion4,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_right_explosion_5</c>.</summary>
    SpriteObjectPuromiRightExplosion5,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_right_explosion_6</c>.</summary>
    SpriteObjectPuromiRightExplosion6,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_left_explosion_0</c>.</summary>
    SpriteObjectPuromiLeftExplosion0,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_left_explosion_1</c>.</summary>
    SpriteObjectPuromiLeftExplosion1,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_left_explosion_2</c>.</summary>
    SpriteObjectPuromiLeftExplosion2,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_left_explosion_3</c>.</summary>
    SpriteObjectPuromiLeftExplosion3,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_left_explosion_4</c>.</summary>
    SpriteObjectPuromiLeftExplosion4,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_left_explosion_5</c>.</summary>
    SpriteObjectPuromiLeftExplosion5,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_left_explosion_6</c>.</summary>
    SpriteObjectPuromiLeftExplosion6,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_splash_0</c>.</summary>
    SpriteObjectPuromiSplash0,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_splash_1</c>.</summary>
    SpriteObjectPuromiSplash1,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_splash_2</c>.</summary>
    SpriteObjectPuromiSplash2,
    /// <summary>The frame the installed artwork names <c>sprite_object_puromi_splash_4</c>.</summary>
    SpriteObjectPuromiSplash4,
    /// <summary>The frame the installed artwork names <c>sprite_object_falling_spark_trail_0</c>.</summary>
    SpriteObjectFallingSparkTrail0,
    /// <summary>The frame the installed artwork names <c>sprite_object_falling_spark_trail_1</c>.</summary>
    SpriteObjectFallingSparkTrail1,
    /// <summary>The frame the installed artwork names <c>sprite_object_falling_spark_trail_2</c>.</summary>
    SpriteObjectFallingSparkTrail2,
    /// <summary>The frame the installed artwork names <c>sprite_object_falling_spark_trail_3</c>.</summary>
    SpriteObjectFallingSparkTrail3,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_shell_0</c>.</summary>
    SpriteObjectMetroidShell0,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_shell_1</c>.</summary>
    SpriteObjectMetroidShell1,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_shell_2</c>.</summary>
    SpriteObjectMetroidShell2,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_0</c>.</summary>
    SpriteObjectMetroidElectricity0,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_1</c>.</summary>
    SpriteObjectMetroidElectricity1,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_2</c>.</summary>
    SpriteObjectMetroidElectricity2,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_3</c>.</summary>
    SpriteObjectMetroidElectricity3,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_4</c>.</summary>
    SpriteObjectMetroidElectricity4,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_5</c>.</summary>
    SpriteObjectMetroidElectricity5,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_6</c>.</summary>
    SpriteObjectMetroidElectricity6,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_7</c>.</summary>
    SpriteObjectMetroidElectricity7,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_8</c>.</summary>
    SpriteObjectMetroidElectricity8,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_9</c>.</summary>
    SpriteObjectMetroidElectricity9,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_10</c>.</summary>
    SpriteObjectMetroidElectricity10,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_11</c>.</summary>
    SpriteObjectMetroidElectricity11,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_12</c>.</summary>
    SpriteObjectMetroidElectricity12,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_13</c>.</summary>
    SpriteObjectMetroidElectricity13,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_14</c>.</summary>
    SpriteObjectMetroidElectricity14,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_15</c>.</summary>
    SpriteObjectMetroidElectricity15,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_16</c>.</summary>
    SpriteObjectMetroidElectricity16,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_17</c>.</summary>
    SpriteObjectMetroidElectricity17,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_18</c>.</summary>
    SpriteObjectMetroidElectricity18,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_19</c>.</summary>
    SpriteObjectMetroidElectricity19,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_20</c>.</summary>
    SpriteObjectMetroidElectricity20,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_21</c>.</summary>
    SpriteObjectMetroidElectricity21,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_22</c>.</summary>
    SpriteObjectMetroidElectricity22,
    /// <summary>The frame the installed artwork names <c>sprite_object_metroid_electricity_23</c>.</summary>
    SpriteObjectMetroidElectricity23,
    /// <summary>The frame the installed artwork names <c>sprite_object_yapping_maw_base_facing_down</c>.</summary>
    SpriteObjectYappingMawBaseFacingDown,
    /// <summary>The frame the installed artwork names <c>sprite_object_yapping_maw_base_facing_up</c>.</summary>
    SpriteObjectYappingMawBaseFacingUp,
    /// <summary>The frame the installed artwork names <c>sprite_object_evir_facing_right_0</c>.</summary>
    SpriteObjectEvirFacingRight0,
    /// <summary>The frame the installed artwork names <c>sprite_object_evir_facing_right_1</c>.</summary>
    SpriteObjectEvirFacingRight1,
    /// <summary>The frame the installed artwork names <c>sprite_object_evir_facing_right_2</c>.</summary>
    SpriteObjectEvirFacingRight2,
    /// <summary>The frame the installed artwork names <c>sprite_object_evir_facing_right_3</c>.</summary>
    SpriteObjectEvirFacingRight3,
    /// <summary>The frame the installed artwork names <c>sprite_object_draygon_foaming_at_the_mouth_0</c>.</summary>
    SpriteObjectDraygonFoamingAtTheMouth0,
    /// <summary>The frame the installed artwork names <c>sprite_object_draygon_foaming_at_the_mouth_1</c>.</summary>
    SpriteObjectDraygonFoamingAtTheMouth1,
    /// <summary>The frame the installed artwork names <c>sprite_object_draygon_foaming_at_the_mouth_2</c>.</summary>
    SpriteObjectDraygonFoamingAtTheMouth2,
    /// <summary>The frame the installed artwork names <c>sprite_object_draygon_foaming_at_the_mouth_3</c>.</summary>
    SpriteObjectDraygonFoamingAtTheMouth3,
    /// <summary>The frame the installed artwork names <c>sprite_object_draygon_foaming_at_the_mouth_4</c>.</summary>
    SpriteObjectDraygonFoamingAtTheMouth4,
    /// <summary>The frame the installed artwork names <c>sprite_object_draygon_foaming_at_the_mouth_5</c>.</summary>
    SpriteObjectDraygonFoamingAtTheMouth5,
    /// <summary>The frame the installed artwork names <c>sprite_object_draygon_foaming_at_the_mouth_6</c>.</summary>
    SpriteObjectDraygonFoamingAtTheMouth6,
    /// <summary>The frame the installed artwork names <c>sprite_object_draygon_foaming_at_the_mouth_7</c>.</summary>
    SpriteObjectDraygonFoamingAtTheMouth7,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_0</c>.</summary>
    YappingMawPose0,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_1</c>.</summary>
    YappingMawPose1,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_2</c>.</summary>
    YappingMawPose2,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_3</c>.</summary>
    YappingMawPose3,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_4</c>.</summary>
    YappingMawPose4,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_5</c>.</summary>
    YappingMawPose5,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_6</c>.</summary>
    YappingMawPose6,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_7</c>.</summary>
    YappingMawPose7,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_8</c>.</summary>
    YappingMawPose8,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_9</c>.</summary>
    YappingMawPose9,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_10</c>.</summary>
    YappingMawPose10,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_11</c>.</summary>
    YappingMawPose11,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_12</c>.</summary>
    YappingMawPose12,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_13</c>.</summary>
    YappingMawPose13,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_14</c>.</summary>
    YappingMawPose14,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_15</c>.</summary>
    YappingMawPose15,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_16</c>.</summary>
    YappingMawPose16,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_17</c>.</summary>
    YappingMawPose17,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_18</c>.</summary>
    YappingMawPose18,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_19</c>.</summary>
    YappingMawPose19,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_20</c>.</summary>
    YappingMawPose20,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_21</c>.</summary>
    YappingMawPose21,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_22</c>.</summary>
    YappingMawPose22,
    /// <summary>The frame the installed artwork names <c>yapping_maw_pose_23</c>.</summary>
    YappingMawPose23,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_0</c>.</summary>
    KiHunterPose0,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_1</c>.</summary>
    KiHunterPose1,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_2</c>.</summary>
    KiHunterPose2,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_3</c>.</summary>
    KiHunterPose3,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_4</c>.</summary>
    KiHunterPose4,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_5</c>.</summary>
    KiHunterPose5,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_6</c>.</summary>
    KiHunterPose6,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_7</c>.</summary>
    KiHunterPose7,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_8</c>.</summary>
    KiHunterPose8,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_9</c>.</summary>
    KiHunterPose9,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_10</c>.</summary>
    KiHunterPose10,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_11</c>.</summary>
    KiHunterPose11,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_12</c>.</summary>
    KiHunterPose12,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_13</c>.</summary>
    KiHunterPose13,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_14</c>.</summary>
    KiHunterPose14,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_15</c>.</summary>
    KiHunterPose15,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_16</c>.</summary>
    KiHunterPose16,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_17</c>.</summary>
    KiHunterPose17,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_18</c>.</summary>
    KiHunterPose18,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_19</c>.</summary>
    KiHunterPose19,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_20</c>.</summary>
    KiHunterPose20,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_21</c>.</summary>
    KiHunterPose21,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_22</c>.</summary>
    KiHunterPose22,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_23</c>.</summary>
    KiHunterPose23,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_24</c>.</summary>
    KiHunterPose24,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_25</c>.</summary>
    KiHunterPose25,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_26</c>.</summary>
    KiHunterPose26,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_27</c>.</summary>
    KiHunterPose27,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_28</c>.</summary>
    KiHunterPose28,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_29</c>.</summary>
    KiHunterPose29,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_30</c>.</summary>
    KiHunterPose30,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_31</c>.</summary>
    KiHunterPose31,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_32</c>.</summary>
    KiHunterPose32,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_33</c>.</summary>
    KiHunterPose33,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_34</c>.</summary>
    KiHunterPose34,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_35</c>.</summary>
    KiHunterPose35,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_36</c>.</summary>
    KiHunterPose36,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_37</c>.</summary>
    KiHunterPose37,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_38</c>.</summary>
    KiHunterPose38,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_39</c>.</summary>
    KiHunterPose39,
    /// <summary>The frame the installed artwork names <c>ki_hunter_pose_40</c>.</summary>
    KiHunterPose40,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_0</c>.</summary>
    MotherBrainHead0,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_1</c>.</summary>
    MotherBrainHead1,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_2</c>.</summary>
    MotherBrainHead2,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_3</c>.</summary>
    MotherBrainHead3,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_4</c>.</summary>
    MotherBrainHead4,
    /// <summary>The frame the installed artwork names <c>mother_brain_neck_joint</c>.</summary>
    MotherBrainNeckJoint,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_6</c>.</summary>
    MotherBrainHead6,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_7</c>.</summary>
    MotherBrainHead7,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_8</c>.</summary>
    MotherBrainHead8,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_9</c>.</summary>
    MotherBrainHead9,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_10</c>.</summary>
    MotherBrainHead10,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_11</c>.</summary>
    MotherBrainHead11,
    /// <summary>The frame the installed artwork names <c>mother_brain_head_12</c>.</summary>
    MotherBrainHead12,
    /// <summary>The frame the installed artwork names <c>mother_brain_tube_0</c>.</summary>
    MotherBrainTube0,
    /// <summary>The frame the installed artwork names <c>mother_brain_tube_1</c>.</summary>
    MotherBrainTube1,
    /// <summary>The frame the installed artwork names <c>mother_brain_tube_2</c>.</summary>
    MotherBrainTube2,
    /// <summary>The frame the installed artwork names <c>mother_brain_tube_3</c>.</summary>
    MotherBrainTube3,
    /// <summary>The frame the installed artwork names <c>mother_brain_tube_4</c>.</summary>
    MotherBrainTube4,
    /// <summary>The frame the installed artwork names <c>dead_torizo_corpse_hook</c>.</summary>
    DeadTorizoCorpseHook,
    /// <summary>The frame the installed artwork names <c>ridley_tail_segment_small</c>.</summary>
    RidleyTailSegmentSmall,
    /// <summary>The frame the installed artwork names <c>ridley_tail_segment_medium</c>.</summary>
    RidleyTailSegmentMedium,
    /// <summary>The frame the installed artwork names <c>ridley_tail_segment_large</c>.</summary>
    RidleyTailSegmentLarge,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_12</c>.</summary>
    RidleyTailTip12,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_11</c>.</summary>
    RidleyTailTip11,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_10</c>.</summary>
    RidleyTailTip10,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_9</c>.</summary>
    RidleyTailTip9,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_8</c>.</summary>
    RidleyTailTip8,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_7</c>.</summary>
    RidleyTailTip7,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_6</c>.</summary>
    RidleyTailTip6,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_5</c>.</summary>
    RidleyTailTip5,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_4</c>.</summary>
    RidleyTailTip4,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_3</c>.</summary>
    RidleyTailTip3,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_2</c>.</summary>
    RidleyTailTip2,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_1</c>.</summary>
    RidleyTailTip1,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_0</c>.</summary>
    RidleyTailTip0,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_15</c>.</summary>
    RidleyTailTip15,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_14</c>.</summary>
    RidleyTailTip14,
    /// <summary>The frame the installed artwork names <c>ridley_tail_tip_13</c>.</summary>
    RidleyTailTip13,
    /// <summary>The frame the installed artwork names <c>ridley_wing_left_0</c>.</summary>
    RidleyWingLeft0,
    /// <summary>The frame the installed artwork names <c>ridley_wing_left_1</c>.</summary>
    RidleyWingLeft1,
    /// <summary>The frame the installed artwork names <c>ridley_wing_left_2</c>.</summary>
    RidleyWingLeft2,
    /// <summary>The frame the installed artwork names <c>ridley_wing_left_3</c>.</summary>
    RidleyWingLeft3,
    /// <summary>The frame the installed artwork names <c>ridley_wing_left_4</c>.</summary>
    RidleyWingLeft4,
    /// <summary>The frame the installed artwork names <c>ridley_wing_left_5</c>.</summary>
    RidleyWingLeft5,
    /// <summary>The frame the installed artwork names <c>ridley_wing_right_0</c>.</summary>
    RidleyWingRight0,
    /// <summary>The frame the installed artwork names <c>ridley_wing_right_1</c>.</summary>
    RidleyWingRight1,
    /// <summary>The frame the installed artwork names <c>ridley_wing_right_2</c>.</summary>
    RidleyWingRight2,
    /// <summary>The frame the installed artwork names <c>ridley_wing_right_3</c>.</summary>
    RidleyWingRight3,
    /// <summary>The frame the installed artwork names <c>ridley_wing_right_4</c>.</summary>
    RidleyWingRight4,
    /// <summary>The frame the installed artwork names <c>ridley_wing_right_5</c>.</summary>
    RidleyWingRight5,
    /// <summary>The frame the installed artwork names <c>sciser_upside_right_0</c>.</summary>
    SciserUpsideRight0,
    /// <summary>The frame the installed artwork names <c>sciser_upside_right_1</c>.</summary>
    SciserUpsideRight1,
    /// <summary>The frame the installed artwork names <c>sciser_upside_right_2</c>.</summary>
    SciserUpsideRight2,
    /// <summary>The frame the installed artwork names <c>sciser_upside_left_0</c>.</summary>
    SciserUpsideLeft0,
    /// <summary>The frame the installed artwork names <c>sciser_upside_left_1</c>.</summary>
    SciserUpsideLeft1,
    /// <summary>The frame the installed artwork names <c>sciser_upside_left_2</c>.</summary>
    SciserUpsideLeft2,
    /// <summary>The frame the installed artwork names <c>sciser_upside_down_0</c>.</summary>
    SciserUpsideDown0,
    /// <summary>The frame the installed artwork names <c>sciser_upside_down_1</c>.</summary>
    SciserUpsideDown1,
    /// <summary>The frame the installed artwork names <c>sciser_upside_down_2</c>.</summary>
    SciserUpsideDown2,
    /// <summary>The frame the installed artwork names <c>sciser_upside_up_0</c>.</summary>
    SciserUpsideUp0,
    /// <summary>The frame the installed artwork names <c>sciser_upside_up_1</c>.</summary>
    SciserUpsideUp1,
    /// <summary>The frame the installed artwork names <c>sciser_upside_up_2</c>.</summary>
    SciserUpsideUp2,
    /// <summary>The frame the installed artwork names <c>fly_shared_0</c>.</summary>
    FlyShared0,
    /// <summary>The frame the installed artwork names <c>fly_shared_1</c>.</summary>
    FlyShared1,
    /// <summary>The frame the installed artwork names <c>fly_shared_2</c>.</summary>
    FlyShared2,
    /// <summary>The frame the installed artwork names <c>fly_shared_3</c>.</summary>
    FlyShared3,
    /// <summary>The frame the installed artwork names <c>kago_cycle_0</c>.</summary>
    KagoCycle0,
    /// <summary>The frame the installed artwork names <c>kago_cycle_1</c>.</summary>
    KagoCycle1,
    /// <summary>The frame the installed artwork names <c>kago_cycle_2</c>.</summary>
    KagoCycle2,
    /// <summary>The frame the installed artwork names <c>face_block_neutral</c>.</summary>
    FaceBlockNeutral,
    /// <summary>The frame the installed artwork names <c>face_block_samus_left_1</c>.</summary>
    FaceBlockSamusLeft1,
    /// <summary>The frame the installed artwork names <c>face_block_samus_left_2</c>.</summary>
    FaceBlockSamusLeft2,
    /// <summary>The frame the installed artwork names <c>face_block_samus_right_1</c>.</summary>
    FaceBlockSamusRight1,
    /// <summary>The frame the installed artwork names <c>face_block_samus_right_2</c>.</summary>
    FaceBlockSamusRight2,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_0</c>.</summary>
    MorphEyeActive0,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_1</c>.</summary>
    MorphEyeActive1,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_2</c>.</summary>
    MorphEyeActive2,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_3</c>.</summary>
    MorphEyeActive3,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_4</c>.</summary>
    MorphEyeActive4,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_5</c>.</summary>
    MorphEyeActive5,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_6</c>.</summary>
    MorphEyeActive6,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_7</c>.</summary>
    MorphEyeActive7,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_8</c>.</summary>
    MorphEyeActive8,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_9</c>.</summary>
    MorphEyeActive9,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_10</c>.</summary>
    MorphEyeActive10,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_11</c>.</summary>
    MorphEyeActive11,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_12</c>.</summary>
    MorphEyeActive12,
    /// <summary>The frame the installed artwork names <c>morph_eye_active_13</c>.</summary>
    MorphEyeActive13,
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
    ShutterGrowing10px,
    /// <summary>The frame the installed artwork names <c>shutter_growing_20px</c>.</summary>
    ShutterGrowing20px,
    /// <summary>The frame the installed artwork names <c>shutter_growing_30px</c>.</summary>
    ShutterGrowing30px,
    /// <summary>The frame the installed artwork names <c>shutter_vertical_40px</c>.</summary>
    ShutterVertical40px,
    /// <summary>The frame the installed artwork names <c>shutter_horizontal</c>.</summary>
    ShutterHorizontal,
    /// <summary>The frame the installed artwork names <c>metroid_body_0</c>.</summary>
    MetroidBody0,
    /// <summary>The frame the installed artwork names <c>metroid_body_1</c>.</summary>
    MetroidBody1,
    /// <summary>The frame the installed artwork names <c>metroid_body_2</c>.</summary>
    MetroidBody2,
    /// <summary>The frame the installed artwork names <c>metroid_body_3</c>.</summary>
    MetroidBody3,
    /// <summary>The frame the installed artwork names <c>shaktool_final_saw_0</c>.</summary>
    ShaktoolFinalSaw0,
    /// <summary>The frame the installed artwork names <c>shaktool_final_saw_1</c>.</summary>
    ShaktoolFinalSaw1,
    /// <summary>The frame the installed artwork names <c>shaktool_final_saw_2</c>.</summary>
    ShaktoolFinalSaw2,
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
    ShaktoolPrimarySaw0,
    /// <summary>The frame the installed artwork names <c>shaktool_primary_saw_1</c>.</summary>
    ShaktoolPrimarySaw1,
    /// <summary>The frame the installed artwork names <c>shaktool_primary_saw_2</c>.</summary>
    ShaktoolPrimarySaw2,
    /// <summary>The frame the installed artwork names <c>tripper_moving_left_0</c>.</summary>
    TripperMovingLeft0,
    /// <summary>The frame the installed artwork names <c>tripper_moving_left_1</c>.</summary>
    TripperMovingLeft1,
    /// <summary>The frame the installed artwork names <c>tripper_moving_left_2</c>.</summary>
    TripperMovingLeft2,
    /// <summary>The frame the installed artwork names <c>tripper_moving_right_0</c>.</summary>
    TripperMovingRight0,
    /// <summary>The frame the installed artwork names <c>tripper_moving_right_1</c>.</summary>
    TripperMovingRight1,
    /// <summary>The frame the installed artwork names <c>tripper_moving_right_2</c>.</summary>
    TripperMovingRight2,
    /// <summary>The frame the installed artwork names <c>tripper_still_moving_left_0</c>.</summary>
    TripperStillMovingLeft0,
    /// <summary>The frame the installed artwork names <c>tripper_still_moving_left_1</c>.</summary>
    TripperStillMovingLeft1,
    /// <summary>The frame the installed artwork names <c>tripper_still_moving_left_2</c>.</summary>
    TripperStillMovingLeft2,
    /// <summary>The frame the installed artwork names <c>tripper_still_moving_right_0</c>.</summary>
    TripperStillMovingRight0,
    /// <summary>The frame the installed artwork names <c>tripper_still_moving_right_1</c>.</summary>
    TripperStillMovingRight1,
    /// <summary>The frame the installed artwork names <c>tripper_still_moving_right_2</c>.</summary>
    TripperStillMovingRight2,
    /// <summary>The frame the installed artwork names <c>tripper_frozen_moving_left</c>.</summary>
    TripperFrozenMovingLeft,
    /// <summary>The frame the installed artwork names <c>tripper_frozen_moving_right</c>.</summary>
    TripperFrozenMovingRight,
    /// <summary>The frame the installed artwork names <c>tripper_kamer_platform_0</c>.</summary>
    TripperKamerPlatform0,
    /// <summary>The frame the installed artwork names <c>tripper_kamer_platform_1</c>.</summary>
    TripperKamerPlatform1,
    /// <summary>The frame the installed artwork names <c>tripper_kamer_platform_2</c>.</summary>
    TripperKamerPlatform2,
    /// <summary>The frame the installed artwork names <c>tripper_kamer_platform_3</c>.</summary>
    TripperKamerPlatform3,
    /// <summary>The frame the installed artwork names <c>dragon_body_idle_left</c>.</summary>
    DragonBodyIdleLeft,
    /// <summary>The frame the installed artwork names <c>dragon_wing_left_0</c>.</summary>
    DragonWingLeft0,
    /// <summary>The frame the installed artwork names <c>dragon_wing_left_1</c>.</summary>
    DragonWingLeft1,
    /// <summary>The frame the installed artwork names <c>dragon_body_idle_right</c>.</summary>
    DragonBodyIdleRight,
    /// <summary>The frame the installed artwork names <c>dragon_wing_right_0</c>.</summary>
    DragonWingRight0,
    /// <summary>The frame the installed artwork names <c>dragon_wing_right_1</c>.</summary>
    DragonWingRight1,
    /// <summary>The frame the installed artwork names <c>dragon_body_attack_left_0</c>.</summary>
    DragonBodyAttackLeft0,
    /// <summary>The frame the installed artwork names <c>dragon_body_attack_left_1</c>.</summary>
    DragonBodyAttackLeft1,
    /// <summary>The frame the installed artwork names <c>dragon_body_attack_left_2</c>.</summary>
    DragonBodyAttackLeft2,
    /// <summary>The frame the installed artwork names <c>dragon_body_attack_right_0</c>.</summary>
    DragonBodyAttackRight0,
    /// <summary>The frame the installed artwork names <c>dragon_body_attack_right_1</c>.</summary>
    DragonBodyAttackRight1,
    /// <summary>The frame the installed artwork names <c>dragon_body_attack_right_2</c>.</summary>
    DragonBodyAttackRight2,
    /// <summary>The frame the installed artwork names <c>multiviola_spin_0</c>.</summary>
    MultiviolaSpin0,
    /// <summary>The frame the installed artwork names <c>multiviola_spin_1</c>.</summary>
    MultiviolaSpin1,
    /// <summary>The frame the installed artwork names <c>multiviola_spin_2</c>.</summary>
    MultiviolaSpin2,
    /// <summary>The frame the installed artwork names <c>multiviola_spin_3</c>.</summary>
    MultiviolaSpin3,
    /// <summary>The frame the installed artwork names <c>multiviola_spin_4</c>.</summary>
    MultiviolaSpin4,
    /// <summary>The frame the installed artwork names <c>multiviola_spin_5</c>.</summary>
    MultiviolaSpin5,
    /// <summary>The frame the installed artwork names <c>multiviola_spin_6</c>.</summary>
    MultiviolaSpin6,
    /// <summary>The frame the installed artwork names <c>multiviola_spin_7</c>.</summary>
    MultiviolaSpin7,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_1</c>.</summary>
    NorfairLavaJumperPose1,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_2</c>.</summary>
    NorfairLavaJumperPose2,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_3</c>.</summary>
    NorfairLavaJumperPose3,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_4</c>.</summary>
    NorfairLavaJumperPose4,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_5</c>.</summary>
    NorfairLavaJumperPose5,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_6</c>.</summary>
    NorfairLavaJumperPose6,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_7</c>.</summary>
    NorfairLavaJumperPose7,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_8</c>.</summary>
    NorfairLavaJumperPose8,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_0</c>.</summary>
    NorfairLavaJumperPose0,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_10</c>.</summary>
    NorfairLavaJumperPose10,
    /// <summary>The frame the installed artwork names <c>norfair_lava_jumper_pose_9</c>.</summary>
    NorfairLavaJumperPose9,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_17</c>.</summary>
    ChozoStatuePose17,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_18</c>.</summary>
    ChozoStatuePose18,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_19</c>.</summary>
    ChozoStatuePose19,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_20</c>.</summary>
    ChozoStatuePose20,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_21</c>.</summary>
    ChozoStatuePose21,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_22</c>.</summary>
    ChozoStatuePose22,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_23</c>.</summary>
    ChozoStatuePose23,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_24</c>.</summary>
    ChozoStatuePose24,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_25</c>.</summary>
    ChozoStatuePose25,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_0</c>.</summary>
    ChozoStatuePose0,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_1</c>.</summary>
    ChozoStatuePose1,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_2</c>.</summary>
    ChozoStatuePose2,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_3</c>.</summary>
    ChozoStatuePose3,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_12</c>.</summary>
    ChozoStatuePose12,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_13</c>.</summary>
    ChozoStatuePose13,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_14</c>.</summary>
    ChozoStatuePose14,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_15</c>.</summary>
    ChozoStatuePose15,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_16</c>.</summary>
    ChozoStatuePose16,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_11</c>.</summary>
    ChozoStatuePose11,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_4</c>.</summary>
    ChozoStatuePose4,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_5</c>.</summary>
    ChozoStatuePose5,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_6</c>.</summary>
    ChozoStatuePose6,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_7</c>.</summary>
    ChozoStatuePose7,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_8</c>.</summary>
    ChozoStatuePose8,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_9</c>.</summary>
    ChozoStatuePose9,
    /// <summary>The frame the installed artwork names <c>chozo_statue_pose_10</c>.</summary>
    ChozoStatuePose10,
    /// <summary>The frame the installed artwork names <c>viola_spin_0</c>.</summary>
    ViolaSpin0,
    /// <summary>The frame the installed artwork names <c>viola_spin_1</c>.</summary>
    ViolaSpin1,
    /// <summary>The frame the installed artwork names <c>viola_spin_2</c>.</summary>
    ViolaSpin2,
    /// <summary>The frame the installed artwork names <c>viola_spin_3</c>.</summary>
    ViolaSpin3,
    /// <summary>The frame the installed artwork names <c>viola_spin_4</c>.</summary>
    ViolaSpin4,
    /// <summary>The frame the installed artwork names <c>viola_spin_5</c>.</summary>
    ViolaSpin5,
    /// <summary>The frame the installed artwork names <c>viola_spin_6</c>.</summary>
    ViolaSpin6,
    /// <summary>The frame the installed artwork names <c>viola_spin_7</c>.</summary>
    ViolaSpin7,
    /// <summary>The frame the installed artwork names <c>rinka_spin_0</c>.</summary>
    RinkaSpin0,
    /// <summary>The frame the installed artwork names <c>rinka_spin_1</c>.</summary>
    RinkaSpin1,
    /// <summary>The frame the installed artwork names <c>rinka_spin_2</c>.</summary>
    RinkaSpin2,
    /// <summary>The frame the installed artwork names <c>rinka_spin_3</c>.</summary>
    RinkaSpin3,
    /// <summary>The frame the installed artwork names <c>rinka_spin_4</c>.</summary>
    RinkaSpin4,
    /// <summary>The frame the installed artwork names <c>dead_torizo_stationary</c>.</summary>
    DeadTorizoStationary,
    /// <summary>The frame the installed artwork names <c>dead_zoomer_corpse_0</c>.</summary>
    DeadZoomerCorpse0,
    /// <summary>The frame the installed artwork names <c>dead_zoomer_corpse_2</c>.</summary>
    DeadZoomerCorpse2,
    /// <summary>The frame the installed artwork names <c>dead_zoomer_corpse_4</c>.</summary>
    DeadZoomerCorpse4,
    /// <summary>The frame the installed artwork names <c>dead_ripper_corpse_0</c>.</summary>
    DeadRipperCorpse0,
    /// <summary>The frame the installed artwork names <c>dead_ripper_corpse_2</c>.</summary>
    DeadRipperCorpse2,
    /// <summary>The frame the installed artwork names <c>dead_skree_corpse_0</c>.</summary>
    DeadSkreeCorpse0,
    /// <summary>The frame the installed artwork names <c>dead_skree_corpse_2</c>.</summary>
    DeadSkreeCorpse2,
    /// <summary>The frame the installed artwork names <c>dead_skree_corpse_4</c>.</summary>
    DeadSkreeCorpse4,
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
    MochtroidFlight0,
    /// <summary>The frame the installed artwork names <c>mochtroid_flight_1</c>.</summary>
    MochtroidFlight1,
    /// <summary>The frame the installed artwork names <c>mochtroid_flight_2</c>.</summary>
    MochtroidFlight2,
    /// <summary>The frame the installed artwork names <c>mochtroid_attached_0</c>.</summary>
    MochtroidAttached0,
    /// <summary>The frame the installed artwork names <c>mochtroid_attached_1</c>.</summary>
    MochtroidAttached1,
    /// <summary>The frame the installed artwork names <c>mochtroid_attached_2</c>.</summary>
    MochtroidAttached2,
    /// <summary>The frame the installed artwork names <c>evir_body_left_0</c>.</summary>
    EvirBodyLeft0,
    /// <summary>The frame the installed artwork names <c>evir_body_left_1</c>.</summary>
    EvirBodyLeft1,
    /// <summary>The frame the installed artwork names <c>evir_body_left_2</c>.</summary>
    EvirBodyLeft2,
    /// <summary>The frame the installed artwork names <c>evir_body_left_3</c>.</summary>
    EvirBodyLeft3,
    /// <summary>The frame the installed artwork names <c>evir_body_left_4</c>.</summary>
    EvirBodyLeft4,
    /// <summary>The frame the installed artwork names <c>evir_body_left_5</c>.</summary>
    EvirBodyLeft5,
    /// <summary>The frame the installed artwork names <c>evir_arms_left_0</c>.</summary>
    EvirArmsLeft0,
    /// <summary>The frame the installed artwork names <c>evir_arms_left_1</c>.</summary>
    EvirArmsLeft1,
    /// <summary>The frame the installed artwork names <c>evir_arms_left_2</c>.</summary>
    EvirArmsLeft2,
    /// <summary>The frame the installed artwork names <c>evir_arms_left_3</c>.</summary>
    EvirArmsLeft3,
    /// <summary>The frame the installed artwork names <c>evir_arms_left_4</c>.</summary>
    EvirArmsLeft4,
    /// <summary>The frame the installed artwork names <c>evir_body_right_0</c>.</summary>
    EvirBodyRight0,
    /// <summary>The frame the installed artwork names <c>evir_body_right_1</c>.</summary>
    EvirBodyRight1,
    /// <summary>The frame the installed artwork names <c>evir_body_right_2</c>.</summary>
    EvirBodyRight2,
    /// <summary>The frame the installed artwork names <c>evir_body_right_3</c>.</summary>
    EvirBodyRight3,
    /// <summary>The frame the installed artwork names <c>evir_body_right_4</c>.</summary>
    EvirBodyRight4,
    /// <summary>The frame the installed artwork names <c>evir_body_right_5</c>.</summary>
    EvirBodyRight5,
    /// <summary>The frame the installed artwork names <c>evir_arms_right_0</c>.</summary>
    EvirArmsRight0,
    /// <summary>The frame the installed artwork names <c>evir_arms_right_1</c>.</summary>
    EvirArmsRight1,
    /// <summary>The frame the installed artwork names <c>evir_arms_right_2</c>.</summary>
    EvirArmsRight2,
    /// <summary>The frame the installed artwork names <c>evir_arms_right_3</c>.</summary>
    EvirArmsRight3,
    /// <summary>The frame the installed artwork names <c>evir_arms_right_4</c>.</summary>
    EvirArmsRight4,
    /// <summary>The frame the installed artwork names <c>evir_projectile_flight</c>.</summary>
    EvirProjectileFlight,
    /// <summary>The frame the installed artwork names <c>evir_projectile_regenerating</c>.</summary>
    EvirProjectileRegenerating,
    /// <summary>The frame the installed artwork names <c>work_robot_left_00</c>.</summary>
    WorkRobotLeft00,
    /// <summary>The frame the installed artwork names <c>work_robot_left_01</c>.</summary>
    WorkRobotLeft01,
    /// <summary>The frame the installed artwork names <c>work_robot_left_02</c>.</summary>
    WorkRobotLeft02,
    /// <summary>The frame the installed artwork names <c>work_robot_left_03</c>.</summary>
    WorkRobotLeft03,
    /// <summary>The frame the installed artwork names <c>work_robot_left_04</c>.</summary>
    WorkRobotLeft04,
    /// <summary>The frame the installed artwork names <c>work_robot_left_05</c>.</summary>
    WorkRobotLeft05,
    /// <summary>The frame the installed artwork names <c>work_robot_left_06</c>.</summary>
    WorkRobotLeft06,
    /// <summary>The frame the installed artwork names <c>work_robot_left_07</c>.</summary>
    WorkRobotLeft07,
    /// <summary>The frame the installed artwork names <c>work_robot_left_08</c>.</summary>
    WorkRobotLeft08,
    /// <summary>The frame the installed artwork names <c>work_robot_left_09</c>.</summary>
    WorkRobotLeft09,
    /// <summary>The frame the installed artwork names <c>work_robot_left_10</c>.</summary>
    WorkRobotLeft10,
    /// <summary>The frame the installed artwork names <c>work_robot_left_11</c>.</summary>
    WorkRobotLeft11,
    /// <summary>The frame the installed artwork names <c>work_robot_right_00</c>.</summary>
    WorkRobotRight00,
    /// <summary>The frame the installed artwork names <c>work_robot_right_01</c>.</summary>
    WorkRobotRight01,
    /// <summary>The frame the installed artwork names <c>work_robot_right_02</c>.</summary>
    WorkRobotRight02,
    /// <summary>The frame the installed artwork names <c>work_robot_right_03</c>.</summary>
    WorkRobotRight03,
    /// <summary>The frame the installed artwork names <c>work_robot_right_04</c>.</summary>
    WorkRobotRight04,
    /// <summary>The frame the installed artwork names <c>work_robot_right_05</c>.</summary>
    WorkRobotRight05,
    /// <summary>The frame the installed artwork names <c>work_robot_right_06</c>.</summary>
    WorkRobotRight06,
    /// <summary>The frame the installed artwork names <c>work_robot_right_07</c>.</summary>
    WorkRobotRight07,
    /// <summary>The frame the installed artwork names <c>work_robot_right_08</c>.</summary>
    WorkRobotRight08,
    /// <summary>The frame the installed artwork names <c>work_robot_right_09</c>.</summary>
    WorkRobotRight09,
    /// <summary>The frame the installed artwork names <c>work_robot_right_10</c>.</summary>
    WorkRobotRight10,
    /// <summary>The frame the installed artwork names <c>work_robot_right_11</c>.</summary>
    WorkRobotRight11,
    /// <summary>The frame the installed artwork names <c>work_robot_unpowered_neutral</c>.</summary>
    WorkRobotUnpoweredNeutral,
    /// <summary>The frame the installed artwork names <c>work_robot_unpowered_left</c>.</summary>
    WorkRobotUnpoweredLeft,
    /// <summary>The frame the installed artwork names <c>work_robot_unpowered_right</c>.</summary>
    WorkRobotUnpoweredRight,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_right_up_00</c>.</summary>
    YardOutsideTurnRightUp00,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_right_up_01</c>.</summary>
    YardOutsideTurnRightUp01,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_right_up_02</c>.</summary>
    YardOutsideTurnRightUp02,
    /// <summary>The frame the installed artwork names <c>yard_crawl_top_left_00</c>.</summary>
    YardCrawlTopLeft00,
    /// <summary>The frame the installed artwork names <c>yard_crawl_top_left_01</c>.</summary>
    YardCrawlTopLeft01,
    /// <summary>The frame the installed artwork names <c>yard_crawl_top_left_02</c>.</summary>
    YardCrawlTopLeft02,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_top_left_00</c>.</summary>
    YardOutsideTurnTopLeft00,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_top_left_01</c>.</summary>
    YardOutsideTurnTopLeft01,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_top_left_02</c>.</summary>
    YardOutsideTurnTopLeft02,
    /// <summary>The frame the installed artwork names <c>yard_crawl_left_down_00</c>.</summary>
    YardCrawlLeftDown00,
    /// <summary>The frame the installed artwork names <c>yard_crawl_left_down_01</c>.</summary>
    YardCrawlLeftDown01,
    /// <summary>The frame the installed artwork names <c>yard_crawl_left_down_02</c>.</summary>
    YardCrawlLeftDown02,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_left_down_00</c>.</summary>
    YardOutsideTurnLeftDown00,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_left_down_01</c>.</summary>
    YardOutsideTurnLeftDown01,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_left_down_02</c>.</summary>
    YardOutsideTurnLeftDown02,
    /// <summary>The frame the installed artwork names <c>yard_crawl_bottom_right_00</c>.</summary>
    YardCrawlBottomRight00,
    /// <summary>The frame the installed artwork names <c>yard_crawl_bottom_right_01</c>.</summary>
    YardCrawlBottomRight01,
    /// <summary>The frame the installed artwork names <c>yard_crawl_bottom_right_02</c>.</summary>
    YardCrawlBottomRight02,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_bottom_right_00</c>.</summary>
    YardOutsideTurnBottomRight00,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_bottom_right_01</c>.</summary>
    YardOutsideTurnBottomRight01,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_bottom_right_02</c>.</summary>
    YardOutsideTurnBottomRight02,
    /// <summary>The frame the installed artwork names <c>yard_crawl_right_up_00</c>.</summary>
    YardCrawlRightUp00,
    /// <summary>The frame the installed artwork names <c>yard_crawl_right_up_01</c>.</summary>
    YardCrawlRightUp01,
    /// <summary>The frame the installed artwork names <c>yard_crawl_right_up_02</c>.</summary>
    YardCrawlRightUp02,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_left_up_00</c>.</summary>
    YardOutsideTurnLeftUp00,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_left_up_01</c>.</summary>
    YardOutsideTurnLeftUp01,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_left_up_02</c>.</summary>
    YardOutsideTurnLeftUp02,
    /// <summary>The frame the installed artwork names <c>yard_crawl_top_right_00</c>.</summary>
    YardCrawlTopRight00,
    /// <summary>The frame the installed artwork names <c>yard_crawl_top_right_01</c>.</summary>
    YardCrawlTopRight01,
    /// <summary>The frame the installed artwork names <c>yard_crawl_top_right_02</c>.</summary>
    YardCrawlTopRight02,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_top_right_00</c>.</summary>
    YardOutsideTurnTopRight00,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_top_right_01</c>.</summary>
    YardOutsideTurnTopRight01,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_top_right_02</c>.</summary>
    YardOutsideTurnTopRight02,
    /// <summary>The frame the installed artwork names <c>yard_crawl_right_down_00</c>.</summary>
    YardCrawlRightDown00,
    /// <summary>The frame the installed artwork names <c>yard_crawl_right_down_01</c>.</summary>
    YardCrawlRightDown01,
    /// <summary>The frame the installed artwork names <c>yard_crawl_right_down_02</c>.</summary>
    YardCrawlRightDown02,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_right_down_00</c>.</summary>
    YardOutsideTurnRightDown00,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_right_down_01</c>.</summary>
    YardOutsideTurnRightDown01,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_right_down_02</c>.</summary>
    YardOutsideTurnRightDown02,
    /// <summary>The frame the installed artwork names <c>yard_crawl_bottom_left_00</c>.</summary>
    YardCrawlBottomLeft00,
    /// <summary>The frame the installed artwork names <c>yard_crawl_bottom_left_01</c>.</summary>
    YardCrawlBottomLeft01,
    /// <summary>The frame the installed artwork names <c>yard_crawl_bottom_left_02</c>.</summary>
    YardCrawlBottomLeft02,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_bottom_left_00</c>.</summary>
    YardOutsideTurnBottomLeft00,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_bottom_left_01</c>.</summary>
    YardOutsideTurnBottomLeft01,
    /// <summary>The frame the installed artwork names <c>yard_outside_turn_bottom_left_02</c>.</summary>
    YardOutsideTurnBottomLeft02,
    /// <summary>The frame the installed artwork names <c>yard_crawl_left_up_00</c>.</summary>
    YardCrawlLeftUp00,
    /// <summary>The frame the installed artwork names <c>yard_crawl_left_up_01</c>.</summary>
    YardCrawlLeftUp01,
    /// <summary>The frame the installed artwork names <c>yard_crawl_left_up_02</c>.</summary>
    YardCrawlLeftUp02,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_top_left_00</c>.</summary>
    YardInsideTurnTopLeft00,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_top_left_01</c>.</summary>
    YardInsideTurnTopLeft01,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_top_left_02</c>.</summary>
    YardInsideTurnTopLeft02,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_right_up_00</c>.</summary>
    YardInsideTurnRightUp00,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_right_up_01</c>.</summary>
    YardInsideTurnRightUp01,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_right_up_02</c>.</summary>
    YardInsideTurnRightUp02,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_bottom_right_00</c>.</summary>
    YardInsideTurnBottomRight00,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_bottom_right_01</c>.</summary>
    YardInsideTurnBottomRight01,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_bottom_right_02</c>.</summary>
    YardInsideTurnBottomRight02,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_left_down_00</c>.</summary>
    YardInsideTurnLeftDown00,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_left_down_01</c>.</summary>
    YardInsideTurnLeftDown01,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_left_down_02</c>.</summary>
    YardInsideTurnLeftDown02,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_top_right_00</c>.</summary>
    YardInsideTurnTopRight00,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_top_right_01</c>.</summary>
    YardInsideTurnTopRight01,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_top_right_02</c>.</summary>
    YardInsideTurnTopRight02,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_left_up_00</c>.</summary>
    YardInsideTurnLeftUp00,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_left_up_01</c>.</summary>
    YardInsideTurnLeftUp01,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_left_up_02</c>.</summary>
    YardInsideTurnLeftUp02,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_bottom_left_00</c>.</summary>
    YardInsideTurnBottomLeft00,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_bottom_left_01</c>.</summary>
    YardInsideTurnBottomLeft01,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_bottom_left_02</c>.</summary>
    YardInsideTurnBottomLeft02,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_right_down_00</c>.</summary>
    YardInsideTurnRightDown00,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_right_down_01</c>.</summary>
    YardInsideTurnRightDown01,
    /// <summary>The frame the installed artwork names <c>yard_inside_turn_right_down_02</c>.</summary>
    YardInsideTurnRightDown02,
    /// <summary>The frame the installed artwork names <c>yard_hiding_top_left_00</c>.</summary>
    YardHidingTopLeft00,
    /// <summary>The frame the installed artwork names <c>yard_hiding_top_left_01</c>.</summary>
    YardHidingTopLeft01,
    /// <summary>The frame the installed artwork names <c>yard_hidden_top_left_00</c>.</summary>
    YardHiddenTopLeft00,
    /// <summary>The frame the installed artwork names <c>yard_hidden_top_left_01</c>.</summary>
    YardHiddenTopLeft01,
    /// <summary>The frame the installed artwork names <c>yard_hiding_bottom_left_00</c>.</summary>
    YardHidingBottomLeft00,
    /// <summary>The frame the installed artwork names <c>yard_hiding_bottom_left_01</c>.</summary>
    YardHidingBottomLeft01,
    /// <summary>The frame the installed artwork names <c>yard_hiding_bottom_left_02</c>.</summary>
    YardHidingBottomLeft02,
    /// <summary>The frame the installed artwork names <c>yard_hiding_bottom_left_03</c>.</summary>
    YardHidingBottomLeft03,
    /// <summary>The frame the installed artwork names <c>yard_hiding_bottom_right_00</c>.</summary>
    YardHidingBottomRight00,
    /// <summary>The frame the installed artwork names <c>yard_hiding_bottom_right_01</c>.</summary>
    YardHidingBottomRight01,
    /// <summary>The frame the installed artwork names <c>yard_hiding_bottom_right_02</c>.</summary>
    YardHidingBottomRight02,
    /// <summary>The frame the installed artwork names <c>yard_hiding_bottom_right_03</c>.</summary>
    YardHidingBottomRight03,
    /// <summary>The frame the installed artwork names <c>yard_hiding_top_right_00</c>.</summary>
    YardHidingTopRight00,
    /// <summary>The frame the installed artwork names <c>yard_hiding_top_right_01</c>.</summary>
    YardHidingTopRight01,
    /// <summary>The frame the installed artwork names <c>yard_hidden_top_right_00</c>.</summary>
    YardHiddenTopRight00,
    /// <summary>The frame the installed artwork names <c>yard_hidden_top_right_01</c>.</summary>
    YardHiddenTopRight01,
    /// <summary>The frame the installed artwork names <c>yard_hiding_right_up_00</c>.</summary>
    YardHidingRightUp00,
    /// <summary>The frame the installed artwork names <c>yard_hiding_right_up_01</c>.</summary>
    YardHidingRightUp01,
    /// <summary>The frame the installed artwork names <c>yard_hiding_right_up_02</c>.</summary>
    YardHidingRightUp02,
    /// <summary>The frame the installed artwork names <c>yard_hiding_right_up_03</c>.</summary>
    YardHidingRightUp03,
    /// <summary>The frame the installed artwork names <c>yard_hiding_left_up_00</c>.</summary>
    YardHidingLeftUp00,
    /// <summary>The frame the installed artwork names <c>yard_hiding_left_up_01</c>.</summary>
    YardHidingLeftUp01,
    /// <summary>The frame the installed artwork names <c>yard_hiding_left_up_02</c>.</summary>
    YardHidingLeftUp02,
    /// <summary>The frame the installed artwork names <c>yard_hiding_left_up_03</c>.</summary>
    YardHidingLeftUp03,
    /// <summary>The frame the installed artwork names <c>yard_hiding_left_down_00</c>.</summary>
    YardHidingLeftDown00,
    /// <summary>The frame the installed artwork names <c>yard_hiding_left_down_01</c>.</summary>
    YardHidingLeftDown01,
    /// <summary>The frame the installed artwork names <c>yard_hiding_left_down_02</c>.</summary>
    YardHidingLeftDown02,
    /// <summary>The frame the installed artwork names <c>yard_hiding_left_down_03</c>.</summary>
    YardHidingLeftDown03,
    /// <summary>The frame the installed artwork names <c>yard_hiding_right_down_00</c>.</summary>
    YardHidingRightDown00,
    /// <summary>The frame the installed artwork names <c>yard_hiding_right_down_01</c>.</summary>
    YardHidingRightDown01,
    /// <summary>The frame the installed artwork names <c>yard_hiding_right_down_02</c>.</summary>
    YardHidingRightDown02,
    /// <summary>The frame the installed artwork names <c>yard_hiding_right_down_03</c>.</summary>
    YardHidingRightDown03,
    /// <summary>The frame the installed artwork names <c>botwoon_head_00</c>.</summary>
    BotwoonHead00,
    /// <summary>The frame the installed artwork names <c>botwoon_head_01</c>.</summary>
    BotwoonHead01,
    /// <summary>The frame the installed artwork names <c>botwoon_head_02</c>.</summary>
    BotwoonHead02,
    /// <summary>The frame the installed artwork names <c>botwoon_head_03</c>.</summary>
    BotwoonHead03,
    /// <summary>The frame the installed artwork names <c>botwoon_head_04</c>.</summary>
    BotwoonHead04,
    /// <summary>The frame the installed artwork names <c>botwoon_head_05</c>.</summary>
    BotwoonHead05,
    /// <summary>The frame the installed artwork names <c>botwoon_head_06</c>.</summary>
    BotwoonHead06,
    /// <summary>The frame the installed artwork names <c>botwoon_head_07</c>.</summary>
    BotwoonHead07,
    /// <summary>The frame the installed artwork names <c>botwoon_head_08</c>.</summary>
    BotwoonHead08,
    /// <summary>The frame the installed artwork names <c>botwoon_head_09</c>.</summary>
    BotwoonHead09,
    /// <summary>The frame the installed artwork names <c>botwoon_head_10</c>.</summary>
    BotwoonHead10,
    /// <summary>The frame the installed artwork names <c>botwoon_head_11</c>.</summary>
    BotwoonHead11,
    /// <summary>The frame the installed artwork names <c>botwoon_head_12</c>.</summary>
    BotwoonHead12,
    /// <summary>The frame the installed artwork names <c>botwoon_head_13</c>.</summary>
    BotwoonHead13,
    /// <summary>The frame the installed artwork names <c>botwoon_head_14</c>.</summary>
    BotwoonHead14,
    /// <summary>The frame the installed artwork names <c>botwoon_head_15</c>.</summary>
    BotwoonHead15,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_00</c>.</summary>
    GunshipEntrance00,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_01</c>.</summary>
    GunshipEntrance01,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_02</c>.</summary>
    GunshipEntrance02,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_03</c>.</summary>
    GunshipEntrance03,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_04</c>.</summary>
    GunshipEntrance04,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_05</c>.</summary>
    GunshipEntrance05,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_06</c>.</summary>
    GunshipEntrance06,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_07</c>.</summary>
    GunshipEntrance07,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_08</c>.</summary>
    GunshipEntrance08,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_09</c>.</summary>
    GunshipEntrance09,
    /// <summary>The frame the installed artwork names <c>gunship_entrance_10</c>.</summary>
    GunshipEntrance10,
    /// <summary>The frame the installed artwork names <c>gunship_top_hull</c>.</summary>
    GunshipTopHull,
    /// <summary>The frame the installed artwork names <c>gunship_bottom_hull</c>.</summary>
    GunshipBottomHull,
    /// <summary>The frame the installed artwork names <c>tatori_00</c>.</summary>
    Tatori00,
    /// <summary>The frame the installed artwork names <c>tatori_01</c>.</summary>
    Tatori01,
    /// <summary>The frame the installed artwork names <c>tatori_02</c>.</summary>
    Tatori02,
    /// <summary>The frame the installed artwork names <c>tatori_03</c>.</summary>
    Tatori03,
    /// <summary>The frame the installed artwork names <c>tatori_04</c>.</summary>
    Tatori04,
    /// <summary>The frame the installed artwork names <c>tatori_05</c>.</summary>
    Tatori05,
    /// <summary>The frame the installed artwork names <c>tatori_06</c>.</summary>
    Tatori06,
    /// <summary>The frame the installed artwork names <c>tatori_07</c>.</summary>
    Tatori07,
    /// <summary>The frame the installed artwork names <c>tatori_08</c>.</summary>
    Tatori08,
    /// <summary>The frame the installed artwork names <c>tatori_09</c>.</summary>
    Tatori09,
    /// <summary>The frame the installed artwork names <c>tatori_10</c>.</summary>
    Tatori10,
    /// <summary>The frame the installed artwork names <c>tatori_11</c>.</summary>
    Tatori11,
    /// <summary>The frame the installed artwork names <c>tatori_12</c>.</summary>
    Tatori12,
    /// <summary>The frame the installed artwork names <c>tatori_13</c>.</summary>
    Tatori13,
    /// <summary>The frame the installed artwork names <c>tatori_14</c>.</summary>
    Tatori14,
    /// <summary>The frame the installed artwork names <c>tatori_15</c>.</summary>
    Tatori15,
    /// <summary>The frame the installed artwork names <c>tatori_16</c>.</summary>
    Tatori16,
    /// <summary>The frame the installed artwork names <c>tatori_17</c>.</summary>
    Tatori17,
    /// <summary>The frame the installed artwork names <c>tatori_18</c>.</summary>
    Tatori18,
    /// <summary>The frame the installed artwork names <c>tatori_19</c>.</summary>
    Tatori19,
    /// <summary>The frame the installed artwork names <c>tatori_20</c>.</summary>
    Tatori20,
    /// <summary>The frame the installed artwork names <c>tatori_21</c>.</summary>
    Tatori21,
    /// <summary>The frame the installed artwork names <c>tatori_22</c>.</summary>
    Tatori22,
    /// <summary>The frame the installed artwork names <c>tatori_23</c>.</summary>
    Tatori23,
    /// <summary>The frame the installed artwork names <c>tatori_24</c>.</summary>
    Tatori24,
    /// <summary>The frame the installed artwork names <c>tatori_25</c>.</summary>
    Tatori25,
    /// <summary>The frame the installed artwork names <c>tatori_26</c>.</summary>
    Tatori26,
    /// <summary>The frame the installed artwork names <c>tatori_27</c>.</summary>
    Tatori27,
    /// <summary>The frame the installed artwork names <c>tatori_28</c>.</summary>
    Tatori28,
    /// <summary>The frame the installed artwork names <c>zero_crawler_00</c>.</summary>
    ZeroCrawler00,
    /// <summary>The frame the installed artwork names <c>zero_crawler_01</c>.</summary>
    ZeroCrawler01,
    /// <summary>The frame the installed artwork names <c>zero_crawler_02</c>.</summary>
    ZeroCrawler02,
    /// <summary>The frame the installed artwork names <c>zero_crawler_03</c>.</summary>
    ZeroCrawler03,
    /// <summary>The frame the installed artwork names <c>zero_crawler_04</c>.</summary>
    ZeroCrawler04,
    /// <summary>The frame the installed artwork names <c>zero_crawler_05</c>.</summary>
    ZeroCrawler05,
    /// <summary>The frame the installed artwork names <c>zero_crawler_06</c>.</summary>
    ZeroCrawler06,
    /// <summary>The frame the installed artwork names <c>zero_crawler_07</c>.</summary>
    ZeroCrawler07,
    /// <summary>The frame the installed artwork names <c>zero_crawler_08</c>.</summary>
    ZeroCrawler08,
    /// <summary>The frame the installed artwork names <c>zero_crawler_09</c>.</summary>
    ZeroCrawler09,
    /// <summary>The frame the installed artwork names <c>zero_crawler_10</c>.</summary>
    ZeroCrawler10,
    /// <summary>The frame the installed artwork names <c>zero_crawler_11</c>.</summary>
    ZeroCrawler11,
    /// <summary>The frame the installed artwork names <c>zero_crawler_12</c>.</summary>
    ZeroCrawler12,
    /// <summary>The frame the installed artwork names <c>zero_crawler_13</c>.</summary>
    ZeroCrawler13,
    /// <summary>The frame the installed artwork names <c>zero_crawler_14</c>.</summary>
    ZeroCrawler14,
    /// <summary>The frame the installed artwork names <c>zero_crawler_15</c>.</summary>
    ZeroCrawler15,
    /// <summary>The frame the installed artwork names <c>etecoon_00</c>.</summary>
    Etecoon00,
    /// <summary>The frame the installed artwork names <c>etecoon_01</c>.</summary>
    Etecoon01,
    /// <summary>The frame the installed artwork names <c>etecoon_02</c>.</summary>
    Etecoon02,
    /// <summary>The frame the installed artwork names <c>etecoon_03</c>.</summary>
    Etecoon03,
    /// <summary>The frame the installed artwork names <c>etecoon_04</c>.</summary>
    Etecoon04,
    /// <summary>The frame the installed artwork names <c>etecoon_05</c>.</summary>
    Etecoon05,
    /// <summary>The frame the installed artwork names <c>etecoon_06</c>.</summary>
    Etecoon06,
    /// <summary>The frame the installed artwork names <c>etecoon_07</c>.</summary>
    Etecoon07,
    /// <summary>The frame the installed artwork names <c>etecoon_08</c>.</summary>
    Etecoon08,
    /// <summary>The frame the installed artwork names <c>etecoon_09</c>.</summary>
    Etecoon09,
    /// <summary>The frame the installed artwork names <c>etecoon_10</c>.</summary>
    Etecoon10,
    /// <summary>The frame the installed artwork names <c>etecoon_11</c>.</summary>
    Etecoon11,
    /// <summary>The frame the installed artwork names <c>etecoon_12</c>.</summary>
    Etecoon12,
    /// <summary>The frame the installed artwork names <c>etecoon_13</c>.</summary>
    Etecoon13,
    /// <summary>The frame the installed artwork names <c>etecoon_14</c>.</summary>
    Etecoon14,
    /// <summary>The frame the installed artwork names <c>etecoon_15</c>.</summary>
    Etecoon15,
    /// <summary>The frame the installed artwork names <c>etecoon_16</c>.</summary>
    Etecoon16,
    /// <summary>The frame the installed artwork names <c>etecoon_17</c>.</summary>
    Etecoon17,
    /// <summary>The frame the installed artwork names <c>etecoon_18</c>.</summary>
    Etecoon18,
    /// <summary>The frame the installed artwork names <c>etecoon_19</c>.</summary>
    Etecoon19,
    /// <summary>The frame the installed artwork names <c>etecoon_20</c>.</summary>
    Etecoon20,
    /// <summary>The frame the installed artwork names <c>etecoon_21</c>.</summary>
    Etecoon21,
    /// <summary>The frame the installed artwork names <c>etecoon_22</c>.</summary>
    Etecoon22,
    /// <summary>The frame the installed artwork names <c>etecoon_23</c>.</summary>
    Etecoon23,
    /// <summary>The frame the installed artwork names <c>etecoon_24</c>.</summary>
    Etecoon24,
    /// <summary>The frame the installed artwork names <c>etecoon_25</c>.</summary>
    Etecoon25,
    /// <summary>The frame the installed artwork names <c>etecoon_26</c>.</summary>
    Etecoon26,
    /// <summary>The frame the installed artwork names <c>etecoon_27</c>.</summary>
    Etecoon27,
    /// <summary>The frame the installed artwork names <c>etecoon_28</c>.</summary>
    Etecoon28,
    /// <summary>The frame the installed artwork names <c>etecoon_29</c>.</summary>
    Etecoon29,
    /// <summary>The frame the installed artwork names <c>etecoon_30</c>.</summary>
    Etecoon30,
    /// <summary>The frame the installed artwork names <c>dachora_00</c>.</summary>
    Dachora00,
    /// <summary>The frame the installed artwork names <c>dachora_01</c>.</summary>
    Dachora01,
    /// <summary>The frame the installed artwork names <c>dachora_02</c>.</summary>
    Dachora02,
    /// <summary>The frame the installed artwork names <c>dachora_03</c>.</summary>
    Dachora03,
    /// <summary>The frame the installed artwork names <c>dachora_04</c>.</summary>
    Dachora04,
    /// <summary>The frame the installed artwork names <c>dachora_05</c>.</summary>
    Dachora05,
    /// <summary>The frame the installed artwork names <c>dachora_06</c>.</summary>
    Dachora06,
    /// <summary>The frame the installed artwork names <c>dachora_07</c>.</summary>
    Dachora07,
    /// <summary>The frame the installed artwork names <c>dachora_08</c>.</summary>
    Dachora08,
    /// <summary>The frame the installed artwork names <c>dachora_09</c>.</summary>
    Dachora09,
    /// <summary>The frame the installed artwork names <c>dachora_10</c>.</summary>
    Dachora10,
    /// <summary>The frame the installed artwork names <c>dachora_11</c>.</summary>
    Dachora11,
    /// <summary>The frame the installed artwork names <c>dachora_12</c>.</summary>
    Dachora12,
    /// <summary>The frame the installed artwork names <c>dachora_13</c>.</summary>
    Dachora13,
    /// <summary>The frame the installed artwork names <c>dachora_14</c>.</summary>
    Dachora14,
    /// <summary>The frame the installed artwork names <c>dachora_15</c>.</summary>
    Dachora15,
    /// <summary>The frame the installed artwork names <c>dachora_16</c>.</summary>
    Dachora16,
    /// <summary>The frame the installed artwork names <c>dachora_17</c>.</summary>
    Dachora17,
    /// <summary>The frame the installed artwork names <c>dachora_18</c>.</summary>
    Dachora18,
    /// <summary>The frame the installed artwork names <c>dachora_19</c>.</summary>
    Dachora19,
    /// <summary>The frame the installed artwork names <c>dachora_20</c>.</summary>
    Dachora20,
    /// <summary>The frame the installed artwork names <c>dachora_21</c>.</summary>
    Dachora21,
    /// <summary>The frame the installed artwork names <c>dachora_22</c>.</summary>
    Dachora22,
    /// <summary>The frame the installed artwork names <c>dachora_23</c>.</summary>
    Dachora23,
    /// <summary>The frame the installed artwork names <c>dachora_24</c>.</summary>
    Dachora24,
    /// <summary>The frame the installed artwork names <c>dachora_25</c>.</summary>
    Dachora25,
    /// <summary>The frame the installed artwork names <c>dachora_26</c>.</summary>
    Dachora26,
    /// <summary>The frame the installed artwork names <c>dachora_27</c>.</summary>
    Dachora27,
    /// <summary>The frame the installed artwork names <c>dachora_28</c>.</summary>
    Dachora28,
    /// <summary>The frame the installed artwork names <c>escape_etecoon_00</c>.</summary>
    EscapeEtecoon00,
    /// <summary>The frame the installed artwork names <c>escape_etecoon_01</c>.</summary>
    EscapeEtecoon01,
    /// <summary>The frame the installed artwork names <c>escape_etecoon_02</c>.</summary>
    EscapeEtecoon02,
    /// <summary>The frame the installed artwork names <c>escape_etecoon_03</c>.</summary>
    EscapeEtecoon03,
    /// <summary>The frame the installed artwork names <c>escape_etecoon_04</c>.</summary>
    EscapeEtecoon04,
    /// <summary>The frame the installed artwork names <c>escape_etecoon_05</c>.</summary>
    EscapeEtecoon05,
    /// <summary>The frame the installed artwork names <c>escape_etecoon_06</c>.</summary>
    EscapeEtecoon06,
    /// <summary>The frame the installed artwork names <c>escape_etecoon_07</c>.</summary>
    EscapeEtecoon07,
    /// <summary>The frame the installed artwork names <c>escape_etecoon_08</c>.</summary>
    EscapeEtecoon08,
    /// <summary>The frame the installed artwork names <c>escape_etecoon_09</c>.</summary>
    EscapeEtecoon09,
    /// <summary>The frame the installed artwork names <c>escape_dachora_00</c>.</summary>
    EscapeDachora00,
    /// <summary>The frame the installed artwork names <c>escape_dachora_01</c>.</summary>
    EscapeDachora01,
    /// <summary>The frame the installed artwork names <c>escape_dachora_02</c>.</summary>
    EscapeDachora02,
    /// <summary>The frame the installed artwork names <c>escape_dachora_03</c>.</summary>
    EscapeDachora03,
    /// <summary>The frame the installed artwork names <c>escape_dachora_04</c>.</summary>
    EscapeDachora04,
    /// <summary>The frame the installed artwork names <c>escape_dachora_05</c>.</summary>
    EscapeDachora05,
    /// <summary>The frame the installed artwork names <c>escape_dachora_06</c>.</summary>
    EscapeDachora06,
    /// <summary>The frame the installed artwork names <c>escape_dachora_07</c>.</summary>
    EscapeDachora07,
    /// <summary>The frame the installed artwork names <c>escape_dachora_08</c>.</summary>
    EscapeDachora08,
    /// <summary>The frame the installed artwork names <c>escape_dachora_09</c>.</summary>
    EscapeDachora09,
    /// <summary>The frame the installed artwork names <c>escape_dachora_10</c>.</summary>
    EscapeDachora10,
    /// <summary>The frame the installed artwork names <c>escape_dachora_11</c>.</summary>
    EscapeDachora11,
    /// <summary>The frame the installed artwork names <c>hibashi_00</c>.</summary>
    Hibashi00,
    /// <summary>The frame the installed artwork names <c>hibashi_01</c>.</summary>
    Hibashi01,
    /// <summary>The frame the installed artwork names <c>hibashi_02</c>.</summary>
    Hibashi02,
    /// <summary>The frame the installed artwork names <c>hibashi_03</c>.</summary>
    Hibashi03,
    /// <summary>The frame the installed artwork names <c>hibashi_04</c>.</summary>
    Hibashi04,
    /// <summary>The frame the installed artwork names <c>hibashi_05</c>.</summary>
    Hibashi05,
    /// <summary>The frame the installed artwork names <c>hibashi_06</c>.</summary>
    Hibashi06,
    /// <summary>The frame the installed artwork names <c>hibashi_07</c>.</summary>
    Hibashi07,
    /// <summary>The frame the installed artwork names <c>hibashi_08</c>.</summary>
    Hibashi08,
    /// <summary>The frame the installed artwork names <c>hibashi_09</c>.</summary>
    Hibashi09,
    /// <summary>The frame the installed artwork names <c>hibashi_10</c>.</summary>
    Hibashi10,
    /// <summary>The frame the installed artwork names <c>hibashi_11</c>.</summary>
    Hibashi11,
    /// <summary>The frame the installed artwork names <c>hibashi_12</c>.</summary>
    Hibashi12,
    /// <summary>The frame the installed artwork names <c>hibashi_13</c>.</summary>
    Hibashi13,
    /// <summary>The frame the installed artwork names <c>hibashi_14</c>.</summary>
    Hibashi14,
    /// <summary>The frame the installed artwork names <c>hibashi_15</c>.</summary>
    Hibashi15,
    /// <summary>The frame the installed artwork names <c>hibashi_16</c>.</summary>
    Hibashi16,
    /// <summary>The frame the installed artwork names <c>hibashi_17</c>.</summary>
    Hibashi17,
    /// <summary>The frame the installed artwork names <c>hibashi_18</c>.</summary>
    Hibashi18,
    /// <summary>The frame the installed artwork names <c>hibashi_19</c>.</summary>
    Hibashi19,
    /// <summary>The frame the installed artwork names <c>hibashi_20</c>.</summary>
    Hibashi20,
    /// <summary>The frame the installed artwork names <c>hibashi_21</c>.</summary>
    Hibashi21,
    /// <summary>The frame the installed artwork names <c>hibashi_22</c>.</summary>
    Hibashi22,
    /// <summary>The frame the installed artwork names <c>zebetite_00</c>.</summary>
    Zebetite00,
    /// <summary>The frame the installed artwork names <c>zebetite_01</c>.</summary>
    Zebetite01,
    /// <summary>The frame the installed artwork names <c>zebetite_02</c>.</summary>
    Zebetite02,
    /// <summary>The frame the installed artwork names <c>zebetite_03</c>.</summary>
    Zebetite03,
    /// <summary>The frame the installed artwork names <c>zebetite_04</c>.</summary>
    Zebetite04,
    /// <summary>The frame the installed artwork names <c>zebetite_05</c>.</summary>
    Zebetite05,
    /// <summary>The frame the installed artwork names <c>zebetite_06</c>.</summary>
    Zebetite06,
    /// <summary>The frame the installed artwork names <c>zebetite_07</c>.</summary>
    Zebetite07,
    /// <summary>The frame the installed artwork names <c>zebetite_08</c>.</summary>
    Zebetite08,
    /// <summary>The frame the installed artwork names <c>zebetite_09</c>.</summary>
    Zebetite09,
    /// <summary>The frame the installed artwork names <c>wrecked_ship_ghost_00</c>.</summary>
    WreckedShipGhost00,
    /// <summary>The frame the installed artwork names <c>wrecked_ship_ghost_01</c>.</summary>
    WreckedShipGhost01,
    /// <summary>The frame the installed artwork names <c>wrecked_ship_ghost_02</c>.</summary>
    WreckedShipGhost02,
    /// <summary>The frame the installed artwork names <c>powamp_00</c>.</summary>
    Powamp00,
    /// <summary>The frame the installed artwork names <c>powamp_01</c>.</summary>
    Powamp01,
    /// <summary>The frame the installed artwork names <c>powamp_02</c>.</summary>
    Powamp02,
    /// <summary>The frame the installed artwork names <c>powamp_03</c>.</summary>
    Powamp03,
    /// <summary>The frame the installed artwork names <c>powamp_04</c>.</summary>
    Powamp04,
    /// <summary>The frame the installed artwork names <c>powamp_05</c>.</summary>
    Powamp05,
    /// <summary>The frame the installed artwork names <c>spark_00</c>.</summary>
    Spark00,
    /// <summary>The frame the installed artwork names <c>spark_01</c>.</summary>
    Spark01,
    /// <summary>The frame the installed artwork names <c>spark_02</c>.</summary>
    Spark02,
    /// <summary>The frame the installed artwork names <c>spark_03</c>.</summary>
    Spark03,
    /// <summary>The frame the installed artwork names <c>spark_04</c>.</summary>
    Spark04,
    /// <summary>The frame the installed artwork names <c>spark_05</c>.</summary>
    Spark05,
    /// <summary>The frame the installed artwork names <c>spark_06</c>.</summary>
    Spark06,
    /// <summary>The frame the installed artwork names <c>spark_07</c>.</summary>
    Spark07,
    /// <summary>The frame the installed artwork names <c>shitroid_00</c>.</summary>
    Shitroid00,
    /// <summary>The frame the installed artwork names <c>shitroid_01</c>.</summary>
    Shitroid01,
    /// <summary>The frame the installed artwork names <c>shitroid_02</c>.</summary>
    Shitroid02,
    /// <summary>The frame the installed artwork names <c>kraid_lint_00</c>.</summary>
    KraidLint00,
    /// <summary>The frame the installed artwork names <c>kraid_lint_01</c>.</summary>
    KraidLint01,
    /// <summary>The frame the installed artwork names <c>nuclear_waffle_00</c>.</summary>
    NuclearWaffle00,
    /// <summary>The frame the installed artwork names <c>nuclear_waffle_01</c>.</summary>
    NuclearWaffle01,
    /// <summary>The frame the installed artwork names <c>nuclear_waffle_02</c>.</summary>
    NuclearWaffle02,
    /// <summary>The frame the installed artwork names <c>nuclear_waffle_03</c>.</summary>
    NuclearWaffle03,
    /// <summary>The frame the installed artwork names <c>nuclear_waffle_04</c>.</summary>
    NuclearWaffle04,
    /// <summary>The frame the installed artwork names <c>nuclear_waffle_05</c>.</summary>
    NuclearWaffle05,
    /// <summary>The frame the installed artwork names <c>nuclear_waffle_06</c>.</summary>
    NuclearWaffle06,
    /// <summary>The frame the installed artwork names <c>nuclear_waffle_07</c>.</summary>
    NuclearWaffle07,
    /// <summary>The frame the installed artwork names <c>kzan_00</c>.</summary>
    Kzan00,
    /// <summary>The frame the installed artwork names <c>polyp_00</c>.</summary>
    Polyp00,
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
    WalkingPirateWalkLeft0,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_left_1</c>.</summary>
    WalkingPirateWalkLeft1,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_left_2</c>.</summary>
    WalkingPirateWalkLeft2,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_left_3</c>.</summary>
    WalkingPirateWalkLeft3,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_left_4</c>.</summary>
    WalkingPirateWalkLeft4,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_left_5</c>.</summary>
    WalkingPirateWalkLeft5,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_left_6</c>.</summary>
    WalkingPirateWalkLeft6,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_left_7</c>.</summary>
    WalkingPirateWalkLeft7,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_left_0</c>.</summary>
    WalkingPirateFireLeft0,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_left_1</c>.</summary>
    WalkingPirateFireLeft1,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_left_2</c>.</summary>
    WalkingPirateFireLeft2,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_left_3</c>.</summary>
    WalkingPirateFireLeft3,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_left_4</c>.</summary>
    WalkingPirateFireLeft4,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_left_5</c>.</summary>
    WalkingPirateFireLeft5,
    /// <summary>The frame the installed artwork names <c>walking_pirate_look_left_0</c>.</summary>
    WalkingPirateLookLeft0,
    /// <summary>The frame the installed artwork names <c>walking_pirate_look_left_1</c>.</summary>
    WalkingPirateLookLeft1,
    /// <summary>The frame the installed artwork names <c>walking_pirate_look_left_2</c>.</summary>
    WalkingPirateLookLeft2,
    /// <summary>The frame the installed artwork names <c>walking_pirate_look_shared</c>.</summary>
    WalkingPirateLookShared,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_right_0</c>.</summary>
    WalkingPirateWalkRight0,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_right_1</c>.</summary>
    WalkingPirateWalkRight1,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_right_2</c>.</summary>
    WalkingPirateWalkRight2,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_right_3</c>.</summary>
    WalkingPirateWalkRight3,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_right_4</c>.</summary>
    WalkingPirateWalkRight4,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_right_5</c>.</summary>
    WalkingPirateWalkRight5,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_right_6</c>.</summary>
    WalkingPirateWalkRight6,
    /// <summary>The frame the installed artwork names <c>walking_pirate_walk_right_7</c>.</summary>
    WalkingPirateWalkRight7,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_right_0</c>.</summary>
    WalkingPirateFireRight0,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_right_1</c>.</summary>
    WalkingPirateFireRight1,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_right_2</c>.</summary>
    WalkingPirateFireRight2,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_right_3</c>.</summary>
    WalkingPirateFireRight3,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_right_4</c>.</summary>
    WalkingPirateFireRight4,
    /// <summary>The frame the installed artwork names <c>walking_pirate_fire_right_5</c>.</summary>
    WalkingPirateFireRight5,
    /// <summary>The frame the installed artwork names <c>walking_pirate_look_right_0</c>.</summary>
    WalkingPirateLookRight0,
    /// <summary>The frame the installed artwork names <c>walking_pirate_look_right_1</c>.</summary>
    WalkingPirateLookRight1,
    /// <summary>The frame the installed artwork names <c>walking_pirate_look_right_2</c>.</summary>
    WalkingPirateLookRight2,
    /// <summary>The frame the installed artwork names <c>wall_pirate_fire_jump_left_0</c>.</summary>
    WallPirateFireJumpLeft0,
    /// <summary>The frame the installed artwork names <c>wall_pirate_fire_jump_left_1</c>.</summary>
    WallPirateFireJumpLeft1,
    /// <summary>The frame the installed artwork names <c>wall_pirate_fire_jump_left_2</c>.</summary>
    WallPirateFireJumpLeft2,
    /// <summary>The frame the installed artwork names <c>wall_pirate_fire_jump_left_3</c>.</summary>
    WallPirateFireJumpLeft3,
    /// <summary>The frame the installed artwork names <c>wall_pirate_climb_left_0</c>.</summary>
    WallPirateClimbLeft0,
    /// <summary>The frame the installed artwork names <c>wall_pirate_climb_left_1</c>.</summary>
    WallPirateClimbLeft1,
    /// <summary>The frame the installed artwork names <c>wall_pirate_climb_left_2</c>.</summary>
    WallPirateClimbLeft2,
    /// <summary>The frame the installed artwork names <c>wall_pirate_climb_left_3</c>.</summary>
    WallPirateClimbLeft3,
    /// <summary>The frame the installed artwork names <c>wall_pirate_climb_left_4</c>.</summary>
    WallPirateClimbLeft4,
    /// <summary>The frame the installed artwork names <c>wall_pirate_fire_jump_right_0</c>.</summary>
    WallPirateFireJumpRight0,
    /// <summary>The frame the installed artwork names <c>wall_pirate_fire_jump_right_1</c>.</summary>
    WallPirateFireJumpRight1,
    /// <summary>The frame the installed artwork names <c>wall_pirate_fire_jump_right_2</c>.</summary>
    WallPirateFireJumpRight2,
    /// <summary>The frame the installed artwork names <c>wall_pirate_fire_jump_right_3</c>.</summary>
    WallPirateFireJumpRight3,
    /// <summary>The frame the installed artwork names <c>wall_pirate_climb_right_0</c>.</summary>
    WallPirateClimbRight0,
    /// <summary>The frame the installed artwork names <c>wall_pirate_climb_right_1</c>.</summary>
    WallPirateClimbRight1,
    /// <summary>The frame the installed artwork names <c>wall_pirate_climb_right_2</c>.</summary>
    WallPirateClimbRight2,
    /// <summary>The frame the installed artwork names <c>wall_pirate_climb_right_3</c>.</summary>
    WallPirateClimbRight3,
    /// <summary>The frame the installed artwork names <c>wall_pirate_climb_right_4</c>.</summary>
    WallPirateClimbRight4,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_0</c>.</summary>
    NinjaPiratePose0,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_1</c>.</summary>
    NinjaPiratePose1,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_2</c>.</summary>
    NinjaPiratePose2,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_3</c>.</summary>
    NinjaPiratePose3,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_4</c>.</summary>
    NinjaPiratePose4,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_5</c>.</summary>
    NinjaPiratePose5,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_6</c>.</summary>
    NinjaPiratePose6,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_7</c>.</summary>
    NinjaPiratePose7,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_8</c>.</summary>
    NinjaPiratePose8,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_17</c>.</summary>
    NinjaPiratePose17,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_18</c>.</summary>
    NinjaPiratePose18,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_19</c>.</summary>
    NinjaPiratePose19,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_20</c>.</summary>
    NinjaPiratePose20,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_21</c>.</summary>
    NinjaPiratePose21,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_22</c>.</summary>
    NinjaPiratePose22,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_23</c>.</summary>
    NinjaPiratePose23,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_24</c>.</summary>
    NinjaPiratePose24,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_49</c>.</summary>
    NinjaPiratePose49,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_50</c>.</summary>
    NinjaPiratePose50,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_51</c>.</summary>
    NinjaPiratePose51,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_55</c>.</summary>
    NinjaPiratePose55,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_65</c>.</summary>
    NinjaPiratePose65,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_57</c>.</summary>
    NinjaPiratePose57,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_58</c>.</summary>
    NinjaPiratePose58,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_33</c>.</summary>
    NinjaPiratePose33,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_34</c>.</summary>
    NinjaPiratePose34,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_35</c>.</summary>
    NinjaPiratePose35,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_36</c>.</summary>
    NinjaPiratePose36,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_37</c>.</summary>
    NinjaPiratePose37,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_38</c>.</summary>
    NinjaPiratePose38,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_39</c>.</summary>
    NinjaPiratePose39,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_40</c>.</summary>
    NinjaPiratePose40,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_69</c>.</summary>
    NinjaPiratePose69,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_70</c>.</summary>
    NinjaPiratePose70,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_71</c>.</summary>
    NinjaPiratePose71,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_75</c>.</summary>
    NinjaPiratePose75,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_73</c>.</summary>
    NinjaPiratePose73,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_52</c>.</summary>
    NinjaPiratePose52,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_53</c>.</summary>
    NinjaPiratePose53,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_54</c>.</summary>
    NinjaPiratePose54,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_61</c>.</summary>
    NinjaPiratePose61,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_67</c>.</summary>
    NinjaPiratePose67,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_62</c>.</summary>
    NinjaPiratePose62,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_9</c>.</summary>
    NinjaPiratePose9,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_10</c>.</summary>
    NinjaPiratePose10,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_11</c>.</summary>
    NinjaPiratePose11,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_12</c>.</summary>
    NinjaPiratePose12,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_13</c>.</summary>
    NinjaPiratePose13,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_14</c>.</summary>
    NinjaPiratePose14,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_15</c>.</summary>
    NinjaPiratePose15,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_16</c>.</summary>
    NinjaPiratePose16,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_25</c>.</summary>
    NinjaPiratePose25,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_26</c>.</summary>
    NinjaPiratePose26,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_27</c>.</summary>
    NinjaPiratePose27,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_28</c>.</summary>
    NinjaPiratePose28,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_29</c>.</summary>
    NinjaPiratePose29,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_30</c>.</summary>
    NinjaPiratePose30,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_31</c>.</summary>
    NinjaPiratePose31,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_32</c>.</summary>
    NinjaPiratePose32,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_56</c>.</summary>
    NinjaPiratePose56,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_66</c>.</summary>
    NinjaPiratePose66,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_59</c>.</summary>
    NinjaPiratePose59,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_60</c>.</summary>
    NinjaPiratePose60,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_41</c>.</summary>
    NinjaPiratePose41,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_42</c>.</summary>
    NinjaPiratePose42,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_43</c>.</summary>
    NinjaPiratePose43,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_44</c>.</summary>
    NinjaPiratePose44,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_45</c>.</summary>
    NinjaPiratePose45,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_46</c>.</summary>
    NinjaPiratePose46,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_47</c>.</summary>
    NinjaPiratePose47,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_48</c>.</summary>
    NinjaPiratePose48,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_72</c>.</summary>
    NinjaPiratePose72,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_74</c>.</summary>
    NinjaPiratePose74,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_63</c>.</summary>
    NinjaPiratePose63,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_68</c>.</summary>
    NinjaPiratePose68,
    /// <summary>The frame the installed artwork names <c>ninja_pirate_pose_64</c>.</summary>
    NinjaPiratePose64,
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
    DraygonFrame0,
    /// <summary>The frame the installed artwork names <c>draygon_frame_1</c>.</summary>
    DraygonFrame1,
    /// <summary>The frame the installed artwork names <c>draygon_frame_2</c>.</summary>
    DraygonFrame2,
    /// <summary>The frame the installed artwork names <c>draygon_frame_3</c>.</summary>
    DraygonFrame3,
    /// <summary>The frame the installed artwork names <c>draygon_frame_4</c>.</summary>
    DraygonFrame4,
    /// <summary>The frame the installed artwork names <c>draygon_frame_5</c>.</summary>
    DraygonFrame5,
    /// <summary>The frame the installed artwork names <c>draygon_frame_23</c>.</summary>
    DraygonFrame23,
    /// <summary>The frame the installed artwork names <c>draygon_frame_24</c>.</summary>
    DraygonFrame24,
    /// <summary>The frame the installed artwork names <c>draygon_frame_25</c>.</summary>
    DraygonFrame25,
    /// <summary>The frame the installed artwork names <c>draygon_frame_26</c>.</summary>
    DraygonFrame26,
    /// <summary>The frame the installed artwork names <c>draygon_frame_27</c>.</summary>
    DraygonFrame27,
    /// <summary>The frame the installed artwork names <c>draygon_frame_28</c>.</summary>
    DraygonFrame28,
    /// <summary>The frame the installed artwork names <c>draygon_frame_29</c>.</summary>
    DraygonFrame29,
    /// <summary>The frame the installed artwork names <c>draygon_frame_30</c>.</summary>
    DraygonFrame30,
    /// <summary>The frame the installed artwork names <c>draygon_frame_31</c>.</summary>
    DraygonFrame31,
    /// <summary>The frame the installed artwork names <c>draygon_frame_32</c>.</summary>
    DraygonFrame32,
    /// <summary>The frame the installed artwork names <c>draygon_frame_33</c>.</summary>
    DraygonFrame33,
    /// <summary>The frame the installed artwork names <c>draygon_frame_34</c>.</summary>
    DraygonFrame34,
    /// <summary>The frame the installed artwork names <c>draygon_frame_35</c>.</summary>
    DraygonFrame35,
    /// <summary>The frame the installed artwork names <c>draygon_frame_36</c>.</summary>
    DraygonFrame36,
    /// <summary>The frame the installed artwork names <c>draygon_frame_37</c>.</summary>
    DraygonFrame37,
    /// <summary>The frame the installed artwork names <c>draygon_frame_38</c>.</summary>
    DraygonFrame38,
    /// <summary>The frame the installed artwork names <c>draygon_frame_39</c>.</summary>
    DraygonFrame39,
    /// <summary>The frame the installed artwork names <c>draygon_frame_40</c>.</summary>
    DraygonFrame40,
    /// <summary>The frame the installed artwork names <c>draygon_frame_41</c>.</summary>
    DraygonFrame41,
    /// <summary>The frame the installed artwork names <c>draygon_frame_42</c>.</summary>
    DraygonFrame42,
    /// <summary>The frame the installed artwork names <c>draygon_frame_43</c>.</summary>
    DraygonFrame43,
    /// <summary>The frame the installed artwork names <c>draygon_frame_44</c>.</summary>
    DraygonFrame44,
    /// <summary>The frame the installed artwork names <c>draygon_frame_45</c>.</summary>
    DraygonFrame45,
    /// <summary>The frame the installed artwork names <c>draygon_frame_46</c>.</summary>
    DraygonFrame46,
    /// <summary>The frame the installed artwork names <c>draygon_frame_64</c>.</summary>
    DraygonFrame64,
    /// <summary>The frame the installed artwork names <c>draygon_frame_65</c>.</summary>
    DraygonFrame65,
    /// <summary>The frame the installed artwork names <c>draygon_frame_66</c>.</summary>
    DraygonFrame66,
    /// <summary>The frame the installed artwork names <c>draygon_frame_67</c>.</summary>
    DraygonFrame67,
    /// <summary>The frame the installed artwork names <c>draygon_frame_68</c>.</summary>
    DraygonFrame68,
    /// <summary>The frame the installed artwork names <c>draygon_frame_69</c>.</summary>
    DraygonFrame69,
    /// <summary>The frame the installed artwork names <c>draygon_frame_70</c>.</summary>
    DraygonFrame70,
    /// <summary>The frame the installed artwork names <c>draygon_frame_71</c>.</summary>
    DraygonFrame71,
    /// <summary>The frame the installed artwork names <c>draygon_frame_72</c>.</summary>
    DraygonFrame72,
    /// <summary>The frame the installed artwork names <c>draygon_frame_73</c>.</summary>
    DraygonFrame73,
    /// <summary>The frame the installed artwork names <c>draygon_frame_74</c>.</summary>
    DraygonFrame74,
    /// <summary>The frame the installed artwork names <c>draygon_frame_75</c>.</summary>
    DraygonFrame75,
    /// <summary>The frame the installed artwork names <c>draygon_frame_76</c>.</summary>
    DraygonFrame76,
    /// <summary>The frame the installed artwork names <c>draygon_frame_77</c>.</summary>
    DraygonFrame77,
    /// <summary>The frame the installed artwork names <c>draygon_frame_78</c>.</summary>
    DraygonFrame78,
    /// <summary>The frame the installed artwork names <c>draygon_frame_79</c>.</summary>
    DraygonFrame79,
    /// <summary>The frame the installed artwork names <c>draygon_frame_80</c>.</summary>
    DraygonFrame80,
    /// <summary>The frame the installed artwork names <c>draygon_frame_81</c>.</summary>
    DraygonFrame81,
    /// <summary>The frame the installed artwork names <c>spore_spawn_dead_0</c>.</summary>
    SporeSpawnDead0,
    /// <summary>The frame the installed artwork names <c>spore_spawn_closed_0</c>.</summary>
    SporeSpawnClosed0,
    /// <summary>The frame the installed artwork names <c>spore_spawn_opening_0</c>.</summary>
    SporeSpawnOpening0,
    /// <summary>The frame the installed artwork names <c>spore_spawn_opening_1</c>.</summary>
    SporeSpawnOpening1,
    /// <summary>The frame the installed artwork names <c>spore_spawn_opening_2</c>.</summary>
    SporeSpawnOpening2,
    /// <summary>The frame the installed artwork names <c>spore_spawn_opening_3</c>.</summary>
    SporeSpawnOpening3,
    /// <summary>The frame the installed artwork names <c>spore_spawn_opening_4</c>.</summary>
    SporeSpawnOpening4,
    /// <summary>The frame the installed artwork names <c>spore_spawn_opening_5</c>.</summary>
    SporeSpawnOpening5,
    /// <summary>The frame the installed artwork names <c>spore_spawn_opening_6</c>.</summary>
    SporeSpawnOpening6,
    /// <summary>The frame the installed artwork names <c>spore_spawn_open_0</c>.</summary>
    SporeSpawnOpen0,
    /// <summary>The frame the installed artwork names <c>spore_spawn_open_1</c>.</summary>
    SporeSpawnOpen1,
    /// <summary>The frame the installed artwork names <c>spore_spawn_open_2</c>.</summary>
    SporeSpawnOpen2,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_0</c>.</summary>
    CeresSteamPose0,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_1</c>.</summary>
    CeresSteamPose1,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_2</c>.</summary>
    CeresSteamPose2,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_3</c>.</summary>
    CeresSteamPose3,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_4</c>.</summary>
    CeresSteamPose4,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_5</c>.</summary>
    CeresSteamPose5,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_6</c>.</summary>
    CeresSteamPose6,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_7</c>.</summary>
    CeresSteamPose7,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_8</c>.</summary>
    CeresSteamPose8,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_9</c>.</summary>
    CeresSteamPose9,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_10</c>.</summary>
    CeresSteamPose10,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_11</c>.</summary>
    CeresSteamPose11,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_12</c>.</summary>
    CeresSteamPose12,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_13</c>.</summary>
    CeresSteamPose13,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_14</c>.</summary>
    CeresSteamPose14,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_15</c>.</summary>
    CeresSteamPose15,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_16</c>.</summary>
    CeresSteamPose16,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_17</c>.</summary>
    CeresSteamPose17,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_18</c>.</summary>
    CeresSteamPose18,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_19</c>.</summary>
    CeresSteamPose19,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_20</c>.</summary>
    CeresSteamPose20,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_21</c>.</summary>
    CeresSteamPose21,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_22</c>.</summary>
    CeresSteamPose22,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_23</c>.</summary>
    CeresSteamPose23,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_24</c>.</summary>
    CeresSteamPose24,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_25</c>.</summary>
    CeresSteamPose25,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_26</c>.</summary>
    CeresSteamPose26,
    /// <summary>The frame the installed artwork names <c>ceres_steam_pose_27</c>.</summary>
    CeresSteamPose27,
    /// <summary>The frame the installed artwork names <c>oum_pose_0</c>.</summary>
    OumPose0,
    /// <summary>The frame the installed artwork names <c>oum_pose_1</c>.</summary>
    OumPose1,
    /// <summary>The frame the installed artwork names <c>oum_pose_2</c>.</summary>
    OumPose2,
    /// <summary>The frame the installed artwork names <c>oum_pose_3</c>.</summary>
    OumPose3,
    /// <summary>The frame the installed artwork names <c>oum_pose_4</c>.</summary>
    OumPose4,
    /// <summary>The frame the installed artwork names <c>oum_pose_5</c>.</summary>
    OumPose5,
    /// <summary>The frame the installed artwork names <c>oum_pose_6</c>.</summary>
    OumPose6,
    /// <summary>The frame the installed artwork names <c>oum_pose_7</c>.</summary>
    OumPose7,
    /// <summary>The frame the installed artwork names <c>oum_pose_8</c>.</summary>
    OumPose8,
    /// <summary>The frame the installed artwork names <c>oum_pose_9</c>.</summary>
    OumPose9,
    /// <summary>The frame the installed artwork names <c>oum_pose_10</c>.</summary>
    OumPose10,
    /// <summary>The frame the installed artwork names <c>oum_pose_11</c>.</summary>
    OumPose11,
    /// <summary>The frame the installed artwork names <c>oum_pose_12</c>.</summary>
    OumPose12,
    /// <summary>The frame the installed artwork names <c>oum_pose_13</c>.</summary>
    OumPose13,
    /// <summary>The frame the installed artwork names <c>oum_pose_14</c>.</summary>
    OumPose14,
    /// <summary>The frame the installed artwork names <c>oum_pose_15</c>.</summary>
    OumPose15,
    /// <summary>The frame the installed artwork names <c>oum_pose_16</c>.</summary>
    OumPose16,
    /// <summary>The frame the installed artwork names <c>oum_pose_17</c>.</summary>
    OumPose17,
    /// <summary>The frame the installed artwork names <c>oum_pose_18</c>.</summary>
    OumPose18,
    /// <summary>The frame the installed artwork names <c>oum_pose_19</c>.</summary>
    OumPose19,
    /// <summary>The frame the installed artwork names <c>oum_pose_20</c>.</summary>
    OumPose20,
    /// <summary>The frame the installed artwork names <c>oum_pose_21</c>.</summary>
    OumPose21,
    /// <summary>The frame the installed artwork names <c>oum_pose_22</c>.</summary>
    OumPose22,
    /// <summary>The frame the installed artwork names <c>oum_pose_23</c>.</summary>
    OumPose23,
    /// <summary>The frame the installed artwork names <c>oum_pose_24</c>.</summary>
    OumPose24,
    /// <summary>The frame the installed artwork names <c>oum_pose_25</c>.</summary>
    OumPose25,
    /// <summary>The frame the installed artwork names <c>oum_pose_26</c>.</summary>
    OumPose26,
    /// <summary>The frame the installed artwork names <c>oum_pose_27</c>.</summary>
    OumPose27,
    /// <summary>The frame the installed artwork names <c>oum_pose_28</c>.</summary>
    OumPose28,
    /// <summary>The frame the installed artwork names <c>oum_pose_29</c>.</summary>
    OumPose29,
    /// <summary>The frame the installed artwork names <c>crocomire_tongue_fight_0</c>.</summary>
    CrocomireTongueFight0,
    /// <summary>The frame the installed artwork names <c>crocomire_tongue_fight_1</c>.</summary>
    CrocomireTongueFight1,
    /// <summary>The frame the installed artwork names <c>crocomire_tongue_fight_2</c>.</summary>
    CrocomireTongueFight2,
    /// <summary>The frame the installed artwork names <c>crocomire_tongue_fight_3</c>.</summary>
    CrocomireTongueFight3,
    /// <summary>The frame the installed artwork names <c>crocomire_tongue_melting_pose_0</c>.</summary>
    CrocomireTongueMeltingPose0,
    /// <summary>The frame the installed artwork names <c>crocomire_tongue_melting_pose_1</c>.</summary>
    CrocomireTongueMeltingPose1,
    /// <summary>The frame the installed artwork names <c>crocomire_tongue_melting_pose_2</c>.</summary>
    CrocomireTongueMeltingPose2,
    /// <summary>The frame the installed artwork names <c>crocomire_tongue_melting_pose_3</c>.</summary>
    CrocomireTongueMeltingPose3,
    /// <summary>The frame the installed artwork names <c>crocomire_tongue_melting_pose_4</c>.</summary>
    CrocomireTongueMeltingPose4,
    /// <summary>The frame the installed artwork names <c>crocomire_body_0</c>.</summary>
    CrocomireBody0,
    /// <summary>The frame the installed artwork names <c>crocomire_body_1</c>.</summary>
    CrocomireBody1,
    /// <summary>The frame the installed artwork names <c>crocomire_body_2</c>.</summary>
    CrocomireBody2,
    /// <summary>The frame the installed artwork names <c>crocomire_body_3</c>.</summary>
    CrocomireBody3,
    /// <summary>The frame the installed artwork names <c>crocomire_body_4</c>.</summary>
    CrocomireBody4,
    /// <summary>The frame the installed artwork names <c>crocomire_body_5</c>.</summary>
    CrocomireBody5,
    /// <summary>The frame the installed artwork names <c>crocomire_body_6</c>.</summary>
    CrocomireBody6,
    /// <summary>The frame the installed artwork names <c>crocomire_body_7</c>.</summary>
    CrocomireBody7,
    /// <summary>The frame the installed artwork names <c>crocomire_body_8</c>.</summary>
    CrocomireBody8,
    /// <summary>The frame the installed artwork names <c>crocomire_body_9</c>.</summary>
    CrocomireBody9,
    /// <summary>The frame the installed artwork names <c>crocomire_body_10</c>.</summary>
    CrocomireBody10,
    /// <summary>The frame the installed artwork names <c>crocomire_body_11</c>.</summary>
    CrocomireBody11,
    /// <summary>The frame the installed artwork names <c>crocomire_body_12</c>.</summary>
    CrocomireBody12,
    /// <summary>The frame the installed artwork names <c>crocomire_body_13</c>.</summary>
    CrocomireBody13,
    /// <summary>The frame the installed artwork names <c>crocomire_body_14</c>.</summary>
    CrocomireBody14,
    /// <summary>The frame the installed artwork names <c>crocomire_body_15</c>.</summary>
    CrocomireBody15,
    /// <summary>The frame the installed artwork names <c>crocomire_body_16</c>.</summary>
    CrocomireBody16,
    /// <summary>The frame the installed artwork names <c>crocomire_body_17</c>.</summary>
    CrocomireBody17,
    /// <summary>The frame the installed artwork names <c>crocomire_body_18</c>.</summary>
    CrocomireBody18,
    /// <summary>The frame the installed artwork names <c>crocomire_body_19</c>.</summary>
    CrocomireBody19,
    /// <summary>The frame the installed artwork names <c>crocomire_body_20</c>.</summary>
    CrocomireBody20,
    /// <summary>The frame the installed artwork names <c>crocomire_body_21</c>.</summary>
    CrocomireBody21,
    /// <summary>The frame the installed artwork names <c>crocomire_body_22</c>.</summary>
    CrocomireBody22,
    /// <summary>The frame the installed artwork names <c>crocomire_body_23</c>.</summary>
    CrocomireBody23,
    /// <summary>The frame the installed artwork names <c>crocomire_body_24</c>.</summary>
    CrocomireBody24,
    /// <summary>The frame the installed artwork names <c>crocomire_body_25</c>.</summary>
    CrocomireBody25,
    /// <summary>The frame the installed artwork names <c>crocomire_body_26</c>.</summary>
    CrocomireBody26,
    /// <summary>The frame the installed artwork names <c>crocomire_body_27</c>.</summary>
    CrocomireBody27,
    /// <summary>The frame the installed artwork names <c>crocomire_body_28</c>.</summary>
    CrocomireBody28,
    /// <summary>The frame the installed artwork names <c>crocomire_body_29</c>.</summary>
    CrocomireBody29,
    /// <summary>The frame the installed artwork names <c>crocomire_body_30</c>.</summary>
    CrocomireBody30,
    /// <summary>The frame the installed artwork names <c>crocomire_body_31</c>.</summary>
    CrocomireBody31,
    /// <summary>The frame the installed artwork names <c>crocomire_body_32</c>.</summary>
    CrocomireBody32,
    /// <summary>The frame the installed artwork names <c>crocomire_body_33</c>.</summary>
    CrocomireBody33,
    /// <summary>The frame the installed artwork names <c>crocomire_body_34</c>.</summary>
    CrocomireBody34,
    /// <summary>The frame the installed artwork names <c>crocomire_body_35</c>.</summary>
    CrocomireBody35,
    /// <summary>The frame the installed artwork names <c>crocomire_body_36</c>.</summary>
    CrocomireBody36,
    /// <summary>The frame the installed artwork names <c>crocomire_body_37</c>.</summary>
    CrocomireBody37,
    /// <summary>The frame the installed artwork names <c>crocomire_body_38</c>.</summary>
    CrocomireBody38,
    /// <summary>The frame the installed artwork names <c>crocomire_body_39</c>.</summary>
    CrocomireBody39,
    /// <summary>The frame the installed artwork names <c>crocomire_body_40</c>.</summary>
    CrocomireBody40,
    /// <summary>The frame the installed artwork names <c>crocomire_body_41</c>.</summary>
    CrocomireBody41,
    /// <summary>The frame the installed artwork names <c>crocomire_body_42</c>.</summary>
    CrocomireBody42,
    /// <summary>The frame the installed artwork names <c>crocomire_body_43</c>.</summary>
    CrocomireBody43,
    /// <summary>The frame the installed artwork names <c>crocomire_body_44</c>.</summary>
    CrocomireBody44,
    /// <summary>The frame the installed artwork names <c>crocomire_body_45</c>.</summary>
    CrocomireBody45,
    /// <summary>The frame the installed artwork names <c>crocomire_body_46</c>.</summary>
    CrocomireBody46,
    /// <summary>The frame the installed artwork names <c>crocomire_body_47</c>.</summary>
    CrocomireBody47,
    /// <summary>The frame the installed artwork names <c>crocomire_body_48</c>.</summary>
    CrocomireBody48,
    /// <summary>The frame the installed artwork names <c>crocomire_body_49</c>.</summary>
    CrocomireBody49,
    /// <summary>The frame the installed artwork names <c>bomb_torizo_dormant</c>.</summary>
    BombTorizoDormant,
    /// <summary>The frame the installed artwork names <c>golden_torizo_initial</c>.</summary>
    GoldenTorizoInitial,
    /// <summary>The frame the installed artwork names <c>kraid_arm_0</c>.</summary>
    KraidArm0,
    /// <summary>The frame the installed artwork names <c>kraid_arm_1</c>.</summary>
    KraidArm1,
    /// <summary>The frame the installed artwork names <c>kraid_arm_2</c>.</summary>
    KraidArm2,
    /// <summary>The frame the installed artwork names <c>kraid_arm_3</c>.</summary>
    KraidArm3,
    /// <summary>The frame the installed artwork names <c>kraid_arm_4</c>.</summary>
    KraidArm4,
    /// <summary>The frame the installed artwork names <c>kraid_arm_5</c>.</summary>
    KraidArm5,
    /// <summary>The frame the installed artwork names <c>kraid_arm_6</c>.</summary>
    KraidArm6,
    /// <summary>The frame the installed artwork names <c>kraid_arm_7</c>.</summary>
    KraidArm7,
    /// <summary>The frame the installed artwork names <c>kraid_arm_8</c>.</summary>
    KraidArm8,
    /// <summary>The frame the installed artwork names <c>kraid_arm_9</c>.</summary>
    KraidArm9,
    /// <summary>The frame the installed artwork names <c>kraid_arm_10</c>.</summary>
    KraidArm10,
    /// <summary>The frame the installed artwork names <c>kraid_arm_11</c>.</summary>
    KraidArm11,
    /// <summary>The frame the installed artwork names <c>kraid_arm_12</c>.</summary>
    KraidArm12,
    /// <summary>The frame the installed artwork names <c>kraid_arm_13</c>.</summary>
    KraidArm13,
    /// <summary>The frame the installed artwork names <c>kraid_arm_14</c>.</summary>
    KraidArm14,
    /// <summary>The frame the installed artwork names <c>kraid_arm_15</c>.</summary>
    KraidArm15,
    /// <summary>The frame the installed artwork names <c>kraid_arm_16</c>.</summary>
    KraidArm16,
    /// <summary>The frame the installed artwork names <c>kraid_arm_17</c>.</summary>
    KraidArm17,
    /// <summary>The frame the installed artwork names <c>kraid_arm_18</c>.</summary>
    KraidArm18,
    /// <summary>The frame the installed artwork names <c>kraid_arm_19</c>.</summary>
    KraidArm19,
    /// <summary>The frame the installed artwork names <c>kraid_arm_20</c>.</summary>
    KraidArm20,
    /// <summary>The frame the installed artwork names <c>kraid_arm_21</c>.</summary>
    KraidArm21,
    /// <summary>The frame the installed artwork names <c>golden_torizo_awake_0</c>.</summary>
    GoldenTorizoAwake0,
    /// <summary>The frame the installed artwork names <c>golden_torizo_awake_1</c>.</summary>
    GoldenTorizoAwake1,
    /// <summary>The frame the installed artwork names <c>golden_torizo_awake_2</c>.</summary>
    GoldenTorizoAwake2,
    /// <summary>The frame the installed artwork names <c>golden_torizo_awake_3</c>.</summary>
    GoldenTorizoAwake3,
    /// <summary>The frame the installed artwork names <c>golden_torizo_awake_4</c>.</summary>
    GoldenTorizoAwake4,
    /// <summary>The frame the installed artwork names <c>golden_torizo_awake_5</c>.</summary>
    GoldenTorizoAwake5,
    /// <summary>The frame the installed artwork names <c>golden_torizo_walk_left_0</c>.</summary>
    GoldenTorizoWalkLeft0,
    /// <summary>The frame the installed artwork names <c>golden_torizo_walk_left_1</c>.</summary>
    GoldenTorizoWalkLeft1,
    /// <summary>The frame the installed artwork names <c>golden_torizo_walk_left_2</c>.</summary>
    GoldenTorizoWalkLeft2,
    /// <summary>The frame the installed artwork names <c>golden_torizo_walk_left_3</c>.</summary>
    GoldenTorizoWalkLeft3,
    /// <summary>The frame the installed artwork names <c>golden_torizo_walk_left_4</c>.</summary>
    GoldenTorizoWalkLeft4,
    /// <summary>The frame the installed artwork names <c>golden_torizo_walk_left_5</c>.</summary>
    GoldenTorizoWalkLeft5,
    /// <summary>The frame the installed artwork names <c>golden_torizo_walk_left_6</c>.</summary>
    GoldenTorizoWalkLeft6,
    /// <summary>The frame the installed artwork names <c>golden_torizo_walk_left_7</c>.</summary>
    GoldenTorizoWalkLeft7,
    /// <summary>The frame the installed artwork names <c>golden_torizo_walk_left_8</c>.</summary>
    GoldenTorizoWalkLeft8,
    /// <summary>The frame the installed artwork names <c>golden_torizo_walk_left_9</c>.</summary>
    GoldenTorizoWalkLeft9,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_0</c>.</summary>
    GoldenTorizoRightward0,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_1</c>.</summary>
    GoldenTorizoRightward1,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_2</c>.</summary>
    GoldenTorizoRightward2,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_3</c>.</summary>
    GoldenTorizoRightward3,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_4</c>.</summary>
    GoldenTorizoRightward4,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_5</c>.</summary>
    GoldenTorizoRightward5,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_6</c>.</summary>
    GoldenTorizoRightward6,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_7</c>.</summary>
    GoldenTorizoRightward7,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_8</c>.</summary>
    GoldenTorizoRightward8,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_9</c>.</summary>
    GoldenTorizoRightward9,
    /// <summary>The frame the installed artwork names <c>golden_torizo_rightward_10</c>.</summary>
    GoldenTorizoRightward10,
    /// <summary>The frame the installed artwork names <c>torizo_jump_back_0</c>.</summary>
    TorizoJumpBack0,
    /// <summary>The frame the installed artwork names <c>torizo_jump_back_1</c>.</summary>
    TorizoJumpBack1,
    /// <summary>The frame the installed artwork names <c>torizo_jump_back_2</c>.</summary>
    TorizoJumpBack2,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_orb_0</c>.</summary>
    GoldenTorizoRightOrb0,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_orb_1</c>.</summary>
    GoldenTorizoRightOrb1,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_orb_2</c>.</summary>
    GoldenTorizoRightOrb2,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_orb_3</c>.</summary>
    GoldenTorizoRightOrb3,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_orb_4</c>.</summary>
    GoldenTorizoRightOrb4,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_orb_5</c>.</summary>
    GoldenTorizoRightOrb5,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_0</c>.</summary>
    GoldenTorizoRightSonic0,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_1</c>.</summary>
    GoldenTorizoRightSonic1,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_2</c>.</summary>
    GoldenTorizoRightSonic2,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_3</c>.</summary>
    GoldenTorizoRightSonic3,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_4</c>.</summary>
    GoldenTorizoRightSonic4,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_5</c>.</summary>
    GoldenTorizoRightSonic5,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_6</c>.</summary>
    GoldenTorizoRightSonic6,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_7</c>.</summary>
    GoldenTorizoRightSonic7,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_8</c>.</summary>
    GoldenTorizoRightSonic8,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_9</c>.</summary>
    GoldenTorizoRightSonic9,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_10</c>.</summary>
    GoldenTorizoRightSonic10,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_11</c>.</summary>
    GoldenTorizoRightSonic11,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_12</c>.</summary>
    GoldenTorizoRightSonic12,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_13</c>.</summary>
    GoldenTorizoRightSonic13,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_14</c>.</summary>
    GoldenTorizoRightSonic14,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_15</c>.</summary>
    GoldenTorizoRightSonic15,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_16</c>.</summary>
    GoldenTorizoRightSonic16,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_17</c>.</summary>
    GoldenTorizoRightSonic17,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_18</c>.</summary>
    GoldenTorizoRightSonic18,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_19</c>.</summary>
    GoldenTorizoRightSonic19,
    /// <summary>The frame the installed artwork names <c>golden_torizo_right_sonic_20</c>.</summary>
    GoldenTorizoRightSonic20,
    /// <summary>The frame the installed artwork names <c>torizo_falling_left</c>.</summary>
    TorizoFallingLeft,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_foot_orb_0</c>.</summary>
    GoldenTorizoLeftFootOrb0,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_foot_orb_1</c>.</summary>
    GoldenTorizoLeftFootOrb1,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_foot_orb_2</c>.</summary>
    GoldenTorizoLeftFootOrb2,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_foot_orb_3</c>.</summary>
    GoldenTorizoLeftFootOrb3,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_foot_orb_4</c>.</summary>
    GoldenTorizoLeftFootOrb4,
    /// <summary>The frame the installed artwork names <c>torizo_jump_back_left_0</c>.</summary>
    TorizoJumpBackLeft0,
    /// <summary>The frame the installed artwork names <c>torizo_jump_back_left_1</c>.</summary>
    TorizoJumpBackLeft1,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_0</c>.</summary>
    GoldenTorizoLeftOrb0,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_1</c>.</summary>
    GoldenTorizoLeftOrb1,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_2</c>.</summary>
    GoldenTorizoLeftOrb2,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_3</c>.</summary>
    GoldenTorizoLeftOrb3,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_4</c>.</summary>
    GoldenTorizoLeftOrb4,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_5</c>.</summary>
    GoldenTorizoLeftOrb5,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_6</c>.</summary>
    GoldenTorizoLeftOrb6,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_7</c>.</summary>
    GoldenTorizoLeftOrb7,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_8</c>.</summary>
    GoldenTorizoLeftOrb8,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_9</c>.</summary>
    GoldenTorizoLeftOrb9,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_10</c>.</summary>
    GoldenTorizoLeftOrb10,
    /// <summary>The frame the installed artwork names <c>golden_torizo_left_orb_11</c>.</summary>
    GoldenTorizoLeftOrb11,
    /// <summary>The frame the installed artwork names <c>torizo_combat_0</c>.</summary>
    TorizoCombat0,
    /// <summary>The frame the installed artwork names <c>torizo_combat_1</c>.</summary>
    TorizoCombat1,
    /// <summary>The frame the installed artwork names <c>torizo_combat_2</c>.</summary>
    TorizoCombat2,
    /// <summary>The frame the installed artwork names <c>torizo_combat_3</c>.</summary>
    TorizoCombat3,
    /// <summary>The frame the installed artwork names <c>torizo_combat_4</c>.</summary>
    TorizoCombat4,
    /// <summary>The frame the installed artwork names <c>torizo_combat_5</c>.</summary>
    TorizoCombat5,
    /// <summary>The frame the installed artwork names <c>torizo_combat_6</c>.</summary>
    TorizoCombat6,
    /// <summary>The frame the installed artwork names <c>torizo_combat_7</c>.</summary>
    TorizoCombat7,
    /// <summary>The frame the installed artwork names <c>torizo_combat_8</c>.</summary>
    TorizoCombat8,
    /// <summary>The frame the installed artwork names <c>torizo_combat_9</c>.</summary>
    TorizoCombat9,
    /// <summary>The frame the installed artwork names <c>torizo_combat_10</c>.</summary>
    TorizoCombat10,
    /// <summary>The frame the installed artwork names <c>torizo_combat_11</c>.</summary>
    TorizoCombat11,
    /// <summary>The frame the installed artwork names <c>torizo_combat_12</c>.</summary>
    TorizoCombat12,
    /// <summary>The frame the installed artwork names <c>torizo_combat_13</c>.</summary>
    TorizoCombat13,
    /// <summary>The frame the installed artwork names <c>torizo_combat_14</c>.</summary>
    TorizoCombat14,
    /// <summary>The frame the installed artwork names <c>torizo_combat_15</c>.</summary>
    TorizoCombat15,
    /// <summary>The frame the installed artwork names <c>torizo_combat_16</c>.</summary>
    TorizoCombat16,
    /// <summary>The frame the installed artwork names <c>torizo_combat_17</c>.</summary>
    TorizoCombat17,
    /// <summary>The frame the installed artwork names <c>torizo_combat_18</c>.</summary>
    TorizoCombat18,
    /// <summary>The frame the installed artwork names <c>torizo_combat_19</c>.</summary>
    TorizoCombat19,
    /// <summary>The frame the installed artwork names <c>torizo_combat_20</c>.</summary>
    TorizoCombat20,
    /// <summary>The frame the installed artwork names <c>torizo_combat_21</c>.</summary>
    TorizoCombat21,
    /// <summary>The frame the installed artwork names <c>torizo_combat_22</c>.</summary>
    TorizoCombat22,
    /// <summary>The frame the installed artwork names <c>torizo_combat_23</c>.</summary>
    TorizoCombat23,
    /// <summary>The frame the installed artwork names <c>torizo_combat_24</c>.</summary>
    TorizoCombat24,
    /// <summary>The frame the installed artwork names <c>torizo_combat_25</c>.</summary>
    TorizoCombat25,
    /// <summary>The frame the installed artwork names <c>torizo_combat_26</c>.</summary>
    TorizoCombat26,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_standing</c>.</summary>
    MotherBrainBodyStanding,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_walking_0</c>.</summary>
    MotherBrainBodyWalking0,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_walking_1</c>.</summary>
    MotherBrainBodyWalking1,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_walking_2</c>.</summary>
    MotherBrainBodyWalking2,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_walking_3</c>.</summary>
    MotherBrainBodyWalking3,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_walking_4</c>.</summary>
    MotherBrainBodyWalking4,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_walking_5</c>.</summary>
    MotherBrainBodyWalking5,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_walking_6</c>.</summary>
    MotherBrainBodyWalking6,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_walking_7</c>.</summary>
    MotherBrainBodyWalking7,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_crouched</c>.</summary>
    MotherBrainBodyCrouched,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_uncrouching</c>.</summary>
    MotherBrainBodyUncrouching,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_leaning_down</c>.</summary>
    MotherBrainBodyLeaningDown,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_initial_dummy</c>.</summary>
    MotherBrainBodyInitialDummy,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_death_beam_0</c>.</summary>
    MotherBrainBodyDeathBeam0,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_death_beam_1</c>.</summary>
    MotherBrainBodyDeathBeam1,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_death_beam_2</c>.</summary>
    MotherBrainBodyDeathBeam2,
    /// <summary>The frame the installed artwork names <c>mother_brain_body_oam_death_beam_3</c>.</summary>
    MotherBrainBodyDeathBeam3,
    /// <summary>The frame the installed artwork names <c>phantoon_invulnerable_body_0</c>.</summary>
    PhantoonInvulnerableBody0,
    /// <summary>The frame the installed artwork names <c>phantoon_full_hitbox_body_0</c>.</summary>
    PhantoonFullHitboxBody0,
    /// <summary>The frame the installed artwork names <c>phantoon_eye_hitbox_body_0</c>.</summary>
    PhantoonEyeHitboxBody0,
    /// <summary>The frame the installed artwork names <c>phantoon_closed_eye_0</c>.</summary>
    PhantoonClosedEye0,
    /// <summary>The frame the installed artwork names <c>phantoon_opening_eye_0</c>.</summary>
    PhantoonOpeningEye0,
    /// <summary>The frame the installed artwork names <c>phantoon_opening_eye_1</c>.</summary>
    PhantoonOpeningEye1,
    /// <summary>The frame the installed artwork names <c>phantoon_opening_eye_2</c>.</summary>
    PhantoonOpeningEye2,
    /// <summary>The frame the installed artwork names <c>phantoon_centered_eye_0</c>.</summary>
    PhantoonCenteredEye0,
    /// <summary>The frame the installed artwork names <c>phantoon_gaze_0</c>.</summary>
    PhantoonGaze0,
    /// <summary>The frame the installed artwork names <c>phantoon_gaze_4</c>.</summary>
    PhantoonGaze4,
    /// <summary>The frame the installed artwork names <c>phantoon_gaze_6</c>.</summary>
    PhantoonGaze6,
    /// <summary>The frame the installed artwork names <c>phantoon_gaze_2</c>.</summary>
    PhantoonGaze2,
    /// <summary>The frame the installed artwork names <c>phantoon_gaze_5</c>.</summary>
    PhantoonGaze5,
    /// <summary>The frame the installed artwork names <c>phantoon_gaze_3</c>.</summary>
    PhantoonGaze3,
    /// <summary>The frame the installed artwork names <c>phantoon_gaze_7</c>.</summary>
    PhantoonGaze7,
    /// <summary>The frame the installed artwork names <c>phantoon_gaze_1</c>.</summary>
    PhantoonGaze1,
    /// <summary>The frame the installed artwork names <c>phantoon_tentacle_0</c>.</summary>
    PhantoonTentacle0,
    /// <summary>The frame the installed artwork names <c>phantoon_tentacle_1</c>.</summary>
    PhantoonTentacle1,
    /// <summary>The frame the installed artwork names <c>phantoon_tentacle_2</c>.</summary>
    PhantoonTentacle2,
    /// <summary>The frame the installed artwork names <c>phantoon_mouth_0</c>.</summary>
    PhantoonMouth0,
    /// <summary>The frame the installed artwork names <c>phantoon_mouth_1</c>.</summary>
    PhantoonMouth1,
    /// <summary>The frame the installed artwork names <c>phantoon_mouth_2</c>.</summary>
    PhantoonMouth2,
    /// <summary>The frame the installed artwork names <c>draygon_frame_6</c>.</summary>
    DraygonFrame6,
    /// <summary>The frame the installed artwork names <c>draygon_frame_7</c>.</summary>
    DraygonFrame7,
    /// <summary>The frame the installed artwork names <c>draygon_frame_8</c>.</summary>
    DraygonFrame8,
    /// <summary>The frame the installed artwork names <c>draygon_frame_9</c>.</summary>
    DraygonFrame9,
    /// <summary>The frame the installed artwork names <c>draygon_frame_10</c>.</summary>
    DraygonFrame10,
    /// <summary>The frame the installed artwork names <c>draygon_frame_11</c>.</summary>
    DraygonFrame11,
    /// <summary>The frame the installed artwork names <c>draygon_frame_12</c>.</summary>
    DraygonFrame12,
    /// <summary>The frame the installed artwork names <c>draygon_frame_13</c>.</summary>
    DraygonFrame13,
    /// <summary>The frame the installed artwork names <c>draygon_frame_14</c>.</summary>
    DraygonFrame14,
    /// <summary>The frame the installed artwork names <c>draygon_frame_15</c>.</summary>
    DraygonFrame15,
    /// <summary>The frame the installed artwork names <c>draygon_frame_16</c>.</summary>
    DraygonFrame16,
    /// <summary>The frame the installed artwork names <c>draygon_frame_17</c>.</summary>
    DraygonFrame17,
    /// <summary>The frame the installed artwork names <c>draygon_frame_18</c>.</summary>
    DraygonFrame18,
    /// <summary>The frame the installed artwork names <c>draygon_frame_19</c>.</summary>
    DraygonFrame19,
    /// <summary>The frame the installed artwork names <c>draygon_frame_20</c>.</summary>
    DraygonFrame20,
    /// <summary>The frame the installed artwork names <c>draygon_frame_21</c>.</summary>
    DraygonFrame21,
    /// <summary>The frame the installed artwork names <c>draygon_frame_22</c>.</summary>
    DraygonFrame22,
    /// <summary>The frame the installed artwork names <c>draygon_frame_47</c>.</summary>
    DraygonFrame47,
    /// <summary>The frame the installed artwork names <c>draygon_frame_48</c>.</summary>
    DraygonFrame48,
    /// <summary>The frame the installed artwork names <c>draygon_frame_49</c>.</summary>
    DraygonFrame49,
    /// <summary>The frame the installed artwork names <c>draygon_frame_50</c>.</summary>
    DraygonFrame50,
    /// <summary>The frame the installed artwork names <c>draygon_frame_51</c>.</summary>
    DraygonFrame51,
    /// <summary>The frame the installed artwork names <c>draygon_frame_52</c>.</summary>
    DraygonFrame52,
    /// <summary>The frame the installed artwork names <c>draygon_frame_53</c>.</summary>
    DraygonFrame53,
    /// <summary>The frame the installed artwork names <c>draygon_frame_54</c>.</summary>
    DraygonFrame54,
    /// <summary>The frame the installed artwork names <c>draygon_frame_55</c>.</summary>
    DraygonFrame55,
    /// <summary>The frame the installed artwork names <c>draygon_frame_56</c>.</summary>
    DraygonFrame56,
    /// <summary>The frame the installed artwork names <c>draygon_frame_57</c>.</summary>
    DraygonFrame57,
    /// <summary>The frame the installed artwork names <c>draygon_frame_58</c>.</summary>
    DraygonFrame58,
    /// <summary>The frame the installed artwork names <c>draygon_frame_59</c>.</summary>
    DraygonFrame59,
    /// <summary>The frame the installed artwork names <c>draygon_frame_60</c>.</summary>
    DraygonFrame60,
    /// <summary>The frame the installed artwork names <c>draygon_frame_61</c>.</summary>
    DraygonFrame61,
    /// <summary>The frame the installed artwork names <c>draygon_frame_62</c>.</summary>
    DraygonFrame62,
    /// <summary>The frame the installed artwork names <c>draygon_frame_63</c>.</summary>
    DraygonFrame63,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_0</c>.</summary>
    CrocomireSkeleton0,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_1</c>.</summary>
    CrocomireSkeleton1,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_2</c>.</summary>
    CrocomireSkeleton2,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_3</c>.</summary>
    CrocomireSkeleton3,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_4</c>.</summary>
    CrocomireSkeleton4,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_5</c>.</summary>
    CrocomireSkeleton5,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_6</c>.</summary>
    CrocomireSkeleton6,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_7</c>.</summary>
    CrocomireSkeleton7,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_8</c>.</summary>
    CrocomireSkeleton8,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_9</c>.</summary>
    CrocomireSkeleton9,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_10</c>.</summary>
    CrocomireSkeleton10,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_11</c>.</summary>
    CrocomireSkeleton11,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_12</c>.</summary>
    CrocomireSkeleton12,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_13</c>.</summary>
    CrocomireSkeleton13,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_14</c>.</summary>
    CrocomireSkeleton14,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_15</c>.</summary>
    CrocomireSkeleton15,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_16</c>.</summary>
    CrocomireSkeleton16,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_17</c>.</summary>
    CrocomireSkeleton17,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_18</c>.</summary>
    CrocomireSkeleton18,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_19</c>.</summary>
    CrocomireSkeleton19,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_20</c>.</summary>
    CrocomireSkeleton20,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_21</c>.</summary>
    CrocomireSkeleton21,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_22</c>.</summary>
    CrocomireSkeleton22,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_23</c>.</summary>
    CrocomireSkeleton23,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_24</c>.</summary>
    CrocomireSkeleton24,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_25</c>.</summary>
    CrocomireSkeleton25,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_26</c>.</summary>
    CrocomireSkeleton26,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_27</c>.</summary>
    CrocomireSkeleton27,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_28</c>.</summary>
    CrocomireSkeleton28,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_29</c>.</summary>
    CrocomireSkeleton29,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_30</c>.</summary>
    CrocomireSkeleton30,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_31</c>.</summary>
    CrocomireSkeleton31,
    /// <summary>The frame the installed artwork names <c>crocomire_skeleton_32</c>.</summary>
    CrocomireSkeleton32,
    /// <summary>The frame the installed artwork names <c>kraid_foot_0</c>.</summary>
    KraidFoot0,
    /// <summary>The frame the installed artwork names <c>kraid_foot_1</c>.</summary>
    KraidFoot1,
    /// <summary>The frame the installed artwork names <c>kraid_foot_2</c>.</summary>
    KraidFoot2,
    /// <summary>The frame the installed artwork names <c>kraid_foot_3</c>.</summary>
    KraidFoot3,
    /// <summary>The frame the installed artwork names <c>kraid_foot_4</c>.</summary>
    KraidFoot4,
    /// <summary>The frame the installed artwork names <c>kraid_foot_5</c>.</summary>
    KraidFoot5,
    /// <summary>The frame the installed artwork names <c>kraid_foot_6</c>.</summary>
    KraidFoot6,
    /// <summary>The frame the installed artwork names <c>kraid_foot_7</c>.</summary>
    KraidFoot7,
    /// <summary>The frame the installed artwork names <c>kraid_foot_8</c>.</summary>
    KraidFoot8,
    /// <summary>The frame the installed artwork names <c>kraid_foot_9</c>.</summary>
    KraidFoot9,
    /// <summary>The frame the installed artwork names <c>kraid_foot_10</c>.</summary>
    KraidFoot10,
    /// <summary>The frame the installed artwork names <c>kraid_foot_11</c>.</summary>
    KraidFoot11,
    /// <summary>The frame the installed artwork names <c>kraid_foot_12</c>.</summary>
    KraidFoot12,
    /// <summary>The frame the installed artwork names <c>kraid_foot_13</c>.</summary>
    KraidFoot13,
    /// <summary>The frame the installed artwork names <c>kraid_foot_14</c>.</summary>
    KraidFoot14,
    /// <summary>The frame the installed artwork names <c>kraid_foot_15</c>.</summary>
    KraidFoot15,
    /// <summary>The frame the installed artwork names <c>kraid_foot_16</c>.</summary>
    KraidFoot16,
    /// <summary>The frame the installed artwork names <c>kraid_foot_17</c>.</summary>
    KraidFoot17,
    /// <summary>The frame the installed artwork names <c>kraid_foot_18</c>.</summary>
    KraidFoot18,
    /// <summary>The frame the installed artwork names <c>kraid_foot_19</c>.</summary>
    KraidFoot19,
    /// <summary>The frame the installed artwork names <c>kraid_foot_20</c>.</summary>
    KraidFoot20,
    /// <summary>The frame the installed artwork names <c>kraid_foot_21</c>.</summary>
    KraidFoot21,
    /// <summary>The frame the installed artwork names <c>kraid_foot_22</c>.</summary>
    KraidFoot22,
    /// <summary>The frame the installed artwork names <c>kraid_foot_23</c>.</summary>
    KraidFoot23,
    /// <summary>The frame the installed artwork names <c>kraid_foot_24</c>.</summary>
    KraidFoot24,
    /// <summary>The frame the installed artwork names <c>kraid_foot_25</c>.</summary>
    KraidFoot25,
    /// <summary>The frame the installed artwork names <c>kraid_foot_26</c>.</summary>
    KraidFoot26,
    /// <summary>The frame the installed artwork names <c>kraid_foot_27</c>.</summary>
    KraidFoot27,
    /// <summary>The frame the installed artwork names <c>kraid_foot_28</c>.</summary>
    KraidFoot28,
    /// <summary>The frame the installed artwork names <c>kraid_foot_29</c>.</summary>
    KraidFoot29,
    /// <summary>The frame the installed artwork names <c>kraid_foot_30</c>.</summary>
    KraidFoot30,
    /// <summary>The frame the installed artwork names <c>kraid_foot_31</c>.</summary>
    KraidFoot31,
    /// <summary>The frame the installed artwork names <c>kraid_foot_32</c>.</summary>
    KraidFoot32,
    /// <summary>The frame the installed artwork names <c>kraid_foot_33</c>.</summary>
    KraidFoot33,
    /// <summary>The frame the installed artwork names <c>kraid_foot_standing</c>.</summary>
    KraidFootStanding,
    /// <summary>
    /// The empty frame every enemy family shares: an enemy showing it draws nothing, and one that collides by its frames'
    /// hitboxes collides at the single point at its origin.
    /// </summary>
    Empty,
}
