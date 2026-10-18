namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The frames an enemy shows: every installed spritemap and extended frame, named by its installed composition, in
/// schema order, and the shared empty frames every enemy bank repeats.
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
    RioBd6c,
    /// <summary><c>rio_bd82</c>, an installed spritemap.</summary>
    RioBd82,
    /// <summary><c>rio_bd98</c>, an installed spritemap.</summary>
    RioBd98,
    /// <summary><c>rio_bdae</c>, an installed spritemap.</summary>
    RioBdae,
    /// <summary><c>rio_bdc4</c>, an installed spritemap.</summary>
    RioBdc4,
    /// <summary><c>rio_bdda</c>, an installed spritemap.</summary>
    RioBdda,
    /// <summary><c>rio_bdf0</c>, an installed spritemap.</summary>
    RioBdf0,
    /// <summary><c>rio_be06</c>, an installed spritemap.</summary>
    RioBe06,
    /// <summary><c>lower_norfair_rio_c8bd</c>, an installed spritemap.</summary>
    LowerNorfairRioC8bd,
    /// <summary><c>lower_norfair_rio_c8d3</c>, an installed spritemap.</summary>
    LowerNorfairRioC8d3,
    /// <summary><c>lower_norfair_rio_c8e9</c>, an installed spritemap.</summary>
    LowerNorfairRioC8e9,
    /// <summary><c>lower_norfair_rio_c8ff</c>, an installed spritemap.</summary>
    LowerNorfairRioC8ff,
    /// <summary><c>lower_norfair_rio_c915</c>, an installed spritemap.</summary>
    LowerNorfairRioC915,
    /// <summary><c>lower_norfair_rio_c92b</c>, an installed spritemap.</summary>
    LowerNorfairRioC92b,
    /// <summary><c>lower_norfair_rio_c941</c>, an installed spritemap.</summary>
    LowerNorfairRioC941,
    /// <summary><c>lower_norfair_rio_c957</c>, an installed spritemap.</summary>
    LowerNorfairRioC957,
    /// <summary><c>lower_norfair_rio_c96d</c>, an installed spritemap.</summary>
    LowerNorfairRioC96d,
    /// <summary><c>lower_norfair_rio_c983</c>, an installed spritemap.</summary>
    LowerNorfairRioC983,
    /// <summary><c>lower_norfair_rio_c999</c>, an installed spritemap.</summary>
    LowerNorfairRioC999,
    /// <summary><c>lower_norfair_rio_c9af</c>, an installed spritemap.</summary>
    LowerNorfairRioC9af,
    /// <summary><c>lower_norfair_rio_c9c5</c>, an installed spritemap.</summary>
    LowerNorfairRioC9c5,
    /// <summary><c>lower_norfair_rio_c9db</c>, an installed spritemap.</summary>
    LowerNorfairRioC9db,
    /// <summary><c>lower_norfair_rio_c9f1</c>, an installed spritemap.</summary>
    LowerNorfairRioC9f1,
    /// <summary><c>lower_norfair_rio_ca07</c>, an installed spritemap.</summary>
    LowerNorfairRioCa07,
    /// <summary><c>lower_norfair_rio_ca13</c>, an installed spritemap.</summary>
    LowerNorfairRioCa13,
    /// <summary><c>lower_norfair_rio_ca1f</c>, an installed spritemap.</summary>
    LowerNorfairRioCa1f,
    /// <summary><c>norfair_rio_c427</c>, an installed spritemap.</summary>
    NorfairRioC427,
    /// <summary><c>norfair_rio_c442</c>, an installed spritemap.</summary>
    NorfairRioC442,
    /// <summary><c>norfair_rio_c45d</c>, an installed spritemap.</summary>
    NorfairRioC45d,
    /// <summary><c>norfair_rio_c493</c>, an installed spritemap.</summary>
    NorfairRioC493,
    /// <summary><c>norfair_rio_c49f</c>, an installed spritemap.</summary>
    NorfairRioC49f,
    /// <summary><c>norfair_rio_c4ab</c>, an installed spritemap.</summary>
    NorfairRioC4ab,
    /// <summary><c>norfair_rio_c4b7</c>, an installed spritemap.</summary>
    NorfairRioC4b7,
    /// <summary><c>norfair_rio_c4d2</c>, an installed spritemap.</summary>
    NorfairRioC4d2,
    /// <summary><c>norfair_rio_c4ed</c>, an installed spritemap.</summary>
    NorfairRioC4ed,
    /// <summary><c>norfair_rio_c508</c>, an installed spritemap.</summary>
    NorfairRioC508,
    /// <summary><c>norfair_rio_c523</c>, an installed spritemap.</summary>
    NorfairRioC523,
    /// <summary><c>norfair_rio_c534</c>, an installed spritemap.</summary>
    NorfairRioC534,
    /// <summary><c>norfair_rio_c54f</c>, an installed spritemap.</summary>
    NorfairRioC54f,
    /// <summary><c>norfair_rio_c56a</c>, an installed spritemap.</summary>
    NorfairRioC56a,
    /// <summary><c>norfair_rio_c585</c>, an installed spritemap.</summary>
    NorfairRioC585,
    /// <summary><c>norfair_rio_c5a0</c>, an installed spritemap.</summary>
    NorfairRioC5a0,
    /// <summary><c>norfair_rio_c5bb</c>, an installed spritemap.</summary>
    NorfairRioC5bb,
    /// <summary><c>norfair_rio_c5d6</c>, an installed spritemap.</summary>
    NorfairRioC5d6,
    /// <summary><c>norfair_rio_c5e2</c>, an installed spritemap.</summary>
    NorfairRioC5e2,
    /// <summary><c>norfair_rio_c5ee</c>, an installed spritemap.</summary>
    NorfairRioC5ee,
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
    RoomSpriteB4Bda6,
    /// <summary><c>room_sprite_b4_c630</c>, an installed spritemap.</summary>
    RoomSpriteB4C630,
    /// <summary><c>room_sprite_b4_c637</c>, an installed spritemap.</summary>
    RoomSpriteB4C637,
    /// <summary><c>room_sprite_b4_c63e</c>, an installed spritemap.</summary>
    RoomSpriteB4C63e,
    /// <summary><c>room_sprite_b4_c645</c>, an installed spritemap.</summary>
    RoomSpriteB4C645,
    /// <summary><c>room_sprite_b4_c64c</c>, an installed spritemap.</summary>
    RoomSpriteB4C64c,
    /// <summary><c>room_sprite_b4_c653</c>, an installed spritemap.</summary>
    RoomSpriteB4C653,
    /// <summary><c>room_sprite_b4_c65a</c>, an installed spritemap.</summary>
    RoomSpriteB4C65a,
    /// <summary><c>room_sprite_b4_c661</c>, an installed spritemap.</summary>
    RoomSpriteB4C661,
    /// <summary><c>room_sprite_b4_c668</c>, an installed spritemap.</summary>
    RoomSpriteB4C668,
    /// <summary><c>room_sprite_b4_c66f</c>, an installed spritemap.</summary>
    RoomSpriteB4C66f,
    /// <summary><c>room_sprite_b4_c676</c>, an installed spritemap.</summary>
    RoomSpriteB4C676,
    /// <summary><c>room_sprite_b4_c67d</c>, an installed spritemap.</summary>
    RoomSpriteB4C67d,
    /// <summary><c>room_sprite_b4_c684</c>, an installed spritemap.</summary>
    RoomSpriteB4C684,
    /// <summary><c>room_sprite_b4_c68b</c>, an installed spritemap.</summary>
    RoomSpriteB4C68b,
    /// <summary><c>room_sprite_b4_c692</c>, an installed spritemap.</summary>
    RoomSpriteB4C692,
    /// <summary><c>room_sprite_b4_c699</c>, an installed spritemap.</summary>
    RoomSpriteB4C699,
    /// <summary><c>room_sprite_b4_c6a0</c>, an installed spritemap.</summary>
    RoomSpriteB4C6a0,
    /// <summary><c>room_sprite_b4_c6a7</c>, an installed spritemap.</summary>
    RoomSpriteB4C6a7,
    /// <summary><c>room_sprite_b4_c6ae</c>, an installed spritemap.</summary>
    RoomSpriteB4C6ae,
    /// <summary><c>room_sprite_b4_c6b5</c>, an installed spritemap.</summary>
    RoomSpriteB4C6b5,
    /// <summary><c>room_sprite_b4_c6bc</c>, an installed spritemap.</summary>
    RoomSpriteB4C6bc,
    /// <summary><c>room_sprite_b4_c6c3</c>, an installed spritemap.</summary>
    RoomSpriteB4C6c3,
    /// <summary><c>room_sprite_b4_c6ca</c>, an installed spritemap.</summary>
    RoomSpriteB4C6ca,
    /// <summary><c>room_sprite_b4_c6d1</c>, an installed spritemap.</summary>
    RoomSpriteB4C6d1,
    /// <summary><c>room_sprite_b4_c6d8</c>, an installed spritemap.</summary>
    RoomSpriteB4C6d8,
    /// <summary><c>room_sprite_b4_c6ee</c>, an installed spritemap.</summary>
    RoomSpriteB4C6ee,
    /// <summary><c>room_sprite_b4_c704</c>, an installed spritemap.</summary>
    RoomSpriteB4C704,
    /// <summary><c>room_sprite_b4_c71a</c>, an installed spritemap.</summary>
    RoomSpriteB4C71a,
    /// <summary><c>room_sprite_b4_c730</c>, an installed spritemap.</summary>
    RoomSpriteB4C730,
    /// <summary><c>room_sprite_b4_c746</c>, an installed spritemap.</summary>
    RoomSpriteB4C746,
    /// <summary><c>room_sprite_b4_c75c</c>, an installed spritemap.</summary>
    RoomSpriteB4C75c,
    /// <summary><c>room_sprite_b4_c772</c>, an installed spritemap.</summary>
    RoomSpriteB4C772,
    /// <summary><c>room_sprite_b4_c788</c>, an installed spritemap.</summary>
    RoomSpriteB4C788,
    /// <summary><c>room_sprite_b4_c79e</c>, an installed spritemap.</summary>
    RoomSpriteB4C79e,
    /// <summary><c>room_sprite_b4_c7b4</c>, an installed spritemap.</summary>
    RoomSpriteB4C7b4,
    /// <summary><c>room_sprite_b4_c7ca</c>, an installed spritemap.</summary>
    RoomSpriteB4C7ca,
    /// <summary><c>room_sprite_b4_c7e0</c>, an installed spritemap.</summary>
    RoomSpriteB4C7e0,
    /// <summary><c>room_sprite_b4_c7f6</c>, an installed spritemap.</summary>
    RoomSpriteB4C7f6,
    /// <summary><c>room_sprite_b4_c80c</c>, an installed spritemap.</summary>
    RoomSpriteB4C80c,
    /// <summary><c>room_sprite_b4_c822</c>, an installed spritemap.</summary>
    RoomSpriteB4C822,
    /// <summary><c>room_sprite_b4_c838</c>, an installed spritemap.</summary>
    RoomSpriteB4C838,
    /// <summary><c>room_sprite_b4_c83f</c>, an installed spritemap.</summary>
    RoomSpriteB4C83f,
    /// <summary><c>room_sprite_b4_c84b</c>, an installed spritemap.</summary>
    RoomSpriteB4C84b,
    /// <summary><c>room_sprite_b4_c85c</c>, an installed spritemap.</summary>
    RoomSpriteB4C85c,
    /// <summary><c>room_sprite_b4_c872</c>, an installed spritemap.</summary>
    RoomSpriteB4C872,
    /// <summary><c>room_sprite_b4_c888</c>, an installed spritemap.</summary>
    RoomSpriteB4C888,
    /// <summary><c>room_sprite_b4_c899</c>, an installed spritemap.</summary>
    RoomSpriteB4C899,
    /// <summary><c>room_sprite_b4_c8a5</c>, an installed spritemap.</summary>
    RoomSpriteB4C8a5,
    /// <summary><c>room_sprite_b4_c8ac</c>, an installed spritemap.</summary>
    RoomSpriteB4C8ac,
    /// <summary><c>room_sprite_b4_c8b3</c>, an installed spritemap.</summary>
    RoomSpriteB4C8b3,
    /// <summary><c>room_sprite_b4_c8ba</c>, an installed spritemap.</summary>
    RoomSpriteB4C8ba,
    /// <summary><c>room_sprite_b4_c8c6</c>, an installed spritemap.</summary>
    RoomSpriteB4C8c6,
    /// <summary><c>room_sprite_b4_c8d2</c>, an installed spritemap.</summary>
    RoomSpriteB4C8d2,
    /// <summary><c>room_sprite_b4_c8e3</c>, an installed spritemap.</summary>
    RoomSpriteB4C8e3,
    /// <summary><c>room_sprite_b4_c8f4</c>, an installed spritemap.</summary>
    RoomSpriteB4C8f4,
    /// <summary><c>room_sprite_b4_c90a</c>, an installed spritemap.</summary>
    RoomSpriteB4C90a,
    /// <summary><c>room_sprite_b4_c9a0</c>, an installed spritemap.</summary>
    RoomSpriteB4C9a0,
    /// <summary><c>room_sprite_b4_c9b6</c>, an installed spritemap.</summary>
    RoomSpriteB4C9b6,
    /// <summary><c>room_sprite_b4_c9cc</c>, an installed spritemap.</summary>
    RoomSpriteB4C9cc,
    /// <summary><c>room_sprite_b4_c9e2</c>, an installed spritemap.</summary>
    RoomSpriteB4C9e2,
    /// <summary><c>room_sprite_b4_c9f8</c>, an installed spritemap.</summary>
    RoomSpriteB4C9f8,
    /// <summary><c>room_sprite_b4_ca0e</c>, an installed spritemap.</summary>
    RoomSpriteB4Ca0e,
    /// <summary><c>room_sprite_b4_ca24</c>, an installed spritemap.</summary>
    RoomSpriteB4Ca24,
    /// <summary><c>room_sprite_b4_ca3a</c>, an installed spritemap.</summary>
    RoomSpriteB4Ca3a,
    /// <summary><c>room_sprite_b4_ca50</c>, an installed spritemap.</summary>
    RoomSpriteB4Ca50,
    /// <summary><c>room_sprite_b4_ca5c</c>, an installed spritemap.</summary>
    RoomSpriteB4Ca5c,
    /// <summary><c>room_sprite_b4_ca63</c>, an installed spritemap.</summary>
    RoomSpriteB4Ca63,
    /// <summary><c>room_sprite_b4_ca6f</c>, an installed spritemap.</summary>
    RoomSpriteB4Ca6f,
    /// <summary><c>room_sprite_b4_ca7b</c>, an installed spritemap.</summary>
    RoomSpriteB4Ca7b,
    /// <summary><c>room_sprite_b4_ca8c</c>, an installed spritemap.</summary>
    RoomSpriteB4Ca8c,
    /// <summary><c>room_sprite_b4_ca9d</c>, an installed spritemap.</summary>
    RoomSpriteB4Ca9d,
    /// <summary><c>room_sprite_b4_cab3</c>, an installed spritemap.</summary>
    RoomSpriteB4Cab3,
    /// <summary><c>room_sprite_b4_cac9</c>, an installed spritemap.</summary>
    RoomSpriteB4Cac9,
    /// <summary><c>room_sprite_b4_cad0</c>, an installed spritemap.</summary>
    RoomSpriteB4Cad0,
    /// <summary><c>room_sprite_b4_cad7</c>, an installed spritemap.</summary>
    RoomSpriteB4Cad7,
    /// <summary><c>room_sprite_b4_cade</c>, an installed spritemap.</summary>
    RoomSpriteB4Cade,
    /// <summary><c>room_sprite_b4_caf4</c>, an installed spritemap.</summary>
    RoomSpriteB4Caf4,
    /// <summary><c>room_sprite_b4_cb05</c>, an installed spritemap.</summary>
    RoomSpriteB4Cb05,
    /// <summary><c>room_sprite_b4_cb16</c>, an installed spritemap.</summary>
    RoomSpriteB4Cb16,
    /// <summary><c>room_sprite_b4_cb27</c>, an installed spritemap.</summary>
    RoomSpriteB4Cb27,
    /// <summary><c>room_sprite_b4_cb38</c>, an installed spritemap.</summary>
    RoomSpriteB4Cb38,
    /// <summary><c>room_sprite_b4_cb49</c>, an installed spritemap.</summary>
    RoomSpriteB4Cb49,
    /// <summary><c>room_sprite_b4_cb5a</c>, an installed spritemap.</summary>
    RoomSpriteB4Cb5a,
    /// <summary><c>room_sprite_b4_cb6b</c>, an installed spritemap.</summary>
    RoomSpriteB4Cb6b,
    /// <summary><c>room_sprite_b4_cb7c</c>, an installed spritemap.</summary>
    RoomSpriteB4Cb7c,
    /// <summary><c>room_sprite_b4_cb8d</c>, an installed spritemap.</summary>
    RoomSpriteB4Cb8d,
    /// <summary><c>room_sprite_b4_cb9e</c>, an installed spritemap.</summary>
    RoomSpriteB4Cb9e,
    /// <summary><c>room_sprite_b4_cbaf</c>, an installed spritemap.</summary>
    RoomSpriteB4Cbaf,
    /// <summary><c>room_sprite_b4_cbc0</c>, an installed spritemap.</summary>
    RoomSpriteB4Cbc0,
    /// <summary><c>room_sprite_b4_cbc7</c>, an installed spritemap.</summary>
    RoomSpriteB4Cbc7,
    /// <summary><c>room_sprite_b4_cbdd</c>, an installed spritemap.</summary>
    RoomSpriteB4Cbdd,
    /// <summary><c>room_sprite_b4_cbf3</c>, an installed spritemap.</summary>
    RoomSpriteB4Cbf3,
    /// <summary><c>room_sprite_b4_cc09</c>, an installed spritemap.</summary>
    RoomSpriteB4Cc09,
    /// <summary><c>room_sprite_b4_cc1f</c>, an installed spritemap.</summary>
    RoomSpriteB4Cc1f,
    /// <summary><c>room_sprite_b4_cc35</c>, an installed spritemap.</summary>
    RoomSpriteB4Cc35,
    /// <summary><c>room_sprite_b4_cc4b</c>, an installed spritemap.</summary>
    RoomSpriteB4Cc4b,
    /// <summary><c>room_sprite_b4_cc61</c>, an installed spritemap.</summary>
    RoomSpriteB4Cc61,
    /// <summary><c>room_sprite_b4_cc77</c>, an installed spritemap.</summary>
    RoomSpriteB4Cc77,
    /// <summary><c>room_sprite_b4_cc8d</c>, an installed spritemap.</summary>
    RoomSpriteB4Cc8d,
    /// <summary><c>room_sprite_b4_ccb9</c>, an installed spritemap.</summary>
    RoomSpriteB4Ccb9,
    /// <summary><c>room_sprite_b4_ccc0</c>, an installed spritemap.</summary>
    RoomSpriteB4Ccc0,
    /// <summary><c>room_sprite_b4_ccc7</c>, an installed spritemap.</summary>
    RoomSpriteB4Ccc7,
    /// <summary><c>room_sprite_b4_ccce</c>, an installed spritemap.</summary>
    RoomSpriteB4Ccce,
    /// <summary><c>room_sprite_b4_ccd5</c>, an installed spritemap.</summary>
    RoomSpriteB4Ccd5,
    /// <summary><c>room_sprite_b4_ccdc</c>, an installed spritemap.</summary>
    RoomSpriteB4Ccdc,
    /// <summary><c>room_sprite_b4_cce3</c>, an installed spritemap.</summary>
    RoomSpriteB4Cce3,
    /// <summary><c>room_sprite_b4_ccf1</c>, an installed spritemap.</summary>
    RoomSpriteB4Ccf1,
    /// <summary><c>room_sprite_b4_ccf8</c>, an installed spritemap.</summary>
    RoomSpriteB4Ccf8,
    /// <summary><c>room_sprite_b4_ccff</c>, an installed spritemap.</summary>
    RoomSpriteB4Ccff,
    /// <summary><c>room_sprite_b4_cd15</c>, an installed spritemap.</summary>
    RoomSpriteB4Cd15,
    /// <summary><c>room_sprite_b4_cd2b</c>, an installed spritemap.</summary>
    RoomSpriteB4Cd2b,
    /// <summary><c>room_sprite_b4_cd41</c>, an installed spritemap.</summary>
    RoomSpriteB4Cd41,
    /// <summary><c>room_sprite_b4_cd57</c>, an installed spritemap.</summary>
    RoomSpriteB4Cd57,
    /// <summary><c>room_sprite_b4_cd6d</c>, an installed spritemap.</summary>
    RoomSpriteB4Cd6d,
    /// <summary><c>room_sprite_b4_cd99</c>, an installed spritemap.</summary>
    RoomSpriteB4Cd99,
    /// <summary><c>room_sprite_b4_cda0</c>, an installed spritemap.</summary>
    RoomSpriteB4Cda0,
    /// <summary><c>room_sprite_b4_cda7</c>, an installed spritemap.</summary>
    RoomSpriteB4Cda7,
    /// <summary><c>room_sprite_b4_cdae</c>, an installed spritemap.</summary>
    RoomSpriteB4Cdae,
    /// <summary><c>room_sprite_b4_cdb5</c>, an installed spritemap.</summary>
    RoomSpriteB4Cdb5,
    /// <summary><c>room_sprite_b4_cdbc</c>, an installed spritemap.</summary>
    RoomSpriteB4Cdbc,
    /// <summary><c>room_sprite_b4_cdc3</c>, an installed spritemap.</summary>
    RoomSpriteB4Cdc3,
    /// <summary><c>room_sprite_b4_cdca</c>, an installed spritemap.</summary>
    RoomSpriteB4Cdca,
    /// <summary><c>room_sprite_b4_cdd1</c>, an installed spritemap.</summary>
    RoomSpriteB4Cdd1,
    /// <summary><c>room_sprite_b4_cdd8</c>, an installed spritemap.</summary>
    RoomSpriteB4Cdd8,
    /// <summary><c>room_sprite_b4_cddf</c>, an installed spritemap.</summary>
    RoomSpriteB4Cddf,
    /// <summary><c>room_sprite_b4_cde6</c>, an installed spritemap.</summary>
    RoomSpriteB4Cde6,
    /// <summary><c>room_sprite_b4_cded</c>, an installed spritemap.</summary>
    RoomSpriteB4Cded,
    /// <summary><c>room_sprite_b4_cdf4</c>, an installed spritemap.</summary>
    RoomSpriteB4Cdf4,
    /// <summary><c>room_sprite_b4_cdfb</c>, an installed spritemap.</summary>
    RoomSpriteB4Cdfb,
    /// <summary><c>room_sprite_b4_ce02</c>, an installed spritemap.</summary>
    RoomSpriteB4Ce02,
    /// <summary><c>room_sprite_b4_ce1e</c>, an installed spritemap.</summary>
    RoomSpriteB4Ce1e,
    /// <summary><c>room_sprite_b4_ce25</c>, an installed spritemap.</summary>
    RoomSpriteB4Ce25,
    /// <summary><c>room_sprite_b4_ce2c</c>, an installed spritemap.</summary>
    RoomSpriteB4Ce2c,
    /// <summary><c>room_sprite_b4_ce33</c>, an installed spritemap.</summary>
    RoomSpriteB4Ce33,
    /// <summary><c>room_sprite_b4_ce3a</c>, an installed spritemap.</summary>
    RoomSpriteB4Ce3a,
    /// <summary><c>room_sprite_b4_ce50</c>, an installed spritemap.</summary>
    RoomSpriteB4Ce50,
    /// <summary><c>room_sprite_b4_ce66</c>, an installed spritemap.</summary>
    RoomSpriteB4Ce66,
    /// <summary><c>room_sprite_b4_ce7c</c>, an installed spritemap.</summary>
    RoomSpriteB4Ce7c,
    /// <summary><c>room_sprite_b4_ce83</c>, an installed spritemap.</summary>
    RoomSpriteB4Ce83,
    /// <summary><c>room_sprite_b4_ce8a</c>, an installed spritemap.</summary>
    RoomSpriteB4Ce8a,
    /// <summary><c>room_sprite_b4_ce91</c>, an installed spritemap.</summary>
    RoomSpriteB4Ce91,
    /// <summary><c>room_sprite_b4_ce98</c>, an installed spritemap.</summary>
    RoomSpriteB4Ce98,
    /// <summary><c>room_sprite_b4_cf87</c>, an installed spritemap.</summary>
    RoomSpriteB4Cf87,
    /// <summary><c>room_sprite_b4_cf8e</c>, an installed spritemap.</summary>
    RoomSpriteB4Cf8e,
    /// <summary><c>room_sprite_b4_cf95</c>, an installed spritemap.</summary>
    RoomSpriteB4Cf95,
    /// <summary><c>room_sprite_b4_cfa3</c>, an installed spritemap.</summary>
    RoomSpriteB4Cfa3,
    /// <summary><c>room_sprite_b4_cfb9</c>, an installed spritemap.</summary>
    RoomSpriteB4Cfb9,
    /// <summary><c>room_sprite_b4_cfcf</c>, an installed spritemap.</summary>
    RoomSpriteB4Cfcf,
    /// <summary><c>room_sprite_b4_cfe5</c>, an installed spritemap.</summary>
    RoomSpriteB4Cfe5,
    /// <summary><c>room_sprite_b4_d023</c>, an installed spritemap.</summary>
    RoomSpriteB4D023,
    /// <summary><c>room_sprite_b4_d04d</c>, an installed spritemap.</summary>
    RoomSpriteB4D04d,
    /// <summary><c>room_sprite_b4_d08b</c>, an installed spritemap.</summary>
    RoomSpriteB4D08b,
    /// <summary><c>room_sprite_b4_d0ba</c>, an installed spritemap.</summary>
    RoomSpriteB4D0ba,
    /// <summary><c>room_sprite_b4_d0e9</c>, an installed spritemap.</summary>
    RoomSpriteB4D0e9,
    /// <summary><c>room_sprite_b4_d118</c>, an installed spritemap.</summary>
    RoomSpriteB4D118,
    /// <summary><c>room_sprite_b4_d147</c>, an installed spritemap.</summary>
    RoomSpriteB4D147,
    /// <summary><c>room_sprite_b4_d176</c>, an installed spritemap.</summary>
    RoomSpriteB4D176,
    /// <summary><c>room_sprite_b4_d1d4</c>, an installed spritemap.</summary>
    RoomSpriteB4D1d4,
    /// <summary><c>room_sprite_b4_d1ef</c>, an installed spritemap.</summary>
    RoomSpriteB4D1ef,
    /// <summary><c>room_sprite_b4_d20a</c>, an installed spritemap.</summary>
    RoomSpriteB4D20a,
    /// <summary><c>room_sprite_b4_d225</c>, an installed spritemap.</summary>
    RoomSpriteB4D225,
    /// <summary><c>room_sprite_b4_d245</c>, an installed spritemap.</summary>
    RoomSpriteB4D245,
    /// <summary><c>room_sprite_b4_d2ac</c>, an installed spritemap.</summary>
    RoomSpriteB4D2ac,
    /// <summary><c>room_sprite_b4_d2b3</c>, an installed spritemap.</summary>
    RoomSpriteB4D2b3,
    /// <summary><c>room_sprite_b4_d2e2</c>, an installed spritemap.</summary>
    RoomSpriteB4D2e2,
    /// <summary><c>room_sprite_b4_d311</c>, an installed spritemap.</summary>
    RoomSpriteB4D311,
    /// <summary><c>room_sprite_b4_d340</c>, an installed spritemap.</summary>
    RoomSpriteB4D340,
    /// <summary><c>room_sprite_b4_d36f</c>, an installed spritemap.</summary>
    RoomSpriteB4D36f,
    /// <summary><c>room_sprite_b4_d39e</c>, an installed spritemap.</summary>
    RoomSpriteB4D39e,
    /// <summary><c>room_sprite_b4_d3fc</c>, an installed spritemap.</summary>
    RoomSpriteB4D3fc,
    /// <summary><c>room_sprite_b4_d417</c>, an installed spritemap.</summary>
    RoomSpriteB4D417,
    /// <summary><c>room_sprite_b4_d432</c>, an installed spritemap.</summary>
    RoomSpriteB4D432,
    /// <summary><c>room_sprite_b4_d44d</c>, an installed spritemap.</summary>
    RoomSpriteB4D44d,
    /// <summary><c>room_sprite_b4_d46d</c>, an installed spritemap.</summary>
    RoomSpriteB4D46d,
    /// <summary><c>room_sprite_b4_d4be</c>, an installed spritemap.</summary>
    RoomSpriteB4D4be,
    /// <summary><c>room_sprite_b4_d4c5</c>, an installed spritemap.</summary>
    RoomSpriteB4D4c5,
    /// <summary><c>room_sprite_b4_d4cc</c>, an installed spritemap.</summary>
    RoomSpriteB4D4cc,
    /// <summary><c>room_sprite_b4_d4d3</c>, an installed spritemap.</summary>
    RoomSpriteB4D4d3,
    /// <summary><c>room_sprite_b4_d4da</c>, an installed spritemap.</summary>
    RoomSpriteB4D4da,
    /// <summary><c>room_sprite_b4_d4e1</c>, an installed spritemap.</summary>
    RoomSpriteB4D4e1,
    /// <summary><c>room_sprite_b4_d4e8</c>, an installed spritemap.</summary>
    RoomSpriteB4D4e8,
    /// <summary><c>room_sprite_b4_d4ef</c>, an installed spritemap.</summary>
    RoomSpriteB4D4ef,
    /// <summary><c>room_sprite_b4_d4f6</c>, an installed spritemap.</summary>
    RoomSpriteB4D4f6,
    /// <summary><c>room_sprite_b4_d4fd</c>, an installed spritemap.</summary>
    RoomSpriteB4D4fd,
    /// <summary><c>room_sprite_b4_d504</c>, an installed spritemap.</summary>
    RoomSpriteB4D504,
    /// <summary><c>room_sprite_b4_d50b</c>, an installed spritemap.</summary>
    RoomSpriteB4D50b,
    /// <summary><c>room_sprite_b4_d512</c>, an installed spritemap.</summary>
    RoomSpriteB4D512,
    /// <summary><c>room_sprite_b4_d519</c>, an installed spritemap.</summary>
    RoomSpriteB4D519,
    /// <summary><c>room_sprite_b4_d520</c>, an installed spritemap.</summary>
    RoomSpriteB4D520,
    /// <summary><c>room_sprite_b4_d527</c>, an installed spritemap.</summary>
    RoomSpriteB4D527,
    /// <summary><c>room_sprite_b4_d52e</c>, an installed spritemap.</summary>
    RoomSpriteB4D52e,
    /// <summary><c>room_sprite_b4_d535</c>, an installed spritemap.</summary>
    RoomSpriteB4D535,
    /// <summary><c>room_sprite_b4_d53c</c>, an installed spritemap.</summary>
    RoomSpriteB4D53c,
    /// <summary><c>room_sprite_b4_d543</c>, an installed spritemap.</summary>
    RoomSpriteB4D543,
    /// <summary><c>room_sprite_b4_d54a</c>, an installed spritemap.</summary>
    RoomSpriteB4D54a,
    /// <summary><c>room_sprite_b4_d551</c>, an installed spritemap.</summary>
    RoomSpriteB4D551,
    /// <summary><c>room_sprite_b4_d558</c>, an installed spritemap.</summary>
    RoomSpriteB4D558,
    /// <summary><c>room_sprite_b4_d564</c>, an installed spritemap.</summary>
    RoomSpriteB4D564,
    /// <summary><c>room_sprite_b4_d570</c>, an installed spritemap.</summary>
    RoomSpriteB4D570,
    /// <summary><c>room_sprite_b4_d588</c>, an installed spritemap.</summary>
    RoomSpriteB4D588,
    /// <summary><c>room_sprite_b4_d594</c>, an installed spritemap.</summary>
    RoomSpriteB4D594,
    /// <summary><c>room_sprite_b4_d59b</c>, an installed spritemap.</summary>
    RoomSpriteB4D59b,
    /// <summary><c>room_sprite_b4_d5a2</c>, an installed spritemap.</summary>
    RoomSpriteB4D5a2,
    /// <summary><c>room_sprite_b4_d5a9</c>, an installed spritemap.</summary>
    RoomSpriteB4D5a9,
    /// <summary><c>room_sprite_b4_d5b0</c>, an installed spritemap.</summary>
    RoomSpriteB4D5b0,
    /// <summary><c>room_sprite_b4_d5b7</c>, an installed spritemap.</summary>
    RoomSpriteB4D5b7,
    /// <summary><c>room_sprite_b4_d5eb</c>, an installed spritemap.</summary>
    RoomSpriteB4D5eb,
    /// <summary><c>room_sprite_b4_d61f</c>, an installed spritemap.</summary>
    RoomSpriteB4D61f,
    /// <summary><c>room_sprite_b4_d653</c>, an installed spritemap.</summary>
    RoomSpriteB4D653,
    /// <summary><c>room_sprite_b4_d67d</c>, an installed spritemap.</summary>
    RoomSpriteB4D67d,
    /// <summary><c>room_sprite_b4_d69d</c>, an installed spritemap.</summary>
    RoomSpriteB4D69d,
    /// <summary><c>room_sprite_b4_d6c7</c>, an installed spritemap.</summary>
    RoomSpriteB4D6c7,
    /// <summary><c>room_sprite_b4_d6f1</c>, an installed spritemap.</summary>
    RoomSpriteB4D6f1,
    /// <summary><c>room_sprite_b4_d702</c>, an installed spritemap.</summary>
    RoomSpriteB4D702,
    /// <summary><c>room_sprite_b4_d713</c>, an installed spritemap.</summary>
    RoomSpriteB4D713,
    /// <summary><c>room_sprite_b4_d724</c>, an installed spritemap.</summary>
    RoomSpriteB4D724,
    /// <summary><c>room_sprite_b4_d73a</c>, an installed spritemap.</summary>
    RoomSpriteB4D73a,
    /// <summary><c>room_sprite_b4_d750</c>, an installed spritemap.</summary>
    RoomSpriteB4D750,
    /// <summary><c>room_sprite_b4_d766</c>, an installed spritemap.</summary>
    RoomSpriteB4D766,
    /// <summary><c>room_sprite_b4_d777</c>, an installed spritemap.</summary>
    RoomSpriteB4D777,
    /// <summary><c>room_sprite_b4_d788</c>, an installed spritemap.</summary>
    RoomSpriteB4D788,
    /// <summary><c>room_sprite_b4_d799</c>, an installed spritemap.</summary>
    RoomSpriteB4D799,
    /// <summary><c>room_sprite_b4_d7af</c>, an installed spritemap.</summary>
    RoomSpriteB4D7af,
    /// <summary><c>room_sprite_b4_d7c5</c>, an installed spritemap.</summary>
    RoomSpriteB4D7c5,
    /// <summary><c>room_sprite_b4_d7db</c>, an installed spritemap.</summary>
    RoomSpriteB4D7db,
    /// <summary><c>room_sprite_b4_d7e7</c>, an installed spritemap.</summary>
    RoomSpriteB4D7e7,
    /// <summary><c>room_sprite_b4_d7ee</c>, an installed spritemap.</summary>
    RoomSpriteB4D7ee,
    /// <summary><c>room_sprite_b4_d7fa</c>, an installed spritemap.</summary>
    RoomSpriteB4D7fa,
    /// <summary><c>room_sprite_b4_d806</c>, an installed spritemap.</summary>
    RoomSpriteB4D806,
    /// <summary><c>room_sprite_b4_d812</c>, an installed spritemap.</summary>
    RoomSpriteB4D812,
    /// <summary><c>room_sprite_b4_d81e</c>, an installed spritemap.</summary>
    RoomSpriteB4D81e,
    /// <summary><c>room_sprite_b4_d82a</c>, an installed spritemap.</summary>
    RoomSpriteB4D82a,
    /// <summary><c>room_sprite_b4_d836</c>, an installed spritemap.</summary>
    RoomSpriteB4D836,
    /// <summary><c>room_sprite_b4_d842</c>, an installed spritemap.</summary>
    RoomSpriteB4D842,
    /// <summary><c>room_sprite_b4_d849</c>, an installed spritemap.</summary>
    RoomSpriteB4D849,
    /// <summary><c>room_sprite_b4_d850</c>, an installed spritemap.</summary>
    RoomSpriteB4D850,
    /// <summary><c>room_sprite_b4_d857</c>, an installed spritemap.</summary>
    RoomSpriteB4D857,
    /// <summary><c>room_sprite_b4_d86d</c>, an installed spritemap.</summary>
    RoomSpriteB4D86d,
    /// <summary><c>room_sprite_b4_d883</c>, an installed spritemap.</summary>
    RoomSpriteB4D883,
    /// <summary><c>room_sprite_b4_d899</c>, an installed spritemap.</summary>
    RoomSpriteB4D899,
    /// <summary><c>room_sprite_b4_d8af</c>, an installed spritemap.</summary>
    RoomSpriteB4D8af,
    /// <summary><c>room_sprite_b4_d8b6</c>, an installed spritemap.</summary>
    RoomSpriteB4D8b6,
    /// <summary><c>room_sprite_b4_d8bd</c>, an installed spritemap.</summary>
    RoomSpriteB4D8bd,
    /// <summary><c>room_sprite_b4_d905</c>, an installed spritemap.</summary>
    RoomSpriteB4D905,
    /// <summary><c>room_sprite_b4_d957</c>, an installed spritemap.</summary>
    RoomSpriteB4D957,
    /// <summary><c>room_sprite_b4_d9a9</c>, an installed spritemap.</summary>
    RoomSpriteB4D9a9,
    /// <summary><c>room_sprite_b4_d9fb</c>, an installed spritemap.</summary>
    RoomSpriteB4D9fb,
    /// <summary><c>room_sprite_b4_da48</c>, an installed spritemap.</summary>
    RoomSpriteB4Da48,
    /// <summary><c>room_sprite_b4_da90</c>, an installed spritemap.</summary>
    RoomSpriteB4Da90,
    /// <summary><c>room_sprite_b4_dad8</c>, an installed spritemap.</summary>
    RoomSpriteB4Dad8,
    /// <summary><c>room_sprite_b4_dc3f</c>, an installed spritemap.</summary>
    RoomSpriteB4Dc3f,
    /// <summary><c>room_sprite_b4_dc7d</c>, an installed spritemap.</summary>
    RoomSpriteB4Dc7d,
    /// <summary><c>room_sprite_b4_dcbb</c>, an installed spritemap.</summary>
    RoomSpriteB4Dcbb,
    /// <summary><c>room_sprite_b4_dcf9</c>, an installed spritemap.</summary>
    RoomSpriteB4Dcf9,
    /// <summary><c>room_sprite_b4_dd51</c>, an installed spritemap.</summary>
    RoomSpriteB4Dd51,
    /// <summary><c>room_sprite_b4_dd58</c>, an installed spritemap.</summary>
    RoomSpriteB4Dd58,
    /// <summary><c>room_sprite_b4_dd5f</c>, an installed spritemap.</summary>
    RoomSpriteB4Dd5f,
    /// <summary><c>room_sprite_b4_dd66</c>, an installed spritemap.</summary>
    RoomSpriteB4Dd66,
    /// <summary><c>room_sprite_b4_dd6d</c>, an installed spritemap.</summary>
    RoomSpriteB4Dd6d,
    /// <summary><c>room_sprite_b4_dd74</c>, an installed spritemap.</summary>
    RoomSpriteB4Dd74,
    /// <summary><c>room_sprite_b4_dd7b</c>, an installed spritemap.</summary>
    RoomSpriteB4Dd7b,
    /// <summary><c>room_sprite_b4_dd82</c>, an installed spritemap.</summary>
    RoomSpriteB4Dd82,
    /// <summary><c>yapping_maw_a8_a8ee</c>, an installed spritemap.</summary>
    YappingMawA8A8ee,
    /// <summary><c>yapping_maw_a8_a909</c>, an installed spritemap.</summary>
    YappingMawA8A909,
    /// <summary><c>yapping_maw_a8_a91a</c>, an installed spritemap.</summary>
    YappingMawA8A91a,
    /// <summary><c>yapping_maw_a8_a935</c>, an installed spritemap.</summary>
    YappingMawA8A935,
    /// <summary><c>yapping_maw_a8_a946</c>, an installed spritemap.</summary>
    YappingMawA8A946,
    /// <summary><c>yapping_maw_a8_a961</c>, an installed spritemap.</summary>
    YappingMawA8A961,
    /// <summary><c>yapping_maw_a8_a972</c>, an installed spritemap.</summary>
    YappingMawA8A972,
    /// <summary><c>yapping_maw_a8_a98d</c>, an installed spritemap.</summary>
    YappingMawA8A98d,
    /// <summary><c>yapping_maw_a8_a99e</c>, an installed spritemap.</summary>
    YappingMawA8A99e,
    /// <summary><c>yapping_maw_a8_a9b9</c>, an installed spritemap.</summary>
    YappingMawA8A9b9,
    /// <summary><c>yapping_maw_a8_a9ca</c>, an installed spritemap.</summary>
    YappingMawA8A9ca,
    /// <summary><c>yapping_maw_a8_a9e5</c>, an installed spritemap.</summary>
    YappingMawA8A9e5,
    /// <summary><c>yapping_maw_a8_a9f6</c>, an installed spritemap.</summary>
    YappingMawA8A9f6,
    /// <summary><c>yapping_maw_a8_aa11</c>, an installed spritemap.</summary>
    YappingMawA8Aa11,
    /// <summary><c>yapping_maw_a8_aa22</c>, an installed spritemap.</summary>
    YappingMawA8Aa22,
    /// <summary><c>yapping_maw_a8_aa3d</c>, an installed spritemap.</summary>
    YappingMawA8Aa3d,
    /// <summary><c>yapping_maw_a8_aa4e</c>, an installed spritemap.</summary>
    YappingMawA8Aa4e,
    /// <summary><c>yapping_maw_a8_aa69</c>, an installed spritemap.</summary>
    YappingMawA8Aa69,
    /// <summary><c>yapping_maw_a8_aa7a</c>, an installed spritemap.</summary>
    YappingMawA8Aa7a,
    /// <summary><c>yapping_maw_a8_aa95</c>, an installed spritemap.</summary>
    YappingMawA8Aa95,
    /// <summary><c>yapping_maw_a8_aaa6</c>, an installed spritemap.</summary>
    YappingMawA8Aaa6,
    /// <summary><c>yapping_maw_a8_aac1</c>, an installed spritemap.</summary>
    YappingMawA8Aac1,
    /// <summary><c>yapping_maw_a8_aad2</c>, an installed spritemap.</summary>
    YappingMawA8Aad2,
    /// <summary><c>yapping_maw_a8_aaed</c>, an installed spritemap.</summary>
    YappingMawA8Aaed,
    /// <summary><c>ki_hunter_a8_eb2e</c>, an installed spritemap.</summary>
    KiHunterA8Eb2e,
    /// <summary><c>ki_hunter_a8_eb58</c>, an installed spritemap.</summary>
    KiHunterA8Eb58,
    /// <summary><c>ki_hunter_a8_eb82</c>, an installed spritemap.</summary>
    KiHunterA8Eb82,
    /// <summary><c>ki_hunter_a8_ebac</c>, an installed spritemap.</summary>
    KiHunterA8Ebac,
    /// <summary><c>ki_hunter_a8_ebc2</c>, an installed spritemap.</summary>
    KiHunterA8Ebc2,
    /// <summary><c>ki_hunter_a8_ebce</c>, an installed spritemap.</summary>
    KiHunterA8Ebce,
    /// <summary><c>ki_hunter_a8_ebf7</c>, an installed spritemap.</summary>
    KiHunterA8Ebf7,
    /// <summary><c>ki_hunter_a8_ec21</c>, an installed spritemap.</summary>
    KiHunterA8Ec21,
    /// <summary><c>ki_hunter_a8_ec4b</c>, an installed spritemap.</summary>
    KiHunterA8Ec4b,
    /// <summary><c>ki_hunter_a8_ec75</c>, an installed spritemap.</summary>
    KiHunterA8Ec75,
    /// <summary><c>ki_hunter_a8_ec8b</c>, an installed spritemap.</summary>
    KiHunterA8Ec8b,
    /// <summary><c>ki_hunter_a8_ec97</c>, an installed spritemap.</summary>
    KiHunterA8Ec97,
    /// <summary><c>ki_hunter_a8_eca8</c>, an installed spritemap.</summary>
    KiHunterA8Eca8,
    /// <summary><c>ki_hunter_a8_ecc0</c>, an installed spritemap.</summary>
    KiHunterA8Ecc0,
    /// <summary><c>ki_hunter_a8_ecea</c>, an installed spritemap.</summary>
    KiHunterA8Ecea,
    /// <summary><c>ki_hunter_a8_ed14</c>, an installed spritemap.</summary>
    KiHunterA8Ed14,
    /// <summary><c>ki_hunter_a8_ed3e</c>, an installed spritemap.</summary>
    KiHunterA8Ed3e,
    /// <summary><c>ki_hunter_a8_ed68</c>, an installed spritemap.</summary>
    KiHunterA8Ed68,
    /// <summary><c>ki_hunter_a8_ed92</c>, an installed spritemap.</summary>
    KiHunterA8Ed92,
    /// <summary><c>ki_hunter_a8_edbc</c>, an installed spritemap.</summary>
    KiHunterA8Edbc,
    /// <summary><c>ki_hunter_a8_ede6</c>, an installed spritemap.</summary>
    KiHunterA8Ede6,
    /// <summary><c>ki_hunter_a8_ee10</c>, an installed spritemap.</summary>
    KiHunterA8Ee10,
    /// <summary><c>ki_hunter_a8_ee3a</c>, an installed spritemap.</summary>
    KiHunterA8Ee3a,
    /// <summary><c>ki_hunter_a8_ee64</c>, an installed spritemap.</summary>
    KiHunterA8Ee64,
    /// <summary><c>ki_hunter_a8_ee8e</c>, an installed spritemap.</summary>
    KiHunterA8Ee8e,
    /// <summary><c>ki_hunter_a8_eeb8</c>, an installed spritemap.</summary>
    KiHunterA8Eeb8,
    /// <summary><c>ki_hunter_a8_eee2</c>, an installed spritemap.</summary>
    KiHunterA8Eee2,
    /// <summary><c>ki_hunter_a8_ef0c</c>, an installed spritemap.</summary>
    KiHunterA8Ef0c,
    /// <summary><c>ki_hunter_a8_ef36</c>, an installed spritemap.</summary>
    KiHunterA8Ef36,
    /// <summary><c>ki_hunter_a8_ef60</c>, an installed spritemap.</summary>
    KiHunterA8Ef60,
    /// <summary><c>ki_hunter_a8_ef8a</c>, an installed spritemap.</summary>
    KiHunterA8Ef8a,
    /// <summary><c>ki_hunter_a8_efb4</c>, an installed spritemap.</summary>
    KiHunterA8Efb4,
    /// <summary><c>ki_hunter_a8_efde</c>, an installed spritemap.</summary>
    KiHunterA8Efde,
    /// <summary><c>ki_hunter_a8_f00d</c>, an installed spritemap.</summary>
    KiHunterA8F00d,
    /// <summary><c>ki_hunter_a8_f03c</c>, an installed spritemap.</summary>
    KiHunterA8F03c,
    /// <summary><c>ki_hunter_a8_f06b</c>, an installed spritemap.</summary>
    KiHunterA8F06b,
    /// <summary><c>ki_hunter_a8_f09a</c>, an installed spritemap.</summary>
    KiHunterA8F09a,
    /// <summary><c>ki_hunter_a8_f0c4</c>, an installed spritemap.</summary>
    KiHunterA8F0c4,
    /// <summary><c>ki_hunter_a8_f0f3</c>, an installed spritemap.</summary>
    KiHunterA8F0f3,
    /// <summary><c>ki_hunter_a8_f122</c>, an installed spritemap.</summary>
    KiHunterA8F122,
    /// <summary><c>ki_hunter_a8_f151</c>, an installed spritemap.</summary>
    KiHunterA8F151,
    /// <summary><c>mother_brain_a9_a586</c>, an installed spritemap.</summary>
    MotherBrainA9A586,
    /// <summary><c>mother_brain_a9_a5bf</c>, an installed spritemap.</summary>
    MotherBrainA9A5bf,
    /// <summary><c>mother_brain_a9_a5f8</c>, an installed spritemap.</summary>
    MotherBrainA9A5f8,
    /// <summary><c>mother_brain_a9_a62c</c>, an installed spritemap.</summary>
    MotherBrainA9A62c,
    /// <summary><c>mother_brain_a9_a660</c>, an installed spritemap.</summary>
    MotherBrainA9A660,
    /// <summary><c>mother_brain_a9_a694</c>, an installed spritemap.</summary>
    MotherBrainA9A694,
    /// <summary><c>mother_brain_a9_a69b</c>, an installed spritemap.</summary>
    MotherBrainA9A69b,
    /// <summary><c>mother_brain_a9_a6d9</c>, an installed spritemap.</summary>
    MotherBrainA9A6d9,
    /// <summary><c>mother_brain_a9_a717</c>, an installed spritemap.</summary>
    MotherBrainA9A717,
    /// <summary><c>mother_brain_a9_a750</c>, an installed spritemap.</summary>
    MotherBrainA9A750,
    /// <summary><c>mother_brain_a9_a789</c>, an installed spritemap.</summary>
    MotherBrainA9A789,
    /// <summary><c>mother_brain_a9_ad3e</c>, an installed spritemap.</summary>
    MotherBrainA9Ad3e,
    /// <summary><c>mother_brain_a9_ad6d</c>, an installed spritemap.</summary>
    MotherBrainA9Ad6d,
    /// <summary><c>mother_brain_a9_ada1</c>, an installed spritemap.</summary>
    MotherBrainA9Ada1,
    /// <summary><c>mother_brain_a9_add5</c>, an installed spritemap.</summary>
    MotherBrainA9Add5,
    /// <summary><c>mother_brain_a9_ae09</c>, an installed spritemap.</summary>
    MotherBrainA9Ae09,
    /// <summary><c>mother_brain_a9_ae33</c>, an installed spritemap.</summary>
    MotherBrainA9Ae33,
    /// <summary><c>mother_brain_a9_ae5d</c>, an installed spritemap.</summary>
    MotherBrainA9Ae5d,
    /// <summary><c>dead_torizo_corpse_a9_d761</c>, an installed spritemap.</summary>
    DeadTorizoCorpseA9D761,
    /// <summary><c>ridley_supplement_a6_dc90</c>, an installed spritemap.</summary>
    RidleySupplementA6Dc90,
    /// <summary><c>ridley_supplement_a6_dc97</c>, an installed spritemap.</summary>
    RidleySupplementA6Dc97,
    /// <summary><c>ridley_supplement_a6_dc9e</c>, an installed spritemap.</summary>
    RidleySupplementA6Dc9e,
    /// <summary><c>ridley_supplement_a6_dcda</c>, an installed spritemap.</summary>
    RidleySupplementA6Dcda,
    /// <summary><c>ridley_supplement_a6_dce1</c>, an installed spritemap.</summary>
    RidleySupplementA6Dce1,
    /// <summary><c>ridley_supplement_a6_dce8</c>, an installed spritemap.</summary>
    RidleySupplementA6Dce8,
    /// <summary><c>ridley_supplement_a6_dcef</c>, an installed spritemap.</summary>
    RidleySupplementA6Dcef,
    /// <summary><c>ridley_supplement_a6_dcf6</c>, an installed spritemap.</summary>
    RidleySupplementA6Dcf6,
    /// <summary><c>ridley_supplement_a6_dcfd</c>, an installed spritemap.</summary>
    RidleySupplementA6Dcfd,
    /// <summary><c>ridley_supplement_a6_dd04</c>, an installed spritemap.</summary>
    RidleySupplementA6Dd04,
    /// <summary><c>ridley_supplement_a6_dd0b</c>, an installed spritemap.</summary>
    RidleySupplementA6Dd0b,
    /// <summary><c>ridley_supplement_a6_dd12</c>, an installed spritemap.</summary>
    RidleySupplementA6Dd12,
    /// <summary><c>ridley_supplement_a6_dd19</c>, an installed spritemap.</summary>
    RidleySupplementA6Dd19,
    /// <summary><c>ridley_supplement_a6_dd20</c>, an installed spritemap.</summary>
    RidleySupplementA6Dd20,
    /// <summary><c>ridley_supplement_a6_dd27</c>, an installed spritemap.</summary>
    RidleySupplementA6Dd27,
    /// <summary><c>ridley_supplement_a6_dd2e</c>, an installed spritemap.</summary>
    RidleySupplementA6Dd2e,
    /// <summary><c>ridley_supplement_a6_dd35</c>, an installed spritemap.</summary>
    RidleySupplementA6Dd35,
    /// <summary><c>ridley_supplement_a6_dd3c</c>, an installed spritemap.</summary>
    RidleySupplementA6Dd3c,
    /// <summary><c>ridley_supplement_a6_dd43</c>, an installed spritemap.</summary>
    RidleySupplementA6Dd43,
    /// <summary><c>ridley_supplement_a6_dd4a</c>, an installed spritemap.</summary>
    RidleySupplementA6Dd4a,
    /// <summary><c>ridley_supplement_a6_dd6a</c>, an installed spritemap.</summary>
    RidleySupplementA6Dd6a,
    /// <summary><c>ridley_supplement_a6_dd85</c>, an installed spritemap.</summary>
    RidleySupplementA6Dd85,
    /// <summary><c>ridley_supplement_a6_dd96</c>, an installed spritemap.</summary>
    RidleySupplementA6Dd96,
    /// <summary><c>ridley_supplement_a6_dda7</c>, an installed spritemap.</summary>
    RidleySupplementA6Dda7,
    /// <summary><c>ridley_supplement_a6_ddc2</c>, an installed spritemap.</summary>
    RidleySupplementA6Ddc2,
    /// <summary><c>ridley_supplement_a6_dde2</c>, an installed spritemap.</summary>
    RidleySupplementA6Dde2,
    /// <summary><c>ridley_supplement_a6_de02</c>, an installed spritemap.</summary>
    RidleySupplementA6De02,
    /// <summary><c>ridley_supplement_a6_de1d</c>, an installed spritemap.</summary>
    RidleySupplementA6De1d,
    /// <summary><c>ridley_supplement_a6_de2e</c>, an installed spritemap.</summary>
    RidleySupplementA6De2e,
    /// <summary><c>ridley_supplement_a6_de3f</c>, an installed spritemap.</summary>
    RidleySupplementA6De3f,
    /// <summary><c>ridley_supplement_a6_de5a</c>, an installed spritemap.</summary>
    RidleySupplementA6De5a,
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
    NorfairLavaJumperC02c,
    /// <summary><c>norfair_lava_jumper_c042</c>, an installed spritemap.</summary>
    NorfairLavaJumperC042,
    /// <summary><c>norfair_lava_jumper_c058</c>, an installed spritemap.</summary>
    NorfairLavaJumperC058,
    /// <summary><c>norfair_lava_jumper_c064</c>, an installed spritemap.</summary>
    NorfairLavaJumperC064,
    /// <summary><c>norfair_lava_jumper_c070</c>, an installed spritemap.</summary>
    NorfairLavaJumperC070,
    /// <summary><c>norfair_lava_jumper_c086</c>, an installed spritemap.</summary>
    NorfairLavaJumperC086,
    /// <summary><c>norfair_lava_jumper_c09c</c>, an installed spritemap.</summary>
    NorfairLavaJumperC09c,
    /// <summary><c>norfair_lava_jumper_c0ad</c>, an installed spritemap.</summary>
    NorfairLavaJumperC0ad,
    /// <summary><c>norfair_lava_jumper_804d</c>, an installed spritemap.</summary>
    NorfairLavaJumper804d,
    /// <summary><c>norfair_lava_jumper_c0c5</c>, an installed spritemap.</summary>
    NorfairLavaJumperC0c5,
    /// <summary><c>norfair_lava_jumper_c0b9</c>, an installed spritemap.</summary>
    NorfairLavaJumperC0b9,
    /// <summary><c>chozo_statue_aa_efd8</c>, an installed spritemap.</summary>
    ChozoStatueAaEfd8,
    /// <summary><c>chozo_statue_aa_f034</c>, an installed spritemap.</summary>
    ChozoStatueAaF034,
    /// <summary><c>chozo_statue_aa_f08b</c>, an installed spritemap.</summary>
    ChozoStatueAaF08b,
    /// <summary><c>chozo_statue_aa_f0e2</c>, an installed spritemap.</summary>
    ChozoStatueAaF0e2,
    /// <summary><c>chozo_statue_aa_f4b4</c>, an installed spritemap.</summary>
    ChozoStatueAaF4b4,
    /// <summary><c>chozo_statue_aa_f510</c>, an installed spritemap.</summary>
    ChozoStatueAaF510,
    /// <summary><c>chozo_statue_aa_f56c</c>, an installed spritemap.</summary>
    ChozoStatueAaF56c,
    /// <summary><c>chozo_statue_aa_f5c8</c>, an installed spritemap.</summary>
    ChozoStatueAaF5c8,
    /// <summary><c>chozo_statue_aa_f624</c>, an installed spritemap.</summary>
    ChozoStatueAaF624,
    /// <summary><c>chozo_statue_aa_e7dd</c>, an installed spritemap.</summary>
    ChozoStatueAaE7dd,
    /// <summary><c>chozo_statue_aa_e839</c>, an installed spritemap.</summary>
    ChozoStatueAaE839,
    /// <summary><c>chozo_statue_aa_e890</c>, an installed spritemap.</summary>
    ChozoStatueAaE890,
    /// <summary><c>chozo_statue_aa_e8e7</c>, an installed spritemap.</summary>
    ChozoStatueAaE8e7,
    /// <summary><c>chozo_statue_aa_ecb9</c>, an installed spritemap.</summary>
    ChozoStatueAaEcb9,
    /// <summary><c>chozo_statue_aa_ed15</c>, an installed spritemap.</summary>
    ChozoStatueAaEd15,
    /// <summary><c>chozo_statue_aa_ed71</c>, an installed spritemap.</summary>
    ChozoStatueAaEd71,
    /// <summary><c>chozo_statue_aa_edcd</c>, an installed spritemap.</summary>
    ChozoStatueAaEdcd,
    /// <summary><c>chozo_statue_aa_ee29</c>, an installed spritemap.</summary>
    ChozoStatueAaEe29,
    /// <summary><c>chozo_statue_aa_ec49</c>, an installed spritemap.</summary>
    ChozoStatueAaEc49,
    /// <summary><c>chozo_statue_aa_e943</c>, an installed spritemap.</summary>
    ChozoStatueAaE943,
    /// <summary><c>chozo_statue_aa_e9ae</c>, an installed spritemap.</summary>
    ChozoStatueAaE9ae,
    /// <summary><c>chozo_statue_aa_ea1e</c>, an installed spritemap.</summary>
    ChozoStatueAaEa1e,
    /// <summary><c>chozo_statue_aa_ea8e</c>, an installed spritemap.</summary>
    ChozoStatueAaEa8e,
    /// <summary><c>chozo_statue_aa_eafe</c>, an installed spritemap.</summary>
    ChozoStatueAaEafe,
    /// <summary><c>chozo_statue_aa_eb69</c>, an installed spritemap.</summary>
    ChozoStatueAaEb69,
    /// <summary><c>chozo_statue_aa_ebd9</c>, an installed spritemap.</summary>
    ChozoStatueAaEbd9,
    /// <summary><c>viola_spin_b689</c>, an installed spritemap.</summary>
    ViolaSpinB689,
    /// <summary><c>viola_spin_b690</c>, an installed spritemap.</summary>
    ViolaSpinB690,
    /// <summary><c>viola_spin_b697</c>, an installed spritemap.</summary>
    ViolaSpinB697,
    /// <summary><c>viola_spin_b69e</c>, an installed spritemap.</summary>
    ViolaSpinB69e,
    /// <summary><c>viola_spin_b6a5</c>, an installed spritemap.</summary>
    ViolaSpinB6a5,
    /// <summary><c>viola_spin_b6ac</c>, an installed spritemap.</summary>
    ViolaSpinB6ac,
    /// <summary><c>viola_spin_b6b3</c>, an installed spritemap.</summary>
    ViolaSpinB6b3,
    /// <summary><c>viola_spin_b6ba</c>, an installed spritemap.</summary>
    ViolaSpinB6ba,
    /// <summary><c>rinka_spin_ba38</c>, an installed spritemap.</summary>
    RinkaSpinBa38,
    /// <summary><c>rinka_spin_ba3f</c>, an installed spritemap.</summary>
    RinkaSpinBa3f,
    /// <summary><c>rinka_spin_ba46</c>, an installed spritemap.</summary>
    RinkaSpinBa46,
    /// <summary><c>rinka_spin_ba4d</c>, an installed spritemap.</summary>
    RinkaSpinBa4d,
    /// <summary><c>rinka_spin_ba54</c>, an installed spritemap.</summary>
    RinkaSpinBa54,
    /// <summary><c>dead_torizo_stationary_a9_d6e2</c>, an installed spritemap.</summary>
    DeadTorizoStationaryA9D6e2,
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
    NinjaPirate8A54,
    /// <summary><c>ninja_pirate_8A66</c>, an installed extended frame.</summary>
    NinjaPirate8A66,
    /// <summary><c>ninja_pirate_8A78</c>, an installed extended frame.</summary>
    NinjaPirate8A78,
    /// <summary><c>ninja_pirate_8A8A</c>, an installed extended frame.</summary>
    NinjaPirate8A8A,
    /// <summary><c>ninja_pirate_8A9C</c>, an installed extended frame.</summary>
    NinjaPirate8A9C,
    /// <summary><c>ninja_pirate_8AAE</c>, an installed extended frame.</summary>
    NinjaPirate8AAE,
    /// <summary><c>ninja_pirate_8AC0</c>, an installed extended frame.</summary>
    NinjaPirate8AC0,
    /// <summary><c>ninja_pirate_8AD2</c>, an installed extended frame.</summary>
    NinjaPirate8AD2,
    /// <summary><c>ninja_pirate_8AE4</c>, an installed extended frame.</summary>
    NinjaPirate8AE4,
    /// <summary><c>ninja_pirate_8D96</c>, an installed extended frame.</summary>
    NinjaPirate8D96,
    /// <summary><c>ninja_pirate_8DA0</c>, an installed extended frame.</summary>
    NinjaPirate8DA0,
    /// <summary><c>ninja_pirate_8DAA</c>, an installed extended frame.</summary>
    NinjaPirate8DAA,
    /// <summary><c>ninja_pirate_8DB4</c>, an installed extended frame.</summary>
    NinjaPirate8DB4,
    /// <summary><c>ninja_pirate_8DBE</c>, an installed extended frame.</summary>
    NinjaPirate8DBE,
    /// <summary><c>ninja_pirate_8DC8</c>, an installed extended frame.</summary>
    NinjaPirate8DC8,
    /// <summary><c>ninja_pirate_8DD2</c>, an installed extended frame.</summary>
    NinjaPirate8DD2,
    /// <summary><c>ninja_pirate_8DDC</c>, an installed extended frame.</summary>
    NinjaPirate8DDC,
    /// <summary><c>ninja_pirate_8F56</c>, an installed extended frame.</summary>
    NinjaPirate8F56,
    /// <summary><c>ninja_pirate_8F60</c>, an installed extended frame.</summary>
    NinjaPirate8F60,
    /// <summary><c>ninja_pirate_8F6A</c>, an installed extended frame.</summary>
    NinjaPirate8F6A,
    /// <summary><c>ninja_pirate_8F9C</c>, an installed extended frame.</summary>
    NinjaPirate8F9C,
    /// <summary><c>ninja_pirate_90FE</c>, an installed extended frame.</summary>
    NinjaPirate90FE,
    /// <summary><c>ninja_pirate_900A</c>, an installed extended frame.</summary>
    NinjaPirate900A,
    /// <summary><c>ninja_pirate_9014</c>, an installed extended frame.</summary>
    NinjaPirate9014,
    /// <summary><c>ninja_pirate_8E36</c>, an installed extended frame.</summary>
    NinjaPirate8E36,
    /// <summary><c>ninja_pirate_8E48</c>, an installed extended frame.</summary>
    NinjaPirate8E48,
    /// <summary><c>ninja_pirate_8E5A</c>, an installed extended frame.</summary>
    NinjaPirate8E5A,
    /// <summary><c>ninja_pirate_8E6C</c>, an installed extended frame.</summary>
    NinjaPirate8E6C,
    /// <summary><c>ninja_pirate_8E7E</c>, an installed extended frame.</summary>
    NinjaPirate8E7E,
    /// <summary><c>ninja_pirate_8E90</c>, an installed extended frame.</summary>
    NinjaPirate8E90,
    /// <summary><c>ninja_pirate_8EA2</c>, an installed extended frame.</summary>
    NinjaPirate8EA2,
    /// <summary><c>ninja_pirate_8EB4</c>, an installed extended frame.</summary>
    NinjaPirate8EB4,
    /// <summary><c>ninja_pirate_93EA</c>, an installed extended frame.</summary>
    NinjaPirate93EA,
    /// <summary><c>ninja_pirate_9404</c>, an installed extended frame.</summary>
    NinjaPirate9404,
    /// <summary><c>ninja_pirate_9416</c>, an installed extended frame.</summary>
    NinjaPirate9416,
    /// <summary><c>ninja_pirate_9476</c>, an installed extended frame.</summary>
    NinjaPirate9476,
    /// <summary><c>ninja_pirate_944A</c>, an installed extended frame.</summary>
    NinjaPirate944A,
    /// <summary><c>ninja_pirate_8F74</c>, an installed extended frame.</summary>
    NinjaPirate8F74,
    /// <summary><c>ninja_pirate_8F7E</c>, an installed extended frame.</summary>
    NinjaPirate8F7E,
    /// <summary><c>ninja_pirate_8F88</c>, an installed extended frame.</summary>
    NinjaPirate8F88,
    /// <summary><c>ninja_pirate_9032</c>, an installed extended frame.</summary>
    NinjaPirate9032,
    /// <summary><c>ninja_pirate_9372</c>, an installed extended frame.</summary>
    NinjaPirate9372,
    /// <summary><c>ninja_pirate_903C</c>, an installed extended frame.</summary>
    NinjaPirate903C,
    /// <summary><c>ninja_pirate_8B86</c>, an installed extended frame.</summary>
    NinjaPirate8B86,
    /// <summary><c>ninja_pirate_8B98</c>, an installed extended frame.</summary>
    NinjaPirate8B98,
    /// <summary><c>ninja_pirate_8BAA</c>, an installed extended frame.</summary>
    NinjaPirate8BAA,
    /// <summary><c>ninja_pirate_8BBC</c>, an installed extended frame.</summary>
    NinjaPirate8BBC,
    /// <summary><c>ninja_pirate_8BCE</c>, an installed extended frame.</summary>
    NinjaPirate8BCE,
    /// <summary><c>ninja_pirate_8BE0</c>, an installed extended frame.</summary>
    NinjaPirate8BE0,
    /// <summary><c>ninja_pirate_8BF2</c>, an installed extended frame.</summary>
    NinjaPirate8BF2,
    /// <summary><c>ninja_pirate_8C16</c>, an installed extended frame.</summary>
    NinjaPirate8C16,
    /// <summary><c>ninja_pirate_8DE6</c>, an installed extended frame.</summary>
    NinjaPirate8DE6,
    /// <summary><c>ninja_pirate_8DF0</c>, an installed extended frame.</summary>
    NinjaPirate8DF0,
    /// <summary><c>ninja_pirate_8DFA</c>, an installed extended frame.</summary>
    NinjaPirate8DFA,
    /// <summary><c>ninja_pirate_8E04</c>, an installed extended frame.</summary>
    NinjaPirate8E04,
    /// <summary><c>ninja_pirate_8E0E</c>, an installed extended frame.</summary>
    NinjaPirate8E0E,
    /// <summary><c>ninja_pirate_8E18</c>, an installed extended frame.</summary>
    NinjaPirate8E18,
    /// <summary><c>ninja_pirate_8E22</c>, an installed extended frame.</summary>
    NinjaPirate8E22,
    /// <summary><c>ninja_pirate_8E2C</c>, an installed extended frame.</summary>
    NinjaPirate8E2C,
    /// <summary><c>ninja_pirate_8FB0</c>, an installed extended frame.</summary>
    NinjaPirate8FB0,
    /// <summary><c>ninja_pirate_9280</c>, an installed extended frame.</summary>
    NinjaPirate9280,
    /// <summary><c>ninja_pirate_901E</c>, an installed extended frame.</summary>
    NinjaPirate901E,
    /// <summary><c>ninja_pirate_9028</c>, an installed extended frame.</summary>
    NinjaPirate9028,
    /// <summary><c>ninja_pirate_8EC6</c>, an installed extended frame.</summary>
    NinjaPirate8EC6,
    /// <summary><c>ninja_pirate_8ED8</c>, an installed extended frame.</summary>
    NinjaPirate8ED8,
    /// <summary><c>ninja_pirate_8EEA</c>, an installed extended frame.</summary>
    NinjaPirate8EEA,
    /// <summary><c>ninja_pirate_8EFC</c>, an installed extended frame.</summary>
    NinjaPirate8EFC,
    /// <summary><c>ninja_pirate_8F0E</c>, an installed extended frame.</summary>
    NinjaPirate8F0E,
    /// <summary><c>ninja_pirate_8F20</c>, an installed extended frame.</summary>
    NinjaPirate8F20,
    /// <summary><c>ninja_pirate_8F32</c>, an installed extended frame.</summary>
    NinjaPirate8F32,
    /// <summary><c>ninja_pirate_8F44</c>, an installed extended frame.</summary>
    NinjaPirate8F44,
    /// <summary><c>ninja_pirate_9430</c>, an installed extended frame.</summary>
    NinjaPirate9430,
    /// <summary><c>ninja_pirate_945C</c>, an installed extended frame.</summary>
    NinjaPirate945C,
    /// <summary><c>ninja_pirate_9046</c>, an installed extended frame.</summary>
    NinjaPirate9046,
    /// <summary><c>ninja_pirate_937C</c>, an installed extended frame.</summary>
    NinjaPirate937C,
    /// <summary><c>ninja_pirate_9050</c>, an installed extended frame.</summary>
    NinjaPirate9050,
    /// <summary><c>ridley_body_E983</c>, an installed extended frame.</summary>
    RidleyBodyE983,
    /// <summary><c>ridley_body_E9A5</c>, an installed extended frame.</summary>
    RidleyBodyE9A5,
    /// <summary><c>ridley_body_E9C7</c>, an installed extended frame.</summary>
    RidleyBodyE9C7,
    /// <summary><c>ridley_body_E9E9</c>, an installed extended frame.</summary>
    RidleyBodyE9E9,
    /// <summary><c>ridley_body_EA0B</c>, an installed extended frame.</summary>
    RidleyBodyEA0B,
    /// <summary><c>ridley_body_EA2D</c>, an installed extended frame.</summary>
    RidleyBodyEA2D,
    /// <summary><c>ridley_body_EA4F</c>, an installed extended frame.</summary>
    RidleyBodyEA4F,
    /// <summary><c>ridley_body_EA71</c>, an installed extended frame.</summary>
    RidleyBodyEA71,
    /// <summary><c>ridley_body_EA93</c>, an installed extended frame.</summary>
    RidleyBodyEA93,
    /// <summary><c>ridley_body_EAB5</c>, an installed extended frame.</summary>
    RidleyBodyEAB5,
    /// <summary><c>ridley_body_EAD7</c>, an installed extended frame.</summary>
    RidleyBodyEAD7,
    /// <summary><c>draygon_oam_A2DF</c>, an installed extended frame.</summary>
    DraygonOamA2DF,
    /// <summary><c>draygon_oam_A2E9</c>, an installed extended frame.</summary>
    DraygonOamA2E9,
    /// <summary><c>draygon_oam_A2F3</c>, an installed extended frame.</summary>
    DraygonOamA2F3,
    /// <summary><c>draygon_oam_A2FD</c>, an installed extended frame.</summary>
    DraygonOamA2FD,
    /// <summary><c>draygon_oam_A307</c>, an installed extended frame.</summary>
    DraygonOamA307,
    /// <summary><c>draygon_oam_A311</c>, an installed extended frame.</summary>
    DraygonOamA311,
    /// <summary><c>draygon_oam_A3C5</c>, an installed extended frame.</summary>
    DraygonOamA3C5,
    /// <summary><c>draygon_oam_A3CF</c>, an installed extended frame.</summary>
    DraygonOamA3CF,
    /// <summary><c>draygon_oam_A3D9</c>, an installed extended frame.</summary>
    DraygonOamA3D9,
    /// <summary><c>draygon_oam_A3E3</c>, an installed extended frame.</summary>
    DraygonOamA3E3,
    /// <summary><c>draygon_oam_A40B</c>, an installed extended frame.</summary>
    DraygonOamA40B,
    /// <summary><c>draygon_oam_A41D</c>, an installed extended frame.</summary>
    DraygonOamA41D,
    /// <summary><c>draygon_oam_A42F</c>, an installed extended frame.</summary>
    DraygonOamA42F,
    /// <summary><c>draygon_oam_A441</c>, an installed extended frame.</summary>
    DraygonOamA441,
    /// <summary><c>draygon_oam_A453</c>, an installed extended frame.</summary>
    DraygonOamA453,
    /// <summary><c>draygon_oam_A465</c>, an installed extended frame.</summary>
    DraygonOamA465,
    /// <summary><c>draygon_oam_A477</c>, an installed extended frame.</summary>
    DraygonOamA477,
    /// <summary><c>draygon_oam_A489</c>, an installed extended frame.</summary>
    DraygonOamA489,
    /// <summary><c>draygon_oam_A4A3</c>, an installed extended frame.</summary>
    DraygonOamA4A3,
    /// <summary><c>draygon_oam_A4C5</c>, an installed extended frame.</summary>
    DraygonOamA4C5,
    /// <summary><c>draygon_oam_A4EF</c>, an installed extended frame.</summary>
    DraygonOamA4EF,
    /// <summary><c>draygon_oam_A521</c>, an installed extended frame.</summary>
    DraygonOamA521,
    /// <summary><c>draygon_oam_A55B</c>, an installed extended frame.</summary>
    DraygonOamA55B,
    /// <summary><c>draygon_oam_A59D</c>, an installed extended frame.</summary>
    DraygonOamA59D,
    /// <summary><c>draygon_oam_A607</c>, an installed extended frame.</summary>
    DraygonOamA607,
    /// <summary><c>draygon_oam_A611</c>, an installed extended frame.</summary>
    DraygonOamA611,
    /// <summary><c>draygon_oam_A61B</c>, an installed extended frame.</summary>
    DraygonOamA61B,
    /// <summary><c>draygon_oam_A625</c>, an installed extended frame.</summary>
    DraygonOamA625,
    /// <summary><c>draygon_oam_A62F</c>, an installed extended frame.</summary>
    DraygonOamA62F,
    /// <summary><c>draygon_oam_A639</c>, an installed extended frame.</summary>
    DraygonOamA639,
    /// <summary><c>draygon_oam_A6ED</c>, an installed extended frame.</summary>
    DraygonOamA6ED,
    /// <summary><c>draygon_oam_A6F7</c>, an installed extended frame.</summary>
    DraygonOamA6F7,
    /// <summary><c>draygon_oam_A701</c>, an installed extended frame.</summary>
    DraygonOamA701,
    /// <summary><c>draygon_oam_A70B</c>, an installed extended frame.</summary>
    DraygonOamA70B,
    /// <summary><c>draygon_oam_A779</c>, an installed extended frame.</summary>
    DraygonOamA779,
    /// <summary><c>draygon_oam_A78B</c>, an installed extended frame.</summary>
    DraygonOamA78B,
    /// <summary><c>draygon_oam_A79D</c>, an installed extended frame.</summary>
    DraygonOamA79D,
    /// <summary><c>draygon_oam_A7AF</c>, an installed extended frame.</summary>
    DraygonOamA7AF,
    /// <summary><c>draygon_oam_A7C1</c>, an installed extended frame.</summary>
    DraygonOamA7C1,
    /// <summary><c>draygon_oam_A7D3</c>, an installed extended frame.</summary>
    DraygonOamA7D3,
    /// <summary><c>draygon_oam_A7E5</c>, an installed extended frame.</summary>
    DraygonOamA7E5,
    /// <summary><c>draygon_oam_A7F7</c>, an installed extended frame.</summary>
    DraygonOamA7F7,
    /// <summary><c>draygon_oam_A811</c>, an installed extended frame.</summary>
    DraygonOamA811,
    /// <summary><c>draygon_oam_A833</c>, an installed extended frame.</summary>
    DraygonOamA833,
    /// <summary><c>draygon_oam_A85D</c>, an installed extended frame.</summary>
    DraygonOamA85D,
    /// <summary><c>draygon_oam_A88F</c>, an installed extended frame.</summary>
    DraygonOamA88F,
    /// <summary><c>draygon_oam_A8C9</c>, an installed extended frame.</summary>
    DraygonOamA8C9,
    /// <summary><c>draygon_oam_A90B</c>, an installed extended frame.</summary>
    DraygonOamA90B,
    /// <summary><c>spore_spawn_oam_EE65</c>, an installed extended frame.</summary>
    SporeSpawnOamEE65,
    /// <summary><c>spore_spawn_oam_EE6F</c>, an installed extended frame.</summary>
    SporeSpawnOamEE6F,
    /// <summary><c>spore_spawn_oam_EE79</c>, an installed extended frame.</summary>
    SporeSpawnOamEE79,
    /// <summary><c>spore_spawn_oam_EE8B</c>, an installed extended frame.</summary>
    SporeSpawnOamEE8B,
    /// <summary><c>spore_spawn_oam_EE9D</c>, an installed extended frame.</summary>
    SporeSpawnOamEE9D,
    /// <summary><c>spore_spawn_oam_EEAF</c>, an installed extended frame.</summary>
    SporeSpawnOamEEAF,
    /// <summary><c>spore_spawn_oam_EEC1</c>, an installed extended frame.</summary>
    SporeSpawnOamEEC1,
    /// <summary><c>spore_spawn_oam_EED3</c>, an installed extended frame.</summary>
    SporeSpawnOamEED3,
    /// <summary><c>spore_spawn_oam_EEE5</c>, an installed extended frame.</summary>
    SporeSpawnOamEEE5,
    /// <summary><c>spore_spawn_oam_EF3D</c>, an installed extended frame.</summary>
    SporeSpawnOamEF3D,
    /// <summary><c>spore_spawn_oam_EF4F</c>, an installed extended frame.</summary>
    SporeSpawnOamEF4F,
    /// <summary><c>spore_spawn_oam_EF61</c>, an installed extended frame.</summary>
    SporeSpawnOamEF61,
    /// <summary><c>ceres_steam_oam_F142</c>, an installed extended frame.</summary>
    CeresSteamOamF142,
    /// <summary><c>ceres_steam_oam_F14C</c>, an installed extended frame.</summary>
    CeresSteamOamF14C,
    /// <summary><c>ceres_steam_oam_F156</c>, an installed extended frame.</summary>
    CeresSteamOamF156,
    /// <summary><c>ceres_steam_oam_F160</c>, an installed extended frame.</summary>
    CeresSteamOamF160,
    /// <summary><c>ceres_steam_oam_F16A</c>, an installed extended frame.</summary>
    CeresSteamOamF16A,
    /// <summary><c>ceres_steam_oam_F174</c>, an installed extended frame.</summary>
    CeresSteamOamF174,
    /// <summary><c>ceres_steam_oam_F17E</c>, an installed extended frame.</summary>
    CeresSteamOamF17E,
    /// <summary><c>ceres_steam_oam_F188</c>, an installed extended frame.</summary>
    CeresSteamOamF188,
    /// <summary><c>ceres_steam_oam_F192</c>, an installed extended frame.</summary>
    CeresSteamOamF192,
    /// <summary><c>ceres_steam_oam_F19C</c>, an installed extended frame.</summary>
    CeresSteamOamF19C,
    /// <summary><c>ceres_steam_oam_F1A6</c>, an installed extended frame.</summary>
    CeresSteamOamF1A6,
    /// <summary><c>ceres_steam_oam_F1B0</c>, an installed extended frame.</summary>
    CeresSteamOamF1B0,
    /// <summary><c>ceres_steam_oam_F1BA</c>, an installed extended frame.</summary>
    CeresSteamOamF1BA,
    /// <summary><c>ceres_steam_oam_F1C4</c>, an installed extended frame.</summary>
    CeresSteamOamF1C4,
    /// <summary><c>ceres_steam_oam_F1CE</c>, an installed extended frame.</summary>
    CeresSteamOamF1CE,
    /// <summary><c>ceres_steam_oam_F1D8</c>, an installed extended frame.</summary>
    CeresSteamOamF1D8,
    /// <summary><c>ceres_steam_oam_F1E2</c>, an installed extended frame.</summary>
    CeresSteamOamF1E2,
    /// <summary><c>ceres_steam_oam_F1EC</c>, an installed extended frame.</summary>
    CeresSteamOamF1EC,
    /// <summary><c>ceres_steam_oam_F1F6</c>, an installed extended frame.</summary>
    CeresSteamOamF1F6,
    /// <summary><c>ceres_steam_oam_F200</c>, an installed extended frame.</summary>
    CeresSteamOamF200,
    /// <summary><c>ceres_steam_oam_F20A</c>, an installed extended frame.</summary>
    CeresSteamOamF20A,
    /// <summary><c>ceres_steam_oam_F214</c>, an installed extended frame.</summary>
    CeresSteamOamF214,
    /// <summary><c>ceres_steam_oam_F21E</c>, an installed extended frame.</summary>
    CeresSteamOamF21E,
    /// <summary><c>ceres_steam_oam_F228</c>, an installed extended frame.</summary>
    CeresSteamOamF228,
    /// <summary><c>ceres_steam_oam_F232</c>, an installed extended frame.</summary>
    CeresSteamOamF232,
    /// <summary><c>ceres_steam_oam_F23C</c>, an installed extended frame.</summary>
    CeresSteamOamF23C,
    /// <summary><c>ceres_steam_oam_F246</c>, an installed extended frame.</summary>
    CeresSteamOamF246,
    /// <summary><c>ceres_steam_oam_F250</c>, an installed extended frame.</summary>
    CeresSteamOamF250,
    /// <summary><c>oum_oam_CB87</c>, an installed extended frame.</summary>
    OumOamCB87,
    /// <summary><c>oum_oam_CB91</c>, an installed extended frame.</summary>
    OumOamCB91,
    /// <summary><c>oum_oam_CB9B</c>, an installed extended frame.</summary>
    OumOamCB9B,
    /// <summary><c>oum_oam_CBA5</c>, an installed extended frame.</summary>
    OumOamCBA5,
    /// <summary><c>oum_oam_CBAF</c>, an installed extended frame.</summary>
    OumOamCBAF,
    /// <summary><c>oum_oam_CBB9</c>, an installed extended frame.</summary>
    OumOamCBB9,
    /// <summary><c>oum_oam_CBC3</c>, an installed extended frame.</summary>
    OumOamCBC3,
    /// <summary><c>oum_oam_CBCD</c>, an installed extended frame.</summary>
    OumOamCBCD,
    /// <summary><c>oum_oam_CBD7</c>, an installed extended frame.</summary>
    OumOamCBD7,
    /// <summary><c>oum_oam_CBE1</c>, an installed extended frame.</summary>
    OumOamCBE1,
    /// <summary><c>oum_oam_CBEB</c>, an installed extended frame.</summary>
    OumOamCBEB,
    /// <summary><c>oum_oam_CBF5</c>, an installed extended frame.</summary>
    OumOamCBF5,
    /// <summary><c>oum_oam_CBFF</c>, an installed extended frame.</summary>
    OumOamCBFF,
    /// <summary><c>oum_oam_CC09</c>, an installed extended frame.</summary>
    OumOamCC09,
    /// <summary><c>oum_oam_CC13</c>, an installed extended frame.</summary>
    OumOamCC13,
    /// <summary><c>oum_oam_CC1D</c>, an installed extended frame.</summary>
    OumOamCC1D,
    /// <summary><c>oum_oam_CC27</c>, an installed extended frame.</summary>
    OumOamCC27,
    /// <summary><c>oum_oam_CC31</c>, an installed extended frame.</summary>
    OumOamCC31,
    /// <summary><c>oum_oam_CC3B</c>, an installed extended frame.</summary>
    OumOamCC3B,
    /// <summary><c>oum_oam_CC45</c>, an installed extended frame.</summary>
    OumOamCC45,
    /// <summary><c>oum_oam_CC4F</c>, an installed extended frame.</summary>
    OumOamCC4F,
    /// <summary><c>oum_oam_CC59</c>, an installed extended frame.</summary>
    OumOamCC59,
    /// <summary><c>oum_oam_CC63</c>, an installed extended frame.</summary>
    OumOamCC63,
    /// <summary><c>oum_oam_CC6D</c>, an installed extended frame.</summary>
    OumOamCC6D,
    /// <summary><c>oum_oam_CC77</c>, an installed extended frame.</summary>
    OumOamCC77,
    /// <summary><c>oum_oam_CC81</c>, an installed extended frame.</summary>
    OumOamCC81,
    /// <summary><c>oum_oam_CC8B</c>, an installed extended frame.</summary>
    OumOamCC8B,
    /// <summary><c>oum_oam_CC95</c>, an installed extended frame.</summary>
    OumOamCC95,
    /// <summary><c>oum_oam_CC9F</c>, an installed extended frame.</summary>
    OumOamCC9F,
    /// <summary><c>oum_oam_CCA9</c>, an installed extended frame.</summary>
    OumOamCCA9,
    /// <summary><c>crocomire_oam_C65E</c>, an installed extended frame.</summary>
    CrocomireOamC65E,
    /// <summary><c>crocomire_oam_C668</c>, an installed extended frame.</summary>
    CrocomireOamC668,
    /// <summary><c>crocomire_oam_C672</c>, an installed extended frame.</summary>
    CrocomireOamC672,
    /// <summary><c>crocomire_oam_C67C</c>, an installed extended frame.</summary>
    CrocomireOamC67C,
    /// <summary><c>crocomire_oam_CACE</c>, an installed extended frame.</summary>
    CrocomireOamCACE,
    /// <summary><c>crocomire_oam_CAD8</c>, an installed extended frame.</summary>
    CrocomireOamCAD8,
    /// <summary><c>crocomire_oam_CAE2</c>, an installed extended frame.</summary>
    CrocomireOamCAE2,
    /// <summary><c>crocomire_oam_CAEC</c>, an installed extended frame.</summary>
    CrocomireOamCAEC,
    /// <summary><c>crocomire_oam_CAF6</c>, an installed extended frame.</summary>
    CrocomireOamCAF6,
    /// <summary><c>crocomire_body_oam_BFC4</c>, an installed extended frame.</summary>
    CrocomireBodyOamBFC4,
    /// <summary><c>crocomire_body_oam_BFF6</c>, an installed extended frame.</summary>
    CrocomireBodyOamBFF6,
    /// <summary><c>crocomire_body_oam_C028</c>, an installed extended frame.</summary>
    CrocomireBodyOamC028,
    /// <summary><c>crocomire_body_oam_C05A</c>, an installed extended frame.</summary>
    CrocomireBodyOamC05A,
    /// <summary><c>crocomire_body_oam_C08C</c>, an installed extended frame.</summary>
    CrocomireBodyOamC08C,
    /// <summary><c>crocomire_body_oam_C0BE</c>, an installed extended frame.</summary>
    CrocomireBodyOamC0BE,
    /// <summary><c>crocomire_body_oam_C0F0</c>, an installed extended frame.</summary>
    CrocomireBodyOamC0F0,
    /// <summary><c>crocomire_body_oam_C122</c>, an installed extended frame.</summary>
    CrocomireBodyOamC122,
    /// <summary><c>crocomire_body_oam_C154</c>, an installed extended frame.</summary>
    CrocomireBodyOamC154,
    /// <summary><c>crocomire_body_oam_C186</c>, an installed extended frame.</summary>
    CrocomireBodyOamC186,
    /// <summary><c>crocomire_body_oam_C1B8</c>, an installed extended frame.</summary>
    CrocomireBodyOamC1B8,
    /// <summary><c>crocomire_body_oam_C1EA</c>, an installed extended frame.</summary>
    CrocomireBodyOamC1EA,
    /// <summary><c>crocomire_body_oam_C2EC</c>, an installed extended frame.</summary>
    CrocomireBodyOamC2EC,
    /// <summary><c>crocomire_body_oam_C326</c>, an installed extended frame.</summary>
    CrocomireBodyOamC326,
    /// <summary><c>crocomire_body_oam_C360</c>, an installed extended frame.</summary>
    CrocomireBodyOamC360,
    /// <summary><c>crocomire_body_oam_C39A</c>, an installed extended frame.</summary>
    CrocomireBodyOamC39A,
    /// <summary><c>crocomire_body_oam_C3D4</c>, an installed extended frame.</summary>
    CrocomireBodyOamC3D4,
    /// <summary><c>crocomire_body_oam_C40E</c>, an installed extended frame.</summary>
    CrocomireBodyOamC40E,
    /// <summary><c>crocomire_body_oam_C448</c>, an installed extended frame.</summary>
    CrocomireBodyOamC448,
    /// <summary><c>crocomire_body_oam_C47A</c>, an installed extended frame.</summary>
    CrocomireBodyOamC47A,
    /// <summary><c>crocomire_body_oam_C4AC</c>, an installed extended frame.</summary>
    CrocomireBodyOamC4AC,
    /// <summary><c>crocomire_body_oam_C4DE</c>, an installed extended frame.</summary>
    CrocomireBodyOamC4DE,
    /// <summary><c>crocomire_body_oam_C510</c>, an installed extended frame.</summary>
    CrocomireBodyOamC510,
    /// <summary><c>crocomire_body_oam_C542</c>, an installed extended frame.</summary>
    CrocomireBodyOamC542,
    /// <summary><c>crocomire_body_oam_C574</c>, an installed extended frame.</summary>
    CrocomireBodyOamC574,
    /// <summary><c>crocomire_body_oam_C5AE</c>, an installed extended frame.</summary>
    CrocomireBodyOamC5AE,
    /// <summary><c>crocomire_body_oam_C5E8</c>, an installed extended frame.</summary>
    CrocomireBodyOamC5E8,
    /// <summary><c>crocomire_body_oam_C6A4</c>, an installed extended frame.</summary>
    CrocomireBodyOamC6A4,
    /// <summary><c>crocomire_body_oam_C752</c>, an installed extended frame.</summary>
    CrocomireBodyOamC752,
    /// <summary><c>crocomire_body_oam_C78C</c>, an installed extended frame.</summary>
    CrocomireBodyOamC78C,
    /// <summary><c>crocomire_body_oam_C7C6</c>, an installed extended frame.</summary>
    CrocomireBodyOamC7C6,
    /// <summary><c>crocomire_body_oam_C800</c>, an installed extended frame.</summary>
    CrocomireBodyOamC800,
    /// <summary><c>crocomire_body_oam_C83A</c>, an installed extended frame.</summary>
    CrocomireBodyOamC83A,
    /// <summary><c>crocomire_body_oam_C874</c>, an installed extended frame.</summary>
    CrocomireBodyOamC874,
    /// <summary><c>crocomire_body_oam_C8AE</c>, an installed extended frame.</summary>
    CrocomireBodyOamC8AE,
    /// <summary><c>crocomire_body_oam_C8E8</c>, an installed extended frame.</summary>
    CrocomireBodyOamC8E8,
    /// <summary><c>crocomire_body_oam_C922</c>, an installed extended frame.</summary>
    CrocomireBodyOamC922,
    /// <summary><c>crocomire_body_oam_C95C</c>, an installed extended frame.</summary>
    CrocomireBodyOamC95C,
    /// <summary><c>crocomire_body_oam_C996</c>, an installed extended frame.</summary>
    CrocomireBodyOamC996,
    /// <summary><c>crocomire_body_oam_C9D0</c>, an installed extended frame.</summary>
    CrocomireBodyOamC9D0,
    /// <summary><c>crocomire_body_oam_CA0A</c>, an installed extended frame.</summary>
    CrocomireBodyOamCA0A,
    /// <summary><c>crocomire_body_oam_CA44</c>, an installed extended frame.</summary>
    CrocomireBodyOamCA44,
    /// <summary><c>crocomire_body_oam_CA7E</c>, an installed extended frame.</summary>
    CrocomireBodyOamCA7E,
    /// <summary><c>crocomire_body_oam_CA88</c>, an installed extended frame.</summary>
    CrocomireBodyOamCA88,
    /// <summary><c>crocomire_body_oam_CA92</c>, an installed extended frame.</summary>
    CrocomireBodyOamCA92,
    /// <summary><c>crocomire_body_oam_CA9C</c>, an installed extended frame.</summary>
    CrocomireBodyOamCA9C,
    /// <summary><c>crocomire_body_oam_CAA6</c>, an installed extended frame.</summary>
    CrocomireBodyOamCAA6,
    /// <summary><c>crocomire_body_oam_CAB0</c>, an installed extended frame.</summary>
    CrocomireBodyOamCAB0,
    /// <summary><c>crocomire_body_oam_CABA</c>, an installed extended frame.</summary>
    CrocomireBodyOamCABA,
    /// <summary><c>crocomire_body_oam_CAC4</c>, an installed extended frame.</summary>
    CrocomireBodyOamCAC4,
    /// <summary><c>bomb_torizo_dormant</c>, an installed extended frame.</summary>
    BombTorizoDormant,
    /// <summary><c>golden_torizo_initial</c>, an installed extended frame.</summary>
    GoldenTorizoInitial,
    /// <summary><c>kraid_arm_oam_8F59</c>, an installed extended frame.</summary>
    KraidArmOam8F59,
    /// <summary><c>kraid_arm_oam_8F83</c>, an installed extended frame.</summary>
    KraidArmOam8F83,
    /// <summary><c>kraid_arm_oam_8FAD</c>, an installed extended frame.</summary>
    KraidArmOam8FAD,
    /// <summary><c>kraid_arm_oam_8FD7</c>, an installed extended frame.</summary>
    KraidArmOam8FD7,
    /// <summary><c>kraid_arm_oam_9001</c>, an installed extended frame.</summary>
    KraidArmOam9001,
    /// <summary><c>kraid_arm_oam_902B</c>, an installed extended frame.</summary>
    KraidArmOam902B,
    /// <summary><c>kraid_arm_oam_9055</c>, an installed extended frame.</summary>
    KraidArmOam9055,
    /// <summary><c>kraid_arm_oam_907F</c>, an installed extended frame.</summary>
    KraidArmOam907F,
    /// <summary><c>kraid_arm_oam_90A9</c>, an installed extended frame.</summary>
    KraidArmOam90A9,
    /// <summary><c>kraid_arm_oam_90D3</c>, an installed extended frame.</summary>
    KraidArmOam90D3,
    /// <summary><c>kraid_arm_oam_90FD</c>, an installed extended frame.</summary>
    KraidArmOam90FD,
    /// <summary><c>kraid_arm_oam_9127</c>, an installed extended frame.</summary>
    KraidArmOam9127,
    /// <summary><c>kraid_arm_oam_9151</c>, an installed extended frame.</summary>
    KraidArmOam9151,
    /// <summary><c>kraid_arm_oam_917B</c>, an installed extended frame.</summary>
    KraidArmOam917B,
    /// <summary><c>kraid_arm_oam_91A5</c>, an installed extended frame.</summary>
    KraidArmOam91A5,
    /// <summary><c>kraid_arm_oam_91CF</c>, an installed extended frame.</summary>
    KraidArmOam91CF,
    /// <summary><c>kraid_arm_oam_91F9</c>, an installed extended frame.</summary>
    KraidArmOam91F9,
    /// <summary><c>kraid_arm_oam_9223</c>, an installed extended frame.</summary>
    KraidArmOam9223,
    /// <summary><c>kraid_arm_oam_924D</c>, an installed extended frame.</summary>
    KraidArmOam924D,
    /// <summary><c>kraid_arm_oam_9277</c>, an installed extended frame.</summary>
    KraidArmOam9277,
    /// <summary><c>kraid_arm_oam_92A1</c>, an installed extended frame.</summary>
    KraidArmOam92A1,
    /// <summary><c>kraid_arm_oam_92AB</c>, an installed extended frame.</summary>
    KraidArmOam92AB,
    /// <summary><c>golden_torizo_awake_AA26</c>, an installed extended frame.</summary>
    GoldenTorizoAwakeAA26,
    /// <summary><c>golden_torizo_awake_AA1C</c>, an installed extended frame.</summary>
    GoldenTorizoAwakeAA1C,
    /// <summary><c>golden_torizo_awake_AA12</c>, an installed extended frame.</summary>
    GoldenTorizoAwakeAA12,
    /// <summary><c>golden_torizo_awake_AA3A</c>, an installed extended frame.</summary>
    GoldenTorizoAwakeAA3A,
    /// <summary><c>golden_torizo_awake_AA4C</c>, an installed extended frame.</summary>
    GoldenTorizoAwakeAA4C,
    /// <summary><c>golden_torizo_awake_AA5E</c>, an installed extended frame.</summary>
    GoldenTorizoAwakeAA5E,
    /// <summary><c>golden_torizo_walk_left_A4FA</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeftA4FA,
    /// <summary><c>golden_torizo_walk_left_A51C</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeftA51C,
    /// <summary><c>golden_torizo_walk_left_A53E</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeftA53E,
    /// <summary><c>golden_torizo_walk_left_A560</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeftA560,
    /// <summary><c>golden_torizo_walk_left_A582</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeftA582,
    /// <summary><c>golden_torizo_walk_left_A5A4</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeftA5A4,
    /// <summary><c>golden_torizo_walk_left_A5C6</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeftA5C6,
    /// <summary><c>golden_torizo_walk_left_A5E8</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeftA5E8,
    /// <summary><c>golden_torizo_walk_left_A60A</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeftA60A,
    /// <summary><c>golden_torizo_walk_left_A62C</c>, an installed extended frame.</summary>
    GoldenTorizoWalkLeftA62C,
    /// <summary><c>golden_torizo_rightward_A4F0</c>, an installed extended frame.</summary>
    GoldenTorizoRightwardA4F0,
    /// <summary><c>golden_torizo_rightward_AA98</c>, an installed extended frame.</summary>
    GoldenTorizoRightwardAA98,
    /// <summary><c>golden_torizo_rightward_AABA</c>, an installed extended frame.</summary>
    GoldenTorizoRightwardAABA,
    /// <summary><c>golden_torizo_rightward_AADC</c>, an installed extended frame.</summary>
    GoldenTorizoRightwardAADC,
    /// <summary><c>golden_torizo_rightward_AAFE</c>, an installed extended frame.</summary>
    GoldenTorizoRightwardAAFE,
    /// <summary><c>golden_torizo_rightward_AB20</c>, an installed extended frame.</summary>
    GoldenTorizoRightwardAB20,
    /// <summary><c>golden_torizo_rightward_AB42</c>, an installed extended frame.</summary>
    GoldenTorizoRightwardAB42,
    /// <summary><c>golden_torizo_rightward_AB64</c>, an installed extended frame.</summary>
    GoldenTorizoRightwardAB64,
    /// <summary><c>golden_torizo_rightward_AB86</c>, an installed extended frame.</summary>
    GoldenTorizoRightwardAB86,
    /// <summary><c>golden_torizo_rightward_ABA8</c>, an installed extended frame.</summary>
    GoldenTorizoRightwardABA8,
    /// <summary><c>golden_torizo_rightward_ABCA</c>, an installed extended frame.</summary>
    GoldenTorizoRightwardABCA,
    /// <summary><c>torizo_jump_back_B048</c>, an installed extended frame.</summary>
    TorizoJumpBackB048,
    /// <summary><c>torizo_jump_back_B062</c>, an installed extended frame.</summary>
    TorizoJumpBackB062,
    /// <summary><c>torizo_jump_back_B07C</c>, an installed extended frame.</summary>
    TorizoJumpBackB07C,
    /// <summary><c>golden_torizo_right_orb_AC88</c>, an installed extended frame.</summary>
    GoldenTorizoRightOrbAC88,
    /// <summary><c>golden_torizo_right_orb_ACA2</c>, an installed extended frame.</summary>
    GoldenTorizoRightOrbACA2,
    /// <summary><c>golden_torizo_right_orb_ACBC</c>, an installed extended frame.</summary>
    GoldenTorizoRightOrbACBC,
    /// <summary><c>golden_torizo_right_orb_ACD6</c>, an installed extended frame.</summary>
    GoldenTorizoRightOrbACD6,
    /// <summary><c>golden_torizo_right_orb_ACF0</c>, an installed extended frame.</summary>
    GoldenTorizoRightOrbACF0,
    /// <summary><c>golden_torizo_right_orb_AD0A</c>, an installed extended frame.</summary>
    GoldenTorizoRightOrbAD0A,
    /// <summary><c>golden_torizo_right_sonic_ABEC</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicABEC,
    /// <summary><c>golden_torizo_right_sonic_AD24</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicAD24,
    /// <summary><c>golden_torizo_right_sonic_AD3E</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicAD3E,
    /// <summary><c>golden_torizo_right_sonic_AD60</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicAD60,
    /// <summary><c>golden_torizo_right_sonic_AD82</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicAD82,
    /// <summary><c>golden_torizo_right_sonic_ADA4</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicADA4,
    /// <summary><c>golden_torizo_right_sonic_ADC6</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicADC6,
    /// <summary><c>golden_torizo_right_sonic_ADE8</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicADE8,
    /// <summary><c>golden_torizo_right_sonic_AE0A</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicAE0A,
    /// <summary><c>golden_torizo_right_sonic_AE2C</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicAE2C,
    /// <summary><c>golden_torizo_right_sonic_AE4E</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicAE4E,
    /// <summary><c>golden_torizo_right_sonic_AE70</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicAE70,
    /// <summary><c>golden_torizo_right_sonic_AE8A</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicAE8A,
    /// <summary><c>golden_torizo_right_sonic_AEA4</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicAEA4,
    /// <summary><c>golden_torizo_right_sonic_AEBE</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicAEBE,
    /// <summary><c>golden_torizo_right_sonic_AED8</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicAED8,
    /// <summary><c>golden_torizo_right_sonic_AEF2</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicAEF2,
    /// <summary><c>golden_torizo_right_sonic_AF14</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicAF14,
    /// <summary><c>golden_torizo_right_sonic_AF36</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicAF36,
    /// <summary><c>golden_torizo_right_sonic_AF58</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicAF58,
    /// <summary><c>golden_torizo_right_sonic_AF7A</c>, an installed extended frame.</summary>
    GoldenTorizoRightSonicAF7A,
    /// <summary><c>torizo_falling_left_B014</c>, an installed extended frame.</summary>
    TorizoFallingLeftB014,
    /// <summary><c>golden_torizo_left_foot_orb_AC06</c>, an installed extended frame.</summary>
    GoldenTorizoLeftFootOrbAC06,
    /// <summary><c>golden_torizo_left_foot_orb_AC20</c>, an installed extended frame.</summary>
    GoldenTorizoLeftFootOrbAC20,
    /// <summary><c>golden_torizo_left_foot_orb_AC3A</c>, an installed extended frame.</summary>
    GoldenTorizoLeftFootOrbAC3A,
    /// <summary><c>golden_torizo_left_foot_orb_AC54</c>, an installed extended frame.</summary>
    GoldenTorizoLeftFootOrbAC54,
    /// <summary><c>golden_torizo_left_foot_orb_AC6E</c>, an installed extended frame.</summary>
    GoldenTorizoLeftFootOrbAC6E,
    /// <summary><c>torizo_jump_back_left_AFFA</c>, an installed extended frame.</summary>
    TorizoJumpBackLeftAFFA,
    /// <summary><c>torizo_jump_back_left_B02E</c>, an installed extended frame.</summary>
    TorizoJumpBackLeftB02E,
    /// <summary><c>golden_torizo_left_orb_A64E</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrbA64E,
    /// <summary><c>golden_torizo_left_orb_A668</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrbA668,
    /// <summary><c>golden_torizo_left_orb_A682</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrbA682,
    /// <summary><c>golden_torizo_left_orb_A69C</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrbA69C,
    /// <summary><c>golden_torizo_left_orb_A6B6</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrbA6B6,
    /// <summary><c>golden_torizo_left_orb_A6D0</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrbA6D0,
    /// <summary><c>golden_torizo_left_orb_A6EA</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrbA6EA,
    /// <summary><c>golden_torizo_left_orb_A704</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrbA704,
    /// <summary><c>golden_torizo_left_orb_A71E</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrbA71E,
    /// <summary><c>golden_torizo_left_orb_A738</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrbA738,
    /// <summary><c>golden_torizo_left_orb_A752</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrbA752,
    /// <summary><c>golden_torizo_left_orb_A76C</c>, an installed extended frame.</summary>
    GoldenTorizoLeftOrbA76C,
    /// <summary><c>torizo_combat_A786</c>, an installed extended frame.</summary>
    TorizoCombatA786,
    /// <summary><c>torizo_combat_A7A0</c>, an installed extended frame.</summary>
    TorizoCombatA7A0,
    /// <summary><c>torizo_combat_A7C2</c>, an installed extended frame.</summary>
    TorizoCombatA7C2,
    /// <summary><c>torizo_combat_A7E4</c>, an installed extended frame.</summary>
    TorizoCombatA7E4,
    /// <summary><c>torizo_combat_A806</c>, an installed extended frame.</summary>
    TorizoCombatA806,
    /// <summary><c>torizo_combat_A828</c>, an installed extended frame.</summary>
    TorizoCombatA828,
    /// <summary><c>torizo_combat_A84A</c>, an installed extended frame.</summary>
    TorizoCombatA84A,
    /// <summary><c>torizo_combat_A86C</c>, an installed extended frame.</summary>
    TorizoCombatA86C,
    /// <summary><c>torizo_combat_A88E</c>, an installed extended frame.</summary>
    TorizoCombatA88E,
    /// <summary><c>torizo_combat_A8B0</c>, an installed extended frame.</summary>
    TorizoCombatA8B0,
    /// <summary><c>torizo_combat_A8D2</c>, an installed extended frame.</summary>
    TorizoCombatA8D2,
    /// <summary><c>torizo_combat_A8EC</c>, an installed extended frame.</summary>
    TorizoCombatA8EC,
    /// <summary><c>torizo_combat_A906</c>, an installed extended frame.</summary>
    TorizoCombatA906,
    /// <summary><c>torizo_combat_A920</c>, an installed extended frame.</summary>
    TorizoCombatA920,
    /// <summary><c>torizo_combat_A93A</c>, an installed extended frame.</summary>
    TorizoCombatA93A,
    /// <summary><c>torizo_combat_A954</c>, an installed extended frame.</summary>
    TorizoCombatA954,
    /// <summary><c>torizo_combat_A976</c>, an installed extended frame.</summary>
    TorizoCombatA976,
    /// <summary><c>torizo_combat_A998</c>, an installed extended frame.</summary>
    TorizoCombatA998,
    /// <summary><c>torizo_combat_A9BA</c>, an installed extended frame.</summary>
    TorizoCombatA9BA,
    /// <summary><c>torizo_combat_A9DC</c>, an installed extended frame.</summary>
    TorizoCombatA9DC,
    /// <summary><c>torizo_combat_AF9C</c>, an installed extended frame.</summary>
    TorizoCombatAF9C,
    /// <summary><c>torizo_combat_AFA6</c>, an installed extended frame.</summary>
    TorizoCombatAFA6,
    /// <summary><c>torizo_combat_AFB0</c>, an installed extended frame.</summary>
    TorizoCombatAFB0,
    /// <summary><c>torizo_combat_AFBA</c>, an installed extended frame.</summary>
    TorizoCombatAFBA,
    /// <summary><c>torizo_combat_AFC4</c>, an installed extended frame.</summary>
    TorizoCombatAFC4,
    /// <summary><c>torizo_combat_AFD6</c>, an installed extended frame.</summary>
    TorizoCombatAFD6,
    /// <summary><c>torizo_combat_AFE8</c>, an installed extended frame.</summary>
    TorizoCombatAFE8,
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
    PhantoonBg2DEDD,
    /// <summary><c>phantoon_bg2_DEE7</c>, an installed extended frame.</summary>
    PhantoonBg2DEE7,
    /// <summary><c>phantoon_bg2_DEF1</c>, an installed extended frame.</summary>
    PhantoonBg2DEF1,
    /// <summary><c>phantoon_bg2_DEFB</c>, an installed extended frame.</summary>
    PhantoonBg2DEFB,
    /// <summary><c>phantoon_bg2_DF05</c>, an installed extended frame.</summary>
    PhantoonBg2DF05,
    /// <summary><c>phantoon_bg2_DF0F</c>, an installed extended frame.</summary>
    PhantoonBg2DF0F,
    /// <summary><c>phantoon_bg2_DF19</c>, an installed extended frame.</summary>
    PhantoonBg2DF19,
    /// <summary><c>phantoon_bg2_DF23</c>, an installed extended frame.</summary>
    PhantoonBg2DF23,
    /// <summary><c>phantoon_bg2_DF2D</c>, an installed extended frame.</summary>
    PhantoonBg2DF2D,
    /// <summary><c>phantoon_bg2_DF37</c>, an installed extended frame.</summary>
    PhantoonBg2DF37,
    /// <summary><c>phantoon_bg2_DF41</c>, an installed extended frame.</summary>
    PhantoonBg2DF41,
    /// <summary><c>phantoon_bg2_DF4B</c>, an installed extended frame.</summary>
    PhantoonBg2DF4B,
    /// <summary><c>phantoon_bg2_DF55</c>, an installed extended frame.</summary>
    PhantoonBg2DF55,
    /// <summary><c>phantoon_bg2_DF5F</c>, an installed extended frame.</summary>
    PhantoonBg2DF5F,
    /// <summary><c>phantoon_bg2_DF69</c>, an installed extended frame.</summary>
    PhantoonBg2DF69,
    /// <summary><c>phantoon_bg2_DF73</c>, an installed extended frame.</summary>
    PhantoonBg2DF73,
    /// <summary><c>phantoon_bg2_DFB3</c>, an installed extended frame.</summary>
    PhantoonBg2DFB3,
    /// <summary><c>phantoon_bg2_DFC5</c>, an installed extended frame.</summary>
    PhantoonBg2DFC5,
    /// <summary><c>phantoon_bg2_DFD7</c>, an installed extended frame.</summary>
    PhantoonBg2DFD7,
    /// <summary><c>phantoon_bg2_DFE9</c>, an installed extended frame.</summary>
    PhantoonBg2DFE9,
    /// <summary><c>phantoon_bg2_DFF3</c>, an installed extended frame.</summary>
    PhantoonBg2DFF3,
    /// <summary><c>phantoon_bg2_DFFD</c>, an installed extended frame.</summary>
    PhantoonBg2DFFD,
    /// <summary><c>draygon_bg2_A31B</c>, an installed extended frame.</summary>
    DraygonBg2A31B,
    /// <summary><c>draygon_bg2_A325</c>, an installed extended frame.</summary>
    DraygonBg2A325,
    /// <summary><c>draygon_bg2_A32F</c>, an installed extended frame.</summary>
    DraygonBg2A32F,
    /// <summary><c>draygon_bg2_A339</c>, an installed extended frame.</summary>
    DraygonBg2A339,
    /// <summary><c>draygon_bg2_A343</c>, an installed extended frame.</summary>
    DraygonBg2A343,
    /// <summary><c>draygon_bg2_A34D</c>, an installed extended frame.</summary>
    DraygonBg2A34D,
    /// <summary><c>draygon_bg2_A357</c>, an installed extended frame.</summary>
    DraygonBg2A357,
    /// <summary><c>draygon_bg2_A361</c>, an installed extended frame.</summary>
    DraygonBg2A361,
    /// <summary><c>draygon_bg2_A36B</c>, an installed extended frame.</summary>
    DraygonBg2A36B,
    /// <summary><c>draygon_bg2_A375</c>, an installed extended frame.</summary>
    DraygonBg2A375,
    /// <summary><c>draygon_bg2_A37F</c>, an installed extended frame.</summary>
    DraygonBg2A37F,
    /// <summary><c>draygon_bg2_A389</c>, an installed extended frame.</summary>
    DraygonBg2A389,
    /// <summary><c>draygon_bg2_A393</c>, an installed extended frame.</summary>
    DraygonBg2A393,
    /// <summary><c>draygon_bg2_A39D</c>, an installed extended frame.</summary>
    DraygonBg2A39D,
    /// <summary><c>draygon_bg2_A3A7</c>, an installed extended frame.</summary>
    DraygonBg2A3A7,
    /// <summary><c>draygon_bg2_A3B1</c>, an installed extended frame.</summary>
    DraygonBg2A3B1,
    /// <summary><c>draygon_bg2_A3BB</c>, an installed extended frame.</summary>
    DraygonBg2A3BB,
    /// <summary><c>draygon_bg2_A643</c>, an installed extended frame.</summary>
    DraygonBg2A643,
    /// <summary><c>draygon_bg2_A64D</c>, an installed extended frame.</summary>
    DraygonBg2A64D,
    /// <summary><c>draygon_bg2_A657</c>, an installed extended frame.</summary>
    DraygonBg2A657,
    /// <summary><c>draygon_bg2_A661</c>, an installed extended frame.</summary>
    DraygonBg2A661,
    /// <summary><c>draygon_bg2_A66B</c>, an installed extended frame.</summary>
    DraygonBg2A66B,
    /// <summary><c>draygon_bg2_A675</c>, an installed extended frame.</summary>
    DraygonBg2A675,
    /// <summary><c>draygon_bg2_A67F</c>, an installed extended frame.</summary>
    DraygonBg2A67F,
    /// <summary><c>draygon_bg2_A689</c>, an installed extended frame.</summary>
    DraygonBg2A689,
    /// <summary><c>draygon_bg2_A693</c>, an installed extended frame.</summary>
    DraygonBg2A693,
    /// <summary><c>draygon_bg2_A69D</c>, an installed extended frame.</summary>
    DraygonBg2A69D,
    /// <summary><c>draygon_bg2_A6A7</c>, an installed extended frame.</summary>
    DraygonBg2A6A7,
    /// <summary><c>draygon_bg2_A6B1</c>, an installed extended frame.</summary>
    DraygonBg2A6B1,
    /// <summary><c>draygon_bg2_A6BB</c>, an installed extended frame.</summary>
    DraygonBg2A6BB,
    /// <summary><c>draygon_bg2_A6C5</c>, an installed extended frame.</summary>
    DraygonBg2A6C5,
    /// <summary><c>draygon_bg2_A6CF</c>, an installed extended frame.</summary>
    DraygonBg2A6CF,
    /// <summary><c>draygon_bg2_A6D9</c>, an installed extended frame.</summary>
    DraygonBg2A6D9,
    /// <summary><c>draygon_bg2_A6E3</c>, an installed extended frame.</summary>
    DraygonBg2A6E3,
    /// <summary><c>crocomire_skeleton_oam_E1FE</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE1FE,
    /// <summary><c>crocomire_skeleton_oam_E228</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE228,
    /// <summary><c>crocomire_skeleton_oam_E252</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE252,
    /// <summary><c>crocomire_skeleton_oam_E27C</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE27C,
    /// <summary><c>crocomire_skeleton_oam_E2A6</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE2A6,
    /// <summary><c>crocomire_skeleton_oam_E2D0</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE2D0,
    /// <summary><c>crocomire_skeleton_oam_E2FA</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE2FA,
    /// <summary><c>crocomire_skeleton_oam_E324</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE324,
    /// <summary><c>crocomire_skeleton_oam_E34E</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE34E,
    /// <summary><c>crocomire_skeleton_oam_E378</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE378,
    /// <summary><c>crocomire_skeleton_oam_E3A2</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE3A2,
    /// <summary><c>crocomire_skeleton_oam_E3CC</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE3CC,
    /// <summary><c>crocomire_skeleton_oam_E3F6</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE3F6,
    /// <summary><c>crocomire_skeleton_oam_E420</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE420,
    /// <summary><c>crocomire_skeleton_oam_E46A</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE46A,
    /// <summary><c>crocomire_skeleton_oam_E4D4</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE4D4,
    /// <summary><c>crocomire_skeleton_oam_E53E</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE53E,
    /// <summary><c>crocomire_skeleton_oam_E5A8</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE5A8,
    /// <summary><c>crocomire_skeleton_oam_E60A</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE60A,
    /// <summary><c>crocomire_skeleton_oam_E65C</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE65C,
    /// <summary><c>crocomire_skeleton_oam_E68E</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE68E,
    /// <summary><c>crocomire_skeleton_oam_E6A8</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE6A8,
    /// <summary><c>crocomire_skeleton_oam_E6B2</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE6B2,
    /// <summary><c>crocomire_skeleton_oam_E6BC</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE6BC,
    /// <summary><c>crocomire_skeleton_oam_E6C6</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE6C6,
    /// <summary><c>crocomire_skeleton_oam_E6D0</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE6D0,
    /// <summary><c>crocomire_skeleton_oam_E6DA</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE6DA,
    /// <summary><c>crocomire_skeleton_oam_E6E4</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE6E4,
    /// <summary><c>crocomire_skeleton_oam_E6EE</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE6EE,
    /// <summary><c>crocomire_skeleton_oam_E6F8</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE6F8,
    /// <summary><c>crocomire_skeleton_oam_E702</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE702,
    /// <summary><c>crocomire_skeleton_oam_E70C</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE70C,
    /// <summary><c>crocomire_skeleton_oam_E716</c>, an installed extended frame.</summary>
    CrocomireSkeletonOamE716,
    /// <summary><c>kraid_foot_oam_8CE3</c>, an installed extended frame.</summary>
    KraidFootOam8CE3,
    /// <summary><c>kraid_foot_oam_8CF5</c>, an installed extended frame.</summary>
    KraidFootOam8CF5,
    /// <summary><c>kraid_foot_oam_8D07</c>, an installed extended frame.</summary>
    KraidFootOam8D07,
    /// <summary><c>kraid_foot_oam_8D19</c>, an installed extended frame.</summary>
    KraidFootOam8D19,
    /// <summary><c>kraid_foot_oam_8D2B</c>, an installed extended frame.</summary>
    KraidFootOam8D2B,
    /// <summary><c>kraid_foot_oam_8D3D</c>, an installed extended frame.</summary>
    KraidFootOam8D3D,
    /// <summary><c>kraid_foot_oam_8D4F</c>, an installed extended frame.</summary>
    KraidFootOam8D4F,
    /// <summary><c>kraid_foot_oam_8D61</c>, an installed extended frame.</summary>
    KraidFootOam8D61,
    /// <summary><c>kraid_foot_oam_8D73</c>, an installed extended frame.</summary>
    KraidFootOam8D73,
    /// <summary><c>kraid_foot_oam_8D85</c>, an installed extended frame.</summary>
    KraidFootOam8D85,
    /// <summary><c>kraid_foot_oam_8D97</c>, an installed extended frame.</summary>
    KraidFootOam8D97,
    /// <summary><c>kraid_foot_oam_8DA9</c>, an installed extended frame.</summary>
    KraidFootOam8DA9,
    /// <summary><c>kraid_foot_oam_8DBB</c>, an installed extended frame.</summary>
    KraidFootOam8DBB,
    /// <summary><c>kraid_foot_oam_8DCD</c>, an installed extended frame.</summary>
    KraidFootOam8DCD,
    /// <summary><c>kraid_foot_oam_8DDF</c>, an installed extended frame.</summary>
    KraidFootOam8DDF,
    /// <summary><c>kraid_foot_oam_8DF1</c>, an installed extended frame.</summary>
    KraidFootOam8DF1,
    /// <summary><c>kraid_foot_oam_8E03</c>, an installed extended frame.</summary>
    KraidFootOam8E03,
    /// <summary><c>kraid_foot_oam_8E15</c>, an installed extended frame.</summary>
    KraidFootOam8E15,
    /// <summary><c>kraid_foot_oam_8E27</c>, an installed extended frame.</summary>
    KraidFootOam8E27,
    /// <summary><c>kraid_foot_oam_8E39</c>, an installed extended frame.</summary>
    KraidFootOam8E39,
    /// <summary><c>kraid_foot_oam_8E4B</c>, an installed extended frame.</summary>
    KraidFootOam8E4B,
    /// <summary><c>kraid_foot_oam_8E5D</c>, an installed extended frame.</summary>
    KraidFootOam8E5D,
    /// <summary><c>kraid_foot_oam_8E6F</c>, an installed extended frame.</summary>
    KraidFootOam8E6F,
    /// <summary><c>kraid_foot_oam_8E81</c>, an installed extended frame.</summary>
    KraidFootOam8E81,
    /// <summary><c>kraid_foot_oam_8E93</c>, an installed extended frame.</summary>
    KraidFootOam8E93,
    /// <summary><c>kraid_foot_oam_8EA5</c>, an installed extended frame.</summary>
    KraidFootOam8EA5,
    /// <summary><c>kraid_foot_oam_8EB7</c>, an installed extended frame.</summary>
    KraidFootOam8EB7,
    /// <summary><c>kraid_foot_oam_8EC9</c>, an installed extended frame.</summary>
    KraidFootOam8EC9,
    /// <summary><c>kraid_foot_oam_8EDB</c>, an installed extended frame.</summary>
    KraidFootOam8EDB,
    /// <summary><c>kraid_foot_oam_8EED</c>, an installed extended frame.</summary>
    KraidFootOam8EED,
    /// <summary><c>kraid_foot_oam_8EFF</c>, an installed extended frame.</summary>
    KraidFootOam8EFF,
    /// <summary><c>kraid_foot_oam_8F11</c>, an installed extended frame.</summary>
    KraidFootOam8F11,
    /// <summary><c>kraid_foot_oam_8F23</c>, an installed extended frame.</summary>
    KraidFootOam8F23,
    /// <summary><c>kraid_foot_oam_8F47</c>, an installed extended frame.</summary>
    KraidFootOam8F47,
    /// <summary><c>kraid_foot_oam_A565</c>, an installed extended frame.</summary>
    KraidFootOamA565,
    /// <summary>The empty spritemap at the same offset in every enemy bank: an enemy showing it draws nothing.</summary>
    Empty,
    /// <summary>The visually empty one-component extended frame at the same offset in every enemy bank.</summary>
    EmptyExtended,
}
