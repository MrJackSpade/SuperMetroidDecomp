namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The frames an enemy shows: every installed spritemap and extended frame in schema order, and the shared empty frames
/// every enemy bank repeats. Each is named for its owner and its place among that owner's frames (the pose, index or part
/// its frame table selects it by); each cites its installed composition, whose name also gives its installed sheet key
/// (<see cref="EnemyFrameSheetKeys"/>), so a frame's name never reaches a file.
/// </summary>
public enum EnemyFrameId
{
    /// <summary><c>boyon_idle_0</c>, an installed spritemap.</summary>
    BoyonIdle0 = 1,
    /// <summary><c>boyon_idle_1</c>, an installed spritemap.</summary>
    BoyonIdle1,
    /// <summary><c>boyon_idle_2</c>, an installed spritemap.</summary>
    BoyonIdle2,
    /// <summary><c>boyon_bounce_0</c>, an installed spritemap.</summary>
    BoyonBounce0,
    /// <summary><c>boyon_bounce_1</c>, an installed spritemap.</summary>
    BoyonBounce1,
    /// <summary><c>boyon_bounce_2</c>, an installed spritemap.</summary>
    BoyonBounce2,
    /// <summary><c>boyon_bounce_3</c>, an installed spritemap.</summary>
    BoyonBounce3,
    /// <summary><c>cacatac_upright_idle_0</c>, an installed spritemap.</summary>
    CacatacUprightIdle0,
    /// <summary><c>cacatac_upright_idle_1</c>, an installed spritemap.</summary>
    CacatacUprightIdle1,
    /// <summary><c>cacatac_upright_idle_2</c>, an installed spritemap.</summary>
    CacatacUprightIdle2,
    /// <summary><c>cacatac_upright_idle_3</c>, an installed spritemap.</summary>
    CacatacUprightIdle3,
    /// <summary><c>cacatac_upright_idle_4</c>, an installed spritemap.</summary>
    CacatacUprightIdle4,
    /// <summary><c>cacatac_upright_idle_5</c>, an installed spritemap.</summary>
    CacatacUprightIdle5,
    /// <summary><c>cacatac_upright_idle_6</c>, an installed spritemap.</summary>
    CacatacUprightIdle6,
    /// <summary><c>cacatac_upright_idle_7</c>, an installed spritemap.</summary>
    CacatacUprightIdle7,
    /// <summary><c>cacatac_upright_attack_1</c>, an installed spritemap.</summary>
    CacatacUprightAttack1,
    /// <summary><c>cacatac_upright_attack_2</c>, an installed spritemap.</summary>
    CacatacUprightAttack2,
    /// <summary><c>cacatac_inverted_idle_0</c>, an installed spritemap.</summary>
    CacatacInvertedIdle0,
    /// <summary><c>cacatac_inverted_idle_1</c>, an installed spritemap.</summary>
    CacatacInvertedIdle1,
    /// <summary><c>cacatac_inverted_idle_2</c>, an installed spritemap.</summary>
    CacatacInvertedIdle2,
    /// <summary><c>cacatac_inverted_idle_3</c>, an installed spritemap.</summary>
    CacatacInvertedIdle3,
    /// <summary><c>cacatac_inverted_idle_4</c>, an installed spritemap.</summary>
    CacatacInvertedIdle4,
    /// <summary><c>cacatac_inverted_idle_5</c>, an installed spritemap.</summary>
    CacatacInvertedIdle5,
    /// <summary><c>cacatac_inverted_idle_6</c>, an installed spritemap.</summary>
    CacatacInvertedIdle6,
    /// <summary><c>cacatac_inverted_idle_7</c>, an installed spritemap.</summary>
    CacatacInvertedIdle7,
    /// <summary><c>cacatac_inverted_attack_1</c>, an installed spritemap.</summary>
    CacatacInvertedAttack1,
    /// <summary><c>cacatac_inverted_attack_2</c>, an installed spritemap.</summary>
    CacatacInvertedAttack2,
    /// <summary><c>boulder_roll_0</c>, an installed spritemap.</summary>
    BoulderRoll0,
    /// <summary><c>boulder_roll_1</c>, an installed spritemap.</summary>
    BoulderRoll1,
    /// <summary><c>boulder_roll_2</c>, an installed spritemap.</summary>
    BoulderRoll2,
    /// <summary><c>boulder_roll_3</c>, an installed spritemap.</summary>
    BoulderRoll3,
    /// <summary><c>boulder_roll_4</c>, an installed spritemap.</summary>
    BoulderRoll4,
    /// <summary><c>boulder_roll_5</c>, an installed spritemap.</summary>
    BoulderRoll5,
    /// <summary><c>boulder_roll_6</c>, an installed spritemap.</summary>
    BoulderRoll6,
    /// <summary><c>boulder_roll_7</c>, an installed spritemap.</summary>
    BoulderRoll7,
    /// <summary><c>atomic_up_right_0</c>, an installed spritemap.</summary>
    AtomicUpRight0,
    /// <summary><c>atomic_up_right_1</c>, an installed spritemap.</summary>
    AtomicUpRight1,
    /// <summary><c>atomic_up_right_2</c>, an installed spritemap.</summary>
    AtomicUpRight2,
    /// <summary><c>atomic_up_right_3</c>, an installed spritemap.</summary>
    AtomicUpRight3,
    /// <summary><c>atomic_up_right_4</c>, an installed spritemap.</summary>
    AtomicUpRight4,
    /// <summary><c>atomic_up_right_5</c>, an installed spritemap.</summary>
    AtomicUpRight5,
    /// <summary><c>atomic_up_left_0</c>, an installed spritemap.</summary>
    AtomicUpLeft0,
    /// <summary><c>atomic_up_left_1</c>, an installed spritemap.</summary>
    AtomicUpLeft1,
    /// <summary><c>atomic_up_left_2</c>, an installed spritemap.</summary>
    AtomicUpLeft2,
    /// <summary><c>atomic_up_left_3</c>, an installed spritemap.</summary>
    AtomicUpLeft3,
    /// <summary><c>atomic_up_left_4</c>, an installed spritemap.</summary>
    AtomicUpLeft4,
    /// <summary><c>atomic_up_left_5</c>, an installed spritemap.</summary>
    AtomicUpLeft5,
    /// <summary><c>skultera_swim_left_0</c>, an installed spritemap.</summary>
    SkulteraSwimLeft0,
    /// <summary><c>skultera_swim_left_1</c>, an installed spritemap.</summary>
    SkulteraSwimLeft1,
    /// <summary><c>skultera_swim_left_2</c>, an installed spritemap.</summary>
    SkulteraSwimLeft2,
    /// <summary><c>skultera_turn_right_0</c>, an installed spritemap.</summary>
    SkulteraTurnRight0,
    /// <summary><c>skultera_turn_right_1</c>, an installed spritemap.</summary>
    SkulteraTurnRight1,
    /// <summary><c>skultera_turn_right_2</c>, an installed spritemap.</summary>
    SkulteraTurnRight2,
    /// <summary><c>skultera_turn_right_3</c>, an installed spritemap.</summary>
    SkulteraTurnRight3,
    /// <summary><c>skultera_turn_right_4</c>, an installed spritemap.</summary>
    SkulteraTurnRight4,
    /// <summary><c>skultera_turn_right_5</c>, an installed spritemap.</summary>
    SkulteraTurnRight5,
    /// <summary><c>skultera_turn_right_6</c>, an installed spritemap.</summary>
    SkulteraTurnRight6,
    /// <summary><c>skultera_turn_right_7</c>, an installed spritemap.</summary>
    SkulteraTurnRight7,
    /// <summary><c>skultera_swim_right_0</c>, an installed spritemap.</summary>
    SkulteraSwimRight0,
    /// <summary><c>skultera_swim_right_1</c>, an installed spritemap.</summary>
    SkulteraSwimRight1,
    /// <summary><c>skultera_swim_right_2</c>, an installed spritemap.</summary>
    SkulteraSwimRight2,
    /// <summary><c>skultera_turn_left_0</c>, an installed spritemap.</summary>
    SkulteraTurnLeft0,
    /// <summary><c>skultera_turn_left_1</c>, an installed spritemap.</summary>
    SkulteraTurnLeft1,
    /// <summary><c>skultera_turn_left_2</c>, an installed spritemap.</summary>
    SkulteraTurnLeft2,
    /// <summary><c>skultera_turn_left_3</c>, an installed spritemap.</summary>
    SkulteraTurnLeft3,
    /// <summary><c>skultera_turn_left_4</c>, an installed spritemap.</summary>
    SkulteraTurnLeft4,
    /// <summary><c>skultera_turn_left_5</c>, an installed spritemap.</summary>
    SkulteraTurnLeft5,
    /// <summary><c>skultera_turn_left_6</c>, an installed spritemap.</summary>
    SkulteraTurnLeft6,
    /// <summary><c>skultera_turn_left_7</c>, an installed spritemap.</summary>
    SkulteraTurnLeft7,
    /// <summary><c>waver_steady_left</c>, an installed spritemap.</summary>
    WaverSteadyLeft,
    /// <summary><c>waver_steady_right</c>, an installed spritemap.</summary>
    WaverSteadyRight,
    /// <summary><c>waver_spin_left_0</c>, an installed spritemap.</summary>
    WaverSpinLeft0,
    /// <summary><c>waver_spin_left_1</c>, an installed spritemap.</summary>
    WaverSpinLeft1,
    /// <summary><c>waver_spin_left_2</c>, an installed spritemap.</summary>
    WaverSpinLeft2,
    /// <summary><c>waver_spin_left_3</c>, an installed spritemap.</summary>
    WaverSpinLeft3,
    /// <summary><c>waver_spin_right_0</c>, an installed spritemap.</summary>
    WaverSpinRight0,
    /// <summary><c>waver_spin_right_1</c>, an installed spritemap.</summary>
    WaverSpinRight1,
    /// <summary><c>waver_spin_right_2</c>, an installed spritemap.</summary>
    WaverSpinRight2,
    /// <summary><c>waver_spin_right_3</c>, an installed spritemap.</summary>
    WaverSpinRight3,
    /// <summary><c>metaree_idle_0</c>, an installed spritemap.</summary>
    MetareeIdle0,
    /// <summary><c>metaree_idle_1</c>, an installed spritemap.</summary>
    MetareeIdle1,
    /// <summary><c>metaree_idle_2</c>, an installed spritemap.</summary>
    MetareeIdle2,
    /// <summary><c>metaree_idle_3</c>, an installed spritemap.</summary>
    MetareeIdle3,
    /// <summary><c>metaree_prepare_1</c>, an installed spritemap.</summary>
    MetareePrepare1,
    /// <summary><c>skree_idle_0</c>, an installed spritemap.</summary>
    SkreeIdle0,
    /// <summary><c>skree_idle_1</c>, an installed spritemap.</summary>
    SkreeIdle1,
    /// <summary><c>skree_idle_2</c>, an installed spritemap.</summary>
    SkreeIdle2,
    /// <summary><c>skree_idle_3</c>, an installed spritemap.</summary>
    SkreeIdle3,
    /// <summary><c>skree_prepare_1</c>, an installed spritemap.</summary>
    SkreePrepare1,
    /// <summary><c>zoa_shoot_left_0</c>, an installed spritemap.</summary>
    ZoaShootLeft0,
    /// <summary><c>zoa_shoot_left_1</c>, an installed spritemap.</summary>
    ZoaShootLeft1,
    /// <summary><c>zoa_shoot_left_2</c>, an installed spritemap.</summary>
    ZoaShootLeft2,
    /// <summary><c>zoa_rise_left_0</c>, an installed spritemap.</summary>
    ZoaRiseLeft0,
    /// <summary><c>zoa_rise_left_1</c>, an installed spritemap.</summary>
    ZoaRiseLeft1,
    /// <summary><c>zoa_rise_left_2</c>, an installed spritemap.</summary>
    ZoaRiseLeft2,
    /// <summary><c>zoa_shoot_right_0</c>, an installed spritemap.</summary>
    ZoaShootRight0,
    /// <summary><c>zoa_shoot_right_1</c>, an installed spritemap.</summary>
    ZoaShootRight1,
    /// <summary><c>zoa_shoot_right_2</c>, an installed spritemap.</summary>
    ZoaShootRight2,
    /// <summary><c>zoa_rise_right_0</c>, an installed spritemap.</summary>
    ZoaRiseRight0,
    /// <summary><c>zoa_rise_right_1</c>, an installed spritemap.</summary>
    ZoaRiseRight1,
    /// <summary><c>zoa_rise_right_2</c>, an installed spritemap.</summary>
    ZoaRiseRight2,
    /// <summary><c>pipe_brinstar_normal_left_0</c>, an installed spritemap.</summary>
    PipeBrinstarNormalLeft0,
    /// <summary><c>pipe_brinstar_normal_left_1</c>, an installed spritemap.</summary>
    PipeBrinstarNormalLeft1,
    /// <summary><c>pipe_brinstar_normal_left_2</c>, an installed spritemap.</summary>
    PipeBrinstarNormalLeft2,
    /// <summary><c>pipe_brinstar_normal_left_3</c>, an installed spritemap.</summary>
    PipeBrinstarNormalLeft3,
    /// <summary><c>pipe_brinstar_normal_left_4</c>, an installed spritemap.</summary>
    PipeBrinstarNormalLeft4,
    /// <summary><c>pipe_brinstar_normal_right_0</c>, an installed spritemap.</summary>
    PipeBrinstarNormalRight0,
    /// <summary><c>pipe_brinstar_normal_right_1</c>, an installed spritemap.</summary>
    PipeBrinstarNormalRight1,
    /// <summary><c>pipe_brinstar_normal_right_2</c>, an installed spritemap.</summary>
    PipeBrinstarNormalRight2,
    /// <summary><c>pipe_brinstar_normal_right_3</c>, an installed spritemap.</summary>
    PipeBrinstarNormalRight3,
    /// <summary><c>pipe_brinstar_normal_right_4</c>, an installed spritemap.</summary>
    PipeBrinstarNormalRight4,
    /// <summary><c>pipe_brinstar_strong_shoot_left_0</c>, an installed spritemap.</summary>
    PipeBrinstarStrongShootLeft0,
    /// <summary><c>pipe_brinstar_strong_shoot_left_1</c>, an installed spritemap.</summary>
    PipeBrinstarStrongShootLeft1,
    /// <summary><c>pipe_brinstar_strong_shoot_left_2</c>, an installed spritemap.</summary>
    PipeBrinstarStrongShootLeft2,
    /// <summary><c>pipe_brinstar_strong_rise_left_0</c>, an installed spritemap.</summary>
    PipeBrinstarStrongRiseLeft0,
    /// <summary><c>pipe_brinstar_strong_rise_left_1</c>, an installed spritemap.</summary>
    PipeBrinstarStrongRiseLeft1,
    /// <summary><c>pipe_brinstar_strong_rise_left_2</c>, an installed spritemap.</summary>
    PipeBrinstarStrongRiseLeft2,
    /// <summary><c>pipe_brinstar_strong_shoot_right_0</c>, an installed spritemap.</summary>
    PipeBrinstarStrongShootRight0,
    /// <summary><c>pipe_brinstar_strong_shoot_right_1</c>, an installed spritemap.</summary>
    PipeBrinstarStrongShootRight1,
    /// <summary><c>pipe_brinstar_strong_shoot_right_2</c>, an installed spritemap.</summary>
    PipeBrinstarStrongShootRight2,
    /// <summary><c>pipe_brinstar_strong_rise_right_0</c>, an installed spritemap.</summary>
    PipeBrinstarStrongRiseRight0,
    /// <summary><c>pipe_brinstar_strong_rise_right_1</c>, an installed spritemap.</summary>
    PipeBrinstarStrongRiseRight1,
    /// <summary><c>pipe_brinstar_strong_rise_right_2</c>, an installed spritemap.</summary>
    PipeBrinstarStrongRiseRight2,
    /// <summary><c>pipe_norfair_left_0</c>, an installed spritemap.</summary>
    PipeNorfairLeft0,
    /// <summary><c>pipe_norfair_left_1</c>, an installed spritemap.</summary>
    PipeNorfairLeft1,
    /// <summary><c>pipe_norfair_left_2</c>, an installed spritemap.</summary>
    PipeNorfairLeft2,
    /// <summary><c>pipe_norfair_left_3</c>, an installed spritemap.</summary>
    PipeNorfairLeft3,
    /// <summary><c>pipe_norfair_left_4</c>, an installed spritemap.</summary>
    PipeNorfairLeft4,
    /// <summary><c>pipe_norfair_right_0</c>, an installed spritemap.</summary>
    PipeNorfairRight0,
    /// <summary><c>pipe_norfair_right_1</c>, an installed spritemap.</summary>
    PipeNorfairRight1,
    /// <summary><c>pipe_norfair_right_2</c>, an installed spritemap.</summary>
    PipeNorfairRight2,
    /// <summary><c>pipe_norfair_right_3</c>, an installed spritemap.</summary>
    PipeNorfairRight3,
    /// <summary><c>pipe_norfair_right_4</c>, an installed spritemap.</summary>
    PipeNorfairRight4,
    /// <summary><c>pipe_yellow_fly_left_0</c>, an installed spritemap.</summary>
    PipeYellowFlyLeft0,
    /// <summary><c>pipe_yellow_fly_left_1</c>, an installed spritemap.</summary>
    PipeYellowFlyLeft1,
    /// <summary><c>pipe_yellow_fly_left_2</c>, an installed spritemap.</summary>
    PipeYellowFlyLeft2,
    /// <summary><c>pipe_yellow_arc_left_0</c>, an installed spritemap.</summary>
    PipeYellowArcLeft0,
    /// <summary><c>pipe_yellow_arc_left_1</c>, an installed spritemap.</summary>
    PipeYellowArcLeft1,
    /// <summary><c>pipe_yellow_arc_left_2</c>, an installed spritemap.</summary>
    PipeYellowArcLeft2,
    /// <summary><c>pipe_yellow_fly_right_0</c>, an installed spritemap.</summary>
    PipeYellowFlyRight0,
    /// <summary><c>pipe_yellow_fly_right_1</c>, an installed spritemap.</summary>
    PipeYellowFlyRight1,
    /// <summary><c>pipe_yellow_fly_right_2</c>, an installed spritemap.</summary>
    PipeYellowFlyRight2,
    /// <summary><c>pipe_yellow_arc_right_0</c>, an installed spritemap.</summary>
    PipeYellowArcRight0,
    /// <summary><c>pipe_yellow_arc_right_1</c>, an installed spritemap.</summary>
    PipeYellowArcRight1,
    /// <summary><c>pipe_yellow_arc_right_2</c>, an installed spritemap.</summary>
    PipeYellowArcRight2,
    /// <summary><c>fake_kraid_walk_left_0</c>, an installed spritemap.</summary>
    FakeKraidWalkLeft0,
    /// <summary><c>fake_kraid_walk_left_1</c>, an installed spritemap.</summary>
    FakeKraidWalkLeft1,
    /// <summary><c>fake_kraid_walk_left_2</c>, an installed spritemap.</summary>
    FakeKraidWalkLeft2,
    /// <summary><c>fake_kraid_walk_left_3</c>, an installed spritemap.</summary>
    FakeKraidWalkLeft3,
    /// <summary><c>fake_kraid_spit_left_0</c>, an installed spritemap.</summary>
    FakeKraidSpitLeft0,
    /// <summary><c>fake_kraid_spit_left_1</c>, an installed spritemap.</summary>
    FakeKraidSpitLeft1,
    /// <summary><c>fake_kraid_spit_left_2</c>, an installed spritemap.</summary>
    FakeKraidSpitLeft2,
    /// <summary><c>fake_kraid_walk_right_0</c>, an installed spritemap.</summary>
    FakeKraidWalkRight0,
    /// <summary><c>fake_kraid_walk_right_1</c>, an installed spritemap.</summary>
    FakeKraidWalkRight1,
    /// <summary><c>fake_kraid_walk_right_2</c>, an installed spritemap.</summary>
    FakeKraidWalkRight2,
    /// <summary><c>fake_kraid_walk_right_3</c>, an installed spritemap.</summary>
    FakeKraidWalkRight3,
    /// <summary><c>fake_kraid_spit_right_0</c>, an installed spritemap.</summary>
    FakeKraidSpitRight0,
    /// <summary><c>fake_kraid_spit_right_1</c>, an installed spritemap.</summary>
    FakeKraidSpitRight1,
    /// <summary><c>fake_kraid_spit_right_2</c>, an installed spritemap.</summary>
    FakeKraidSpitRight2,
    /// <summary><c>kraid_nail_0</c>, an installed spritemap.</summary>
    KraidNail0,
    /// <summary><c>kraid_nail_1</c>, an installed spritemap.</summary>
    KraidNail1,
    /// <summary><c>kraid_nail_2</c>, an installed spritemap.</summary>
    KraidNail2,
    /// <summary><c>kraid_nail_3</c>, an installed spritemap.</summary>
    KraidNail3,
    /// <summary><c>kraid_nail_4</c>, an installed spritemap.</summary>
    KraidNail4,
    /// <summary><c>kraid_nail_5</c>, an installed spritemap.</summary>
    KraidNail5,
    /// <summary><c>kraid_nail_6</c>, an installed spritemap.</summary>
    KraidNail6,
    /// <summary><c>kraid_nail_7</c>, an installed spritemap.</summary>
    KraidNail7,
    /// <summary><c>owtch_left_0</c>, an installed spritemap.</summary>
    OwtchLeft0,
    /// <summary><c>owtch_left_1</c>, an installed spritemap.</summary>
    OwtchLeft1,
    /// <summary><c>owtch_left_2</c>, an installed spritemap.</summary>
    OwtchLeft2,
    /// <summary><c>stoke_walk_left_0</c>, an installed spritemap.</summary>
    StokeWalkLeft0,
    /// <summary><c>stoke_walk_left_1</c>, an installed spritemap.</summary>
    StokeWalkLeft1,
    /// <summary><c>stoke_walk_left_2</c>, an installed spritemap.</summary>
    StokeWalkLeft2,
    /// <summary><c>stoke_walk_left_3</c>, an installed spritemap.</summary>
    StokeWalkLeft3,
    /// <summary><c>stoke_attack_left</c>, an installed spritemap.</summary>
    StokeAttackLeft,
    /// <summary><c>stoke_walk_right_0</c>, an installed spritemap.</summary>
    StokeWalkRight0,
    /// <summary><c>stoke_walk_right_1</c>, an installed spritemap.</summary>
    StokeWalkRight1,
    /// <summary><c>stoke_walk_right_2</c>, an installed spritemap.</summary>
    StokeWalkRight2,
    /// <summary><c>stoke_walk_right_3</c>, an installed spritemap.</summary>
    StokeWalkRight3,
    /// <summary><c>stoke_attack_right</c>, an installed spritemap.</summary>
    StokeAttackRight,
    /// <summary><c>ripper_shared_left_0</c>, an installed spritemap.</summary>
    RipperSharedLeft0,
    /// <summary><c>ripper_shared_left_1</c>, an installed spritemap.</summary>
    RipperSharedLeft1,
    /// <summary><c>ripper_shared_left_2</c>, an installed spritemap.</summary>
    RipperSharedLeft2,
    /// <summary><c>ripper_shared_right_0</c>, an installed spritemap.</summary>
    RipperSharedRight0,
    /// <summary><c>ripper_shared_right_1</c>, an installed spritemap.</summary>
    RipperSharedRight1,
    /// <summary><c>ripper_shared_right_2</c>, an installed spritemap.</summary>
    RipperSharedRight2,
    /// <summary><c>ripper_shared_frozen_left</c>, an installed spritemap.</summary>
    RipperSharedFrozenLeft,
    /// <summary><c>ripper_shared_frozen_right</c>, an installed spritemap.</summary>
    RipperSharedFrozenRight,
    /// <summary><c>ripper_left_0</c>, an installed spritemap.</summary>
    RipperLeft0,
    /// <summary><c>ripper_left_1</c>, an installed spritemap.</summary>
    RipperLeft1,
    /// <summary><c>ripper_left_2</c>, an installed spritemap.</summary>
    RipperLeft2,
    /// <summary><c>ripper_right_0</c>, an installed spritemap.</summary>
    RipperRight0,
    /// <summary><c>ripper_right_1</c>, an installed spritemap.</summary>
    RipperRight1,
    /// <summary><c>ripper_right_2</c>, an installed spritemap.</summary>
    RipperRight2,
    /// <summary><c>fireflea_cycle_0</c>, an installed spritemap.</summary>
    FirefleaCycle0,
    /// <summary><c>fireflea_cycle_1</c>, an installed spritemap.</summary>
    FirefleaCycle1,
    /// <summary><c>fireflea_cycle_2</c>, an installed spritemap.</summary>
    FirefleaCycle2,
    /// <summary><c>fireflea_cycle_3</c>, an installed spritemap.</summary>
    FirefleaCycle3,
    /// <summary><c>fireflea_cycle_4</c>, an installed spritemap.</summary>
    FirefleaCycle4,
    /// <summary><c>fireflea_cycle_5</c>, an installed spritemap.</summary>
    FirefleaCycle5,
    /// <summary><c>fireflea_cycle_6</c>, an installed spritemap.</summary>
    FirefleaCycle6,
    /// <summary><c>fireflea_cycle_7</c>, an installed spritemap.</summary>
    FirefleaCycle7,
    /// <summary><c>fireflea_cycle_8</c>, an installed spritemap.</summary>
    FirefleaCycle8,
    /// <summary><c>fireflea_cycle_9</c>, an installed spritemap.</summary>
    FirefleaCycle9,
    /// <summary><c>fireflea_cycle_10</c>, an installed spritemap.</summary>
    FirefleaCycle10,
    /// <summary><c>fireflea_cycle_11</c>, an installed spritemap.</summary>
    FirefleaCycle11,
    /// <summary><c>fireflea_cycle_12</c>, an installed spritemap.</summary>
    FirefleaCycle12,
    /// <summary><c>fireflea_cycle_13</c>, an installed spritemap.</summary>
    FirefleaCycle13,
    /// <summary><c>fireflea_cycle_14</c>, an installed spritemap.</summary>
    FirefleaCycle14,
    /// <summary><c>fireflea_cycle_15</c>, an installed spritemap.</summary>
    FirefleaCycle15,
    /// <summary><c>fireflea_cycle_16</c>, an installed spritemap.</summary>
    FirefleaCycle16,
    /// <summary><c>fireflea_cycle_17</c>, an installed spritemap.</summary>
    FirefleaCycle17,
    /// <summary><c>fireflea_cycle_18</c>, an installed spritemap.</summary>
    FirefleaCycle18,
    /// <summary><c>fireflea_cycle_19</c>, an installed spritemap.</summary>
    FirefleaCycle19,
    /// <summary><c>fireflea_cycle_20</c>, an installed spritemap.</summary>
    FirefleaCycle20,
    /// <summary><c>magdollite_left_idle_0</c>, an installed spritemap.</summary>
    MagdolliteLeftIdle0,
    /// <summary><c>magdollite_left_idle_1</c>, an installed spritemap.</summary>
    MagdolliteLeftIdle1,
    /// <summary><c>magdollite_left_idle_2</c>, an installed spritemap.</summary>
    MagdolliteLeftIdle2,
    /// <summary><c>magdollite_left_throw_0</c>, an installed spritemap.</summary>
    MagdolliteLeftThrow0,
    /// <summary><c>magdollite_left_throw_1</c>, an installed spritemap.</summary>
    MagdolliteLeftThrow1,
    /// <summary><c>magdollite_left_throw_2</c>, an installed spritemap.</summary>
    MagdolliteLeftThrow2,
    /// <summary><c>magdollite_left_throw_3</c>, an installed spritemap.</summary>
    MagdolliteLeftThrow3,
    /// <summary><c>magdollite_pillar_cap</c>, an installed spritemap.</summary>
    MagdollitePillarCap,
    /// <summary><c>magdollite_left_submerge_0</c>, an installed spritemap.</summary>
    MagdolliteLeftSubmerge0,
    /// <summary><c>magdollite_left_submerge_1</c>, an installed spritemap.</summary>
    MagdolliteLeftSubmerge1,
    /// <summary><c>magdollite_left_submerge_2</c>, an installed spritemap.</summary>
    MagdolliteLeftSubmerge2,
    /// <summary><c>magdollite_right_idle_0</c>, an installed spritemap.</summary>
    MagdolliteRightIdle0,
    /// <summary><c>magdollite_right_idle_1</c>, an installed spritemap.</summary>
    MagdolliteRightIdle1,
    /// <summary><c>magdollite_right_idle_2</c>, an installed spritemap.</summary>
    MagdolliteRightIdle2,
    /// <summary><c>magdollite_right_throw_0</c>, an installed spritemap.</summary>
    MagdolliteRightThrow0,
    /// <summary><c>magdollite_right_throw_1</c>, an installed spritemap.</summary>
    MagdolliteRightThrow1,
    /// <summary><c>magdollite_right_throw_2</c>, an installed spritemap.</summary>
    MagdolliteRightThrow2,
    /// <summary><c>magdollite_right_throw_3</c>, an installed spritemap.</summary>
    MagdolliteRightThrow3,
    /// <summary><c>magdollite_right_submerge_0</c>, an installed spritemap.</summary>
    MagdolliteRightSubmerge0,
    /// <summary><c>magdollite_right_submerge_1</c>, an installed spritemap.</summary>
    MagdolliteRightSubmerge1,
    /// <summary><c>magdollite_right_submerge_2</c>, an installed spritemap.</summary>
    MagdolliteRightSubmerge2,
    /// <summary><c>magdollite_pillar_phase_0</c>, an installed spritemap.</summary>
    MagdollitePillarPhase0,
    /// <summary><c>magdollite_pillar_phase_1</c>, an installed spritemap.</summary>
    MagdollitePillarPhase1,
    /// <summary><c>magdollite_pillar_phase_2</c>, an installed spritemap.</summary>
    MagdollitePillarPhase2,
    /// <summary><c>magdollite_pillar_phase_3</c>, an installed spritemap.</summary>
    MagdollitePillarPhase3,
    /// <summary><c>magdollite_pillar_phase_4</c>, an installed spritemap.</summary>
    MagdollitePillarPhase4,
    /// <summary><c>magdollite_pillar_phase_5</c>, an installed spritemap.</summary>
    MagdollitePillarPhase5,
    /// <summary><c>magdollite_pillar_phase_6</c>, an installed spritemap.</summary>
    MagdollitePillarPhase6,
    /// <summary><c>magdollite_pillar_phase_7</c>, an installed spritemap.</summary>
    MagdollitePillarPhase7,
    /// <summary><c>ceres_door_rotating_overlay</c>, an installed spritemap.</summary>
    CeresDoorRotatingOverlay,
    /// <summary><c>ceres_door_left_hold</c>, an installed spritemap.</summary>
    CeresDoorLeftHold,
    /// <summary><c>ceres_door_left_transition_0</c>, an installed spritemap.</summary>
    CeresDoorLeftTransition0,
    /// <summary><c>ceres_door_left_transition_1</c>, an installed spritemap.</summary>
    CeresDoorLeftTransition1,
    /// <summary><c>ceres_door_left_transition_2</c>, an installed spritemap.</summary>
    CeresDoorLeftTransition2,
    /// <summary><c>ceres_door_left_transition_3</c>, an installed spritemap.</summary>
    CeresDoorLeftTransition3,
    /// <summary><c>ceres_door_right_hold</c>, an installed spritemap.</summary>
    CeresDoorRightHold,
    /// <summary><c>ceres_door_right_transition_0</c>, an installed spritemap.</summary>
    CeresDoorRightTransition0,
    /// <summary><c>ceres_door_right_transition_1</c>, an installed spritemap.</summary>
    CeresDoorRightTransition1,
    /// <summary><c>ceres_door_right_transition_2</c>, an installed spritemap.</summary>
    CeresDoorRightTransition2,
    /// <summary><c>ceres_door_right_transition_3</c>, an installed spritemap.</summary>
    CeresDoorRightTransition3,
    /// <summary><c>ceres_door_initial</c>, an installed spritemap.</summary>
    CeresDoorInitial,
    /// <summary><c>ceres_door_mode7_left_wall</c>, an installed spritemap.</summary>
    CeresDoorTurningFloorLeftWall,
    /// <summary><c>ceres_door_mode7_right_wall</c>, an installed spritemap.</summary>
    CeresDoorTurningFloorRightWall,
    /// <summary><c>ceres_door_ridley_private_overlay</c>, an installed spritemap.</summary>
    CeresDoorRidleyPrivateOverlay,
    /// <summary><c>ceres_baby_horizontal</c>, an installed spritemap.</summary>
    CeresBabyHorizontal,
    /// <summary><c>ceres_baby_round</c>, an installed spritemap.</summary>
    CeresBabyRound,
    /// <summary><c>ceres_baby_vertical</c>, an installed spritemap.</summary>
    CeresBabyVertical,
    /// <summary><c>rio_pose_0</c>, an installed spritemap.</summary>
    RioPose0,
    /// <summary><c>rio_pose_1</c>, an installed spritemap.</summary>
    RioPose1,
    /// <summary><c>rio_pose_2</c>, an installed spritemap.</summary>
    RioPose2,
    /// <summary><c>rio_pose_3</c>, an installed spritemap.</summary>
    RioPose3,
    /// <summary><c>rio_pose_4</c>, an installed spritemap.</summary>
    RioPose4,
    /// <summary><c>rio_pose_5</c>, an installed spritemap.</summary>
    RioPose5,
    /// <summary><c>rio_pose_6</c>, an installed spritemap.</summary>
    RioPose6,
    /// <summary><c>rio_pose_7</c>, an installed spritemap.</summary>
    RioPose7,
    /// <summary><c>lower_norfair_rio_pose_0</c>, an installed spritemap.</summary>
    LowerNorfairRioPose0,
    /// <summary><c>lower_norfair_rio_pose_1</c>, an installed spritemap.</summary>
    LowerNorfairRioPose1,
    /// <summary><c>lower_norfair_rio_pose_2</c>, an installed spritemap.</summary>
    LowerNorfairRioPose2,
    /// <summary><c>lower_norfair_rio_pose_3</c>, an installed spritemap.</summary>
    LowerNorfairRioPose3,
    /// <summary><c>lower_norfair_rio_pose_4</c>, an installed spritemap.</summary>
    LowerNorfairRioPose4,
    /// <summary><c>lower_norfair_rio_pose_5</c>, an installed spritemap.</summary>
    LowerNorfairRioPose5,
    /// <summary><c>lower_norfair_rio_pose_6</c>, an installed spritemap.</summary>
    LowerNorfairRioPose6,
    /// <summary><c>lower_norfair_rio_pose_7</c>, an installed spritemap.</summary>
    LowerNorfairRioPose7,
    /// <summary><c>lower_norfair_rio_pose_8</c>, an installed spritemap.</summary>
    LowerNorfairRioPose8,
    /// <summary><c>lower_norfair_rio_pose_9</c>, an installed spritemap.</summary>
    LowerNorfairRioPose9,
    /// <summary><c>lower_norfair_rio_pose_10</c>, an installed spritemap.</summary>
    LowerNorfairRioPose10,
    /// <summary><c>lower_norfair_rio_pose_11</c>, an installed spritemap.</summary>
    LowerNorfairRioPose11,
    /// <summary><c>lower_norfair_rio_pose_12</c>, an installed spritemap.</summary>
    LowerNorfairRioPose12,
    /// <summary><c>lower_norfair_rio_pose_13</c>, an installed spritemap.</summary>
    LowerNorfairRioPose13,
    /// <summary><c>lower_norfair_rio_pose_14</c>, an installed spritemap.</summary>
    LowerNorfairRioPose14,
    /// <summary><c>lower_norfair_rio_pose_15</c>, an installed spritemap.</summary>
    LowerNorfairRioPose15,
    /// <summary><c>lower_norfair_rio_pose_16</c>, an installed spritemap.</summary>
    LowerNorfairRioPose16,
    /// <summary><c>lower_norfair_rio_pose_17</c>, an installed spritemap.</summary>
    LowerNorfairRioPose17,
    /// <summary><c>norfair_rio_pose_0</c>, an installed spritemap.</summary>
    NorfairRioPose0,
    /// <summary><c>norfair_rio_pose_1</c>, an installed spritemap.</summary>
    NorfairRioPose1,
    /// <summary><c>norfair_rio_pose_2</c>, an installed spritemap.</summary>
    NorfairRioPose2,
    /// <summary><c>norfair_rio_pose_3</c>, an installed spritemap.</summary>
    NorfairRioPose3,
    /// <summary><c>norfair_rio_pose_4</c>, an installed spritemap.</summary>
    NorfairRioPose4,
    /// <summary><c>norfair_rio_pose_5</c>, an installed spritemap.</summary>
    NorfairRioPose5,
    /// <summary><c>norfair_rio_pose_6</c>, an installed spritemap.</summary>
    NorfairRioPose6,
    /// <summary><c>norfair_rio_pose_7</c>, an installed spritemap.</summary>
    NorfairRioPose7,
    /// <summary><c>norfair_rio_pose_8</c>, an installed spritemap.</summary>
    NorfairRioPose8,
    /// <summary><c>norfair_rio_pose_9</c>, an installed spritemap.</summary>
    NorfairRioPose9,
    /// <summary><c>norfair_rio_pose_10</c>, an installed spritemap.</summary>
    NorfairRioPose10,
    /// <summary><c>norfair_rio_pose_11</c>, an installed spritemap.</summary>
    NorfairRioPose11,
    /// <summary><c>norfair_rio_pose_12</c>, an installed spritemap.</summary>
    NorfairRioPose12,
    /// <summary><c>norfair_rio_pose_13</c>, an installed spritemap.</summary>
    NorfairRioPose13,
    /// <summary><c>norfair_rio_pose_14</c>, an installed spritemap.</summary>
    NorfairRioPose14,
    /// <summary><c>norfair_rio_pose_15</c>, an installed spritemap.</summary>
    NorfairRioPose15,
    /// <summary><c>norfair_rio_pose_16</c>, an installed spritemap.</summary>
    NorfairRioPose16,
    /// <summary><c>norfair_rio_pose_17</c>, an installed spritemap.</summary>
    NorfairRioPose17,
    /// <summary><c>norfair_rio_pose_18</c>, an installed spritemap.</summary>
    NorfairRioPose18,
    /// <summary><c>norfair_rio_pose_19</c>, an installed spritemap.</summary>
    NorfairRioPose19,
    /// <summary><c>puyo_ground_0</c>, an installed spritemap.</summary>
    PuyoGround0,
    /// <summary><c>puyo_ground_1</c>, an installed spritemap.</summary>
    PuyoGround1,
    /// <summary><c>puyo_ground_2</c>, an installed spritemap.</summary>
    PuyoGround2,
    /// <summary><c>puyo_air_0</c>, an installed spritemap.</summary>
    PuyoAir0,
    /// <summary><c>puyo_air_1</c>, an installed spritemap.</summary>
    PuyoAir1,
    /// <summary><c>puyo_air_2</c>, an installed spritemap.</summary>
    PuyoAir2,
    /// <summary><c>puyo_air_3</c>, an installed spritemap.</summary>
    PuyoAir3,
    /// <summary><c>puyo_air_4</c>, an installed spritemap.</summary>
    PuyoAir4,
    /// <summary><c>bull_idle_0</c>, an installed spritemap.</summary>
    BullIdle0,
    /// <summary><c>bull_idle_1</c>, an installed spritemap.</summary>
    BullIdle1,
    /// <summary><c>bull_idle_2</c>, an installed spritemap.</summary>
    BullIdle2,
    /// <summary><c>alcoon_left_walk_0</c>, an installed spritemap.</summary>
    AlcoonLeftWalk0,
    /// <summary><c>alcoon_left_walk_1</c>, an installed spritemap.</summary>
    AlcoonLeftWalk1,
    /// <summary><c>alcoon_left_walk_2</c>, an installed spritemap.</summary>
    AlcoonLeftWalk2,
    /// <summary><c>alcoon_left_walk_3</c>, an installed spritemap.</summary>
    AlcoonLeftWalk3,
    /// <summary><c>alcoon_left_fire_0</c>, an installed spritemap.</summary>
    AlcoonLeftFire0,
    /// <summary><c>alcoon_left_fire_1</c>, an installed spritemap.</summary>
    AlcoonLeftFire1,
    /// <summary><c>alcoon_left_fire_2</c>, an installed spritemap.</summary>
    AlcoonLeftFire2,
    /// <summary><c>alcoon_left_fire_3</c>, an installed spritemap.</summary>
    AlcoonLeftFire3,
    /// <summary><c>alcoon_left_air_up</c>, an installed spritemap.</summary>
    AlcoonLeftAirUp,
    /// <summary><c>alcoon_right_walk_0</c>, an installed spritemap.</summary>
    AlcoonRightWalk0,
    /// <summary><c>alcoon_right_walk_1</c>, an installed spritemap.</summary>
    AlcoonRightWalk1,
    /// <summary><c>alcoon_right_walk_2</c>, an installed spritemap.</summary>
    AlcoonRightWalk2,
    /// <summary><c>alcoon_right_walk_3</c>, an installed spritemap.</summary>
    AlcoonRightWalk3,
    /// <summary><c>alcoon_right_fire_0</c>, an installed spritemap.</summary>
    AlcoonRightFire0,
    /// <summary><c>alcoon_right_fire_1</c>, an installed spritemap.</summary>
    AlcoonRightFire1,
    /// <summary><c>alcoon_right_fire_2</c>, an installed spritemap.</summary>
    AlcoonRightFire2,
    /// <summary><c>alcoon_right_fire_3</c>, an installed spritemap.</summary>
    AlcoonRightFire3,
    /// <summary><c>alcoon_right_air_up</c>, an installed spritemap.</summary>
    AlcoonRightAirUp,
    /// <summary><c>beetom_left_crawl_0</c>, an installed spritemap.</summary>
    BeetomLeftCrawl0,
    /// <summary><c>beetom_left_crawl_1</c>, an installed spritemap.</summary>
    BeetomLeftCrawl1,
    /// <summary><c>beetom_left_crawl_2</c>, an installed spritemap.</summary>
    BeetomLeftCrawl2,
    /// <summary><c>beetom_left_hop_0</c>, an installed spritemap.</summary>
    BeetomLeftHop0,
    /// <summary><c>beetom_left_hop_1</c>, an installed spritemap.</summary>
    BeetomLeftHop1,
    /// <summary><c>beetom_left_drain_0</c>, an installed spritemap.</summary>
    BeetomLeftDrain0,
    /// <summary><c>beetom_left_drain_1</c>, an installed spritemap.</summary>
    BeetomLeftDrain1,
    /// <summary><c>beetom_left_drain_2</c>, an installed spritemap.</summary>
    BeetomLeftDrain2,
    /// <summary><c>beetom_left_drain_3</c>, an installed spritemap.</summary>
    BeetomLeftDrain3,
    /// <summary><c>beetom_left_drain_4</c>, an installed spritemap.</summary>
    BeetomLeftDrain4,
    /// <summary><c>beetom_left_drain_5</c>, an installed spritemap.</summary>
    BeetomLeftDrain5,
    /// <summary><c>beetom_right_crawl_0</c>, an installed spritemap.</summary>
    BeetomRightCrawl0,
    /// <summary><c>beetom_right_crawl_1</c>, an installed spritemap.</summary>
    BeetomRightCrawl1,
    /// <summary><c>beetom_right_crawl_2</c>, an installed spritemap.</summary>
    BeetomRightCrawl2,
    /// <summary><c>beetom_right_hop_0</c>, an installed spritemap.</summary>
    BeetomRightHop0,
    /// <summary><c>beetom_right_hop_1</c>, an installed spritemap.</summary>
    BeetomRightHop1,
    /// <summary><c>beetom_right_drain_0</c>, an installed spritemap.</summary>
    BeetomRightDrain0,
    /// <summary><c>beetom_right_drain_1</c>, an installed spritemap.</summary>
    BeetomRightDrain1,
    /// <summary><c>beetom_right_drain_2</c>, an installed spritemap.</summary>
    BeetomRightDrain2,
    /// <summary><c>beetom_right_drain_3</c>, an installed spritemap.</summary>
    BeetomRightDrain3,
    /// <summary><c>beetom_right_drain_4</c>, an installed spritemap.</summary>
    BeetomRightDrain4,
    /// <summary><c>beetom_right_drain_5</c>, an installed spritemap.</summary>
    BeetomRightDrain5,
    /// <summary><c>sidehopper_jump_floor</c>, an installed spritemap.</summary>
    SidehopperJumpFloor,
    /// <summary><c>sidehopper_land_floor_0</c>, an installed spritemap.</summary>
    SidehopperLandFloor0,
    /// <summary><c>sidehopper_land_floor_1</c>, an installed spritemap.</summary>
    SidehopperLandFloor1,
    /// <summary><c>sidehopper_jump_ceiling</c>, an installed spritemap.</summary>
    SidehopperJumpCeiling,
    /// <summary><c>sidehopper_land_ceiling_0</c>, an installed spritemap.</summary>
    SidehopperLandCeiling0,
    /// <summary><c>sidehopper_land_ceiling_1</c>, an installed spritemap.</summary>
    SidehopperLandCeiling1,
    /// <summary><c>dessgeega_jump_floor</c>, an installed spritemap.</summary>
    DessgeegaJumpFloor,
    /// <summary><c>dessgeega_land_floor_0</c>, an installed spritemap.</summary>
    DessgeegaLandFloor0,
    /// <summary><c>dessgeega_land_floor_1</c>, an installed spritemap.</summary>
    DessgeegaLandFloor1,
    /// <summary><c>dessgeega_jump_ceiling</c>, an installed spritemap.</summary>
    DessgeegaJumpCeiling,
    /// <summary><c>dessgeega_land_ceiling_0</c>, an installed spritemap.</summary>
    DessgeegaLandCeiling0,
    /// <summary><c>dessgeega_land_ceiling_1</c>, an installed spritemap.</summary>
    DessgeegaLandCeiling1,
    /// <summary><c>large_sidehopper_jump_floor</c>, an installed spritemap.</summary>
    LargeSidehopperJumpFloor,
    /// <summary><c>large_sidehopper_land_floor_0</c>, an installed spritemap.</summary>
    LargeSidehopperLandFloor0,
    /// <summary><c>large_sidehopper_land_floor_1</c>, an installed spritemap.</summary>
    LargeSidehopperLandFloor1,
    /// <summary><c>large_sidehopper_jump_ceiling</c>, an installed spritemap.</summary>
    LargeSidehopperJumpCeiling,
    /// <summary><c>large_sidehopper_land_ceiling_0</c>, an installed spritemap.</summary>
    LargeSidehopperLandCeiling0,
    /// <summary><c>large_sidehopper_land_ceiling_1</c>, an installed spritemap.</summary>
    LargeSidehopperLandCeiling1,
    /// <summary><c>large_dessgeega_jump_floor</c>, an installed spritemap.</summary>
    LargeDessgeegaJumpFloor,
    /// <summary><c>large_dessgeega_land_floor_0</c>, an installed spritemap.</summary>
    LargeDessgeegaLandFloor0,
    /// <summary><c>large_dessgeega_land_floor_1</c>, an installed spritemap.</summary>
    LargeDessgeegaLandFloor1,
    /// <summary><c>large_dessgeega_jump_ceiling</c>, an installed spritemap.</summary>
    LargeDessgeegaJumpCeiling,
    /// <summary><c>large_dessgeega_land_ceiling_0</c>, an installed spritemap.</summary>
    LargeDessgeegaLandCeiling0,
    /// <summary><c>large_dessgeega_land_ceiling_1</c>, an installed spritemap.</summary>
    LargeDessgeegaLandCeiling1,
    /// <summary><c>choot_idle</c>, an installed spritemap.</summary>
    ChootIdle,
    /// <summary><c>choot_jump</c>, an installed spritemap.</summary>
    ChootJump,
    /// <summary><c>choot_jump_apex</c>, an installed spritemap.</summary>
    ChootJumpApex,
    /// <summary><c>choot_fall_end</c>, an installed spritemap.</summary>
    ChootFallEnd,
    /// <summary><c>hzoomer_upside_right_0</c>, an installed spritemap.</summary>
    HzoomerUpsideRight0,
    /// <summary><c>hzoomer_upside_right_1</c>, an installed spritemap.</summary>
    HzoomerUpsideRight1,
    /// <summary><c>hzoomer_upside_right_2</c>, an installed spritemap.</summary>
    HzoomerUpsideRight2,
    /// <summary><c>hzoomer_upside_right_3</c>, an installed spritemap.</summary>
    HzoomerUpsideRight3,
    /// <summary><c>hzoomer_upside_right_4</c>, an installed spritemap.</summary>
    HzoomerUpsideRight4,
    /// <summary><c>hzoomer_upside_left_0</c>, an installed spritemap.</summary>
    HzoomerUpsideLeft0,
    /// <summary><c>hzoomer_upside_left_1</c>, an installed spritemap.</summary>
    HzoomerUpsideLeft1,
    /// <summary><c>hzoomer_upside_left_2</c>, an installed spritemap.</summary>
    HzoomerUpsideLeft2,
    /// <summary><c>hzoomer_upside_left_3</c>, an installed spritemap.</summary>
    HzoomerUpsideLeft3,
    /// <summary><c>hzoomer_upside_left_4</c>, an installed spritemap.</summary>
    HzoomerUpsideLeft4,
    /// <summary><c>hzoomer_upside_down_0</c>, an installed spritemap.</summary>
    HzoomerUpsideDown0,
    /// <summary><c>hzoomer_upside_down_1</c>, an installed spritemap.</summary>
    HzoomerUpsideDown1,
    /// <summary><c>hzoomer_upside_down_2</c>, an installed spritemap.</summary>
    HzoomerUpsideDown2,
    /// <summary><c>hzoomer_upside_down_3</c>, an installed spritemap.</summary>
    HzoomerUpsideDown3,
    /// <summary><c>hzoomer_upside_down_4</c>, an installed spritemap.</summary>
    HzoomerUpsideDown4,
    /// <summary><c>hzoomer_upside_up_0</c>, an installed spritemap.</summary>
    HzoomerUpsideUp0,
    /// <summary><c>hzoomer_upside_up_1</c>, an installed spritemap.</summary>
    HzoomerUpsideUp1,
    /// <summary><c>hzoomer_upside_up_2</c>, an installed spritemap.</summary>
    HzoomerUpsideUp2,
    /// <summary><c>hzoomer_upside_up_3</c>, an installed spritemap.</summary>
    HzoomerUpsideUp3,
    /// <summary><c>hzoomer_upside_up_4</c>, an installed spritemap.</summary>
    HzoomerUpsideUp4,
    /// <summary><c>sbug_right_0</c>, an installed spritemap.</summary>
    SbugRight0,
    /// <summary><c>sbug_right_1</c>, an installed spritemap.</summary>
    SbugRight1,
    /// <summary><c>sbug_right_2</c>, an installed spritemap.</summary>
    SbugRight2,
    /// <summary><c>sbug_up_right_0</c>, an installed spritemap.</summary>
    SbugUpRight0,
    /// <summary><c>sbug_up_right_1</c>, an installed spritemap.</summary>
    SbugUpRight1,
    /// <summary><c>sbug_up_right_2</c>, an installed spritemap.</summary>
    SbugUpRight2,
    /// <summary><c>sbug_up_0</c>, an installed spritemap.</summary>
    SbugUp0,
    /// <summary><c>sbug_up_1</c>, an installed spritemap.</summary>
    SbugUp1,
    /// <summary><c>sbug_up_2</c>, an installed spritemap.</summary>
    SbugUp2,
    /// <summary><c>sbug_up_left_0</c>, an installed spritemap.</summary>
    SbugUpLeft0,
    /// <summary><c>sbug_up_left_1</c>, an installed spritemap.</summary>
    SbugUpLeft1,
    /// <summary><c>sbug_up_left_2</c>, an installed spritemap.</summary>
    SbugUpLeft2,
    /// <summary><c>sbug_left_0</c>, an installed spritemap.</summary>
    SbugLeft0,
    /// <summary><c>sbug_left_1</c>, an installed spritemap.</summary>
    SbugLeft1,
    /// <summary><c>sbug_left_2</c>, an installed spritemap.</summary>
    SbugLeft2,
    /// <summary><c>sbug_down_left_0</c>, an installed spritemap.</summary>
    SbugDownLeft0,
    /// <summary><c>sbug_down_left_1</c>, an installed spritemap.</summary>
    SbugDownLeft1,
    /// <summary><c>sbug_down_left_2</c>, an installed spritemap.</summary>
    SbugDownLeft2,
    /// <summary><c>sbug_down_0</c>, an installed spritemap.</summary>
    SbugDown0,
    /// <summary><c>sbug_down_1</c>, an installed spritemap.</summary>
    SbugDown1,
    /// <summary><c>sbug_down_2</c>, an installed spritemap.</summary>
    SbugDown2,
    /// <summary><c>sbug_down_right_0</c>, an installed spritemap.</summary>
    SbugDownRight0,
    /// <summary><c>sbug_down_right_1</c>, an installed spritemap.</summary>
    SbugDownRight1,
    /// <summary><c>sbug_down_right_2</c>, an installed spritemap.</summary>
    SbugDownRight2,
    /// <summary><c>fune_left_idle</c>, an installed spritemap.</summary>
    FuneLeftIdle,
    /// <summary><c>fune_left_active_0</c>, an installed spritemap.</summary>
    FuneLeftActive0,
    /// <summary><c>fune_left_active_1</c>, an installed spritemap.</summary>
    FuneLeftActive1,
    /// <summary><c>fune_left_active_2</c>, an installed spritemap.</summary>
    FuneLeftActive2,
    /// <summary><c>fune_left_active_3</c>, an installed spritemap.</summary>
    FuneLeftActive3,
    /// <summary><c>fune_right_idle</c>, an installed spritemap.</summary>
    FuneRightIdle,
    /// <summary><c>fune_right_active_0</c>, an installed spritemap.</summary>
    FuneRightActive0,
    /// <summary><c>fune_right_active_1</c>, an installed spritemap.</summary>
    FuneRightActive1,
    /// <summary><c>fune_right_active_2</c>, an installed spritemap.</summary>
    FuneRightActive2,
    /// <summary><c>fune_right_active_3</c>, an installed spritemap.</summary>
    FuneRightActive3,
    /// <summary><c>namihe_left_idle</c>, an installed spritemap.</summary>
    NamiheLeftIdle,
    /// <summary><c>namihe_left_active_0</c>, an installed spritemap.</summary>
    NamiheLeftActive0,
    /// <summary><c>namihe_left_active_1</c>, an installed spritemap.</summary>
    NamiheLeftActive1,
    /// <summary><c>namihe_left_active_2</c>, an installed spritemap.</summary>
    NamiheLeftActive2,
    /// <summary><c>namihe_left_active_3</c>, an installed spritemap.</summary>
    NamiheLeftActive3,
    /// <summary><c>namihe_left_active_4</c>, an installed spritemap.</summary>
    NamiheLeftActive4,
    /// <summary><c>namihe_right_idle</c>, an installed spritemap.</summary>
    NamiheRightIdle,
    /// <summary><c>namihe_right_active_0</c>, an installed spritemap.</summary>
    NamiheRightActive0,
    /// <summary><c>namihe_right_active_1</c>, an installed spritemap.</summary>
    NamiheRightActive1,
    /// <summary><c>namihe_right_active_2</c>, an installed spritemap.</summary>
    NamiheRightActive2,
    /// <summary><c>namihe_right_active_3</c>, an installed spritemap.</summary>
    NamiheRightActive3,
    /// <summary><c>namihe_right_active_4</c>, an installed spritemap.</summary>
    NamiheRightActive4,
    /// <summary><c>kamer_platform_0</c>, an installed spritemap.</summary>
    KamerPlatform0,
    /// <summary><c>kamer_platform_1</c>, an installed spritemap.</summary>
    KamerPlatform1,
    /// <summary><c>kamer_platform_2</c>, an installed spritemap.</summary>
    KamerPlatform2,
    /// <summary><c>kamer_platform_3</c>, an installed spritemap.</summary>
    KamerPlatform3,
    /// <summary><c>elevator_platform_0</c>, an installed spritemap.</summary>
    ElevatorPlatform0,
    /// <summary><c>elevator_platform_1</c>, an installed spritemap.</summary>
    ElevatorPlatform1,
    /// <summary><c>draygon_intro_evir_0</c>, an installed spritemap.</summary>
    DraygonIntroEvir0,
    /// <summary><c>draygon_intro_evir_1</c>, an installed spritemap.</summary>
    DraygonIntroEvir1,
    /// <summary><c>draygon_intro_evir_2</c>, an installed spritemap.</summary>
    DraygonIntroEvir2,
    /// <summary><c>draygon_intro_evir_3</c>, an installed spritemap.</summary>
    DraygonIntroEvir3,
    /// <summary><c>draygon_breath_bubble_0</c>, an installed spritemap.</summary>
    DraygonBreathBubble0,
    /// <summary><c>draygon_breath_bubble_1</c>, an installed spritemap.</summary>
    DraygonBreathBubble1,
    /// <summary><c>draygon_breath_bubble_2</c>, an installed spritemap.</summary>
    DraygonBreathBubble2,
    /// <summary><c>draygon_breath_bubble_3</c>, an installed spritemap.</summary>
    DraygonBreathBubble3,
    /// <summary><c>draygon_breath_bubble_4</c>, an installed spritemap.</summary>
    DraygonBreathBubble4,
    /// <summary><c>draygon_breath_bubble_5</c>, an installed spritemap.</summary>
    DraygonBreathBubble5,
    /// <summary><c>draygon_breath_bubble_6</c>, an installed spritemap.</summary>
    DraygonBreathBubble6,
    /// <summary><c>draygon_breath_bubble_7</c>, an installed spritemap.</summary>
    DraygonBreathBubble7,
    /// <summary><c>draygon_breath_bubble_8</c>, an installed spritemap.</summary>
    DraygonBreathBubble8,
    /// <summary><c>sprite_object_empty</c>, an installed spritemap.</summary>
    SpriteObjectEmpty,
    /// <summary><c>sprite_object_unused_object_30_frame_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject30Frame0,
    /// <summary><c>sprite_object_unused_object_30_frame_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject30Frame1,
    /// <summary><c>sprite_object_unused_object_30_frame_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject30Frame2,
    /// <summary><c>sprite_object_unused_object_31_frame_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject31Frame0,
    /// <summary><c>sprite_object_unused_object_31_frame_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject31Frame1,
    /// <summary><c>sprite_object_unused_object_31_frame_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject31Frame2,
    /// <summary><c>sprite_object_unused_object_32_frame_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject32Frame0,
    /// <summary><c>sprite_object_unused_object_32_frame_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject32Frame1,
    /// <summary><c>sprite_object_unused_object_32_frame_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject32Frame2,
    /// <summary><c>sprite_object_unused_object_33_frame_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject33Frame0,
    /// <summary><c>sprite_object_unused_object_33_frame_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject33Frame1,
    /// <summary><c>sprite_object_unused_object_33_frame_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject33Frame2,
    /// <summary><c>sprite_object_unused_object_34_frame_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject34Frame0,
    /// <summary><c>sprite_object_unused_object_34_frame_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject34Frame1,
    /// <summary><c>sprite_object_unused_object_34_frame_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject34Frame2,
    /// <summary><c>sprite_object_unused_object_35_frame_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject35Frame0,
    /// <summary><c>sprite_object_unused_object_35_frame_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject35Frame1,
    /// <summary><c>sprite_object_unused_object_35_frame_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject35Frame2,
    /// <summary><c>sprite_object_unused_object_36_frame_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject36Frame0,
    /// <summary><c>sprite_object_unused_object_36_frame_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject36Frame1,
    /// <summary><c>sprite_object_unused_object_36_frame_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject36Frame2,
    /// <summary><c>sprite_object_unused_object_37_frame_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject37Frame0,
    /// <summary><c>sprite_object_unused_object_37_frame_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject37Frame1,
    /// <summary><c>sprite_object_unused_object_37_frame_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject37Frame2,
    /// <summary><c>sprite_object_unused_rock_particles_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles0,
    /// <summary><c>sprite_object_unused_rock_particles_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles1,
    /// <summary><c>sprite_object_unused_rock_particles_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles2,
    /// <summary><c>sprite_object_unused_rock_particles_3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles3,
    /// <summary><c>sprite_object_unused_rock_particles_4</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles4,
    /// <summary><c>sprite_object_unused_rock_particles_5</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles5,
    /// <summary><c>sprite_object_unused_rock_particles_6</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles6,
    /// <summary><c>sprite_object_unused_rock_particles_7</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles7,
    /// <summary><c>sprite_object_unused_rock_particles_8</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles8,
    /// <summary><c>sprite_object_unused_rock_particles_9</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles9,
    /// <summary><c>sprite_object_unused_rock_particles_10</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles10,
    /// <summary><c>sprite_object_unused_rock_particles_11</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles11,
    /// <summary><c>sprite_object_unused_rock_particles_12</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles12,
    /// <summary><c>sprite_object_unused_rock_particles_13</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles13,
    /// <summary><c>sprite_object_unused_rock_particles_14</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles14,
    /// <summary><c>sprite_object_unused_rock_particles_15</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles15,
    /// <summary><c>sprite_object_big_dust_cloud_0</c>, an installed spritemap.</summary>
    SpriteObjectBigDustCloud0,
    /// <summary><c>sprite_object_big_dust_cloud_1</c>, an installed spritemap.</summary>
    SpriteObjectBigDustCloud1,
    /// <summary><c>sprite_object_big_dust_cloud_2</c>, an installed spritemap.</summary>
    SpriteObjectBigDustCloud2,
    /// <summary><c>sprite_object_big_dust_cloud_3</c>, an installed spritemap.</summary>
    SpriteObjectBigDustCloud3,
    /// <summary><c>sprite_object_big_dust_cloud_4</c>, an installed spritemap.</summary>
    SpriteObjectBigDustCloud4,
    /// <summary><c>sprite_object_big_dust_cloud_5</c>, an installed spritemap.</summary>
    SpriteObjectBigDustCloud5,
    /// <summary><c>sprite_object_big_dust_cloud_6</c>, an installed spritemap.</summary>
    SpriteObjectBigDustCloud6,
    /// <summary><c>sprite_object_big_dust_cloud_7</c>, an installed spritemap.</summary>
    SpriteObjectBigDustCloud7,
    /// <summary><c>sprite_object_unused_dust_cloud_beam_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedDustCloudBeam0,
    /// <summary><c>sprite_object_unused_dust_cloud_beam_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedDustCloudBeam1,
    /// <summary><c>sprite_object_unused_dust_cloud_beam_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedDustCloudBeam2,
    /// <summary><c>sprite_object_unused_dust_cloud_beam_3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedDustCloudBeam3,
    /// <summary><c>sprite_object_unused_dust_cloud_beam_4</c>, an installed spritemap.</summary>
    SpriteObjectUnusedDustCloudBeam4,
    /// <summary><c>sprite_object_unused_dust_cloud_beam_5</c>, an installed spritemap.</summary>
    SpriteObjectUnusedDustCloudBeam5,
    /// <summary><c>sprite_object_unused_dust_cloud_beam_6</c>, an installed spritemap.</summary>
    SpriteObjectUnusedDustCloudBeam6,
    /// <summary><c>sprite_object_unused_dust_cloud_beam_7</c>, an installed spritemap.</summary>
    SpriteObjectUnusedDustCloudBeam7,
    /// <summary><c>sprite_object_unused_save_station_electricity_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSaveStationElectricity0,
    /// <summary><c>sprite_object_unused_save_station_electricity_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSaveStationElectricity1,
    /// <summary><c>sprite_object_unused_save_station_electricity_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSaveStationElectricity2,
    /// <summary><c>sprite_object_unused_save_station_electricity_3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSaveStationElectricity3,
    /// <summary><c>sprite_object_unused_save_station_electricity_4</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSaveStationElectricity4,
    /// <summary><c>sprite_object_unused_save_station_electricity_5</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSaveStationElectricity5,
    /// <summary><c>sprite_object_unused_save_station_electricity_6</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSaveStationElectricity6,
    /// <summary><c>sprite_object_unused_save_station_electricity_7</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSaveStationElectricity7,
    /// <summary><c>sprite_object_unused_vertical_gate_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedVerticalGate0,
    /// <summary><c>sprite_object_unused_vertical_gate_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedVerticalGate1,
    /// <summary><c>sprite_object_unused_vertical_gate_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedVerticalGate2,
    /// <summary><c>sprite_object_unused_vertical_gate_3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedVerticalGate3,
    /// <summary><c>sprite_object_unused_vertical_gate_4</c>, an installed spritemap.</summary>
    SpriteObjectUnusedVerticalGate4,
    /// <summary><c>sprite_object_unused_vertical_gate_5</c>, an installed spritemap.</summary>
    SpriteObjectUnusedVerticalGate5,
    /// <summary><c>sprite_object_unused_vertical_gate_6</c>, an installed spritemap.</summary>
    SpriteObjectUnusedVerticalGate6,
    /// <summary><c>sprite_object_unused_vertical_gate_7</c>, an installed spritemap.</summary>
    SpriteObjectUnusedVerticalGate7,
    /// <summary><c>sprite_object_unused_beam_charge_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBeamCharge0,
    /// <summary><c>sprite_object_unused_beam_charge_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBeamCharge1,
    /// <summary><c>sprite_object_unused_beam_charge_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBeamCharge2,
    /// <summary><c>sprite_object_unused_beam_charge_3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBeamCharge3,
    /// <summary><c>sprite_object_unused_mother_brain_elbow_particle_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowParticle0,
    /// <summary><c>sprite_object_unused_mother_brain_elbow_particle_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowParticle1,
    /// <summary><c>sprite_object_unused_mother_brain_elbow_particle_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowParticle2,
    /// <summary><c>sprite_object_unused_mother_brain_elbow_particle_3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowParticle3,
    /// <summary><c>sprite_object_unused_mother_brain_elbow_particle_4</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowParticle4,
    /// <summary><c>sprite_object_unused_mother_brain_elbow_particle_5</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowParticle5,
    /// <summary><c>sprite_object_unused_mother_brain_elbow_energy_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowEnergy0,
    /// <summary><c>sprite_object_unused_mother_brain_elbow_energy_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowEnergy1,
    /// <summary><c>sprite_object_unused_mother_brain_elbow_energy_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowEnergy2,
    /// <summary><c>sprite_object_unused_mother_brain_elbow_energy_3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowEnergy3,
    /// <summary><c>sprite_object_unused_mother_brain_elbow_energy_4</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowEnergy4,
    /// <summary><c>sprite_object_unused_mother_brain_elbow_energy_5</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowEnergy5,
    /// <summary><c>sprite_object_small_explosion_0</c>, an installed spritemap.</summary>
    SpriteObjectSmallExplosion0,
    /// <summary><c>sprite_object_small_explosion_1</c>, an installed spritemap.</summary>
    SpriteObjectSmallExplosion1,
    /// <summary><c>sprite_object_small_explosion_2</c>, an installed spritemap.</summary>
    SpriteObjectSmallExplosion2,
    /// <summary><c>sprite_object_small_explosion_3</c>, an installed spritemap.</summary>
    SpriteObjectSmallExplosion3,
    /// <summary><c>sprite_object_small_explosion_4</c>, an installed spritemap.</summary>
    SpriteObjectSmallExplosion4,
    /// <summary><c>sprite_object_small_explosion_5</c>, an installed spritemap.</summary>
    SpriteObjectSmallExplosion5,
    /// <summary><c>sprite_object_unused_bomb_explosion_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBombExplosion0,
    /// <summary><c>sprite_object_unused_bomb_explosion_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBombExplosion1,
    /// <summary><c>sprite_object_unused_bomb_explosion_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBombExplosion2,
    /// <summary><c>sprite_object_unused_bomb_explosion_3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBombExplosion3,
    /// <summary><c>sprite_object_unused_bomb_explosion_4</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBombExplosion4,
    /// <summary><c>sprite_object_unused_beam_trail_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBeamTrail0,
    /// <summary><c>sprite_object_unused_beam_trail_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBeamTrail1,
    /// <summary><c>sprite_object_unused_beam_trail_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBeamTrail2,
    /// <summary><c>sprite_object_unused_beam_trail_3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBeamTrail3,
    /// <summary><c>sprite_object_unused_power_bomb_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedPowerBomb0,
    /// <summary><c>sprite_object_unused_power_bomb_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedPowerBomb1,
    /// <summary><c>sprite_object_unused_power_bomb_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedPowerBomb2,
    /// <summary><c>sprite_object_dud_shot_0</c>, an installed spritemap.</summary>
    SpriteObjectDudShot0,
    /// <summary><c>sprite_object_dud_shot_1</c>, an installed spritemap.</summary>
    SpriteObjectDudShot1,
    /// <summary><c>sprite_object_dud_shot_2</c>, an installed spritemap.</summary>
    SpriteObjectDudShot2,
    /// <summary><c>sprite_object_dud_shot_3</c>, an installed spritemap.</summary>
    SpriteObjectDudShot3,
    /// <summary><c>sprite_object_dud_shot_4</c>, an installed spritemap.</summary>
    SpriteObjectDudShot4,
    /// <summary><c>sprite_object_dud_shot_5</c>, an installed spritemap.</summary>
    SpriteObjectDudShot5,
    /// <summary><c>sprite_object_unused_elevator_pad_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedElevatorPad0,
    /// <summary><c>sprite_object_unused_elevator_pad_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedElevatorPad1,
    /// <summary><c>sprite_object_small_dust_cloud_0</c>, an installed spritemap.</summary>
    SpriteObjectSmallDustCloud0,
    /// <summary><c>sprite_object_small_dust_cloud_1</c>, an installed spritemap.</summary>
    SpriteObjectSmallDustCloud1,
    /// <summary><c>sprite_object_small_dust_cloud_2</c>, an installed spritemap.</summary>
    SpriteObjectSmallDustCloud2,
    /// <summary><c>sprite_object_small_dust_cloud_3</c>, an installed spritemap.</summary>
    SpriteObjectSmallDustCloud3,
    /// <summary><c>sprite_object_pirate_landing_dust_cloud_0</c>, an installed spritemap.</summary>
    SpriteObjectPirateLandingDustCloud0,
    /// <summary><c>sprite_object_pirate_landing_dust_cloud_1</c>, an installed spritemap.</summary>
    SpriteObjectPirateLandingDustCloud1,
    /// <summary><c>sprite_object_pirate_landing_dust_cloud_2</c>, an installed spritemap.</summary>
    SpriteObjectPirateLandingDustCloud2,
    /// <summary><c>sprite_object_pirate_landing_dust_cloud_3</c>, an installed spritemap.</summary>
    SpriteObjectPirateLandingDustCloud3,
    /// <summary><c>sprite_object_unused_eye_door_sweat_drop_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedEyeDoorSweatDrop0,
    /// <summary><c>sprite_object_unused_eye_door_sweat_drop_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedEyeDoorSweatDrop1,
    /// <summary><c>sprite_object_unused_eye_door_sweat_drop_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedEyeDoorSweatDrop2,
    /// <summary><c>sprite_object_unused_eye_door_sweat_drop_3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedEyeDoorSweatDrop3,
    /// <summary><c>sprite_object_smoke_0</c>, an installed spritemap.</summary>
    SpriteObjectSmoke0,
    /// <summary><c>sprite_object_smoke_1</c>, an installed spritemap.</summary>
    SpriteObjectSmoke1,
    /// <summary><c>sprite_object_smoke_2</c>, an installed spritemap.</summary>
    SpriteObjectSmoke2,
    /// <summary><c>sprite_object_smoke_3</c>, an installed spritemap.</summary>
    SpriteObjectSmoke3,
    /// <summary><c>sprite_object_unused_small_energy_drop_object_13_frame_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSmallEnergyDropObject13Frame0,
    /// <summary><c>sprite_object_unused_small_energy_drop_object_13_frame_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSmallEnergyDropObject13Frame1,
    /// <summary><c>sprite_object_unused_small_energy_drop_object_13_frame_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSmallEnergyDropObject13Frame2,
    /// <summary><c>sprite_object_unused_small_energy_drop_object_13_frame_3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSmallEnergyDropObject13Frame3,
    /// <summary><c>sprite_object_unused_big_energy_drop_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBigEnergyDrop0,
    /// <summary><c>sprite_object_unused_big_energy_drop_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBigEnergyDrop1,
    /// <summary><c>sprite_object_unused_big_energy_drop_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBigEnergyDrop2,
    /// <summary><c>sprite_object_unused_big_energy_drop_3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBigEnergyDrop3,
    /// <summary><c>sprite_object_unused_bomb_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBomb0,
    /// <summary><c>sprite_object_unused_bomb_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBomb1,
    /// <summary><c>sprite_object_unused_bomb_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBomb2,
    /// <summary><c>sprite_object_unused_bomb_3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBomb3,
    /// <summary><c>sprite_object_unused_small_energy_drop_object_16_frame_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSmallEnergyDropObject16Frame0,
    /// <summary><c>sprite_object_unused_small_energy_drop_object_16_frame_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSmallEnergyDropObject16Frame1,
    /// <summary><c>sprite_object_unused_small_energy_drop_object_16_frame_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSmallEnergyDropObject16Frame2,
    /// <summary><c>sprite_object_big_explosion_0</c>, an installed spritemap.</summary>
    SpriteObjectBigExplosion0,
    /// <summary><c>sprite_object_big_explosion_1</c>, an installed spritemap.</summary>
    SpriteObjectBigExplosion1,
    /// <summary><c>sprite_object_big_explosion_2</c>, an installed spritemap.</summary>
    SpriteObjectBigExplosion2,
    /// <summary><c>sprite_object_big_explosion_3</c>, an installed spritemap.</summary>
    SpriteObjectBigExplosion3,
    /// <summary><c>sprite_object_big_explosion_4</c>, an installed spritemap.</summary>
    SpriteObjectBigExplosion4,
    /// <summary><c>sprite_object_big_explosion_5</c>, an installed spritemap.</summary>
    SpriteObjectBigExplosion5,
    /// <summary><c>sprite_object_unused_object_38_frame_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject38Frame0,
    /// <summary><c>sprite_object_unused_object_38_frame_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject38Frame1,
    /// <summary><c>sprite_object_unused_object_38_frame_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject38Frame2,
    /// <summary><c>sprite_object_unused_object_38_frame_3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject38Frame3,
    /// <summary><c>sprite_object_unused_object_38_frame_4</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject38Frame4,
    /// <summary><c>sprite_object_unused_object_38_frame_5</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject38Frame5,
    /// <summary><c>sprite_object_unused_object_39_frame_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject39Frame0,
    /// <summary><c>sprite_object_unused_object_39_frame_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject39Frame1,
    /// <summary><c>sprite_object_unused_object_39_frame_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject39Frame2,
    /// <summary><c>sprite_object_unused_object_39_frame_3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject39Frame3,
    /// <summary><c>sprite_object_unused_object_39_frame_4</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject39Frame4,
    /// <summary><c>sprite_object_unused_object_42</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject42,
    /// <summary><c>sprite_object_unused_object_40_frame_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject40Frame0,
    /// <summary><c>sprite_object_unused_object_40_frame_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject40Frame1,
    /// <summary><c>sprite_object_unused_object_40_frame_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject40Frame2,
    /// <summary><c>sprite_object_unused_object_40_frame_3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject40Frame3,
    /// <summary><c>sprite_object_unused_object_40_frame_4</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject40Frame4,
    /// <summary><c>sprite_object_unused_object_40_frame_5</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject40Frame5,
    /// <summary><c>sprite_object_unused_object_41_frame_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject41Frame0,
    /// <summary><c>sprite_object_unused_object_41_frame_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject41Frame1,
    /// <summary><c>sprite_object_unused_object_41_frame_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject41Frame2,
    /// <summary><c>sprite_object_unused_object_41_frame_3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject41Frame3,
    /// <summary><c>sprite_object_unused_object_41_frame_4</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject41Frame4,
    /// <summary><c>sprite_object_puromi_0</c>, an installed spritemap.</summary>
    SpriteObjectPuromi0,
    /// <summary><c>sprite_object_puromi_1</c>, an installed spritemap.</summary>
    SpriteObjectPuromi1,
    /// <summary><c>sprite_object_puromi_2</c>, an installed spritemap.</summary>
    SpriteObjectPuromi2,
    /// <summary><c>sprite_object_puromi_3</c>, an installed spritemap.</summary>
    SpriteObjectPuromi3,
    /// <summary><c>sprite_object_puromi_4</c>, an installed spritemap.</summary>
    SpriteObjectPuromi4,
    /// <summary><c>sprite_object_puromi_5</c>, an installed spritemap.</summary>
    SpriteObjectPuromi5,
    /// <summary><c>sprite_object_puromi_6</c>, an installed spritemap.</summary>
    SpriteObjectPuromi6,
    /// <summary><c>sprite_object_puromi_7</c>, an installed spritemap.</summary>
    SpriteObjectPuromi7,
    /// <summary><c>sprite_object_puromi_right_explosion_0</c>, an installed spritemap.</summary>
    SpriteObjectPuromiRightExplosion0,
    /// <summary><c>sprite_object_puromi_right_explosion_1</c>, an installed spritemap.</summary>
    SpriteObjectPuromiRightExplosion1,
    /// <summary><c>sprite_object_puromi_right_explosion_2</c>, an installed spritemap.</summary>
    SpriteObjectPuromiRightExplosion2,
    /// <summary><c>sprite_object_puromi_right_explosion_3</c>, an installed spritemap.</summary>
    SpriteObjectPuromiRightExplosion3,
    /// <summary><c>sprite_object_puromi_right_explosion_4</c>, an installed spritemap.</summary>
    SpriteObjectPuromiRightExplosion4,
    /// <summary><c>sprite_object_puromi_right_explosion_5</c>, an installed spritemap.</summary>
    SpriteObjectPuromiRightExplosion5,
    /// <summary><c>sprite_object_puromi_right_explosion_6</c>, an installed spritemap.</summary>
    SpriteObjectPuromiRightExplosion6,
    /// <summary><c>sprite_object_puromi_left_explosion_0</c>, an installed spritemap.</summary>
    SpriteObjectPuromiLeftExplosion0,
    /// <summary><c>sprite_object_puromi_left_explosion_1</c>, an installed spritemap.</summary>
    SpriteObjectPuromiLeftExplosion1,
    /// <summary><c>sprite_object_puromi_left_explosion_2</c>, an installed spritemap.</summary>
    SpriteObjectPuromiLeftExplosion2,
    /// <summary><c>sprite_object_puromi_left_explosion_3</c>, an installed spritemap.</summary>
    SpriteObjectPuromiLeftExplosion3,
    /// <summary><c>sprite_object_puromi_left_explosion_4</c>, an installed spritemap.</summary>
    SpriteObjectPuromiLeftExplosion4,
    /// <summary><c>sprite_object_puromi_left_explosion_5</c>, an installed spritemap.</summary>
    SpriteObjectPuromiLeftExplosion5,
    /// <summary><c>sprite_object_puromi_left_explosion_6</c>, an installed spritemap.</summary>
    SpriteObjectPuromiLeftExplosion6,
    /// <summary><c>sprite_object_puromi_splash_0</c>, an installed spritemap.</summary>
    SpriteObjectPuromiSplash0,
    /// <summary><c>sprite_object_puromi_splash_1</c>, an installed spritemap.</summary>
    SpriteObjectPuromiSplash1,
    /// <summary><c>sprite_object_puromi_splash_2</c>, an installed spritemap.</summary>
    SpriteObjectPuromiSplash2,
    /// <summary><c>sprite_object_puromi_splash_4</c>, an installed spritemap.</summary>
    SpriteObjectPuromiSplash4,
    /// <summary><c>sprite_object_unused_object_47</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject47,
    /// <summary><c>sprite_object_falling_spark_trail_0</c>, an installed spritemap.</summary>
    SpriteObjectFallingSparkTrail0,
    /// <summary><c>sprite_object_falling_spark_trail_1</c>, an installed spritemap.</summary>
    SpriteObjectFallingSparkTrail1,
    /// <summary><c>sprite_object_falling_spark_trail_2</c>, an installed spritemap.</summary>
    SpriteObjectFallingSparkTrail2,
    /// <summary><c>sprite_object_falling_spark_trail_3</c>, an installed spritemap.</summary>
    SpriteObjectFallingSparkTrail3,
    /// <summary><c>sprite_object_metroid_shell_0</c>, an installed spritemap.</summary>
    SpriteObjectMetroidShell0,
    /// <summary><c>sprite_object_metroid_shell_1</c>, an installed spritemap.</summary>
    SpriteObjectMetroidShell1,
    /// <summary><c>sprite_object_metroid_shell_2</c>, an installed spritemap.</summary>
    SpriteObjectMetroidShell2,
    /// <summary><c>sprite_object_unused_metroid_insides_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMetroidInsides0,
    /// <summary><c>sprite_object_unused_metroid_insides_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMetroidInsides1,
    /// <summary><c>sprite_object_unused_metroid_insides_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMetroidInsides2,
    /// <summary><c>sprite_object_unused_metroid_insides_3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMetroidInsides3,
    /// <summary><c>sprite_object_metroid_electricity_0</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity0,
    /// <summary><c>sprite_object_metroid_electricity_1</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity1,
    /// <summary><c>sprite_object_metroid_electricity_2</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity2,
    /// <summary><c>sprite_object_metroid_electricity_3</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity3,
    /// <summary><c>sprite_object_metroid_electricity_4</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity4,
    /// <summary><c>sprite_object_metroid_electricity_5</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity5,
    /// <summary><c>sprite_object_metroid_electricity_6</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity6,
    /// <summary><c>sprite_object_metroid_electricity_7</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity7,
    /// <summary><c>sprite_object_metroid_electricity_8</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity8,
    /// <summary><c>sprite_object_metroid_electricity_9</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity9,
    /// <summary><c>sprite_object_metroid_electricity_10</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity10,
    /// <summary><c>sprite_object_metroid_electricity_11</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity11,
    /// <summary><c>sprite_object_metroid_electricity_12</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity12,
    /// <summary><c>sprite_object_metroid_electricity_13</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity13,
    /// <summary><c>sprite_object_metroid_electricity_14</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity14,
    /// <summary><c>sprite_object_metroid_electricity_15</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity15,
    /// <summary><c>sprite_object_metroid_electricity_16</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity16,
    /// <summary><c>sprite_object_metroid_electricity_17</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity17,
    /// <summary><c>sprite_object_metroid_electricity_18</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity18,
    /// <summary><c>sprite_object_metroid_electricity_19</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity19,
    /// <summary><c>sprite_object_metroid_electricity_20</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity20,
    /// <summary><c>sprite_object_metroid_electricity_21</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity21,
    /// <summary><c>sprite_object_metroid_electricity_22</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity22,
    /// <summary><c>sprite_object_metroid_electricity_23</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity23,
    /// <summary><c>sprite_object_enemy_shot_0</c>, an installed spritemap.</summary>
    SpriteObjectEnemyShot0,
    /// <summary><c>sprite_object_enemy_shot_1</c>, an installed spritemap.</summary>
    SpriteObjectEnemyShot1,
    /// <summary><c>sprite_object_enemy_shot_2</c>, an installed spritemap.</summary>
    SpriteObjectEnemyShot2,
    /// <summary><c>sprite_object_enemy_shot_3</c>, an installed spritemap.</summary>
    SpriteObjectEnemyShot3,
    /// <summary><c>sprite_object_yapping_maw_base_facing_down</c>, an installed spritemap.</summary>
    SpriteObjectYappingMawBaseFacingDown,
    /// <summary><c>sprite_object_yapping_maw_base_facing_up</c>, an installed spritemap.</summary>
    SpriteObjectYappingMawBaseFacingUp,
    /// <summary><c>sprite_object_unused_object_58_frame_0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject58Frame0,
    /// <summary><c>sprite_object_unused_object_58_frame_1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject58Frame1,
    /// <summary><c>sprite_object_unused_object_58_frame_2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject58Frame2,
    /// <summary><c>sprite_object_unused_object_58_frame_3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject58Frame3,
    /// <summary><c>sprite_object_unused_object_58_frame_4</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject58Frame4,
    /// <summary><c>sprite_object_unused_object_58_frame_5</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject58Frame5,
    /// <summary><c>sprite_object_unused_object_58_frame_6</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject58Frame6,
    /// <summary><c>sprite_object_unused_object_58_frame_7</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject58Frame7,
    /// <summary><c>sprite_object_evir_facing_right_0</c>, an installed spritemap.</summary>
    SpriteObjectEvirFacingRight0,
    /// <summary><c>sprite_object_evir_facing_right_1</c>, an installed spritemap.</summary>
    SpriteObjectEvirFacingRight1,
    /// <summary><c>sprite_object_evir_facing_right_2</c>, an installed spritemap.</summary>
    SpriteObjectEvirFacingRight2,
    /// <summary><c>sprite_object_evir_facing_right_3</c>, an installed spritemap.</summary>
    SpriteObjectEvirFacingRight3,
    /// <summary><c>sprite_object_draygon_foaming_at_the_mouth_0</c>, an installed spritemap.</summary>
    SpriteObjectDraygonFoamingAtTheMouth0,
    /// <summary><c>sprite_object_draygon_foaming_at_the_mouth_1</c>, an installed spritemap.</summary>
    SpriteObjectDraygonFoamingAtTheMouth1,
    /// <summary><c>sprite_object_draygon_foaming_at_the_mouth_2</c>, an installed spritemap.</summary>
    SpriteObjectDraygonFoamingAtTheMouth2,
    /// <summary><c>sprite_object_draygon_foaming_at_the_mouth_3</c>, an installed spritemap.</summary>
    SpriteObjectDraygonFoamingAtTheMouth3,
    /// <summary><c>sprite_object_draygon_foaming_at_the_mouth_4</c>, an installed spritemap.</summary>
    SpriteObjectDraygonFoamingAtTheMouth4,
    /// <summary><c>sprite_object_draygon_foaming_at_the_mouth_5</c>, an installed spritemap.</summary>
    SpriteObjectDraygonFoamingAtTheMouth5,
    /// <summary><c>sprite_object_draygon_foaming_at_the_mouth_6</c>, an installed spritemap.</summary>
    SpriteObjectDraygonFoamingAtTheMouth6,
    /// <summary><c>sprite_object_draygon_foaming_at_the_mouth_7</c>, an installed spritemap.</summary>
    SpriteObjectDraygonFoamingAtTheMouth7,
    /// <summary><c>yapping_maw_pose_0</c>, an installed spritemap.</summary>
    YappingMawPose0,
    /// <summary><c>yapping_maw_pose_1</c>, an installed spritemap.</summary>
    YappingMawPose1,
    /// <summary><c>yapping_maw_pose_2</c>, an installed spritemap.</summary>
    YappingMawPose2,
    /// <summary><c>yapping_maw_pose_3</c>, an installed spritemap.</summary>
    YappingMawPose3,
    /// <summary><c>yapping_maw_pose_4</c>, an installed spritemap.</summary>
    YappingMawPose4,
    /// <summary><c>yapping_maw_pose_5</c>, an installed spritemap.</summary>
    YappingMawPose5,
    /// <summary><c>yapping_maw_pose_6</c>, an installed spritemap.</summary>
    YappingMawPose6,
    /// <summary><c>yapping_maw_pose_7</c>, an installed spritemap.</summary>
    YappingMawPose7,
    /// <summary><c>yapping_maw_pose_8</c>, an installed spritemap.</summary>
    YappingMawPose8,
    /// <summary><c>yapping_maw_pose_9</c>, an installed spritemap.</summary>
    YappingMawPose9,
    /// <summary><c>yapping_maw_pose_10</c>, an installed spritemap.</summary>
    YappingMawPose10,
    /// <summary><c>yapping_maw_pose_11</c>, an installed spritemap.</summary>
    YappingMawPose11,
    /// <summary><c>yapping_maw_pose_12</c>, an installed spritemap.</summary>
    YappingMawPose12,
    /// <summary><c>yapping_maw_pose_13</c>, an installed spritemap.</summary>
    YappingMawPose13,
    /// <summary><c>yapping_maw_pose_14</c>, an installed spritemap.</summary>
    YappingMawPose14,
    /// <summary><c>yapping_maw_pose_15</c>, an installed spritemap.</summary>
    YappingMawPose15,
    /// <summary><c>yapping_maw_pose_16</c>, an installed spritemap.</summary>
    YappingMawPose16,
    /// <summary><c>yapping_maw_pose_17</c>, an installed spritemap.</summary>
    YappingMawPose17,
    /// <summary><c>yapping_maw_pose_18</c>, an installed spritemap.</summary>
    YappingMawPose18,
    /// <summary><c>yapping_maw_pose_19</c>, an installed spritemap.</summary>
    YappingMawPose19,
    /// <summary><c>yapping_maw_pose_20</c>, an installed spritemap.</summary>
    YappingMawPose20,
    /// <summary><c>yapping_maw_pose_21</c>, an installed spritemap.</summary>
    YappingMawPose21,
    /// <summary><c>yapping_maw_pose_22</c>, an installed spritemap.</summary>
    YappingMawPose22,
    /// <summary><c>yapping_maw_pose_23</c>, an installed spritemap.</summary>
    YappingMawPose23,
    /// <summary><c>ki_hunter_pose_0</c>, an installed spritemap.</summary>
    KiHunterPose0,
    /// <summary><c>ki_hunter_pose_1</c>, an installed spritemap.</summary>
    KiHunterPose1,
    /// <summary><c>ki_hunter_pose_2</c>, an installed spritemap.</summary>
    KiHunterPose2,
    /// <summary><c>ki_hunter_pose_3</c>, an installed spritemap.</summary>
    KiHunterPose3,
    /// <summary><c>ki_hunter_pose_4</c>, an installed spritemap.</summary>
    KiHunterPose4,
    /// <summary><c>ki_hunter_pose_5</c>, an installed spritemap.</summary>
    KiHunterPose5,
    /// <summary><c>ki_hunter_pose_6</c>, an installed spritemap.</summary>
    KiHunterPose6,
    /// <summary><c>ki_hunter_pose_7</c>, an installed spritemap.</summary>
    KiHunterPose7,
    /// <summary><c>ki_hunter_pose_8</c>, an installed spritemap.</summary>
    KiHunterPose8,
    /// <summary><c>ki_hunter_pose_9</c>, an installed spritemap.</summary>
    KiHunterPose9,
    /// <summary><c>ki_hunter_pose_10</c>, an installed spritemap.</summary>
    KiHunterPose10,
    /// <summary><c>ki_hunter_pose_11</c>, an installed spritemap.</summary>
    KiHunterPose11,
    /// <summary><c>ki_hunter_pose_12</c>, an installed spritemap.</summary>
    KiHunterPose12,
    /// <summary><c>ki_hunter_pose_13</c>, an installed spritemap.</summary>
    KiHunterPose13,
    /// <summary><c>ki_hunter_pose_14</c>, an installed spritemap.</summary>
    KiHunterPose14,
    /// <summary><c>ki_hunter_pose_15</c>, an installed spritemap.</summary>
    KiHunterPose15,
    /// <summary><c>ki_hunter_pose_16</c>, an installed spritemap.</summary>
    KiHunterPose16,
    /// <summary><c>ki_hunter_pose_17</c>, an installed spritemap.</summary>
    KiHunterPose17,
    /// <summary><c>ki_hunter_pose_18</c>, an installed spritemap.</summary>
    KiHunterPose18,
    /// <summary><c>ki_hunter_pose_19</c>, an installed spritemap.</summary>
    KiHunterPose19,
    /// <summary><c>ki_hunter_pose_20</c>, an installed spritemap.</summary>
    KiHunterPose20,
    /// <summary><c>ki_hunter_pose_21</c>, an installed spritemap.</summary>
    KiHunterPose21,
    /// <summary><c>ki_hunter_pose_22</c>, an installed spritemap.</summary>
    KiHunterPose22,
    /// <summary><c>ki_hunter_pose_23</c>, an installed spritemap.</summary>
    KiHunterPose23,
    /// <summary><c>ki_hunter_pose_24</c>, an installed spritemap.</summary>
    KiHunterPose24,
    /// <summary><c>ki_hunter_pose_25</c>, an installed spritemap.</summary>
    KiHunterPose25,
    /// <summary><c>ki_hunter_pose_26</c>, an installed spritemap.</summary>
    KiHunterPose26,
    /// <summary><c>ki_hunter_pose_27</c>, an installed spritemap.</summary>
    KiHunterPose27,
    /// <summary><c>ki_hunter_pose_28</c>, an installed spritemap.</summary>
    KiHunterPose28,
    /// <summary><c>ki_hunter_pose_29</c>, an installed spritemap.</summary>
    KiHunterPose29,
    /// <summary><c>ki_hunter_pose_30</c>, an installed spritemap.</summary>
    KiHunterPose30,
    /// <summary><c>ki_hunter_pose_31</c>, an installed spritemap.</summary>
    KiHunterPose31,
    /// <summary><c>ki_hunter_pose_32</c>, an installed spritemap.</summary>
    KiHunterPose32,
    /// <summary><c>ki_hunter_pose_33</c>, an installed spritemap.</summary>
    KiHunterPose33,
    /// <summary><c>ki_hunter_pose_34</c>, an installed spritemap.</summary>
    KiHunterPose34,
    /// <summary><c>ki_hunter_pose_35</c>, an installed spritemap.</summary>
    KiHunterPose35,
    /// <summary><c>ki_hunter_pose_36</c>, an installed spritemap.</summary>
    KiHunterPose36,
    /// <summary><c>ki_hunter_pose_37</c>, an installed spritemap.</summary>
    KiHunterPose37,
    /// <summary><c>ki_hunter_pose_38</c>, an installed spritemap.</summary>
    KiHunterPose38,
    /// <summary><c>ki_hunter_pose_39</c>, an installed spritemap.</summary>
    KiHunterPose39,
    /// <summary><c>ki_hunter_pose_40</c>, an installed spritemap.</summary>
    KiHunterPose40,
    /// <summary><c>mother_brain_head_0</c>, an installed spritemap.</summary>
    MotherBrainHead0,
    /// <summary><c>mother_brain_head_1</c>, an installed spritemap.</summary>
    MotherBrainHead1,
    /// <summary><c>mother_brain_head_2</c>, an installed spritemap.</summary>
    MotherBrainHead2,
    /// <summary><c>mother_brain_head_3</c>, an installed spritemap.</summary>
    MotherBrainHead3,
    /// <summary><c>mother_brain_head_4</c>, an installed spritemap.</summary>
    MotherBrainHead4,
    /// <summary><c>mother_brain_neck_joint</c>, an installed spritemap.</summary>
    MotherBrainNeckJoint,
    /// <summary><c>mother_brain_head_6</c>, an installed spritemap.</summary>
    MotherBrainHead6,
    /// <summary><c>mother_brain_head_7</c>, an installed spritemap.</summary>
    MotherBrainHead7,
    /// <summary><c>mother_brain_head_8</c>, an installed spritemap.</summary>
    MotherBrainHead8,
    /// <summary><c>mother_brain_head_9</c>, an installed spritemap.</summary>
    MotherBrainHead9,
    /// <summary><c>mother_brain_head_10</c>, an installed spritemap.</summary>
    MotherBrainHead10,
    /// <summary><c>mother_brain_head_11</c>, an installed spritemap.</summary>
    MotherBrainHead11,
    /// <summary><c>mother_brain_head_12</c>, an installed spritemap.</summary>
    MotherBrainHead12,
    /// <summary><c>mother_brain_tube_0</c>, an installed spritemap.</summary>
    MotherBrainTube0,
    /// <summary><c>mother_brain_tube_1</c>, an installed spritemap.</summary>
    MotherBrainTube1,
    /// <summary><c>mother_brain_tube_2</c>, an installed spritemap.</summary>
    MotherBrainTube2,
    /// <summary><c>mother_brain_tube_3</c>, an installed spritemap.</summary>
    MotherBrainTube3,
    /// <summary><c>mother_brain_tube_4</c>, an installed spritemap.</summary>
    MotherBrainTube4,
    /// <summary><c>dead_torizo_corpse_hook</c>, an installed spritemap.</summary>
    DeadTorizoCorpseHook,
    /// <summary><c>ridley_tail_segment_small</c>, an installed spritemap.</summary>
    RidleyTailSegmentSmall,
    /// <summary><c>ridley_tail_segment_medium</c>, an installed spritemap.</summary>
    RidleyTailSegmentMedium,
    /// <summary><c>ridley_tail_segment_large</c>, an installed spritemap.</summary>
    RidleyTailSegmentLarge,
    /// <summary><c>ridley_tail_tip_12</c>, an installed spritemap.</summary>
    RidleyTailTip12,
    /// <summary><c>ridley_tail_tip_11</c>, an installed spritemap.</summary>
    RidleyTailTip11,
    /// <summary><c>ridley_tail_tip_10</c>, an installed spritemap.</summary>
    RidleyTailTip10,
    /// <summary><c>ridley_tail_tip_9</c>, an installed spritemap.</summary>
    RidleyTailTip9,
    /// <summary><c>ridley_tail_tip_8</c>, an installed spritemap.</summary>
    RidleyTailTip8,
    /// <summary><c>ridley_tail_tip_7</c>, an installed spritemap.</summary>
    RidleyTailTip7,
    /// <summary><c>ridley_tail_tip_6</c>, an installed spritemap.</summary>
    RidleyTailTip6,
    /// <summary><c>ridley_tail_tip_5</c>, an installed spritemap.</summary>
    RidleyTailTip5,
    /// <summary><c>ridley_tail_tip_4</c>, an installed spritemap.</summary>
    RidleyTailTip4,
    /// <summary><c>ridley_tail_tip_3</c>, an installed spritemap.</summary>
    RidleyTailTip3,
    /// <summary><c>ridley_tail_tip_2</c>, an installed spritemap.</summary>
    RidleyTailTip2,
    /// <summary><c>ridley_tail_tip_1</c>, an installed spritemap.</summary>
    RidleyTailTip1,
    /// <summary><c>ridley_tail_tip_0</c>, an installed spritemap.</summary>
    RidleyTailTip0,
    /// <summary><c>ridley_tail_tip_15</c>, an installed spritemap.</summary>
    RidleyTailTip15,
    /// <summary><c>ridley_tail_tip_14</c>, an installed spritemap.</summary>
    RidleyTailTip14,
    /// <summary><c>ridley_tail_tip_13</c>, an installed spritemap.</summary>
    RidleyTailTip13,
    /// <summary><c>ridley_wing_left_0</c>, an installed spritemap.</summary>
    RidleyWingLeft0,
    /// <summary><c>ridley_wing_left_1</c>, an installed spritemap.</summary>
    RidleyWingLeft1,
    /// <summary><c>ridley_wing_left_2</c>, an installed spritemap.</summary>
    RidleyWingLeft2,
    /// <summary><c>ridley_wing_left_3</c>, an installed spritemap.</summary>
    RidleyWingLeft3,
    /// <summary><c>ridley_wing_left_4</c>, an installed spritemap.</summary>
    RidleyWingLeft4,
    /// <summary><c>ridley_wing_left_5</c>, an installed spritemap.</summary>
    RidleyWingLeft5,
    /// <summary><c>ridley_wing_right_0</c>, an installed spritemap.</summary>
    RidleyWingRight0,
    /// <summary><c>ridley_wing_right_1</c>, an installed spritemap.</summary>
    RidleyWingRight1,
    /// <summary><c>ridley_wing_right_2</c>, an installed spritemap.</summary>
    RidleyWingRight2,
    /// <summary><c>ridley_wing_right_3</c>, an installed spritemap.</summary>
    RidleyWingRight3,
    /// <summary><c>ridley_wing_right_4</c>, an installed spritemap.</summary>
    RidleyWingRight4,
    /// <summary><c>ridley_wing_right_5</c>, an installed spritemap.</summary>
    RidleyWingRight5,
    /// <summary><c>sciser_upside_right_0</c>, an installed spritemap.</summary>
    SciserUpsideRight0,
    /// <summary><c>sciser_upside_right_1</c>, an installed spritemap.</summary>
    SciserUpsideRight1,
    /// <summary><c>sciser_upside_right_2</c>, an installed spritemap.</summary>
    SciserUpsideRight2,
    /// <summary><c>sciser_upside_left_0</c>, an installed spritemap.</summary>
    SciserUpsideLeft0,
    /// <summary><c>sciser_upside_left_1</c>, an installed spritemap.</summary>
    SciserUpsideLeft1,
    /// <summary><c>sciser_upside_left_2</c>, an installed spritemap.</summary>
    SciserUpsideLeft2,
    /// <summary><c>sciser_upside_down_0</c>, an installed spritemap.</summary>
    SciserUpsideDown0,
    /// <summary><c>sciser_upside_down_1</c>, an installed spritemap.</summary>
    SciserUpsideDown1,
    /// <summary><c>sciser_upside_down_2</c>, an installed spritemap.</summary>
    SciserUpsideDown2,
    /// <summary><c>sciser_upside_up_0</c>, an installed spritemap.</summary>
    SciserUpsideUp0,
    /// <summary><c>sciser_upside_up_1</c>, an installed spritemap.</summary>
    SciserUpsideUp1,
    /// <summary><c>sciser_upside_up_2</c>, an installed spritemap.</summary>
    SciserUpsideUp2,
    /// <summary><c>fly_shared_0</c>, an installed spritemap.</summary>
    FlyShared0,
    /// <summary><c>fly_shared_1</c>, an installed spritemap.</summary>
    FlyShared1,
    /// <summary><c>fly_shared_2</c>, an installed spritemap.</summary>
    FlyShared2,
    /// <summary><c>fly_shared_3</c>, an installed spritemap.</summary>
    FlyShared3,
    /// <summary><c>kago_cycle_0</c>, an installed spritemap.</summary>
    KagoCycle0,
    /// <summary><c>kago_cycle_1</c>, an installed spritemap.</summary>
    KagoCycle1,
    /// <summary><c>kago_cycle_2</c>, an installed spritemap.</summary>
    KagoCycle2,
    /// <summary><c>face_block_neutral</c>, an installed spritemap.</summary>
    FaceBlockNeutral,
    /// <summary><c>face_block_samus_left_1</c>, an installed spritemap.</summary>
    FaceBlockSamusLeft1,
    /// <summary><c>face_block_samus_left_2</c>, an installed spritemap.</summary>
    FaceBlockSamusLeft2,
    /// <summary><c>face_block_samus_right_1</c>, an installed spritemap.</summary>
    FaceBlockSamusRight1,
    /// <summary><c>face_block_samus_right_2</c>, an installed spritemap.</summary>
    FaceBlockSamusRight2,
    /// <summary><c>morph_eye_active_0</c>, an installed spritemap.</summary>
    MorphEyeActive0,
    /// <summary><c>morph_eye_active_1</c>, an installed spritemap.</summary>
    MorphEyeActive1,
    /// <summary><c>morph_eye_active_2</c>, an installed spritemap.</summary>
    MorphEyeActive2,
    /// <summary><c>morph_eye_active_3</c>, an installed spritemap.</summary>
    MorphEyeActive3,
    /// <summary><c>morph_eye_active_4</c>, an installed spritemap.</summary>
    MorphEyeActive4,
    /// <summary><c>morph_eye_active_5</c>, an installed spritemap.</summary>
    MorphEyeActive5,
    /// <summary><c>morph_eye_active_6</c>, an installed spritemap.</summary>
    MorphEyeActive6,
    /// <summary><c>morph_eye_active_7</c>, an installed spritemap.</summary>
    MorphEyeActive7,
    /// <summary><c>morph_eye_active_8</c>, an installed spritemap.</summary>
    MorphEyeActive8,
    /// <summary><c>morph_eye_active_9</c>, an installed spritemap.</summary>
    MorphEyeActive9,
    /// <summary><c>morph_eye_active_10</c>, an installed spritemap.</summary>
    MorphEyeActive10,
    /// <summary><c>morph_eye_active_11</c>, an installed spritemap.</summary>
    MorphEyeActive11,
    /// <summary><c>morph_eye_active_12</c>, an installed spritemap.</summary>
    MorphEyeActive12,
    /// <summary><c>morph_eye_active_13</c>, an installed spritemap.</summary>
    MorphEyeActive13,
    /// <summary><c>morph_eye_right_transition</c>, an installed spritemap.</summary>
    MorphEyeRightTransition,
    /// <summary><c>morph_eye_right_closed</c>, an installed spritemap.</summary>
    MorphEyeRightClosed,
    /// <summary><c>morph_eye_left_transition</c>, an installed spritemap.</summary>
    MorphEyeLeftTransition,
    /// <summary><c>morph_eye_left_closed</c>, an installed spritemap.</summary>
    MorphEyeLeftClosed,
    /// <summary><c>morph_eye_mount_right</c>, an installed spritemap.</summary>
    MorphEyeMountRight,
    /// <summary><c>morph_eye_mount_down</c>, an installed spritemap.</summary>
    MorphEyeMountDown,
    /// <summary><c>morph_eye_mount_left</c>, an installed spritemap.</summary>
    MorphEyeMountLeft,
    /// <summary><c>morph_eye_mount_up</c>, an installed spritemap.</summary>
    MorphEyeMountUp,
    /// <summary><c>shutter_growing_10px</c>, an installed spritemap.</summary>
    ShutterGrowing10px,
    /// <summary><c>shutter_growing_20px</c>, an installed spritemap.</summary>
    ShutterGrowing20px,
    /// <summary><c>shutter_growing_30px</c>, an installed spritemap.</summary>
    ShutterGrowing30px,
    /// <summary><c>shutter_vertical_40px</c>, an installed spritemap.</summary>
    ShutterVertical40px,
    /// <summary><c>shutter_horizontal</c>, an installed spritemap.</summary>
    ShutterHorizontal,
    /// <summary><c>metroid_body_0</c>, an installed spritemap.</summary>
    MetroidBody0,
    /// <summary><c>metroid_body_1</c>, an installed spritemap.</summary>
    MetroidBody1,
    /// <summary><c>metroid_body_2</c>, an installed spritemap.</summary>
    MetroidBody2,
    /// <summary><c>metroid_body_3</c>, an installed spritemap.</summary>
    MetroidBody3,
    /// <summary><c>shaktool_final_saw_0</c>, an installed spritemap.</summary>
    ShaktoolFinalSaw0,
    /// <summary><c>shaktool_final_saw_1</c>, an installed spritemap.</summary>
    ShaktoolFinalSaw1,
    /// <summary><c>shaktool_final_saw_2</c>, an installed spritemap.</summary>
    ShaktoolFinalSaw2,
    /// <summary><c>shaktool_arm</c>, an installed spritemap.</summary>
    ShaktoolArm,
    /// <summary><c>shaktool_head_left</c>, an installed spritemap.</summary>
    ShaktoolHeadLeft,
    /// <summary><c>shaktool_head_up_left</c>, an installed spritemap.</summary>
    ShaktoolHeadUpLeft,
    /// <summary><c>shaktool_head_up</c>, an installed spritemap.</summary>
    ShaktoolHeadUp,
    /// <summary><c>shaktool_head_up_right</c>, an installed spritemap.</summary>
    ShaktoolHeadUpRight,
    /// <summary><c>shaktool_head_right</c>, an installed spritemap.</summary>
    ShaktoolHeadRight,
    /// <summary><c>shaktool_head_down_right</c>, an installed spritemap.</summary>
    ShaktoolHeadDownRight,
    /// <summary><c>shaktool_head_down</c>, an installed spritemap.</summary>
    ShaktoolHeadDown,
    /// <summary><c>shaktool_head_down_left</c>, an installed spritemap.</summary>
    ShaktoolHeadDownLeft,
    /// <summary><c>shaktool_primary_saw_0</c>, an installed spritemap.</summary>
    ShaktoolPrimarySaw0,
    /// <summary><c>shaktool_primary_saw_1</c>, an installed spritemap.</summary>
    ShaktoolPrimarySaw1,
    /// <summary><c>shaktool_primary_saw_2</c>, an installed spritemap.</summary>
    ShaktoolPrimarySaw2,
    /// <summary><c>tripper_moving_left_0</c>, an installed spritemap.</summary>
    TripperMovingLeft0,
    /// <summary><c>tripper_moving_left_1</c>, an installed spritemap.</summary>
    TripperMovingLeft1,
    /// <summary><c>tripper_moving_left_2</c>, an installed spritemap.</summary>
    TripperMovingLeft2,
    /// <summary><c>tripper_moving_right_0</c>, an installed spritemap.</summary>
    TripperMovingRight0,
    /// <summary><c>tripper_moving_right_1</c>, an installed spritemap.</summary>
    TripperMovingRight1,
    /// <summary><c>tripper_moving_right_2</c>, an installed spritemap.</summary>
    TripperMovingRight2,
    /// <summary><c>tripper_still_moving_left_0</c>, an installed spritemap.</summary>
    TripperStillMovingLeft0,
    /// <summary><c>tripper_still_moving_left_1</c>, an installed spritemap.</summary>
    TripperStillMovingLeft1,
    /// <summary><c>tripper_still_moving_left_2</c>, an installed spritemap.</summary>
    TripperStillMovingLeft2,
    /// <summary><c>tripper_still_moving_right_0</c>, an installed spritemap.</summary>
    TripperStillMovingRight0,
    /// <summary><c>tripper_still_moving_right_1</c>, an installed spritemap.</summary>
    TripperStillMovingRight1,
    /// <summary><c>tripper_still_moving_right_2</c>, an installed spritemap.</summary>
    TripperStillMovingRight2,
    /// <summary><c>tripper_frozen_moving_left</c>, an installed spritemap.</summary>
    TripperFrozenMovingLeft,
    /// <summary><c>tripper_frozen_moving_right</c>, an installed spritemap.</summary>
    TripperFrozenMovingRight,
    /// <summary><c>tripper_kamer_platform_0</c>, an installed spritemap.</summary>
    TripperKamerPlatform0,
    /// <summary><c>tripper_kamer_platform_1</c>, an installed spritemap.</summary>
    TripperKamerPlatform1,
    /// <summary><c>tripper_kamer_platform_2</c>, an installed spritemap.</summary>
    TripperKamerPlatform2,
    /// <summary><c>tripper_kamer_platform_3</c>, an installed spritemap.</summary>
    TripperKamerPlatform3,
    /// <summary><c>dragon_body_idle_left</c>, an installed spritemap.</summary>
    DragonBodyIdleLeft,
    /// <summary><c>dragon_wing_left_0</c>, an installed spritemap.</summary>
    DragonWingLeft0,
    /// <summary><c>dragon_wing_left_1</c>, an installed spritemap.</summary>
    DragonWingLeft1,
    /// <summary><c>dragon_body_idle_right</c>, an installed spritemap.</summary>
    DragonBodyIdleRight,
    /// <summary><c>dragon_wing_right_0</c>, an installed spritemap.</summary>
    DragonWingRight0,
    /// <summary><c>dragon_wing_right_1</c>, an installed spritemap.</summary>
    DragonWingRight1,
    /// <summary><c>dragon_body_attack_left_0</c>, an installed spritemap.</summary>
    DragonBodyAttackLeft0,
    /// <summary><c>dragon_body_attack_left_1</c>, an installed spritemap.</summary>
    DragonBodyAttackLeft1,
    /// <summary><c>dragon_body_attack_left_2</c>, an installed spritemap.</summary>
    DragonBodyAttackLeft2,
    /// <summary><c>dragon_body_attack_right_0</c>, an installed spritemap.</summary>
    DragonBodyAttackRight0,
    /// <summary><c>dragon_body_attack_right_1</c>, an installed spritemap.</summary>
    DragonBodyAttackRight1,
    /// <summary><c>dragon_body_attack_right_2</c>, an installed spritemap.</summary>
    DragonBodyAttackRight2,
    /// <summary><c>multiviola_spin_0</c>, an installed spritemap.</summary>
    MultiviolaSpin0,
    /// <summary><c>multiviola_spin_1</c>, an installed spritemap.</summary>
    MultiviolaSpin1,
    /// <summary><c>multiviola_spin_2</c>, an installed spritemap.</summary>
    MultiviolaSpin2,
    /// <summary><c>multiviola_spin_3</c>, an installed spritemap.</summary>
    MultiviolaSpin3,
    /// <summary><c>multiviola_spin_4</c>, an installed spritemap.</summary>
    MultiviolaSpin4,
    /// <summary><c>multiviola_spin_5</c>, an installed spritemap.</summary>
    MultiviolaSpin5,
    /// <summary><c>multiviola_spin_6</c>, an installed spritemap.</summary>
    MultiviolaSpin6,
    /// <summary><c>multiviola_spin_7</c>, an installed spritemap.</summary>
    MultiviolaSpin7,
    /// <summary><c>norfair_lava_jumper_pose_1</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose1,
    /// <summary><c>norfair_lava_jumper_pose_2</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose2,
    /// <summary><c>norfair_lava_jumper_pose_3</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose3,
    /// <summary><c>norfair_lava_jumper_pose_4</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose4,
    /// <summary><c>norfair_lava_jumper_pose_5</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose5,
    /// <summary><c>norfair_lava_jumper_pose_6</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose6,
    /// <summary><c>norfair_lava_jumper_pose_7</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose7,
    /// <summary><c>norfair_lava_jumper_pose_8</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose8,
    /// <summary><c>norfair_lava_jumper_pose_0</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose0,
    /// <summary><c>norfair_lava_jumper_pose_10</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose10,
    /// <summary><c>norfair_lava_jumper_pose_9</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose9,
    /// <summary><c>chozo_statue_pose_17</c>, an installed spritemap.</summary>
    ChozoStatuePose17,
    /// <summary><c>chozo_statue_pose_18</c>, an installed spritemap.</summary>
    ChozoStatuePose18,
    /// <summary><c>chozo_statue_pose_19</c>, an installed spritemap.</summary>
    ChozoStatuePose19,
    /// <summary><c>chozo_statue_pose_20</c>, an installed spritemap.</summary>
    ChozoStatuePose20,
    /// <summary><c>chozo_statue_pose_21</c>, an installed spritemap.</summary>
    ChozoStatuePose21,
    /// <summary><c>chozo_statue_pose_22</c>, an installed spritemap.</summary>
    ChozoStatuePose22,
    /// <summary><c>chozo_statue_pose_23</c>, an installed spritemap.</summary>
    ChozoStatuePose23,
    /// <summary><c>chozo_statue_pose_24</c>, an installed spritemap.</summary>
    ChozoStatuePose24,
    /// <summary><c>chozo_statue_pose_25</c>, an installed spritemap.</summary>
    ChozoStatuePose25,
    /// <summary><c>chozo_statue_pose_0</c>, an installed spritemap.</summary>
    ChozoStatuePose0,
    /// <summary><c>chozo_statue_pose_1</c>, an installed spritemap.</summary>
    ChozoStatuePose1,
    /// <summary><c>chozo_statue_pose_2</c>, an installed spritemap.</summary>
    ChozoStatuePose2,
    /// <summary><c>chozo_statue_pose_3</c>, an installed spritemap.</summary>
    ChozoStatuePose3,
    /// <summary><c>chozo_statue_pose_12</c>, an installed spritemap.</summary>
    ChozoStatuePose12,
    /// <summary><c>chozo_statue_pose_13</c>, an installed spritemap.</summary>
    ChozoStatuePose13,
    /// <summary><c>chozo_statue_pose_14</c>, an installed spritemap.</summary>
    ChozoStatuePose14,
    /// <summary><c>chozo_statue_pose_15</c>, an installed spritemap.</summary>
    ChozoStatuePose15,
    /// <summary><c>chozo_statue_pose_16</c>, an installed spritemap.</summary>
    ChozoStatuePose16,
    /// <summary><c>chozo_statue_pose_11</c>, an installed spritemap.</summary>
    ChozoStatuePose11,
    /// <summary><c>chozo_statue_pose_4</c>, an installed spritemap.</summary>
    ChozoStatuePose4,
    /// <summary><c>chozo_statue_pose_5</c>, an installed spritemap.</summary>
    ChozoStatuePose5,
    /// <summary><c>chozo_statue_pose_6</c>, an installed spritemap.</summary>
    ChozoStatuePose6,
    /// <summary><c>chozo_statue_pose_7</c>, an installed spritemap.</summary>
    ChozoStatuePose7,
    /// <summary><c>chozo_statue_pose_8</c>, an installed spritemap.</summary>
    ChozoStatuePose8,
    /// <summary><c>chozo_statue_pose_9</c>, an installed spritemap.</summary>
    ChozoStatuePose9,
    /// <summary><c>chozo_statue_pose_10</c>, an installed spritemap.</summary>
    ChozoStatuePose10,
    /// <summary><c>viola_spin_0</c>, an installed spritemap.</summary>
    ViolaSpin0,
    /// <summary><c>viola_spin_1</c>, an installed spritemap.</summary>
    ViolaSpin1,
    /// <summary><c>viola_spin_2</c>, an installed spritemap.</summary>
    ViolaSpin2,
    /// <summary><c>viola_spin_3</c>, an installed spritemap.</summary>
    ViolaSpin3,
    /// <summary><c>viola_spin_4</c>, an installed spritemap.</summary>
    ViolaSpin4,
    /// <summary><c>viola_spin_5</c>, an installed spritemap.</summary>
    ViolaSpin5,
    /// <summary><c>viola_spin_6</c>, an installed spritemap.</summary>
    ViolaSpin6,
    /// <summary><c>viola_spin_7</c>, an installed spritemap.</summary>
    ViolaSpin7,
    /// <summary><c>rinka_spin_0</c>, an installed spritemap.</summary>
    RinkaSpin0,
    /// <summary><c>rinka_spin_1</c>, an installed spritemap.</summary>
    RinkaSpin1,
    /// <summary><c>rinka_spin_2</c>, an installed spritemap.</summary>
    RinkaSpin2,
    /// <summary><c>rinka_spin_3</c>, an installed spritemap.</summary>
    RinkaSpin3,
    /// <summary><c>rinka_spin_4</c>, an installed spritemap.</summary>
    RinkaSpin4,
    /// <summary><c>dead_torizo_stationary</c>, an installed spritemap.</summary>
    DeadTorizoStationary,
    /// <summary><c>dead_zoomer_corpse_0</c>, an installed spritemap.</summary>
    DeadZoomerCorpse0,
    /// <summary><c>dead_zoomer_corpse_2</c>, an installed spritemap.</summary>
    DeadZoomerCorpse2,
    /// <summary><c>dead_zoomer_corpse_4</c>, an installed spritemap.</summary>
    DeadZoomerCorpse4,
    /// <summary><c>dead_ripper_corpse_0</c>, an installed spritemap.</summary>
    DeadRipperCorpse0,
    /// <summary><c>dead_ripper_corpse_2</c>, an installed spritemap.</summary>
    DeadRipperCorpse2,
    /// <summary><c>dead_skree_corpse_0</c>, an installed spritemap.</summary>
    DeadSkreeCorpse0,
    /// <summary><c>dead_skree_corpse_2</c>, an installed spritemap.</summary>
    DeadSkreeCorpse2,
    /// <summary><c>dead_skree_corpse_4</c>, an installed spritemap.</summary>
    DeadSkreeCorpse4,
    /// <summary><c>dead_sidehopper_hop_body</c>, an installed spritemap.</summary>
    DeadSidehopperHopBody,
    /// <summary><c>dead_sidehopper_hop_alt</c>, an installed spritemap.</summary>
    DeadSidehopperHopAlt,
    /// <summary><c>dead_sidehopper_idle</c>, an installed spritemap.</summary>
    DeadSidehopperIdle,
    /// <summary><c>dead_sidehopper_corpse</c>, an installed spritemap.</summary>
    DeadSidehopperCorpse,
    /// <summary><c>dead_sidehopper_initially_dead</c>, an installed spritemap.</summary>
    DeadSidehopperInitiallyDead,
    /// <summary><c>mochtroid_flight_0</c>, an installed spritemap.</summary>
    MochtroidFlight0,
    /// <summary><c>mochtroid_flight_1</c>, an installed spritemap.</summary>
    MochtroidFlight1,
    /// <summary><c>mochtroid_flight_2</c>, an installed spritemap.</summary>
    MochtroidFlight2,
    /// <summary><c>mochtroid_attached_0</c>, an installed spritemap.</summary>
    MochtroidAttached0,
    /// <summary><c>mochtroid_attached_1</c>, an installed spritemap.</summary>
    MochtroidAttached1,
    /// <summary><c>mochtroid_attached_2</c>, an installed spritemap.</summary>
    MochtroidAttached2,
    /// <summary><c>evir_body_left_0</c>, an installed spritemap.</summary>
    EvirBodyLeft0,
    /// <summary><c>evir_body_left_1</c>, an installed spritemap.</summary>
    EvirBodyLeft1,
    /// <summary><c>evir_body_left_2</c>, an installed spritemap.</summary>
    EvirBodyLeft2,
    /// <summary><c>evir_body_left_3</c>, an installed spritemap.</summary>
    EvirBodyLeft3,
    /// <summary><c>evir_body_left_4</c>, an installed spritemap.</summary>
    EvirBodyLeft4,
    /// <summary><c>evir_body_left_5</c>, an installed spritemap.</summary>
    EvirBodyLeft5,
    /// <summary><c>evir_arms_left_0</c>, an installed spritemap.</summary>
    EvirArmsLeft0,
    /// <summary><c>evir_arms_left_1</c>, an installed spritemap.</summary>
    EvirArmsLeft1,
    /// <summary><c>evir_arms_left_2</c>, an installed spritemap.</summary>
    EvirArmsLeft2,
    /// <summary><c>evir_arms_left_3</c>, an installed spritemap.</summary>
    EvirArmsLeft3,
    /// <summary><c>evir_arms_left_4</c>, an installed spritemap.</summary>
    EvirArmsLeft4,
    /// <summary><c>evir_body_right_0</c>, an installed spritemap.</summary>
    EvirBodyRight0,
    /// <summary><c>evir_body_right_1</c>, an installed spritemap.</summary>
    EvirBodyRight1,
    /// <summary><c>evir_body_right_2</c>, an installed spritemap.</summary>
    EvirBodyRight2,
    /// <summary><c>evir_body_right_3</c>, an installed spritemap.</summary>
    EvirBodyRight3,
    /// <summary><c>evir_body_right_4</c>, an installed spritemap.</summary>
    EvirBodyRight4,
    /// <summary><c>evir_body_right_5</c>, an installed spritemap.</summary>
    EvirBodyRight5,
    /// <summary><c>evir_arms_right_0</c>, an installed spritemap.</summary>
    EvirArmsRight0,
    /// <summary><c>evir_arms_right_1</c>, an installed spritemap.</summary>
    EvirArmsRight1,
    /// <summary><c>evir_arms_right_2</c>, an installed spritemap.</summary>
    EvirArmsRight2,
    /// <summary><c>evir_arms_right_3</c>, an installed spritemap.</summary>
    EvirArmsRight3,
    /// <summary><c>evir_arms_right_4</c>, an installed spritemap.</summary>
    EvirArmsRight4,
    /// <summary><c>evir_projectile_flight</c>, an installed spritemap.</summary>
    EvirProjectileFlight,
    /// <summary><c>evir_projectile_regenerating</c>, an installed spritemap.</summary>
    EvirProjectileRegenerating,
    /// <summary><c>work_robot_left_00</c>, an installed spritemap.</summary>
    WorkRobotLeft00,
    /// <summary><c>work_robot_left_01</c>, an installed spritemap.</summary>
    WorkRobotLeft01,
    /// <summary><c>work_robot_left_02</c>, an installed spritemap.</summary>
    WorkRobotLeft02,
    /// <summary><c>work_robot_left_03</c>, an installed spritemap.</summary>
    WorkRobotLeft03,
    /// <summary><c>work_robot_left_04</c>, an installed spritemap.</summary>
    WorkRobotLeft04,
    /// <summary><c>work_robot_left_05</c>, an installed spritemap.</summary>
    WorkRobotLeft05,
    /// <summary><c>work_robot_left_06</c>, an installed spritemap.</summary>
    WorkRobotLeft06,
    /// <summary><c>work_robot_left_07</c>, an installed spritemap.</summary>
    WorkRobotLeft07,
    /// <summary><c>work_robot_left_08</c>, an installed spritemap.</summary>
    WorkRobotLeft08,
    /// <summary><c>work_robot_left_09</c>, an installed spritemap.</summary>
    WorkRobotLeft09,
    /// <summary><c>work_robot_left_10</c>, an installed spritemap.</summary>
    WorkRobotLeft10,
    /// <summary><c>work_robot_left_11</c>, an installed spritemap.</summary>
    WorkRobotLeft11,
    /// <summary><c>work_robot_right_00</c>, an installed spritemap.</summary>
    WorkRobotRight00,
    /// <summary><c>work_robot_right_01</c>, an installed spritemap.</summary>
    WorkRobotRight01,
    /// <summary><c>work_robot_right_02</c>, an installed spritemap.</summary>
    WorkRobotRight02,
    /// <summary><c>work_robot_right_03</c>, an installed spritemap.</summary>
    WorkRobotRight03,
    /// <summary><c>work_robot_right_04</c>, an installed spritemap.</summary>
    WorkRobotRight04,
    /// <summary><c>work_robot_right_05</c>, an installed spritemap.</summary>
    WorkRobotRight05,
    /// <summary><c>work_robot_right_06</c>, an installed spritemap.</summary>
    WorkRobotRight06,
    /// <summary><c>work_robot_right_07</c>, an installed spritemap.</summary>
    WorkRobotRight07,
    /// <summary><c>work_robot_right_08</c>, an installed spritemap.</summary>
    WorkRobotRight08,
    /// <summary><c>work_robot_right_09</c>, an installed spritemap.</summary>
    WorkRobotRight09,
    /// <summary><c>work_robot_right_10</c>, an installed spritemap.</summary>
    WorkRobotRight10,
    /// <summary><c>work_robot_right_11</c>, an installed spritemap.</summary>
    WorkRobotRight11,
    /// <summary><c>work_robot_unpowered_neutral</c>, an installed spritemap.</summary>
    WorkRobotUnpoweredNeutral,
    /// <summary><c>work_robot_unpowered_left</c>, an installed spritemap.</summary>
    WorkRobotUnpoweredLeft,
    /// <summary><c>work_robot_unpowered_right</c>, an installed spritemap.</summary>
    WorkRobotUnpoweredRight,
    /// <summary><c>yard_outside_turn_right_up_00</c>, an installed spritemap.</summary>
    YardOutsideTurnRightUp00,
    /// <summary><c>yard_outside_turn_right_up_01</c>, an installed spritemap.</summary>
    YardOutsideTurnRightUp01,
    /// <summary><c>yard_outside_turn_right_up_02</c>, an installed spritemap.</summary>
    YardOutsideTurnRightUp02,
    /// <summary><c>yard_crawl_top_left_00</c>, an installed spritemap.</summary>
    YardCrawlTopLeft00,
    /// <summary><c>yard_crawl_top_left_01</c>, an installed spritemap.</summary>
    YardCrawlTopLeft01,
    /// <summary><c>yard_crawl_top_left_02</c>, an installed spritemap.</summary>
    YardCrawlTopLeft02,
    /// <summary><c>yard_outside_turn_top_left_00</c>, an installed spritemap.</summary>
    YardOutsideTurnTopLeft00,
    /// <summary><c>yard_outside_turn_top_left_01</c>, an installed spritemap.</summary>
    YardOutsideTurnTopLeft01,
    /// <summary><c>yard_outside_turn_top_left_02</c>, an installed spritemap.</summary>
    YardOutsideTurnTopLeft02,
    /// <summary><c>yard_crawl_left_down_00</c>, an installed spritemap.</summary>
    YardCrawlLeftDown00,
    /// <summary><c>yard_crawl_left_down_01</c>, an installed spritemap.</summary>
    YardCrawlLeftDown01,
    /// <summary><c>yard_crawl_left_down_02</c>, an installed spritemap.</summary>
    YardCrawlLeftDown02,
    /// <summary><c>yard_outside_turn_left_down_00</c>, an installed spritemap.</summary>
    YardOutsideTurnLeftDown00,
    /// <summary><c>yard_outside_turn_left_down_01</c>, an installed spritemap.</summary>
    YardOutsideTurnLeftDown01,
    /// <summary><c>yard_outside_turn_left_down_02</c>, an installed spritemap.</summary>
    YardOutsideTurnLeftDown02,
    /// <summary><c>yard_crawl_bottom_right_00</c>, an installed spritemap.</summary>
    YardCrawlBottomRight00,
    /// <summary><c>yard_crawl_bottom_right_01</c>, an installed spritemap.</summary>
    YardCrawlBottomRight01,
    /// <summary><c>yard_crawl_bottom_right_02</c>, an installed spritemap.</summary>
    YardCrawlBottomRight02,
    /// <summary><c>yard_outside_turn_bottom_right_00</c>, an installed spritemap.</summary>
    YardOutsideTurnBottomRight00,
    /// <summary><c>yard_outside_turn_bottom_right_01</c>, an installed spritemap.</summary>
    YardOutsideTurnBottomRight01,
    /// <summary><c>yard_outside_turn_bottom_right_02</c>, an installed spritemap.</summary>
    YardOutsideTurnBottomRight02,
    /// <summary><c>yard_crawl_right_up_00</c>, an installed spritemap.</summary>
    YardCrawlRightUp00,
    /// <summary><c>yard_crawl_right_up_01</c>, an installed spritemap.</summary>
    YardCrawlRightUp01,
    /// <summary><c>yard_crawl_right_up_02</c>, an installed spritemap.</summary>
    YardCrawlRightUp02,
    /// <summary><c>yard_outside_turn_left_up_00</c>, an installed spritemap.</summary>
    YardOutsideTurnLeftUp00,
    /// <summary><c>yard_outside_turn_left_up_01</c>, an installed spritemap.</summary>
    YardOutsideTurnLeftUp01,
    /// <summary><c>yard_outside_turn_left_up_02</c>, an installed spritemap.</summary>
    YardOutsideTurnLeftUp02,
    /// <summary><c>yard_crawl_top_right_00</c>, an installed spritemap.</summary>
    YardCrawlTopRight00,
    /// <summary><c>yard_crawl_top_right_01</c>, an installed spritemap.</summary>
    YardCrawlTopRight01,
    /// <summary><c>yard_crawl_top_right_02</c>, an installed spritemap.</summary>
    YardCrawlTopRight02,
    /// <summary><c>yard_outside_turn_top_right_00</c>, an installed spritemap.</summary>
    YardOutsideTurnTopRight00,
    /// <summary><c>yard_outside_turn_top_right_01</c>, an installed spritemap.</summary>
    YardOutsideTurnTopRight01,
    /// <summary><c>yard_outside_turn_top_right_02</c>, an installed spritemap.</summary>
    YardOutsideTurnTopRight02,
    /// <summary><c>yard_crawl_right_down_00</c>, an installed spritemap.</summary>
    YardCrawlRightDown00,
    /// <summary><c>yard_crawl_right_down_01</c>, an installed spritemap.</summary>
    YardCrawlRightDown01,
    /// <summary><c>yard_crawl_right_down_02</c>, an installed spritemap.</summary>
    YardCrawlRightDown02,
    /// <summary><c>yard_outside_turn_right_down_00</c>, an installed spritemap.</summary>
    YardOutsideTurnRightDown00,
    /// <summary><c>yard_outside_turn_right_down_01</c>, an installed spritemap.</summary>
    YardOutsideTurnRightDown01,
    /// <summary><c>yard_outside_turn_right_down_02</c>, an installed spritemap.</summary>
    YardOutsideTurnRightDown02,
    /// <summary><c>yard_crawl_bottom_left_00</c>, an installed spritemap.</summary>
    YardCrawlBottomLeft00,
    /// <summary><c>yard_crawl_bottom_left_01</c>, an installed spritemap.</summary>
    YardCrawlBottomLeft01,
    /// <summary><c>yard_crawl_bottom_left_02</c>, an installed spritemap.</summary>
    YardCrawlBottomLeft02,
    /// <summary><c>yard_outside_turn_bottom_left_00</c>, an installed spritemap.</summary>
    YardOutsideTurnBottomLeft00,
    /// <summary><c>yard_outside_turn_bottom_left_01</c>, an installed spritemap.</summary>
    YardOutsideTurnBottomLeft01,
    /// <summary><c>yard_outside_turn_bottom_left_02</c>, an installed spritemap.</summary>
    YardOutsideTurnBottomLeft02,
    /// <summary><c>yard_crawl_left_up_00</c>, an installed spritemap.</summary>
    YardCrawlLeftUp00,
    /// <summary><c>yard_crawl_left_up_01</c>, an installed spritemap.</summary>
    YardCrawlLeftUp01,
    /// <summary><c>yard_crawl_left_up_02</c>, an installed spritemap.</summary>
    YardCrawlLeftUp02,
    /// <summary><c>yard_inside_turn_top_left_00</c>, an installed spritemap.</summary>
    YardInsideTurnTopLeft00,
    /// <summary><c>yard_inside_turn_top_left_01</c>, an installed spritemap.</summary>
    YardInsideTurnTopLeft01,
    /// <summary><c>yard_inside_turn_top_left_02</c>, an installed spritemap.</summary>
    YardInsideTurnTopLeft02,
    /// <summary><c>yard_inside_turn_right_up_00</c>, an installed spritemap.</summary>
    YardInsideTurnRightUp00,
    /// <summary><c>yard_inside_turn_right_up_01</c>, an installed spritemap.</summary>
    YardInsideTurnRightUp01,
    /// <summary><c>yard_inside_turn_right_up_02</c>, an installed spritemap.</summary>
    YardInsideTurnRightUp02,
    /// <summary><c>yard_inside_turn_bottom_right_00</c>, an installed spritemap.</summary>
    YardInsideTurnBottomRight00,
    /// <summary><c>yard_inside_turn_bottom_right_01</c>, an installed spritemap.</summary>
    YardInsideTurnBottomRight01,
    /// <summary><c>yard_inside_turn_bottom_right_02</c>, an installed spritemap.</summary>
    YardInsideTurnBottomRight02,
    /// <summary><c>yard_inside_turn_left_down_00</c>, an installed spritemap.</summary>
    YardInsideTurnLeftDown00,
    /// <summary><c>yard_inside_turn_left_down_01</c>, an installed spritemap.</summary>
    YardInsideTurnLeftDown01,
    /// <summary><c>yard_inside_turn_left_down_02</c>, an installed spritemap.</summary>
    YardInsideTurnLeftDown02,
    /// <summary><c>yard_inside_turn_top_right_00</c>, an installed spritemap.</summary>
    YardInsideTurnTopRight00,
    /// <summary><c>yard_inside_turn_top_right_01</c>, an installed spritemap.</summary>
    YardInsideTurnTopRight01,
    /// <summary><c>yard_inside_turn_top_right_02</c>, an installed spritemap.</summary>
    YardInsideTurnTopRight02,
    /// <summary><c>yard_inside_turn_left_up_00</c>, an installed spritemap.</summary>
    YardInsideTurnLeftUp00,
    /// <summary><c>yard_inside_turn_left_up_01</c>, an installed spritemap.</summary>
    YardInsideTurnLeftUp01,
    /// <summary><c>yard_inside_turn_left_up_02</c>, an installed spritemap.</summary>
    YardInsideTurnLeftUp02,
    /// <summary><c>yard_inside_turn_bottom_left_00</c>, an installed spritemap.</summary>
    YardInsideTurnBottomLeft00,
    /// <summary><c>yard_inside_turn_bottom_left_01</c>, an installed spritemap.</summary>
    YardInsideTurnBottomLeft01,
    /// <summary><c>yard_inside_turn_bottom_left_02</c>, an installed spritemap.</summary>
    YardInsideTurnBottomLeft02,
    /// <summary><c>yard_inside_turn_right_down_00</c>, an installed spritemap.</summary>
    YardInsideTurnRightDown00,
    /// <summary><c>yard_inside_turn_right_down_01</c>, an installed spritemap.</summary>
    YardInsideTurnRightDown01,
    /// <summary><c>yard_inside_turn_right_down_02</c>, an installed spritemap.</summary>
    YardInsideTurnRightDown02,
    /// <summary><c>yard_hiding_top_left_00</c>, an installed spritemap.</summary>
    YardHidingTopLeft00,
    /// <summary><c>yard_hiding_top_left_01</c>, an installed spritemap.</summary>
    YardHidingTopLeft01,
    /// <summary><c>yard_hidden_top_left_00</c>, an installed spritemap.</summary>
    YardHiddenTopLeft00,
    /// <summary><c>yard_hidden_top_left_01</c>, an installed spritemap.</summary>
    YardHiddenTopLeft01,
    /// <summary><c>yard_hiding_bottom_left_00</c>, an installed spritemap.</summary>
    YardHidingBottomLeft00,
    /// <summary><c>yard_hiding_bottom_left_01</c>, an installed spritemap.</summary>
    YardHidingBottomLeft01,
    /// <summary><c>yard_hiding_bottom_left_02</c>, an installed spritemap.</summary>
    YardHidingBottomLeft02,
    /// <summary><c>yard_hiding_bottom_left_03</c>, an installed spritemap.</summary>
    YardHidingBottomLeft03,
    /// <summary><c>yard_hiding_bottom_right_00</c>, an installed spritemap.</summary>
    YardHidingBottomRight00,
    /// <summary><c>yard_hiding_bottom_right_01</c>, an installed spritemap.</summary>
    YardHidingBottomRight01,
    /// <summary><c>yard_hiding_bottom_right_02</c>, an installed spritemap.</summary>
    YardHidingBottomRight02,
    /// <summary><c>yard_hiding_bottom_right_03</c>, an installed spritemap.</summary>
    YardHidingBottomRight03,
    /// <summary><c>yard_hiding_top_right_00</c>, an installed spritemap.</summary>
    YardHidingTopRight00,
    /// <summary><c>yard_hiding_top_right_01</c>, an installed spritemap.</summary>
    YardHidingTopRight01,
    /// <summary><c>yard_hidden_top_right_00</c>, an installed spritemap.</summary>
    YardHiddenTopRight00,
    /// <summary><c>yard_hidden_top_right_01</c>, an installed spritemap.</summary>
    YardHiddenTopRight01,
    /// <summary><c>yard_hiding_right_up_00</c>, an installed spritemap.</summary>
    YardHidingRightUp00,
    /// <summary><c>yard_hiding_right_up_01</c>, an installed spritemap.</summary>
    YardHidingRightUp01,
    /// <summary><c>yard_hiding_right_up_02</c>, an installed spritemap.</summary>
    YardHidingRightUp02,
    /// <summary><c>yard_hiding_right_up_03</c>, an installed spritemap.</summary>
    YardHidingRightUp03,
    /// <summary><c>yard_hiding_left_up_00</c>, an installed spritemap.</summary>
    YardHidingLeftUp00,
    /// <summary><c>yard_hiding_left_up_01</c>, an installed spritemap.</summary>
    YardHidingLeftUp01,
    /// <summary><c>yard_hiding_left_up_02</c>, an installed spritemap.</summary>
    YardHidingLeftUp02,
    /// <summary><c>yard_hiding_left_up_03</c>, an installed spritemap.</summary>
    YardHidingLeftUp03,
    /// <summary><c>yard_hiding_left_down_00</c>, an installed spritemap.</summary>
    YardHidingLeftDown00,
    /// <summary><c>yard_hiding_left_down_01</c>, an installed spritemap.</summary>
    YardHidingLeftDown01,
    /// <summary><c>yard_hiding_left_down_02</c>, an installed spritemap.</summary>
    YardHidingLeftDown02,
    /// <summary><c>yard_hiding_left_down_03</c>, an installed spritemap.</summary>
    YardHidingLeftDown03,
    /// <summary><c>yard_hiding_right_down_00</c>, an installed spritemap.</summary>
    YardHidingRightDown00,
    /// <summary><c>yard_hiding_right_down_01</c>, an installed spritemap.</summary>
    YardHidingRightDown01,
    /// <summary><c>yard_hiding_right_down_02</c>, an installed spritemap.</summary>
    YardHidingRightDown02,
    /// <summary><c>yard_hiding_right_down_03</c>, an installed spritemap.</summary>
    YardHidingRightDown03,
    /// <summary><c>botwoon_head_00</c>, an installed spritemap.</summary>
    BotwoonHead00,
    /// <summary><c>botwoon_head_01</c>, an installed spritemap.</summary>
    BotwoonHead01,
    /// <summary><c>botwoon_head_02</c>, an installed spritemap.</summary>
    BotwoonHead02,
    /// <summary><c>botwoon_head_03</c>, an installed spritemap.</summary>
    BotwoonHead03,
    /// <summary><c>botwoon_head_04</c>, an installed spritemap.</summary>
    BotwoonHead04,
    /// <summary><c>botwoon_head_05</c>, an installed spritemap.</summary>
    BotwoonHead05,
    /// <summary><c>botwoon_head_06</c>, an installed spritemap.</summary>
    BotwoonHead06,
    /// <summary><c>botwoon_head_07</c>, an installed spritemap.</summary>
    BotwoonHead07,
    /// <summary><c>botwoon_head_08</c>, an installed spritemap.</summary>
    BotwoonHead08,
    /// <summary><c>botwoon_head_09</c>, an installed spritemap.</summary>
    BotwoonHead09,
    /// <summary><c>botwoon_head_10</c>, an installed spritemap.</summary>
    BotwoonHead10,
    /// <summary><c>botwoon_head_11</c>, an installed spritemap.</summary>
    BotwoonHead11,
    /// <summary><c>botwoon_head_12</c>, an installed spritemap.</summary>
    BotwoonHead12,
    /// <summary><c>botwoon_head_13</c>, an installed spritemap.</summary>
    BotwoonHead13,
    /// <summary><c>botwoon_head_14</c>, an installed spritemap.</summary>
    BotwoonHead14,
    /// <summary><c>botwoon_head_15</c>, an installed spritemap.</summary>
    BotwoonHead15,
    /// <summary><c>gunship_entrance_00</c>, an installed spritemap.</summary>
    GunshipEntrance00,
    /// <summary><c>gunship_entrance_01</c>, an installed spritemap.</summary>
    GunshipEntrance01,
    /// <summary><c>gunship_entrance_02</c>, an installed spritemap.</summary>
    GunshipEntrance02,
    /// <summary><c>gunship_entrance_03</c>, an installed spritemap.</summary>
    GunshipEntrance03,
    /// <summary><c>gunship_entrance_04</c>, an installed spritemap.</summary>
    GunshipEntrance04,
    /// <summary><c>gunship_entrance_05</c>, an installed spritemap.</summary>
    GunshipEntrance05,
    /// <summary><c>gunship_entrance_06</c>, an installed spritemap.</summary>
    GunshipEntrance06,
    /// <summary><c>gunship_entrance_07</c>, an installed spritemap.</summary>
    GunshipEntrance07,
    /// <summary><c>gunship_entrance_08</c>, an installed spritemap.</summary>
    GunshipEntrance08,
    /// <summary><c>gunship_entrance_09</c>, an installed spritemap.</summary>
    GunshipEntrance09,
    /// <summary><c>gunship_entrance_10</c>, an installed spritemap.</summary>
    GunshipEntrance10,
    /// <summary><c>gunship_top_hull</c>, an installed spritemap.</summary>
    GunshipTopHull,
    /// <summary><c>gunship_bottom_hull</c>, an installed spritemap.</summary>
    GunshipBottomHull,
    /// <summary><c>tatori_00</c>, an installed spritemap.</summary>
    Tatori00,
    /// <summary><c>tatori_01</c>, an installed spritemap.</summary>
    Tatori01,
    /// <summary><c>tatori_02</c>, an installed spritemap.</summary>
    Tatori02,
    /// <summary><c>tatori_03</c>, an installed spritemap.</summary>
    Tatori03,
    /// <summary><c>tatori_04</c>, an installed spritemap.</summary>
    Tatori04,
    /// <summary><c>tatori_05</c>, an installed spritemap.</summary>
    Tatori05,
    /// <summary><c>tatori_06</c>, an installed spritemap.</summary>
    Tatori06,
    /// <summary><c>tatori_07</c>, an installed spritemap.</summary>
    Tatori07,
    /// <summary><c>tatori_08</c>, an installed spritemap.</summary>
    Tatori08,
    /// <summary><c>tatori_09</c>, an installed spritemap.</summary>
    Tatori09,
    /// <summary><c>tatori_10</c>, an installed spritemap.</summary>
    Tatori10,
    /// <summary><c>tatori_11</c>, an installed spritemap.</summary>
    Tatori11,
    /// <summary><c>tatori_12</c>, an installed spritemap.</summary>
    Tatori12,
    /// <summary><c>tatori_13</c>, an installed spritemap.</summary>
    Tatori13,
    /// <summary><c>tatori_14</c>, an installed spritemap.</summary>
    Tatori14,
    /// <summary><c>tatori_15</c>, an installed spritemap.</summary>
    Tatori15,
    /// <summary><c>tatori_16</c>, an installed spritemap.</summary>
    Tatori16,
    /// <summary><c>tatori_17</c>, an installed spritemap.</summary>
    Tatori17,
    /// <summary><c>tatori_18</c>, an installed spritemap.</summary>
    Tatori18,
    /// <summary><c>tatori_19</c>, an installed spritemap.</summary>
    Tatori19,
    /// <summary><c>tatori_20</c>, an installed spritemap.</summary>
    Tatori20,
    /// <summary><c>tatori_21</c>, an installed spritemap.</summary>
    Tatori21,
    /// <summary><c>tatori_22</c>, an installed spritemap.</summary>
    Tatori22,
    /// <summary><c>tatori_23</c>, an installed spritemap.</summary>
    Tatori23,
    /// <summary><c>tatori_24</c>, an installed spritemap.</summary>
    Tatori24,
    /// <summary><c>tatori_25</c>, an installed spritemap.</summary>
    Tatori25,
    /// <summary><c>tatori_26</c>, an installed spritemap.</summary>
    Tatori26,
    /// <summary><c>tatori_27</c>, an installed spritemap.</summary>
    Tatori27,
    /// <summary><c>tatori_28</c>, an installed spritemap.</summary>
    Tatori28,
    /// <summary><c>zero_crawler_00</c>, an installed spritemap.</summary>
    ZeroCrawler00,
    /// <summary><c>zero_crawler_01</c>, an installed spritemap.</summary>
    ZeroCrawler01,
    /// <summary><c>zero_crawler_02</c>, an installed spritemap.</summary>
    ZeroCrawler02,
    /// <summary><c>zero_crawler_03</c>, an installed spritemap.</summary>
    ZeroCrawler03,
    /// <summary><c>zero_crawler_04</c>, an installed spritemap.</summary>
    ZeroCrawler04,
    /// <summary><c>zero_crawler_05</c>, an installed spritemap.</summary>
    ZeroCrawler05,
    /// <summary><c>zero_crawler_06</c>, an installed spritemap.</summary>
    ZeroCrawler06,
    /// <summary><c>zero_crawler_07</c>, an installed spritemap.</summary>
    ZeroCrawler07,
    /// <summary><c>zero_crawler_08</c>, an installed spritemap.</summary>
    ZeroCrawler08,
    /// <summary><c>zero_crawler_09</c>, an installed spritemap.</summary>
    ZeroCrawler09,
    /// <summary><c>zero_crawler_10</c>, an installed spritemap.</summary>
    ZeroCrawler10,
    /// <summary><c>zero_crawler_11</c>, an installed spritemap.</summary>
    ZeroCrawler11,
    /// <summary><c>zero_crawler_12</c>, an installed spritemap.</summary>
    ZeroCrawler12,
    /// <summary><c>zero_crawler_13</c>, an installed spritemap.</summary>
    ZeroCrawler13,
    /// <summary><c>zero_crawler_14</c>, an installed spritemap.</summary>
    ZeroCrawler14,
    /// <summary><c>zero_crawler_15</c>, an installed spritemap.</summary>
    ZeroCrawler15,
    /// <summary><c>etecoon_00</c>, an installed spritemap.</summary>
    Etecoon00,
    /// <summary><c>etecoon_01</c>, an installed spritemap.</summary>
    Etecoon01,
    /// <summary><c>etecoon_02</c>, an installed spritemap.</summary>
    Etecoon02,
    /// <summary><c>etecoon_03</c>, an installed spritemap.</summary>
    Etecoon03,
    /// <summary><c>etecoon_04</c>, an installed spritemap.</summary>
    Etecoon04,
    /// <summary><c>etecoon_05</c>, an installed spritemap.</summary>
    Etecoon05,
    /// <summary><c>etecoon_06</c>, an installed spritemap.</summary>
    Etecoon06,
    /// <summary><c>etecoon_07</c>, an installed spritemap.</summary>
    Etecoon07,
    /// <summary><c>etecoon_08</c>, an installed spritemap.</summary>
    Etecoon08,
    /// <summary><c>etecoon_09</c>, an installed spritemap.</summary>
    Etecoon09,
    /// <summary><c>etecoon_10</c>, an installed spritemap.</summary>
    Etecoon10,
    /// <summary><c>etecoon_11</c>, an installed spritemap.</summary>
    Etecoon11,
    /// <summary><c>etecoon_12</c>, an installed spritemap.</summary>
    Etecoon12,
    /// <summary><c>etecoon_13</c>, an installed spritemap.</summary>
    Etecoon13,
    /// <summary><c>etecoon_14</c>, an installed spritemap.</summary>
    Etecoon14,
    /// <summary><c>etecoon_15</c>, an installed spritemap.</summary>
    Etecoon15,
    /// <summary><c>etecoon_16</c>, an installed spritemap.</summary>
    Etecoon16,
    /// <summary><c>etecoon_17</c>, an installed spritemap.</summary>
    Etecoon17,
    /// <summary><c>etecoon_18</c>, an installed spritemap.</summary>
    Etecoon18,
    /// <summary><c>etecoon_19</c>, an installed spritemap.</summary>
    Etecoon19,
    /// <summary><c>etecoon_20</c>, an installed spritemap.</summary>
    Etecoon20,
    /// <summary><c>etecoon_21</c>, an installed spritemap.</summary>
    Etecoon21,
    /// <summary><c>etecoon_22</c>, an installed spritemap.</summary>
    Etecoon22,
    /// <summary><c>etecoon_23</c>, an installed spritemap.</summary>
    Etecoon23,
    /// <summary><c>etecoon_24</c>, an installed spritemap.</summary>
    Etecoon24,
    /// <summary><c>etecoon_25</c>, an installed spritemap.</summary>
    Etecoon25,
    /// <summary><c>etecoon_26</c>, an installed spritemap.</summary>
    Etecoon26,
    /// <summary><c>etecoon_27</c>, an installed spritemap.</summary>
    Etecoon27,
    /// <summary><c>etecoon_28</c>, an installed spritemap.</summary>
    Etecoon28,
    /// <summary><c>etecoon_29</c>, an installed spritemap.</summary>
    Etecoon29,
    /// <summary><c>etecoon_30</c>, an installed spritemap.</summary>
    Etecoon30,
    /// <summary><c>dachora_00</c>, an installed spritemap.</summary>
    Dachora00,
    /// <summary><c>dachora_01</c>, an installed spritemap.</summary>
    Dachora01,
    /// <summary><c>dachora_02</c>, an installed spritemap.</summary>
    Dachora02,
    /// <summary><c>dachora_03</c>, an installed spritemap.</summary>
    Dachora03,
    /// <summary><c>dachora_04</c>, an installed spritemap.</summary>
    Dachora04,
    /// <summary><c>dachora_05</c>, an installed spritemap.</summary>
    Dachora05,
    /// <summary><c>dachora_06</c>, an installed spritemap.</summary>
    Dachora06,
    /// <summary><c>dachora_07</c>, an installed spritemap.</summary>
    Dachora07,
    /// <summary><c>dachora_08</c>, an installed spritemap.</summary>
    Dachora08,
    /// <summary><c>dachora_09</c>, an installed spritemap.</summary>
    Dachora09,
    /// <summary><c>dachora_10</c>, an installed spritemap.</summary>
    Dachora10,
    /// <summary><c>dachora_11</c>, an installed spritemap.</summary>
    Dachora11,
    /// <summary><c>dachora_12</c>, an installed spritemap.</summary>
    Dachora12,
    /// <summary><c>dachora_13</c>, an installed spritemap.</summary>
    Dachora13,
    /// <summary><c>dachora_14</c>, an installed spritemap.</summary>
    Dachora14,
    /// <summary><c>dachora_15</c>, an installed spritemap.</summary>
    Dachora15,
    /// <summary><c>dachora_16</c>, an installed spritemap.</summary>
    Dachora16,
    /// <summary><c>dachora_17</c>, an installed spritemap.</summary>
    Dachora17,
    /// <summary><c>dachora_18</c>, an installed spritemap.</summary>
    Dachora18,
    /// <summary><c>dachora_19</c>, an installed spritemap.</summary>
    Dachora19,
    /// <summary><c>dachora_20</c>, an installed spritemap.</summary>
    Dachora20,
    /// <summary><c>dachora_21</c>, an installed spritemap.</summary>
    Dachora21,
    /// <summary><c>dachora_22</c>, an installed spritemap.</summary>
    Dachora22,
    /// <summary><c>dachora_23</c>, an installed spritemap.</summary>
    Dachora23,
    /// <summary><c>dachora_24</c>, an installed spritemap.</summary>
    Dachora24,
    /// <summary><c>dachora_25</c>, an installed spritemap.</summary>
    Dachora25,
    /// <summary><c>dachora_26</c>, an installed spritemap.</summary>
    Dachora26,
    /// <summary><c>dachora_27</c>, an installed spritemap.</summary>
    Dachora27,
    /// <summary><c>dachora_28</c>, an installed spritemap.</summary>
    Dachora28,
    /// <summary><c>escape_etecoon_00</c>, an installed spritemap.</summary>
    EscapeEtecoon00,
    /// <summary><c>escape_etecoon_01</c>, an installed spritemap.</summary>
    EscapeEtecoon01,
    /// <summary><c>escape_etecoon_02</c>, an installed spritemap.</summary>
    EscapeEtecoon02,
    /// <summary><c>escape_etecoon_03</c>, an installed spritemap.</summary>
    EscapeEtecoon03,
    /// <summary><c>escape_etecoon_04</c>, an installed spritemap.</summary>
    EscapeEtecoon04,
    /// <summary><c>escape_etecoon_05</c>, an installed spritemap.</summary>
    EscapeEtecoon05,
    /// <summary><c>escape_etecoon_06</c>, an installed spritemap.</summary>
    EscapeEtecoon06,
    /// <summary><c>escape_etecoon_07</c>, an installed spritemap.</summary>
    EscapeEtecoon07,
    /// <summary><c>escape_etecoon_08</c>, an installed spritemap.</summary>
    EscapeEtecoon08,
    /// <summary><c>escape_etecoon_09</c>, an installed spritemap.</summary>
    EscapeEtecoon09,
    /// <summary><c>escape_dachora_00</c>, an installed spritemap.</summary>
    EscapeDachora00,
    /// <summary><c>escape_dachora_01</c>, an installed spritemap.</summary>
    EscapeDachora01,
    /// <summary><c>escape_dachora_02</c>, an installed spritemap.</summary>
    EscapeDachora02,
    /// <summary><c>escape_dachora_03</c>, an installed spritemap.</summary>
    EscapeDachora03,
    /// <summary><c>escape_dachora_04</c>, an installed spritemap.</summary>
    EscapeDachora04,
    /// <summary><c>escape_dachora_05</c>, an installed spritemap.</summary>
    EscapeDachora05,
    /// <summary><c>escape_dachora_06</c>, an installed spritemap.</summary>
    EscapeDachora06,
    /// <summary><c>escape_dachora_07</c>, an installed spritemap.</summary>
    EscapeDachora07,
    /// <summary><c>escape_dachora_08</c>, an installed spritemap.</summary>
    EscapeDachora08,
    /// <summary><c>escape_dachora_09</c>, an installed spritemap.</summary>
    EscapeDachora09,
    /// <summary><c>escape_dachora_10</c>, an installed spritemap.</summary>
    EscapeDachora10,
    /// <summary><c>escape_dachora_11</c>, an installed spritemap.</summary>
    EscapeDachora11,
    /// <summary><c>hibashi_00</c>, an installed spritemap.</summary>
    Hibashi00,
    /// <summary><c>hibashi_01</c>, an installed spritemap.</summary>
    Hibashi01,
    /// <summary><c>hibashi_02</c>, an installed spritemap.</summary>
    Hibashi02,
    /// <summary><c>hibashi_03</c>, an installed spritemap.</summary>
    Hibashi03,
    /// <summary><c>hibashi_04</c>, an installed spritemap.</summary>
    Hibashi04,
    /// <summary><c>hibashi_05</c>, an installed spritemap.</summary>
    Hibashi05,
    /// <summary><c>hibashi_06</c>, an installed spritemap.</summary>
    Hibashi06,
    /// <summary><c>hibashi_07</c>, an installed spritemap.</summary>
    Hibashi07,
    /// <summary><c>hibashi_08</c>, an installed spritemap.</summary>
    Hibashi08,
    /// <summary><c>hibashi_09</c>, an installed spritemap.</summary>
    Hibashi09,
    /// <summary><c>hibashi_10</c>, an installed spritemap.</summary>
    Hibashi10,
    /// <summary><c>hibashi_11</c>, an installed spritemap.</summary>
    Hibashi11,
    /// <summary><c>hibashi_12</c>, an installed spritemap.</summary>
    Hibashi12,
    /// <summary><c>hibashi_13</c>, an installed spritemap.</summary>
    Hibashi13,
    /// <summary><c>hibashi_14</c>, an installed spritemap.</summary>
    Hibashi14,
    /// <summary><c>hibashi_15</c>, an installed spritemap.</summary>
    Hibashi15,
    /// <summary><c>hibashi_16</c>, an installed spritemap.</summary>
    Hibashi16,
    /// <summary><c>hibashi_17</c>, an installed spritemap.</summary>
    Hibashi17,
    /// <summary><c>hibashi_18</c>, an installed spritemap.</summary>
    Hibashi18,
    /// <summary><c>hibashi_19</c>, an installed spritemap.</summary>
    Hibashi19,
    /// <summary><c>hibashi_20</c>, an installed spritemap.</summary>
    Hibashi20,
    /// <summary><c>hibashi_21</c>, an installed spritemap.</summary>
    Hibashi21,
    /// <summary><c>hibashi_22</c>, an installed spritemap.</summary>
    Hibashi22,
    /// <summary><c>zebetite_00</c>, an installed spritemap.</summary>
    Zebetite00,
    /// <summary><c>zebetite_01</c>, an installed spritemap.</summary>
    Zebetite01,
    /// <summary><c>zebetite_02</c>, an installed spritemap.</summary>
    Zebetite02,
    /// <summary><c>zebetite_03</c>, an installed spritemap.</summary>
    Zebetite03,
    /// <summary><c>zebetite_04</c>, an installed spritemap.</summary>
    Zebetite04,
    /// <summary><c>zebetite_05</c>, an installed spritemap.</summary>
    Zebetite05,
    /// <summary><c>zebetite_06</c>, an installed spritemap.</summary>
    Zebetite06,
    /// <summary><c>zebetite_07</c>, an installed spritemap.</summary>
    Zebetite07,
    /// <summary><c>zebetite_08</c>, an installed spritemap.</summary>
    Zebetite08,
    /// <summary><c>zebetite_09</c>, an installed spritemap.</summary>
    Zebetite09,
    /// <summary><c>wrecked_ship_ghost_00</c>, an installed spritemap.</summary>
    WreckedShipGhost00,
    /// <summary><c>wrecked_ship_ghost_01</c>, an installed spritemap.</summary>
    WreckedShipGhost01,
    /// <summary><c>wrecked_ship_ghost_02</c>, an installed spritemap.</summary>
    WreckedShipGhost02,
    /// <summary><c>powamp_00</c>, an installed spritemap.</summary>
    Powamp00,
    /// <summary><c>powamp_01</c>, an installed spritemap.</summary>
    Powamp01,
    /// <summary><c>powamp_02</c>, an installed spritemap.</summary>
    Powamp02,
    /// <summary><c>powamp_03</c>, an installed spritemap.</summary>
    Powamp03,
    /// <summary><c>powamp_04</c>, an installed spritemap.</summary>
    Powamp04,
    /// <summary><c>powamp_05</c>, an installed spritemap.</summary>
    Powamp05,
    /// <summary><c>spark_00</c>, an installed spritemap.</summary>
    Spark00,
    /// <summary><c>spark_01</c>, an installed spritemap.</summary>
    Spark01,
    /// <summary><c>spark_02</c>, an installed spritemap.</summary>
    Spark02,
    /// <summary><c>spark_03</c>, an installed spritemap.</summary>
    Spark03,
    /// <summary><c>spark_04</c>, an installed spritemap.</summary>
    Spark04,
    /// <summary><c>spark_05</c>, an installed spritemap.</summary>
    Spark05,
    /// <summary><c>spark_06</c>, an installed spritemap.</summary>
    Spark06,
    /// <summary><c>spark_07</c>, an installed spritemap.</summary>
    Spark07,
    /// <summary><c>shitroid_00</c>, an installed spritemap.</summary>
    Shitroid00,
    /// <summary><c>shitroid_01</c>, an installed spritemap.</summary>
    Shitroid01,
    /// <summary><c>shitroid_02</c>, an installed spritemap.</summary>
    Shitroid02,
    /// <summary><c>kraid_lint_00</c>, an installed spritemap.</summary>
    KraidLint00,
    /// <summary><c>kraid_lint_01</c>, an installed spritemap.</summary>
    KraidLint01,
    /// <summary><c>nuclear_waffle_00</c>, an installed spritemap.</summary>
    NuclearWaffle00,
    /// <summary><c>nuclear_waffle_01</c>, an installed spritemap.</summary>
    NuclearWaffle01,
    /// <summary><c>nuclear_waffle_02</c>, an installed spritemap.</summary>
    NuclearWaffle02,
    /// <summary><c>nuclear_waffle_03</c>, an installed spritemap.</summary>
    NuclearWaffle03,
    /// <summary><c>nuclear_waffle_04</c>, an installed spritemap.</summary>
    NuclearWaffle04,
    /// <summary><c>nuclear_waffle_05</c>, an installed spritemap.</summary>
    NuclearWaffle05,
    /// <summary><c>nuclear_waffle_06</c>, an installed spritemap.</summary>
    NuclearWaffle06,
    /// <summary><c>nuclear_waffle_07</c>, an installed spritemap.</summary>
    NuclearWaffle07,
    /// <summary><c>kzan_00</c>, an installed spritemap.</summary>
    Kzan00,
    /// <summary><c>polyp_00</c>, an installed spritemap.</summary>
    Polyp00,
    /// <summary><c>ridley_breakup_legs_left</c>, an installed spritemap.</summary>
    RidleyBreakupLegsLeft,
    /// <summary><c>ridley_breakup_legs_right</c>, an installed spritemap.</summary>
    RidleyBreakupLegsRight,
    /// <summary><c>ridley_breakup_torso_left</c>, an installed spritemap.</summary>
    RidleyBreakupTorsoLeft,
    /// <summary><c>ridley_breakup_torso_right</c>, an installed spritemap.</summary>
    RidleyBreakupTorsoRight,
    /// <summary><c>ridley_breakup_head_left</c>, an installed spritemap.</summary>
    RidleyBreakupHeadLeft,
    /// <summary><c>ridley_breakup_head_right</c>, an installed spritemap.</summary>
    RidleyBreakupHeadRight,
    /// <summary><c>ridley_breakup_claw_left</c>, an installed spritemap.</summary>
    RidleyBreakupClawLeft,
    /// <summary><c>ridley_breakup_claw_right</c>, an installed spritemap.</summary>
    RidleyBreakupClawRight,
    /// <summary><c>walking_pirate_flinch_left</c>, an installed extended frame.</summary>
    WalkingPirateFlinchLeft,
    /// <summary><c>walking_pirate_flinch_right</c>, an installed extended frame.</summary>
    WalkingPirateFlinchRight,
    /// <summary><c>walking_pirate_walk_left_0</c>, an installed extended frame.</summary>
    WalkingPirateWalkLeft0,
    /// <summary><c>walking_pirate_walk_left_1</c>, an installed extended frame.</summary>
    WalkingPirateWalkLeft1,
    /// <summary><c>walking_pirate_walk_left_2</c>, an installed extended frame.</summary>
    WalkingPirateWalkLeft2,
    /// <summary><c>walking_pirate_walk_left_3</c>, an installed extended frame.</summary>
    WalkingPirateWalkLeft3,
    /// <summary><c>walking_pirate_walk_left_4</c>, an installed extended frame.</summary>
    WalkingPirateWalkLeft4,
    /// <summary><c>walking_pirate_walk_left_5</c>, an installed extended frame.</summary>
    WalkingPirateWalkLeft5,
    /// <summary><c>walking_pirate_walk_left_6</c>, an installed extended frame.</summary>
    WalkingPirateWalkLeft6,
    /// <summary><c>walking_pirate_walk_left_7</c>, an installed extended frame.</summary>
    WalkingPirateWalkLeft7,
    /// <summary><c>walking_pirate_fire_left_0</c>, an installed extended frame.</summary>
    WalkingPirateFireLeft0,
    /// <summary><c>walking_pirate_fire_left_1</c>, an installed extended frame.</summary>
    WalkingPirateFireLeft1,
    /// <summary><c>walking_pirate_fire_left_2</c>, an installed extended frame.</summary>
    WalkingPirateFireLeft2,
    /// <summary><c>walking_pirate_fire_left_3</c>, an installed extended frame.</summary>
    WalkingPirateFireLeft3,
    /// <summary><c>walking_pirate_fire_left_4</c>, an installed extended frame.</summary>
    WalkingPirateFireLeft4,
    /// <summary><c>walking_pirate_fire_left_5</c>, an installed extended frame.</summary>
    WalkingPirateFireLeft5,
    /// <summary><c>walking_pirate_look_left_0</c>, an installed extended frame.</summary>
    WalkingPirateLookLeft0,
    /// <summary><c>walking_pirate_look_left_1</c>, an installed extended frame.</summary>
    WalkingPirateLookLeft1,
    /// <summary><c>walking_pirate_look_left_2</c>, an installed extended frame.</summary>
    WalkingPirateLookLeft2,
    /// <summary><c>walking_pirate_look_shared</c>, an installed extended frame.</summary>
    WalkingPirateLookShared,
    /// <summary><c>walking_pirate_walk_right_0</c>, an installed extended frame.</summary>
    WalkingPirateWalkRight0,
    /// <summary><c>walking_pirate_walk_right_1</c>, an installed extended frame.</summary>
    WalkingPirateWalkRight1,
    /// <summary><c>walking_pirate_walk_right_2</c>, an installed extended frame.</summary>
    WalkingPirateWalkRight2,
    /// <summary><c>walking_pirate_walk_right_3</c>, an installed extended frame.</summary>
    WalkingPirateWalkRight3,
    /// <summary><c>walking_pirate_walk_right_4</c>, an installed extended frame.</summary>
    WalkingPirateWalkRight4,
    /// <summary><c>walking_pirate_walk_right_5</c>, an installed extended frame.</summary>
    WalkingPirateWalkRight5,
    /// <summary><c>walking_pirate_walk_right_6</c>, an installed extended frame.</summary>
    WalkingPirateWalkRight6,
    /// <summary><c>walking_pirate_walk_right_7</c>, an installed extended frame.</summary>
    WalkingPirateWalkRight7,
    /// <summary><c>walking_pirate_fire_right_0</c>, an installed extended frame.</summary>
    WalkingPirateFireRight0,
    /// <summary><c>walking_pirate_fire_right_1</c>, an installed extended frame.</summary>
    WalkingPirateFireRight1,
    /// <summary><c>walking_pirate_fire_right_2</c>, an installed extended frame.</summary>
    WalkingPirateFireRight2,
    /// <summary><c>walking_pirate_fire_right_3</c>, an installed extended frame.</summary>
    WalkingPirateFireRight3,
    /// <summary><c>walking_pirate_fire_right_4</c>, an installed extended frame.</summary>
    WalkingPirateFireRight4,
    /// <summary><c>walking_pirate_fire_right_5</c>, an installed extended frame.</summary>
    WalkingPirateFireRight5,
    /// <summary><c>walking_pirate_look_right_0</c>, an installed extended frame.</summary>
    WalkingPirateLookRight0,
    /// <summary><c>walking_pirate_look_right_1</c>, an installed extended frame.</summary>
    WalkingPirateLookRight1,
    /// <summary><c>walking_pirate_look_right_2</c>, an installed extended frame.</summary>
    WalkingPirateLookRight2,
    /// <summary><c>wall_pirate_fire_jump_left_0</c>, an installed extended frame.</summary>
    WallPirateFireJumpLeft0,
    /// <summary><c>wall_pirate_fire_jump_left_1</c>, an installed extended frame.</summary>
    WallPirateFireJumpLeft1,
    /// <summary><c>wall_pirate_fire_jump_left_2</c>, an installed extended frame.</summary>
    WallPirateFireJumpLeft2,
    /// <summary><c>wall_pirate_fire_jump_left_3</c>, an installed extended frame.</summary>
    WallPirateFireJumpLeft3,
    /// <summary><c>wall_pirate_climb_left_0</c>, an installed extended frame.</summary>
    WallPirateClimbLeft0,
    /// <summary><c>wall_pirate_climb_left_1</c>, an installed extended frame.</summary>
    WallPirateClimbLeft1,
    /// <summary><c>wall_pirate_climb_left_2</c>, an installed extended frame.</summary>
    WallPirateClimbLeft2,
    /// <summary><c>wall_pirate_climb_left_3</c>, an installed extended frame.</summary>
    WallPirateClimbLeft3,
    /// <summary><c>wall_pirate_climb_left_4</c>, an installed extended frame.</summary>
    WallPirateClimbLeft4,
    /// <summary><c>wall_pirate_fire_jump_right_0</c>, an installed extended frame.</summary>
    WallPirateFireJumpRight0,
    /// <summary><c>wall_pirate_fire_jump_right_1</c>, an installed extended frame.</summary>
    WallPirateFireJumpRight1,
    /// <summary><c>wall_pirate_fire_jump_right_2</c>, an installed extended frame.</summary>
    WallPirateFireJumpRight2,
    /// <summary><c>wall_pirate_fire_jump_right_3</c>, an installed extended frame.</summary>
    WallPirateFireJumpRight3,
    /// <summary><c>wall_pirate_climb_right_0</c>, an installed extended frame.</summary>
    WallPirateClimbRight0,
    /// <summary><c>wall_pirate_climb_right_1</c>, an installed extended frame.</summary>
    WallPirateClimbRight1,
    /// <summary><c>wall_pirate_climb_right_2</c>, an installed extended frame.</summary>
    WallPirateClimbRight2,
    /// <summary><c>wall_pirate_climb_right_3</c>, an installed extended frame.</summary>
    WallPirateClimbRight3,
    /// <summary><c>wall_pirate_climb_right_4</c>, an installed extended frame.</summary>
    WallPirateClimbRight4,
    /// <summary><c>ninja_pirate_pose_0</c>, an installed extended frame.</summary>
    NinjaPiratePose0,
    /// <summary><c>ninja_pirate_pose_1</c>, an installed extended frame.</summary>
    NinjaPiratePose1,
    /// <summary><c>ninja_pirate_pose_2</c>, an installed extended frame.</summary>
    NinjaPiratePose2,
    /// <summary><c>ninja_pirate_pose_3</c>, an installed extended frame.</summary>
    NinjaPiratePose3,
    /// <summary><c>ninja_pirate_pose_4</c>, an installed extended frame.</summary>
    NinjaPiratePose4,
    /// <summary><c>ninja_pirate_pose_5</c>, an installed extended frame.</summary>
    NinjaPiratePose5,
    /// <summary><c>ninja_pirate_pose_6</c>, an installed extended frame.</summary>
    NinjaPiratePose6,
    /// <summary><c>ninja_pirate_pose_7</c>, an installed extended frame.</summary>
    NinjaPiratePose7,
    /// <summary><c>ninja_pirate_pose_8</c>, an installed extended frame.</summary>
    NinjaPiratePose8,
    /// <summary><c>ninja_pirate_pose_17</c>, an installed extended frame.</summary>
    NinjaPiratePose17,
    /// <summary><c>ninja_pirate_pose_18</c>, an installed extended frame.</summary>
    NinjaPiratePose18,
    /// <summary><c>ninja_pirate_pose_19</c>, an installed extended frame.</summary>
    NinjaPiratePose19,
    /// <summary><c>ninja_pirate_pose_20</c>, an installed extended frame.</summary>
    NinjaPiratePose20,
    /// <summary><c>ninja_pirate_pose_21</c>, an installed extended frame.</summary>
    NinjaPiratePose21,
    /// <summary><c>ninja_pirate_pose_22</c>, an installed extended frame.</summary>
    NinjaPiratePose22,
    /// <summary><c>ninja_pirate_pose_23</c>, an installed extended frame.</summary>
    NinjaPiratePose23,
    /// <summary><c>ninja_pirate_pose_24</c>, an installed extended frame.</summary>
    NinjaPiratePose24,
    /// <summary><c>ninja_pirate_pose_49</c>, an installed extended frame.</summary>
    NinjaPiratePose49,
    /// <summary><c>ninja_pirate_pose_50</c>, an installed extended frame.</summary>
    NinjaPiratePose50,
    /// <summary><c>ninja_pirate_pose_51</c>, an installed extended frame.</summary>
    NinjaPiratePose51,
    /// <summary><c>ninja_pirate_pose_55</c>, an installed extended frame.</summary>
    NinjaPiratePose55,
    /// <summary><c>ninja_pirate_pose_65</c>, an installed extended frame.</summary>
    NinjaPiratePose65,
    /// <summary><c>ninja_pirate_pose_57</c>, an installed extended frame.</summary>
    NinjaPiratePose57,
    /// <summary><c>ninja_pirate_pose_58</c>, an installed extended frame.</summary>
    NinjaPiratePose58,
    /// <summary><c>ninja_pirate_pose_33</c>, an installed extended frame.</summary>
    NinjaPiratePose33,
    /// <summary><c>ninja_pirate_pose_34</c>, an installed extended frame.</summary>
    NinjaPiratePose34,
    /// <summary><c>ninja_pirate_pose_35</c>, an installed extended frame.</summary>
    NinjaPiratePose35,
    /// <summary><c>ninja_pirate_pose_36</c>, an installed extended frame.</summary>
    NinjaPiratePose36,
    /// <summary><c>ninja_pirate_pose_37</c>, an installed extended frame.</summary>
    NinjaPiratePose37,
    /// <summary><c>ninja_pirate_pose_38</c>, an installed extended frame.</summary>
    NinjaPiratePose38,
    /// <summary><c>ninja_pirate_pose_39</c>, an installed extended frame.</summary>
    NinjaPiratePose39,
    /// <summary><c>ninja_pirate_pose_40</c>, an installed extended frame.</summary>
    NinjaPiratePose40,
    /// <summary><c>ninja_pirate_pose_69</c>, an installed extended frame.</summary>
    NinjaPiratePose69,
    /// <summary><c>ninja_pirate_pose_70</c>, an installed extended frame.</summary>
    NinjaPiratePose70,
    /// <summary><c>ninja_pirate_pose_71</c>, an installed extended frame.</summary>
    NinjaPiratePose71,
    /// <summary><c>ninja_pirate_pose_75</c>, an installed extended frame.</summary>
    NinjaPiratePose75,
    /// <summary><c>ninja_pirate_pose_73</c>, an installed extended frame.</summary>
    NinjaPiratePose73,
    /// <summary><c>ninja_pirate_pose_52</c>, an installed extended frame.</summary>
    NinjaPiratePose52,
    /// <summary><c>ninja_pirate_pose_53</c>, an installed extended frame.</summary>
    NinjaPiratePose53,
    /// <summary><c>ninja_pirate_pose_54</c>, an installed extended frame.</summary>
    NinjaPiratePose54,
    /// <summary><c>ninja_pirate_pose_61</c>, an installed extended frame.</summary>
    NinjaPiratePose61,
    /// <summary><c>ninja_pirate_pose_67</c>, an installed extended frame.</summary>
    NinjaPiratePose67,
    /// <summary><c>ninja_pirate_pose_62</c>, an installed extended frame.</summary>
    NinjaPiratePose62,
    /// <summary><c>ninja_pirate_pose_9</c>, an installed extended frame.</summary>
    NinjaPiratePose9,
    /// <summary><c>ninja_pirate_pose_10</c>, an installed extended frame.</summary>
    NinjaPiratePose10,
    /// <summary><c>ninja_pirate_pose_11</c>, an installed extended frame.</summary>
    NinjaPiratePose11,
    /// <summary><c>ninja_pirate_pose_12</c>, an installed extended frame.</summary>
    NinjaPiratePose12,
    /// <summary><c>ninja_pirate_pose_13</c>, an installed extended frame.</summary>
    NinjaPiratePose13,
    /// <summary><c>ninja_pirate_pose_14</c>, an installed extended frame.</summary>
    NinjaPiratePose14,
    /// <summary><c>ninja_pirate_pose_15</c>, an installed extended frame.</summary>
    NinjaPiratePose15,
    /// <summary><c>ninja_pirate_pose_16</c>, an installed extended frame.</summary>
    NinjaPiratePose16,
    /// <summary><c>ninja_pirate_pose_25</c>, an installed extended frame.</summary>
    NinjaPiratePose25,
    /// <summary><c>ninja_pirate_pose_26</c>, an installed extended frame.</summary>
    NinjaPiratePose26,
    /// <summary><c>ninja_pirate_pose_27</c>, an installed extended frame.</summary>
    NinjaPiratePose27,
    /// <summary><c>ninja_pirate_pose_28</c>, an installed extended frame.</summary>
    NinjaPiratePose28,
    /// <summary><c>ninja_pirate_pose_29</c>, an installed extended frame.</summary>
    NinjaPiratePose29,
    /// <summary><c>ninja_pirate_pose_30</c>, an installed extended frame.</summary>
    NinjaPiratePose30,
    /// <summary><c>ninja_pirate_pose_31</c>, an installed extended frame.</summary>
    NinjaPiratePose31,
    /// <summary><c>ninja_pirate_pose_32</c>, an installed extended frame.</summary>
    NinjaPiratePose32,
    /// <summary><c>ninja_pirate_pose_56</c>, an installed extended frame.</summary>
    NinjaPiratePose56,
    /// <summary><c>ninja_pirate_pose_66</c>, an installed extended frame.</summary>
    NinjaPiratePose66,
    /// <summary><c>ninja_pirate_pose_59</c>, an installed extended frame.</summary>
    NinjaPiratePose59,
    /// <summary><c>ninja_pirate_pose_60</c>, an installed extended frame.</summary>
    NinjaPiratePose60,
    /// <summary><c>ninja_pirate_pose_41</c>, an installed extended frame.</summary>
    NinjaPiratePose41,
    /// <summary><c>ninja_pirate_pose_42</c>, an installed extended frame.</summary>
    NinjaPiratePose42,
    /// <summary><c>ninja_pirate_pose_43</c>, an installed extended frame.</summary>
    NinjaPiratePose43,
    /// <summary><c>ninja_pirate_pose_44</c>, an installed extended frame.</summary>
    NinjaPiratePose44,
    /// <summary><c>ninja_pirate_pose_45</c>, an installed extended frame.</summary>
    NinjaPiratePose45,
    /// <summary><c>ninja_pirate_pose_46</c>, an installed extended frame.</summary>
    NinjaPiratePose46,
    /// <summary><c>ninja_pirate_pose_47</c>, an installed extended frame.</summary>
    NinjaPiratePose47,
    /// <summary><c>ninja_pirate_pose_48</c>, an installed extended frame.</summary>
    NinjaPiratePose48,
    /// <summary><c>ninja_pirate_pose_72</c>, an installed extended frame.</summary>
    NinjaPiratePose72,
    /// <summary><c>ninja_pirate_pose_74</c>, an installed extended frame.</summary>
    NinjaPiratePose74,
    /// <summary><c>ninja_pirate_pose_63</c>, an installed extended frame.</summary>
    NinjaPiratePose63,
    /// <summary><c>ninja_pirate_pose_68</c>, an installed extended frame.</summary>
    NinjaPiratePose68,
    /// <summary><c>ninja_pirate_pose_64</c>, an installed extended frame.</summary>
    NinjaPiratePose64,
    /// <summary><c>ridley_body_left</c>, an installed extended frame.</summary>
    RidleyBodyLeft,
    /// <summary><c>ridley_body_right</c>, an installed extended frame.</summary>
    RidleyBodyRight,
    /// <summary><c>ridley_body_left_mouth_half_open</c>, an installed extended frame.</summary>
    RidleyBodyLeftMouthHalfOpen,
    /// <summary><c>ridley_body_left_mouth_open</c>, an installed extended frame.</summary>
    RidleyBodyLeftMouthOpen,
    /// <summary><c>ridley_body_right_mouth_half_open</c>, an installed extended frame.</summary>
    RidleyBodyRightMouthHalfOpen,
    /// <summary><c>ridley_body_right_mouth_open</c>, an installed extended frame.</summary>
    RidleyBodyRightMouthOpen,
    /// <summary><c>ridley_body_left_legs_half_extended</c>, an installed extended frame.</summary>
    RidleyBodyLeftLegsHalfExtended,
    /// <summary><c>ridley_body_left_legs_extended</c>, an installed extended frame.</summary>
    RidleyBodyLeftLegsExtended,
    /// <summary><c>ridley_body_right_legs_half_extended</c>, an installed extended frame.</summary>
    RidleyBodyRightLegsHalfExtended,
    /// <summary><c>ridley_body_right_legs_extended</c>, an installed extended frame.</summary>
    RidleyBodyRightLegsExtended,
    /// <summary><c>ridley_body_forward</c>, an installed extended frame.</summary>
    RidleyBodyForward,
    /// <summary><c>draygon_frame_0</c>, an installed extended frame.</summary>
    DraygonFrame0,
    /// <summary><c>draygon_frame_1</c>, an installed extended frame.</summary>
    DraygonFrame1,
    /// <summary><c>draygon_frame_2</c>, an installed extended frame.</summary>
    DraygonFrame2,
    /// <summary><c>draygon_frame_3</c>, an installed extended frame.</summary>
    DraygonFrame3,
    /// <summary><c>draygon_frame_4</c>, an installed extended frame.</summary>
    DraygonFrame4,
    /// <summary><c>draygon_frame_5</c>, an installed extended frame.</summary>
    DraygonFrame5,
    /// <summary><c>draygon_frame_23</c>, an installed extended frame.</summary>
    DraygonFrame23,
    /// <summary><c>draygon_frame_24</c>, an installed extended frame.</summary>
    DraygonFrame24,
    /// <summary><c>draygon_frame_25</c>, an installed extended frame.</summary>
    DraygonFrame25,
    /// <summary><c>draygon_frame_26</c>, an installed extended frame.</summary>
    DraygonFrame26,
    /// <summary><c>draygon_frame_27</c>, an installed extended frame.</summary>
    DraygonFrame27,
    /// <summary><c>draygon_frame_28</c>, an installed extended frame.</summary>
    DraygonFrame28,
    /// <summary><c>draygon_frame_29</c>, an installed extended frame.</summary>
    DraygonFrame29,
    /// <summary><c>draygon_frame_30</c>, an installed extended frame.</summary>
    DraygonFrame30,
    /// <summary><c>draygon_frame_31</c>, an installed extended frame.</summary>
    DraygonFrame31,
    /// <summary><c>draygon_frame_32</c>, an installed extended frame.</summary>
    DraygonFrame32,
    /// <summary><c>draygon_frame_33</c>, an installed extended frame.</summary>
    DraygonFrame33,
    /// <summary><c>draygon_frame_34</c>, an installed extended frame.</summary>
    DraygonFrame34,
    /// <summary><c>draygon_frame_35</c>, an installed extended frame.</summary>
    DraygonFrame35,
    /// <summary><c>draygon_frame_36</c>, an installed extended frame.</summary>
    DraygonFrame36,
    /// <summary><c>draygon_frame_37</c>, an installed extended frame.</summary>
    DraygonFrame37,
    /// <summary><c>draygon_frame_38</c>, an installed extended frame.</summary>
    DraygonFrame38,
    /// <summary><c>draygon_frame_39</c>, an installed extended frame.</summary>
    DraygonFrame39,
    /// <summary><c>draygon_frame_40</c>, an installed extended frame.</summary>
    DraygonFrame40,
    /// <summary><c>draygon_frame_41</c>, an installed extended frame.</summary>
    DraygonFrame41,
    /// <summary><c>draygon_frame_42</c>, an installed extended frame.</summary>
    DraygonFrame42,
    /// <summary><c>draygon_frame_43</c>, an installed extended frame.</summary>
    DraygonFrame43,
    /// <summary><c>draygon_frame_44</c>, an installed extended frame.</summary>
    DraygonFrame44,
    /// <summary><c>draygon_frame_45</c>, an installed extended frame.</summary>
    DraygonFrame45,
    /// <summary><c>draygon_frame_46</c>, an installed extended frame.</summary>
    DraygonFrame46,
    /// <summary><c>draygon_frame_64</c>, an installed extended frame.</summary>
    DraygonFrame64,
    /// <summary><c>draygon_frame_65</c>, an installed extended frame.</summary>
    DraygonFrame65,
    /// <summary><c>draygon_frame_66</c>, an installed extended frame.</summary>
    DraygonFrame66,
    /// <summary><c>draygon_frame_67</c>, an installed extended frame.</summary>
    DraygonFrame67,
    /// <summary><c>draygon_frame_68</c>, an installed extended frame.</summary>
    DraygonFrame68,
    /// <summary><c>draygon_frame_69</c>, an installed extended frame.</summary>
    DraygonFrame69,
    /// <summary><c>draygon_frame_70</c>, an installed extended frame.</summary>
    DraygonFrame70,
    /// <summary><c>draygon_frame_71</c>, an installed extended frame.</summary>
    DraygonFrame71,
    /// <summary><c>draygon_frame_72</c>, an installed extended frame.</summary>
    DraygonFrame72,
    /// <summary><c>draygon_frame_73</c>, an installed extended frame.</summary>
    DraygonFrame73,
    /// <summary><c>draygon_frame_74</c>, an installed extended frame.</summary>
    DraygonFrame74,
    /// <summary><c>draygon_frame_75</c>, an installed extended frame.</summary>
    DraygonFrame75,
    /// <summary><c>draygon_frame_76</c>, an installed extended frame.</summary>
    DraygonFrame76,
    /// <summary><c>draygon_frame_77</c>, an installed extended frame.</summary>
    DraygonFrame77,
    /// <summary><c>draygon_frame_78</c>, an installed extended frame.</summary>
    DraygonFrame78,
    /// <summary><c>draygon_frame_79</c>, an installed extended frame.</summary>
    DraygonFrame79,
    /// <summary><c>draygon_frame_80</c>, an installed extended frame.</summary>
    DraygonFrame80,
    /// <summary><c>draygon_frame_81</c>, an installed extended frame.</summary>
    DraygonFrame81,
    /// <summary><c>spore_spawn_dead_0</c>, an installed extended frame.</summary>
    SporeSpawnDead0,
    /// <summary><c>spore_spawn_closed_0</c>, an installed extended frame.</summary>
    SporeSpawnClosed0,
    /// <summary><c>spore_spawn_opening_0</c>, an installed extended frame.</summary>
    SporeSpawnOpening0,
    /// <summary><c>spore_spawn_opening_1</c>, an installed extended frame.</summary>
    SporeSpawnOpening1,
    /// <summary><c>spore_spawn_opening_2</c>, an installed extended frame.</summary>
    SporeSpawnOpening2,
    /// <summary><c>spore_spawn_opening_3</c>, an installed extended frame.</summary>
    SporeSpawnOpening3,
    /// <summary><c>spore_spawn_opening_4</c>, an installed extended frame.</summary>
    SporeSpawnOpening4,
    /// <summary><c>spore_spawn_opening_5</c>, an installed extended frame.</summary>
    SporeSpawnOpening5,
    /// <summary><c>spore_spawn_opening_6</c>, an installed extended frame.</summary>
    SporeSpawnOpening6,
    /// <summary><c>spore_spawn_open_0</c>, an installed extended frame.</summary>
    SporeSpawnOpen0,
    /// <summary><c>spore_spawn_open_1</c>, an installed extended frame.</summary>
    SporeSpawnOpen1,
    /// <summary><c>spore_spawn_open_2</c>, an installed extended frame.</summary>
    SporeSpawnOpen2,
    /// <summary><c>ceres_steam_pose_0</c>, an installed extended frame.</summary>
    CeresSteamPose0,
    /// <summary><c>ceres_steam_pose_1</c>, an installed extended frame.</summary>
    CeresSteamPose1,
    /// <summary><c>ceres_steam_pose_2</c>, an installed extended frame.</summary>
    CeresSteamPose2,
    /// <summary><c>ceres_steam_pose_3</c>, an installed extended frame.</summary>
    CeresSteamPose3,
    /// <summary><c>ceres_steam_pose_4</c>, an installed extended frame.</summary>
    CeresSteamPose4,
    /// <summary><c>ceres_steam_pose_5</c>, an installed extended frame.</summary>
    CeresSteamPose5,
    /// <summary><c>ceres_steam_pose_6</c>, an installed extended frame.</summary>
    CeresSteamPose6,
    /// <summary><c>ceres_steam_pose_7</c>, an installed extended frame.</summary>
    CeresSteamPose7,
    /// <summary><c>ceres_steam_pose_8</c>, an installed extended frame.</summary>
    CeresSteamPose8,
    /// <summary><c>ceres_steam_pose_9</c>, an installed extended frame.</summary>
    CeresSteamPose9,
    /// <summary><c>ceres_steam_pose_10</c>, an installed extended frame.</summary>
    CeresSteamPose10,
    /// <summary><c>ceres_steam_pose_11</c>, an installed extended frame.</summary>
    CeresSteamPose11,
    /// <summary><c>ceres_steam_pose_12</c>, an installed extended frame.</summary>
    CeresSteamPose12,
    /// <summary><c>ceres_steam_pose_13</c>, an installed extended frame.</summary>
    CeresSteamPose13,
    /// <summary><c>ceres_steam_pose_14</c>, an installed extended frame.</summary>
    CeresSteamPose14,
    /// <summary><c>ceres_steam_pose_15</c>, an installed extended frame.</summary>
    CeresSteamPose15,
    /// <summary><c>ceres_steam_pose_16</c>, an installed extended frame.</summary>
    CeresSteamPose16,
    /// <summary><c>ceres_steam_pose_17</c>, an installed extended frame.</summary>
    CeresSteamPose17,
    /// <summary><c>ceres_steam_pose_18</c>, an installed extended frame.</summary>
    CeresSteamPose18,
    /// <summary><c>ceres_steam_pose_19</c>, an installed extended frame.</summary>
    CeresSteamPose19,
    /// <summary><c>ceres_steam_pose_20</c>, an installed extended frame.</summary>
    CeresSteamPose20,
    /// <summary><c>ceres_steam_pose_21</c>, an installed extended frame.</summary>
    CeresSteamPose21,
    /// <summary><c>ceres_steam_pose_22</c>, an installed extended frame.</summary>
    CeresSteamPose22,
    /// <summary><c>ceres_steam_pose_23</c>, an installed extended frame.</summary>
    CeresSteamPose23,
    /// <summary><c>ceres_steam_pose_24</c>, an installed extended frame.</summary>
    CeresSteamPose24,
    /// <summary><c>ceres_steam_pose_25</c>, an installed extended frame.</summary>
    CeresSteamPose25,
    /// <summary><c>ceres_steam_pose_26</c>, an installed extended frame.</summary>
    CeresSteamPose26,
    /// <summary><c>ceres_steam_pose_27</c>, an installed extended frame.</summary>
    CeresSteamPose27,
    /// <summary><c>oum_pose_0</c>, an installed extended frame.</summary>
    OumPose0,
    /// <summary><c>oum_pose_1</c>, an installed extended frame.</summary>
    OumPose1,
    /// <summary><c>oum_pose_2</c>, an installed extended frame.</summary>
    OumPose2,
    /// <summary><c>oum_pose_3</c>, an installed extended frame.</summary>
    OumPose3,
    /// <summary><c>oum_pose_4</c>, an installed extended frame.</summary>
    OumPose4,
    /// <summary><c>oum_pose_5</c>, an installed extended frame.</summary>
    OumPose5,
    /// <summary><c>oum_pose_6</c>, an installed extended frame.</summary>
    OumPose6,
    /// <summary><c>oum_pose_7</c>, an installed extended frame.</summary>
    OumPose7,
    /// <summary><c>oum_pose_8</c>, an installed extended frame.</summary>
    OumPose8,
    /// <summary><c>oum_pose_9</c>, an installed extended frame.</summary>
    OumPose9,
    /// <summary><c>oum_pose_10</c>, an installed extended frame.</summary>
    OumPose10,
    /// <summary><c>oum_pose_11</c>, an installed extended frame.</summary>
    OumPose11,
    /// <summary><c>oum_pose_12</c>, an installed extended frame.</summary>
    OumPose12,
    /// <summary><c>oum_pose_13</c>, an installed extended frame.</summary>
    OumPose13,
    /// <summary><c>oum_pose_14</c>, an installed extended frame.</summary>
    OumPose14,
    /// <summary><c>oum_pose_15</c>, an installed extended frame.</summary>
    OumPose15,
    /// <summary><c>oum_pose_16</c>, an installed extended frame.</summary>
    OumPose16,
    /// <summary><c>oum_pose_17</c>, an installed extended frame.</summary>
    OumPose17,
    /// <summary><c>oum_pose_18</c>, an installed extended frame.</summary>
    OumPose18,
    /// <summary><c>oum_pose_19</c>, an installed extended frame.</summary>
    OumPose19,
    /// <summary><c>oum_pose_20</c>, an installed extended frame.</summary>
    OumPose20,
    /// <summary><c>oum_pose_21</c>, an installed extended frame.</summary>
    OumPose21,
    /// <summary><c>oum_pose_22</c>, an installed extended frame.</summary>
    OumPose22,
    /// <summary><c>oum_pose_23</c>, an installed extended frame.</summary>
    OumPose23,
    /// <summary><c>oum_pose_24</c>, an installed extended frame.</summary>
    OumPose24,
    /// <summary><c>oum_pose_25</c>, an installed extended frame.</summary>
    OumPose25,
    /// <summary><c>oum_pose_26</c>, an installed extended frame.</summary>
    OumPose26,
    /// <summary><c>oum_pose_27</c>, an installed extended frame.</summary>
    OumPose27,
    /// <summary><c>oum_pose_28</c>, an installed extended frame.</summary>
    OumPose28,
    /// <summary><c>oum_pose_29</c>, an installed extended frame.</summary>
    OumPose29,
    /// <summary><c>crocomire_tongue_fight_0</c>, an installed extended frame.</summary>
    CrocomireTongueFight0,
    /// <summary><c>crocomire_tongue_fight_1</c>, an installed extended frame.</summary>
    CrocomireTongueFight1,
    /// <summary><c>crocomire_tongue_fight_2</c>, an installed extended frame.</summary>
    CrocomireTongueFight2,
    /// <summary><c>crocomire_tongue_fight_3</c>, an installed extended frame.</summary>
    CrocomireTongueFight3,
    /// <summary><c>crocomire_tongue_melting_pose_0</c>, an installed extended frame.</summary>
    CrocomireTongueMeltingPose0,
    /// <summary><c>crocomire_tongue_melting_pose_1</c>, an installed extended frame.</summary>
    CrocomireTongueMeltingPose1,
    /// <summary><c>crocomire_tongue_melting_pose_2</c>, an installed extended frame.</summary>
    CrocomireTongueMeltingPose2,
    /// <summary><c>crocomire_tongue_melting_pose_3</c>, an installed extended frame.</summary>
    CrocomireTongueMeltingPose3,
    /// <summary><c>crocomire_tongue_melting_pose_4</c>, an installed extended frame.</summary>
    CrocomireTongueMeltingPose4,
    /// <summary><c>crocomire_body_0</c>, an installed extended frame.</summary>
    CrocomireBody0,
    /// <summary><c>crocomire_body_1</c>, an installed extended frame.</summary>
    CrocomireBody1,
    /// <summary><c>crocomire_body_2</c>, an installed extended frame.</summary>
    CrocomireBody2,
    /// <summary><c>crocomire_body_3</c>, an installed extended frame.</summary>
    CrocomireBody3,
    /// <summary><c>crocomire_body_4</c>, an installed extended frame.</summary>
    CrocomireBody4,
    /// <summary><c>crocomire_body_5</c>, an installed extended frame.</summary>
    CrocomireBody5,
    /// <summary><c>crocomire_body_6</c>, an installed extended frame.</summary>
    CrocomireBody6,
    /// <summary><c>crocomire_body_7</c>, an installed extended frame.</summary>
    CrocomireBody7,
    /// <summary><c>crocomire_body_8</c>, an installed extended frame.</summary>
    CrocomireBody8,
    /// <summary><c>crocomire_body_9</c>, an installed extended frame.</summary>
    CrocomireBody9,
    /// <summary><c>crocomire_body_10</c>, an installed extended frame.</summary>
    CrocomireBody10,
    /// <summary><c>crocomire_body_11</c>, an installed extended frame.</summary>
    CrocomireBody11,
    /// <summary><c>crocomire_body_12</c>, an installed extended frame.</summary>
    CrocomireBody12,
    /// <summary><c>crocomire_body_13</c>, an installed extended frame.</summary>
    CrocomireBody13,
    /// <summary><c>crocomire_body_14</c>, an installed extended frame.</summary>
    CrocomireBody14,
    /// <summary><c>crocomire_body_15</c>, an installed extended frame.</summary>
    CrocomireBody15,
    /// <summary><c>crocomire_body_16</c>, an installed extended frame.</summary>
    CrocomireBody16,
    /// <summary><c>crocomire_body_17</c>, an installed extended frame.</summary>
    CrocomireBody17,
    /// <summary><c>crocomire_body_18</c>, an installed extended frame.</summary>
    CrocomireBody18,
    /// <summary><c>crocomire_body_19</c>, an installed extended frame.</summary>
    CrocomireBody19,
    /// <summary><c>crocomire_body_20</c>, an installed extended frame.</summary>
    CrocomireBody20,
    /// <summary><c>crocomire_body_21</c>, an installed extended frame.</summary>
    CrocomireBody21,
    /// <summary><c>crocomire_body_22</c>, an installed extended frame.</summary>
    CrocomireBody22,
    /// <summary><c>crocomire_body_23</c>, an installed extended frame.</summary>
    CrocomireBody23,
    /// <summary><c>crocomire_body_24</c>, an installed extended frame.</summary>
    CrocomireBody24,
    /// <summary><c>crocomire_body_25</c>, an installed extended frame.</summary>
    CrocomireBody25,
    /// <summary><c>crocomire_body_26</c>, an installed extended frame.</summary>
    CrocomireBody26,
    /// <summary><c>crocomire_body_27</c>, an installed extended frame.</summary>
    CrocomireBody27,
    /// <summary><c>crocomire_body_28</c>, an installed extended frame.</summary>
    CrocomireBody28,
    /// <summary><c>crocomire_body_29</c>, an installed extended frame.</summary>
    CrocomireBody29,
    /// <summary><c>crocomire_body_30</c>, an installed extended frame.</summary>
    CrocomireBody30,
    /// <summary><c>crocomire_body_31</c>, an installed extended frame.</summary>
    CrocomireBody31,
    /// <summary><c>crocomire_body_32</c>, an installed extended frame.</summary>
    CrocomireBody32,
    /// <summary><c>crocomire_body_33</c>, an installed extended frame.</summary>
    CrocomireBody33,
    /// <summary><c>crocomire_body_34</c>, an installed extended frame.</summary>
    CrocomireBody34,
    /// <summary><c>crocomire_body_35</c>, an installed extended frame.</summary>
    CrocomireBody35,
    /// <summary><c>crocomire_body_36</c>, an installed extended frame.</summary>
    CrocomireBody36,
    /// <summary><c>crocomire_body_37</c>, an installed extended frame.</summary>
    CrocomireBody37,
    /// <summary><c>crocomire_body_38</c>, an installed extended frame.</summary>
    CrocomireBody38,
    /// <summary><c>crocomire_body_39</c>, an installed extended frame.</summary>
    CrocomireBody39,
    /// <summary><c>crocomire_body_40</c>, an installed extended frame.</summary>
    CrocomireBody40,
    /// <summary><c>crocomire_body_41</c>, an installed extended frame.</summary>
    CrocomireBody41,
    /// <summary><c>crocomire_body_42</c>, an installed extended frame.</summary>
    CrocomireBody42,
    /// <summary><c>crocomire_body_43</c>, an installed extended frame.</summary>
    CrocomireBody43,
    /// <summary><c>crocomire_body_44</c>, an installed extended frame.</summary>
    CrocomireBody44,
    /// <summary><c>crocomire_body_45</c>, an installed extended frame.</summary>
    CrocomireBody45,
    /// <summary><c>crocomire_body_46</c>, an installed extended frame.</summary>
    CrocomireBody46,
    /// <summary><c>crocomire_body_47</c>, an installed extended frame.</summary>
    CrocomireBody47,
    /// <summary><c>crocomire_body_48</c>, an installed extended frame.</summary>
    CrocomireBody48,
    /// <summary><c>crocomire_body_49</c>, an installed extended frame.</summary>
    CrocomireBody49,
    /// <summary><c>bomb_torizo_dormant</c>, an installed extended frame.</summary>
    BombTorizoDormant,
    /// <summary><c>golden_torizo_initial</c>, an installed extended frame.</summary>
    GoldenTorizoInitial,
    /// <summary><c>kraid_arm_0</c>, an installed extended frame.</summary>
    KraidArm0,
    /// <summary><c>kraid_arm_1</c>, an installed extended frame.</summary>
    KraidArm1,
    /// <summary><c>kraid_arm_2</c>, an installed extended frame.</summary>
    KraidArm2,
    /// <summary><c>kraid_arm_3</c>, an installed extended frame.</summary>
    KraidArm3,
    /// <summary><c>kraid_arm_4</c>, an installed extended frame.</summary>
    KraidArm4,
    /// <summary><c>kraid_arm_5</c>, an installed extended frame.</summary>
    KraidArm5,
    /// <summary><c>kraid_arm_6</c>, an installed extended frame.</summary>
    KraidArm6,
    /// <summary><c>kraid_arm_7</c>, an installed extended frame.</summary>
    KraidArm7,
    /// <summary><c>kraid_arm_8</c>, an installed extended frame.</summary>
    KraidArm8,
    /// <summary><c>kraid_arm_9</c>, an installed extended frame.</summary>
    KraidArm9,
    /// <summary><c>kraid_arm_10</c>, an installed extended frame.</summary>
    KraidArm10,
    /// <summary><c>kraid_arm_11</c>, an installed extended frame.</summary>
    KraidArm11,
    /// <summary><c>kraid_arm_12</c>, an installed extended frame.</summary>
    KraidArm12,
    /// <summary><c>kraid_arm_13</c>, an installed extended frame.</summary>
    KraidArm13,
    /// <summary><c>kraid_arm_14</c>, an installed extended frame.</summary>
    KraidArm14,
    /// <summary><c>kraid_arm_15</c>, an installed extended frame.</summary>
    KraidArm15,
    /// <summary><c>kraid_arm_16</c>, an installed extended frame.</summary>
    KraidArm16,
    /// <summary><c>kraid_arm_17</c>, an installed extended frame.</summary>
    KraidArm17,
    /// <summary><c>kraid_arm_18</c>, an installed extended frame.</summary>
    KraidArm18,
    /// <summary><c>kraid_arm_19</c>, an installed extended frame.</summary>
    KraidArm19,
    /// <summary><c>kraid_arm_20</c>, an installed extended frame.</summary>
    KraidArm20,
    /// <summary><c>kraid_arm_21</c>, an installed extended frame.</summary>
    KraidArm21,
    /// <summary><c>golden_torizo_awake_0</c>, an installed extended frame.</summary>
    GoldenTorizoAwake0,
    /// <summary><c>golden_torizo_awake_1</c>, an installed extended frame.</summary>
    GoldenTorizoAwake1,
    /// <summary><c>golden_torizo_awake_2</c>, an installed extended frame.</summary>
    GoldenTorizoAwake2,
    /// <summary><c>golden_torizo_awake_3</c>, an installed extended frame.</summary>
    GoldenTorizoAwake3,
    /// <summary><c>golden_torizo_awake_4</c>, an installed extended frame.</summary>
    GoldenTorizoAwake4,
    /// <summary><c>golden_torizo_awake_5</c>, an installed extended frame.</summary>
    GoldenTorizoAwake5,
    /// <summary><c>golden_torizo_walk_left_0</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeft0,
    /// <summary><c>golden_torizo_walk_left_1</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeft1,
    /// <summary><c>golden_torizo_walk_left_2</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeft2,
    /// <summary><c>golden_torizo_walk_left_3</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeft3,
    /// <summary><c>golden_torizo_walk_left_4</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeft4,
    /// <summary><c>golden_torizo_walk_left_5</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeft5,
    /// <summary><c>golden_torizo_walk_left_6</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeft6,
    /// <summary><c>golden_torizo_walk_left_7</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeft7,
    /// <summary><c>golden_torizo_walk_left_8</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeft8,
    /// <summary><c>golden_torizo_walk_left_9</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeft9,
    /// <summary><c>golden_torizo_rightward_0</c>, an installed extended frame.</summary>
    GoldenTorizoRightward0,
    /// <summary><c>golden_torizo_rightward_1</c>, an installed extended frame.</summary>
    GoldenTorizoRightward1,
    /// <summary><c>golden_torizo_rightward_2</c>, an installed extended frame.</summary>
    GoldenTorizoRightward2,
    /// <summary><c>golden_torizo_rightward_3</c>, an installed extended frame.</summary>
    GoldenTorizoRightward3,
    /// <summary><c>golden_torizo_rightward_4</c>, an installed extended frame.</summary>
    GoldenTorizoRightward4,
    /// <summary><c>golden_torizo_rightward_5</c>, an installed extended frame.</summary>
    GoldenTorizoRightward5,
    /// <summary><c>golden_torizo_rightward_6</c>, an installed extended frame.</summary>
    GoldenTorizoRightward6,
    /// <summary><c>golden_torizo_rightward_7</c>, an installed extended frame.</summary>
    GoldenTorizoRightward7,
    /// <summary><c>golden_torizo_rightward_8</c>, an installed extended frame.</summary>
    GoldenTorizoRightward8,
    /// <summary><c>golden_torizo_rightward_9</c>, an installed extended frame.</summary>
    GoldenTorizoRightward9,
    /// <summary><c>golden_torizo_rightward_10</c>, an installed extended frame.</summary>
    GoldenTorizoRightward10,
    /// <summary><c>torizo_jump_back_0</c>, an installed extended frame.</summary>
    TorizoJumpBack0,
    /// <summary><c>torizo_jump_back_1</c>, an installed extended frame.</summary>
    TorizoJumpBack1,
    /// <summary><c>torizo_jump_back_2</c>, an installed extended frame.</summary>
    TorizoJumpBack2,
    /// <summary><c>golden_torizo_right_orb_0</c>, an installed extended frame.</summary>
    GoldenTorizoRightOrb0,
    /// <summary><c>golden_torizo_right_orb_1</c>, an installed extended frame.</summary>
    GoldenTorizoRightOrb1,
    /// <summary><c>golden_torizo_right_orb_2</c>, an installed extended frame.</summary>
    GoldenTorizoRightOrb2,
    /// <summary><c>golden_torizo_right_orb_3</c>, an installed extended frame.</summary>
    GoldenTorizoRightOrb3,
    /// <summary><c>golden_torizo_right_orb_4</c>, an installed extended frame.</summary>
    GoldenTorizoRightOrb4,
    /// <summary><c>golden_torizo_right_orb_5</c>, an installed extended frame.</summary>
    GoldenTorizoRightOrb5,
    /// <summary><c>golden_torizo_right_sonic_0</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic0,
    /// <summary><c>golden_torizo_right_sonic_1</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic1,
    /// <summary><c>golden_torizo_right_sonic_2</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic2,
    /// <summary><c>golden_torizo_right_sonic_3</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic3,
    /// <summary><c>golden_torizo_right_sonic_4</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic4,
    /// <summary><c>golden_torizo_right_sonic_5</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic5,
    /// <summary><c>golden_torizo_right_sonic_6</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic6,
    /// <summary><c>golden_torizo_right_sonic_7</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic7,
    /// <summary><c>golden_torizo_right_sonic_8</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic8,
    /// <summary><c>golden_torizo_right_sonic_9</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic9,
    /// <summary><c>golden_torizo_right_sonic_10</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic10,
    /// <summary><c>golden_torizo_right_sonic_11</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic11,
    /// <summary><c>golden_torizo_right_sonic_12</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic12,
    /// <summary><c>golden_torizo_right_sonic_13</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic13,
    /// <summary><c>golden_torizo_right_sonic_14</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic14,
    /// <summary><c>golden_torizo_right_sonic_15</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic15,
    /// <summary><c>golden_torizo_right_sonic_16</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic16,
    /// <summary><c>golden_torizo_right_sonic_17</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic17,
    /// <summary><c>golden_torizo_right_sonic_18</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic18,
    /// <summary><c>golden_torizo_right_sonic_19</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic19,
    /// <summary><c>golden_torizo_right_sonic_20</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic20,
    /// <summary><c>torizo_falling_left</c>, an installed extended frame.</summary>
    TorizoFallingLeft,
    /// <summary><c>golden_torizo_left_foot_orb_0</c>, an installed extended frame.</summary>
    GoldenTorizoLeftFootOrb0,
    /// <summary><c>golden_torizo_left_foot_orb_1</c>, an installed extended frame.</summary>
    GoldenTorizoLeftFootOrb1,
    /// <summary><c>golden_torizo_left_foot_orb_2</c>, an installed extended frame.</summary>
    GoldenTorizoLeftFootOrb2,
    /// <summary><c>golden_torizo_left_foot_orb_3</c>, an installed extended frame.</summary>
    GoldenTorizoLeftFootOrb3,
    /// <summary><c>golden_torizo_left_foot_orb_4</c>, an installed extended frame.</summary>
    GoldenTorizoLeftFootOrb4,
    /// <summary><c>torizo_jump_back_left_0</c>, an installed extended frame.</summary>
    TorizoJumpBackLeft0,
    /// <summary><c>torizo_jump_back_left_1</c>, an installed extended frame.</summary>
    TorizoJumpBackLeft1,
    /// <summary><c>golden_torizo_left_orb_0</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb0,
    /// <summary><c>golden_torizo_left_orb_1</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb1,
    /// <summary><c>golden_torizo_left_orb_2</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb2,
    /// <summary><c>golden_torizo_left_orb_3</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb3,
    /// <summary><c>golden_torizo_left_orb_4</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb4,
    /// <summary><c>golden_torizo_left_orb_5</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb5,
    /// <summary><c>golden_torizo_left_orb_6</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb6,
    /// <summary><c>golden_torizo_left_orb_7</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb7,
    /// <summary><c>golden_torizo_left_orb_8</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb8,
    /// <summary><c>golden_torizo_left_orb_9</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb9,
    /// <summary><c>golden_torizo_left_orb_10</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb10,
    /// <summary><c>golden_torizo_left_orb_11</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb11,
    /// <summary><c>torizo_combat_0</c>, an installed extended frame.</summary>
    TorizoCombat0,
    /// <summary><c>torizo_combat_1</c>, an installed extended frame.</summary>
    TorizoCombat1,
    /// <summary><c>torizo_combat_2</c>, an installed extended frame.</summary>
    TorizoCombat2,
    /// <summary><c>torizo_combat_3</c>, an installed extended frame.</summary>
    TorizoCombat3,
    /// <summary><c>torizo_combat_4</c>, an installed extended frame.</summary>
    TorizoCombat4,
    /// <summary><c>torizo_combat_5</c>, an installed extended frame.</summary>
    TorizoCombat5,
    /// <summary><c>torizo_combat_6</c>, an installed extended frame.</summary>
    TorizoCombat6,
    /// <summary><c>torizo_combat_7</c>, an installed extended frame.</summary>
    TorizoCombat7,
    /// <summary><c>torizo_combat_8</c>, an installed extended frame.</summary>
    TorizoCombat8,
    /// <summary><c>torizo_combat_9</c>, an installed extended frame.</summary>
    TorizoCombat9,
    /// <summary><c>torizo_combat_10</c>, an installed extended frame.</summary>
    TorizoCombat10,
    /// <summary><c>torizo_combat_11</c>, an installed extended frame.</summary>
    TorizoCombat11,
    /// <summary><c>torizo_combat_12</c>, an installed extended frame.</summary>
    TorizoCombat12,
    /// <summary><c>torizo_combat_13</c>, an installed extended frame.</summary>
    TorizoCombat13,
    /// <summary><c>torizo_combat_14</c>, an installed extended frame.</summary>
    TorizoCombat14,
    /// <summary><c>torizo_combat_15</c>, an installed extended frame.</summary>
    TorizoCombat15,
    /// <summary><c>torizo_combat_16</c>, an installed extended frame.</summary>
    TorizoCombat16,
    /// <summary><c>torizo_combat_17</c>, an installed extended frame.</summary>
    TorizoCombat17,
    /// <summary><c>torizo_combat_18</c>, an installed extended frame.</summary>
    TorizoCombat18,
    /// <summary><c>torizo_combat_19</c>, an installed extended frame.</summary>
    TorizoCombat19,
    /// <summary><c>torizo_combat_20</c>, an installed extended frame.</summary>
    TorizoCombat20,
    /// <summary><c>torizo_combat_21</c>, an installed extended frame.</summary>
    TorizoCombat21,
    /// <summary><c>torizo_combat_22</c>, an installed extended frame.</summary>
    TorizoCombat22,
    /// <summary><c>torizo_combat_23</c>, an installed extended frame.</summary>
    TorizoCombat23,
    /// <summary><c>torizo_combat_24</c>, an installed extended frame.</summary>
    TorizoCombat24,
    /// <summary><c>torizo_combat_25</c>, an installed extended frame.</summary>
    TorizoCombat25,
    /// <summary><c>torizo_combat_26</c>, an installed extended frame.</summary>
    TorizoCombat26,
    /// <summary><c>mother_brain_body_oam_standing</c>, an installed extended frame.</summary>
    MotherBrainBodyStanding,
    /// <summary><c>mother_brain_body_oam_walking_0</c>, an installed extended frame.</summary>
    MotherBrainBodyWalking0,
    /// <summary><c>mother_brain_body_oam_walking_1</c>, an installed extended frame.</summary>
    MotherBrainBodyWalking1,
    /// <summary><c>mother_brain_body_oam_walking_2</c>, an installed extended frame.</summary>
    MotherBrainBodyWalking2,
    /// <summary><c>mother_brain_body_oam_walking_3</c>, an installed extended frame.</summary>
    MotherBrainBodyWalking3,
    /// <summary><c>mother_brain_body_oam_walking_4</c>, an installed extended frame.</summary>
    MotherBrainBodyWalking4,
    /// <summary><c>mother_brain_body_oam_walking_5</c>, an installed extended frame.</summary>
    MotherBrainBodyWalking5,
    /// <summary><c>mother_brain_body_oam_walking_6</c>, an installed extended frame.</summary>
    MotherBrainBodyWalking6,
    /// <summary><c>mother_brain_body_oam_walking_7</c>, an installed extended frame.</summary>
    MotherBrainBodyWalking7,
    /// <summary><c>mother_brain_body_oam_crouched</c>, an installed extended frame.</summary>
    MotherBrainBodyCrouched,
    /// <summary><c>mother_brain_body_oam_uncrouching</c>, an installed extended frame.</summary>
    MotherBrainBodyUncrouching,
    /// <summary><c>mother_brain_body_oam_leaning_down</c>, an installed extended frame.</summary>
    MotherBrainBodyLeaningDown,
    /// <summary><c>mother_brain_body_oam_initial_dummy</c>, an installed extended frame.</summary>
    MotherBrainBodyInitialDummy,
    /// <summary><c>mother_brain_body_oam_death_beam_0</c>, an installed extended frame.</summary>
    MotherBrainBodyDeathBeam0,
    /// <summary><c>mother_brain_body_oam_death_beam_1</c>, an installed extended frame.</summary>
    MotherBrainBodyDeathBeam1,
    /// <summary><c>mother_brain_body_oam_death_beam_2</c>, an installed extended frame.</summary>
    MotherBrainBodyDeathBeam2,
    /// <summary><c>mother_brain_body_oam_death_beam_3</c>, an installed extended frame.</summary>
    MotherBrainBodyDeathBeam3,
    /// <summary><c>phantoon_invulnerable_body_0</c>, an installed extended frame.</summary>
    PhantoonInvulnerableBody0,
    /// <summary><c>phantoon_full_hitbox_body_0</c>, an installed extended frame.</summary>
    PhantoonFullHitboxBody0,
    /// <summary><c>phantoon_eye_hitbox_body_0</c>, an installed extended frame.</summary>
    PhantoonEyeHitboxBody0,
    /// <summary><c>phantoon_closed_eye_0</c>, an installed extended frame.</summary>
    PhantoonClosedEye0,
    /// <summary><c>phantoon_opening_eye_0</c>, an installed extended frame.</summary>
    PhantoonOpeningEye0,
    /// <summary><c>phantoon_opening_eye_1</c>, an installed extended frame.</summary>
    PhantoonOpeningEye1,
    /// <summary><c>phantoon_opening_eye_2</c>, an installed extended frame.</summary>
    PhantoonOpeningEye2,
    /// <summary><c>phantoon_centered_eye_0</c>, an installed extended frame.</summary>
    PhantoonCenteredEye0,
    /// <summary><c>phantoon_gaze_0</c>, an installed extended frame.</summary>
    PhantoonGaze0,
    /// <summary><c>phantoon_gaze_4</c>, an installed extended frame.</summary>
    PhantoonGaze4,
    /// <summary><c>phantoon_gaze_6</c>, an installed extended frame.</summary>
    PhantoonGaze6,
    /// <summary><c>phantoon_gaze_2</c>, an installed extended frame.</summary>
    PhantoonGaze2,
    /// <summary><c>phantoon_gaze_5</c>, an installed extended frame.</summary>
    PhantoonGaze5,
    /// <summary><c>phantoon_gaze_3</c>, an installed extended frame.</summary>
    PhantoonGaze3,
    /// <summary><c>phantoon_gaze_7</c>, an installed extended frame.</summary>
    PhantoonGaze7,
    /// <summary><c>phantoon_gaze_1</c>, an installed extended frame.</summary>
    PhantoonGaze1,
    /// <summary><c>phantoon_tentacle_0</c>, an installed extended frame.</summary>
    PhantoonTentacle0,
    /// <summary><c>phantoon_tentacle_1</c>, an installed extended frame.</summary>
    PhantoonTentacle1,
    /// <summary><c>phantoon_tentacle_2</c>, an installed extended frame.</summary>
    PhantoonTentacle2,
    /// <summary><c>phantoon_mouth_0</c>, an installed extended frame.</summary>
    PhantoonMouth0,
    /// <summary><c>phantoon_mouth_1</c>, an installed extended frame.</summary>
    PhantoonMouth1,
    /// <summary><c>phantoon_mouth_2</c>, an installed extended frame.</summary>
    PhantoonMouth2,
    /// <summary><c>draygon_frame_6</c>, an installed extended frame.</summary>
    DraygonFrame6,
    /// <summary><c>draygon_frame_7</c>, an installed extended frame.</summary>
    DraygonFrame7,
    /// <summary><c>draygon_frame_8</c>, an installed extended frame.</summary>
    DraygonFrame8,
    /// <summary><c>draygon_frame_9</c>, an installed extended frame.</summary>
    DraygonFrame9,
    /// <summary><c>draygon_frame_10</c>, an installed extended frame.</summary>
    DraygonFrame10,
    /// <summary><c>draygon_frame_11</c>, an installed extended frame.</summary>
    DraygonFrame11,
    /// <summary><c>draygon_frame_12</c>, an installed extended frame.</summary>
    DraygonFrame12,
    /// <summary><c>draygon_frame_13</c>, an installed extended frame.</summary>
    DraygonFrame13,
    /// <summary><c>draygon_frame_14</c>, an installed extended frame.</summary>
    DraygonFrame14,
    /// <summary><c>draygon_frame_15</c>, an installed extended frame.</summary>
    DraygonFrame15,
    /// <summary><c>draygon_frame_16</c>, an installed extended frame.</summary>
    DraygonFrame16,
    /// <summary><c>draygon_frame_17</c>, an installed extended frame.</summary>
    DraygonFrame17,
    /// <summary><c>draygon_frame_18</c>, an installed extended frame.</summary>
    DraygonFrame18,
    /// <summary><c>draygon_frame_19</c>, an installed extended frame.</summary>
    DraygonFrame19,
    /// <summary><c>draygon_frame_20</c>, an installed extended frame.</summary>
    DraygonFrame20,
    /// <summary><c>draygon_frame_21</c>, an installed extended frame.</summary>
    DraygonFrame21,
    /// <summary><c>draygon_frame_22</c>, an installed extended frame.</summary>
    DraygonFrame22,
    /// <summary><c>draygon_frame_47</c>, an installed extended frame.</summary>
    DraygonFrame47,
    /// <summary><c>draygon_frame_48</c>, an installed extended frame.</summary>
    DraygonFrame48,
    /// <summary><c>draygon_frame_49</c>, an installed extended frame.</summary>
    DraygonFrame49,
    /// <summary><c>draygon_frame_50</c>, an installed extended frame.</summary>
    DraygonFrame50,
    /// <summary><c>draygon_frame_51</c>, an installed extended frame.</summary>
    DraygonFrame51,
    /// <summary><c>draygon_frame_52</c>, an installed extended frame.</summary>
    DraygonFrame52,
    /// <summary><c>draygon_frame_53</c>, an installed extended frame.</summary>
    DraygonFrame53,
    /// <summary><c>draygon_frame_54</c>, an installed extended frame.</summary>
    DraygonFrame54,
    /// <summary><c>draygon_frame_55</c>, an installed extended frame.</summary>
    DraygonFrame55,
    /// <summary><c>draygon_frame_56</c>, an installed extended frame.</summary>
    DraygonFrame56,
    /// <summary><c>draygon_frame_57</c>, an installed extended frame.</summary>
    DraygonFrame57,
    /// <summary><c>draygon_frame_58</c>, an installed extended frame.</summary>
    DraygonFrame58,
    /// <summary><c>draygon_frame_59</c>, an installed extended frame.</summary>
    DraygonFrame59,
    /// <summary><c>draygon_frame_60</c>, an installed extended frame.</summary>
    DraygonFrame60,
    /// <summary><c>draygon_frame_61</c>, an installed extended frame.</summary>
    DraygonFrame61,
    /// <summary><c>draygon_frame_62</c>, an installed extended frame.</summary>
    DraygonFrame62,
    /// <summary><c>draygon_frame_63</c>, an installed extended frame.</summary>
    DraygonFrame63,
    /// <summary><c>crocomire_skeleton_0</c>, an installed extended frame.</summary>
    CrocomireSkeleton0,
    /// <summary><c>crocomire_skeleton_1</c>, an installed extended frame.</summary>
    CrocomireSkeleton1,
    /// <summary><c>crocomire_skeleton_2</c>, an installed extended frame.</summary>
    CrocomireSkeleton2,
    /// <summary><c>crocomire_skeleton_3</c>, an installed extended frame.</summary>
    CrocomireSkeleton3,
    /// <summary><c>crocomire_skeleton_4</c>, an installed extended frame.</summary>
    CrocomireSkeleton4,
    /// <summary><c>crocomire_skeleton_5</c>, an installed extended frame.</summary>
    CrocomireSkeleton5,
    /// <summary><c>crocomire_skeleton_6</c>, an installed extended frame.</summary>
    CrocomireSkeleton6,
    /// <summary><c>crocomire_skeleton_7</c>, an installed extended frame.</summary>
    CrocomireSkeleton7,
    /// <summary><c>crocomire_skeleton_8</c>, an installed extended frame.</summary>
    CrocomireSkeleton8,
    /// <summary><c>crocomire_skeleton_9</c>, an installed extended frame.</summary>
    CrocomireSkeleton9,
    /// <summary><c>crocomire_skeleton_10</c>, an installed extended frame.</summary>
    CrocomireSkeleton10,
    /// <summary><c>crocomire_skeleton_11</c>, an installed extended frame.</summary>
    CrocomireSkeleton11,
    /// <summary><c>crocomire_skeleton_12</c>, an installed extended frame.</summary>
    CrocomireSkeleton12,
    /// <summary><c>crocomire_skeleton_13</c>, an installed extended frame.</summary>
    CrocomireSkeleton13,
    /// <summary><c>crocomire_skeleton_14</c>, an installed extended frame.</summary>
    CrocomireSkeleton14,
    /// <summary><c>crocomire_skeleton_15</c>, an installed extended frame.</summary>
    CrocomireSkeleton15,
    /// <summary><c>crocomire_skeleton_16</c>, an installed extended frame.</summary>
    CrocomireSkeleton16,
    /// <summary><c>crocomire_skeleton_17</c>, an installed extended frame.</summary>
    CrocomireSkeleton17,
    /// <summary><c>crocomire_skeleton_18</c>, an installed extended frame.</summary>
    CrocomireSkeleton18,
    /// <summary><c>crocomire_skeleton_19</c>, an installed extended frame.</summary>
    CrocomireSkeleton19,
    /// <summary><c>crocomire_skeleton_20</c>, an installed extended frame.</summary>
    CrocomireSkeleton20,
    /// <summary><c>crocomire_skeleton_21</c>, an installed extended frame.</summary>
    CrocomireSkeleton21,
    /// <summary><c>crocomire_skeleton_22</c>, an installed extended frame.</summary>
    CrocomireSkeleton22,
    /// <summary><c>crocomire_skeleton_23</c>, an installed extended frame.</summary>
    CrocomireSkeleton23,
    /// <summary><c>crocomire_skeleton_24</c>, an installed extended frame.</summary>
    CrocomireSkeleton24,
    /// <summary><c>crocomire_skeleton_25</c>, an installed extended frame.</summary>
    CrocomireSkeleton25,
    /// <summary><c>crocomire_skeleton_26</c>, an installed extended frame.</summary>
    CrocomireSkeleton26,
    /// <summary><c>crocomire_skeleton_27</c>, an installed extended frame.</summary>
    CrocomireSkeleton27,
    /// <summary><c>crocomire_skeleton_28</c>, an installed extended frame.</summary>
    CrocomireSkeleton28,
    /// <summary><c>crocomire_skeleton_29</c>, an installed extended frame.</summary>
    CrocomireSkeleton29,
    /// <summary><c>crocomire_skeleton_30</c>, an installed extended frame.</summary>
    CrocomireSkeleton30,
    /// <summary><c>crocomire_skeleton_31</c>, an installed extended frame.</summary>
    CrocomireSkeleton31,
    /// <summary><c>crocomire_skeleton_32</c>, an installed extended frame.</summary>
    CrocomireSkeleton32,
    /// <summary><c>kraid_foot_0</c>, an installed extended frame.</summary>
    KraidFoot0,
    /// <summary><c>kraid_foot_1</c>, an installed extended frame.</summary>
    KraidFoot1,
    /// <summary><c>kraid_foot_2</c>, an installed extended frame.</summary>
    KraidFoot2,
    /// <summary><c>kraid_foot_3</c>, an installed extended frame.</summary>
    KraidFoot3,
    /// <summary><c>kraid_foot_4</c>, an installed extended frame.</summary>
    KraidFoot4,
    /// <summary><c>kraid_foot_5</c>, an installed extended frame.</summary>
    KraidFoot5,
    /// <summary><c>kraid_foot_6</c>, an installed extended frame.</summary>
    KraidFoot6,
    /// <summary><c>kraid_foot_7</c>, an installed extended frame.</summary>
    KraidFoot7,
    /// <summary><c>kraid_foot_8</c>, an installed extended frame.</summary>
    KraidFoot8,
    /// <summary><c>kraid_foot_9</c>, an installed extended frame.</summary>
    KraidFoot9,
    /// <summary><c>kraid_foot_10</c>, an installed extended frame.</summary>
    KraidFoot10,
    /// <summary><c>kraid_foot_11</c>, an installed extended frame.</summary>
    KraidFoot11,
    /// <summary><c>kraid_foot_12</c>, an installed extended frame.</summary>
    KraidFoot12,
    /// <summary><c>kraid_foot_13</c>, an installed extended frame.</summary>
    KraidFoot13,
    /// <summary><c>kraid_foot_14</c>, an installed extended frame.</summary>
    KraidFoot14,
    /// <summary><c>kraid_foot_15</c>, an installed extended frame.</summary>
    KraidFoot15,
    /// <summary><c>kraid_foot_16</c>, an installed extended frame.</summary>
    KraidFoot16,
    /// <summary><c>kraid_foot_17</c>, an installed extended frame.</summary>
    KraidFoot17,
    /// <summary><c>kraid_foot_18</c>, an installed extended frame.</summary>
    KraidFoot18,
    /// <summary><c>kraid_foot_19</c>, an installed extended frame.</summary>
    KraidFoot19,
    /// <summary><c>kraid_foot_20</c>, an installed extended frame.</summary>
    KraidFoot20,
    /// <summary><c>kraid_foot_21</c>, an installed extended frame.</summary>
    KraidFoot21,
    /// <summary><c>kraid_foot_22</c>, an installed extended frame.</summary>
    KraidFoot22,
    /// <summary><c>kraid_foot_23</c>, an installed extended frame.</summary>
    KraidFoot23,
    /// <summary><c>kraid_foot_24</c>, an installed extended frame.</summary>
    KraidFoot24,
    /// <summary><c>kraid_foot_25</c>, an installed extended frame.</summary>
    KraidFoot25,
    /// <summary><c>kraid_foot_26</c>, an installed extended frame.</summary>
    KraidFoot26,
    /// <summary><c>kraid_foot_27</c>, an installed extended frame.</summary>
    KraidFoot27,
    /// <summary><c>kraid_foot_28</c>, an installed extended frame.</summary>
    KraidFoot28,
    /// <summary><c>kraid_foot_29</c>, an installed extended frame.</summary>
    KraidFoot29,
    /// <summary><c>kraid_foot_30</c>, an installed extended frame.</summary>
    KraidFoot30,
    /// <summary><c>kraid_foot_31</c>, an installed extended frame.</summary>
    KraidFoot31,
    /// <summary><c>kraid_foot_32</c>, an installed extended frame.</summary>
    KraidFoot32,
    /// <summary><c>kraid_foot_33</c>, an installed extended frame.</summary>
    KraidFoot33,
    /// <summary><c>kraid_foot_standing</c>, an installed extended frame.</summary>
    KraidFootStanding,
    /// <summary>The empty spritemap at the same offset in every enemy bank: an enemy showing it draws nothing.</summary>
    Empty,
    /// <summary>The visually empty one-component extended frame at the same offset in every enemy bank.</summary>
    EmptyExtended,
}
