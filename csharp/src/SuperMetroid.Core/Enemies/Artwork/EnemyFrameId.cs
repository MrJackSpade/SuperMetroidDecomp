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
    CeresDoorMode7LeftWall,
    /// <summary><c>ceres_door_mode7_right_wall</c>, an installed spritemap.</summary>
    CeresDoorMode7RightWall,
    /// <summary><c>ceres_door_ridley_private_overlay</c>, an installed spritemap.</summary>
    CeresDoorRidleyPrivateOverlay,
    /// <summary><c>ceres_baby_horizontal</c>, an installed spritemap.</summary>
    CeresBabyHorizontal,
    /// <summary><c>ceres_baby_round</c>, an installed spritemap.</summary>
    CeresBabyRound,
    /// <summary><c>ceres_baby_vertical</c>, an installed spritemap.</summary>
    CeresBabyVertical,
    /// <summary><c>rio_bd6c</c>, an installed spritemap.</summary>
    RioPose0,
    /// <summary><c>rio_bd82</c>, an installed spritemap.</summary>
    RioPose1,
    /// <summary><c>rio_bd98</c>, an installed spritemap.</summary>
    RioPose2,
    /// <summary><c>rio_bdae</c>, an installed spritemap.</summary>
    RioPose3,
    /// <summary><c>rio_bdc4</c>, an installed spritemap.</summary>
    RioPose4,
    /// <summary><c>rio_bdda</c>, an installed spritemap.</summary>
    RioPose5,
    /// <summary><c>rio_bdf0</c>, an installed spritemap.</summary>
    RioPose6,
    /// <summary><c>rio_be06</c>, an installed spritemap.</summary>
    RioPose7,
    /// <summary><c>lower_norfair_rio_c8bd</c>, an installed spritemap.</summary>
    LowerNorfairRioPose0,
    /// <summary><c>lower_norfair_rio_c8d3</c>, an installed spritemap.</summary>
    LowerNorfairRioPose1,
    /// <summary><c>lower_norfair_rio_c8e9</c>, an installed spritemap.</summary>
    LowerNorfairRioPose2,
    /// <summary><c>lower_norfair_rio_c8ff</c>, an installed spritemap.</summary>
    LowerNorfairRioPose3,
    /// <summary><c>lower_norfair_rio_c915</c>, an installed spritemap.</summary>
    LowerNorfairRioPose4,
    /// <summary><c>lower_norfair_rio_c92b</c>, an installed spritemap.</summary>
    LowerNorfairRioPose5,
    /// <summary><c>lower_norfair_rio_c941</c>, an installed spritemap.</summary>
    LowerNorfairRioPose6,
    /// <summary><c>lower_norfair_rio_c957</c>, an installed spritemap.</summary>
    LowerNorfairRioPose7,
    /// <summary><c>lower_norfair_rio_c96d</c>, an installed spritemap.</summary>
    LowerNorfairRioPose8,
    /// <summary><c>lower_norfair_rio_c983</c>, an installed spritemap.</summary>
    LowerNorfairRioPose9,
    /// <summary><c>lower_norfair_rio_c999</c>, an installed spritemap.</summary>
    LowerNorfairRioPose10,
    /// <summary><c>lower_norfair_rio_c9af</c>, an installed spritemap.</summary>
    LowerNorfairRioPose11,
    /// <summary><c>lower_norfair_rio_c9c5</c>, an installed spritemap.</summary>
    LowerNorfairRioPose12,
    /// <summary><c>lower_norfair_rio_c9db</c>, an installed spritemap.</summary>
    LowerNorfairRioPose13,
    /// <summary><c>lower_norfair_rio_c9f1</c>, an installed spritemap.</summary>
    LowerNorfairRioPose14,
    /// <summary><c>lower_norfair_rio_ca07</c>, an installed spritemap.</summary>
    LowerNorfairRioPose15,
    /// <summary><c>lower_norfair_rio_ca13</c>, an installed spritemap.</summary>
    LowerNorfairRioPose16,
    /// <summary><c>lower_norfair_rio_ca1f</c>, an installed spritemap.</summary>
    LowerNorfairRioPose17,
    /// <summary><c>norfair_rio_c427</c>, an installed spritemap.</summary>
    NorfairRioPose0,
    /// <summary><c>norfair_rio_c442</c>, an installed spritemap.</summary>
    NorfairRioPose1,
    /// <summary><c>norfair_rio_c45d</c>, an installed spritemap.</summary>
    NorfairRioPose2,
    /// <summary><c>norfair_rio_c493</c>, an installed spritemap.</summary>
    NorfairRioPose3,
    /// <summary><c>norfair_rio_c49f</c>, an installed spritemap.</summary>
    NorfairRioPose4,
    /// <summary><c>norfair_rio_c4ab</c>, an installed spritemap.</summary>
    NorfairRioPose5,
    /// <summary><c>norfair_rio_c4b7</c>, an installed spritemap.</summary>
    NorfairRioPose6,
    /// <summary><c>norfair_rio_c4d2</c>, an installed spritemap.</summary>
    NorfairRioPose7,
    /// <summary><c>norfair_rio_c4ed</c>, an installed spritemap.</summary>
    NorfairRioPose8,
    /// <summary><c>norfair_rio_c508</c>, an installed spritemap.</summary>
    NorfairRioPose9,
    /// <summary><c>norfair_rio_c523</c>, an installed spritemap.</summary>
    NorfairRioPose10,
    /// <summary><c>norfair_rio_c534</c>, an installed spritemap.</summary>
    NorfairRioPose11,
    /// <summary><c>norfair_rio_c54f</c>, an installed spritemap.</summary>
    NorfairRioPose12,
    /// <summary><c>norfair_rio_c56a</c>, an installed spritemap.</summary>
    NorfairRioPose13,
    /// <summary><c>norfair_rio_c585</c>, an installed spritemap.</summary>
    NorfairRioPose14,
    /// <summary><c>norfair_rio_c5a0</c>, an installed spritemap.</summary>
    NorfairRioPose15,
    /// <summary><c>norfair_rio_c5bb</c>, an installed spritemap.</summary>
    NorfairRioPose16,
    /// <summary><c>norfair_rio_c5d6</c>, an installed spritemap.</summary>
    NorfairRioPose17,
    /// <summary><c>norfair_rio_c5e2</c>, an installed spritemap.</summary>
    NorfairRioPose18,
    /// <summary><c>norfair_rio_c5ee</c>, an installed spritemap.</summary>
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
    /// <summary><c>room_sprite_b4_bda6</c>, an installed spritemap.</summary>
    SpriteObjectEmpty,
    /// <summary><c>room_sprite_b4_c630</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject30Frame0,
    /// <summary><c>room_sprite_b4_c637</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject30Frame1,
    /// <summary><c>room_sprite_b4_c63e</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject30Frame2,
    /// <summary><c>room_sprite_b4_c645</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject31Frame0,
    /// <summary><c>room_sprite_b4_c64c</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject31Frame1,
    /// <summary><c>room_sprite_b4_c653</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject31Frame2,
    /// <summary><c>room_sprite_b4_c65a</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject32Frame0,
    /// <summary><c>room_sprite_b4_c661</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject32Frame1,
    /// <summary><c>room_sprite_b4_c668</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject32Frame2,
    /// <summary><c>room_sprite_b4_c66f</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject33Frame0,
    /// <summary><c>room_sprite_b4_c676</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject33Frame1,
    /// <summary><c>room_sprite_b4_c67d</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject33Frame2,
    /// <summary><c>room_sprite_b4_c684</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject34Frame0,
    /// <summary><c>room_sprite_b4_c68b</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject34Frame1,
    /// <summary><c>room_sprite_b4_c692</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject34Frame2,
    /// <summary><c>room_sprite_b4_c699</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject35Frame0,
    /// <summary><c>room_sprite_b4_c6a0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject35Frame1,
    /// <summary><c>room_sprite_b4_c6a7</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject35Frame2,
    /// <summary><c>room_sprite_b4_c6ae</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject36Frame0,
    /// <summary><c>room_sprite_b4_c6b5</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject36Frame1,
    /// <summary><c>room_sprite_b4_c6bc</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject36Frame2,
    /// <summary><c>room_sprite_b4_c6c3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject37Frame0,
    /// <summary><c>room_sprite_b4_c6ca</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject37Frame1,
    /// <summary><c>room_sprite_b4_c6d1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject37Frame2,
    /// <summary><c>room_sprite_b4_c6d8</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles0,
    /// <summary><c>room_sprite_b4_c6ee</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles1,
    /// <summary><c>room_sprite_b4_c704</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles2,
    /// <summary><c>room_sprite_b4_c71a</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles3,
    /// <summary><c>room_sprite_b4_c730</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles4,
    /// <summary><c>room_sprite_b4_c746</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles5,
    /// <summary><c>room_sprite_b4_c75c</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles6,
    /// <summary><c>room_sprite_b4_c772</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles7,
    /// <summary><c>room_sprite_b4_c788</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles8,
    /// <summary><c>room_sprite_b4_c79e</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles9,
    /// <summary><c>room_sprite_b4_c7b4</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles10,
    /// <summary><c>room_sprite_b4_c7ca</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles11,
    /// <summary><c>room_sprite_b4_c7e0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles12,
    /// <summary><c>room_sprite_b4_c7f6</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles13,
    /// <summary><c>room_sprite_b4_c80c</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles14,
    /// <summary><c>room_sprite_b4_c822</c>, an installed spritemap.</summary>
    SpriteObjectUnusedRockParticles15,
    /// <summary><c>room_sprite_b4_c838</c>, an installed spritemap.</summary>
    SpriteObjectBigDustCloud0,
    /// <summary><c>room_sprite_b4_c83f</c>, an installed spritemap.</summary>
    SpriteObjectBigDustCloud1,
    /// <summary><c>room_sprite_b4_c84b</c>, an installed spritemap.</summary>
    SpriteObjectBigDustCloud2,
    /// <summary><c>room_sprite_b4_c85c</c>, an installed spritemap.</summary>
    SpriteObjectBigDustCloud3,
    /// <summary><c>room_sprite_b4_c872</c>, an installed spritemap.</summary>
    SpriteObjectBigDustCloud4,
    /// <summary><c>room_sprite_b4_c888</c>, an installed spritemap.</summary>
    SpriteObjectBigDustCloud5,
    /// <summary><c>room_sprite_b4_c899</c>, an installed spritemap.</summary>
    SpriteObjectBigDustCloud6,
    /// <summary><c>room_sprite_b4_c8a5</c>, an installed spritemap.</summary>
    SpriteObjectBigDustCloud7,
    /// <summary><c>room_sprite_b4_c8ac</c>, an installed spritemap.</summary>
    SpriteObjectUnusedDustCloudBeam0,
    /// <summary><c>room_sprite_b4_c8b3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedDustCloudBeam1,
    /// <summary><c>room_sprite_b4_c8ba</c>, an installed spritemap.</summary>
    SpriteObjectUnusedDustCloudBeam2,
    /// <summary><c>room_sprite_b4_c8c6</c>, an installed spritemap.</summary>
    SpriteObjectUnusedDustCloudBeam3,
    /// <summary><c>room_sprite_b4_c8d2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedDustCloudBeam4,
    /// <summary><c>room_sprite_b4_c8e3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedDustCloudBeam5,
    /// <summary><c>room_sprite_b4_c8f4</c>, an installed spritemap.</summary>
    SpriteObjectUnusedDustCloudBeam6,
    /// <summary><c>room_sprite_b4_c90a</c>, an installed spritemap.</summary>
    SpriteObjectUnusedDustCloudBeam7,
    /// <summary><c>room_sprite_b4_c9a0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSaveStationElectricity0,
    /// <summary><c>room_sprite_b4_c9b6</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSaveStationElectricity1,
    /// <summary><c>room_sprite_b4_c9cc</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSaveStationElectricity2,
    /// <summary><c>room_sprite_b4_c9e2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSaveStationElectricity3,
    /// <summary><c>room_sprite_b4_c9f8</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSaveStationElectricity4,
    /// <summary><c>room_sprite_b4_ca0e</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSaveStationElectricity5,
    /// <summary><c>room_sprite_b4_ca24</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSaveStationElectricity6,
    /// <summary><c>room_sprite_b4_ca3a</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSaveStationElectricity7,
    /// <summary><c>room_sprite_b4_ca50</c>, an installed spritemap.</summary>
    SpriteObjectUnusedVerticalGate0,
    /// <summary><c>room_sprite_b4_ca5c</c>, an installed spritemap.</summary>
    SpriteObjectUnusedVerticalGate1,
    /// <summary><c>room_sprite_b4_ca63</c>, an installed spritemap.</summary>
    SpriteObjectUnusedVerticalGate2,
    /// <summary><c>room_sprite_b4_ca6f</c>, an installed spritemap.</summary>
    SpriteObjectUnusedVerticalGate3,
    /// <summary><c>room_sprite_b4_ca7b</c>, an installed spritemap.</summary>
    SpriteObjectUnusedVerticalGate4,
    /// <summary><c>room_sprite_b4_ca8c</c>, an installed spritemap.</summary>
    SpriteObjectUnusedVerticalGate5,
    /// <summary><c>room_sprite_b4_ca9d</c>, an installed spritemap.</summary>
    SpriteObjectUnusedVerticalGate6,
    /// <summary><c>room_sprite_b4_cab3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedVerticalGate7,
    /// <summary><c>room_sprite_b4_cac9</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBeamCharge0,
    /// <summary><c>room_sprite_b4_cad0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBeamCharge1,
    /// <summary><c>room_sprite_b4_cad7</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBeamCharge2,
    /// <summary><c>room_sprite_b4_cade</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBeamCharge3,
    /// <summary><c>room_sprite_b4_caf4</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowParticle0,
    /// <summary><c>room_sprite_b4_cb05</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowParticle1,
    /// <summary><c>room_sprite_b4_cb16</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowParticle2,
    /// <summary><c>room_sprite_b4_cb27</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowParticle3,
    /// <summary><c>room_sprite_b4_cb38</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowParticle4,
    /// <summary><c>room_sprite_b4_cb49</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowParticle5,
    /// <summary><c>room_sprite_b4_cb5a</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowEnergy0,
    /// <summary><c>room_sprite_b4_cb6b</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowEnergy1,
    /// <summary><c>room_sprite_b4_cb7c</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowEnergy2,
    /// <summary><c>room_sprite_b4_cb8d</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowEnergy3,
    /// <summary><c>room_sprite_b4_cb9e</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowEnergy4,
    /// <summary><c>room_sprite_b4_cbaf</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMotherBrainElbowEnergy5,
    /// <summary><c>room_sprite_b4_cbc0</c>, an installed spritemap.</summary>
    SpriteObjectSmallExplosion0,
    /// <summary><c>room_sprite_b4_cbc7</c>, an installed spritemap.</summary>
    SpriteObjectSmallExplosion1,
    /// <summary><c>room_sprite_b4_cbdd</c>, an installed spritemap.</summary>
    SpriteObjectSmallExplosion2,
    /// <summary><c>room_sprite_b4_cbf3</c>, an installed spritemap.</summary>
    SpriteObjectSmallExplosion3,
    /// <summary><c>room_sprite_b4_cc09</c>, an installed spritemap.</summary>
    SpriteObjectSmallExplosion4,
    /// <summary><c>room_sprite_b4_cc1f</c>, an installed spritemap.</summary>
    SpriteObjectSmallExplosion5,
    /// <summary><c>room_sprite_b4_cc35</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBombExplosion0,
    /// <summary><c>room_sprite_b4_cc4b</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBombExplosion1,
    /// <summary><c>room_sprite_b4_cc61</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBombExplosion2,
    /// <summary><c>room_sprite_b4_cc77</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBombExplosion3,
    /// <summary><c>room_sprite_b4_cc8d</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBombExplosion4,
    /// <summary><c>room_sprite_b4_ccb9</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBeamTrail0,
    /// <summary><c>room_sprite_b4_ccc0</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBeamTrail1,
    /// <summary><c>room_sprite_b4_ccc7</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBeamTrail2,
    /// <summary><c>room_sprite_b4_ccce</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBeamTrail3,
    /// <summary><c>room_sprite_b4_ccd5</c>, an installed spritemap.</summary>
    SpriteObjectUnusedPowerBomb0,
    /// <summary><c>room_sprite_b4_ccdc</c>, an installed spritemap.</summary>
    SpriteObjectUnusedPowerBomb1,
    /// <summary><c>room_sprite_b4_cce3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedPowerBomb2,
    /// <summary><c>room_sprite_b4_ccf1</c>, an installed spritemap.</summary>
    SpriteObjectDudShot0,
    /// <summary><c>room_sprite_b4_ccf8</c>, an installed spritemap.</summary>
    SpriteObjectDudShot1,
    /// <summary><c>room_sprite_b4_ccff</c>, an installed spritemap.</summary>
    SpriteObjectDudShot2,
    /// <summary><c>room_sprite_b4_cd15</c>, an installed spritemap.</summary>
    SpriteObjectDudShot3,
    /// <summary><c>room_sprite_b4_cd2b</c>, an installed spritemap.</summary>
    SpriteObjectDudShot4,
    /// <summary><c>room_sprite_b4_cd41</c>, an installed spritemap.</summary>
    SpriteObjectDudShot5,
    /// <summary><c>room_sprite_b4_cd57</c>, an installed spritemap.</summary>
    SpriteObjectUnusedElevatorPad0,
    /// <summary><c>room_sprite_b4_cd6d</c>, an installed spritemap.</summary>
    SpriteObjectUnusedElevatorPad1,
    /// <summary><c>room_sprite_b4_cd99</c>, an installed spritemap.</summary>
    SpriteObjectSmallDustCloud0,
    /// <summary><c>room_sprite_b4_cda0</c>, an installed spritemap.</summary>
    SpriteObjectSmallDustCloud1,
    /// <summary><c>room_sprite_b4_cda7</c>, an installed spritemap.</summary>
    SpriteObjectSmallDustCloud2,
    /// <summary><c>room_sprite_b4_cdae</c>, an installed spritemap.</summary>
    SpriteObjectSmallDustCloud3,
    /// <summary><c>room_sprite_b4_cdb5</c>, an installed spritemap.</summary>
    SpriteObjectPirateLandingDustCloud0,
    /// <summary><c>room_sprite_b4_cdbc</c>, an installed spritemap.</summary>
    SpriteObjectPirateLandingDustCloud1,
    /// <summary><c>room_sprite_b4_cdc3</c>, an installed spritemap.</summary>
    SpriteObjectPirateLandingDustCloud2,
    /// <summary><c>room_sprite_b4_cdca</c>, an installed spritemap.</summary>
    SpriteObjectPirateLandingDustCloud3,
    /// <summary><c>room_sprite_b4_cdd1</c>, an installed spritemap.</summary>
    SpriteObjectUnusedEyeDoorSweatDrop0,
    /// <summary><c>room_sprite_b4_cdd8</c>, an installed spritemap.</summary>
    SpriteObjectUnusedEyeDoorSweatDrop1,
    /// <summary><c>room_sprite_b4_cddf</c>, an installed spritemap.</summary>
    SpriteObjectUnusedEyeDoorSweatDrop2,
    /// <summary><c>room_sprite_b4_cde6</c>, an installed spritemap.</summary>
    SpriteObjectUnusedEyeDoorSweatDrop3,
    /// <summary><c>room_sprite_b4_cded</c>, an installed spritemap.</summary>
    SpriteObjectSmoke0,
    /// <summary><c>room_sprite_b4_cdf4</c>, an installed spritemap.</summary>
    SpriteObjectSmoke1,
    /// <summary><c>room_sprite_b4_cdfb</c>, an installed spritemap.</summary>
    SpriteObjectSmoke2,
    /// <summary><c>room_sprite_b4_ce02</c>, an installed spritemap.</summary>
    SpriteObjectSmoke3,
    /// <summary><c>room_sprite_b4_ce1e</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSmallEnergyDropObject13Frame0,
    /// <summary><c>room_sprite_b4_ce25</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSmallEnergyDropObject13Frame1,
    /// <summary><c>room_sprite_b4_ce2c</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSmallEnergyDropObject13Frame2,
    /// <summary><c>room_sprite_b4_ce33</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSmallEnergyDropObject13Frame3,
    /// <summary><c>room_sprite_b4_ce3a</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBigEnergyDrop0,
    /// <summary><c>room_sprite_b4_ce50</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBigEnergyDrop1,
    /// <summary><c>room_sprite_b4_ce66</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBigEnergyDrop2,
    /// <summary><c>room_sprite_b4_ce7c</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBigEnergyDrop3,
    /// <summary><c>room_sprite_b4_ce83</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBomb0,
    /// <summary><c>room_sprite_b4_ce8a</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBomb1,
    /// <summary><c>room_sprite_b4_ce91</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBomb2,
    /// <summary><c>room_sprite_b4_ce98</c>, an installed spritemap.</summary>
    SpriteObjectUnusedBomb3,
    /// <summary><c>room_sprite_b4_cf87</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSmallEnergyDropObject16Frame0,
    /// <summary><c>room_sprite_b4_cf8e</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSmallEnergyDropObject16Frame1,
    /// <summary><c>room_sprite_b4_cf95</c>, an installed spritemap.</summary>
    SpriteObjectUnusedSmallEnergyDropObject16Frame2,
    /// <summary><c>room_sprite_b4_cfa3</c>, an installed spritemap.</summary>
    SpriteObjectBigExplosion0,
    /// <summary><c>room_sprite_b4_cfb9</c>, an installed spritemap.</summary>
    SpriteObjectBigExplosion1,
    /// <summary><c>room_sprite_b4_cfcf</c>, an installed spritemap.</summary>
    SpriteObjectBigExplosion2,
    /// <summary><c>room_sprite_b4_cfe5</c>, an installed spritemap.</summary>
    SpriteObjectBigExplosion3,
    /// <summary><c>room_sprite_b4_d023</c>, an installed spritemap.</summary>
    SpriteObjectBigExplosion4,
    /// <summary><c>room_sprite_b4_d04d</c>, an installed spritemap.</summary>
    SpriteObjectBigExplosion5,
    /// <summary><c>room_sprite_b4_d08b</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject38Frame0,
    /// <summary><c>room_sprite_b4_d0ba</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject38Frame1,
    /// <summary><c>room_sprite_b4_d0e9</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject38Frame2,
    /// <summary><c>room_sprite_b4_d118</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject38Frame3,
    /// <summary><c>room_sprite_b4_d147</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject38Frame4,
    /// <summary><c>room_sprite_b4_d176</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject38Frame5,
    /// <summary><c>room_sprite_b4_d1d4</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject39Frame0,
    /// <summary><c>room_sprite_b4_d1ef</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject39Frame1,
    /// <summary><c>room_sprite_b4_d20a</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject39Frame2,
    /// <summary><c>room_sprite_b4_d225</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject39Frame3,
    /// <summary><c>room_sprite_b4_d245</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject39Frame4,
    /// <summary><c>room_sprite_b4_d2ac</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject42,
    /// <summary><c>room_sprite_b4_d2b3</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject40Frame0,
    /// <summary><c>room_sprite_b4_d2e2</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject40Frame1,
    /// <summary><c>room_sprite_b4_d311</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject40Frame2,
    /// <summary><c>room_sprite_b4_d340</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject40Frame3,
    /// <summary><c>room_sprite_b4_d36f</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject40Frame4,
    /// <summary><c>room_sprite_b4_d39e</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject40Frame5,
    /// <summary><c>room_sprite_b4_d3fc</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject41Frame0,
    /// <summary><c>room_sprite_b4_d417</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject41Frame1,
    /// <summary><c>room_sprite_b4_d432</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject41Frame2,
    /// <summary><c>room_sprite_b4_d44d</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject41Frame3,
    /// <summary><c>room_sprite_b4_d46d</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject41Frame4,
    /// <summary><c>room_sprite_b4_d4be</c>, an installed spritemap.</summary>
    SpriteObjectPuromi0,
    /// <summary><c>room_sprite_b4_d4c5</c>, an installed spritemap.</summary>
    SpriteObjectPuromi1,
    /// <summary><c>room_sprite_b4_d4cc</c>, an installed spritemap.</summary>
    SpriteObjectPuromi2,
    /// <summary><c>room_sprite_b4_d4d3</c>, an installed spritemap.</summary>
    SpriteObjectPuromi3,
    /// <summary><c>room_sprite_b4_d4da</c>, an installed spritemap.</summary>
    SpriteObjectPuromi4,
    /// <summary><c>room_sprite_b4_d4e1</c>, an installed spritemap.</summary>
    SpriteObjectPuromi5,
    /// <summary><c>room_sprite_b4_d4e8</c>, an installed spritemap.</summary>
    SpriteObjectPuromi6,
    /// <summary><c>room_sprite_b4_d4ef</c>, an installed spritemap.</summary>
    SpriteObjectPuromi7,
    /// <summary><c>room_sprite_b4_d4f6</c>, an installed spritemap.</summary>
    SpriteObjectPuromiRightExplosion0,
    /// <summary><c>room_sprite_b4_d4fd</c>, an installed spritemap.</summary>
    SpriteObjectPuromiRightExplosion1,
    /// <summary><c>room_sprite_b4_d504</c>, an installed spritemap.</summary>
    SpriteObjectPuromiRightExplosion2,
    /// <summary><c>room_sprite_b4_d50b</c>, an installed spritemap.</summary>
    SpriteObjectPuromiRightExplosion3,
    /// <summary><c>room_sprite_b4_d512</c>, an installed spritemap.</summary>
    SpriteObjectPuromiRightExplosion4,
    /// <summary><c>room_sprite_b4_d519</c>, an installed spritemap.</summary>
    SpriteObjectPuromiRightExplosion5,
    /// <summary><c>room_sprite_b4_d520</c>, an installed spritemap.</summary>
    SpriteObjectPuromiRightExplosion6,
    /// <summary><c>room_sprite_b4_d527</c>, an installed spritemap.</summary>
    SpriteObjectPuromiLeftExplosion0,
    /// <summary><c>room_sprite_b4_d52e</c>, an installed spritemap.</summary>
    SpriteObjectPuromiLeftExplosion1,
    /// <summary><c>room_sprite_b4_d535</c>, an installed spritemap.</summary>
    SpriteObjectPuromiLeftExplosion2,
    /// <summary><c>room_sprite_b4_d53c</c>, an installed spritemap.</summary>
    SpriteObjectPuromiLeftExplosion3,
    /// <summary><c>room_sprite_b4_d543</c>, an installed spritemap.</summary>
    SpriteObjectPuromiLeftExplosion4,
    /// <summary><c>room_sprite_b4_d54a</c>, an installed spritemap.</summary>
    SpriteObjectPuromiLeftExplosion5,
    /// <summary><c>room_sprite_b4_d551</c>, an installed spritemap.</summary>
    SpriteObjectPuromiLeftExplosion6,
    /// <summary><c>room_sprite_b4_d558</c>, an installed spritemap.</summary>
    SpriteObjectPuromiSplash0,
    /// <summary><c>room_sprite_b4_d564</c>, an installed spritemap.</summary>
    SpriteObjectPuromiSplash1,
    /// <summary><c>room_sprite_b4_d570</c>, an installed spritemap.</summary>
    SpriteObjectPuromiSplash2,
    /// <summary><c>room_sprite_b4_d588</c>, an installed spritemap.</summary>
    SpriteObjectPuromiSplash4,
    /// <summary><c>room_sprite_b4_d594</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject47,
    /// <summary><c>room_sprite_b4_d59b</c>, an installed spritemap.</summary>
    SpriteObjectFallingSparkTrail0,
    /// <summary><c>room_sprite_b4_d5a2</c>, an installed spritemap.</summary>
    SpriteObjectFallingSparkTrail1,
    /// <summary><c>room_sprite_b4_d5a9</c>, an installed spritemap.</summary>
    SpriteObjectFallingSparkTrail2,
    /// <summary><c>room_sprite_b4_d5b0</c>, an installed spritemap.</summary>
    SpriteObjectFallingSparkTrail3,
    /// <summary><c>room_sprite_b4_d5b7</c>, an installed spritemap.</summary>
    SpriteObjectMetroidShell0,
    /// <summary><c>room_sprite_b4_d5eb</c>, an installed spritemap.</summary>
    SpriteObjectMetroidShell1,
    /// <summary><c>room_sprite_b4_d61f</c>, an installed spritemap.</summary>
    SpriteObjectMetroidShell2,
    /// <summary><c>room_sprite_b4_d653</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMetroidInsides0,
    /// <summary><c>room_sprite_b4_d67d</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMetroidInsides1,
    /// <summary><c>room_sprite_b4_d69d</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMetroidInsides2,
    /// <summary><c>room_sprite_b4_d6c7</c>, an installed spritemap.</summary>
    SpriteObjectUnusedMetroidInsides3,
    /// <summary><c>room_sprite_b4_d6f1</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity0,
    /// <summary><c>room_sprite_b4_d702</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity1,
    /// <summary><c>room_sprite_b4_d713</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity2,
    /// <summary><c>room_sprite_b4_d724</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity3,
    /// <summary><c>room_sprite_b4_d73a</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity4,
    /// <summary><c>room_sprite_b4_d750</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity5,
    /// <summary><c>room_sprite_b4_d766</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity6,
    /// <summary><c>room_sprite_b4_d777</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity7,
    /// <summary><c>room_sprite_b4_d788</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity8,
    /// <summary><c>room_sprite_b4_d799</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity9,
    /// <summary><c>room_sprite_b4_d7af</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity10,
    /// <summary><c>room_sprite_b4_d7c5</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity11,
    /// <summary><c>room_sprite_b4_d7db</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity12,
    /// <summary><c>room_sprite_b4_d7e7</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity13,
    /// <summary><c>room_sprite_b4_d7ee</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity14,
    /// <summary><c>room_sprite_b4_d7fa</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity15,
    /// <summary><c>room_sprite_b4_d806</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity16,
    /// <summary><c>room_sprite_b4_d812</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity17,
    /// <summary><c>room_sprite_b4_d81e</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity18,
    /// <summary><c>room_sprite_b4_d82a</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity19,
    /// <summary><c>room_sprite_b4_d836</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity20,
    /// <summary><c>room_sprite_b4_d842</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity21,
    /// <summary><c>room_sprite_b4_d849</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity22,
    /// <summary><c>room_sprite_b4_d850</c>, an installed spritemap.</summary>
    SpriteObjectMetroidElectricity23,
    /// <summary><c>room_sprite_b4_d857</c>, an installed spritemap.</summary>
    SpriteObjectEnemyShot0,
    /// <summary><c>room_sprite_b4_d86d</c>, an installed spritemap.</summary>
    SpriteObjectEnemyShot1,
    /// <summary><c>room_sprite_b4_d883</c>, an installed spritemap.</summary>
    SpriteObjectEnemyShot2,
    /// <summary><c>room_sprite_b4_d899</c>, an installed spritemap.</summary>
    SpriteObjectEnemyShot3,
    /// <summary><c>room_sprite_b4_d8af</c>, an installed spritemap.</summary>
    SpriteObjectYappingMawBaseFacingDown,
    /// <summary><c>room_sprite_b4_d8b6</c>, an installed spritemap.</summary>
    SpriteObjectYappingMawBaseFacingUp,
    /// <summary><c>room_sprite_b4_d8bd</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject58Frame0,
    /// <summary><c>room_sprite_b4_d905</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject58Frame1,
    /// <summary><c>room_sprite_b4_d957</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject58Frame2,
    /// <summary><c>room_sprite_b4_d9a9</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject58Frame3,
    /// <summary><c>room_sprite_b4_d9fb</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject58Frame4,
    /// <summary><c>room_sprite_b4_da48</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject58Frame5,
    /// <summary><c>room_sprite_b4_da90</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject58Frame6,
    /// <summary><c>room_sprite_b4_dad8</c>, an installed spritemap.</summary>
    SpriteObjectUnusedObject58Frame7,
    /// <summary><c>room_sprite_b4_dc3f</c>, an installed spritemap.</summary>
    SpriteObjectEvirFacingRight0,
    /// <summary><c>room_sprite_b4_dc7d</c>, an installed spritemap.</summary>
    SpriteObjectEvirFacingRight1,
    /// <summary><c>room_sprite_b4_dcbb</c>, an installed spritemap.</summary>
    SpriteObjectEvirFacingRight2,
    /// <summary><c>room_sprite_b4_dcf9</c>, an installed spritemap.</summary>
    SpriteObjectEvirFacingRight3,
    /// <summary><c>room_sprite_b4_dd51</c>, an installed spritemap.</summary>
    SpriteObjectDraygonFoamingAtTheMouth0,
    /// <summary><c>room_sprite_b4_dd58</c>, an installed spritemap.</summary>
    SpriteObjectDraygonFoamingAtTheMouth1,
    /// <summary><c>room_sprite_b4_dd5f</c>, an installed spritemap.</summary>
    SpriteObjectDraygonFoamingAtTheMouth2,
    /// <summary><c>room_sprite_b4_dd66</c>, an installed spritemap.</summary>
    SpriteObjectDraygonFoamingAtTheMouth3,
    /// <summary><c>room_sprite_b4_dd6d</c>, an installed spritemap.</summary>
    SpriteObjectDraygonFoamingAtTheMouth4,
    /// <summary><c>room_sprite_b4_dd74</c>, an installed spritemap.</summary>
    SpriteObjectDraygonFoamingAtTheMouth5,
    /// <summary><c>room_sprite_b4_dd7b</c>, an installed spritemap.</summary>
    SpriteObjectDraygonFoamingAtTheMouth6,
    /// <summary><c>room_sprite_b4_dd82</c>, an installed spritemap.</summary>
    SpriteObjectDraygonFoamingAtTheMouth7,
    /// <summary><c>yapping_maw_a8_a8ee</c>, an installed spritemap.</summary>
    YappingMawPose0,
    /// <summary><c>yapping_maw_a8_a909</c>, an installed spritemap.</summary>
    YappingMawPose1,
    /// <summary><c>yapping_maw_a8_a91a</c>, an installed spritemap.</summary>
    YappingMawPose2,
    /// <summary><c>yapping_maw_a8_a935</c>, an installed spritemap.</summary>
    YappingMawPose3,
    /// <summary><c>yapping_maw_a8_a946</c>, an installed spritemap.</summary>
    YappingMawPose4,
    /// <summary><c>yapping_maw_a8_a961</c>, an installed spritemap.</summary>
    YappingMawPose5,
    /// <summary><c>yapping_maw_a8_a972</c>, an installed spritemap.</summary>
    YappingMawPose6,
    /// <summary><c>yapping_maw_a8_a98d</c>, an installed spritemap.</summary>
    YappingMawPose7,
    /// <summary><c>yapping_maw_a8_a99e</c>, an installed spritemap.</summary>
    YappingMawPose8,
    /// <summary><c>yapping_maw_a8_a9b9</c>, an installed spritemap.</summary>
    YappingMawPose9,
    /// <summary><c>yapping_maw_a8_a9ca</c>, an installed spritemap.</summary>
    YappingMawPose10,
    /// <summary><c>yapping_maw_a8_a9e5</c>, an installed spritemap.</summary>
    YappingMawPose11,
    /// <summary><c>yapping_maw_a8_a9f6</c>, an installed spritemap.</summary>
    YappingMawPose12,
    /// <summary><c>yapping_maw_a8_aa11</c>, an installed spritemap.</summary>
    YappingMawPose13,
    /// <summary><c>yapping_maw_a8_aa22</c>, an installed spritemap.</summary>
    YappingMawPose14,
    /// <summary><c>yapping_maw_a8_aa3d</c>, an installed spritemap.</summary>
    YappingMawPose15,
    /// <summary><c>yapping_maw_a8_aa4e</c>, an installed spritemap.</summary>
    YappingMawPose16,
    /// <summary><c>yapping_maw_a8_aa69</c>, an installed spritemap.</summary>
    YappingMawPose17,
    /// <summary><c>yapping_maw_a8_aa7a</c>, an installed spritemap.</summary>
    YappingMawPose18,
    /// <summary><c>yapping_maw_a8_aa95</c>, an installed spritemap.</summary>
    YappingMawPose19,
    /// <summary><c>yapping_maw_a8_aaa6</c>, an installed spritemap.</summary>
    YappingMawPose20,
    /// <summary><c>yapping_maw_a8_aac1</c>, an installed spritemap.</summary>
    YappingMawPose21,
    /// <summary><c>yapping_maw_a8_aad2</c>, an installed spritemap.</summary>
    YappingMawPose22,
    /// <summary><c>yapping_maw_a8_aaed</c>, an installed spritemap.</summary>
    YappingMawPose23,
    /// <summary><c>ki_hunter_a8_eb2e</c>, an installed spritemap.</summary>
    KiHunterPose0,
    /// <summary><c>ki_hunter_a8_eb58</c>, an installed spritemap.</summary>
    KiHunterPose1,
    /// <summary><c>ki_hunter_a8_eb82</c>, an installed spritemap.</summary>
    KiHunterPose2,
    /// <summary><c>ki_hunter_a8_ebac</c>, an installed spritemap.</summary>
    KiHunterPose3,
    /// <summary><c>ki_hunter_a8_ebc2</c>, an installed spritemap.</summary>
    KiHunterPose4,
    /// <summary><c>ki_hunter_a8_ebce</c>, an installed spritemap.</summary>
    KiHunterPose5,
    /// <summary><c>ki_hunter_a8_ebf7</c>, an installed spritemap.</summary>
    KiHunterPose6,
    /// <summary><c>ki_hunter_a8_ec21</c>, an installed spritemap.</summary>
    KiHunterPose7,
    /// <summary><c>ki_hunter_a8_ec4b</c>, an installed spritemap.</summary>
    KiHunterPose8,
    /// <summary><c>ki_hunter_a8_ec75</c>, an installed spritemap.</summary>
    KiHunterPose9,
    /// <summary><c>ki_hunter_a8_ec8b</c>, an installed spritemap.</summary>
    KiHunterPose10,
    /// <summary><c>ki_hunter_a8_ec97</c>, an installed spritemap.</summary>
    KiHunterPose11,
    /// <summary><c>ki_hunter_a8_eca8</c>, an installed spritemap.</summary>
    KiHunterPose12,
    /// <summary><c>ki_hunter_a8_ecc0</c>, an installed spritemap.</summary>
    KiHunterPose13,
    /// <summary><c>ki_hunter_a8_ecea</c>, an installed spritemap.</summary>
    KiHunterPose14,
    /// <summary><c>ki_hunter_a8_ed14</c>, an installed spritemap.</summary>
    KiHunterPose15,
    /// <summary><c>ki_hunter_a8_ed3e</c>, an installed spritemap.</summary>
    KiHunterPose16,
    /// <summary><c>ki_hunter_a8_ed68</c>, an installed spritemap.</summary>
    KiHunterPose17,
    /// <summary><c>ki_hunter_a8_ed92</c>, an installed spritemap.</summary>
    KiHunterPose18,
    /// <summary><c>ki_hunter_a8_edbc</c>, an installed spritemap.</summary>
    KiHunterPose19,
    /// <summary><c>ki_hunter_a8_ede6</c>, an installed spritemap.</summary>
    KiHunterPose20,
    /// <summary><c>ki_hunter_a8_ee10</c>, an installed spritemap.</summary>
    KiHunterPose21,
    /// <summary><c>ki_hunter_a8_ee3a</c>, an installed spritemap.</summary>
    KiHunterPose22,
    /// <summary><c>ki_hunter_a8_ee64</c>, an installed spritemap.</summary>
    KiHunterPose23,
    /// <summary><c>ki_hunter_a8_ee8e</c>, an installed spritemap.</summary>
    KiHunterPose24,
    /// <summary><c>ki_hunter_a8_eeb8</c>, an installed spritemap.</summary>
    KiHunterPose25,
    /// <summary><c>ki_hunter_a8_eee2</c>, an installed spritemap.</summary>
    KiHunterPose26,
    /// <summary><c>ki_hunter_a8_ef0c</c>, an installed spritemap.</summary>
    KiHunterPose27,
    /// <summary><c>ki_hunter_a8_ef36</c>, an installed spritemap.</summary>
    KiHunterPose28,
    /// <summary><c>ki_hunter_a8_ef60</c>, an installed spritemap.</summary>
    KiHunterPose29,
    /// <summary><c>ki_hunter_a8_ef8a</c>, an installed spritemap.</summary>
    KiHunterPose30,
    /// <summary><c>ki_hunter_a8_efb4</c>, an installed spritemap.</summary>
    KiHunterPose31,
    /// <summary><c>ki_hunter_a8_efde</c>, an installed spritemap.</summary>
    KiHunterPose32,
    /// <summary><c>ki_hunter_a8_f00d</c>, an installed spritemap.</summary>
    KiHunterPose33,
    /// <summary><c>ki_hunter_a8_f03c</c>, an installed spritemap.</summary>
    KiHunterPose34,
    /// <summary><c>ki_hunter_a8_f06b</c>, an installed spritemap.</summary>
    KiHunterPose35,
    /// <summary><c>ki_hunter_a8_f09a</c>, an installed spritemap.</summary>
    KiHunterPose36,
    /// <summary><c>ki_hunter_a8_f0c4</c>, an installed spritemap.</summary>
    KiHunterPose37,
    /// <summary><c>ki_hunter_a8_f0f3</c>, an installed spritemap.</summary>
    KiHunterPose38,
    /// <summary><c>ki_hunter_a8_f122</c>, an installed spritemap.</summary>
    KiHunterPose39,
    /// <summary><c>ki_hunter_a8_f151</c>, an installed spritemap.</summary>
    KiHunterPose40,
    /// <summary><c>mother_brain_a9_a586</c>, an installed spritemap.</summary>
    MotherBrainHead0,
    /// <summary><c>mother_brain_a9_a5bf</c>, an installed spritemap.</summary>
    MotherBrainHead1,
    /// <summary><c>mother_brain_a9_a5f8</c>, an installed spritemap.</summary>
    MotherBrainHead2,
    /// <summary><c>mother_brain_a9_a62c</c>, an installed spritemap.</summary>
    MotherBrainHead3,
    /// <summary><c>mother_brain_a9_a660</c>, an installed spritemap.</summary>
    MotherBrainHead4,
    /// <summary><c>mother_brain_a9_a694</c>, an installed spritemap.</summary>
    MotherBrainNeckJoint,
    /// <summary><c>mother_brain_a9_a69b</c>, an installed spritemap.</summary>
    MotherBrainHead6,
    /// <summary><c>mother_brain_a9_a6d9</c>, an installed spritemap.</summary>
    MotherBrainHead7,
    /// <summary><c>mother_brain_a9_a717</c>, an installed spritemap.</summary>
    MotherBrainHead8,
    /// <summary><c>mother_brain_a9_a750</c>, an installed spritemap.</summary>
    MotherBrainHead9,
    /// <summary><c>mother_brain_a9_a789</c>, an installed spritemap.</summary>
    MotherBrainHead10,
    /// <summary><c>mother_brain_a9_ad3e</c>, an installed spritemap.</summary>
    MotherBrainHead11,
    /// <summary><c>mother_brain_a9_ad6d</c>, an installed spritemap.</summary>
    MotherBrainHead12,
    /// <summary><c>mother_brain_a9_ada1</c>, an installed spritemap.</summary>
    MotherBrainTube0,
    /// <summary><c>mother_brain_a9_add5</c>, an installed spritemap.</summary>
    MotherBrainTube1,
    /// <summary><c>mother_brain_a9_ae09</c>, an installed spritemap.</summary>
    MotherBrainTube2,
    /// <summary><c>mother_brain_a9_ae33</c>, an installed spritemap.</summary>
    MotherBrainTube3,
    /// <summary><c>mother_brain_a9_ae5d</c>, an installed spritemap.</summary>
    MotherBrainTube4,
    /// <summary><c>dead_torizo_corpse_a9_d761</c>, an installed spritemap.</summary>
    DeadTorizoCorpseHook,
    /// <summary><c>ridley_supplement_a6_dc90</c>, an installed spritemap.</summary>
    RidleyTailSegmentSmall,
    /// <summary><c>ridley_supplement_a6_dc97</c>, an installed spritemap.</summary>
    RidleyTailSegmentMedium,
    /// <summary><c>ridley_supplement_a6_dc9e</c>, an installed spritemap.</summary>
    RidleyTailSegmentLarge,
    /// <summary><c>ridley_supplement_a6_dcda</c>, an installed spritemap.</summary>
    RidleyTailTip12,
    /// <summary><c>ridley_supplement_a6_dce1</c>, an installed spritemap.</summary>
    RidleyTailTip11,
    /// <summary><c>ridley_supplement_a6_dce8</c>, an installed spritemap.</summary>
    RidleyTailTip10,
    /// <summary><c>ridley_supplement_a6_dcef</c>, an installed spritemap.</summary>
    RidleyTailTip9,
    /// <summary><c>ridley_supplement_a6_dcf6</c>, an installed spritemap.</summary>
    RidleyTailTip8,
    /// <summary><c>ridley_supplement_a6_dcfd</c>, an installed spritemap.</summary>
    RidleyTailTip7,
    /// <summary><c>ridley_supplement_a6_dd04</c>, an installed spritemap.</summary>
    RidleyTailTip6,
    /// <summary><c>ridley_supplement_a6_dd0b</c>, an installed spritemap.</summary>
    RidleyTailTip5,
    /// <summary><c>ridley_supplement_a6_dd12</c>, an installed spritemap.</summary>
    RidleyTailTip4,
    /// <summary><c>ridley_supplement_a6_dd19</c>, an installed spritemap.</summary>
    RidleyTailTip3,
    /// <summary><c>ridley_supplement_a6_dd20</c>, an installed spritemap.</summary>
    RidleyTailTip2,
    /// <summary><c>ridley_supplement_a6_dd27</c>, an installed spritemap.</summary>
    RidleyTailTip1,
    /// <summary><c>ridley_supplement_a6_dd2e</c>, an installed spritemap.</summary>
    RidleyTailTip0,
    /// <summary><c>ridley_supplement_a6_dd35</c>, an installed spritemap.</summary>
    RidleyTailTip15,
    /// <summary><c>ridley_supplement_a6_dd3c</c>, an installed spritemap.</summary>
    RidleyTailTip14,
    /// <summary><c>ridley_supplement_a6_dd43</c>, an installed spritemap.</summary>
    RidleyTailTip13,
    /// <summary><c>ridley_supplement_a6_dd4a</c>, an installed spritemap.</summary>
    RidleyWingLeft0,
    /// <summary><c>ridley_supplement_a6_dd6a</c>, an installed spritemap.</summary>
    RidleyWingLeft1,
    /// <summary><c>ridley_supplement_a6_dd85</c>, an installed spritemap.</summary>
    RidleyWingLeft2,
    /// <summary><c>ridley_supplement_a6_dd96</c>, an installed spritemap.</summary>
    RidleyWingLeft3,
    /// <summary><c>ridley_supplement_a6_dda7</c>, an installed spritemap.</summary>
    RidleyWingLeft4,
    /// <summary><c>ridley_supplement_a6_ddc2</c>, an installed spritemap.</summary>
    RidleyWingLeft5,
    /// <summary><c>ridley_supplement_a6_dde2</c>, an installed spritemap.</summary>
    RidleyWingRight0,
    /// <summary><c>ridley_supplement_a6_de02</c>, an installed spritemap.</summary>
    RidleyWingRight1,
    /// <summary><c>ridley_supplement_a6_de1d</c>, an installed spritemap.</summary>
    RidleyWingRight2,
    /// <summary><c>ridley_supplement_a6_de2e</c>, an installed spritemap.</summary>
    RidleyWingRight3,
    /// <summary><c>ridley_supplement_a6_de3f</c>, an installed spritemap.</summary>
    RidleyWingRight4,
    /// <summary><c>ridley_supplement_a6_de5a</c>, an installed spritemap.</summary>
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
    /// <summary><c>norfair_lava_jumper_c02c</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose1,
    /// <summary><c>norfair_lava_jumper_c042</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose2,
    /// <summary><c>norfair_lava_jumper_c058</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose3,
    /// <summary><c>norfair_lava_jumper_c064</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose4,
    /// <summary><c>norfair_lava_jumper_c070</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose5,
    /// <summary><c>norfair_lava_jumper_c086</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose6,
    /// <summary><c>norfair_lava_jumper_c09c</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose7,
    /// <summary><c>norfair_lava_jumper_c0ad</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose8,
    /// <summary><c>norfair_lava_jumper_804d</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose0,
    /// <summary><c>norfair_lava_jumper_c0c5</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose10,
    /// <summary><c>norfair_lava_jumper_c0b9</c>, an installed spritemap.</summary>
    NorfairLavaJumperPose9,
    /// <summary><c>chozo_statue_aa_efd8</c>, an installed spritemap.</summary>
    ChozoStatuePose17,
    /// <summary><c>chozo_statue_aa_f034</c>, an installed spritemap.</summary>
    ChozoStatuePose18,
    /// <summary><c>chozo_statue_aa_f08b</c>, an installed spritemap.</summary>
    ChozoStatuePose19,
    /// <summary><c>chozo_statue_aa_f0e2</c>, an installed spritemap.</summary>
    ChozoStatuePose20,
    /// <summary><c>chozo_statue_aa_f4b4</c>, an installed spritemap.</summary>
    ChozoStatuePose21,
    /// <summary><c>chozo_statue_aa_f510</c>, an installed spritemap.</summary>
    ChozoStatuePose22,
    /// <summary><c>chozo_statue_aa_f56c</c>, an installed spritemap.</summary>
    ChozoStatuePose23,
    /// <summary><c>chozo_statue_aa_f5c8</c>, an installed spritemap.</summary>
    ChozoStatuePose24,
    /// <summary><c>chozo_statue_aa_f624</c>, an installed spritemap.</summary>
    ChozoStatuePose25,
    /// <summary><c>chozo_statue_aa_e7dd</c>, an installed spritemap.</summary>
    ChozoStatuePose0,
    /// <summary><c>chozo_statue_aa_e839</c>, an installed spritemap.</summary>
    ChozoStatuePose1,
    /// <summary><c>chozo_statue_aa_e890</c>, an installed spritemap.</summary>
    ChozoStatuePose2,
    /// <summary><c>chozo_statue_aa_e8e7</c>, an installed spritemap.</summary>
    ChozoStatuePose3,
    /// <summary><c>chozo_statue_aa_ecb9</c>, an installed spritemap.</summary>
    ChozoStatuePose12,
    /// <summary><c>chozo_statue_aa_ed15</c>, an installed spritemap.</summary>
    ChozoStatuePose13,
    /// <summary><c>chozo_statue_aa_ed71</c>, an installed spritemap.</summary>
    ChozoStatuePose14,
    /// <summary><c>chozo_statue_aa_edcd</c>, an installed spritemap.</summary>
    ChozoStatuePose15,
    /// <summary><c>chozo_statue_aa_ee29</c>, an installed spritemap.</summary>
    ChozoStatuePose16,
    /// <summary><c>chozo_statue_aa_ec49</c>, an installed spritemap.</summary>
    ChozoStatuePose11,
    /// <summary><c>chozo_statue_aa_e943</c>, an installed spritemap.</summary>
    ChozoStatuePose4,
    /// <summary><c>chozo_statue_aa_e9ae</c>, an installed spritemap.</summary>
    ChozoStatuePose5,
    /// <summary><c>chozo_statue_aa_ea1e</c>, an installed spritemap.</summary>
    ChozoStatuePose6,
    /// <summary><c>chozo_statue_aa_ea8e</c>, an installed spritemap.</summary>
    ChozoStatuePose7,
    /// <summary><c>chozo_statue_aa_eafe</c>, an installed spritemap.</summary>
    ChozoStatuePose8,
    /// <summary><c>chozo_statue_aa_eb69</c>, an installed spritemap.</summary>
    ChozoStatuePose9,
    /// <summary><c>chozo_statue_aa_ebd9</c>, an installed spritemap.</summary>
    ChozoStatuePose10,
    /// <summary><c>viola_spin_b689</c>, an installed spritemap.</summary>
    ViolaSpin0,
    /// <summary><c>viola_spin_b690</c>, an installed spritemap.</summary>
    ViolaSpin1,
    /// <summary><c>viola_spin_b697</c>, an installed spritemap.</summary>
    ViolaSpin2,
    /// <summary><c>viola_spin_b69e</c>, an installed spritemap.</summary>
    ViolaSpin3,
    /// <summary><c>viola_spin_b6a5</c>, an installed spritemap.</summary>
    ViolaSpin4,
    /// <summary><c>viola_spin_b6ac</c>, an installed spritemap.</summary>
    ViolaSpin5,
    /// <summary><c>viola_spin_b6b3</c>, an installed spritemap.</summary>
    ViolaSpin6,
    /// <summary><c>viola_spin_b6ba</c>, an installed spritemap.</summary>
    ViolaSpin7,
    /// <summary><c>rinka_spin_ba38</c>, an installed spritemap.</summary>
    RinkaSpin0,
    /// <summary><c>rinka_spin_ba3f</c>, an installed spritemap.</summary>
    RinkaSpin1,
    /// <summary><c>rinka_spin_ba46</c>, an installed spritemap.</summary>
    RinkaSpin2,
    /// <summary><c>rinka_spin_ba4d</c>, an installed spritemap.</summary>
    RinkaSpin3,
    /// <summary><c>rinka_spin_ba54</c>, an installed spritemap.</summary>
    RinkaSpin4,
    /// <summary><c>dead_torizo_stationary_a9_d6e2</c>, an installed spritemap.</summary>
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
    /// <summary><c>ninja_pirate_8A54</c>, an installed extended frame.</summary>
    NinjaPiratePose0,
    /// <summary><c>ninja_pirate_8A66</c>, an installed extended frame.</summary>
    NinjaPiratePose1,
    /// <summary><c>ninja_pirate_8A78</c>, an installed extended frame.</summary>
    NinjaPiratePose2,
    /// <summary><c>ninja_pirate_8A8A</c>, an installed extended frame.</summary>
    NinjaPiratePose3,
    /// <summary><c>ninja_pirate_8A9C</c>, an installed extended frame.</summary>
    NinjaPiratePose4,
    /// <summary><c>ninja_pirate_8AAE</c>, an installed extended frame.</summary>
    NinjaPiratePose5,
    /// <summary><c>ninja_pirate_8AC0</c>, an installed extended frame.</summary>
    NinjaPiratePose6,
    /// <summary><c>ninja_pirate_8AD2</c>, an installed extended frame.</summary>
    NinjaPiratePose7,
    /// <summary><c>ninja_pirate_8AE4</c>, an installed extended frame.</summary>
    NinjaPiratePose8,
    /// <summary><c>ninja_pirate_8D96</c>, an installed extended frame.</summary>
    NinjaPiratePose17,
    /// <summary><c>ninja_pirate_8DA0</c>, an installed extended frame.</summary>
    NinjaPiratePose18,
    /// <summary><c>ninja_pirate_8DAA</c>, an installed extended frame.</summary>
    NinjaPiratePose19,
    /// <summary><c>ninja_pirate_8DB4</c>, an installed extended frame.</summary>
    NinjaPiratePose20,
    /// <summary><c>ninja_pirate_8DBE</c>, an installed extended frame.</summary>
    NinjaPiratePose21,
    /// <summary><c>ninja_pirate_8DC8</c>, an installed extended frame.</summary>
    NinjaPiratePose22,
    /// <summary><c>ninja_pirate_8DD2</c>, an installed extended frame.</summary>
    NinjaPiratePose23,
    /// <summary><c>ninja_pirate_8DDC</c>, an installed extended frame.</summary>
    NinjaPiratePose24,
    /// <summary><c>ninja_pirate_8F56</c>, an installed extended frame.</summary>
    NinjaPiratePose49,
    /// <summary><c>ninja_pirate_8F60</c>, an installed extended frame.</summary>
    NinjaPiratePose50,
    /// <summary><c>ninja_pirate_8F6A</c>, an installed extended frame.</summary>
    NinjaPiratePose51,
    /// <summary><c>ninja_pirate_8F9C</c>, an installed extended frame.</summary>
    NinjaPiratePose55,
    /// <summary><c>ninja_pirate_90FE</c>, an installed extended frame.</summary>
    NinjaPiratePose65,
    /// <summary><c>ninja_pirate_900A</c>, an installed extended frame.</summary>
    NinjaPiratePose57,
    /// <summary><c>ninja_pirate_9014</c>, an installed extended frame.</summary>
    NinjaPiratePose58,
    /// <summary><c>ninja_pirate_8E36</c>, an installed extended frame.</summary>
    NinjaPiratePose33,
    /// <summary><c>ninja_pirate_8E48</c>, an installed extended frame.</summary>
    NinjaPiratePose34,
    /// <summary><c>ninja_pirate_8E5A</c>, an installed extended frame.</summary>
    NinjaPiratePose35,
    /// <summary><c>ninja_pirate_8E6C</c>, an installed extended frame.</summary>
    NinjaPiratePose36,
    /// <summary><c>ninja_pirate_8E7E</c>, an installed extended frame.</summary>
    NinjaPiratePose37,
    /// <summary><c>ninja_pirate_8E90</c>, an installed extended frame.</summary>
    NinjaPiratePose38,
    /// <summary><c>ninja_pirate_8EA2</c>, an installed extended frame.</summary>
    NinjaPiratePose39,
    /// <summary><c>ninja_pirate_8EB4</c>, an installed extended frame.</summary>
    NinjaPiratePose40,
    /// <summary><c>ninja_pirate_93EA</c>, an installed extended frame.</summary>
    NinjaPiratePose69,
    /// <summary><c>ninja_pirate_9404</c>, an installed extended frame.</summary>
    NinjaPiratePose70,
    /// <summary><c>ninja_pirate_9416</c>, an installed extended frame.</summary>
    NinjaPiratePose71,
    /// <summary><c>ninja_pirate_9476</c>, an installed extended frame.</summary>
    NinjaPiratePose75,
    /// <summary><c>ninja_pirate_944A</c>, an installed extended frame.</summary>
    NinjaPiratePose73,
    /// <summary><c>ninja_pirate_8F74</c>, an installed extended frame.</summary>
    NinjaPiratePose52,
    /// <summary><c>ninja_pirate_8F7E</c>, an installed extended frame.</summary>
    NinjaPiratePose53,
    /// <summary><c>ninja_pirate_8F88</c>, an installed extended frame.</summary>
    NinjaPiratePose54,
    /// <summary><c>ninja_pirate_9032</c>, an installed extended frame.</summary>
    NinjaPiratePose61,
    /// <summary><c>ninja_pirate_9372</c>, an installed extended frame.</summary>
    NinjaPiratePose67,
    /// <summary><c>ninja_pirate_903C</c>, an installed extended frame.</summary>
    NinjaPiratePose62,
    /// <summary><c>ninja_pirate_8B86</c>, an installed extended frame.</summary>
    NinjaPiratePose9,
    /// <summary><c>ninja_pirate_8B98</c>, an installed extended frame.</summary>
    NinjaPiratePose10,
    /// <summary><c>ninja_pirate_8BAA</c>, an installed extended frame.</summary>
    NinjaPiratePose11,
    /// <summary><c>ninja_pirate_8BBC</c>, an installed extended frame.</summary>
    NinjaPiratePose12,
    /// <summary><c>ninja_pirate_8BCE</c>, an installed extended frame.</summary>
    NinjaPiratePose13,
    /// <summary><c>ninja_pirate_8BE0</c>, an installed extended frame.</summary>
    NinjaPiratePose14,
    /// <summary><c>ninja_pirate_8BF2</c>, an installed extended frame.</summary>
    NinjaPiratePose15,
    /// <summary><c>ninja_pirate_8C16</c>, an installed extended frame.</summary>
    NinjaPiratePose16,
    /// <summary><c>ninja_pirate_8DE6</c>, an installed extended frame.</summary>
    NinjaPiratePose25,
    /// <summary><c>ninja_pirate_8DF0</c>, an installed extended frame.</summary>
    NinjaPiratePose26,
    /// <summary><c>ninja_pirate_8DFA</c>, an installed extended frame.</summary>
    NinjaPiratePose27,
    /// <summary><c>ninja_pirate_8E04</c>, an installed extended frame.</summary>
    NinjaPiratePose28,
    /// <summary><c>ninja_pirate_8E0E</c>, an installed extended frame.</summary>
    NinjaPiratePose29,
    /// <summary><c>ninja_pirate_8E18</c>, an installed extended frame.</summary>
    NinjaPiratePose30,
    /// <summary><c>ninja_pirate_8E22</c>, an installed extended frame.</summary>
    NinjaPiratePose31,
    /// <summary><c>ninja_pirate_8E2C</c>, an installed extended frame.</summary>
    NinjaPiratePose32,
    /// <summary><c>ninja_pirate_8FB0</c>, an installed extended frame.</summary>
    NinjaPiratePose56,
    /// <summary><c>ninja_pirate_9280</c>, an installed extended frame.</summary>
    NinjaPiratePose66,
    /// <summary><c>ninja_pirate_901E</c>, an installed extended frame.</summary>
    NinjaPiratePose59,
    /// <summary><c>ninja_pirate_9028</c>, an installed extended frame.</summary>
    NinjaPiratePose60,
    /// <summary><c>ninja_pirate_8EC6</c>, an installed extended frame.</summary>
    NinjaPiratePose41,
    /// <summary><c>ninja_pirate_8ED8</c>, an installed extended frame.</summary>
    NinjaPiratePose42,
    /// <summary><c>ninja_pirate_8EEA</c>, an installed extended frame.</summary>
    NinjaPiratePose43,
    /// <summary><c>ninja_pirate_8EFC</c>, an installed extended frame.</summary>
    NinjaPiratePose44,
    /// <summary><c>ninja_pirate_8F0E</c>, an installed extended frame.</summary>
    NinjaPiratePose45,
    /// <summary><c>ninja_pirate_8F20</c>, an installed extended frame.</summary>
    NinjaPiratePose46,
    /// <summary><c>ninja_pirate_8F32</c>, an installed extended frame.</summary>
    NinjaPiratePose47,
    /// <summary><c>ninja_pirate_8F44</c>, an installed extended frame.</summary>
    NinjaPiratePose48,
    /// <summary><c>ninja_pirate_9430</c>, an installed extended frame.</summary>
    NinjaPiratePose72,
    /// <summary><c>ninja_pirate_945C</c>, an installed extended frame.</summary>
    NinjaPiratePose74,
    /// <summary><c>ninja_pirate_9046</c>, an installed extended frame.</summary>
    NinjaPiratePose63,
    /// <summary><c>ninja_pirate_937C</c>, an installed extended frame.</summary>
    NinjaPiratePose68,
    /// <summary><c>ninja_pirate_9050</c>, an installed extended frame.</summary>
    NinjaPiratePose64,
    /// <summary><c>ridley_body_E983</c>, an installed extended frame.</summary>
    RidleyBodyLeft,
    /// <summary><c>ridley_body_E9A5</c>, an installed extended frame.</summary>
    RidleyBodyRight,
    /// <summary><c>ridley_body_E9C7</c>, an installed extended frame.</summary>
    RidleyBodyLeftMouthHalfOpen,
    /// <summary><c>ridley_body_E9E9</c>, an installed extended frame.</summary>
    RidleyBodyLeftMouthOpen,
    /// <summary><c>ridley_body_EA0B</c>, an installed extended frame.</summary>
    RidleyBodyRightMouthHalfOpen,
    /// <summary><c>ridley_body_EA2D</c>, an installed extended frame.</summary>
    RidleyBodyRightMouthOpen,
    /// <summary><c>ridley_body_EA4F</c>, an installed extended frame.</summary>
    RidleyBodyLeftLegsHalfExtended,
    /// <summary><c>ridley_body_EA71</c>, an installed extended frame.</summary>
    RidleyBodyLeftLegsExtended,
    /// <summary><c>ridley_body_EA93</c>, an installed extended frame.</summary>
    RidleyBodyRightLegsHalfExtended,
    /// <summary><c>ridley_body_EAB5</c>, an installed extended frame.</summary>
    RidleyBodyRightLegsExtended,
    /// <summary><c>ridley_body_EAD7</c>, an installed extended frame.</summary>
    RidleyBodyForward,
    /// <summary><c>draygon_oam_A2DF</c>, an installed extended frame.</summary>
    DraygonFrame0,
    /// <summary><c>draygon_oam_A2E9</c>, an installed extended frame.</summary>
    DraygonFrame1,
    /// <summary><c>draygon_oam_A2F3</c>, an installed extended frame.</summary>
    DraygonFrame2,
    /// <summary><c>draygon_oam_A2FD</c>, an installed extended frame.</summary>
    DraygonFrame3,
    /// <summary><c>draygon_oam_A307</c>, an installed extended frame.</summary>
    DraygonFrame4,
    /// <summary><c>draygon_oam_A311</c>, an installed extended frame.</summary>
    DraygonFrame5,
    /// <summary><c>draygon_oam_A3C5</c>, an installed extended frame.</summary>
    DraygonFrame23,
    /// <summary><c>draygon_oam_A3CF</c>, an installed extended frame.</summary>
    DraygonFrame24,
    /// <summary><c>draygon_oam_A3D9</c>, an installed extended frame.</summary>
    DraygonFrame25,
    /// <summary><c>draygon_oam_A3E3</c>, an installed extended frame.</summary>
    DraygonFrame26,
    /// <summary><c>draygon_oam_A40B</c>, an installed extended frame.</summary>
    DraygonFrame27,
    /// <summary><c>draygon_oam_A41D</c>, an installed extended frame.</summary>
    DraygonFrame28,
    /// <summary><c>draygon_oam_A42F</c>, an installed extended frame.</summary>
    DraygonFrame29,
    /// <summary><c>draygon_oam_A441</c>, an installed extended frame.</summary>
    DraygonFrame30,
    /// <summary><c>draygon_oam_A453</c>, an installed extended frame.</summary>
    DraygonFrame31,
    /// <summary><c>draygon_oam_A465</c>, an installed extended frame.</summary>
    DraygonFrame32,
    /// <summary><c>draygon_oam_A477</c>, an installed extended frame.</summary>
    DraygonFrame33,
    /// <summary><c>draygon_oam_A489</c>, an installed extended frame.</summary>
    DraygonFrame34,
    /// <summary><c>draygon_oam_A4A3</c>, an installed extended frame.</summary>
    DraygonFrame35,
    /// <summary><c>draygon_oam_A4C5</c>, an installed extended frame.</summary>
    DraygonFrame36,
    /// <summary><c>draygon_oam_A4EF</c>, an installed extended frame.</summary>
    DraygonFrame37,
    /// <summary><c>draygon_oam_A521</c>, an installed extended frame.</summary>
    DraygonFrame38,
    /// <summary><c>draygon_oam_A55B</c>, an installed extended frame.</summary>
    DraygonFrame39,
    /// <summary><c>draygon_oam_A59D</c>, an installed extended frame.</summary>
    DraygonFrame40,
    /// <summary><c>draygon_oam_A607</c>, an installed extended frame.</summary>
    DraygonFrame41,
    /// <summary><c>draygon_oam_A611</c>, an installed extended frame.</summary>
    DraygonFrame42,
    /// <summary><c>draygon_oam_A61B</c>, an installed extended frame.</summary>
    DraygonFrame43,
    /// <summary><c>draygon_oam_A625</c>, an installed extended frame.</summary>
    DraygonFrame44,
    /// <summary><c>draygon_oam_A62F</c>, an installed extended frame.</summary>
    DraygonFrame45,
    /// <summary><c>draygon_oam_A639</c>, an installed extended frame.</summary>
    DraygonFrame46,
    /// <summary><c>draygon_oam_A6ED</c>, an installed extended frame.</summary>
    DraygonFrame64,
    /// <summary><c>draygon_oam_A6F7</c>, an installed extended frame.</summary>
    DraygonFrame65,
    /// <summary><c>draygon_oam_A701</c>, an installed extended frame.</summary>
    DraygonFrame66,
    /// <summary><c>draygon_oam_A70B</c>, an installed extended frame.</summary>
    DraygonFrame67,
    /// <summary><c>draygon_oam_A779</c>, an installed extended frame.</summary>
    DraygonFrame68,
    /// <summary><c>draygon_oam_A78B</c>, an installed extended frame.</summary>
    DraygonFrame69,
    /// <summary><c>draygon_oam_A79D</c>, an installed extended frame.</summary>
    DraygonFrame70,
    /// <summary><c>draygon_oam_A7AF</c>, an installed extended frame.</summary>
    DraygonFrame71,
    /// <summary><c>draygon_oam_A7C1</c>, an installed extended frame.</summary>
    DraygonFrame72,
    /// <summary><c>draygon_oam_A7D3</c>, an installed extended frame.</summary>
    DraygonFrame73,
    /// <summary><c>draygon_oam_A7E5</c>, an installed extended frame.</summary>
    DraygonFrame74,
    /// <summary><c>draygon_oam_A7F7</c>, an installed extended frame.</summary>
    DraygonFrame75,
    /// <summary><c>draygon_oam_A811</c>, an installed extended frame.</summary>
    DraygonFrame76,
    /// <summary><c>draygon_oam_A833</c>, an installed extended frame.</summary>
    DraygonFrame77,
    /// <summary><c>draygon_oam_A85D</c>, an installed extended frame.</summary>
    DraygonFrame78,
    /// <summary><c>draygon_oam_A88F</c>, an installed extended frame.</summary>
    DraygonFrame79,
    /// <summary><c>draygon_oam_A8C9</c>, an installed extended frame.</summary>
    DraygonFrame80,
    /// <summary><c>draygon_oam_A90B</c>, an installed extended frame.</summary>
    DraygonFrame81,
    /// <summary><c>spore_spawn_oam_EE65</c>, an installed extended frame.</summary>
    SporeSpawnDead0,
    /// <summary><c>spore_spawn_oam_EE6F</c>, an installed extended frame.</summary>
    SporeSpawnClosed0,
    /// <summary><c>spore_spawn_oam_EE79</c>, an installed extended frame.</summary>
    SporeSpawnOpening0,
    /// <summary><c>spore_spawn_oam_EE8B</c>, an installed extended frame.</summary>
    SporeSpawnOpening1,
    /// <summary><c>spore_spawn_oam_EE9D</c>, an installed extended frame.</summary>
    SporeSpawnOpening2,
    /// <summary><c>spore_spawn_oam_EEAF</c>, an installed extended frame.</summary>
    SporeSpawnOpening3,
    /// <summary><c>spore_spawn_oam_EEC1</c>, an installed extended frame.</summary>
    SporeSpawnOpening4,
    /// <summary><c>spore_spawn_oam_EED3</c>, an installed extended frame.</summary>
    SporeSpawnOpening5,
    /// <summary><c>spore_spawn_oam_EEE5</c>, an installed extended frame.</summary>
    SporeSpawnOpening6,
    /// <summary><c>spore_spawn_oam_EF3D</c>, an installed extended frame.</summary>
    SporeSpawnOpen0,
    /// <summary><c>spore_spawn_oam_EF4F</c>, an installed extended frame.</summary>
    SporeSpawnOpen1,
    /// <summary><c>spore_spawn_oam_EF61</c>, an installed extended frame.</summary>
    SporeSpawnOpen2,
    /// <summary><c>ceres_steam_oam_F142</c>, an installed extended frame.</summary>
    CeresSteamPose0,
    /// <summary><c>ceres_steam_oam_F14C</c>, an installed extended frame.</summary>
    CeresSteamPose1,
    /// <summary><c>ceres_steam_oam_F156</c>, an installed extended frame.</summary>
    CeresSteamPose2,
    /// <summary><c>ceres_steam_oam_F160</c>, an installed extended frame.</summary>
    CeresSteamPose3,
    /// <summary><c>ceres_steam_oam_F16A</c>, an installed extended frame.</summary>
    CeresSteamPose4,
    /// <summary><c>ceres_steam_oam_F174</c>, an installed extended frame.</summary>
    CeresSteamPose5,
    /// <summary><c>ceres_steam_oam_F17E</c>, an installed extended frame.</summary>
    CeresSteamPose6,
    /// <summary><c>ceres_steam_oam_F188</c>, an installed extended frame.</summary>
    CeresSteamPose7,
    /// <summary><c>ceres_steam_oam_F192</c>, an installed extended frame.</summary>
    CeresSteamPose8,
    /// <summary><c>ceres_steam_oam_F19C</c>, an installed extended frame.</summary>
    CeresSteamPose9,
    /// <summary><c>ceres_steam_oam_F1A6</c>, an installed extended frame.</summary>
    CeresSteamPose10,
    /// <summary><c>ceres_steam_oam_F1B0</c>, an installed extended frame.</summary>
    CeresSteamPose11,
    /// <summary><c>ceres_steam_oam_F1BA</c>, an installed extended frame.</summary>
    CeresSteamPose12,
    /// <summary><c>ceres_steam_oam_F1C4</c>, an installed extended frame.</summary>
    CeresSteamPose13,
    /// <summary><c>ceres_steam_oam_F1CE</c>, an installed extended frame.</summary>
    CeresSteamPose14,
    /// <summary><c>ceres_steam_oam_F1D8</c>, an installed extended frame.</summary>
    CeresSteamPose15,
    /// <summary><c>ceres_steam_oam_F1E2</c>, an installed extended frame.</summary>
    CeresSteamPose16,
    /// <summary><c>ceres_steam_oam_F1EC</c>, an installed extended frame.</summary>
    CeresSteamPose17,
    /// <summary><c>ceres_steam_oam_F1F6</c>, an installed extended frame.</summary>
    CeresSteamPose18,
    /// <summary><c>ceres_steam_oam_F200</c>, an installed extended frame.</summary>
    CeresSteamPose19,
    /// <summary><c>ceres_steam_oam_F20A</c>, an installed extended frame.</summary>
    CeresSteamPose20,
    /// <summary><c>ceres_steam_oam_F214</c>, an installed extended frame.</summary>
    CeresSteamPose21,
    /// <summary><c>ceres_steam_oam_F21E</c>, an installed extended frame.</summary>
    CeresSteamPose22,
    /// <summary><c>ceres_steam_oam_F228</c>, an installed extended frame.</summary>
    CeresSteamPose23,
    /// <summary><c>ceres_steam_oam_F232</c>, an installed extended frame.</summary>
    CeresSteamPose24,
    /// <summary><c>ceres_steam_oam_F23C</c>, an installed extended frame.</summary>
    CeresSteamPose25,
    /// <summary><c>ceres_steam_oam_F246</c>, an installed extended frame.</summary>
    CeresSteamPose26,
    /// <summary><c>ceres_steam_oam_F250</c>, an installed extended frame.</summary>
    CeresSteamPose27,
    /// <summary><c>oum_oam_CB87</c>, an installed extended frame.</summary>
    OumPose0,
    /// <summary><c>oum_oam_CB91</c>, an installed extended frame.</summary>
    OumPose1,
    /// <summary><c>oum_oam_CB9B</c>, an installed extended frame.</summary>
    OumPose2,
    /// <summary><c>oum_oam_CBA5</c>, an installed extended frame.</summary>
    OumPose3,
    /// <summary><c>oum_oam_CBAF</c>, an installed extended frame.</summary>
    OumPose4,
    /// <summary><c>oum_oam_CBB9</c>, an installed extended frame.</summary>
    OumPose5,
    /// <summary><c>oum_oam_CBC3</c>, an installed extended frame.</summary>
    OumPose6,
    /// <summary><c>oum_oam_CBCD</c>, an installed extended frame.</summary>
    OumPose7,
    /// <summary><c>oum_oam_CBD7</c>, an installed extended frame.</summary>
    OumPose8,
    /// <summary><c>oum_oam_CBE1</c>, an installed extended frame.</summary>
    OumPose9,
    /// <summary><c>oum_oam_CBEB</c>, an installed extended frame.</summary>
    OumPose10,
    /// <summary><c>oum_oam_CBF5</c>, an installed extended frame.</summary>
    OumPose11,
    /// <summary><c>oum_oam_CBFF</c>, an installed extended frame.</summary>
    OumPose12,
    /// <summary><c>oum_oam_CC09</c>, an installed extended frame.</summary>
    OumPose13,
    /// <summary><c>oum_oam_CC13</c>, an installed extended frame.</summary>
    OumPose14,
    /// <summary><c>oum_oam_CC1D</c>, an installed extended frame.</summary>
    OumPose15,
    /// <summary><c>oum_oam_CC27</c>, an installed extended frame.</summary>
    OumPose16,
    /// <summary><c>oum_oam_CC31</c>, an installed extended frame.</summary>
    OumPose17,
    /// <summary><c>oum_oam_CC3B</c>, an installed extended frame.</summary>
    OumPose18,
    /// <summary><c>oum_oam_CC45</c>, an installed extended frame.</summary>
    OumPose19,
    /// <summary><c>oum_oam_CC4F</c>, an installed extended frame.</summary>
    OumPose20,
    /// <summary><c>oum_oam_CC59</c>, an installed extended frame.</summary>
    OumPose21,
    /// <summary><c>oum_oam_CC63</c>, an installed extended frame.</summary>
    OumPose22,
    /// <summary><c>oum_oam_CC6D</c>, an installed extended frame.</summary>
    OumPose23,
    /// <summary><c>oum_oam_CC77</c>, an installed extended frame.</summary>
    OumPose24,
    /// <summary><c>oum_oam_CC81</c>, an installed extended frame.</summary>
    OumPose25,
    /// <summary><c>oum_oam_CC8B</c>, an installed extended frame.</summary>
    OumPose26,
    /// <summary><c>oum_oam_CC95</c>, an installed extended frame.</summary>
    OumPose27,
    /// <summary><c>oum_oam_CC9F</c>, an installed extended frame.</summary>
    OumPose28,
    /// <summary><c>oum_oam_CCA9</c>, an installed extended frame.</summary>
    OumPose29,
    /// <summary><c>crocomire_oam_C65E</c>, an installed extended frame.</summary>
    CrocomireTongueFight0,
    /// <summary><c>crocomire_oam_C668</c>, an installed extended frame.</summary>
    CrocomireTongueFight1,
    /// <summary><c>crocomire_oam_C672</c>, an installed extended frame.</summary>
    CrocomireTongueFight2,
    /// <summary><c>crocomire_oam_C67C</c>, an installed extended frame.</summary>
    CrocomireTongueFight3,
    /// <summary><c>crocomire_oam_CACE</c>, an installed extended frame.</summary>
    CrocomireTongueMeltingPose0,
    /// <summary><c>crocomire_oam_CAD8</c>, an installed extended frame.</summary>
    CrocomireTongueMeltingPose1,
    /// <summary><c>crocomire_oam_CAE2</c>, an installed extended frame.</summary>
    CrocomireTongueMeltingPose2,
    /// <summary><c>crocomire_oam_CAEC</c>, an installed extended frame.</summary>
    CrocomireTongueMeltingPose3,
    /// <summary><c>crocomire_oam_CAF6</c>, an installed extended frame.</summary>
    CrocomireTongueMeltingPose4,
    /// <summary><c>crocomire_body_oam_BFC4</c>, an installed extended frame.</summary>
    CrocomireBody0,
    /// <summary><c>crocomire_body_oam_BFF6</c>, an installed extended frame.</summary>
    CrocomireBody1,
    /// <summary><c>crocomire_body_oam_C028</c>, an installed extended frame.</summary>
    CrocomireBody2,
    /// <summary><c>crocomire_body_oam_C05A</c>, an installed extended frame.</summary>
    CrocomireBody3,
    /// <summary><c>crocomire_body_oam_C08C</c>, an installed extended frame.</summary>
    CrocomireBody4,
    /// <summary><c>crocomire_body_oam_C0BE</c>, an installed extended frame.</summary>
    CrocomireBody5,
    /// <summary><c>crocomire_body_oam_C0F0</c>, an installed extended frame.</summary>
    CrocomireBody6,
    /// <summary><c>crocomire_body_oam_C122</c>, an installed extended frame.</summary>
    CrocomireBody7,
    /// <summary><c>crocomire_body_oam_C154</c>, an installed extended frame.</summary>
    CrocomireBody8,
    /// <summary><c>crocomire_body_oam_C186</c>, an installed extended frame.</summary>
    CrocomireBody9,
    /// <summary><c>crocomire_body_oam_C1B8</c>, an installed extended frame.</summary>
    CrocomireBody10,
    /// <summary><c>crocomire_body_oam_C1EA</c>, an installed extended frame.</summary>
    CrocomireBody11,
    /// <summary><c>crocomire_body_oam_C2EC</c>, an installed extended frame.</summary>
    CrocomireBody12,
    /// <summary><c>crocomire_body_oam_C326</c>, an installed extended frame.</summary>
    CrocomireBody13,
    /// <summary><c>crocomire_body_oam_C360</c>, an installed extended frame.</summary>
    CrocomireBody14,
    /// <summary><c>crocomire_body_oam_C39A</c>, an installed extended frame.</summary>
    CrocomireBody15,
    /// <summary><c>crocomire_body_oam_C3D4</c>, an installed extended frame.</summary>
    CrocomireBody16,
    /// <summary><c>crocomire_body_oam_C40E</c>, an installed extended frame.</summary>
    CrocomireBody17,
    /// <summary><c>crocomire_body_oam_C448</c>, an installed extended frame.</summary>
    CrocomireBody18,
    /// <summary><c>crocomire_body_oam_C47A</c>, an installed extended frame.</summary>
    CrocomireBody19,
    /// <summary><c>crocomire_body_oam_C4AC</c>, an installed extended frame.</summary>
    CrocomireBody20,
    /// <summary><c>crocomire_body_oam_C4DE</c>, an installed extended frame.</summary>
    CrocomireBody21,
    /// <summary><c>crocomire_body_oam_C510</c>, an installed extended frame.</summary>
    CrocomireBody22,
    /// <summary><c>crocomire_body_oam_C542</c>, an installed extended frame.</summary>
    CrocomireBody23,
    /// <summary><c>crocomire_body_oam_C574</c>, an installed extended frame.</summary>
    CrocomireBody24,
    /// <summary><c>crocomire_body_oam_C5AE</c>, an installed extended frame.</summary>
    CrocomireBody25,
    /// <summary><c>crocomire_body_oam_C5E8</c>, an installed extended frame.</summary>
    CrocomireBody26,
    /// <summary><c>crocomire_body_oam_C6A4</c>, an installed extended frame.</summary>
    CrocomireBody27,
    /// <summary><c>crocomire_body_oam_C752</c>, an installed extended frame.</summary>
    CrocomireBody28,
    /// <summary><c>crocomire_body_oam_C78C</c>, an installed extended frame.</summary>
    CrocomireBody29,
    /// <summary><c>crocomire_body_oam_C7C6</c>, an installed extended frame.</summary>
    CrocomireBody30,
    /// <summary><c>crocomire_body_oam_C800</c>, an installed extended frame.</summary>
    CrocomireBody31,
    /// <summary><c>crocomire_body_oam_C83A</c>, an installed extended frame.</summary>
    CrocomireBody32,
    /// <summary><c>crocomire_body_oam_C874</c>, an installed extended frame.</summary>
    CrocomireBody33,
    /// <summary><c>crocomire_body_oam_C8AE</c>, an installed extended frame.</summary>
    CrocomireBody34,
    /// <summary><c>crocomire_body_oam_C8E8</c>, an installed extended frame.</summary>
    CrocomireBody35,
    /// <summary><c>crocomire_body_oam_C922</c>, an installed extended frame.</summary>
    CrocomireBody36,
    /// <summary><c>crocomire_body_oam_C95C</c>, an installed extended frame.</summary>
    CrocomireBody37,
    /// <summary><c>crocomire_body_oam_C996</c>, an installed extended frame.</summary>
    CrocomireBody38,
    /// <summary><c>crocomire_body_oam_C9D0</c>, an installed extended frame.</summary>
    CrocomireBody39,
    /// <summary><c>crocomire_body_oam_CA0A</c>, an installed extended frame.</summary>
    CrocomireBody40,
    /// <summary><c>crocomire_body_oam_CA44</c>, an installed extended frame.</summary>
    CrocomireBody41,
    /// <summary><c>crocomire_body_oam_CA7E</c>, an installed extended frame.</summary>
    CrocomireBody42,
    /// <summary><c>crocomire_body_oam_CA88</c>, an installed extended frame.</summary>
    CrocomireBody43,
    /// <summary><c>crocomire_body_oam_CA92</c>, an installed extended frame.</summary>
    CrocomireBody44,
    /// <summary><c>crocomire_body_oam_CA9C</c>, an installed extended frame.</summary>
    CrocomireBody45,
    /// <summary><c>crocomire_body_oam_CAA6</c>, an installed extended frame.</summary>
    CrocomireBody46,
    /// <summary><c>crocomire_body_oam_CAB0</c>, an installed extended frame.</summary>
    CrocomireBody47,
    /// <summary><c>crocomire_body_oam_CABA</c>, an installed extended frame.</summary>
    CrocomireBody48,
    /// <summary><c>crocomire_body_oam_CAC4</c>, an installed extended frame.</summary>
    CrocomireBody49,
    /// <summary><c>bomb_torizo_dormant</c>, an installed extended frame.</summary>
    BombTorizoDormant,
    /// <summary><c>golden_torizo_initial</c>, an installed extended frame.</summary>
    GoldenTorizoInitial,
    /// <summary><c>kraid_arm_oam_8F59</c>, an installed extended frame.</summary>
    KraidArm0,
    /// <summary><c>kraid_arm_oam_8F83</c>, an installed extended frame.</summary>
    KraidArm1,
    /// <summary><c>kraid_arm_oam_8FAD</c>, an installed extended frame.</summary>
    KraidArm2,
    /// <summary><c>kraid_arm_oam_8FD7</c>, an installed extended frame.</summary>
    KraidArm3,
    /// <summary><c>kraid_arm_oam_9001</c>, an installed extended frame.</summary>
    KraidArm4,
    /// <summary><c>kraid_arm_oam_902B</c>, an installed extended frame.</summary>
    KraidArm5,
    /// <summary><c>kraid_arm_oam_9055</c>, an installed extended frame.</summary>
    KraidArm6,
    /// <summary><c>kraid_arm_oam_907F</c>, an installed extended frame.</summary>
    KraidArm7,
    /// <summary><c>kraid_arm_oam_90A9</c>, an installed extended frame.</summary>
    KraidArm8,
    /// <summary><c>kraid_arm_oam_90D3</c>, an installed extended frame.</summary>
    KraidArm9,
    /// <summary><c>kraid_arm_oam_90FD</c>, an installed extended frame.</summary>
    KraidArm10,
    /// <summary><c>kraid_arm_oam_9127</c>, an installed extended frame.</summary>
    KraidArm11,
    /// <summary><c>kraid_arm_oam_9151</c>, an installed extended frame.</summary>
    KraidArm12,
    /// <summary><c>kraid_arm_oam_917B</c>, an installed extended frame.</summary>
    KraidArm13,
    /// <summary><c>kraid_arm_oam_91A5</c>, an installed extended frame.</summary>
    KraidArm14,
    /// <summary><c>kraid_arm_oam_91CF</c>, an installed extended frame.</summary>
    KraidArm15,
    /// <summary><c>kraid_arm_oam_91F9</c>, an installed extended frame.</summary>
    KraidArm16,
    /// <summary><c>kraid_arm_oam_9223</c>, an installed extended frame.</summary>
    KraidArm17,
    /// <summary><c>kraid_arm_oam_924D</c>, an installed extended frame.</summary>
    KraidArm18,
    /// <summary><c>kraid_arm_oam_9277</c>, an installed extended frame.</summary>
    KraidArm19,
    /// <summary><c>kraid_arm_oam_92A1</c>, an installed extended frame.</summary>
    KraidArm20,
    /// <summary><c>kraid_arm_oam_92AB</c>, an installed extended frame.</summary>
    KraidArm21,
    /// <summary><c>golden_torizo_awake_AA26</c>, an installed extended frame.</summary>
    GoldenTorizoAwake0,
    /// <summary><c>golden_torizo_awake_AA1C</c>, an installed extended frame.</summary>
    GoldenTorizoAwake1,
    /// <summary><c>golden_torizo_awake_AA12</c>, an installed extended frame.</summary>
    GoldenTorizoAwake2,
    /// <summary><c>golden_torizo_awake_AA3A</c>, an installed extended frame.</summary>
    GoldenTorizoAwake3,
    /// <summary><c>golden_torizo_awake_AA4C</c>, an installed extended frame.</summary>
    GoldenTorizoAwake4,
    /// <summary><c>golden_torizo_awake_AA5E</c>, an installed extended frame.</summary>
    GoldenTorizoAwake5,
    /// <summary><c>golden_torizo_walk_left_A4FA</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeft0,
    /// <summary><c>golden_torizo_walk_left_A51C</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeft1,
    /// <summary><c>golden_torizo_walk_left_A53E</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeft2,
    /// <summary><c>golden_torizo_walk_left_A560</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeft3,
    /// <summary><c>golden_torizo_walk_left_A582</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeft4,
    /// <summary><c>golden_torizo_walk_left_A5A4</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeft5,
    /// <summary><c>golden_torizo_walk_left_A5C6</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeft6,
    /// <summary><c>golden_torizo_walk_left_A5E8</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeft7,
    /// <summary><c>golden_torizo_walk_left_A60A</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeft8,
    /// <summary><c>golden_torizo_walk_left_A62C</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeft9,
    /// <summary><c>golden_torizo_rightward_A4F0</c>, an installed extended frame.</summary>
    GoldenTorizoRightward0,
    /// <summary><c>golden_torizo_rightward_AA98</c>, an installed extended frame.</summary>
    GoldenTorizoRightward1,
    /// <summary><c>golden_torizo_rightward_AABA</c>, an installed extended frame.</summary>
    GoldenTorizoRightward2,
    /// <summary><c>golden_torizo_rightward_AADC</c>, an installed extended frame.</summary>
    GoldenTorizoRightward3,
    /// <summary><c>golden_torizo_rightward_AAFE</c>, an installed extended frame.</summary>
    GoldenTorizoRightward4,
    /// <summary><c>golden_torizo_rightward_AB20</c>, an installed extended frame.</summary>
    GoldenTorizoRightward5,
    /// <summary><c>golden_torizo_rightward_AB42</c>, an installed extended frame.</summary>
    GoldenTorizoRightward6,
    /// <summary><c>golden_torizo_rightward_AB64</c>, an installed extended frame.</summary>
    GoldenTorizoRightward7,
    /// <summary><c>golden_torizo_rightward_AB86</c>, an installed extended frame.</summary>
    GoldenTorizoRightward8,
    /// <summary><c>golden_torizo_rightward_ABA8</c>, an installed extended frame.</summary>
    GoldenTorizoRightward9,
    /// <summary><c>golden_torizo_rightward_ABCA</c>, an installed extended frame.</summary>
    GoldenTorizoRightward10,
    /// <summary><c>torizo_jump_back_B048</c>, an installed extended frame.</summary>
    TorizoJumpBack0,
    /// <summary><c>torizo_jump_back_B062</c>, an installed extended frame.</summary>
    TorizoJumpBack1,
    /// <summary><c>torizo_jump_back_B07C</c>, an installed extended frame.</summary>
    TorizoJumpBack2,
    /// <summary><c>golden_torizo_right_orb_AC88</c>, an installed extended frame.</summary>
    GoldenTorizoRightOrb0,
    /// <summary><c>golden_torizo_right_orb_ACA2</c>, an installed extended frame.</summary>
    GoldenTorizoRightOrb1,
    /// <summary><c>golden_torizo_right_orb_ACBC</c>, an installed extended frame.</summary>
    GoldenTorizoRightOrb2,
    /// <summary><c>golden_torizo_right_orb_ACD6</c>, an installed extended frame.</summary>
    GoldenTorizoRightOrb3,
    /// <summary><c>golden_torizo_right_orb_ACF0</c>, an installed extended frame.</summary>
    GoldenTorizoRightOrb4,
    /// <summary><c>golden_torizo_right_orb_AD0A</c>, an installed extended frame.</summary>
    GoldenTorizoRightOrb5,
    /// <summary><c>golden_torizo_right_sonic_ABEC</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic0,
    /// <summary><c>golden_torizo_right_sonic_AD24</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic1,
    /// <summary><c>golden_torizo_right_sonic_AD3E</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic2,
    /// <summary><c>golden_torizo_right_sonic_AD60</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic3,
    /// <summary><c>golden_torizo_right_sonic_AD82</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic4,
    /// <summary><c>golden_torizo_right_sonic_ADA4</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic5,
    /// <summary><c>golden_torizo_right_sonic_ADC6</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic6,
    /// <summary><c>golden_torizo_right_sonic_ADE8</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic7,
    /// <summary><c>golden_torizo_right_sonic_AE0A</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic8,
    /// <summary><c>golden_torizo_right_sonic_AE2C</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic9,
    /// <summary><c>golden_torizo_right_sonic_AE4E</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic10,
    /// <summary><c>golden_torizo_right_sonic_AE70</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic11,
    /// <summary><c>golden_torizo_right_sonic_AE8A</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic12,
    /// <summary><c>golden_torizo_right_sonic_AEA4</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic13,
    /// <summary><c>golden_torizo_right_sonic_AEBE</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic14,
    /// <summary><c>golden_torizo_right_sonic_AED8</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic15,
    /// <summary><c>golden_torizo_right_sonic_AEF2</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic16,
    /// <summary><c>golden_torizo_right_sonic_AF14</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic17,
    /// <summary><c>golden_torizo_right_sonic_AF36</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic18,
    /// <summary><c>golden_torizo_right_sonic_AF58</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic19,
    /// <summary><c>golden_torizo_right_sonic_AF7A</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonic20,
    /// <summary><c>torizo_falling_left_B014</c>, an installed extended frame.</summary>
    TorizoFallingLeft,
    /// <summary><c>golden_torizo_left_foot_orb_AC06</c>, an installed extended frame.</summary>
    GoldenTorizoLeftFootOrb0,
    /// <summary><c>golden_torizo_left_foot_orb_AC20</c>, an installed extended frame.</summary>
    GoldenTorizoLeftFootOrb1,
    /// <summary><c>golden_torizo_left_foot_orb_AC3A</c>, an installed extended frame.</summary>
    GoldenTorizoLeftFootOrb2,
    /// <summary><c>golden_torizo_left_foot_orb_AC54</c>, an installed extended frame.</summary>
    GoldenTorizoLeftFootOrb3,
    /// <summary><c>golden_torizo_left_foot_orb_AC6E</c>, an installed extended frame.</summary>
    GoldenTorizoLeftFootOrb4,
    /// <summary><c>torizo_jump_back_left_AFFA</c>, an installed extended frame.</summary>
    TorizoJumpBackLeft0,
    /// <summary><c>torizo_jump_back_left_B02E</c>, an installed extended frame.</summary>
    TorizoJumpBackLeft1,
    /// <summary><c>golden_torizo_left_orb_A64E</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb0,
    /// <summary><c>golden_torizo_left_orb_A668</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb1,
    /// <summary><c>golden_torizo_left_orb_A682</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb2,
    /// <summary><c>golden_torizo_left_orb_A69C</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb3,
    /// <summary><c>golden_torizo_left_orb_A6B6</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb4,
    /// <summary><c>golden_torizo_left_orb_A6D0</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb5,
    /// <summary><c>golden_torizo_left_orb_A6EA</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb6,
    /// <summary><c>golden_torizo_left_orb_A704</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb7,
    /// <summary><c>golden_torizo_left_orb_A71E</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb8,
    /// <summary><c>golden_torizo_left_orb_A738</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb9,
    /// <summary><c>golden_torizo_left_orb_A752</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb10,
    /// <summary><c>golden_torizo_left_orb_A76C</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrb11,
    /// <summary><c>torizo_combat_A786</c>, an installed extended frame.</summary>
    TorizoCombat0,
    /// <summary><c>torizo_combat_A7A0</c>, an installed extended frame.</summary>
    TorizoCombat1,
    /// <summary><c>torizo_combat_A7C2</c>, an installed extended frame.</summary>
    TorizoCombat2,
    /// <summary><c>torizo_combat_A7E4</c>, an installed extended frame.</summary>
    TorizoCombat3,
    /// <summary><c>torizo_combat_A806</c>, an installed extended frame.</summary>
    TorizoCombat4,
    /// <summary><c>torizo_combat_A828</c>, an installed extended frame.</summary>
    TorizoCombat5,
    /// <summary><c>torizo_combat_A84A</c>, an installed extended frame.</summary>
    TorizoCombat6,
    /// <summary><c>torizo_combat_A86C</c>, an installed extended frame.</summary>
    TorizoCombat7,
    /// <summary><c>torizo_combat_A88E</c>, an installed extended frame.</summary>
    TorizoCombat8,
    /// <summary><c>torizo_combat_A8B0</c>, an installed extended frame.</summary>
    TorizoCombat9,
    /// <summary><c>torizo_combat_A8D2</c>, an installed extended frame.</summary>
    TorizoCombat10,
    /// <summary><c>torizo_combat_A8EC</c>, an installed extended frame.</summary>
    TorizoCombat11,
    /// <summary><c>torizo_combat_A906</c>, an installed extended frame.</summary>
    TorizoCombat12,
    /// <summary><c>torizo_combat_A920</c>, an installed extended frame.</summary>
    TorizoCombat13,
    /// <summary><c>torizo_combat_A93A</c>, an installed extended frame.</summary>
    TorizoCombat14,
    /// <summary><c>torizo_combat_A954</c>, an installed extended frame.</summary>
    TorizoCombat15,
    /// <summary><c>torizo_combat_A976</c>, an installed extended frame.</summary>
    TorizoCombat16,
    /// <summary><c>torizo_combat_A998</c>, an installed extended frame.</summary>
    TorizoCombat17,
    /// <summary><c>torizo_combat_A9BA</c>, an installed extended frame.</summary>
    TorizoCombat18,
    /// <summary><c>torizo_combat_A9DC</c>, an installed extended frame.</summary>
    TorizoCombat19,
    /// <summary><c>torizo_combat_AF9C</c>, an installed extended frame.</summary>
    TorizoCombat20,
    /// <summary><c>torizo_combat_AFA6</c>, an installed extended frame.</summary>
    TorizoCombat21,
    /// <summary><c>torizo_combat_AFB0</c>, an installed extended frame.</summary>
    TorizoCombat22,
    /// <summary><c>torizo_combat_AFBA</c>, an installed extended frame.</summary>
    TorizoCombat23,
    /// <summary><c>torizo_combat_AFC4</c>, an installed extended frame.</summary>
    TorizoCombat24,
    /// <summary><c>torizo_combat_AFD6</c>, an installed extended frame.</summary>
    TorizoCombat25,
    /// <summary><c>torizo_combat_AFE8</c>, an installed extended frame.</summary>
    TorizoCombat26,
    /// <summary><c>mother_brain_body_oam_standing</c>, an installed extended frame.</summary>
    MotherBrainBodyOamStanding,
    /// <summary><c>mother_brain_body_oam_walking_0</c>, an installed extended frame.</summary>
    MotherBrainBodyOamWalking0,
    /// <summary><c>mother_brain_body_oam_walking_1</c>, an installed extended frame.</summary>
    MotherBrainBodyOamWalking1,
    /// <summary><c>mother_brain_body_oam_walking_2</c>, an installed extended frame.</summary>
    MotherBrainBodyOamWalking2,
    /// <summary><c>mother_brain_body_oam_walking_3</c>, an installed extended frame.</summary>
    MotherBrainBodyOamWalking3,
    /// <summary><c>mother_brain_body_oam_walking_4</c>, an installed extended frame.</summary>
    MotherBrainBodyOamWalking4,
    /// <summary><c>mother_brain_body_oam_walking_5</c>, an installed extended frame.</summary>
    MotherBrainBodyOamWalking5,
    /// <summary><c>mother_brain_body_oam_walking_6</c>, an installed extended frame.</summary>
    MotherBrainBodyOamWalking6,
    /// <summary><c>mother_brain_body_oam_walking_7</c>, an installed extended frame.</summary>
    MotherBrainBodyOamWalking7,
    /// <summary><c>mother_brain_body_oam_crouched</c>, an installed extended frame.</summary>
    MotherBrainBodyOamCrouched,
    /// <summary><c>mother_brain_body_oam_uncrouching</c>, an installed extended frame.</summary>
    MotherBrainBodyOamUncrouching,
    /// <summary><c>mother_brain_body_oam_leaning_down</c>, an installed extended frame.</summary>
    MotherBrainBodyOamLeaningDown,
    /// <summary><c>mother_brain_body_oam_initial_dummy</c>, an installed extended frame.</summary>
    MotherBrainBodyOamInitialDummy,
    /// <summary><c>mother_brain_body_oam_death_beam_0</c>, an installed extended frame.</summary>
    MotherBrainBodyOamDeathBeam0,
    /// <summary><c>mother_brain_body_oam_death_beam_1</c>, an installed extended frame.</summary>
    MotherBrainBodyOamDeathBeam1,
    /// <summary><c>mother_brain_body_oam_death_beam_2</c>, an installed extended frame.</summary>
    MotherBrainBodyOamDeathBeam2,
    /// <summary><c>mother_brain_body_oam_death_beam_3</c>, an installed extended frame.</summary>
    MotherBrainBodyOamDeathBeam3,
    /// <summary><c>phantoon_bg2_DEDD</c>, an installed extended frame.</summary>
    PhantoonInvulnerableBody0,
    /// <summary><c>phantoon_bg2_DEE7</c>, an installed extended frame.</summary>
    PhantoonFullHitboxBody0,
    /// <summary><c>phantoon_bg2_DEF1</c>, an installed extended frame.</summary>
    PhantoonEyeHitboxBody0,
    /// <summary><c>phantoon_bg2_DEFB</c>, an installed extended frame.</summary>
    PhantoonClosedEye0,
    /// <summary><c>phantoon_bg2_DF05</c>, an installed extended frame.</summary>
    PhantoonOpeningEye0,
    /// <summary><c>phantoon_bg2_DF0F</c>, an installed extended frame.</summary>
    PhantoonOpeningEye1,
    /// <summary><c>phantoon_bg2_DF19</c>, an installed extended frame.</summary>
    PhantoonOpeningEye2,
    /// <summary><c>phantoon_bg2_DF23</c>, an installed extended frame.</summary>
    PhantoonCenteredEye0,
    /// <summary><c>phantoon_bg2_DF2D</c>, an installed extended frame.</summary>
    PhantoonGaze0,
    /// <summary><c>phantoon_bg2_DF37</c>, an installed extended frame.</summary>
    PhantoonGaze4,
    /// <summary><c>phantoon_bg2_DF41</c>, an installed extended frame.</summary>
    PhantoonGaze6,
    /// <summary><c>phantoon_bg2_DF4B</c>, an installed extended frame.</summary>
    PhantoonGaze2,
    /// <summary><c>phantoon_bg2_DF55</c>, an installed extended frame.</summary>
    PhantoonGaze5,
    /// <summary><c>phantoon_bg2_DF5F</c>, an installed extended frame.</summary>
    PhantoonGaze3,
    /// <summary><c>phantoon_bg2_DF69</c>, an installed extended frame.</summary>
    PhantoonGaze7,
    /// <summary><c>phantoon_bg2_DF73</c>, an installed extended frame.</summary>
    PhantoonGaze1,
    /// <summary><c>phantoon_bg2_DFB3</c>, an installed extended frame.</summary>
    PhantoonTentacle0,
    /// <summary><c>phantoon_bg2_DFC5</c>, an installed extended frame.</summary>
    PhantoonTentacle1,
    /// <summary><c>phantoon_bg2_DFD7</c>, an installed extended frame.</summary>
    PhantoonTentacle2,
    /// <summary><c>phantoon_bg2_DFE9</c>, an installed extended frame.</summary>
    PhantoonMouth0,
    /// <summary><c>phantoon_bg2_DFF3</c>, an installed extended frame.</summary>
    PhantoonMouth1,
    /// <summary><c>phantoon_bg2_DFFD</c>, an installed extended frame.</summary>
    PhantoonMouth2,
    /// <summary><c>draygon_bg2_A31B</c>, an installed extended frame.</summary>
    DraygonFrame6,
    /// <summary><c>draygon_bg2_A325</c>, an installed extended frame.</summary>
    DraygonFrame7,
    /// <summary><c>draygon_bg2_A32F</c>, an installed extended frame.</summary>
    DraygonFrame8,
    /// <summary><c>draygon_bg2_A339</c>, an installed extended frame.</summary>
    DraygonFrame9,
    /// <summary><c>draygon_bg2_A343</c>, an installed extended frame.</summary>
    DraygonFrame10,
    /// <summary><c>draygon_bg2_A34D</c>, an installed extended frame.</summary>
    DraygonFrame11,
    /// <summary><c>draygon_bg2_A357</c>, an installed extended frame.</summary>
    DraygonFrame12,
    /// <summary><c>draygon_bg2_A361</c>, an installed extended frame.</summary>
    DraygonFrame13,
    /// <summary><c>draygon_bg2_A36B</c>, an installed extended frame.</summary>
    DraygonFrame14,
    /// <summary><c>draygon_bg2_A375</c>, an installed extended frame.</summary>
    DraygonFrame15,
    /// <summary><c>draygon_bg2_A37F</c>, an installed extended frame.</summary>
    DraygonFrame16,
    /// <summary><c>draygon_bg2_A389</c>, an installed extended frame.</summary>
    DraygonFrame17,
    /// <summary><c>draygon_bg2_A393</c>, an installed extended frame.</summary>
    DraygonFrame18,
    /// <summary><c>draygon_bg2_A39D</c>, an installed extended frame.</summary>
    DraygonFrame19,
    /// <summary><c>draygon_bg2_A3A7</c>, an installed extended frame.</summary>
    DraygonFrame20,
    /// <summary><c>draygon_bg2_A3B1</c>, an installed extended frame.</summary>
    DraygonFrame21,
    /// <summary><c>draygon_bg2_A3BB</c>, an installed extended frame.</summary>
    DraygonFrame22,
    /// <summary><c>draygon_bg2_A643</c>, an installed extended frame.</summary>
    DraygonFrame47,
    /// <summary><c>draygon_bg2_A64D</c>, an installed extended frame.</summary>
    DraygonFrame48,
    /// <summary><c>draygon_bg2_A657</c>, an installed extended frame.</summary>
    DraygonFrame49,
    /// <summary><c>draygon_bg2_A661</c>, an installed extended frame.</summary>
    DraygonFrame50,
    /// <summary><c>draygon_bg2_A66B</c>, an installed extended frame.</summary>
    DraygonFrame51,
    /// <summary><c>draygon_bg2_A675</c>, an installed extended frame.</summary>
    DraygonFrame52,
    /// <summary><c>draygon_bg2_A67F</c>, an installed extended frame.</summary>
    DraygonFrame53,
    /// <summary><c>draygon_bg2_A689</c>, an installed extended frame.</summary>
    DraygonFrame54,
    /// <summary><c>draygon_bg2_A693</c>, an installed extended frame.</summary>
    DraygonFrame55,
    /// <summary><c>draygon_bg2_A69D</c>, an installed extended frame.</summary>
    DraygonFrame56,
    /// <summary><c>draygon_bg2_A6A7</c>, an installed extended frame.</summary>
    DraygonFrame57,
    /// <summary><c>draygon_bg2_A6B1</c>, an installed extended frame.</summary>
    DraygonFrame58,
    /// <summary><c>draygon_bg2_A6BB</c>, an installed extended frame.</summary>
    DraygonFrame59,
    /// <summary><c>draygon_bg2_A6C5</c>, an installed extended frame.</summary>
    DraygonFrame60,
    /// <summary><c>draygon_bg2_A6CF</c>, an installed extended frame.</summary>
    DraygonFrame61,
    /// <summary><c>draygon_bg2_A6D9</c>, an installed extended frame.</summary>
    DraygonFrame62,
    /// <summary><c>draygon_bg2_A6E3</c>, an installed extended frame.</summary>
    DraygonFrame63,
    /// <summary><c>crocomire_skeleton_oam_E1FE</c>, an installed extended frame.</summary>
    CrocomireSkeleton0,
    /// <summary><c>crocomire_skeleton_oam_E228</c>, an installed extended frame.</summary>
    CrocomireSkeleton1,
    /// <summary><c>crocomire_skeleton_oam_E252</c>, an installed extended frame.</summary>
    CrocomireSkeleton2,
    /// <summary><c>crocomire_skeleton_oam_E27C</c>, an installed extended frame.</summary>
    CrocomireSkeleton3,
    /// <summary><c>crocomire_skeleton_oam_E2A6</c>, an installed extended frame.</summary>
    CrocomireSkeleton4,
    /// <summary><c>crocomire_skeleton_oam_E2D0</c>, an installed extended frame.</summary>
    CrocomireSkeleton5,
    /// <summary><c>crocomire_skeleton_oam_E2FA</c>, an installed extended frame.</summary>
    CrocomireSkeleton6,
    /// <summary><c>crocomire_skeleton_oam_E324</c>, an installed extended frame.</summary>
    CrocomireSkeleton7,
    /// <summary><c>crocomire_skeleton_oam_E34E</c>, an installed extended frame.</summary>
    CrocomireSkeleton8,
    /// <summary><c>crocomire_skeleton_oam_E378</c>, an installed extended frame.</summary>
    CrocomireSkeleton9,
    /// <summary><c>crocomire_skeleton_oam_E3A2</c>, an installed extended frame.</summary>
    CrocomireSkeleton10,
    /// <summary><c>crocomire_skeleton_oam_E3CC</c>, an installed extended frame.</summary>
    CrocomireSkeleton11,
    /// <summary><c>crocomire_skeleton_oam_E3F6</c>, an installed extended frame.</summary>
    CrocomireSkeleton12,
    /// <summary><c>crocomire_skeleton_oam_E420</c>, an installed extended frame.</summary>
    CrocomireSkeleton13,
    /// <summary><c>crocomire_skeleton_oam_E46A</c>, an installed extended frame.</summary>
    CrocomireSkeleton14,
    /// <summary><c>crocomire_skeleton_oam_E4D4</c>, an installed extended frame.</summary>
    CrocomireSkeleton15,
    /// <summary><c>crocomire_skeleton_oam_E53E</c>, an installed extended frame.</summary>
    CrocomireSkeleton16,
    /// <summary><c>crocomire_skeleton_oam_E5A8</c>, an installed extended frame.</summary>
    CrocomireSkeleton17,
    /// <summary><c>crocomire_skeleton_oam_E60A</c>, an installed extended frame.</summary>
    CrocomireSkeleton18,
    /// <summary><c>crocomire_skeleton_oam_E65C</c>, an installed extended frame.</summary>
    CrocomireSkeleton19,
    /// <summary><c>crocomire_skeleton_oam_E68E</c>, an installed extended frame.</summary>
    CrocomireSkeleton20,
    /// <summary><c>crocomire_skeleton_oam_E6A8</c>, an installed extended frame.</summary>
    CrocomireSkeleton21,
    /// <summary><c>crocomire_skeleton_oam_E6B2</c>, an installed extended frame.</summary>
    CrocomireSkeleton22,
    /// <summary><c>crocomire_skeleton_oam_E6BC</c>, an installed extended frame.</summary>
    CrocomireSkeleton23,
    /// <summary><c>crocomire_skeleton_oam_E6C6</c>, an installed extended frame.</summary>
    CrocomireSkeleton24,
    /// <summary><c>crocomire_skeleton_oam_E6D0</c>, an installed extended frame.</summary>
    CrocomireSkeleton25,
    /// <summary><c>crocomire_skeleton_oam_E6DA</c>, an installed extended frame.</summary>
    CrocomireSkeleton26,
    /// <summary><c>crocomire_skeleton_oam_E6E4</c>, an installed extended frame.</summary>
    CrocomireSkeleton27,
    /// <summary><c>crocomire_skeleton_oam_E6EE</c>, an installed extended frame.</summary>
    CrocomireSkeleton28,
    /// <summary><c>crocomire_skeleton_oam_E6F8</c>, an installed extended frame.</summary>
    CrocomireSkeleton29,
    /// <summary><c>crocomire_skeleton_oam_E702</c>, an installed extended frame.</summary>
    CrocomireSkeleton30,
    /// <summary><c>crocomire_skeleton_oam_E70C</c>, an installed extended frame.</summary>
    CrocomireSkeleton31,
    /// <summary><c>crocomire_skeleton_oam_E716</c>, an installed extended frame.</summary>
    CrocomireSkeleton32,
    /// <summary><c>kraid_foot_oam_8CE3</c>, an installed extended frame.</summary>
    KraidFoot0,
    /// <summary><c>kraid_foot_oam_8CF5</c>, an installed extended frame.</summary>
    KraidFoot1,
    /// <summary><c>kraid_foot_oam_8D07</c>, an installed extended frame.</summary>
    KraidFoot2,
    /// <summary><c>kraid_foot_oam_8D19</c>, an installed extended frame.</summary>
    KraidFoot3,
    /// <summary><c>kraid_foot_oam_8D2B</c>, an installed extended frame.</summary>
    KraidFoot4,
    /// <summary><c>kraid_foot_oam_8D3D</c>, an installed extended frame.</summary>
    KraidFoot5,
    /// <summary><c>kraid_foot_oam_8D4F</c>, an installed extended frame.</summary>
    KraidFoot6,
    /// <summary><c>kraid_foot_oam_8D61</c>, an installed extended frame.</summary>
    KraidFoot7,
    /// <summary><c>kraid_foot_oam_8D73</c>, an installed extended frame.</summary>
    KraidFoot8,
    /// <summary><c>kraid_foot_oam_8D85</c>, an installed extended frame.</summary>
    KraidFoot9,
    /// <summary><c>kraid_foot_oam_8D97</c>, an installed extended frame.</summary>
    KraidFoot10,
    /// <summary><c>kraid_foot_oam_8DA9</c>, an installed extended frame.</summary>
    KraidFoot11,
    /// <summary><c>kraid_foot_oam_8DBB</c>, an installed extended frame.</summary>
    KraidFoot12,
    /// <summary><c>kraid_foot_oam_8DCD</c>, an installed extended frame.</summary>
    KraidFoot13,
    /// <summary><c>kraid_foot_oam_8DDF</c>, an installed extended frame.</summary>
    KraidFoot14,
    /// <summary><c>kraid_foot_oam_8DF1</c>, an installed extended frame.</summary>
    KraidFoot15,
    /// <summary><c>kraid_foot_oam_8E03</c>, an installed extended frame.</summary>
    KraidFoot16,
    /// <summary><c>kraid_foot_oam_8E15</c>, an installed extended frame.</summary>
    KraidFoot17,
    /// <summary><c>kraid_foot_oam_8E27</c>, an installed extended frame.</summary>
    KraidFoot18,
    /// <summary><c>kraid_foot_oam_8E39</c>, an installed extended frame.</summary>
    KraidFoot19,
    /// <summary><c>kraid_foot_oam_8E4B</c>, an installed extended frame.</summary>
    KraidFoot20,
    /// <summary><c>kraid_foot_oam_8E5D</c>, an installed extended frame.</summary>
    KraidFoot21,
    /// <summary><c>kraid_foot_oam_8E6F</c>, an installed extended frame.</summary>
    KraidFoot22,
    /// <summary><c>kraid_foot_oam_8E81</c>, an installed extended frame.</summary>
    KraidFoot23,
    /// <summary><c>kraid_foot_oam_8E93</c>, an installed extended frame.</summary>
    KraidFoot24,
    /// <summary><c>kraid_foot_oam_8EA5</c>, an installed extended frame.</summary>
    KraidFoot25,
    /// <summary><c>kraid_foot_oam_8EB7</c>, an installed extended frame.</summary>
    KraidFoot26,
    /// <summary><c>kraid_foot_oam_8EC9</c>, an installed extended frame.</summary>
    KraidFoot27,
    /// <summary><c>kraid_foot_oam_8EDB</c>, an installed extended frame.</summary>
    KraidFoot28,
    /// <summary><c>kraid_foot_oam_8EED</c>, an installed extended frame.</summary>
    KraidFoot29,
    /// <summary><c>kraid_foot_oam_8EFF</c>, an installed extended frame.</summary>
    KraidFoot30,
    /// <summary><c>kraid_foot_oam_8F11</c>, an installed extended frame.</summary>
    KraidFoot31,
    /// <summary><c>kraid_foot_oam_8F23</c>, an installed extended frame.</summary>
    KraidFoot32,
    /// <summary><c>kraid_foot_oam_8F47</c>, an installed extended frame.</summary>
    KraidFoot33,
    /// <summary><c>kraid_foot_oam_A565</c>, an installed extended frame.</summary>
    KraidFootStanding,
    /// <summary>The empty spritemap at the same offset in every enemy bank: an enemy showing it draws nothing.</summary>
    Empty,
    /// <summary>The visually empty one-component extended frame at the same offset in every enemy bank.</summary>
    EmptyExtended,
}
