namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The frames an enemy projectile shows: the compositions its segments select, then every installed animation frame. Each
/// is named for what it shows: its owner, the animation (or the part, facing or stage) that first shows it, and its place
/// there; its values are arbitrary and its installed name is its name in snake case.
/// </summary>
public enum EnemyProjectileFrameId
{
    /// <summary>The empty projectile frame, which draws nothing.</summary>
    Blank,
    /// <summary><c>skree_debris</c>, a composition.</summary>
    SkreeDebris,
    /// <summary><c>metaree_debris</c>, a composition.</summary>
    MetareeDebris,
    /// <summary><c>draygon_goop_first</c>, an installed animation frame.</summary>
    DraygonGoopFirst,
    /// <summary><c>draygon_goop_second</c>, an installed animation frame.</summary>
    DraygonGoopSecond,
    /// <summary><c>draygon_goop_third</c>, an installed animation frame.</summary>
    DraygonGoopThird,
    /// <summary><c>draygon_goop_fourth</c>, an installed animation frame.</summary>
    DraygonGoopFourth,
    /// <summary><c>draygon_goop_fifth</c>, an installed animation frame.</summary>
    DraygonGoopFifth,
    /// <summary><c>draygon_goop_sixth</c>, an installed animation frame.</summary>
    DraygonGoopSixth,
    /// <summary><c>draygon_goop_shot_first</c>, an installed animation frame.</summary>
    DraygonGoopShotFirst,
    /// <summary><c>draygon_goop_shot_second</c>, an installed animation frame.</summary>
    DraygonGoopShotSecond,
    /// <summary><c>draygon_wall_turret_bloom_first</c>, an installed animation frame.</summary>
    DraygonWallTurretBloomFirst,
    /// <summary><c>draygon_wall_turret_bloom_second</c>, an installed animation frame.</summary>
    DraygonWallTurretBloomSecond,
    /// <summary><c>draygon_wall_turret_bloom_third</c>, an installed animation frame.</summary>
    DraygonWallTurretBloomThird,
    /// <summary><c>draygon_wall_turret_bloom_fourth</c>, an installed animation frame.</summary>
    DraygonWallTurretBloomFourth,
    /// <summary><c>draygon_wall_turret_bloom_fifth</c>, an installed animation frame.</summary>
    DraygonWallTurretBloomFifth,
    /// <summary><c>draygon_wall_turret_bloom_sixth</c>, an installed animation frame.</summary>
    DraygonWallTurretBloomSixth,
    /// <summary><c>draygon_wall_turret_bloom_seventh</c>, an installed animation frame.</summary>
    DraygonWallTurretBloomSeventh,
    /// <summary><c>draygon_wall_turret_bloom_eighth</c>, an installed animation frame.</summary>
    DraygonWallTurretBloomEighth,
    /// <summary><c>draygon_wall_turret_bloom_ninth</c>, an installed animation frame.</summary>
    DraygonWallTurretBloomNinth,
    /// <summary><c>draygon_wall_turret_bloom_tenth</c>, an installed animation frame.</summary>
    DraygonWallTurretBloomTenth,
    /// <summary><c>draygon_wall_turret_bloom_eleventh</c>, an installed animation frame.</summary>
    DraygonWallTurretBloomEleventh,
    /// <summary><c>draygon_wall_turret_bloom_twelfth</c>, an installed animation frame.</summary>
    DraygonWallTurretBloomTwelfth,
    /// <summary><c>draygon_wall_turret_bloom_thirteenth</c>, an installed animation frame.</summary>
    DraygonWallTurretBloomThirteenth,
    /// <summary><c>draygon_wall_turret_bloom_fourteenth</c>, an installed animation frame.</summary>
    DraygonWallTurretBloomFourteenth,
    /// <summary><c>draygon_wall_turret_bloom_fifteenth</c>, an installed animation frame.</summary>
    DraygonWallTurretBloomFifteenth,
    /// <summary><c>draygon_wall_turret_bloom_sixteenth</c>, an installed animation frame.</summary>
    DraygonWallTurretBloomSixteenth,
    /// <summary><c>draygon_wall_turret_flight_first</c>, an installed animation frame.</summary>
    DraygonWallTurretFlightFirst,
    /// <summary><c>draygon_wall_turret_flight_second</c>, an installed animation frame.</summary>
    DraygonWallTurretFlightSecond,
    /// <summary><c>draygon_wall_turret_flight_third</c>, an installed animation frame.</summary>
    DraygonWallTurretFlightThird,
    /// <summary><c>crocomire_mouth_first</c>, an installed animation frame.</summary>
    CrocomireMouthFirst,
    /// <summary><c>crocomire_mouth_second</c>, an installed animation frame.</summary>
    CrocomireMouthSecond,
    /// <summary><c>crocomire_mouth_third</c>, an installed animation frame.</summary>
    CrocomireMouthThird,
    /// <summary><c>crocomire_mouth_fourth</c>, an installed animation frame.</summary>
    CrocomireMouthFourth,
    /// <summary><c>crocomire_mouth_fifth</c>, an installed animation frame.</summary>
    CrocomireMouthFifth,
    /// <summary><c>crocomire_mouth_sixth</c>, an installed animation frame.</summary>
    CrocomireMouthSixth,
    /// <summary><c>crocomire_bridge_fragment</c>, an installed animation frame.</summary>
    CrocomireBridgeFragment,
    /// <summary><c>crocomire_spike_wall_piece</c>, an installed animation frame.</summary>
    CrocomireSpikeWallPiece,
    /// <summary><c>crocomire_shot_first</c>, an installed animation frame.</summary>
    CrocomireShotFirst,
    /// <summary><c>crocomire_shot_second</c>, an installed animation frame.</summary>
    CrocomireShotSecond,
    /// <summary><c>crocomire_shot_third</c>, an installed animation frame.</summary>
    CrocomireShotThird,
    /// <summary><c>crocomire_shot_fourth</c>, an installed animation frame.</summary>
    CrocomireShotFourth,
    /// <summary><c>crocomire_shot_fifth</c>, an installed animation frame.</summary>
    CrocomireShotFifth,
    /// <summary><c>ceres_ridley_launch_first</c>, an installed animation frame.</summary>
    CeresRidleyLaunchFirst,
    /// <summary><c>ceres_ridley_launch_second</c>, an installed animation frame.</summary>
    CeresRidleyLaunchSecond,
    /// <summary><c>ceres_ridley_flight_first</c>, an installed animation frame.</summary>
    CeresRidleyFlightFirst,
    /// <summary><c>ceres_ridley_flight_second</c>, an installed animation frame.</summary>
    CeresRidleyFlightSecond,
    /// <summary><c>ceres_ridley_flight_third</c>, an installed animation frame.</summary>
    CeresRidleyFlightThird,
    /// <summary><c>ceres_ridley_flight_fourth</c>, an installed animation frame.</summary>
    CeresRidleyFlightFourth,
    /// <summary><c>ceres_ridley_afterburn_final_first</c>, an installed animation frame.</summary>
    CeresRidleyAfterburnFinalFirst,
    /// <summary><c>ceres_ridley_afterburn_final_second</c>, an installed animation frame.</summary>
    CeresRidleyAfterburnFinalSecond,
    /// <summary><c>ceres_ridley_afterburn_final_third</c>, an installed animation frame.</summary>
    CeresRidleyAfterburnFinalThird,
    /// <summary><c>ceres_ridley_afterburn_final_fourth</c>, an installed animation frame.</summary>
    CeresRidleyAfterburnFinalFourth,
    /// <summary><c>ceres_ridley_afterburn_final_fifth</c>, an installed animation frame.</summary>
    CeresRidleyAfterburnFinalFifth,
    /// <summary><c>ceres_ridley_horizontal_center_first</c>, an installed animation frame.</summary>
    CeresRidleyHorizontalCenterFirst,
    /// <summary><c>ceres_ridley_horizontal_center_second</c>, an installed animation frame.</summary>
    CeresRidleyHorizontalCenterSecond,
    /// <summary><c>ceres_ridley_horizontal_center_third</c>, an installed animation frame.</summary>
    CeresRidleyHorizontalCenterThird,
    /// <summary><c>ceres_ridley_horizontal_center_fourth</c>, an installed animation frame.</summary>
    CeresRidleyHorizontalCenterFourth,
    /// <summary><c>ceres_ridley_horizontal_center_fifth</c>, an installed animation frame.</summary>
    CeresRidleyHorizontalCenterFifth,
    /// <summary><c>ceres_ridley_vertical_center_first</c>, an installed animation frame.</summary>
    CeresRidleyVerticalCenterFirst,
    /// <summary><c>ceres_ridley_vertical_center_second</c>, an installed animation frame.</summary>
    CeresRidleyVerticalCenterSecond,
    /// <summary><c>ceres_ridley_vertical_center_third</c>, an installed animation frame.</summary>
    CeresRidleyVerticalCenterThird,
    /// <summary><c>ceres_ridley_vertical_center_fourth</c>, an installed animation frame.</summary>
    CeresRidleyVerticalCenterFourth,
    /// <summary><c>ceres_ridley_vertical_center_fifth</c>, an installed animation frame.</summary>
    CeresRidleyVerticalCenterFifth,
    /// <summary><c>ceres_ridley_directional_first</c>, an installed animation frame.</summary>
    CeresRidleyDirectionalFirst,
    /// <summary><c>ceres_ridley_directional_second</c>, an installed animation frame.</summary>
    CeresRidleyDirectionalSecond,
    /// <summary><c>ceres_ridley_directional_third</c>, an installed animation frame.</summary>
    CeresRidleyDirectionalThird,
    /// <summary><c>ceres_ridley_directional_fourth</c>, an installed animation frame.</summary>
    CeresRidleyDirectionalFourth,
    /// <summary><c>ceres_ridley_directional_fifth</c>, an installed animation frame.</summary>
    CeresRidleyDirectionalFifth,
    /// <summary><c>ceres_falling_debris_light</c>, an installed animation frame.</summary>
    CeresFallingDebrisLight,
    /// <summary><c>ceres_falling_debris_dark</c>, an installed animation frame.</summary>
    CeresFallingDebrisDark,
    /// <summary><c>phantoon_destroyable_idle_first</c>, an installed animation frame.</summary>
    PhantoonDestroyableIdleFirst,
    /// <summary><c>phantoon_destroyable_idle_second</c>, an installed animation frame.</summary>
    PhantoonDestroyableIdleSecond,
    /// <summary><c>phantoon_destroyable_idle_third</c>, an installed animation frame.</summary>
    PhantoonDestroyableIdleThird,
    /// <summary><c>phantoon_destroyable_idle_fifth</c>, an installed animation frame.</summary>
    PhantoonDestroyableIdleFifth,
    /// <summary><c>phantoon_casual_bouncing_first</c>, an installed animation frame.</summary>
    PhantoonCasualBouncingFirst,
    /// <summary><c>phantoon_casual_bouncing_second</c>, an installed animation frame.</summary>
    PhantoonCasualBouncingSecond,
    /// <summary><c>phantoon_casual_bouncing_third</c>, an installed animation frame.</summary>
    PhantoonCasualBouncingThird,
    /// <summary><c>phantoon_casual_bouncing_fifth</c>, an installed animation frame.</summary>
    PhantoonCasualBouncingFifth,
    /// <summary><c>phantoon_casual_bouncing_sixth</c>, an installed animation frame.</summary>
    PhantoonCasualBouncingSixth,
    /// <summary><c>phantoon_casual_resting_third</c>, an installed animation frame.</summary>
    PhantoonCasualRestingThird,
    /// <summary><c>phantoon_casual_resting_fourth</c>, an installed animation frame.</summary>
    PhantoonCasualRestingFourth,
    /// <summary><c>phantoon_casual_resting_fifth</c>, an installed animation frame.</summary>
    PhantoonCasualRestingFifth,
    /// <summary><c>phantoon_casual_resting_sixth</c>, an installed animation frame.</summary>
    PhantoonCasualRestingSixth,
    /// <summary><c>phantoon_dying_first</c>, an installed animation frame.</summary>
    PhantoonDyingFirst,
    /// <summary><c>phantoon_dying_second</c>, an installed animation frame.</summary>
    PhantoonDyingSecond,
    /// <summary><c>phantoon_dying_third</c>, an installed animation frame.</summary>
    PhantoonDyingThird,
    /// <summary><c>phantoon_dying_fourth</c>, an installed animation frame.</summary>
    PhantoonDyingFourth,
    /// <summary><c>phantoon_rain_impact_first</c>, an installed animation frame.</summary>
    PhantoonRainImpactFirst,
    /// <summary><c>phantoon_rain_impact_fourth</c>, an installed animation frame.</summary>
    PhantoonRainImpactFourth,
    /// <summary><c>phantoon_rain_impact_fifth</c>, an installed animation frame.</summary>
    PhantoonRainImpactFifth,
    /// <summary><c>phantoon_casual_falling_third</c>, an installed animation frame.</summary>
    PhantoonCasualFallingThird,
    /// <summary><c>phantoon_casual_falling_fourth</c>, an installed animation frame.</summary>
    PhantoonCasualFallingFourth,
    /// <summary><c>phantoon_casual_falling_fifth</c>, an installed animation frame.</summary>
    PhantoonCasualFallingFifth,
    /// <summary><c>phantoon_casual_falling_sixth</c>, an installed animation frame.</summary>
    PhantoonCasualFallingSixth,
    /// <summary><c>phantoon_starting_flame_first</c>, an installed animation frame.</summary>
    PhantoonStartingFlameFirst,
    /// <summary><c>phantoon_starting_flame_second</c>, an installed animation frame.</summary>
    PhantoonStartingFlameSecond,
    /// <summary><c>phantoon_starting_flame_third</c>, an installed animation frame.</summary>
    PhantoonStartingFlameThird,
    /// <summary><c>phantoon_destroyable_shot_first</c>, an installed animation frame.</summary>
    PhantoonDestroyableShotFirst,
    /// <summary><c>phantoon_destroyable_shot_second</c>, an installed animation frame.</summary>
    PhantoonDestroyableShotSecond,
    /// <summary><c>phantoon_destroyable_shot_third</c>, an installed animation frame.</summary>
    PhantoonDestroyableShotThird,
    /// <summary><c>phantoon_destroyable_shot_fourth</c>, an installed animation frame.</summary>
    PhantoonDestroyableShotFourth,
    /// <summary><c>kraid_rock</c>, an installed animation frame.</summary>
    KraidRock,
    /// <summary><c>kraid_rock_rising_right_pose</c>, an installed animation frame.</summary>
    KraidRockRisingRightPose,
    /// <summary><c>kraid_rock_burst_first</c>, an installed animation frame.</summary>
    KraidRockBurstFirst,
    /// <summary><c>kraid_rock_burst_second</c>, an installed animation frame.</summary>
    KraidRockBurstSecond,
    /// <summary><c>kraid_rock_burst_third</c>, an installed animation frame.</summary>
    KraidRockBurstThird,
    /// <summary><c>kraid_rock_burst_fourth</c>, an installed animation frame.</summary>
    KraidRockBurstFourth,
    /// <summary><c>kraid_rock_burst_fifth</c>, an installed animation frame.</summary>
    KraidRockBurstFifth,
    /// <summary><c>fake_kraid_spit</c>, an installed animation frame.</summary>
    FakeKraidSpit,
    /// <summary><c>fake_kraid_spike_left</c>, an installed animation frame.</summary>
    FakeKraidSpikeLeft,
    /// <summary><c>fake_kraid_spike_right</c>, an installed animation frame.</summary>
    FakeKraidSpikeRight,
    /// <summary><c>alcoon_fireball_initial_first</c>, an installed animation frame.</summary>
    AlcoonFireballInitialFirst,
    /// <summary><c>alcoon_fireball_initial_second</c>, an installed animation frame.</summary>
    AlcoonFireballInitialSecond,
    /// <summary><c>alcoon_fireball_initial_third</c>, an installed animation frame.</summary>
    AlcoonFireballInitialThird,
    /// <summary><c>alcoon_fireball_initial_fourth</c>, an installed animation frame.</summary>
    AlcoonFireballInitialFourth,
    /// <summary><c>space_pirate_laser_left_first</c>, an installed animation frame.</summary>
    SpacePirateLaserLeftFirst,
    /// <summary><c>space_pirate_laser_left_second</c>, an installed animation frame.</summary>
    SpacePirateLaserLeftSecond,
    /// <summary><c>space_pirate_laser_left_third</c>, an installed animation frame.</summary>
    SpacePirateLaserLeftThird,
    /// <summary><c>space_pirate_laser_left_fourth</c>, an installed animation frame.</summary>
    SpacePirateLaserLeftFourth,
    /// <summary><c>space_pirate_laser_left_fifth</c>, an installed animation frame.</summary>
    SpacePirateLaserLeftFifth,
    /// <summary><c>space_pirate_laser_left_sixth</c>, an installed animation frame.</summary>
    SpacePirateLaserLeftSixth,
    /// <summary><c>space_pirate_laser_left_seventh</c>, an installed animation frame.</summary>
    SpacePirateLaserLeftSeventh,
    /// <summary><c>space_pirate_laser_left_eighth</c>, an installed animation frame.</summary>
    SpacePirateLaserLeftEighth,
    /// <summary><c>space_pirate_laser_left_ninth</c>, an installed animation frame.</summary>
    SpacePirateLaserLeftNinth,
    /// <summary><c>space_pirate_laser_left_tenth</c>, an installed animation frame.</summary>
    SpacePirateLaserLeftTenth,
    /// <summary><c>space_pirate_laser_left_eleventh</c>, an installed animation frame.</summary>
    SpacePirateLaserLeftEleventh,
    /// <summary><c>space_pirate_laser_left_loop_first</c>, an installed animation frame.</summary>
    SpacePirateLaserLeftLoopFirst,
    /// <summary><c>space_pirate_laser_left_loop_second</c>, an installed animation frame.</summary>
    SpacePirateLaserLeftLoopSecond,
    /// <summary><c>space_pirate_laser_right_first</c>, an installed animation frame.</summary>
    SpacePirateLaserRightFirst,
    /// <summary><c>space_pirate_laser_right_second</c>, an installed animation frame.</summary>
    SpacePirateLaserRightSecond,
    /// <summary><c>space_pirate_laser_right_third</c>, an installed animation frame.</summary>
    SpacePirateLaserRightThird,
    /// <summary><c>space_pirate_laser_right_fourth</c>, an installed animation frame.</summary>
    SpacePirateLaserRightFourth,
    /// <summary><c>space_pirate_laser_right_fifth</c>, an installed animation frame.</summary>
    SpacePirateLaserRightFifth,
    /// <summary><c>space_pirate_laser_right_sixth</c>, an installed animation frame.</summary>
    SpacePirateLaserRightSixth,
    /// <summary><c>space_pirate_laser_right_seventh</c>, an installed animation frame.</summary>
    SpacePirateLaserRightSeventh,
    /// <summary><c>space_pirate_laser_right_eighth</c>, an installed animation frame.</summary>
    SpacePirateLaserRightEighth,
    /// <summary><c>space_pirate_laser_right_ninth</c>, an installed animation frame.</summary>
    SpacePirateLaserRightNinth,
    /// <summary><c>space_pirate_laser_right_tenth</c>, an installed animation frame.</summary>
    SpacePirateLaserRightTenth,
    /// <summary><c>space_pirate_laser_right_eleventh</c>, an installed animation frame.</summary>
    SpacePirateLaserRightEleventh,
    /// <summary><c>space_pirate_laser_right_loop_first</c>, an installed animation frame.</summary>
    SpacePirateLaserRightLoopFirst,
    /// <summary><c>space_pirate_laser_right_loop_second</c>, an installed animation frame.</summary>
    SpacePirateLaserRightLoopSecond,
    /// <summary><c>space_pirate_claw_left_loop_first</c>, an installed animation frame.</summary>
    SpacePirateClawLeftLoopFirst,
    /// <summary><c>space_pirate_claw_left_loop_second</c>, an installed animation frame.</summary>
    SpacePirateClawLeftLoopSecond,
    /// <summary><c>space_pirate_claw_left_loop_third</c>, an installed animation frame.</summary>
    SpacePirateClawLeftLoopThird,
    /// <summary><c>space_pirate_claw_left_loop_fourth</c>, an installed animation frame.</summary>
    SpacePirateClawLeftLoopFourth,
    /// <summary><c>space_pirate_claw_left_loop_fifth</c>, an installed animation frame.</summary>
    SpacePirateClawLeftLoopFifth,
    /// <summary><c>space_pirate_claw_left_loop_sixth</c>, an installed animation frame.</summary>
    SpacePirateClawLeftLoopSixth,
    /// <summary><c>space_pirate_claw_left_loop_seventh</c>, an installed animation frame.</summary>
    SpacePirateClawLeftLoopSeventh,
    /// <summary><c>space_pirate_claw_left_loop_eighth</c>, an installed animation frame.</summary>
    SpacePirateClawLeftLoopEighth,
    /// <summary><c>space_pirate_claw_right_loop_first</c>, an installed animation frame.</summary>
    SpacePirateClawRightLoopFirst,
    /// <summary><c>space_pirate_claw_right_loop_second</c>, an installed animation frame.</summary>
    SpacePirateClawRightLoopSecond,
    /// <summary><c>space_pirate_claw_right_loop_third</c>, an installed animation frame.</summary>
    SpacePirateClawRightLoopThird,
    /// <summary><c>space_pirate_claw_right_loop_fourth</c>, an installed animation frame.</summary>
    SpacePirateClawRightLoopFourth,
    /// <summary><c>space_pirate_claw_right_loop_fifth</c>, an installed animation frame.</summary>
    SpacePirateClawRightLoopFifth,
    /// <summary><c>space_pirate_claw_right_loop_sixth</c>, an installed animation frame.</summary>
    SpacePirateClawRightLoopSixth,
    /// <summary><c>space_pirate_claw_right_loop_seventh</c>, an installed animation frame.</summary>
    SpacePirateClawRightLoopSeventh,
    /// <summary><c>space_pirate_claw_right_loop_eighth</c>, an installed animation frame.</summary>
    SpacePirateClawRightLoopEighth,
    /// <summary><c>gunship_dust_right_inner_loop_first</c>, an installed animation frame.</summary>
    GunshipDustRightInnerLoopFirst,
    /// <summary><c>gunship_dust_right_inner_loop_second</c>, an installed animation frame.</summary>
    GunshipDustRightInnerLoopSecond,
    /// <summary><c>gunship_dust_right_inner_loop_third</c>, an installed animation frame.</summary>
    GunshipDustRightInnerLoopThird,
    /// <summary><c>gunship_dust_right_inner_loop_fourth</c>, an installed animation frame.</summary>
    GunshipDustRightInnerLoopFourth,
    /// <summary><c>gunship_dust_right_inner_loop_fifth</c>, an installed animation frame.</summary>
    GunshipDustRightInnerLoopFifth,
    /// <summary><c>gunship_dust_right_inner_loop_sixth</c>, an installed animation frame.</summary>
    GunshipDustRightInnerLoopSixth,
    /// <summary><c>gunship_dust_right_inner_loop_seventh</c>, an installed animation frame.</summary>
    GunshipDustRightInnerLoopSeventh,
    /// <summary><c>gunship_dust_right_inner_loop_eighth</c>, an installed animation frame.</summary>
    GunshipDustRightInnerLoopEighth,
    /// <summary><c>gunship_dust_right_middle_loop_first</c>, an installed animation frame.</summary>
    GunshipDustRightMiddleLoopFirst,
    /// <summary><c>gunship_dust_right_middle_loop_second</c>, an installed animation frame.</summary>
    GunshipDustRightMiddleLoopSecond,
    /// <summary><c>gunship_dust_right_middle_loop_third</c>, an installed animation frame.</summary>
    GunshipDustRightMiddleLoopThird,
    /// <summary><c>gunship_dust_right_middle_loop_fourth</c>, an installed animation frame.</summary>
    GunshipDustRightMiddleLoopFourth,
    /// <summary><c>gunship_dust_right_middle_loop_fifth</c>, an installed animation frame.</summary>
    GunshipDustRightMiddleLoopFifth,
    /// <summary><c>gunship_dust_right_middle_loop_sixth</c>, an installed animation frame.</summary>
    GunshipDustRightMiddleLoopSixth,
    /// <summary><c>gunship_dust_right_middle_loop_seventh</c>, an installed animation frame.</summary>
    GunshipDustRightMiddleLoopSeventh,
    /// <summary><c>gunship_dust_right_middle_loop_eighth</c>, an installed animation frame.</summary>
    GunshipDustRightMiddleLoopEighth,
    /// <summary><c>gunship_dust_right_outer_loop_first</c>, an installed animation frame.</summary>
    GunshipDustRightOuterLoopFirst,
    /// <summary><c>gunship_dust_right_outer_loop_second</c>, an installed animation frame.</summary>
    GunshipDustRightOuterLoopSecond,
    /// <summary><c>gunship_dust_right_outer_loop_third</c>, an installed animation frame.</summary>
    GunshipDustRightOuterLoopThird,
    /// <summary><c>gunship_dust_right_outer_loop_fourth</c>, an installed animation frame.</summary>
    GunshipDustRightOuterLoopFourth,
    /// <summary><c>gunship_dust_right_outer_loop_fifth</c>, an installed animation frame.</summary>
    GunshipDustRightOuterLoopFifth,
    /// <summary><c>gunship_dust_right_outer_loop_sixth</c>, an installed animation frame.</summary>
    GunshipDustRightOuterLoopSixth,
    /// <summary><c>gunship_dust_right_outer_loop_seventh</c>, an installed animation frame.</summary>
    GunshipDustRightOuterLoopSeventh,
    /// <summary><c>gunship_dust_left_inner_loop_first</c>, an installed animation frame.</summary>
    GunshipDustLeftInnerLoopFirst,
    /// <summary><c>gunship_dust_left_inner_loop_second</c>, an installed animation frame.</summary>
    GunshipDustLeftInnerLoopSecond,
    /// <summary><c>gunship_dust_left_inner_loop_third</c>, an installed animation frame.</summary>
    GunshipDustLeftInnerLoopThird,
    /// <summary><c>gunship_dust_left_inner_loop_fourth</c>, an installed animation frame.</summary>
    GunshipDustLeftInnerLoopFourth,
    /// <summary><c>gunship_dust_left_inner_loop_fifth</c>, an installed animation frame.</summary>
    GunshipDustLeftInnerLoopFifth,
    /// <summary><c>gunship_dust_left_inner_loop_sixth</c>, an installed animation frame.</summary>
    GunshipDustLeftInnerLoopSixth,
    /// <summary><c>gunship_dust_left_inner_loop_seventh</c>, an installed animation frame.</summary>
    GunshipDustLeftInnerLoopSeventh,
    /// <summary><c>gunship_dust_left_inner_loop_eighth</c>, an installed animation frame.</summary>
    GunshipDustLeftInnerLoopEighth,
    /// <summary><c>gunship_dust_left_middle_loop_first</c>, an installed animation frame.</summary>
    GunshipDustLeftMiddleLoopFirst,
    /// <summary><c>gunship_dust_left_middle_loop_second</c>, an installed animation frame.</summary>
    GunshipDustLeftMiddleLoopSecond,
    /// <summary><c>gunship_dust_left_middle_loop_third</c>, an installed animation frame.</summary>
    GunshipDustLeftMiddleLoopThird,
    /// <summary><c>gunship_dust_left_middle_loop_fourth</c>, an installed animation frame.</summary>
    GunshipDustLeftMiddleLoopFourth,
    /// <summary><c>gunship_dust_left_middle_loop_fifth</c>, an installed animation frame.</summary>
    GunshipDustLeftMiddleLoopFifth,
    /// <summary><c>gunship_dust_left_middle_loop_sixth</c>, an installed animation frame.</summary>
    GunshipDustLeftMiddleLoopSixth,
    /// <summary><c>gunship_dust_left_middle_loop_seventh</c>, an installed animation frame.</summary>
    GunshipDustLeftMiddleLoopSeventh,
    /// <summary><c>gunship_dust_left_middle_loop_eighth</c>, an installed animation frame.</summary>
    GunshipDustLeftMiddleLoopEighth,
    /// <summary><c>gunship_dust_left_outer_loop_first</c>, an installed animation frame.</summary>
    GunshipDustLeftOuterLoopFirst,
    /// <summary><c>gunship_dust_left_outer_loop_second</c>, an installed animation frame.</summary>
    GunshipDustLeftOuterLoopSecond,
    /// <summary><c>gunship_dust_left_outer_loop_third</c>, an installed animation frame.</summary>
    GunshipDustLeftOuterLoopThird,
    /// <summary><c>gunship_dust_left_outer_loop_fourth</c>, an installed animation frame.</summary>
    GunshipDustLeftOuterLoopFourth,
    /// <summary><c>gunship_dust_left_outer_loop_fifth</c>, an installed animation frame.</summary>
    GunshipDustLeftOuterLoopFifth,
    /// <summary><c>gunship_dust_left_outer_loop_sixth</c>, an installed animation frame.</summary>
    GunshipDustLeftOuterLoopSixth,
    /// <summary><c>gunship_dust_left_outer_loop_seventh</c>, an installed animation frame.</summary>
    GunshipDustLeftOuterLoopSeventh,
    /// <summary><c>torizo_explosion_low_health_initial_loop_first</c>, an installed animation frame.</summary>
    TorizoExplosionLowHealthInitialLoopFirst,
    /// <summary><c>torizo_explosion_low_health_initial_loop_second</c>, an installed animation frame.</summary>
    TorizoExplosionLowHealthInitialLoopSecond,
    /// <summary><c>torizo_explosion_low_health_initial_loop_third</c>, an installed animation frame.</summary>
    TorizoExplosionLowHealthInitialLoopThird,
    /// <summary><c>torizo_explosion_low_health_initial_loop_fourth</c>, an installed animation frame.</summary>
    TorizoExplosionLowHealthInitialLoopFourth,
    /// <summary><c>torizo_explosion_low_health_initial_loop_fifth</c>, an installed animation frame.</summary>
    TorizoExplosionLowHealthInitialLoopFifth,
    /// <summary><c>torizo_explosion_death_initial_loop_first</c>, an installed animation frame.</summary>
    TorizoExplosionDeathInitialLoopFirst,
    /// <summary><c>torizo_explosion_death_initial_loop_second</c>, an installed animation frame.</summary>
    TorizoExplosionDeathInitialLoopSecond,
    /// <summary><c>torizo_explosion_death_initial_loop_third</c>, an installed animation frame.</summary>
    TorizoExplosionDeathInitialLoopThird,
    /// <summary><c>torizo_explosion_death_initial_loop_fourth</c>, an installed animation frame.</summary>
    TorizoExplosionDeathInitialLoopFourth,
    /// <summary><c>torizo_explosion_death_initial_loop_fifth</c>, an installed animation frame.</summary>
    TorizoExplosionDeathInitialLoopFifth,
    /// <summary><c>torizo_explosion_death_initial_loop_sixth</c>, an installed animation frame.</summary>
    TorizoExplosionDeathInitialLoopSixth,
    /// <summary><c>torizo_explosion_death_smoke_setup_loop_first</c>, an installed animation frame.</summary>
    TorizoExplosionDeathSmokeSetupLoopFirst,
    /// <summary><c>torizo_explosion_death_smoke_setup_loop_second</c>, an installed animation frame.</summary>
    TorizoExplosionDeathSmokeSetupLoopSecond,
    /// <summary><c>torizo_explosion_death_smoke_setup_loop_third</c>, an installed animation frame.</summary>
    TorizoExplosionDeathSmokeSetupLoopThird,
    /// <summary><c>torizo_explosion_death_smoke_setup_loop_fourth</c>, an installed animation frame.</summary>
    TorizoExplosionDeathSmokeSetupLoopFourth,
    /// <summary><c>torizo_drool_four_frame_delay</c>, an installed animation frame.</summary>
    TorizoDroolFourFrameDelay,
    /// <summary><c>torizo_drool_two_frame_delay</c>, an installed animation frame.</summary>
    TorizoDroolTwoFrameDelay,
    /// <summary><c>torizo_drool_no_delay</c>, an installed animation frame.</summary>
    TorizoDroolNoDelay,
    /// <summary><c>torizo_drool_falling_loop</c>, an installed animation frame.</summary>
    TorizoDroolFallingLoop,
    /// <summary><c>torizo_drool_floor_impact_first</c>, an installed animation frame.</summary>
    TorizoDroolFloorImpactFirst,
    /// <summary><c>torizo_drool_floor_impact_second</c>, an installed animation frame.</summary>
    TorizoDroolFloorImpactSecond,
    /// <summary><c>torizo_drool_floor_impact_third</c>, an installed animation frame.</summary>
    TorizoDroolFloorImpactThird,
    /// <summary><c>torizo_explosive_swipe_initial_first</c>, an installed animation frame.</summary>
    TorizoExplosiveSwipeInitialFirst,
    /// <summary><c>torizo_explosive_swipe_initial_second</c>, an installed animation frame.</summary>
    TorizoExplosiveSwipeInitialSecond,
    /// <summary><c>torizo_explosive_swipe_initial_third</c>, an installed animation frame.</summary>
    TorizoExplosiveSwipeInitialThird,
    /// <summary><c>torizo_explosive_swipe_initial_fourth</c>, an installed animation frame.</summary>
    TorizoExplosiveSwipeInitialFourth,
    /// <summary><c>torizo_explosive_swipe_initial_fifth</c>, an installed animation frame.</summary>
    TorizoExplosiveSwipeInitialFifth,
    /// <summary><c>bomb_torizo_first_statue_fragment_first</c>, an installed animation frame.</summary>
    BombTorizoFirstStatueFragmentFirst,
    /// <summary><c>bomb_torizo_first_statue_fragment_second</c>, an installed animation frame.</summary>
    BombTorizoFirstStatueFragmentSecond,
    /// <summary><c>bomb_torizo_second_statue_fragment_first</c>, an installed animation frame.</summary>
    BombTorizoSecondStatueFragmentFirst,
    /// <summary><c>bomb_torizo_second_statue_fragment_second</c>, an installed animation frame.</summary>
    BombTorizoSecondStatueFragmentSecond,
    /// <summary><c>bomb_torizo_third_statue_fragment_first</c>, an installed animation frame.</summary>
    BombTorizoThirdStatueFragmentFirst,
    /// <summary><c>bomb_torizo_third_statue_fragment_second</c>, an installed animation frame.</summary>
    BombTorizoThirdStatueFragmentSecond,
    /// <summary><c>bomb_torizo_fourth_statue_fragment_first</c>, an installed animation frame.</summary>
    BombTorizoFourthStatueFragmentFirst,
    /// <summary><c>bomb_torizo_fourth_statue_fragment_second</c>, an installed animation frame.</summary>
    BombTorizoFourthStatueFragmentSecond,
    /// <summary><c>bomb_torizo_fifth_statue_fragment_first</c>, an installed animation frame.</summary>
    BombTorizoFifthStatueFragmentFirst,
    /// <summary><c>bomb_torizo_fifth_statue_fragment_second</c>, an installed animation frame.</summary>
    BombTorizoFifthStatueFragmentSecond,
    /// <summary><c>bomb_torizo_sixth_statue_fragment_first</c>, an installed animation frame.</summary>
    BombTorizoSixthStatueFragmentFirst,
    /// <summary><c>bomb_torizo_sixth_statue_fragment_second</c>, an installed animation frame.</summary>
    BombTorizoSixthStatueFragmentSecond,
    /// <summary><c>bomb_torizo_seventh_statue_fragment_first</c>, an installed animation frame.</summary>
    BombTorizoSeventhStatueFragmentFirst,
    /// <summary><c>bomb_torizo_seventh_statue_fragment_second</c>, an installed animation frame.</summary>
    BombTorizoSeventhStatueFragmentSecond,
    /// <summary><c>bomb_torizo_eighth_statue_fragment_first</c>, an installed animation frame.</summary>
    BombTorizoEighthStatueFragmentFirst,
    /// <summary><c>bomb_torizo_eighth_statue_fragment_second</c>, an installed animation frame.</summary>
    BombTorizoEighthStatueFragmentSecond,
    /// <summary><c>bomb_torizo_ninth_statue_fragment_first</c>, an installed animation frame.</summary>
    BombTorizoNinthStatueFragmentFirst,
    /// <summary><c>bomb_torizo_ninth_statue_fragment_second</c>, an installed animation frame.</summary>
    BombTorizoNinthStatueFragmentSecond,
    /// <summary><c>bomb_torizo_tenth_statue_fragment_first</c>, an installed animation frame.</summary>
    BombTorizoTenthStatueFragmentFirst,
    /// <summary><c>bomb_torizo_tenth_statue_fragment_second</c>, an installed animation frame.</summary>
    BombTorizoTenthStatueFragmentSecond,
    /// <summary><c>bomb_torizo_eleventh_statue_fragment_first</c>, an installed animation frame.</summary>
    BombTorizoEleventhStatueFragmentFirst,
    /// <summary><c>bomb_torizo_eleventh_statue_fragment_second</c>, an installed animation frame.</summary>
    BombTorizoEleventhStatueFragmentSecond,
    /// <summary><c>bomb_torizo_twelfth_statue_fragment_first</c>, an installed animation frame.</summary>
    BombTorizoTwelfthStatueFragmentFirst,
    /// <summary><c>bomb_torizo_twelfth_statue_fragment_second</c>, an installed animation frame.</summary>
    BombTorizoTwelfthStatueFragmentSecond,
    /// <summary><c>bomb_torizo_thirteenth_statue_fragment_first</c>, an installed animation frame.</summary>
    BombTorizoThirteenthStatueFragmentFirst,
    /// <summary><c>bomb_torizo_thirteenth_statue_fragment_second</c>, an installed animation frame.</summary>
    BombTorizoThirteenthStatueFragmentSecond,
    /// <summary><c>bomb_torizo_fourteenth_statue_fragment_first</c>, an installed animation frame.</summary>
    BombTorizoFourteenthStatueFragmentFirst,
    /// <summary><c>bomb_torizo_fourteenth_statue_fragment_second</c>, an installed animation frame.</summary>
    BombTorizoFourteenthStatueFragmentSecond,
    /// <summary><c>bomb_torizo_fifteenth_statue_fragment_first</c>, an installed animation frame.</summary>
    BombTorizoFifteenthStatueFragmentFirst,
    /// <summary><c>bomb_torizo_fifteenth_statue_fragment_second</c>, an installed animation frame.</summary>
    BombTorizoFifteenthStatueFragmentSecond,
    /// <summary><c>bomb_torizo_sixteenth_statue_fragment_first</c>, an installed animation frame.</summary>
    BombTorizoSixteenthStatueFragmentFirst,
    /// <summary><c>bomb_torizo_sixteenth_statue_fragment_second</c>, an installed animation frame.</summary>
    BombTorizoSixteenthStatueFragmentSecond,
    /// <summary><c>torizo_chozo_orb_moving_left</c>, an installed animation frame.</summary>
    TorizoChozoOrbMovingLeft,
    /// <summary><c>torizo_chozo_orb_moving_right</c>, an installed animation frame.</summary>
    TorizoChozoOrbMovingRight,
    /// <summary><c>torizo_chozo_orb_wall_impact_first</c>, an installed animation frame.</summary>
    TorizoChozoOrbWallImpactFirst,
    /// <summary><c>torizo_chozo_orb_wall_impact_second</c>, an installed animation frame.</summary>
    TorizoChozoOrbWallImpactSecond,
    /// <summary><c>torizo_chozo_orb_wall_impact_third</c>, an installed animation frame.</summary>
    TorizoChozoOrbWallImpactThird,
    /// <summary><c>torizo_chozo_orb_wall_impact_fourth</c>, an installed animation frame.</summary>
    TorizoChozoOrbWallImpactFourth,
    /// <summary><c>torizo_chozo_orb_wall_impact_fifth</c>, an installed animation frame.</summary>
    TorizoChozoOrbWallImpactFifth,
    /// <summary><c>torizo_chozo_orb_floor_impact_first</c>, an installed animation frame.</summary>
    TorizoChozoOrbFloorImpactFirst,
    /// <summary><c>torizo_chozo_orb_floor_impact_second</c>, an installed animation frame.</summary>
    TorizoChozoOrbFloorImpactSecond,
    /// <summary><c>torizo_chozo_orb_floor_impact_third</c>, an installed animation frame.</summary>
    TorizoChozoOrbFloorImpactThird,
    /// <summary><c>torizo_chozo_orb_floor_impact_fourth</c>, an installed animation frame.</summary>
    TorizoChozoOrbFloorImpactFourth,
    /// <summary><c>torizo_chozo_orb_floor_impact_fifth</c>, an installed animation frame.</summary>
    TorizoChozoOrbFloorImpactFifth,
    /// <summary><c>torizo_chozo_orb_floor_impact_sixth</c>, an installed animation frame.</summary>
    TorizoChozoOrbFloorImpactSixth,
    /// <summary><c>torizo_chozo_orb_shot_first</c>, an installed animation frame.</summary>
    TorizoChozoOrbShotFirst,
    /// <summary><c>torizo_chozo_orb_shot_second</c>, an installed animation frame.</summary>
    TorizoChozoOrbShotSecond,
    /// <summary><c>torizo_chozo_orb_shot_third</c>, an installed animation frame.</summary>
    TorizoChozoOrbShotThird,
    /// <summary><c>torizo_chozo_orb_shot_fourth</c>, an installed animation frame.</summary>
    TorizoChozoOrbShotFourth,
    /// <summary><c>torizo_chozo_orb_shot_fifth</c>, an installed animation frame.</summary>
    TorizoChozoOrbShotFifth,
    /// <summary><c>torizo_sonic_boom_fired_left_first</c>, an installed animation frame.</summary>
    TorizoSonicBoomFiredLeftFirst,
    /// <summary><c>torizo_sonic_boom_fired_left_second</c>, an installed animation frame.</summary>
    TorizoSonicBoomFiredLeftSecond,
    /// <summary><c>torizo_sonic_boom_moving_left</c>, an installed animation frame.</summary>
    TorizoSonicBoomMovingLeft,
    /// <summary><c>torizo_sonic_boom_fired_right_first</c>, an installed animation frame.</summary>
    TorizoSonicBoomFiredRightFirst,
    /// <summary><c>torizo_sonic_boom_fired_right_second</c>, an installed animation frame.</summary>
    TorizoSonicBoomFiredRightSecond,
    /// <summary><c>torizo_sonic_boom_moving_right</c>, an installed animation frame.</summary>
    TorizoSonicBoomMovingRight,
    /// <summary><c>torizo_sonic_boom_wall_impact_loop_first</c>, an installed animation frame.</summary>
    TorizoSonicBoomWallImpactLoopFirst,
    /// <summary><c>torizo_sonic_boom_wall_impact_loop_second</c>, an installed animation frame.</summary>
    TorizoSonicBoomWallImpactLoopSecond,
    /// <summary><c>torizo_sonic_boom_wall_impact_loop_third</c>, an installed animation frame.</summary>
    TorizoSonicBoomWallImpactLoopThird,
    /// <summary><c>torizo_sonic_boom_wall_impact_loop_fourth</c>, an installed animation frame.</summary>
    TorizoSonicBoomWallImpactLoopFourth,
    /// <summary><c>torizo_sonic_boom_wall_impact_loop_fifth</c>, an installed animation frame.</summary>
    TorizoSonicBoomWallImpactLoopFifth,
    /// <summary><c>chozo_tourian_dust_footsteps_first</c>, an installed animation frame.</summary>
    ChozoTourianDustFootstepsFirst,
    /// <summary><c>chozo_tourian_dust_footsteps_second</c>, an installed animation frame.</summary>
    ChozoTourianDustFootstepsSecond,
    /// <summary><c>chozo_tourian_dust_footsteps_third</c>, an installed animation frame.</summary>
    ChozoTourianDustFootstepsThird,
    /// <summary><c>chozo_tourian_dust_footsteps_fourth</c>, an installed animation frame.</summary>
    ChozoTourianDustFootstepsFourth,
    /// <summary><c>chozo_tourian_dust_spike_clearing_explosions_first</c>, an installed animation frame.</summary>
    ChozoTourianDustSpikeClearingExplosionsFirst,
    /// <summary><c>chozo_tourian_dust_spike_clearing_explosions_second</c>, an installed animation frame.</summary>
    ChozoTourianDustSpikeClearingExplosionsSecond,
    /// <summary><c>chozo_tourian_dust_spike_clearing_explosions_third</c>, an installed animation frame.</summary>
    ChozoTourianDustSpikeClearingExplosionsThird,
    /// <summary><c>chozo_tourian_dust_spike_clearing_explosions_fourth</c>, an installed animation frame.</summary>
    ChozoTourianDustSpikeClearingExplosionsFourth,
    /// <summary><c>chozo_tourian_dust_spike_clearing_explosions_fifth</c>, an installed animation frame.</summary>
    ChozoTourianDustSpikeClearingExplosionsFifth,
    /// <summary><c>chozo_tourian_dust_spike_clearing_explosions_sixth</c>, an installed animation frame.</summary>
    ChozoTourianDustSpikeClearingExplosionsSixth,
    /// <summary><c>chozo_tourian_dust_tourian_descent_dust_first</c>, an installed animation frame.</summary>
    ChozoTourianDustTourianDescentDustFirst,
    /// <summary><c>chozo_tourian_dust_tourian_descent_dust_second</c>, an installed animation frame.</summary>
    ChozoTourianDustTourianDescentDustSecond,
    /// <summary><c>chozo_tourian_dust_tourian_descent_dust_third</c>, an installed animation frame.</summary>
    ChozoTourianDustTourianDescentDustThird,
    /// <summary><c>chozo_tourian_dust_tourian_descent_dust_fourth</c>, an installed animation frame.</summary>
    ChozoTourianDustTourianDescentDustFourth,
    /// <summary><c>torizo_landing_dust_right_foot_first</c>, an installed animation frame.</summary>
    TorizoLandingDustRightFootFirst,
    /// <summary><c>torizo_landing_dust_right_foot_second</c>, an installed animation frame.</summary>
    TorizoLandingDustRightFootSecond,
    /// <summary><c>torizo_landing_dust_right_foot_third</c>, an installed animation frame.</summary>
    TorizoLandingDustRightFootThird,
    /// <summary><c>torizo_landing_dust_right_foot_fourth</c>, an installed animation frame.</summary>
    TorizoLandingDustRightFootFourth,
    /// <summary><c>torizo_landing_dust_left_foot_first</c>, an installed animation frame.</summary>
    TorizoLandingDustLeftFootFirst,
    /// <summary><c>torizo_landing_dust_left_foot_second</c>, an installed animation frame.</summary>
    TorizoLandingDustLeftFootSecond,
    /// <summary><c>torizo_landing_dust_left_foot_third</c>, an installed animation frame.</summary>
    TorizoLandingDustLeftFootThird,
    /// <summary><c>torizo_landing_dust_left_foot_fourth</c>, an installed animation frame.</summary>
    TorizoLandingDustLeftFootFourth,
    /// <summary><c>golden_torizo_egg_bouncing_left</c>, an installed animation frame.</summary>
    GoldenTorizoEggBouncingLeft,
    /// <summary><c>golden_torizo_egg_bouncing_left_hatching_first</c>, an installed animation frame.</summary>
    GoldenTorizoEggBouncingLeftHatchingFirst,
    /// <summary><c>golden_torizo_egg_bouncing_left_hatching_second</c>, an installed animation frame.</summary>
    GoldenTorizoEggBouncingLeftHatchingSecond,
    /// <summary><c>golden_torizo_egg_bouncing_left_hatching_third</c>, an installed animation frame.</summary>
    GoldenTorizoEggBouncingLeftHatchingThird,
    /// <summary><c>golden_torizo_egg_bouncing_right</c>, an installed animation frame.</summary>
    GoldenTorizoEggBouncingRight,
    /// <summary><c>golden_torizo_egg_bouncing_right_hatching_first</c>, an installed animation frame.</summary>
    GoldenTorizoEggBouncingRightHatchingFirst,
    /// <summary><c>golden_torizo_egg_bouncing_right_hatching_second</c>, an installed animation frame.</summary>
    GoldenTorizoEggBouncingRightHatchingSecond,
    /// <summary><c>golden_torizo_egg_bouncing_right_hatching_third</c>, an installed animation frame.</summary>
    GoldenTorizoEggBouncingRightHatchingThird,
    /// <summary><c>golden_torizo_egg_hatched_left_loop_first</c>, an installed animation frame.</summary>
    GoldenTorizoEggHatchedLeftLoopFirst,
    /// <summary><c>golden_torizo_egg_hatched_left_loop_second</c>, an installed animation frame.</summary>
    GoldenTorizoEggHatchedLeftLoopSecond,
    /// <summary><c>golden_torizo_egg_hatched_left_loop_third</c>, an installed animation frame.</summary>
    GoldenTorizoEggHatchedLeftLoopThird,
    /// <summary><c>golden_torizo_egg_hatched_left_loop_fourth</c>, an installed animation frame.</summary>
    GoldenTorizoEggHatchedLeftLoopFourth,
    /// <summary><c>golden_torizo_egg_hatched_right_loop_first</c>, an installed animation frame.</summary>
    GoldenTorizoEggHatchedRightLoopFirst,
    /// <summary><c>golden_torizo_egg_hatched_right_loop_second</c>, an installed animation frame.</summary>
    GoldenTorizoEggHatchedRightLoopSecond,
    /// <summary><c>golden_torizo_egg_hatched_right_loop_third</c>, an installed animation frame.</summary>
    GoldenTorizoEggHatchedRightLoopThird,
    /// <summary><c>golden_torizo_egg_hatched_right_loop_fourth</c>, an installed animation frame.</summary>
    GoldenTorizoEggHatchedRightLoopFourth,
    /// <summary><c>golden_torizo_egg_break_left_first</c>, an installed animation frame.</summary>
    GoldenTorizoEggBreakLeftFirst,
    /// <summary><c>golden_torizo_egg_break_left_second</c>, an installed animation frame.</summary>
    GoldenTorizoEggBreakLeftSecond,
    /// <summary><c>golden_torizo_egg_break_left_third</c>, an installed animation frame.</summary>
    GoldenTorizoEggBreakLeftThird,
    /// <summary><c>golden_torizo_egg_break_left_fourth</c>, an installed animation frame.</summary>
    GoldenTorizoEggBreakLeftFourth,
    /// <summary><c>golden_torizo_egg_break_left_fifth</c>, an installed animation frame.</summary>
    GoldenTorizoEggBreakLeftFifth,
    /// <summary><c>golden_torizo_egg_break_right_first</c>, an installed animation frame.</summary>
    GoldenTorizoEggBreakRightFirst,
    /// <summary><c>golden_torizo_egg_break_right_second</c>, an installed animation frame.</summary>
    GoldenTorizoEggBreakRightSecond,
    /// <summary><c>golden_torizo_egg_break_right_third</c>, an installed animation frame.</summary>
    GoldenTorizoEggBreakRightThird,
    /// <summary><c>golden_torizo_egg_break_right_fourth</c>, an installed animation frame.</summary>
    GoldenTorizoEggBreakRightFourth,
    /// <summary><c>golden_torizo_egg_break_right_fifth</c>, an installed animation frame.</summary>
    GoldenTorizoEggBreakRightFifth,
    /// <summary><c>golden_torizo_super_missile_right_initial</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileRightInitial,
    /// <summary><c>golden_torizo_super_missile_right_loop_first</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileRightLoopFirst,
    /// <summary><c>golden_torizo_super_missile_right_loop_second</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileRightLoopSecond,
    /// <summary><c>golden_torizo_super_missile_right_loop_third</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileRightLoopThird,
    /// <summary><c>golden_torizo_super_missile_right_loop_fourth</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileRightLoopFourth,
    /// <summary><c>golden_torizo_super_missile_right_loop_fifth</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileRightLoopFifth,
    /// <summary><c>golden_torizo_super_missile_right_loop_sixth</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileRightLoopSixth,
    /// <summary><c>golden_torizo_super_missile_right_loop_seventh</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileRightLoopSeventh,
    /// <summary><c>golden_torizo_super_missile_right_loop_eighth</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileRightLoopEighth,
    /// <summary><c>golden_torizo_super_missile_left_initial</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileLeftInitial,
    /// <summary><c>golden_torizo_super_missile_left_loop_first</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileLeftLoopFirst,
    /// <summary><c>golden_torizo_super_missile_left_loop_second</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileLeftLoopSecond,
    /// <summary><c>golden_torizo_super_missile_left_loop_third</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileLeftLoopThird,
    /// <summary><c>golden_torizo_super_missile_left_loop_fourth</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileLeftLoopFourth,
    /// <summary><c>golden_torizo_super_missile_left_loop_fifth</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileLeftLoopFifth,
    /// <summary><c>golden_torizo_super_missile_left_loop_sixth</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileLeftLoopSixth,
    /// <summary><c>golden_torizo_super_missile_left_loop_seventh</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileLeftLoopSeventh,
    /// <summary><c>golden_torizo_super_missile_left_loop_eighth</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileLeftLoopEighth,
    /// <summary><c>golden_torizo_super_missile_impact_first</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileImpactFirst,
    /// <summary><c>golden_torizo_super_missile_impact_second</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileImpactSecond,
    /// <summary><c>golden_torizo_super_missile_impact_third</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileImpactThird,
    /// <summary><c>golden_torizo_super_missile_impact_fourth</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileImpactFourth,
    /// <summary><c>golden_torizo_super_missile_impact_fifth</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileImpactFifth,
    /// <summary><c>golden_torizo_super_missile_impact_sixth</c>, an installed animation frame.</summary>
    GoldenTorizoSuperMissileImpactSixth,
    /// <summary><c>golden_torizo_eye_beam_wall_impact_first</c>, an installed animation frame.</summary>
    GoldenTorizoEyeBeamWallImpactFirst,
    /// <summary><c>golden_torizo_eye_beam_wall_impact_second</c>, an installed animation frame.</summary>
    GoldenTorizoEyeBeamWallImpactSecond,
    /// <summary><c>golden_torizo_eye_beam_wall_impact_third</c>, an installed animation frame.</summary>
    GoldenTorizoEyeBeamWallImpactThird,
    /// <summary><c>golden_torizo_eye_beam_wall_impact_fourth</c>, an installed animation frame.</summary>
    GoldenTorizoEyeBeamWallImpactFourth,
    /// <summary><c>golden_torizo_eye_beam_wall_impact_fifth</c>, an installed animation frame.</summary>
    GoldenTorizoEyeBeamWallImpactFifth,
    /// <summary><c>golden_torizo_eye_beam_floor_impact_loop_first</c>, an installed animation frame.</summary>
    GoldenTorizoEyeBeamFloorImpactLoopFirst,
    /// <summary><c>golden_torizo_eye_beam_floor_impact_loop_second</c>, an installed animation frame.</summary>
    GoldenTorizoEyeBeamFloorImpactLoopSecond,
    /// <summary><c>golden_torizo_eye_beam_floor_impact_loop_third</c>, an installed animation frame.</summary>
    GoldenTorizoEyeBeamFloorImpactLoopThird,
    /// <summary><c>golden_torizo_eye_beam_floor_impact_loop_fourth</c>, an installed animation frame.</summary>
    GoldenTorizoEyeBeamFloorImpactLoopFourth,
    /// <summary><c>golden_torizo_eye_beam_floor_impact_loop_fifth</c>, an installed animation frame.</summary>
    GoldenTorizoEyeBeamFloorImpactLoopFifth,
    /// <summary><c>golden_torizo_eye_beam_floor_impact_loop_sixth</c>, an installed animation frame.</summary>
    GoldenTorizoEyeBeamFloorImpactLoopSixth,
    /// <summary><c>golden_torizo_eye_beam_floor_impact_loop_seventh</c>, an installed animation frame.</summary>
    GoldenTorizoEyeBeamFloorImpactLoopSeventh,
    /// <summary><c>golden_torizo_eye_beam_normal_first</c>, an installed animation frame.</summary>
    GoldenTorizoEyeBeamNormalFirst,
    /// <summary><c>golden_torizo_eye_beam_normal_second</c>, an installed animation frame.</summary>
    GoldenTorizoEyeBeamNormalSecond,
    /// <summary><c>golden_torizo_eye_beam_normal_third</c>, an installed animation frame.</summary>
    GoldenTorizoEyeBeamNormalThird,
    /// <summary><c>golden_torizo_eye_beam_normal_fourth</c>, an installed animation frame.</summary>
    GoldenTorizoEyeBeamNormalFourth,
    /// <summary><c>golden_torizo_eye_beam_normal_fifth</c>, an installed animation frame.</summary>
    GoldenTorizoEyeBeamNormalFifth,
    /// <summary><c>old_tourian_escape_wall_explosion_first</c>, an installed animation frame.</summary>
    OldTourianEscapeWallExplosionFirst,
    /// <summary><c>old_tourian_escape_wall_explosion_second</c>, an installed animation frame.</summary>
    OldTourianEscapeWallExplosionSecond,
    /// <summary><c>old_tourian_escape_wall_explosion_third</c>, an installed animation frame.</summary>
    OldTourianEscapeWallExplosionThird,
    /// <summary><c>old_tourian_escape_wall_explosion_fourth</c>, an installed animation frame.</summary>
    OldTourianEscapeWallExplosionFourth,
    /// <summary><c>old_tourian_escape_wall_explosion_fifth</c>, an installed animation frame.</summary>
    OldTourianEscapeWallExplosionFifth,
    /// <summary><c>old_tourian_escape_wall_explosion_sixth</c>, an installed animation frame.</summary>
    OldTourianEscapeWallExplosionSixth,
    /// <summary><c>dragon_fireball_rising_left_first</c>, an installed animation frame.</summary>
    DragonFireballRisingLeftFirst,
    /// <summary><c>dragon_fireball_rising_left_second</c>, an installed animation frame.</summary>
    DragonFireballRisingLeftSecond,
    /// <summary><c>dragon_fireball_rising_right_first</c>, an installed animation frame.</summary>
    DragonFireballRisingRightFirst,
    /// <summary><c>dragon_fireball_rising_right_second</c>, an installed animation frame.</summary>
    DragonFireballRisingRightSecond,
    /// <summary><c>dragon_fireball_falling_left_first</c>, an installed animation frame.</summary>
    DragonFireballFallingLeftFirst,
    /// <summary><c>dragon_fireball_falling_left_second</c>, an installed animation frame.</summary>
    DragonFireballFallingLeftSecond,
    /// <summary><c>dragon_fireball_falling_right_first</c>, an installed animation frame.</summary>
    DragonFireballFallingRightFirst,
    /// <summary><c>dragon_fireball_falling_right_second</c>, an installed animation frame.</summary>
    DragonFireballFallingRightSecond,
    /// <summary><c>eye_door_initial_first</c>, an installed animation frame.</summary>
    EyeDoorInitialFirst,
    /// <summary><c>eye_door_initial_second</c>, an installed animation frame.</summary>
    EyeDoorInitialSecond,
    /// <summary><c>eye_door_initial_third</c>, an installed animation frame.</summary>
    EyeDoorInitialThird,
    /// <summary><c>eye_door_flying_loop</c>, an installed animation frame.</summary>
    EyeDoorFlyingLoop,
    /// <summary><c>eye_door_impact_first</c>, an installed animation frame.</summary>
    EyeDoorImpactFirst,
    /// <summary><c>eye_door_impact_second</c>, an installed animation frame.</summary>
    EyeDoorImpactSecond,
    /// <summary><c>eye_door_impact_third</c>, an installed animation frame.</summary>
    EyeDoorImpactThird,
    /// <summary><c>eye_door_shot_first</c>, an installed animation frame.</summary>
    EyeDoorShotFirst,
    /// <summary><c>eye_door_shot_second</c>, an installed animation frame.</summary>
    EyeDoorShotSecond,
    /// <summary><c>eye_door_shot_third</c>, an installed animation frame.</summary>
    EyeDoorShotThird,
    /// <summary><c>eye_door_shot_fourth</c>, an installed animation frame.</summary>
    EyeDoorShotFourth,
    /// <summary><c>eye_door_sweat_falling</c>, an installed animation frame.</summary>
    EyeDoorSweatFalling,
    /// <summary><c>eye_door_sweat_hit_floor_first</c>, an installed animation frame.</summary>
    EyeDoorSweatHitFloorFirst,
    /// <summary><c>eye_door_sweat_hit_floor_second</c>, an installed animation frame.</summary>
    EyeDoorSweatHitFloorSecond,
    /// <summary><c>eye_door_sweat_hit_floor_third</c>, an installed animation frame.</summary>
    EyeDoorSweatHitFloorThird,
    /// <summary><c>tourian_statue_splash_first</c>, an installed animation frame.</summary>
    TourianStatueSplashFirst,
    /// <summary><c>tourian_statue_splash_second</c>, an installed animation frame.</summary>
    TourianStatueSplashSecond,
    /// <summary><c>tourian_statue_splash_third</c>, an installed animation frame.</summary>
    TourianStatueSplashThird,
    /// <summary><c>tourian_statue_splash_fourth</c>, an installed animation frame.</summary>
    TourianStatueSplashFourth,
    /// <summary><c>tourian_statue_eye_glow_first</c>, an installed animation frame.</summary>
    TourianStatueEyeGlowFirst,
    /// <summary><c>tourian_statue_eye_glow_second</c>, an installed animation frame.</summary>
    TourianStatueEyeGlowSecond,
    /// <summary><c>tourian_statue_eye_glow_third</c>, an installed animation frame.</summary>
    TourianStatueEyeGlowThird,
    /// <summary><c>tourian_statue_eye_glow_fourth</c>, an installed animation frame.</summary>
    TourianStatueEyeGlowFourth,
    /// <summary><c>tourian_statue_eye_glow_fifth</c>, an installed animation frame.</summary>
    TourianStatueEyeGlowFifth,
    /// <summary><c>tourian_statue_eye_glow_sixth</c>, an installed animation frame.</summary>
    TourianStatueEyeGlowSixth,
    /// <summary><c>tourian_statue_eye_glow_seventh</c>, an installed animation frame.</summary>
    TourianStatueEyeGlowSeventh,
    /// <summary><c>tourian_statue_eye_glow_eighth</c>, an installed animation frame.</summary>
    TourianStatueEyeGlowEighth,
    /// <summary><c>tourian_statue_eye_glow_ninth</c>, an installed animation frame.</summary>
    TourianStatueEyeGlowNinth,
    /// <summary><c>tourian_statue_eye_glow_tenth</c>, an installed animation frame.</summary>
    TourianStatueEyeGlowTenth,
    /// <summary><c>tourian_statue_particle_first</c>, an installed animation frame.</summary>
    TourianStatueParticleFirst,
    /// <summary><c>tourian_statue_particle_second</c>, an installed animation frame.</summary>
    TourianStatueParticleSecond,
    /// <summary><c>tourian_statue_particle_third</c>, an installed animation frame.</summary>
    TourianStatueParticleThird,
    /// <summary><c>tourian_statue_particle_fourth</c>, an installed animation frame.</summary>
    TourianStatueParticleFourth,
    /// <summary><c>tourian_statue_tail_first</c>, an installed animation frame.</summary>
    TourianStatueTailFirst,
    /// <summary><c>tourian_statue_tail_second</c>, an installed animation frame.</summary>
    TourianStatueTailSecond,
    /// <summary><c>tourian_statue_tail_third</c>, an installed animation frame.</summary>
    TourianStatueTailThird,
    /// <summary><c>tourian_statue_tail_fourth</c>, an installed animation frame.</summary>
    TourianStatueTailFourth,
    /// <summary><c>tourian_statue_soul_first</c>, an installed animation frame.</summary>
    TourianStatueSoulFirst,
    /// <summary><c>tourian_statue_soul_second</c>, an installed animation frame.</summary>
    TourianStatueSoulSecond,
    /// <summary><c>tourian_statue_base_decoration</c>, an installed animation frame.</summary>
    TourianStatueBaseDecoration,
    /// <summary><c>tourian_statue_base_decoration_loop</c>, an installed animation frame.</summary>
    TourianStatueBaseDecorationLoop,
    /// <summary><c>tourian_statue_ridley</c>, an installed animation frame.</summary>
    TourianStatueRidley,
    /// <summary><c>tourian_statue_phantoon</c>, an installed animation frame.</summary>
    TourianStatuePhantoon,
    /// <summary><c>nuclear_waffle_initial_first</c>, an installed animation frame.</summary>
    NuclearWaffleInitialFirst,
    /// <summary><c>nuclear_waffle_initial_second</c>, an installed animation frame.</summary>
    NuclearWaffleInitialSecond,
    /// <summary><c>nuclear_waffle_initial_third</c>, an installed animation frame.</summary>
    NuclearWaffleInitialThird,
    /// <summary><c>nuclear_waffle_initial_fourth</c>, an installed animation frame.</summary>
    NuclearWaffleInitialFourth,
    /// <summary><c>nuclear_waffle_initial_fifth</c>, an installed animation frame.</summary>
    NuclearWaffleInitialFifth,
    /// <summary><c>nuclear_waffle_initial_sixth</c>, an installed animation frame.</summary>
    NuclearWaffleInitialSixth,
    /// <summary><c>nuclear_waffle_initial_seventh</c>, an installed animation frame.</summary>
    NuclearWaffleInitialSeventh,
    /// <summary><c>nuclear_waffle_initial_eighth</c>, an installed animation frame.</summary>
    NuclearWaffleInitialEighth,
    /// <summary><c>nuclear_waffle_initial_ninth</c>, an installed animation frame.</summary>
    NuclearWaffleInitialNinth,
    /// <summary><c>nuclear_waffle_initial_tenth</c>, an installed animation frame.</summary>
    NuclearWaffleInitialTenth,
    /// <summary><c>nuclear_waffle_initial_eleventh</c>, an installed animation frame.</summary>
    NuclearWaffleInitialEleventh,
    /// <summary><c>nuclear_waffle_initial_twelfth</c>, an installed animation frame.</summary>
    NuclearWaffleInitialTwelfth,
    /// <summary><c>polyp_rock_initial</c>, an installed animation frame.</summary>
    PolypRockInitial,
    /// <summary><c>shaktool_front_first</c>, an installed animation frame.</summary>
    ShaktoolFrontFirst,
    /// <summary><c>shaktool_front_second</c>, an installed animation frame.</summary>
    ShaktoolFrontSecond,
    /// <summary><c>shaktool_front_loop</c>, an installed animation frame.</summary>
    ShaktoolFrontLoop,
    /// <summary><c>shaktool_middle_first</c>, an installed animation frame.</summary>
    ShaktoolMiddleFirst,
    /// <summary><c>shaktool_middle_second</c>, an installed animation frame.</summary>
    ShaktoolMiddleSecond,
    /// <summary><c>shaktool_middle_loop</c>, an installed animation frame.</summary>
    ShaktoolMiddleLoop,
    /// <summary><c>shaktool_back</c>, an installed animation frame.</summary>
    ShaktoolBack,
    /// <summary><c>shaktool_back_loop</c>, an installed animation frame.</summary>
    ShaktoolBackLoop,
    /// <summary><c>mother_brain_turret_pose_first</c>, an installed animation frame.</summary>
    MotherBrainTurretPoseFirst,
    /// <summary><c>mother_brain_turret_pose_second</c>, an installed animation frame.</summary>
    MotherBrainTurretPoseSecond,
    /// <summary><c>mother_brain_turret_pose_third</c>, an installed animation frame.</summary>
    MotherBrainTurretPoseThird,
    /// <summary><c>mother_brain_turret_pose_fourth</c>, an installed animation frame.</summary>
    MotherBrainTurretPoseFourth,
    /// <summary><c>mother_brain_turret_pose_fifth</c>, an installed animation frame.</summary>
    MotherBrainTurretPoseFifth,
    /// <summary><c>mother_brain_turret_pose_sixth</c>, an installed animation frame.</summary>
    MotherBrainTurretPoseSixth,
    /// <summary><c>mother_brain_turret_pose_seventh</c>, an installed animation frame.</summary>
    MotherBrainTurretPoseSeventh,
    /// <summary><c>mother_brain_turret_pose_eighth</c>, an installed animation frame.</summary>
    MotherBrainTurretPoseEighth,
    /// <summary><c>mother_brain_turret_bullet_pose_first</c>, an installed animation frame.</summary>
    MotherBrainTurretBulletPoseFirst,
    /// <summary><c>mother_brain_turret_bullet_pose_second</c>, an installed animation frame.</summary>
    MotherBrainTurretBulletPoseSecond,
    /// <summary><c>mother_brain_turret_bullet_pose_third</c>, an installed animation frame.</summary>
    MotherBrainTurretBulletPoseThird,
    /// <summary><c>mother_brain_turret_bullet_pose_fourth</c>, an installed animation frame.</summary>
    MotherBrainTurretBulletPoseFourth,
    /// <summary><c>mother_brain_turret_bullet_pose_fifth</c>, an installed animation frame.</summary>
    MotherBrainTurretBulletPoseFifth,
    /// <summary><c>mother_brain_turret_bullet_pose_sixth</c>, an installed animation frame.</summary>
    MotherBrainTurretBulletPoseSixth,
    /// <summary><c>mother_brain_turret_bullet_pose_seventh</c>, an installed animation frame.</summary>
    MotherBrainTurretBulletPoseSeventh,
    /// <summary><c>mother_brain_turret_bullet_pose_eighth</c>, an installed animation frame.</summary>
    MotherBrainTurretBulletPoseEighth,
    /// <summary><c>mother_brain_turret_bullet_impact_first</c>, an installed animation frame.</summary>
    MotherBrainTurretBulletImpactFirst,
    /// <summary><c>mother_brain_turret_bullet_impact_second</c>, an installed animation frame.</summary>
    MotherBrainTurretBulletImpactSecond,
    /// <summary><c>mother_brain_turret_bullet_impact_third</c>, an installed animation frame.</summary>
    MotherBrainTurretBulletImpactThird,
    /// <summary><c>mother_brain_turret_bullet_impact_fourth</c>, an installed animation frame.</summary>
    MotherBrainTurretBulletImpactFourth,
    /// <summary><c>mother_brain_turret_bullet_impact_fifth</c>, an installed animation frame.</summary>
    MotherBrainTurretBulletImpactFifth,
    /// <summary><c>mother_brain_blue_ring_onion_ring_first</c>, an installed animation frame.</summary>
    MotherBrainBlueRingOnionRingFirst,
    /// <summary><c>mother_brain_blue_ring_onion_ring_second</c>, an installed animation frame.</summary>
    MotherBrainBlueRingOnionRingSecond,
    /// <summary><c>mother_brain_blue_ring_onion_ring_third</c>, an installed animation frame.</summary>
    MotherBrainBlueRingOnionRingThird,
    /// <summary><c>mother_brain_blue_ring_onion_ring_fourth</c>, an installed animation frame.</summary>
    MotherBrainBlueRingOnionRingFourth,
    /// <summary><c>mother_brain_blue_ring_onion_ring_fifth</c>, an installed animation frame.</summary>
    MotherBrainBlueRingOnionRingFifth,
    /// <summary><c>mother_brain_blue_ring_onion_ring_sixth</c>, an installed animation frame.</summary>
    MotherBrainBlueRingOnionRingSixth,
    /// <summary><c>mother_brain_blue_ring_impact_onion_ring_hit_first</c>, an installed animation frame.</summary>
    MotherBrainBlueRingImpactOnionRingHitFirst,
    /// <summary><c>mother_brain_blue_ring_impact_onion_ring_hit_second</c>, an installed animation frame.</summary>
    MotherBrainBlueRingImpactOnionRingHitSecond,
    /// <summary><c>mother_brain_blue_ring_impact_onion_ring_hit_third</c>, an installed animation frame.</summary>
    MotherBrainBlueRingImpactOnionRingHitThird,
    /// <summary><c>mother_brain_blue_ring_impact_onion_ring_hit_fourth</c>, an installed animation frame.</summary>
    MotherBrainBlueRingImpactOnionRingHitFourth,
    /// <summary><c>mother_brain_blue_ring_impact_onion_ring_hit_fifth</c>, an installed animation frame.</summary>
    MotherBrainBlueRingImpactOnionRingHitFifth,
    /// <summary><c>mother_brain_blue_ring_impact_onion_ring_hit_sixth</c>, an installed animation frame.</summary>
    MotherBrainBlueRingImpactOnionRingHitSixth,
    /// <summary><c>mother_brain_bomb_first</c>, an installed animation frame.</summary>
    MotherBrainBombFirst,
    /// <summary><c>mother_brain_bomb_second</c>, an installed animation frame.</summary>
    MotherBrainBombSecond,
    /// <summary><c>mother_brain_bomb_third</c>, an installed animation frame.</summary>
    MotherBrainBombThird,
    /// <summary><c>mother_brain_bomb_fourth</c>, an installed animation frame.</summary>
    MotherBrainBombFourth,
    /// <summary><c>mother_brain_bomb_fifth</c>, an installed animation frame.</summary>
    MotherBrainBombFifth,
    /// <summary><c>mother_brain_bomb_sixth</c>, an installed animation frame.</summary>
    MotherBrainBombSixth,
    /// <summary><c>mother_brain_bomb_seventh</c>, an installed animation frame.</summary>
    MotherBrainBombSeventh,
    /// <summary><c>mother_brain_bomb_eighth</c>, an installed animation frame.</summary>
    MotherBrainBombEighth,
    /// <summary><c>mother_brain_bomb_ninth</c>, an installed animation frame.</summary>
    MotherBrainBombNinth,
    /// <summary><c>mother_brain_hand_beam_first_stage_first_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamFirstStageFirstPhase,
    /// <summary><c>mother_brain_hand_beam_first_stage_second_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamFirstStageSecondPhase,
    /// <summary><c>mother_brain_hand_beam_first_stage_third_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamFirstStageThirdPhase,
    /// <summary><c>mother_brain_hand_beam_first_stage_fourth_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamFirstStageFourthPhase,
    /// <summary><c>mother_brain_hand_beam_first_stage_fifth_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamFirstStageFifthPhase,
    /// <summary><c>mother_brain_hand_beam_first_stage_sixth_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamFirstStageSixthPhase,
    /// <summary><c>mother_brain_hand_beam_first_stage_seventh_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamFirstStageSeventhPhase,
    /// <summary><c>mother_brain_hand_beam_second_stage_first_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamSecondStageFirstPhase,
    /// <summary><c>mother_brain_hand_beam_second_stage_second_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamSecondStageSecondPhase,
    /// <summary><c>mother_brain_hand_beam_second_stage_third_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamSecondStageThirdPhase,
    /// <summary><c>mother_brain_hand_beam_second_stage_fourth_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamSecondStageFourthPhase,
    /// <summary><c>mother_brain_hand_beam_second_stage_fifth_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamSecondStageFifthPhase,
    /// <summary><c>mother_brain_hand_beam_second_stage_sixth_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamSecondStageSixthPhase,
    /// <summary><c>mother_brain_hand_beam_second_stage_seventh_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamSecondStageSeventhPhase,
    /// <summary><c>mother_brain_hand_beam_third_stage_first_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamThirdStageFirstPhase,
    /// <summary><c>mother_brain_hand_beam_third_stage_second_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamThirdStageSecondPhase,
    /// <summary><c>mother_brain_hand_beam_third_stage_third_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamThirdStageThirdPhase,
    /// <summary><c>mother_brain_hand_beam_third_stage_fourth_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamThirdStageFourthPhase,
    /// <summary><c>mother_brain_hand_beam_third_stage_fifth_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamThirdStageFifthPhase,
    /// <summary><c>mother_brain_hand_beam_third_stage_sixth_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamThirdStageSixthPhase,
    /// <summary><c>mother_brain_hand_beam_third_stage_seventh_phase</c>, an installed animation frame.</summary>
    MotherBrainHandBeamThirdStageSeventhPhase,
    /// <summary><c>mother_brain_rainbow_beam_charging_first</c>, an installed animation frame.</summary>
    MotherBrainRainbowBeamChargingFirst,
    /// <summary><c>mother_brain_rainbow_beam_charging_second</c>, an installed animation frame.</summary>
    MotherBrainRainbowBeamChargingSecond,
    /// <summary><c>mother_brain_rainbow_beam_charging_third</c>, an installed animation frame.</summary>
    MotherBrainRainbowBeamChargingThird,
    /// <summary><c>mother_brain_rainbow_beam_charging_fourth</c>, an installed animation frame.</summary>
    MotherBrainRainbowBeamChargingFourth,
    /// <summary><c>mother_brain_rainbow_beam_charging_fifth</c>, an installed animation frame.</summary>
    MotherBrainRainbowBeamChargingFifth,
    /// <summary><c>mother_brain_rainbow_beam_charging_sixth</c>, an installed animation frame.</summary>
    MotherBrainRainbowBeamChargingSixth,
    /// <summary><c>mother_brain_drool_attached_drool_first</c>, an installed animation frame.</summary>
    MotherBrainDroolAttachedDroolFirst,
    /// <summary><c>mother_brain_drool_attached_drool_second</c>, an installed animation frame.</summary>
    MotherBrainDroolAttachedDroolSecond,
    /// <summary><c>mother_brain_drool_attached_drool_third</c>, an installed animation frame.</summary>
    MotherBrainDroolAttachedDroolThird,
    /// <summary><c>mother_brain_drool_attached_drool_fourth</c>, an installed animation frame.</summary>
    MotherBrainDroolAttachedDroolFourth,
    /// <summary><c>mother_brain_drool_attached_drool_fifth</c>, an installed animation frame.</summary>
    MotherBrainDroolAttachedDroolFifth,
    /// <summary><c>mother_brain_drool_released</c>, an installed animation frame.</summary>
    MotherBrainDroolReleased,
    /// <summary><c>mother_brain_drool_hit_floor_first</c>, an installed animation frame.</summary>
    MotherBrainDroolHitFloorFirst,
    /// <summary><c>mother_brain_drool_hit_floor_second</c>, an installed animation frame.</summary>
    MotherBrainDroolHitFloorSecond,
    /// <summary><c>mother_brain_drool_hit_floor_third</c>, an installed animation frame.</summary>
    MotherBrainDroolHitFloorThird,
    /// <summary><c>mother_brain_drool_hit_floor_fourth</c>, an installed animation frame.</summary>
    MotherBrainDroolHitFloorFourth,
    /// <summary><c>mother_brain_escape_door_particle_first</c>, an installed animation frame.</summary>
    MotherBrainEscapeDoorParticleFirst,
    /// <summary><c>mother_brain_escape_door_particle_second</c>, an installed animation frame.</summary>
    MotherBrainEscapeDoorParticleSecond,
    /// <summary><c>mother_brain_escape_door_particle_third</c>, an installed animation frame.</summary>
    MotherBrainEscapeDoorParticleThird,
    /// <summary><c>mother_brain_escape_door_particle_fourth</c>, an installed animation frame.</summary>
    MotherBrainEscapeDoorParticleFourth,
    /// <summary><c>mother_brain_escape_door_particle_fifth</c>, an installed animation frame.</summary>
    MotherBrainEscapeDoorParticleFifth,
    /// <summary><c>mother_brain_escape_door_particle_sixth</c>, an installed animation frame.</summary>
    MotherBrainEscapeDoorParticleSixth,
    /// <summary><c>mother_brain_escape_door_particle_seventh</c>, an installed animation frame.</summary>
    MotherBrainEscapeDoorParticleSeventh,
    /// <summary><c>mother_brain_escape_door_particle_eighth</c>, an installed animation frame.</summary>
    MotherBrainEscapeDoorParticleEighth,
    /// <summary><c>mother_brain_purple_breath_big_first</c>, an installed animation frame.</summary>
    MotherBrainPurpleBreathBigFirst,
    /// <summary><c>mother_brain_purple_breath_big_second</c>, an installed animation frame.</summary>
    MotherBrainPurpleBreathBigSecond,
    /// <summary><c>mother_brain_purple_breath_big_third</c>, an installed animation frame.</summary>
    MotherBrainPurpleBreathBigThird,
    /// <summary><c>mother_brain_purple_breath_big_fourth</c>, an installed animation frame.</summary>
    MotherBrainPurpleBreathBigFourth,
    /// <summary><c>mother_brain_purple_breath_big_fifth</c>, an installed animation frame.</summary>
    MotherBrainPurpleBreathBigFifth,
    /// <summary><c>mother_brain_purple_breath_big_sixth</c>, an installed animation frame.</summary>
    MotherBrainPurpleBreathBigSixth,
    /// <summary><c>mother_brain_purple_breath_big_seventh</c>, an installed animation frame.</summary>
    MotherBrainPurpleBreathBigSeventh,
    /// <summary><c>mother_brain_purple_breath_big_eighth</c>, an installed animation frame.</summary>
    MotherBrainPurpleBreathBigEighth,
    /// <summary><c>mother_brain_purple_breath_small_first</c>, an installed animation frame.</summary>
    MotherBrainPurpleBreathSmallFirst,
    /// <summary><c>mother_brain_purple_breath_small_second</c>, an installed animation frame.</summary>
    MotherBrainPurpleBreathSmallSecond,
    /// <summary><c>mother_brain_purple_breath_small_third</c>, an installed animation frame.</summary>
    MotherBrainPurpleBreathSmallThird,
    /// <summary><c>mother_brain_purple_breath_small_fourth</c>, an installed animation frame.</summary>
    MotherBrainPurpleBreathSmallFourth,
    /// <summary><c>mother_brain_purple_breath_small_fifth</c>, an installed animation frame.</summary>
    MotherBrainPurpleBreathSmallFifth,
    /// <summary><c>mother_brain_purple_breath_small_sixth</c>, an installed animation frame.</summary>
    MotherBrainPurpleBreathSmallSixth,
    /// <summary><c>mother_brain_purple_breath_small_seventh</c>, an installed animation frame.</summary>
    MotherBrainPurpleBreathSmallSeventh,
    /// <summary><c>mother_brain_purple_breath_small_eighth</c>, an installed animation frame.</summary>
    MotherBrainPurpleBreathSmallEighth,
    /// <summary><c>mother_brain_escape_subtitle</c>, an installed animation frame.</summary>
    MotherBrainEscapeSubtitle,
    /// <summary><c>mother_brain_top_tube_top_right</c>, an installed animation frame.</summary>
    MotherBrainTopTubeTopRight,
    /// <summary><c>mother_brain_top_tube_top_left</c>, an installed animation frame.</summary>
    MotherBrainTopTubeTopLeft,
    /// <summary><c>mother_brain_top_tube_top_middle_left</c>, an installed animation frame.</summary>
    MotherBrainTopTubeTopMiddleLeft,
    /// <summary><c>mother_brain_top_tube_top_middle_right</c>, an installed animation frame.</summary>
    MotherBrainTopTubeTopMiddleRight,
    /// <summary><c>mother_brain_glass_first_shard_group_first</c>, an installed animation frame.</summary>
    MotherBrainGlassFirstShardGroupFirst,
    /// <summary><c>mother_brain_glass_first_shard_group_second</c>, an installed animation frame.</summary>
    MotherBrainGlassFirstShardGroupSecond,
    /// <summary><c>mother_brain_glass_first_shard_group_third</c>, an installed animation frame.</summary>
    MotherBrainGlassFirstShardGroupThird,
    /// <summary><c>mother_brain_glass_first_shard_group_fourth</c>, an installed animation frame.</summary>
    MotherBrainGlassFirstShardGroupFourth,
    /// <summary><c>mother_brain_glass_first_shard_group_fifth</c>, an installed animation frame.</summary>
    MotherBrainGlassFirstShardGroupFifth,
    /// <summary><c>mother_brain_glass_first_shard_group_sixth</c>, an installed animation frame.</summary>
    MotherBrainGlassFirstShardGroupSixth,
    /// <summary><c>mother_brain_glass_first_shard_group_seventh</c>, an installed animation frame.</summary>
    MotherBrainGlassFirstShardGroupSeventh,
    /// <summary><c>mother_brain_glass_first_shard_group_eighth</c>, an installed animation frame.</summary>
    MotherBrainGlassFirstShardGroupEighth,
    /// <summary><c>mother_brain_glass_second_shard_group_first</c>, an installed animation frame.</summary>
    MotherBrainGlassSecondShardGroupFirst,
    /// <summary><c>mother_brain_glass_second_shard_group_second</c>, an installed animation frame.</summary>
    MotherBrainGlassSecondShardGroupSecond,
    /// <summary><c>mother_brain_glass_second_shard_group_third</c>, an installed animation frame.</summary>
    MotherBrainGlassSecondShardGroupThird,
    /// <summary><c>mother_brain_glass_second_shard_group_fourth</c>, an installed animation frame.</summary>
    MotherBrainGlassSecondShardGroupFourth,
    /// <summary><c>mother_brain_glass_second_shard_group_fifth</c>, an installed animation frame.</summary>
    MotherBrainGlassSecondShardGroupFifth,
    /// <summary><c>mother_brain_glass_second_shard_group_sixth</c>, an installed animation frame.</summary>
    MotherBrainGlassSecondShardGroupSixth,
    /// <summary><c>mother_brain_glass_second_shard_group_seventh</c>, an installed animation frame.</summary>
    MotherBrainGlassSecondShardGroupSeventh,
    /// <summary><c>mother_brain_glass_second_shard_group_eighth</c>, an installed animation frame.</summary>
    MotherBrainGlassSecondShardGroupEighth,
    /// <summary><c>mother_brain_glass_third_shard_group_first</c>, an installed animation frame.</summary>
    MotherBrainGlassThirdShardGroupFirst,
    /// <summary><c>mother_brain_glass_third_shard_group_second</c>, an installed animation frame.</summary>
    MotherBrainGlassThirdShardGroupSecond,
    /// <summary><c>mother_brain_glass_third_shard_group_third</c>, an installed animation frame.</summary>
    MotherBrainGlassThirdShardGroupThird,
    /// <summary><c>mother_brain_glass_third_shard_group_fourth</c>, an installed animation frame.</summary>
    MotherBrainGlassThirdShardGroupFourth,
    /// <summary><c>mother_brain_glass_third_shard_group_fifth</c>, an installed animation frame.</summary>
    MotherBrainGlassThirdShardGroupFifth,
    /// <summary><c>mother_brain_glass_third_shard_group_sixth</c>, an installed animation frame.</summary>
    MotherBrainGlassThirdShardGroupSixth,
    /// <summary><c>mother_brain_glass_third_shard_group_seventh</c>, an installed animation frame.</summary>
    MotherBrainGlassThirdShardGroupSeventh,
    /// <summary><c>mother_brain_glass_third_shard_group_eighth</c>, an installed animation frame.</summary>
    MotherBrainGlassThirdShardGroupEighth,
    /// <summary><c>mother_brain_glass_fourth_shard_group_first</c>, an installed animation frame.</summary>
    MotherBrainGlassFourthShardGroupFirst,
    /// <summary><c>mother_brain_glass_fourth_shard_group_second</c>, an installed animation frame.</summary>
    MotherBrainGlassFourthShardGroupSecond,
    /// <summary><c>mother_brain_glass_fourth_shard_group_third</c>, an installed animation frame.</summary>
    MotherBrainGlassFourthShardGroupThird,
    /// <summary><c>mother_brain_glass_fourth_shard_group_fourth</c>, an installed animation frame.</summary>
    MotherBrainGlassFourthShardGroupFourth,
    /// <summary><c>mother_brain_glass_fourth_shard_group_fifth</c>, an installed animation frame.</summary>
    MotherBrainGlassFourthShardGroupFifth,
    /// <summary><c>mother_brain_glass_fourth_shard_group_sixth</c>, an installed animation frame.</summary>
    MotherBrainGlassFourthShardGroupSixth,
    /// <summary><c>mother_brain_glass_fourth_shard_group_seventh</c>, an installed animation frame.</summary>
    MotherBrainGlassFourthShardGroupSeventh,
    /// <summary><c>mother_brain_glass_fourth_shard_group_eighth</c>, an installed animation frame.</summary>
    MotherBrainGlassFourthShardGroupEighth,
    /// <summary><c>mother_brain_glass_fifth_shard_group_first</c>, an installed animation frame.</summary>
    MotherBrainGlassFifthShardGroupFirst,
    /// <summary><c>mother_brain_glass_fifth_shard_group_second</c>, an installed animation frame.</summary>
    MotherBrainGlassFifthShardGroupSecond,
    /// <summary><c>mother_brain_glass_fifth_shard_group_third</c>, an installed animation frame.</summary>
    MotherBrainGlassFifthShardGroupThird,
    /// <summary><c>mother_brain_glass_fifth_shard_group_fourth</c>, an installed animation frame.</summary>
    MotherBrainGlassFifthShardGroupFourth,
    /// <summary><c>mother_brain_glass_fifth_shard_group_fifth</c>, an installed animation frame.</summary>
    MotherBrainGlassFifthShardGroupFifth,
    /// <summary><c>mother_brain_glass_fifth_shard_group_sixth</c>, an installed animation frame.</summary>
    MotherBrainGlassFifthShardGroupSixth,
    /// <summary><c>mother_brain_glass_fifth_shard_group_seventh</c>, an installed animation frame.</summary>
    MotherBrainGlassFifthShardGroupSeventh,
    /// <summary><c>mother_brain_glass_fifth_shard_group_eighth</c>, an installed animation frame.</summary>
    MotherBrainGlassFifthShardGroupEighth,
    /// <summary><c>mother_brain_glass_sixth_shard_group_first</c>, an installed animation frame.</summary>
    MotherBrainGlassSixthShardGroupFirst,
    /// <summary><c>mother_brain_glass_sixth_shard_group_second</c>, an installed animation frame.</summary>
    MotherBrainGlassSixthShardGroupSecond,
    /// <summary><c>mother_brain_glass_sixth_shard_group_third</c>, an installed animation frame.</summary>
    MotherBrainGlassSixthShardGroupThird,
    /// <summary><c>mother_brain_glass_sixth_shard_group_fourth</c>, an installed animation frame.</summary>
    MotherBrainGlassSixthShardGroupFourth,
    /// <summary><c>mother_brain_glass_sixth_shard_group_fifth</c>, an installed animation frame.</summary>
    MotherBrainGlassSixthShardGroupFifth,
    /// <summary><c>mother_brain_glass_sixth_shard_group_sixth</c>, an installed animation frame.</summary>
    MotherBrainGlassSixthShardGroupSixth,
    /// <summary><c>mother_brain_glass_sixth_shard_group_seventh</c>, an installed animation frame.</summary>
    MotherBrainGlassSixthShardGroupSeventh,
    /// <summary><c>mother_brain_glass_sixth_shard_group_eighth</c>, an installed animation frame.</summary>
    MotherBrainGlassSixthShardGroupEighth,
    /// <summary><c>mother_brain_glass_seventh_shard_group_first</c>, an installed animation frame.</summary>
    MotherBrainGlassSeventhShardGroupFirst,
    /// <summary><c>mother_brain_glass_seventh_shard_group_second</c>, an installed animation frame.</summary>
    MotherBrainGlassSeventhShardGroupSecond,
    /// <summary><c>mother_brain_glass_seventh_shard_group_third</c>, an installed animation frame.</summary>
    MotherBrainGlassSeventhShardGroupThird,
    /// <summary><c>mother_brain_glass_seventh_shard_group_fourth</c>, an installed animation frame.</summary>
    MotherBrainGlassSeventhShardGroupFourth,
    /// <summary><c>mother_brain_glass_seventh_shard_group_fifth</c>, an installed animation frame.</summary>
    MotherBrainGlassSeventhShardGroupFifth,
    /// <summary><c>mother_brain_glass_seventh_shard_group_sixth</c>, an installed animation frame.</summary>
    MotherBrainGlassSeventhShardGroupSixth,
    /// <summary><c>mother_brain_glass_seventh_shard_group_seventh</c>, an installed animation frame.</summary>
    MotherBrainGlassSeventhShardGroupSeventh,
    /// <summary><c>mother_brain_glass_seventh_shard_group_eighth</c>, an installed animation frame.</summary>
    MotherBrainGlassSeventhShardGroupEighth,
    /// <summary><c>mother_brain_glass_eighth_shard_group_first</c>, an installed animation frame.</summary>
    MotherBrainGlassEighthShardGroupFirst,
    /// <summary><c>mother_brain_glass_eighth_shard_group_second</c>, an installed animation frame.</summary>
    MotherBrainGlassEighthShardGroupSecond,
    /// <summary><c>mother_brain_glass_eighth_shard_group_third</c>, an installed animation frame.</summary>
    MotherBrainGlassEighthShardGroupThird,
    /// <summary><c>mother_brain_glass_eighth_shard_group_fourth</c>, an installed animation frame.</summary>
    MotherBrainGlassEighthShardGroupFourth,
    /// <summary><c>mother_brain_glass_eighth_shard_group_fifth</c>, an installed animation frame.</summary>
    MotherBrainGlassEighthShardGroupFifth,
    /// <summary><c>mother_brain_glass_eighth_shard_group_sixth</c>, an installed animation frame.</summary>
    MotherBrainGlassEighthShardGroupSixth,
    /// <summary><c>mother_brain_glass_eighth_shard_group_seventh</c>, an installed animation frame.</summary>
    MotherBrainGlassEighthShardGroupSeventh,
    /// <summary><c>mother_brain_glass_eighth_shard_group_eighth</c>, an installed animation frame.</summary>
    MotherBrainGlassEighthShardGroupEighth,
    /// <summary><c>mother_brain_glass_sparkle_first</c>, an installed animation frame.</summary>
    MotherBrainGlassSparkleFirst,
    /// <summary><c>mother_brain_glass_sparkle_second</c>, an installed animation frame.</summary>
    MotherBrainGlassSparkleSecond,
    /// <summary><c>mother_brain_glass_sparkle_third</c>, an installed animation frame.</summary>
    MotherBrainGlassSparkleThird,
    /// <summary><c>mother_brain_glass_sparkle_fourth</c>, an installed animation frame.</summary>
    MotherBrainGlassSparkleFourth,
    /// <summary><c>ki_hunter_acid_spit_left_first</c>, an installed animation frame.</summary>
    KiHunterAcidSpitLeftFirst,
    /// <summary><c>ki_hunter_acid_spit_left_second</c>, an installed animation frame.</summary>
    KiHunterAcidSpitLeftSecond,
    /// <summary><c>ki_hunter_acid_spit_left_third</c>, an installed animation frame.</summary>
    KiHunterAcidSpitLeftThird,
    /// <summary><c>ki_hunter_acid_spit_left_fourth</c>, an installed animation frame.</summary>
    KiHunterAcidSpitLeftFourth,
    /// <summary><c>ki_hunter_acid_spit_left_fifth</c>, an installed animation frame.</summary>
    KiHunterAcidSpitLeftFifth,
    /// <summary><c>ki_hunter_acid_spit_left_sixth</c>, an installed animation frame.</summary>
    KiHunterAcidSpitLeftSixth,
    /// <summary><c>ki_hunter_acid_spit_left_seventh</c>, an installed animation frame.</summary>
    KiHunterAcidSpitLeftSeventh,
    /// <summary><c>ki_hunter_acid_spit_hit_floor_first</c>, an installed animation frame.</summary>
    KiHunterAcidSpitHitFloorFirst,
    /// <summary><c>ki_hunter_acid_spit_hit_floor_second</c>, an installed animation frame.</summary>
    KiHunterAcidSpitHitFloorSecond,
    /// <summary><c>ki_hunter_acid_spit_hit_floor_third</c>, an installed animation frame.</summary>
    KiHunterAcidSpitHitFloorThird,
    /// <summary><c>ki_hunter_acid_spit_hit_floor_fourth</c>, an installed animation frame.</summary>
    KiHunterAcidSpitHitFloorFourth,
    /// <summary><c>ki_hunter_acid_spit_hit_floor_fifth</c>, an installed animation frame.</summary>
    KiHunterAcidSpitHitFloorFifth,
    /// <summary><c>ki_hunter_acid_spit_right_first</c>, an installed animation frame.</summary>
    KiHunterAcidSpitRightFirst,
    /// <summary><c>ki_hunter_acid_spit_right_second</c>, an installed animation frame.</summary>
    KiHunterAcidSpitRightSecond,
    /// <summary><c>ki_hunter_acid_spit_right_third</c>, an installed animation frame.</summary>
    KiHunterAcidSpitRightThird,
    /// <summary><c>ki_hunter_acid_spit_right_fourth</c>, an installed animation frame.</summary>
    KiHunterAcidSpitRightFourth,
    /// <summary><c>ki_hunter_acid_spit_right_fifth</c>, an installed animation frame.</summary>
    KiHunterAcidSpitRightFifth,
    /// <summary><c>ki_hunter_acid_spit_right_sixth</c>, an installed animation frame.</summary>
    KiHunterAcidSpitRightSixth,
    /// <summary><c>ki_hunter_acid_spit_right_seventh</c>, an installed animation frame.</summary>
    KiHunterAcidSpitRightSeventh,
    /// <summary><c>kago_bug_landed_first</c>, an installed animation frame.</summary>
    KagoBugLandedFirst,
    /// <summary><c>kago_bug_landed_second</c>, an installed animation frame.</summary>
    KagoBugLandedSecond,
    /// <summary><c>kago_bug_falling</c>, an installed animation frame.</summary>
    KagoBugFalling,
    /// <summary><c>kago_bug_jump_start_first</c>, an installed animation frame.</summary>
    KagoBugJumpStartFirst,
    /// <summary><c>kago_bug_jump_start_second</c>, an installed animation frame.</summary>
    KagoBugJumpStartSecond,
    /// <summary><c>kago_bug_jump_loop</c>, an installed animation frame.</summary>
    KagoBugJumpLoop,
    /// <summary><c>kago_bug_shot_first</c>, an installed animation frame.</summary>
    KagoBugShotFirst,
    /// <summary><c>kago_bug_shot_second</c>, an installed animation frame.</summary>
    KagoBugShotSecond,
    /// <summary><c>kago_bug_shot_third</c>, an installed animation frame.</summary>
    KagoBugShotThird,
    /// <summary><c>kago_bug_shot_fourth</c>, an installed animation frame.</summary>
    KagoBugShotFourth,
    /// <summary><c>kago_bug_shot_fifth</c>, an installed animation frame.</summary>
    KagoBugShotFifth,
    /// <summary><c>powamp_spike_initial_first</c>, an installed animation frame.</summary>
    PowampSpikeInitialFirst,
    /// <summary><c>powamp_spike_initial_second</c>, an installed animation frame.</summary>
    PowampSpikeInitialSecond,
    /// <summary><c>powamp_spike_initial_third</c>, an installed animation frame.</summary>
    PowampSpikeInitialThird,
    /// <summary><c>work_robot_laser_initial_first</c>, an installed animation frame.</summary>
    WorkRobotLaserInitialFirst,
    /// <summary><c>work_robot_laser_initial_second</c>, an installed animation frame.</summary>
    WorkRobotLaserInitialSecond,
    /// <summary><c>work_robot_laser_initial_third</c>, an installed animation frame.</summary>
    WorkRobotLaserInitialThird,
    /// <summary><c>work_robot_laser_loop_first</c>, an installed animation frame.</summary>
    WorkRobotLaserLoopFirst,
    /// <summary><c>work_robot_laser_loop_second</c>, an installed animation frame.</summary>
    WorkRobotLaserLoopSecond,
    /// <summary><c>work_robot_laser_loop_third</c>, an installed animation frame.</summary>
    WorkRobotLaserLoopThird,
    /// <summary><c>work_robot_laser_loop_fourth</c>, an installed animation frame.</summary>
    WorkRobotLaserLoopFourth,
    /// <summary><c>noob_tube_first</c>, an installed animation frame.</summary>
    NoobTubeFirst,
    /// <summary><c>noob_tube_second</c>, an installed animation frame.</summary>
    NoobTubeSecond,
    /// <summary><c>noob_tube_third</c>, an installed animation frame.</summary>
    NoobTubeThird,
    /// <summary><c>noob_tube_fourth</c>, an installed animation frame.</summary>
    NoobTubeFourth,
    /// <summary><c>noob_tube_fifth</c>, an installed animation frame.</summary>
    NoobTubeFifth,
    /// <summary><c>noob_tube_sixth</c>, an installed animation frame.</summary>
    NoobTubeSixth,
    /// <summary><c>noob_tube_seventh</c>, an installed animation frame.</summary>
    NoobTubeSeventh,
    /// <summary><c>noob_tube_eighth</c>, an installed animation frame.</summary>
    NoobTubeEighth,
    /// <summary><c>noob_tube_ninth</c>, an installed animation frame.</summary>
    NoobTubeNinth,
    /// <summary><c>noob_tube_tenth</c>, an installed animation frame.</summary>
    NoobTubeTenth,
    /// <summary><c>noob_tube_eleventh</c>, an installed animation frame.</summary>
    NoobTubeEleventh,
    /// <summary><c>noob_tube_twelfth</c>, an installed animation frame.</summary>
    NoobTubeTwelfth,
    /// <summary><c>noob_tube_thirteenth</c>, an installed animation frame.</summary>
    NoobTubeThirteenth,
    /// <summary><c>noob_tube_fourteenth</c>, an installed animation frame.</summary>
    NoobTubeFourteenth,
    /// <summary><c>noob_tube_fifteenth</c>, an installed animation frame.</summary>
    NoobTubeFifteenth,
    /// <summary><c>noob_tube_sixteenth</c>, an installed animation frame.</summary>
    NoobTubeSixteenth,
    /// <summary><c>noob_tube_seventeenth</c>, an installed animation frame.</summary>
    NoobTubeSeventeenth,
    /// <summary><c>noob_tube_eighteenth</c>, an installed animation frame.</summary>
    NoobTubeEighteenth,
    /// <summary><c>noob_tube_nineteenth</c>, an installed animation frame.</summary>
    NoobTubeNineteenth,
    /// <summary><c>noob_tube_twentieth</c>, an installed animation frame.</summary>
    NoobTubeTwentieth,
    /// <summary><c>noob_tube_twenty_first</c>, an installed animation frame.</summary>
    NoobTubeTwentyFirst,
    /// <summary><c>noob_tube_twenty_second</c>, an installed animation frame.</summary>
    NoobTubeTwentySecond,
    /// <summary><c>noob_tube_twenty_third</c>, an installed animation frame.</summary>
    NoobTubeTwentyThird,
    /// <summary><c>noob_tube_twenty_fourth</c>, an installed animation frame.</summary>
    NoobTubeTwentyFourth,
    /// <summary><c>noob_tube_twenty_fifth</c>, an installed animation frame.</summary>
    NoobTubeTwentyFifth,
    /// <summary><c>noob_tube_twenty_sixth</c>, an installed animation frame.</summary>
    NoobTubeTwentySixth,
    /// <summary><c>noob_tube_twenty_seventh</c>, an installed animation frame.</summary>
    NoobTubeTwentySeventh,
    /// <summary><c>noob_tube_twenty_eighth</c>, an installed animation frame.</summary>
    NoobTubeTwentyEighth,
    /// <summary><c>noob_tube_twenty_ninth</c>, an installed animation frame.</summary>
    NoobTubeTwentyNinth,
    /// <summary><c>noob_tube_thirtieth</c>, an installed animation frame.</summary>
    NoobTubeThirtieth,
    /// <summary><c>noob_tube_thirty_first</c>, an installed animation frame.</summary>
    NoobTubeThirtyFirst,
    /// <summary><c>noob_tube_thirty_second</c>, an installed animation frame.</summary>
    NoobTubeThirtySecond,
    /// <summary><c>noob_tube_thirty_third</c>, an installed animation frame.</summary>
    NoobTubeThirtyThird,
    /// <summary><c>noob_tube_thirty_fourth</c>, an installed animation frame.</summary>
    NoobTubeThirtyFourth,
    /// <summary><c>noob_tube_thirty_fifth</c>, an installed animation frame.</summary>
    NoobTubeThirtyFifth,
    /// <summary><c>noob_tube_thirty_sixth</c>, an installed animation frame.</summary>
    NoobTubeThirtySixth,
    /// <summary><c>noob_tube_thirty_seventh</c>, an installed animation frame.</summary>
    NoobTubeThirtySeventh,
    /// <summary><c>noob_tube_thirty_eighth</c>, an installed animation frame.</summary>
    NoobTubeThirtyEighth,
    /// <summary><c>noob_tube_thirty_ninth</c>, an installed animation frame.</summary>
    NoobTubeThirtyNinth,
    /// <summary><c>noob_tube_fortieth</c>, an installed animation frame.</summary>
    NoobTubeFortieth,
    /// <summary><c>noob_tube_forty_first</c>, an installed animation frame.</summary>
    NoobTubeFortyFirst,
    /// <summary><c>noob_tube_forty_second</c>, an installed animation frame.</summary>
    NoobTubeFortySecond,
    /// <summary><c>noob_tube_forty_third</c>, an installed animation frame.</summary>
    NoobTubeFortyThird,
    /// <summary><c>noob_tube_forty_fourth</c>, an installed animation frame.</summary>
    NoobTubeFortyFourth,
    /// <summary><c>noob_tube_forty_fifth</c>, an installed animation frame.</summary>
    NoobTubeFortyFifth,
    /// <summary><c>noob_tube_forty_sixth</c>, an installed animation frame.</summary>
    NoobTubeFortySixth,
    /// <summary><c>noob_tube_forty_seventh</c>, an installed animation frame.</summary>
    NoobTubeFortySeventh,
    /// <summary><c>noob_tube_forty_eighth</c>, an installed animation frame.</summary>
    NoobTubeFortyEighth,
    /// <summary><c>noob_tube_forty_ninth</c>, an installed animation frame.</summary>
    NoobTubeFortyNinth,
    /// <summary><c>noob_tube_fiftieth</c>, an installed animation frame.</summary>
    NoobTubeFiftieth,
    /// <summary><c>noob_tube_fifty_first</c>, an installed animation frame.</summary>
    NoobTubeFiftyFirst,
    /// <summary><c>noob_tube_fifty_second</c>, an installed animation frame.</summary>
    NoobTubeFiftySecond,
    /// <summary><c>noob_tube_fifty_third</c>, an installed animation frame.</summary>
    NoobTubeFiftyThird,
    /// <summary><c>noob_tube_fifty_fourth</c>, an installed animation frame.</summary>
    NoobTubeFiftyFourth,
    /// <summary><c>noob_tube_fifty_fifth</c>, an installed animation frame.</summary>
    NoobTubeFiftyFifth,
    /// <summary><c>noob_tube_fifty_sixth</c>, an installed animation frame.</summary>
    NoobTubeFiftySixth,
    /// <summary><c>noob_tube_fifty_seventh</c>, an installed animation frame.</summary>
    NoobTubeFiftySeventh,
    /// <summary><c>noob_tube_fifty_eighth</c>, an installed animation frame.</summary>
    NoobTubeFiftyEighth,
    /// <summary><c>noob_tube_fifty_ninth</c>, an installed animation frame.</summary>
    NoobTubeFiftyNinth,
    /// <summary><c>noob_tube_sixtieth</c>, an installed animation frame.</summary>
    NoobTubeSixtieth,
    /// <summary><c>noob_tube_sixty_first</c>, an installed animation frame.</summary>
    NoobTubeSixtyFirst,
    /// <summary><c>noob_tube_sixty_second</c>, an installed animation frame.</summary>
    NoobTubeSixtySecond,
    /// <summary><c>noob_tube_sixty_third</c>, an installed animation frame.</summary>
    NoobTubeSixtyThird,
    /// <summary><c>noob_tube_sixty_fourth</c>, an installed animation frame.</summary>
    NoobTubeSixtyFourth,
    /// <summary><c>noob_tube_sixty_fifth</c>, an installed animation frame.</summary>
    NoobTubeSixtyFifth,
    /// <summary><c>noob_tube_sixty_sixth</c>, an installed animation frame.</summary>
    NoobTubeSixtySixth,
    /// <summary><c>noob_tube_sixty_seventh</c>, an installed animation frame.</summary>
    NoobTubeSixtySeventh,
    /// <summary><c>noob_tube_sixty_eighth</c>, an installed animation frame.</summary>
    NoobTubeSixtyEighth,
    /// <summary><c>noob_tube_sixty_ninth</c>, an installed animation frame.</summary>
    NoobTubeSixtyNinth,
    /// <summary><c>noob_tube_seventieth</c>, an installed animation frame.</summary>
    NoobTubeSeventieth,
    /// <summary><c>noob_tube_seventy_first</c>, an installed animation frame.</summary>
    NoobTubeSeventyFirst,
    /// <summary><c>noob_tube_seventy_second</c>, an installed animation frame.</summary>
    NoobTubeSeventySecond,
    /// <summary><c>noob_tube_seventy_third</c>, an installed animation frame.</summary>
    NoobTubeSeventyThird,
    /// <summary><c>noob_tube_seventy_fourth</c>, an installed animation frame.</summary>
    NoobTubeSeventyFourth,
    /// <summary><c>noob_tube_seventy_fifth</c>, an installed animation frame.</summary>
    NoobTubeSeventyFifth,
    /// <summary><c>noob_tube_seventy_sixth</c>, an installed animation frame.</summary>
    NoobTubeSeventySixth,
    /// <summary><c>noob_tube_seventy_seventh</c>, an installed animation frame.</summary>
    NoobTubeSeventySeventh,
    /// <summary><c>noob_tube_seventy_eighth</c>, an installed animation frame.</summary>
    NoobTubeSeventyEighth,
    /// <summary><c>noob_tube_seventy_ninth</c>, an installed animation frame.</summary>
    NoobTubeSeventyNinth,
    /// <summary><c>noob_tube_eightieth</c>, an installed animation frame.</summary>
    NoobTubeEightieth,
    /// <summary><c>noob_tube_eighty_first</c>, an installed animation frame.</summary>
    NoobTubeEightyFirst,
    /// <summary><c>noob_tube_eighty_second</c>, an installed animation frame.</summary>
    NoobTubeEightySecond,
    /// <summary><c>noob_tube_eighty_third</c>, an installed animation frame.</summary>
    NoobTubeEightyThird,
    /// <summary><c>noob_tube_eighty_fourth</c>, an installed animation frame.</summary>
    NoobTubeEightyFourth,
    /// <summary><c>noob_tube_eighty_fifth</c>, an installed animation frame.</summary>
    NoobTubeEightyFifth,
    /// <summary><c>noob_tube_eighty_sixth</c>, an installed animation frame.</summary>
    NoobTubeEightySixth,
    /// <summary><c>noob_tube_eighty_seventh</c>, an installed animation frame.</summary>
    NoobTubeEightySeventh,
    /// <summary><c>noob_tube_eighty_eighth</c>, an installed animation frame.</summary>
    NoobTubeEightyEighth,
    /// <summary><c>noob_tube_eighty_ninth</c>, an installed animation frame.</summary>
    NoobTubeEightyNinth,
    /// <summary><c>noob_tube_ninetieth</c>, an installed animation frame.</summary>
    NoobTubeNinetieth,
    /// <summary><c>cacatac_spike_left_facing_up</c>, an installed animation frame.</summary>
    CacatacSpikeLeftFacingUp,
    /// <summary><c>cacatac_spike_up_left</c>, an installed animation frame.</summary>
    CacatacSpikeUpLeft,
    /// <summary><c>cacatac_spike_up</c>, an installed animation frame.</summary>
    CacatacSpikeUp,
    /// <summary><c>cacatac_spike_up_right</c>, an installed animation frame.</summary>
    CacatacSpikeUpRight,
    /// <summary><c>cacatac_spike_right_facing_up</c>, an installed animation frame.</summary>
    CacatacSpikeRightFacingUp,
    /// <summary><c>cacatac_spike_left_facing_down</c>, an installed animation frame.</summary>
    CacatacSpikeLeftFacingDown,
    /// <summary><c>cacatac_spike_down_left</c>, an installed animation frame.</summary>
    CacatacSpikeDownLeft,
    /// <summary><c>cacatac_spike_down</c>, an installed animation frame.</summary>
    CacatacSpikeDown,
    /// <summary><c>cacatac_spike_down_right</c>, an installed animation frame.</summary>
    CacatacSpikeDownRight,
    /// <summary><c>cacatac_spike_right_facing_down</c>, an installed animation frame.</summary>
    CacatacSpikeRightFacingDown,
    /// <summary><c>spore_spawn_spawner_closed</c>, an installed animation frame.</summary>
    SporeSpawnSpawnerClosed,
    /// <summary><c>spore_spawn_spawner_release_first</c>, an installed animation frame.</summary>
    SporeSpawnSpawnerReleaseFirst,
    /// <summary><c>spore_spawn_spawner_release_second</c>, an installed animation frame.</summary>
    SporeSpawnSpawnerReleaseSecond,
    /// <summary><c>spore_spawn_spawner_release_third</c>, an installed animation frame.</summary>
    SporeSpawnSpawnerReleaseThird,
    /// <summary><c>spore_spawn_spawner_release_fourth</c>, an installed animation frame.</summary>
    SporeSpawnSpawnerReleaseFourth,
    /// <summary><c>spore_spawn_spawner_release_fifth</c>, an installed animation frame.</summary>
    SporeSpawnSpawnerReleaseFifth,
    /// <summary><c>spore_spawn_spore_first</c>, an installed animation frame.</summary>
    SporeSpawnSporeFirst,
    /// <summary><c>spore_spawn_spore_second</c>, an installed animation frame.</summary>
    SporeSpawnSporeSecond,
    /// <summary><c>spore_spawn_spore_third</c>, an installed animation frame.</summary>
    SporeSpawnSporeThird,
    /// <summary><c>spore_spawn_stalk</c>, an installed animation frame.</summary>
    SporeSpawnStalk,
    /// <summary><c>spore_spawn_spore_shot_first</c>, an installed animation frame.</summary>
    SporeSpawnSporeShotFirst,
    /// <summary><c>spore_spawn_spore_shot_second</c>, an installed animation frame.</summary>
    SporeSpawnSporeShotSecond,
    /// <summary><c>spore_spawn_spore_shot_third</c>, an installed animation frame.</summary>
    SporeSpawnSporeShotThird,
    /// <summary><c>spore_spawn_spore_shot_fourth</c>, an installed animation frame.</summary>
    SporeSpawnSporeShotFourth,
    /// <summary><c>spore_spawn_spore_shot_fifth</c>, an installed animation frame.</summary>
    SporeSpawnSporeShotFifth,
    /// <summary><c>spore_spawn_spore_shot_sixth</c>, an installed animation frame.</summary>
    SporeSpawnSporeShotSixth,
    /// <summary><c>spore_spawn_spore_shot_seventh</c>, an installed animation frame.</summary>
    SporeSpawnSporeShotSeventh,
    /// <summary><c>fune_namihe_fireball_left_first</c>, an installed animation frame.</summary>
    FuneNamiheFireballLeftFirst,
    /// <summary><c>fune_namihe_fireball_left_second</c>, an installed animation frame.</summary>
    FuneNamiheFireballLeftSecond,
    /// <summary><c>fune_namihe_fireball_left_third</c>, an installed animation frame.</summary>
    FuneNamiheFireballLeftThird,
    /// <summary><c>fune_namihe_fireball_right_first</c>, an installed animation frame.</summary>
    FuneNamiheFireballRightFirst,
    /// <summary><c>fune_namihe_fireball_right_second</c>, an installed animation frame.</summary>
    FuneNamiheFireballRightSecond,
    /// <summary><c>fune_namihe_fireball_right_third</c>, an installed animation frame.</summary>
    FuneNamiheFireballRightThird,
    /// <summary><c>magdollite_lava_left</c>, an installed animation frame.</summary>
    MagdolliteLavaLeft,
    /// <summary><c>magdollite_lava_right</c>, an installed animation frame.</summary>
    MagdolliteLavaRight,
    /// <summary><c>misc_dust_elbow_charge_particle_first</c>, an installed animation frame.</summary>
    MiscDustElbowChargeParticleFirst,
    /// <summary><c>misc_dust_elbow_charge_particle_second</c>, an installed animation frame.</summary>
    MiscDustElbowChargeParticleSecond,
    /// <summary><c>misc_dust_elbow_charge_particle_third</c>, an installed animation frame.</summary>
    MiscDustElbowChargeParticleThird,
    /// <summary><c>misc_dust_elbow_charge_particle_fourth</c>, an installed animation frame.</summary>
    MiscDustElbowChargeParticleFourth,
    /// <summary><c>misc_dust_elbow_charge_particle_fifth</c>, an installed animation frame.</summary>
    MiscDustElbowChargeParticleFifth,
    /// <summary><c>misc_dust_elbow_charge_particle_sixth</c>, an installed animation frame.</summary>
    MiscDustElbowChargeParticleSixth,
    /// <summary><c>misc_dust_elbow_charge_energy_first</c>, an installed animation frame.</summary>
    MiscDustElbowChargeEnergyFirst,
    /// <summary><c>misc_dust_elbow_charge_energy_second</c>, an installed animation frame.</summary>
    MiscDustElbowChargeEnergySecond,
    /// <summary><c>misc_dust_elbow_charge_energy_third</c>, an installed animation frame.</summary>
    MiscDustElbowChargeEnergyThird,
    /// <summary><c>misc_dust_elbow_charge_energy_fourth</c>, an installed animation frame.</summary>
    MiscDustElbowChargeEnergyFourth,
    /// <summary><c>misc_dust_elbow_charge_energy_fifth</c>, an installed animation frame.</summary>
    MiscDustElbowChargeEnergyFifth,
    /// <summary><c>misc_dust_elbow_charge_energy_sixth</c>, an installed animation frame.</summary>
    MiscDustElbowChargeEnergySixth,
    /// <summary><c>misc_dust_elbow_charge_energy_seventh</c>, an installed animation frame.</summary>
    MiscDustElbowChargeEnergySeventh,
    /// <summary><c>misc_dust_small_explosion_first</c>, an installed animation frame.</summary>
    MiscDustSmallExplosionFirst,
    /// <summary><c>misc_dust_small_explosion_second</c>, an installed animation frame.</summary>
    MiscDustSmallExplosionSecond,
    /// <summary><c>misc_dust_small_explosion_third</c>, an installed animation frame.</summary>
    MiscDustSmallExplosionThird,
    /// <summary><c>misc_dust_small_explosion_fourth</c>, an installed animation frame.</summary>
    MiscDustSmallExplosionFourth,
    /// <summary><c>misc_dust_small_explosion_fifth</c>, an installed animation frame.</summary>
    MiscDustSmallExplosionFifth,
    /// <summary><c>misc_dust_small_explosion_sixth</c>, an installed animation frame.</summary>
    MiscDustSmallExplosionSixth,
    /// <summary><c>misc_dust_rainbow_beam_explosion_first</c>, an installed animation frame.</summary>
    MiscDustRainbowBeamExplosionFirst,
    /// <summary><c>misc_dust_rainbow_beam_explosion_second</c>, an installed animation frame.</summary>
    MiscDustRainbowBeamExplosionSecond,
    /// <summary><c>misc_dust_rainbow_beam_explosion_third</c>, an installed animation frame.</summary>
    MiscDustRainbowBeamExplosionThird,
    /// <summary><c>misc_dust_rainbow_beam_explosion_fourth</c>, an installed animation frame.</summary>
    MiscDustRainbowBeamExplosionFourth,
    /// <summary><c>misc_dust_rainbow_beam_explosion_fifth</c>, an installed animation frame.</summary>
    MiscDustRainbowBeamExplosionFifth,
    /// <summary><c>misc_dust_dud_shot_first</c>, an installed animation frame.</summary>
    MiscDustDudShotFirst,
    /// <summary><c>misc_dust_dud_shot_second</c>, an installed animation frame.</summary>
    MiscDustDudShotSecond,
    /// <summary><c>misc_dust_dud_shot_third</c>, an installed animation frame.</summary>
    MiscDustDudShotThird,
    /// <summary><c>misc_dust_dud_shot_fourth</c>, an installed animation frame.</summary>
    MiscDustDudShotFourth,
    /// <summary><c>misc_dust_dud_shot_fifth</c>, an installed animation frame.</summary>
    MiscDustDudShotFifth,
    /// <summary><c>misc_dust_dud_shot_sixth</c>, an installed animation frame.</summary>
    MiscDustDudShotSixth,
    /// <summary><c>misc_dust_small_dust_cloud_first</c>, an installed animation frame.</summary>
    MiscDustSmallDustCloudFirst,
    /// <summary><c>misc_dust_small_dust_cloud_second</c>, an installed animation frame.</summary>
    MiscDustSmallDustCloudSecond,
    /// <summary><c>misc_dust_small_dust_cloud_third</c>, an installed animation frame.</summary>
    MiscDustSmallDustCloudThird,
    /// <summary><c>misc_dust_small_dust_cloud_fourth</c>, an installed animation frame.</summary>
    MiscDustSmallDustCloudFourth,
    /// <summary><c>misc_dust_small_dust_cloud_fifth</c>, an installed animation frame.</summary>
    MiscDustSmallDustCloudFifth,
    /// <summary><c>misc_dust_corpse_dust_cloud_first</c>, an installed animation frame.</summary>
    MiscDustCorpseDustCloudFirst,
    /// <summary><c>misc_dust_corpse_dust_cloud_second</c>, an installed animation frame.</summary>
    MiscDustCorpseDustCloudSecond,
    /// <summary><c>misc_dust_corpse_dust_cloud_third</c>, an installed animation frame.</summary>
    MiscDustCorpseDustCloudThird,
    /// <summary><c>misc_dust_corpse_dust_cloud_fourth</c>, an installed animation frame.</summary>
    MiscDustCorpseDustCloudFourth,
    /// <summary><c>misc_dust_eye_door_sweat_drop_first</c>, an installed animation frame.</summary>
    MiscDustEyeDoorSweatDropFirst,
    /// <summary><c>misc_dust_eye_door_sweat_drop_second</c>, an installed animation frame.</summary>
    MiscDustEyeDoorSweatDropSecond,
    /// <summary><c>misc_dust_eye_door_sweat_drop_third</c>, an installed animation frame.</summary>
    MiscDustEyeDoorSweatDropThird,
    /// <summary><c>misc_dust_eye_door_sweat_drop_fourth</c>, an installed animation frame.</summary>
    MiscDustEyeDoorSweatDropFourth,
    /// <summary><c>misc_dust_smoke_first</c>, an installed animation frame.</summary>
    MiscDustSmokeFirst,
    /// <summary><c>misc_dust_smoke_second</c>, an installed animation frame.</summary>
    MiscDustSmokeSecond,
    /// <summary><c>misc_dust_smoke_third</c>, an installed animation frame.</summary>
    MiscDustSmokeThird,
    /// <summary><c>misc_dust_smoke_fourth</c>, an installed animation frame.</summary>
    MiscDustSmokeFourth,
    /// <summary><c>misc_dust_big_explosion_first</c>, an installed animation frame.</summary>
    MiscDustBigExplosionFirst,
    /// <summary><c>misc_dust_big_explosion_second</c>, an installed animation frame.</summary>
    MiscDustBigExplosionSecond,
    /// <summary><c>misc_dust_big_explosion_third</c>, an installed animation frame.</summary>
    MiscDustBigExplosionThird,
    /// <summary><c>misc_dust_big_explosion_fourth</c>, an installed animation frame.</summary>
    MiscDustBigExplosionFourth,
    /// <summary><c>misc_dust_big_explosion_fifth</c>, an installed animation frame.</summary>
    MiscDustBigExplosionFifth,
    /// <summary><c>misc_dust_big_explosion_sixth</c>, an installed animation frame.</summary>
    MiscDustBigExplosionSixth,
    /// <summary><c>misc_dust_rock_particles_first</c>, an installed animation frame.</summary>
    MiscDustRockParticlesFirst,
    /// <summary><c>misc_dust_rock_particles_second</c>, an installed animation frame.</summary>
    MiscDustRockParticlesSecond,
    /// <summary><c>misc_dust_rock_particles_third</c>, an installed animation frame.</summary>
    MiscDustRockParticlesThird,
    /// <summary><c>misc_dust_rock_particles_fourth</c>, an installed animation frame.</summary>
    MiscDustRockParticlesFourth,
    /// <summary><c>misc_dust_rock_particles_fifth</c>, an installed animation frame.</summary>
    MiscDustRockParticlesFifth,
    /// <summary><c>misc_dust_rock_particles_sixth</c>, an installed animation frame.</summary>
    MiscDustRockParticlesSixth,
    /// <summary><c>misc_dust_rock_particles_seventh</c>, an installed animation frame.</summary>
    MiscDustRockParticlesSeventh,
    /// <summary><c>misc_dust_rock_particles_eighth</c>, an installed animation frame.</summary>
    MiscDustRockParticlesEighth,
    /// <summary><c>misc_dust_rock_particles_ninth</c>, an installed animation frame.</summary>
    MiscDustRockParticlesNinth,
    /// <summary><c>misc_dust_rock_particles_tenth</c>, an installed animation frame.</summary>
    MiscDustRockParticlesTenth,
    /// <summary><c>misc_dust_rock_particles_eleventh</c>, an installed animation frame.</summary>
    MiscDustRockParticlesEleventh,
    /// <summary><c>misc_dust_rock_particles_twelfth</c>, an installed animation frame.</summary>
    MiscDustRockParticlesTwelfth,
    /// <summary><c>misc_dust_rock_particles_thirteenth</c>, an installed animation frame.</summary>
    MiscDustRockParticlesThirteenth,
    /// <summary><c>misc_dust_rock_particles_fourteenth</c>, an installed animation frame.</summary>
    MiscDustRockParticlesFourteenth,
    /// <summary><c>misc_dust_rock_particles_fifteenth</c>, an installed animation frame.</summary>
    MiscDustRockParticlesFifteenth,
    /// <summary><c>misc_dust_rock_particles_sixteenth</c>, an installed animation frame.</summary>
    MiscDustRockParticlesSixteenth,
    /// <summary><c>misc_dust_short_big_dust_cloud_first</c>, an installed animation frame.</summary>
    MiscDustShortBigDustCloudFirst,
    /// <summary><c>misc_dust_short_big_dust_cloud_second</c>, an installed animation frame.</summary>
    MiscDustShortBigDustCloudSecond,
    /// <summary><c>misc_dust_short_big_dust_cloud_third</c>, an installed animation frame.</summary>
    MiscDustShortBigDustCloudThird,
    /// <summary><c>misc_dust_short_big_dust_cloud_fourth</c>, an installed animation frame.</summary>
    MiscDustShortBigDustCloudFourth,
    /// <summary><c>misc_dust_big_dust_cloud_first</c>, an installed animation frame.</summary>
    MiscDustBigDustCloudFirst,
    /// <summary><c>misc_dust_big_dust_cloud_second</c>, an installed animation frame.</summary>
    MiscDustBigDustCloudSecond,
    /// <summary><c>misc_dust_big_dust_cloud_third</c>, an installed animation frame.</summary>
    MiscDustBigDustCloudThird,
    /// <summary><c>misc_dust_big_dust_cloud_fourth</c>, an installed animation frame.</summary>
    MiscDustBigDustCloudFourth,
    /// <summary><c>misc_dust_big_dust_cloud_fifth</c>, an installed animation frame.</summary>
    MiscDustBigDustCloudFifth,
    /// <summary><c>misc_dust_big_dust_cloud_sixth</c>, an installed animation frame.</summary>
    MiscDustBigDustCloudSixth,
    /// <summary><c>misc_dust_big_dust_cloud_seventh</c>, an installed animation frame.</summary>
    MiscDustBigDustCloudSeventh,
    /// <summary><c>misc_dust_big_dust_cloud_eighth</c>, an installed animation frame.</summary>
    MiscDustBigDustCloudEighth,
    /// <summary><c>misc_dust_draygon_breath_bubbles_first</c>, an installed animation frame.</summary>
    MiscDustDraygonBreathBubblesFirst,
    /// <summary><c>misc_dust_draygon_breath_bubbles_second</c>, an installed animation frame.</summary>
    MiscDustDraygonBreathBubblesSecond,
    /// <summary><c>misc_dust_draygon_breath_bubbles_third</c>, an installed animation frame.</summary>
    MiscDustDraygonBreathBubblesThird,
    /// <summary><c>misc_dust_draygon_breath_bubbles_fourth</c>, an installed animation frame.</summary>
    MiscDustDraygonBreathBubblesFourth,
    /// <summary><c>misc_dust_draygon_breath_bubbles_fifth</c>, an installed animation frame.</summary>
    MiscDustDraygonBreathBubblesFifth,
    /// <summary><c>misc_dust_draygon_breath_bubbles_sixth</c>, an installed animation frame.</summary>
    MiscDustDraygonBreathBubblesSixth,
    /// <summary><c>misc_dust_draygon_breath_bubbles_seventh</c>, an installed animation frame.</summary>
    MiscDustDraygonBreathBubblesSeventh,
    /// <summary><c>misc_dust_draygon_breath_bubbles_eighth</c>, an installed animation frame.</summary>
    MiscDustDraygonBreathBubblesEighth,
    /// <summary><c>misc_dust_draygon_breath_bubbles_ninth</c>, an installed animation frame.</summary>
    MiscDustDraygonBreathBubblesNinth,
    /// <summary><c>downward_gate_moving</c>, an installed animation frame.</summary>
    DownwardGateMoving,
    /// <summary><c>downward_gate_third_moving_step</c>, an installed animation frame.</summary>
    DownwardGateThirdMovingStep,
    /// <summary><c>downward_gate_fourth_moving_step</c>, an installed animation frame.</summary>
    DownwardGateFourthMovingStep,
    /// <summary><c>downward_gate_fifth_moving_step</c>, an installed animation frame.</summary>
    DownwardGateFifthMovingStep,
    /// <summary><c>downward_gate_closed</c>, an installed animation frame.</summary>
    DownwardGateClosed,
    /// <summary><c>downward_gate_third_closed_step</c>, an installed animation frame.</summary>
    DownwardGateThirdClosedStep,
    /// <summary><c>downward_gate_fourth_closed_step</c>, an installed animation frame.</summary>
    DownwardGateFourthClosedStep,
    /// <summary><c>downward_gate_fifth_closed_step</c>, an installed animation frame.</summary>
    DownwardGateFifthClosedStep,
    /// <summary><c>downward_gate_sixth_closed_step</c>, an installed animation frame.</summary>
    DownwardGateSixthClosedStep,
    /// <summary><c>save_station_electricity_start_first</c>, an installed animation frame.</summary>
    SaveStationElectricityStartFirst,
    /// <summary><c>save_station_electricity_start_second</c>, an installed animation frame.</summary>
    SaveStationElectricityStartSecond,
    /// <summary><c>save_station_electricity_start_third</c>, an installed animation frame.</summary>
    SaveStationElectricityStartThird,
    /// <summary><c>save_station_electricity_start_fourth</c>, an installed animation frame.</summary>
    SaveStationElectricityStartFourth,
    /// <summary><c>save_station_electricity_start_fifth</c>, an installed animation frame.</summary>
    SaveStationElectricityStartFifth,
    /// <summary><c>save_station_electricity_start_sixth</c>, an installed animation frame.</summary>
    SaveStationElectricityStartSixth,
    /// <summary><c>save_station_electricity_start_seventh</c>, an installed animation frame.</summary>
    SaveStationElectricityStartSeventh,
    /// <summary><c>save_station_electricity_start_eighth</c>, an installed animation frame.</summary>
    SaveStationElectricityStartEighth,
    /// <summary><c>botwoon_body_up_left_first</c>, an installed animation frame.</summary>
    BotwoonBodyUpLeftFirst,
    /// <summary><c>botwoon_body_up_left_second</c>, an installed animation frame.</summary>
    BotwoonBodyUpLeftSecond,
    /// <summary><c>botwoon_body_up_left_third</c>, an installed animation frame.</summary>
    BotwoonBodyUpLeftThird,
    /// <summary><c>botwoon_body_up_left_fourth</c>, an installed animation frame.</summary>
    BotwoonBodyUpLeftFourth,
    /// <summary><c>botwoon_body_left_first</c>, an installed animation frame.</summary>
    BotwoonBodyLeftFirst,
    /// <summary><c>botwoon_body_left_second</c>, an installed animation frame.</summary>
    BotwoonBodyLeftSecond,
    /// <summary><c>botwoon_body_left_third</c>, an installed animation frame.</summary>
    BotwoonBodyLeftThird,
    /// <summary><c>botwoon_body_left_fourth</c>, an installed animation frame.</summary>
    BotwoonBodyLeftFourth,
    /// <summary><c>botwoon_body_down_left_first</c>, an installed animation frame.</summary>
    BotwoonBodyDownLeftFirst,
    /// <summary><c>botwoon_body_down_left_second</c>, an installed animation frame.</summary>
    BotwoonBodyDownLeftSecond,
    /// <summary><c>botwoon_body_down_left_third</c>, an installed animation frame.</summary>
    BotwoonBodyDownLeftThird,
    /// <summary><c>botwoon_body_down_left_fourth</c>, an installed animation frame.</summary>
    BotwoonBodyDownLeftFourth,
    /// <summary><c>botwoon_body_down_first</c>, an installed animation frame.</summary>
    BotwoonBodyDownFirst,
    /// <summary><c>botwoon_body_down_second</c>, an installed animation frame.</summary>
    BotwoonBodyDownSecond,
    /// <summary><c>botwoon_body_down_third</c>, an installed animation frame.</summary>
    BotwoonBodyDownThird,
    /// <summary><c>botwoon_body_down_fourth</c>, an installed animation frame.</summary>
    BotwoonBodyDownFourth,
    /// <summary><c>botwoon_body_down_right_first</c>, an installed animation frame.</summary>
    BotwoonBodyDownRightFirst,
    /// <summary><c>botwoon_body_down_right_second</c>, an installed animation frame.</summary>
    BotwoonBodyDownRightSecond,
    /// <summary><c>botwoon_body_down_right_third</c>, an installed animation frame.</summary>
    BotwoonBodyDownRightThird,
    /// <summary><c>botwoon_body_down_right_fourth</c>, an installed animation frame.</summary>
    BotwoonBodyDownRightFourth,
    /// <summary><c>botwoon_body_right_first</c>, an installed animation frame.</summary>
    BotwoonBodyRightFirst,
    /// <summary><c>botwoon_body_right_second</c>, an installed animation frame.</summary>
    BotwoonBodyRightSecond,
    /// <summary><c>botwoon_body_right_third</c>, an installed animation frame.</summary>
    BotwoonBodyRightThird,
    /// <summary><c>botwoon_body_right_fourth</c>, an installed animation frame.</summary>
    BotwoonBodyRightFourth,
    /// <summary><c>botwoon_body_up_right_first</c>, an installed animation frame.</summary>
    BotwoonBodyUpRightFirst,
    /// <summary><c>botwoon_body_up_right_second</c>, an installed animation frame.</summary>
    BotwoonBodyUpRightSecond,
    /// <summary><c>botwoon_body_up_right_third</c>, an installed animation frame.</summary>
    BotwoonBodyUpRightThird,
    /// <summary><c>botwoon_body_up_right_fourth</c>, an installed animation frame.</summary>
    BotwoonBodyUpRightFourth,
    /// <summary><c>botwoon_body_up_first</c>, an installed animation frame.</summary>
    BotwoonBodyUpFirst,
    /// <summary><c>botwoon_body_up_second</c>, an installed animation frame.</summary>
    BotwoonBodyUpSecond,
    /// <summary><c>botwoon_body_up_third</c>, an installed animation frame.</summary>
    BotwoonBodyUpThird,
    /// <summary><c>botwoon_body_up_fourth</c>, an installed animation frame.</summary>
    BotwoonBodyUpFourth,
    /// <summary><c>botwoon_tail_up</c>, an installed animation frame.</summary>
    BotwoonTailUp,
    /// <summary><c>botwoon_tail_up_left</c>, an installed animation frame.</summary>
    BotwoonTailUpLeft,
    /// <summary><c>botwoon_tail_left</c>, an installed animation frame.</summary>
    BotwoonTailLeft,
    /// <summary><c>botwoon_tail_down_left</c>, an installed animation frame.</summary>
    BotwoonTailDownLeft,
    /// <summary><c>botwoon_tail_down</c>, an installed animation frame.</summary>
    BotwoonTailDown,
    /// <summary><c>botwoon_tail_down_right</c>, an installed animation frame.</summary>
    BotwoonTailDownRight,
    /// <summary><c>botwoon_tail_right</c>, an installed animation frame.</summary>
    BotwoonTailRight,
    /// <summary><c>botwoon_tail_up_right</c>, an installed animation frame.</summary>
    BotwoonTailUpRight,
    /// <summary><c>botwoon_hidden_up</c>, an installed animation frame.</summary>
    BotwoonHiddenUp,
    /// <summary><c>botwoon_spit_up_first</c>, an installed animation frame.</summary>
    BotwoonSpitUpFirst,
    /// <summary><c>botwoon_spit_up_second</c>, an installed animation frame.</summary>
    BotwoonSpitUpSecond,
    /// <summary><c>botwoon_spit_up_third</c>, an installed animation frame.</summary>
    BotwoonSpitUpThird,
    /// <summary><c>botwoon_spit_up_fourth</c>, an installed animation frame.</summary>
    BotwoonSpitUpFourth,
    /// <summary><c>botwoon_spit_up_fifth</c>, an installed animation frame.</summary>
    BotwoonSpitUpFifth,
    /// <summary><c>yapping_maw_facing_down</c>, an installed animation frame.</summary>
    YappingMawFacingDown,
    /// <summary><c>yapping_maw_facing_up</c>, an installed animation frame.</summary>
    YappingMawFacingUp,
    /// <summary><c>enemy_death_respawn_tail</c>, an installed animation frame.</summary>
    EnemyDeathRespawnTail,
    /// <summary><c>enemy_death_big_explosion</c>, an installed animation frame.</summary>
    EnemyDeathBigExplosion,
    /// <summary><c>enemy_death_mini_kraid_explosion</c>, an installed animation frame.</summary>
    EnemyDeathMiniKraidExplosion,
    /// <summary><c>enemy_death_normal_explosion_first</c>, an installed animation frame.</summary>
    EnemyDeathNormalExplosionFirst,
    /// <summary><c>enemy_death_normal_explosion_second</c>, an installed animation frame.</summary>
    EnemyDeathNormalExplosionSecond,
    /// <summary><c>enemy_death_normal_explosion_third</c>, an installed animation frame.</summary>
    EnemyDeathNormalExplosionThird,
    /// <summary><c>enemy_death_normal_explosion_fourth</c>, an installed animation frame.</summary>
    EnemyDeathNormalExplosionFourth,
    /// <summary><c>enemy_death_normal_explosion_fifth</c>, an installed animation frame.</summary>
    EnemyDeathNormalExplosionFifth,
    /// <summary><c>enemy_death_normal_explosion_sixth</c>, an installed animation frame.</summary>
    EnemyDeathNormalExplosionSixth,
    /// <summary><c>enemy_death_small_explosion_first</c>, an installed animation frame.</summary>
    EnemyDeathSmallExplosionFirst,
    /// <summary><c>enemy_death_small_explosion_second</c>, an installed animation frame.</summary>
    EnemyDeathSmallExplosionSecond,
    /// <summary><c>enemy_death_small_explosion_third</c>, an installed animation frame.</summary>
    EnemyDeathSmallExplosionThird,
    /// <summary><c>enemy_death_small_explosion_fourth</c>, an installed animation frame.</summary>
    EnemyDeathSmallExplosionFourth,
    /// <summary><c>enemy_death_small_explosion_fifth</c>, an installed animation frame.</summary>
    EnemyDeathSmallExplosionFifth,
    /// <summary><c>enemy_death_small_explosion_sixth</c>, an installed animation frame.</summary>
    EnemyDeathSmallExplosionSixth,
    /// <summary><c>enemy_pickup_small_energy_first</c>, an installed animation frame.</summary>
    EnemyPickupSmallEnergyFirst,
    /// <summary><c>enemy_pickup_small_energy_second</c>, an installed animation frame.</summary>
    EnemyPickupSmallEnergySecond,
    /// <summary><c>enemy_pickup_small_energy_third</c>, an installed animation frame.</summary>
    EnemyPickupSmallEnergyThird,
    /// <summary><c>enemy_pickup_small_energy_fourth</c>, an installed animation frame.</summary>
    EnemyPickupSmallEnergyFourth,
    /// <summary><c>enemy_pickup_big_energy_first</c>, an installed animation frame.</summary>
    EnemyPickupBigEnergyFirst,
    /// <summary><c>enemy_pickup_big_energy_second</c>, an installed animation frame.</summary>
    EnemyPickupBigEnergySecond,
    /// <summary><c>enemy_pickup_big_energy_third</c>, an installed animation frame.</summary>
    EnemyPickupBigEnergyThird,
    /// <summary><c>enemy_pickup_big_energy_fourth</c>, an installed animation frame.</summary>
    EnemyPickupBigEnergyFourth,
    /// <summary><c>enemy_pickup_missiles_first</c>, an installed animation frame.</summary>
    EnemyPickupMissilesFirst,
    /// <summary><c>enemy_pickup_missiles_second</c>, an installed animation frame.</summary>
    EnemyPickupMissilesSecond,
    /// <summary><c>enemy_pickup_super_missiles_first</c>, an installed animation frame.</summary>
    EnemyPickupSuperMissilesFirst,
    /// <summary><c>enemy_pickup_super_missiles_second</c>, an installed animation frame.</summary>
    EnemyPickupSuperMissilesSecond,
    /// <summary><c>enemy_pickup_power_bombs_first</c>, an installed animation frame.</summary>
    EnemyPickupPowerBombsFirst,
    /// <summary><c>enemy_pickup_power_bombs_second</c>, an installed animation frame.</summary>
    EnemyPickupPowerBombsSecond,
    /// <summary><c>enemy_pickup_power_bombs_third</c>, an installed animation frame.</summary>
    EnemyPickupPowerBombsThird,
    /// <summary><c>enemy_pickup_power_bombs_fourth</c>, an installed animation frame.</summary>
    EnemyPickupPowerBombsFourth,
    /// <summary><c>enemy_death_killed_by_samus_contact_first</c>, an installed animation frame.</summary>
    EnemyDeathKilledBySamusContactFirst,
    /// <summary><c>enemy_death_killed_by_samus_contact_second</c>, an installed animation frame.</summary>
    EnemyDeathKilledBySamusContactSecond,
    /// <summary><c>enemy_death_killed_by_samus_contact_third</c>, an installed animation frame.</summary>
    EnemyDeathKilledBySamusContactThird,
    /// <summary><c>enemy_death_killed_by_samus_contact_fourth</c>, an installed animation frame.</summary>
    EnemyDeathKilledBySamusContactFourth,
    /// <summary><c>enemy_death_killed_by_samus_contact_fifth</c>, an installed animation frame.</summary>
    EnemyDeathKilledBySamusContactFifth,
    /// <summary><c>enemy_death_killed_by_samus_contact_sixth</c>, an installed animation frame.</summary>
    EnemyDeathKilledBySamusContactSixth,
    /// <summary><c>enemy_death_killed_by_samus_contact_seventh</c>, an installed animation frame.</summary>
    EnemyDeathKilledBySamusContactSeventh,
    /// <summary><c>enemy_death_killed_by_samus_contact_eighth</c>, an installed animation frame.</summary>
    EnemyDeathKilledBySamusContactEighth,
    /// <summary><c>enemy_death_killed_by_samus_contact_ninth</c>, an installed animation frame.</summary>
    EnemyDeathKilledBySamusContactNinth,
    /// <summary><c>enemy_death_killed_by_samus_contact_tenth</c>, an installed animation frame.</summary>
    EnemyDeathKilledBySamusContactTenth,
    /// <summary><c>enemy_death_killed_by_samus_contact_eleventh</c>, an installed animation frame.</summary>
    EnemyDeathKilledBySamusContactEleventh,
    /// <summary><c>enemy_death_killed_by_samus_contact_twelfth</c>, an installed animation frame.</summary>
    EnemyDeathKilledBySamusContactTwelfth,
    /// <summary><c>enemy_death_killed_by_samus_contact_thirteenth</c>, an installed animation frame.</summary>
    EnemyDeathKilledBySamusContactThirteenth,
    /// <summary><c>enemy_death_killed_by_samus_contact_fourteenth</c>, an installed animation frame.</summary>
    EnemyDeathKilledBySamusContactFourteenth,
    /// <summary><c>enemy_death_killed_by_samus_contact_fifteenth</c>, an installed animation frame.</summary>
    EnemyDeathKilledBySamusContactFifteenth,
    /// <summary><c>enemy_death_killed_by_samus_contact_sixteenth</c>, an installed animation frame.</summary>
    EnemyDeathKilledBySamusContactSixteenth,
    /// <summary><c>falling_spark_falling_first</c>, an installed animation frame.</summary>
    FallingSparkFallingFirst,
    /// <summary><c>falling_spark_falling_second</c>, an installed animation frame.</summary>
    FallingSparkFallingSecond,
    /// <summary><c>falling_spark_falling_third</c>, an installed animation frame.</summary>
    FallingSparkFallingThird,
    /// <summary><c>falling_spark_hit_floor_first</c>, an installed animation frame.</summary>
    FallingSparkHitFloorFirst,
    /// <summary><c>falling_spark_hit_floor_second</c>, an installed animation frame.</summary>
    FallingSparkHitFloorSecond,
    /// <summary><c>falling_spark_hit_floor_third</c>, an installed animation frame.</summary>
    FallingSparkHitFloorThird,
    /// <summary><c>falling_spark_hit_floor_fourth</c>, an installed animation frame.</summary>
    FallingSparkHitFloorFourth,
    /// <summary><c>falling_spark_hit_floor_fifth</c>, an installed animation frame.</summary>
    FallingSparkHitFloorFifth,
    /// <summary><c>falling_spark_hit_floor_sixth</c>, an installed animation frame.</summary>
    FallingSparkHitFloorSixth,
    /// <summary><c>falling_spark_hit_floor_seventh</c>, an installed animation frame.</summary>
    FallingSparkHitFloorSeventh,
    /// <summary><c>falling_spark_hit_floor_eighth</c>, an installed animation frame.</summary>
    FallingSparkHitFloorEighth,
    /// <summary><c>falling_spark_hit_floor_ninth</c>, an installed animation frame.</summary>
    FallingSparkHitFloorNinth,
    /// <summary><c>falling_spark_hit_floor_tenth</c>, an installed animation frame.</summary>
    FallingSparkHitFloorTenth,
    /// <summary><c>falling_spark_hit_floor_eleventh</c>, an installed animation frame.</summary>
    FallingSparkHitFloorEleventh,
    /// <summary><c>ceres_elevator_pad_first</c>, a composition.</summary>
    CeresElevatorPadFirst,
    /// <summary><c>ceres_elevator_pad_second</c>, a composition.</summary>
    CeresElevatorPadSecond,
    /// <summary><c>ceres_elevator_platform</c>, a composition.</summary>
    CeresElevatorPlatform,
}
