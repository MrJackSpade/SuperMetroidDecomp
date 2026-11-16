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
    /// <summary>The Skree debris, one composed picture.</summary>
    SkreeDebris,
    /// <summary>The Metaree debris, one composed picture.</summary>
    MetareeDebris,
    /// <summary>The first frame of the Draygon goop.</summary>
    DraygonGoopFirst,
    /// <summary>The second frame of the Draygon goop.</summary>
    DraygonGoopSecond,
    /// <summary>The third frame of the Draygon goop.</summary>
    DraygonGoopThird,
    /// <summary>The fourth frame of the Draygon goop.</summary>
    DraygonGoopFourth,
    /// <summary>The fifth frame of the Draygon goop.</summary>
    DraygonGoopFifth,
    /// <summary>The sixth frame of the Draygon goop.</summary>
    DraygonGoopSixth,
    /// <summary>The first frame of the Draygon goop shot.</summary>
    DraygonGoopShotFirst,
    /// <summary>The second frame of the Draygon goop shot.</summary>
    DraygonGoopShotSecond,
    /// <summary>The first frame of the Draygon wall turret bloom.</summary>
    DraygonWallTurretBloomFirst,
    /// <summary>The second frame of the Draygon wall turret bloom.</summary>
    DraygonWallTurretBloomSecond,
    /// <summary>The third frame of the Draygon wall turret bloom.</summary>
    DraygonWallTurretBloomThird,
    /// <summary>The fourth frame of the Draygon wall turret bloom.</summary>
    DraygonWallTurretBloomFourth,
    /// <summary>The fifth frame of the Draygon wall turret bloom.</summary>
    DraygonWallTurretBloomFifth,
    /// <summary>The sixth frame of the Draygon wall turret bloom.</summary>
    DraygonWallTurretBloomSixth,
    /// <summary>The seventh frame of the Draygon wall turret bloom.</summary>
    DraygonWallTurretBloomSeventh,
    /// <summary>The eighth frame of the Draygon wall turret bloom.</summary>
    DraygonWallTurretBloomEighth,
    /// <summary>The ninth frame of the Draygon wall turret bloom.</summary>
    DraygonWallTurretBloomNinth,
    /// <summary>The tenth frame of the Draygon wall turret bloom.</summary>
    DraygonWallTurretBloomTenth,
    /// <summary>The eleventh frame of the Draygon wall turret bloom.</summary>
    DraygonWallTurretBloomEleventh,
    /// <summary>The twelfth frame of the Draygon wall turret bloom.</summary>
    DraygonWallTurretBloomTwelfth,
    /// <summary>The thirteenth frame of the Draygon wall turret bloom.</summary>
    DraygonWallTurretBloomThirteenth,
    /// <summary>The fourteenth frame of the Draygon wall turret bloom.</summary>
    DraygonWallTurretBloomFourteenth,
    /// <summary>The fifteenth frame of the Draygon wall turret bloom.</summary>
    DraygonWallTurretBloomFifteenth,
    /// <summary>The sixteenth frame of the Draygon wall turret bloom.</summary>
    DraygonWallTurretBloomSixteenth,
    /// <summary>The first frame of the Draygon wall turret flight.</summary>
    DraygonWallTurretFlightFirst,
    /// <summary>The second frame of the Draygon wall turret flight.</summary>
    DraygonWallTurretFlightSecond,
    /// <summary>The third frame of the Draygon wall turret flight.</summary>
    DraygonWallTurretFlightThird,
    /// <summary>The first frame of the Crocomire mouth.</summary>
    CrocomireMouthFirst,
    /// <summary>The second frame of the Crocomire mouth.</summary>
    CrocomireMouthSecond,
    /// <summary>The third frame of the Crocomire mouth.</summary>
    CrocomireMouthThird,
    /// <summary>The fourth frame of the Crocomire mouth.</summary>
    CrocomireMouthFourth,
    /// <summary>The fifth frame of the Crocomire mouth.</summary>
    CrocomireMouthFifth,
    /// <summary>The sixth frame of the Crocomire mouth.</summary>
    CrocomireMouthSixth,
    /// <summary>The frame of the Crocomire bridge fragment.</summary>
    CrocomireBridgeFragment,
    /// <summary>The frame of the Crocomire spike wall piece.</summary>
    CrocomireSpikeWallPiece,
    /// <summary>The first frame of the Crocomire shot.</summary>
    CrocomireShotFirst,
    /// <summary>The second frame of the Crocomire shot.</summary>
    CrocomireShotSecond,
    /// <summary>The third frame of the Crocomire shot.</summary>
    CrocomireShotThird,
    /// <summary>The fourth frame of the Crocomire shot.</summary>
    CrocomireShotFourth,
    /// <summary>The fifth frame of the Crocomire shot.</summary>
    CrocomireShotFifth,
    /// <summary>The first frame of the Ceres Ridley launch.</summary>
    CeresRidleyLaunchFirst,
    /// <summary>The second frame of the Ceres Ridley launch.</summary>
    CeresRidleyLaunchSecond,
    /// <summary>The first frame of the Ceres Ridley flight.</summary>
    CeresRidleyFlightFirst,
    /// <summary>The second frame of the Ceres Ridley flight.</summary>
    CeresRidleyFlightSecond,
    /// <summary>The third frame of the Ceres Ridley flight.</summary>
    CeresRidleyFlightThird,
    /// <summary>The fourth frame of the Ceres Ridley flight.</summary>
    CeresRidleyFlightFourth,
    /// <summary>The first frame of the Ceres Ridley afterburn final.</summary>
    CeresRidleyAfterburnFinalFirst,
    /// <summary>The second frame of the Ceres Ridley afterburn final.</summary>
    CeresRidleyAfterburnFinalSecond,
    /// <summary>The third frame of the Ceres Ridley afterburn final.</summary>
    CeresRidleyAfterburnFinalThird,
    /// <summary>The fourth frame of the Ceres Ridley afterburn final.</summary>
    CeresRidleyAfterburnFinalFourth,
    /// <summary>The fifth frame of the Ceres Ridley afterburn final.</summary>
    CeresRidleyAfterburnFinalFifth,
    /// <summary>The first frame of the Ceres Ridley horizontal center.</summary>
    CeresRidleyHorizontalCenterFirst,
    /// <summary>The second frame of the Ceres Ridley horizontal center.</summary>
    CeresRidleyHorizontalCenterSecond,
    /// <summary>The third frame of the Ceres Ridley horizontal center.</summary>
    CeresRidleyHorizontalCenterThird,
    /// <summary>The fourth frame of the Ceres Ridley horizontal center.</summary>
    CeresRidleyHorizontalCenterFourth,
    /// <summary>The fifth frame of the Ceres Ridley horizontal center.</summary>
    CeresRidleyHorizontalCenterFifth,
    /// <summary>The first frame of the Ceres Ridley vertical center.</summary>
    CeresRidleyVerticalCenterFirst,
    /// <summary>The second frame of the Ceres Ridley vertical center.</summary>
    CeresRidleyVerticalCenterSecond,
    /// <summary>The third frame of the Ceres Ridley vertical center.</summary>
    CeresRidleyVerticalCenterThird,
    /// <summary>The fourth frame of the Ceres Ridley vertical center.</summary>
    CeresRidleyVerticalCenterFourth,
    /// <summary>The fifth frame of the Ceres Ridley vertical center.</summary>
    CeresRidleyVerticalCenterFifth,
    /// <summary>The first frame of the Ceres Ridley directional.</summary>
    CeresRidleyDirectionalFirst,
    /// <summary>The second frame of the Ceres Ridley directional.</summary>
    CeresRidleyDirectionalSecond,
    /// <summary>The third frame of the Ceres Ridley directional.</summary>
    CeresRidleyDirectionalThird,
    /// <summary>The fourth frame of the Ceres Ridley directional.</summary>
    CeresRidleyDirectionalFourth,
    /// <summary>The fifth frame of the Ceres Ridley directional.</summary>
    CeresRidleyDirectionalFifth,
    /// <summary>The frame of the Ceres falling debris light.</summary>
    CeresFallingDebrisLight,
    /// <summary>The frame of the Ceres falling debris dark.</summary>
    CeresFallingDebrisDark,
    /// <summary>The first frame of the Phantoon destroyable idle.</summary>
    PhantoonDestroyableIdleFirst,
    /// <summary>The second frame of the Phantoon destroyable idle.</summary>
    PhantoonDestroyableIdleSecond,
    /// <summary>The third frame of the Phantoon destroyable idle.</summary>
    PhantoonDestroyableIdleThird,
    /// <summary>The fifth frame of the Phantoon destroyable idle.</summary>
    PhantoonDestroyableIdleFifth,
    /// <summary>The first frame of the Phantoon casual bouncing.</summary>
    PhantoonCasualBouncingFirst,
    /// <summary>The second frame of the Phantoon casual bouncing.</summary>
    PhantoonCasualBouncingSecond,
    /// <summary>The third frame of the Phantoon casual bouncing.</summary>
    PhantoonCasualBouncingThird,
    /// <summary>The fifth frame of the Phantoon casual bouncing.</summary>
    PhantoonCasualBouncingFifth,
    /// <summary>The sixth frame of the Phantoon casual bouncing.</summary>
    PhantoonCasualBouncingSixth,
    /// <summary>The third frame of the Phantoon casual resting.</summary>
    PhantoonCasualRestingThird,
    /// <summary>The fourth frame of the Phantoon casual resting.</summary>
    PhantoonCasualRestingFourth,
    /// <summary>The fifth frame of the Phantoon casual resting.</summary>
    PhantoonCasualRestingFifth,
    /// <summary>The sixth frame of the Phantoon casual resting.</summary>
    PhantoonCasualRestingSixth,
    /// <summary>The first frame of the Phantoon dying.</summary>
    PhantoonDyingFirst,
    /// <summary>The second frame of the Phantoon dying.</summary>
    PhantoonDyingSecond,
    /// <summary>The third frame of the Phantoon dying.</summary>
    PhantoonDyingThird,
    /// <summary>The fourth frame of the Phantoon dying.</summary>
    PhantoonDyingFourth,
    /// <summary>The first frame of the Phantoon rain impact.</summary>
    PhantoonRainImpactFirst,
    /// <summary>The fourth frame of the Phantoon rain impact.</summary>
    PhantoonRainImpactFourth,
    /// <summary>The fifth frame of the Phantoon rain impact.</summary>
    PhantoonRainImpactFifth,
    /// <summary>The third frame of the Phantoon casual falling.</summary>
    PhantoonCasualFallingThird,
    /// <summary>The fourth frame of the Phantoon casual falling.</summary>
    PhantoonCasualFallingFourth,
    /// <summary>The fifth frame of the Phantoon casual falling.</summary>
    PhantoonCasualFallingFifth,
    /// <summary>The sixth frame of the Phantoon casual falling.</summary>
    PhantoonCasualFallingSixth,
    /// <summary>The first frame of the Phantoon starting flame.</summary>
    PhantoonStartingFlameFirst,
    /// <summary>The second frame of the Phantoon starting flame.</summary>
    PhantoonStartingFlameSecond,
    /// <summary>The third frame of the Phantoon starting flame.</summary>
    PhantoonStartingFlameThird,
    /// <summary>The first frame of the Phantoon destroyable shot.</summary>
    PhantoonDestroyableShotFirst,
    /// <summary>The second frame of the Phantoon destroyable shot.</summary>
    PhantoonDestroyableShotSecond,
    /// <summary>The third frame of the Phantoon destroyable shot.</summary>
    PhantoonDestroyableShotThird,
    /// <summary>The fourth frame of the Phantoon destroyable shot.</summary>
    PhantoonDestroyableShotFourth,
    /// <summary>The frame of the Kraid rock.</summary>
    KraidRock,
    /// <summary>The frame of the Kraid rock rising right pose.</summary>
    KraidRockRisingRightPose,
    /// <summary>The first frame of the Kraid rock burst.</summary>
    KraidRockBurstFirst,
    /// <summary>The second frame of the Kraid rock burst.</summary>
    KraidRockBurstSecond,
    /// <summary>The third frame of the Kraid rock burst.</summary>
    KraidRockBurstThird,
    /// <summary>The fourth frame of the Kraid rock burst.</summary>
    KraidRockBurstFourth,
    /// <summary>The fifth frame of the Kraid rock burst.</summary>
    KraidRockBurstFifth,
    /// <summary>The frame of the Fake Kraid spit.</summary>
    FakeKraidSpit,
    /// <summary>The frame of the Fake Kraid spike left.</summary>
    FakeKraidSpikeLeft,
    /// <summary>The frame of the Fake Kraid spike right.</summary>
    FakeKraidSpikeRight,
    /// <summary>The first frame of the Alcoon fireball initial.</summary>
    AlcoonFireballInitialFirst,
    /// <summary>The second frame of the Alcoon fireball initial.</summary>
    AlcoonFireballInitialSecond,
    /// <summary>The third frame of the Alcoon fireball initial.</summary>
    AlcoonFireballInitialThird,
    /// <summary>The fourth frame of the Alcoon fireball initial.</summary>
    AlcoonFireballInitialFourth,
    /// <summary>The first frame of the Space Pirate laser left.</summary>
    SpacePirateLaserLeftFirst,
    /// <summary>The second frame of the Space Pirate laser left.</summary>
    SpacePirateLaserLeftSecond,
    /// <summary>The third frame of the Space Pirate laser left.</summary>
    SpacePirateLaserLeftThird,
    /// <summary>The fourth frame of the Space Pirate laser left.</summary>
    SpacePirateLaserLeftFourth,
    /// <summary>The fifth frame of the Space Pirate laser left.</summary>
    SpacePirateLaserLeftFifth,
    /// <summary>The sixth frame of the Space Pirate laser left.</summary>
    SpacePirateLaserLeftSixth,
    /// <summary>The seventh frame of the Space Pirate laser left.</summary>
    SpacePirateLaserLeftSeventh,
    /// <summary>The eighth frame of the Space Pirate laser left.</summary>
    SpacePirateLaserLeftEighth,
    /// <summary>The ninth frame of the Space Pirate laser left.</summary>
    SpacePirateLaserLeftNinth,
    /// <summary>The tenth frame of the Space Pirate laser left.</summary>
    SpacePirateLaserLeftTenth,
    /// <summary>The eleventh frame of the Space Pirate laser left.</summary>
    SpacePirateLaserLeftEleventh,
    /// <summary>The first frame of the Space Pirate laser left loop.</summary>
    SpacePirateLaserLeftLoopFirst,
    /// <summary>The second frame of the Space Pirate laser left loop.</summary>
    SpacePirateLaserLeftLoopSecond,
    /// <summary>The first frame of the Space Pirate laser right.</summary>
    SpacePirateLaserRightFirst,
    /// <summary>The second frame of the Space Pirate laser right.</summary>
    SpacePirateLaserRightSecond,
    /// <summary>The third frame of the Space Pirate laser right.</summary>
    SpacePirateLaserRightThird,
    /// <summary>The fourth frame of the Space Pirate laser right.</summary>
    SpacePirateLaserRightFourth,
    /// <summary>The fifth frame of the Space Pirate laser right.</summary>
    SpacePirateLaserRightFifth,
    /// <summary>The sixth frame of the Space Pirate laser right.</summary>
    SpacePirateLaserRightSixth,
    /// <summary>The seventh frame of the Space Pirate laser right.</summary>
    SpacePirateLaserRightSeventh,
    /// <summary>The eighth frame of the Space Pirate laser right.</summary>
    SpacePirateLaserRightEighth,
    /// <summary>The ninth frame of the Space Pirate laser right.</summary>
    SpacePirateLaserRightNinth,
    /// <summary>The tenth frame of the Space Pirate laser right.</summary>
    SpacePirateLaserRightTenth,
    /// <summary>The eleventh frame of the Space Pirate laser right.</summary>
    SpacePirateLaserRightEleventh,
    /// <summary>The first frame of the Space Pirate laser right loop.</summary>
    SpacePirateLaserRightLoopFirst,
    /// <summary>The second frame of the Space Pirate laser right loop.</summary>
    SpacePirateLaserRightLoopSecond,
    /// <summary>The first frame of the Space Pirate claw left loop.</summary>
    SpacePirateClawLeftLoopFirst,
    /// <summary>The second frame of the Space Pirate claw left loop.</summary>
    SpacePirateClawLeftLoopSecond,
    /// <summary>The third frame of the Space Pirate claw left loop.</summary>
    SpacePirateClawLeftLoopThird,
    /// <summary>The fourth frame of the Space Pirate claw left loop.</summary>
    SpacePirateClawLeftLoopFourth,
    /// <summary>The fifth frame of the Space Pirate claw left loop.</summary>
    SpacePirateClawLeftLoopFifth,
    /// <summary>The sixth frame of the Space Pirate claw left loop.</summary>
    SpacePirateClawLeftLoopSixth,
    /// <summary>The seventh frame of the Space Pirate claw left loop.</summary>
    SpacePirateClawLeftLoopSeventh,
    /// <summary>The eighth frame of the Space Pirate claw left loop.</summary>
    SpacePirateClawLeftLoopEighth,
    /// <summary>The first frame of the Space Pirate claw right loop.</summary>
    SpacePirateClawRightLoopFirst,
    /// <summary>The second frame of the Space Pirate claw right loop.</summary>
    SpacePirateClawRightLoopSecond,
    /// <summary>The third frame of the Space Pirate claw right loop.</summary>
    SpacePirateClawRightLoopThird,
    /// <summary>The fourth frame of the Space Pirate claw right loop.</summary>
    SpacePirateClawRightLoopFourth,
    /// <summary>The fifth frame of the Space Pirate claw right loop.</summary>
    SpacePirateClawRightLoopFifth,
    /// <summary>The sixth frame of the Space Pirate claw right loop.</summary>
    SpacePirateClawRightLoopSixth,
    /// <summary>The seventh frame of the Space Pirate claw right loop.</summary>
    SpacePirateClawRightLoopSeventh,
    /// <summary>The eighth frame of the Space Pirate claw right loop.</summary>
    SpacePirateClawRightLoopEighth,
    /// <summary>The first frame of the gunship dust right inner.</summary>
    GunshipDustRightInnerFirst,
    /// <summary>The second frame of the gunship dust right inner.</summary>
    GunshipDustRightInnerSecond,
    /// <summary>The third frame of the gunship dust right inner.</summary>
    GunshipDustRightInnerThird,
    /// <summary>The fourth frame of the gunship dust right inner.</summary>
    GunshipDustRightInnerFourth,
    /// <summary>The fifth frame of the gunship dust right inner.</summary>
    GunshipDustRightInnerFifth,
    /// <summary>The sixth frame of the gunship dust right inner.</summary>
    GunshipDustRightInnerSixth,
    /// <summary>The seventh frame of the gunship dust right inner.</summary>
    GunshipDustRightInnerSeventh,
    /// <summary>The eighth frame of the gunship dust right inner.</summary>
    GunshipDustRightInnerEighth,
    /// <summary>The first frame of the gunship dust right middle.</summary>
    GunshipDustRightMiddleFirst,
    /// <summary>The second frame of the gunship dust right middle.</summary>
    GunshipDustRightMiddleSecond,
    /// <summary>The third frame of the gunship dust right middle.</summary>
    GunshipDustRightMiddleThird,
    /// <summary>The fourth frame of the gunship dust right middle.</summary>
    GunshipDustRightMiddleFourth,
    /// <summary>The fifth frame of the gunship dust right middle.</summary>
    GunshipDustRightMiddleFifth,
    /// <summary>The sixth frame of the gunship dust right middle.</summary>
    GunshipDustRightMiddleSixth,
    /// <summary>The seventh frame of the gunship dust right middle.</summary>
    GunshipDustRightMiddleSeventh,
    /// <summary>The eighth frame of the gunship dust right middle.</summary>
    GunshipDustRightMiddleEighth,
    /// <summary>The first frame of the gunship dust right outer.</summary>
    GunshipDustRightOuterFirst,
    /// <summary>The second frame of the gunship dust right outer.</summary>
    GunshipDustRightOuterSecond,
    /// <summary>The third frame of the gunship dust right outer.</summary>
    GunshipDustRightOuterThird,
    /// <summary>The fourth frame of the gunship dust right outer.</summary>
    GunshipDustRightOuterFourth,
    /// <summary>The fifth frame of the gunship dust right outer.</summary>
    GunshipDustRightOuterFifth,
    /// <summary>The sixth frame of the gunship dust right outer.</summary>
    GunshipDustRightOuterSixth,
    /// <summary>The seventh frame of the gunship dust right outer.</summary>
    GunshipDustRightOuterSeventh,
    /// <summary>The first frame of the gunship dust left inner.</summary>
    GunshipDustLeftInnerFirst,
    /// <summary>The second frame of the gunship dust left inner.</summary>
    GunshipDustLeftInnerSecond,
    /// <summary>The third frame of the gunship dust left inner.</summary>
    GunshipDustLeftInnerThird,
    /// <summary>The fourth frame of the gunship dust left inner.</summary>
    GunshipDustLeftInnerFourth,
    /// <summary>The fifth frame of the gunship dust left inner.</summary>
    GunshipDustLeftInnerFifth,
    /// <summary>The sixth frame of the gunship dust left inner.</summary>
    GunshipDustLeftInnerSixth,
    /// <summary>The seventh frame of the gunship dust left inner.</summary>
    GunshipDustLeftInnerSeventh,
    /// <summary>The eighth frame of the gunship dust left inner.</summary>
    GunshipDustLeftInnerEighth,
    /// <summary>The first frame of the gunship dust left middle.</summary>
    GunshipDustLeftMiddleFirst,
    /// <summary>The second frame of the gunship dust left middle.</summary>
    GunshipDustLeftMiddleSecond,
    /// <summary>The third frame of the gunship dust left middle.</summary>
    GunshipDustLeftMiddleThird,
    /// <summary>The fourth frame of the gunship dust left middle.</summary>
    GunshipDustLeftMiddleFourth,
    /// <summary>The fifth frame of the gunship dust left middle.</summary>
    GunshipDustLeftMiddleFifth,
    /// <summary>The sixth frame of the gunship dust left middle.</summary>
    GunshipDustLeftMiddleSixth,
    /// <summary>The seventh frame of the gunship dust left middle.</summary>
    GunshipDustLeftMiddleSeventh,
    /// <summary>The eighth frame of the gunship dust left middle.</summary>
    GunshipDustLeftMiddleEighth,
    /// <summary>The first frame of the gunship dust left outer.</summary>
    GunshipDustLeftOuterFirst,
    /// <summary>The second frame of the gunship dust left outer.</summary>
    GunshipDustLeftOuterSecond,
    /// <summary>The third frame of the gunship dust left outer.</summary>
    GunshipDustLeftOuterThird,
    /// <summary>The fourth frame of the gunship dust left outer.</summary>
    GunshipDustLeftOuterFourth,
    /// <summary>The fifth frame of the gunship dust left outer.</summary>
    GunshipDustLeftOuterFifth,
    /// <summary>The sixth frame of the gunship dust left outer.</summary>
    GunshipDustLeftOuterSixth,
    /// <summary>The seventh frame of the gunship dust left outer.</summary>
    GunshipDustLeftOuterSeventh,
    /// <summary>The first frame of the torizo explosion low health initial loop.</summary>
    TorizoExplosionLowHealthInitialLoopFirst,
    /// <summary>The second frame of the Torizo explosion low health initial loop.</summary>
    TorizoExplosionLowHealthInitialLoopSecond,
    /// <summary>The third frame of the Torizo explosion low health initial loop.</summary>
    TorizoExplosionLowHealthInitialLoopThird,
    /// <summary>The fourth frame of the Torizo explosion low health initial loop.</summary>
    TorizoExplosionLowHealthInitialLoopFourth,
    /// <summary>The fifth frame of the Torizo explosion low health initial loop.</summary>
    TorizoExplosionLowHealthInitialLoopFifth,
    /// <summary>The first frame of the Torizo explosion death initial loop.</summary>
    TorizoExplosionDeathInitialLoopFirst,
    /// <summary>The second frame of the Torizo explosion death initial loop.</summary>
    TorizoExplosionDeathInitialLoopSecond,
    /// <summary>The third frame of the Torizo explosion death initial loop.</summary>
    TorizoExplosionDeathInitialLoopThird,
    /// <summary>The fourth frame of the Torizo explosion death initial loop.</summary>
    TorizoExplosionDeathInitialLoopFourth,
    /// <summary>The fifth frame of the Torizo explosion death initial loop.</summary>
    TorizoExplosionDeathInitialLoopFifth,
    /// <summary>The sixth frame of the Torizo explosion death initial loop.</summary>
    TorizoExplosionDeathInitialLoopSixth,
    /// <summary>The first frame of the Torizo explosion death smoke setup loop.</summary>
    TorizoExplosionDeathSmokeSetupLoopFirst,
    /// <summary>The second frame of the Torizo explosion death smoke setup loop.</summary>
    TorizoExplosionDeathSmokeSetupLoopSecond,
    /// <summary>The third frame of the Torizo explosion death smoke setup loop.</summary>
    TorizoExplosionDeathSmokeSetupLoopThird,
    /// <summary>The fourth frame of the Torizo explosion death smoke setup loop.</summary>
    TorizoExplosionDeathSmokeSetupLoopFourth,
    /// <summary>The frame of the Torizo drool four frame delay.</summary>
    TorizoDroolFourFrameDelay,
    /// <summary>The frame of the Torizo drool two frame delay.</summary>
    TorizoDroolTwoFrameDelay,
    /// <summary>The frame of the Torizo drool no delay.</summary>
    TorizoDroolNoDelay,
    /// <summary>The frame of the Torizo drool falling loop.</summary>
    TorizoDroolFallingLoop,
    /// <summary>The first frame of the Torizo drool floor impact.</summary>
    TorizoDroolFloorImpactFirst,
    /// <summary>The second frame of the Torizo drool floor impact.</summary>
    TorizoDroolFloorImpactSecond,
    /// <summary>The third frame of the Torizo drool floor impact.</summary>
    TorizoDroolFloorImpactThird,
    /// <summary>The first frame of the Torizo explosive swipe initial.</summary>
    TorizoExplosiveSwipeInitialFirst,
    /// <summary>The second frame of the Torizo explosive swipe initial.</summary>
    TorizoExplosiveSwipeInitialSecond,
    /// <summary>The third frame of the Torizo explosive swipe initial.</summary>
    TorizoExplosiveSwipeInitialThird,
    /// <summary>The fourth frame of the Torizo explosive swipe initial.</summary>
    TorizoExplosiveSwipeInitialFourth,
    /// <summary>The fifth frame of the Torizo explosive swipe initial.</summary>
    TorizoExplosiveSwipeInitialFifth,
    /// <summary>The first frame of the Bomb Torizo first statue fragment.</summary>
    BombTorizoFirstStatueFragmentFirst,
    /// <summary>The second frame of the Bomb Torizo first statue fragment.</summary>
    BombTorizoFirstStatueFragmentSecond,
    /// <summary>The first frame of the Bomb Torizo second statue fragment.</summary>
    BombTorizoSecondStatueFragmentFirst,
    /// <summary>The second frame of the Bomb Torizo second statue fragment.</summary>
    BombTorizoSecondStatueFragmentSecond,
    /// <summary>The first frame of the Bomb Torizo third statue fragment.</summary>
    BombTorizoThirdStatueFragmentFirst,
    /// <summary>The second frame of the Bomb Torizo third statue fragment.</summary>
    BombTorizoThirdStatueFragmentSecond,
    /// <summary>The first frame of the Bomb Torizo fourth statue fragment.</summary>
    BombTorizoFourthStatueFragmentFirst,
    /// <summary>The second frame of the Bomb Torizo fourth statue fragment.</summary>
    BombTorizoFourthStatueFragmentSecond,
    /// <summary>The first frame of the Bomb Torizo fifth statue fragment.</summary>
    BombTorizoFifthStatueFragmentFirst,
    /// <summary>The second frame of the Bomb Torizo fifth statue fragment.</summary>
    BombTorizoFifthStatueFragmentSecond,
    /// <summary>The first frame of the Bomb Torizo sixth statue fragment.</summary>
    BombTorizoSixthStatueFragmentFirst,
    /// <summary>The second frame of the Bomb Torizo sixth statue fragment.</summary>
    BombTorizoSixthStatueFragmentSecond,
    /// <summary>The first frame of the Bomb Torizo seventh statue fragment.</summary>
    BombTorizoSeventhStatueFragmentFirst,
    /// <summary>The second frame of the Bomb Torizo seventh statue fragment.</summary>
    BombTorizoSeventhStatueFragmentSecond,
    /// <summary>The first frame of the Bomb Torizo eighth statue fragment.</summary>
    BombTorizoEighthStatueFragmentFirst,
    /// <summary>The second frame of the Bomb Torizo eighth statue fragment.</summary>
    BombTorizoEighthStatueFragmentSecond,
    /// <summary>The frame of the Torizo Chozo orb moving left.</summary>
    TorizoChozoOrbMovingLeft,
    /// <summary>The frame of the Torizo Chozo orb moving right.</summary>
    TorizoChozoOrbMovingRight,
    /// <summary>The first frame of the Torizo Chozo orb wall impact.</summary>
    TorizoChozoOrbWallImpactFirst,
    /// <summary>The second frame of the Torizo Chozo orb wall impact.</summary>
    TorizoChozoOrbWallImpactSecond,
    /// <summary>The third frame of the Torizo Chozo orb wall impact.</summary>
    TorizoChozoOrbWallImpactThird,
    /// <summary>The fourth frame of the Torizo Chozo orb wall impact.</summary>
    TorizoChozoOrbWallImpactFourth,
    /// <summary>The fifth frame of the Torizo Chozo orb wall impact.</summary>
    TorizoChozoOrbWallImpactFifth,
    /// <summary>The first frame of the Torizo Chozo orb floor impact.</summary>
    TorizoChozoOrbFloorImpactFirst,
    /// <summary>The second frame of the Torizo Chozo orb floor impact.</summary>
    TorizoChozoOrbFloorImpactSecond,
    /// <summary>The third frame of the Torizo Chozo orb floor impact.</summary>
    TorizoChozoOrbFloorImpactThird,
    /// <summary>The fourth frame of the Torizo Chozo orb floor impact.</summary>
    TorizoChozoOrbFloorImpactFourth,
    /// <summary>The fifth frame of the Torizo Chozo orb floor impact.</summary>
    TorizoChozoOrbFloorImpactFifth,
    /// <summary>The sixth frame of the Torizo Chozo orb floor impact.</summary>
    TorizoChozoOrbFloorImpactSixth,
    /// <summary>The first frame of the Torizo Chozo orb shot.</summary>
    TorizoChozoOrbShotFirst,
    /// <summary>The second frame of the Torizo Chozo orb shot.</summary>
    TorizoChozoOrbShotSecond,
    /// <summary>The third frame of the Torizo Chozo orb shot.</summary>
    TorizoChozoOrbShotThird,
    /// <summary>The fourth frame of the Torizo Chozo orb shot.</summary>
    TorizoChozoOrbShotFourth,
    /// <summary>The fifth frame of the Torizo Chozo orb shot.</summary>
    TorizoChozoOrbShotFifth,
    /// <summary>The first frame of the Torizo sonic boom fired left.</summary>
    TorizoSonicBoomFiredLeftFirst,
    /// <summary>The second frame of the Torizo sonic boom fired left.</summary>
    TorizoSonicBoomFiredLeftSecond,
    /// <summary>The frame of the Torizo sonic boom moving left.</summary>
    TorizoSonicBoomMovingLeft,
    /// <summary>The first frame of the Torizo sonic boom fired right.</summary>
    TorizoSonicBoomFiredRightFirst,
    /// <summary>The second frame of the Torizo sonic boom fired right.</summary>
    TorizoSonicBoomFiredRightSecond,
    /// <summary>The frame of the Torizo sonic boom moving right.</summary>
    TorizoSonicBoomMovingRight,
    /// <summary>The first frame of the Torizo sonic boom wall impact loop.</summary>
    TorizoSonicBoomWallImpactLoopFirst,
    /// <summary>The second frame of the Torizo sonic boom wall impact loop.</summary>
    TorizoSonicBoomWallImpactLoopSecond,
    /// <summary>The third frame of the Torizo sonic boom wall impact loop.</summary>
    TorizoSonicBoomWallImpactLoopThird,
    /// <summary>The fourth frame of the Torizo sonic boom wall impact loop.</summary>
    TorizoSonicBoomWallImpactLoopFourth,
    /// <summary>The fifth frame of the Torizo sonic boom wall impact loop.</summary>
    TorizoSonicBoomWallImpactLoopFifth,
    /// <summary>The first frame of the Chozo Tourian dust footsteps.</summary>
    ChozoTourianDustFootstepsFirst,
    /// <summary>The second frame of the Chozo Tourian dust footsteps.</summary>
    ChozoTourianDustFootstepsSecond,
    /// <summary>The third frame of the Chozo Tourian dust footsteps.</summary>
    ChozoTourianDustFootstepsThird,
    /// <summary>The fourth frame of the Chozo Tourian dust footsteps.</summary>
    ChozoTourianDustFootstepsFourth,
    /// <summary>The first frame of the Chozo Tourian dust Tourian descent dust.</summary>
    ChozoTourianDustTourianDescentDustFirst,
    /// <summary>The second frame of the Chozo Tourian dust Tourian descent dust.</summary>
    ChozoTourianDustTourianDescentDustSecond,
    /// <summary>The third frame of the Chozo Tourian dust Tourian descent dust.</summary>
    ChozoTourianDustTourianDescentDustThird,
    /// <summary>The fourth frame of the Chozo Tourian dust Tourian descent dust.</summary>
    ChozoTourianDustTourianDescentDustFourth,
    /// <summary>The first frame of the Torizo landing dust right foot.</summary>
    TorizoLandingDustRightFootFirst,
    /// <summary>The second frame of the Torizo landing dust right foot.</summary>
    TorizoLandingDustRightFootSecond,
    /// <summary>The third frame of the Torizo landing dust right foot.</summary>
    TorizoLandingDustRightFootThird,
    /// <summary>The fourth frame of the Torizo landing dust right foot.</summary>
    TorizoLandingDustRightFootFourth,
    /// <summary>The first frame of the Torizo landing dust left foot.</summary>
    TorizoLandingDustLeftFootFirst,
    /// <summary>The second frame of the Torizo landing dust left foot.</summary>
    TorizoLandingDustLeftFootSecond,
    /// <summary>The third frame of the Torizo landing dust left foot.</summary>
    TorizoLandingDustLeftFootThird,
    /// <summary>The fourth frame of the Torizo landing dust left foot.</summary>
    TorizoLandingDustLeftFootFourth,
    /// <summary>The frame of the Golden Torizo egg bouncing left.</summary>
    GoldenTorizoEggBouncingLeft,
    /// <summary>The first frame of the Golden Torizo egg bouncing left hatching.</summary>
    GoldenTorizoEggBouncingLeftHatchingFirst,
    /// <summary>The second frame of the Golden Torizo egg bouncing left hatching.</summary>
    GoldenTorizoEggBouncingLeftHatchingSecond,
    /// <summary>The third frame of the Golden Torizo egg bouncing left hatching.</summary>
    GoldenTorizoEggBouncingLeftHatchingThird,
    /// <summary>The frame of the Golden Torizo egg bouncing right.</summary>
    GoldenTorizoEggBouncingRight,
    /// <summary>The first frame of the Golden Torizo egg bouncing right hatching.</summary>
    GoldenTorizoEggBouncingRightHatchingFirst,
    /// <summary>The second frame of the Golden Torizo egg bouncing right hatching.</summary>
    GoldenTorizoEggBouncingRightHatchingSecond,
    /// <summary>The third frame of the Golden Torizo egg bouncing right hatching.</summary>
    GoldenTorizoEggBouncingRightHatchingThird,
    /// <summary>The first frame of the Golden Torizo egg hatched left loop.</summary>
    GoldenTorizoEggHatchedLeftLoopFirst,
    /// <summary>The second frame of the Golden Torizo egg hatched left loop.</summary>
    GoldenTorizoEggHatchedLeftLoopSecond,
    /// <summary>The third frame of the Golden Torizo egg hatched left loop.</summary>
    GoldenTorizoEggHatchedLeftLoopThird,
    /// <summary>The fourth frame of the Golden Torizo egg hatched left loop.</summary>
    GoldenTorizoEggHatchedLeftLoopFourth,
    /// <summary>The first frame of the Golden Torizo egg hatched right loop.</summary>
    GoldenTorizoEggHatchedRightLoopFirst,
    /// <summary>The second frame of the Golden Torizo egg hatched right loop.</summary>
    GoldenTorizoEggHatchedRightLoopSecond,
    /// <summary>The third frame of the Golden Torizo egg hatched right loop.</summary>
    GoldenTorizoEggHatchedRightLoopThird,
    /// <summary>The fourth frame of the Golden Torizo egg hatched right loop.</summary>
    GoldenTorizoEggHatchedRightLoopFourth,
    /// <summary>The first frame of the Golden Torizo egg break left.</summary>
    GoldenTorizoEggBreakLeftFirst,
    /// <summary>The second frame of the Golden Torizo egg break left.</summary>
    GoldenTorizoEggBreakLeftSecond,
    /// <summary>The third frame of the Golden Torizo egg break left.</summary>
    GoldenTorizoEggBreakLeftThird,
    /// <summary>The fourth frame of the Golden Torizo egg break left.</summary>
    GoldenTorizoEggBreakLeftFourth,
    /// <summary>The fifth frame of the Golden Torizo egg break left.</summary>
    GoldenTorizoEggBreakLeftFifth,
    /// <summary>The first frame of the Golden Torizo egg break right.</summary>
    GoldenTorizoEggBreakRightFirst,
    /// <summary>The second frame of the Golden Torizo egg break right.</summary>
    GoldenTorizoEggBreakRightSecond,
    /// <summary>The third frame of the Golden Torizo egg break right.</summary>
    GoldenTorizoEggBreakRightThird,
    /// <summary>The fourth frame of the Golden Torizo egg break right.</summary>
    GoldenTorizoEggBreakRightFourth,
    /// <summary>The fifth frame of the Golden Torizo egg break right.</summary>
    GoldenTorizoEggBreakRightFifth,
    /// <summary>The frame of the Golden Torizo Super Missile right initial.</summary>
    GoldenTorizoSuperMissileRightInitial,
    /// <summary>The first frame of the Golden Torizo Super Missile right loop.</summary>
    GoldenTorizoSuperMissileRightLoopFirst,
    /// <summary>The second frame of the Golden Torizo Super Missile right loop.</summary>
    GoldenTorizoSuperMissileRightLoopSecond,
    /// <summary>The third frame of the Golden Torizo Super Missile right loop.</summary>
    GoldenTorizoSuperMissileRightLoopThird,
    /// <summary>The fourth frame of the Golden Torizo Super Missile right loop.</summary>
    GoldenTorizoSuperMissileRightLoopFourth,
    /// <summary>The fifth frame of the Golden Torizo Super Missile right loop.</summary>
    GoldenTorizoSuperMissileRightLoopFifth,
    /// <summary>The sixth frame of the Golden Torizo Super Missile right loop.</summary>
    GoldenTorizoSuperMissileRightLoopSixth,
    /// <summary>The seventh frame of the Golden Torizo Super Missile right loop.</summary>
    GoldenTorizoSuperMissileRightLoopSeventh,
    /// <summary>The eighth frame of the Golden Torizo Super Missile right loop.</summary>
    GoldenTorizoSuperMissileRightLoopEighth,
    /// <summary>The frame of the Golden Torizo Super Missile left initial.</summary>
    GoldenTorizoSuperMissileLeftInitial,
    /// <summary>The first frame of the Golden Torizo Super Missile left loop.</summary>
    GoldenTorizoSuperMissileLeftLoopFirst,
    /// <summary>The second frame of the Golden Torizo Super Missile left loop.</summary>
    GoldenTorizoSuperMissileLeftLoopSecond,
    /// <summary>The third frame of the Golden Torizo Super Missile left loop.</summary>
    GoldenTorizoSuperMissileLeftLoopThird,
    /// <summary>The fourth frame of the Golden Torizo Super Missile left loop.</summary>
    GoldenTorizoSuperMissileLeftLoopFourth,
    /// <summary>The fifth frame of the Golden Torizo Super Missile left loop.</summary>
    GoldenTorizoSuperMissileLeftLoopFifth,
    /// <summary>The sixth frame of the Golden Torizo Super Missile left loop.</summary>
    GoldenTorizoSuperMissileLeftLoopSixth,
    /// <summary>The seventh frame of the Golden Torizo Super Missile left loop.</summary>
    GoldenTorizoSuperMissileLeftLoopSeventh,
    /// <summary>The eighth frame of the Golden Torizo Super Missile left loop.</summary>
    GoldenTorizoSuperMissileLeftLoopEighth,
    /// <summary>The first frame of the Golden Torizo Super Missile impact.</summary>
    GoldenTorizoSuperMissileImpactFirst,
    /// <summary>The second frame of the Golden Torizo Super Missile impact.</summary>
    GoldenTorizoSuperMissileImpactSecond,
    /// <summary>The third frame of the Golden Torizo Super Missile impact.</summary>
    GoldenTorizoSuperMissileImpactThird,
    /// <summary>The fourth frame of the Golden Torizo Super Missile impact.</summary>
    GoldenTorizoSuperMissileImpactFourth,
    /// <summary>The fifth frame of the Golden Torizo Super Missile impact.</summary>
    GoldenTorizoSuperMissileImpactFifth,
    /// <summary>The sixth frame of the Golden Torizo Super Missile impact.</summary>
    GoldenTorizoSuperMissileImpactSixth,
    /// <summary>The first frame of the Golden Torizo eye beam wall impact.</summary>
    GoldenTorizoEyeBeamWallImpactFirst,
    /// <summary>The second frame of the Golden Torizo eye beam wall impact.</summary>
    GoldenTorizoEyeBeamWallImpactSecond,
    /// <summary>The third frame of the Golden Torizo eye beam wall impact.</summary>
    GoldenTorizoEyeBeamWallImpactThird,
    /// <summary>The fourth frame of the Golden Torizo eye beam wall impact.</summary>
    GoldenTorizoEyeBeamWallImpactFourth,
    /// <summary>The fifth frame of the Golden Torizo eye beam wall impact.</summary>
    GoldenTorizoEyeBeamWallImpactFifth,
    /// <summary>The first frame of the Golden Torizo eye beam floor impact loop.</summary>
    GoldenTorizoEyeBeamFloorImpactLoopFirst,
    /// <summary>The second frame of the Golden Torizo eye beam floor impact loop.</summary>
    GoldenTorizoEyeBeamFloorImpactLoopSecond,
    /// <summary>The third frame of the Golden Torizo eye beam floor impact loop.</summary>
    GoldenTorizoEyeBeamFloorImpactLoopThird,
    /// <summary>The fourth frame of the Golden Torizo eye beam floor impact loop.</summary>
    GoldenTorizoEyeBeamFloorImpactLoopFourth,
    /// <summary>The fifth frame of the Golden Torizo eye beam floor impact loop.</summary>
    GoldenTorizoEyeBeamFloorImpactLoopFifth,
    /// <summary>The sixth frame of the Golden Torizo eye beam floor impact loop.</summary>
    GoldenTorizoEyeBeamFloorImpactLoopSixth,
    /// <summary>The seventh frame of the Golden Torizo eye beam floor impact loop.</summary>
    GoldenTorizoEyeBeamFloorImpactLoopSeventh,
    /// <summary>The first frame of the Golden Torizo eye beam normal.</summary>
    GoldenTorizoEyeBeamNormalFirst,
    /// <summary>The second frame of the Golden Torizo eye beam normal.</summary>
    GoldenTorizoEyeBeamNormalSecond,
    /// <summary>The third frame of the Golden Torizo eye beam normal.</summary>
    GoldenTorizoEyeBeamNormalThird,
    /// <summary>The fourth frame of the Golden Torizo eye beam normal.</summary>
    GoldenTorizoEyeBeamNormalFourth,
    /// <summary>The fifth frame of the Golden Torizo eye beam normal.</summary>
    GoldenTorizoEyeBeamNormalFifth,
    /// <summary>The first frame of the old Tourian escape wall explosion.</summary>
    OldTourianEscapeWallExplosionFirst,
    /// <summary>The second frame of the old Tourian escape wall explosion.</summary>
    OldTourianEscapeWallExplosionSecond,
    /// <summary>The third frame of the old Tourian escape wall explosion.</summary>
    OldTourianEscapeWallExplosionThird,
    /// <summary>The fourth frame of the old Tourian escape wall explosion.</summary>
    OldTourianEscapeWallExplosionFourth,
    /// <summary>The fifth frame of the old Tourian escape wall explosion.</summary>
    OldTourianEscapeWallExplosionFifth,
    /// <summary>The sixth frame of the old Tourian escape wall explosion.</summary>
    OldTourianEscapeWallExplosionSixth,
    /// <summary>The first frame of the Dragon fireball rising left.</summary>
    DragonFireballRisingLeftFirst,
    /// <summary>The second frame of the Dragon fireball rising left.</summary>
    DragonFireballRisingLeftSecond,
    /// <summary>The first frame of the Dragon fireball rising right.</summary>
    DragonFireballRisingRightFirst,
    /// <summary>The second frame of the Dragon fireball rising right.</summary>
    DragonFireballRisingRightSecond,
    /// <summary>The first frame of the Dragon fireball falling left.</summary>
    DragonFireballFallingLeftFirst,
    /// <summary>The second frame of the Dragon fireball falling left.</summary>
    DragonFireballFallingLeftSecond,
    /// <summary>The first frame of the Dragon fireball falling right.</summary>
    DragonFireballFallingRightFirst,
    /// <summary>The second frame of the Dragon fireball falling right.</summary>
    DragonFireballFallingRightSecond,
    /// <summary>The first frame of the eye door initial.</summary>
    EyeDoorInitialFirst,
    /// <summary>The second frame of the eye door initial.</summary>
    EyeDoorInitialSecond,
    /// <summary>The third frame of the eye door initial.</summary>
    EyeDoorInitialThird,
    /// <summary>The frame of the eye door flying loop.</summary>
    EyeDoorFlyingLoop,
    /// <summary>The first frame of the eye door impact.</summary>
    EyeDoorImpactFirst,
    /// <summary>The second frame of the eye door impact.</summary>
    EyeDoorImpactSecond,
    /// <summary>The third frame of the eye door impact.</summary>
    EyeDoorImpactThird,
    /// <summary>The first frame of the eye door shot.</summary>
    EyeDoorShotFirst,
    /// <summary>The second frame of the eye door shot.</summary>
    EyeDoorShotSecond,
    /// <summary>The third frame of the eye door shot.</summary>
    EyeDoorShotThird,
    /// <summary>The fourth frame of the eye door shot.</summary>
    EyeDoorShotFourth,
    /// <summary>The frame of the eye door sweat falling.</summary>
    EyeDoorSweatFalling,
    /// <summary>The first frame of the eye door sweat hit floor.</summary>
    EyeDoorSweatHitFloorFirst,
    /// <summary>The second frame of the eye door sweat hit floor.</summary>
    EyeDoorSweatHitFloorSecond,
    /// <summary>The third frame of the eye door sweat hit floor.</summary>
    EyeDoorSweatHitFloorThird,
    /// <summary>The first frame of the Tourian statue splash.</summary>
    TourianStatueSplashFirst,
    /// <summary>The second frame of the Tourian statue splash.</summary>
    TourianStatueSplashSecond,
    /// <summary>The third frame of the Tourian statue splash.</summary>
    TourianStatueSplashThird,
    /// <summary>The fourth frame of the Tourian statue splash.</summary>
    TourianStatueSplashFourth,
    /// <summary>The first frame of the Tourian statue eye glow.</summary>
    TourianStatueEyeGlowFirst,
    /// <summary>The second frame of the Tourian statue eye glow.</summary>
    TourianStatueEyeGlowSecond,
    /// <summary>The third frame of the Tourian statue eye glow.</summary>
    TourianStatueEyeGlowThird,
    /// <summary>The fourth frame of the Tourian statue eye glow.</summary>
    TourianStatueEyeGlowFourth,
    /// <summary>The fifth frame of the Tourian statue eye glow.</summary>
    TourianStatueEyeGlowFifth,
    /// <summary>The sixth frame of the Tourian statue eye glow.</summary>
    TourianStatueEyeGlowSixth,
    /// <summary>The seventh frame of the Tourian statue eye glow.</summary>
    TourianStatueEyeGlowSeventh,
    /// <summary>The eighth frame of the Tourian statue eye glow.</summary>
    TourianStatueEyeGlowEighth,
    /// <summary>The ninth frame of the Tourian statue eye glow.</summary>
    TourianStatueEyeGlowNinth,
    /// <summary>The tenth frame of the Tourian statue eye glow.</summary>
    TourianStatueEyeGlowTenth,
    /// <summary>The first frame of the Tourian statue particle.</summary>
    TourianStatueParticleFirst,
    /// <summary>The second frame of the Tourian statue particle.</summary>
    TourianStatueParticleSecond,
    /// <summary>The third frame of the Tourian statue particle.</summary>
    TourianStatueParticleThird,
    /// <summary>The fourth frame of the Tourian statue particle.</summary>
    TourianStatueParticleFourth,
    /// <summary>The first frame of the Tourian statue tail.</summary>
    TourianStatueTailFirst,
    /// <summary>The second frame of the Tourian statue tail.</summary>
    TourianStatueTailSecond,
    /// <summary>The third frame of the Tourian statue tail.</summary>
    TourianStatueTailThird,
    /// <summary>The fourth frame of the Tourian statue tail.</summary>
    TourianStatueTailFourth,
    /// <summary>The first frame of the Tourian statue soul.</summary>
    TourianStatueSoulFirst,
    /// <summary>The second frame of the Tourian statue soul.</summary>
    TourianStatueSoulSecond,
    /// <summary>The frame of the Tourian statue base decoration.</summary>
    TourianStatueBaseDecoration,
    /// <summary>The frame of the Tourian statue base decoration loop.</summary>
    TourianStatueBaseDecorationLoop,
    /// <summary>The frame of the Tourian statue Ridley.</summary>
    TourianStatueRidley,
    /// <summary>The frame of the Tourian statue Phantoon.</summary>
    TourianStatuePhantoon,
    /// <summary>The first frame of the Nuclear Waffle initial.</summary>
    NuclearWaffleInitialFirst,
    /// <summary>The second frame of the Nuclear Waffle initial.</summary>
    NuclearWaffleInitialSecond,
    /// <summary>The third frame of the Nuclear Waffle initial.</summary>
    NuclearWaffleInitialThird,
    /// <summary>The fourth frame of the Nuclear Waffle initial.</summary>
    NuclearWaffleInitialFourth,
    /// <summary>The fifth frame of the Nuclear Waffle initial.</summary>
    NuclearWaffleInitialFifth,
    /// <summary>The sixth frame of the Nuclear Waffle initial.</summary>
    NuclearWaffleInitialSixth,
    /// <summary>The seventh frame of the Nuclear Waffle initial.</summary>
    NuclearWaffleInitialSeventh,
    /// <summary>The eighth frame of the Nuclear Waffle initial.</summary>
    NuclearWaffleInitialEighth,
    /// <summary>The ninth frame of the Nuclear Waffle initial.</summary>
    NuclearWaffleInitialNinth,
    /// <summary>The tenth frame of the Nuclear Waffle initial.</summary>
    NuclearWaffleInitialTenth,
    /// <summary>The eleventh frame of the Nuclear Waffle initial.</summary>
    NuclearWaffleInitialEleventh,
    /// <summary>The twelfth frame of the Nuclear Waffle initial.</summary>
    NuclearWaffleInitialTwelfth,
    /// <summary>The frame of the Polyp rock initial.</summary>
    PolypRockInitial,
    /// <summary>The first frame of the Shaktool front.</summary>
    ShaktoolFrontFirst,
    /// <summary>The second frame of the Shaktool front.</summary>
    ShaktoolFrontSecond,
    /// <summary>The frame of the Shaktool front loop.</summary>
    ShaktoolFrontLoop,
    /// <summary>The first frame of the Shaktool middle.</summary>
    ShaktoolMiddleFirst,
    /// <summary>The second frame of the Shaktool middle.</summary>
    ShaktoolMiddleSecond,
    /// <summary>The frame of the Shaktool middle loop.</summary>
    ShaktoolMiddleLoop,
    /// <summary>The frame of the Shaktool back.</summary>
    ShaktoolBack,
    /// <summary>The frame of the Shaktool back loop.</summary>
    ShaktoolBackLoop,
    /// <summary>The first frame of the Mother Brain turret pose.</summary>
    MotherBrainTurretPoseFirst,
    /// <summary>The second frame of the Mother Brain turret pose.</summary>
    MotherBrainTurretPoseSecond,
    /// <summary>The third frame of the Mother Brain turret pose.</summary>
    MotherBrainTurretPoseThird,
    /// <summary>The fourth frame of the Mother Brain turret pose.</summary>
    MotherBrainTurretPoseFourth,
    /// <summary>The fifth frame of the Mother Brain turret pose.</summary>
    MotherBrainTurretPoseFifth,
    /// <summary>The sixth frame of the Mother Brain turret pose.</summary>
    MotherBrainTurretPoseSixth,
    /// <summary>The seventh frame of the Mother Brain turret pose.</summary>
    MotherBrainTurretPoseSeventh,
    /// <summary>The eighth frame of the Mother Brain turret pose.</summary>
    MotherBrainTurretPoseEighth,
    /// <summary>The first frame of the Mother Brain turret bullet pose.</summary>
    MotherBrainTurretBulletPoseFirst,
    /// <summary>The second frame of the Mother Brain turret bullet pose.</summary>
    MotherBrainTurretBulletPoseSecond,
    /// <summary>The third frame of the Mother Brain turret bullet pose.</summary>
    MotherBrainTurretBulletPoseThird,
    /// <summary>The fourth frame of the Mother Brain turret bullet pose.</summary>
    MotherBrainTurretBulletPoseFourth,
    /// <summary>The fifth frame of the Mother Brain turret bullet pose.</summary>
    MotherBrainTurretBulletPoseFifth,
    /// <summary>The sixth frame of the Mother Brain turret bullet pose.</summary>
    MotherBrainTurretBulletPoseSixth,
    /// <summary>The seventh frame of the Mother Brain turret bullet pose.</summary>
    MotherBrainTurretBulletPoseSeventh,
    /// <summary>The eighth frame of the Mother Brain turret bullet pose.</summary>
    MotherBrainTurretBulletPoseEighth,
    /// <summary>The first frame of the Mother Brain turret bullet impact.</summary>
    MotherBrainTurretBulletImpactFirst,
    /// <summary>The second frame of the Mother Brain turret bullet impact.</summary>
    MotherBrainTurretBulletImpactSecond,
    /// <summary>The third frame of the Mother Brain turret bullet impact.</summary>
    MotherBrainTurretBulletImpactThird,
    /// <summary>The fourth frame of the Mother Brain turret bullet impact.</summary>
    MotherBrainTurretBulletImpactFourth,
    /// <summary>The fifth frame of the Mother Brain turret bullet impact.</summary>
    MotherBrainTurretBulletImpactFifth,
    /// <summary>The first frame of the Mother Brain blue ring onion ring.</summary>
    MotherBrainBlueRingOnionRingFirst,
    /// <summary>The second frame of the Mother Brain blue ring onion ring.</summary>
    MotherBrainBlueRingOnionRingSecond,
    /// <summary>The third frame of the Mother Brain blue ring onion ring.</summary>
    MotherBrainBlueRingOnionRingThird,
    /// <summary>The fourth frame of the Mother Brain blue ring onion ring.</summary>
    MotherBrainBlueRingOnionRingFourth,
    /// <summary>The fifth frame of the Mother Brain blue ring onion ring.</summary>
    MotherBrainBlueRingOnionRingFifth,
    /// <summary>The sixth frame of the Mother Brain blue ring onion ring.</summary>
    MotherBrainBlueRingOnionRingSixth,
    /// <summary>The first frame of the Mother Brain blue ring impact onion ring hit.</summary>
    MotherBrainBlueRingImpactOnionRingHitFirst,
    /// <summary>The second frame of the Mother Brain blue ring impact onion ring hit.</summary>
    MotherBrainBlueRingImpactOnionRingHitSecond,
    /// <summary>The third frame of the Mother Brain blue ring impact onion ring hit.</summary>
    MotherBrainBlueRingImpactOnionRingHitThird,
    /// <summary>The fourth frame of the Mother Brain blue ring impact onion ring hit.</summary>
    MotherBrainBlueRingImpactOnionRingHitFourth,
    /// <summary>The fifth frame of the Mother Brain blue ring impact onion ring hit.</summary>
    MotherBrainBlueRingImpactOnionRingHitFifth,
    /// <summary>The sixth frame of the Mother Brain blue ring impact onion ring hit.</summary>
    MotherBrainBlueRingImpactOnionRingHitSixth,
    /// <summary>The first frame of the Mother Brain Bomb.</summary>
    MotherBrainBombFirst,
    /// <summary>The second frame of the Mother Brain Bomb.</summary>
    MotherBrainBombSecond,
    /// <summary>The third frame of the Mother Brain Bomb.</summary>
    MotherBrainBombThird,
    /// <summary>The fourth frame of the Mother Brain Bomb.</summary>
    MotherBrainBombFourth,
    /// <summary>The fifth frame of the Mother Brain Bomb.</summary>
    MotherBrainBombFifth,
    /// <summary>The sixth frame of the Mother Brain Bomb.</summary>
    MotherBrainBombSixth,
    /// <summary>The seventh frame of the Mother Brain Bomb.</summary>
    MotherBrainBombSeventh,
    /// <summary>The eighth frame of the Mother Brain Bomb.</summary>
    MotherBrainBombEighth,
    /// <summary>The ninth frame of the Mother Brain Bomb.</summary>
    MotherBrainBombNinth,
    /// <summary>The frame of the Mother Brain hand beam first stage first phase.</summary>
    MotherBrainHandBeamFirstStageFirstPhase,
    /// <summary>The frame of the Mother Brain hand beam first stage second phase.</summary>
    MotherBrainHandBeamFirstStageSecondPhase,
    /// <summary>The frame of the Mother Brain hand beam first stage third phase.</summary>
    MotherBrainHandBeamFirstStageThirdPhase,
    /// <summary>The frame of the Mother Brain hand beam first stage fourth phase.</summary>
    MotherBrainHandBeamFirstStageFourthPhase,
    /// <summary>The frame of the Mother Brain hand beam first stage fifth phase.</summary>
    MotherBrainHandBeamFirstStageFifthPhase,
    /// <summary>The frame of the Mother Brain hand beam first stage sixth phase.</summary>
    MotherBrainHandBeamFirstStageSixthPhase,
    /// <summary>The frame of the Mother Brain hand beam first stage seventh phase.</summary>
    MotherBrainHandBeamFirstStageSeventhPhase,
    /// <summary>The frame of the Mother Brain hand beam second stage first phase.</summary>
    MotherBrainHandBeamSecondStageFirstPhase,
    /// <summary>The frame of the Mother Brain hand beam second stage second phase.</summary>
    MotherBrainHandBeamSecondStageSecondPhase,
    /// <summary>The frame of the Mother Brain hand beam second stage third phase.</summary>
    MotherBrainHandBeamSecondStageThirdPhase,
    /// <summary>The frame of the Mother Brain hand beam second stage fourth phase.</summary>
    MotherBrainHandBeamSecondStageFourthPhase,
    /// <summary>The frame of the Mother Brain hand beam second stage fifth phase.</summary>
    MotherBrainHandBeamSecondStageFifthPhase,
    /// <summary>The frame of the Mother Brain hand beam second stage sixth phase.</summary>
    MotherBrainHandBeamSecondStageSixthPhase,
    /// <summary>The frame of the Mother Brain hand beam second stage seventh phase.</summary>
    MotherBrainHandBeamSecondStageSeventhPhase,
    /// <summary>The frame of the Mother Brain hand beam third stage first phase.</summary>
    MotherBrainHandBeamThirdStageFirstPhase,
    /// <summary>The frame of the Mother Brain hand beam third stage second phase.</summary>
    MotherBrainHandBeamThirdStageSecondPhase,
    /// <summary>The frame of the Mother Brain hand beam third stage third phase.</summary>
    MotherBrainHandBeamThirdStageThirdPhase,
    /// <summary>The frame of the Mother Brain hand beam third stage fourth phase.</summary>
    MotherBrainHandBeamThirdStageFourthPhase,
    /// <summary>The frame of the Mother Brain hand beam third stage fifth phase.</summary>
    MotherBrainHandBeamThirdStageFifthPhase,
    /// <summary>The frame of the Mother Brain hand beam third stage sixth phase.</summary>
    MotherBrainHandBeamThirdStageSixthPhase,
    /// <summary>The frame of the Mother Brain hand beam third stage seventh phase.</summary>
    MotherBrainHandBeamThirdStageSeventhPhase,
    /// <summary>The first frame of the Mother Brain rainbow beam charging.</summary>
    MotherBrainRainbowBeamChargingFirst,
    /// <summary>The second frame of the Mother Brain rainbow beam charging.</summary>
    MotherBrainRainbowBeamChargingSecond,
    /// <summary>The third frame of the Mother Brain rainbow beam charging.</summary>
    MotherBrainRainbowBeamChargingThird,
    /// <summary>The fourth frame of the Mother Brain rainbow beam charging.</summary>
    MotherBrainRainbowBeamChargingFourth,
    /// <summary>The fifth frame of the Mother Brain rainbow beam charging.</summary>
    MotherBrainRainbowBeamChargingFifth,
    /// <summary>The sixth frame of the Mother Brain rainbow beam charging.</summary>
    MotherBrainRainbowBeamChargingSixth,
    /// <summary>The first frame of the Mother Brain drool attached drool.</summary>
    MotherBrainDroolAttachedDroolFirst,
    /// <summary>The second frame of the Mother Brain drool attached drool.</summary>
    MotherBrainDroolAttachedDroolSecond,
    /// <summary>The third frame of the Mother Brain drool attached drool.</summary>
    MotherBrainDroolAttachedDroolThird,
    /// <summary>The fourth frame of the Mother Brain drool attached drool.</summary>
    MotherBrainDroolAttachedDroolFourth,
    /// <summary>The fifth frame of the Mother Brain drool attached drool.</summary>
    MotherBrainDroolAttachedDroolFifth,
    /// <summary>The frame of the Mother Brain drool released.</summary>
    MotherBrainDroolReleased,
    /// <summary>The first frame of the Mother Brain drool hit floor.</summary>
    MotherBrainDroolHitFloorFirst,
    /// <summary>The second frame of the Mother Brain drool hit floor.</summary>
    MotherBrainDroolHitFloorSecond,
    /// <summary>The third frame of the Mother Brain drool hit floor.</summary>
    MotherBrainDroolHitFloorThird,
    /// <summary>The fourth frame of the Mother Brain drool hit floor.</summary>
    MotherBrainDroolHitFloorFourth,
    /// <summary>The first frame of the Mother Brain escape door particle.</summary>
    MotherBrainEscapeDoorParticleFirst,
    /// <summary>The second frame of the Mother Brain escape door particle.</summary>
    MotherBrainEscapeDoorParticleSecond,
    /// <summary>The third frame of the Mother Brain escape door particle.</summary>
    MotherBrainEscapeDoorParticleThird,
    /// <summary>The fourth frame of the Mother Brain escape door particle.</summary>
    MotherBrainEscapeDoorParticleFourth,
    /// <summary>The fifth frame of the Mother Brain escape door particle.</summary>
    MotherBrainEscapeDoorParticleFifth,
    /// <summary>The sixth frame of the Mother Brain escape door particle.</summary>
    MotherBrainEscapeDoorParticleSixth,
    /// <summary>The seventh frame of the Mother Brain escape door particle.</summary>
    MotherBrainEscapeDoorParticleSeventh,
    /// <summary>The eighth frame of the Mother Brain escape door particle.</summary>
    MotherBrainEscapeDoorParticleEighth,
    /// <summary>The first frame of the Mother Brain purple breath big.</summary>
    MotherBrainPurpleBreathBigFirst,
    /// <summary>The second frame of the Mother Brain purple breath big.</summary>
    MotherBrainPurpleBreathBigSecond,
    /// <summary>The third frame of the Mother Brain purple breath big.</summary>
    MotherBrainPurpleBreathBigThird,
    /// <summary>The fourth frame of the Mother Brain purple breath big.</summary>
    MotherBrainPurpleBreathBigFourth,
    /// <summary>The fifth frame of the Mother Brain purple breath big.</summary>
    MotherBrainPurpleBreathBigFifth,
    /// <summary>The sixth frame of the Mother Brain purple breath big.</summary>
    MotherBrainPurpleBreathBigSixth,
    /// <summary>The seventh frame of the Mother Brain purple breath big.</summary>
    MotherBrainPurpleBreathBigSeventh,
    /// <summary>The eighth frame of the Mother Brain purple breath big.</summary>
    MotherBrainPurpleBreathBigEighth,
    /// <summary>The first frame of the Mother Brain purple breath small.</summary>
    MotherBrainPurpleBreathSmallFirst,
    /// <summary>The second frame of the Mother Brain purple breath small.</summary>
    MotherBrainPurpleBreathSmallSecond,
    /// <summary>The third frame of the Mother Brain purple breath small.</summary>
    MotherBrainPurpleBreathSmallThird,
    /// <summary>The fourth frame of the Mother Brain purple breath small.</summary>
    MotherBrainPurpleBreathSmallFourth,
    /// <summary>The fifth frame of the Mother Brain purple breath small.</summary>
    MotherBrainPurpleBreathSmallFifth,
    /// <summary>The sixth frame of the Mother Brain purple breath small.</summary>
    MotherBrainPurpleBreathSmallSixth,
    /// <summary>The seventh frame of the Mother Brain purple breath small.</summary>
    MotherBrainPurpleBreathSmallSeventh,
    /// <summary>The eighth frame of the Mother Brain purple breath small.</summary>
    MotherBrainPurpleBreathSmallEighth,
    /// <summary>The frame of the Mother Brain escape subtitle.</summary>
    MotherBrainEscapeSubtitle,
    /// <summary>The frame of the Mother Brain top tube top right.</summary>
    MotherBrainTopTubeTopRight,
    /// <summary>The frame of the Mother Brain top tube top left.</summary>
    MotherBrainTopTubeTopLeft,
    /// <summary>The frame of the Mother Brain top tube top middle left.</summary>
    MotherBrainTopTubeTopMiddleLeft,
    /// <summary>The frame of the Mother Brain top tube top middle right.</summary>
    MotherBrainTopTubeTopMiddleRight,
    /// <summary>The first frame of the Mother Brain glass shard flying up.</summary>
    MotherBrainGlassShardFlyingUpFirst,
    /// <summary>The second frame of the Mother Brain glass shard flying up.</summary>
    MotherBrainGlassShardFlyingUpSecond,
    /// <summary>The third frame of the Mother Brain glass shard flying up.</summary>
    MotherBrainGlassShardFlyingUpThird,
    /// <summary>The fourth frame of the Mother Brain glass shard flying up.</summary>
    MotherBrainGlassShardFlyingUpFourth,
    /// <summary>The fifth frame of the Mother Brain glass shard flying up.</summary>
    MotherBrainGlassShardFlyingUpFifth,
    /// <summary>The sixth frame of the Mother Brain glass shard flying up.</summary>
    MotherBrainGlassShardFlyingUpSixth,
    /// <summary>The seventh frame of the Mother Brain glass shard flying up.</summary>
    MotherBrainGlassShardFlyingUpSeventh,
    /// <summary>The eighth frame of the Mother Brain glass shard flying up.</summary>
    MotherBrainGlassShardFlyingUpEighth,
    /// <summary>The first frame of the Mother Brain glass shard flying up right.</summary>
    MotherBrainGlassShardFlyingUpRightFirst,
    /// <summary>The second frame of the Mother Brain glass shard flying up right.</summary>
    MotherBrainGlassShardFlyingUpRightSecond,
    /// <summary>The third frame of the Mother Brain glass shard flying up right.</summary>
    MotherBrainGlassShardFlyingUpRightThird,
    /// <summary>The fourth frame of the Mother Brain glass shard flying up right.</summary>
    MotherBrainGlassShardFlyingUpRightFourth,
    /// <summary>The fifth frame of the Mother Brain glass shard flying up right.</summary>
    MotherBrainGlassShardFlyingUpRightFifth,
    /// <summary>The sixth frame of the Mother Brain glass shard flying up right.</summary>
    MotherBrainGlassShardFlyingUpRightSixth,
    /// <summary>The seventh frame of the Mother Brain glass shard flying up right.</summary>
    MotherBrainGlassShardFlyingUpRightSeventh,
    /// <summary>The eighth frame of the Mother Brain glass shard flying up right.</summary>
    MotherBrainGlassShardFlyingUpRightEighth,
    /// <summary>The first frame of the Mother Brain glass shard flying right.</summary>
    MotherBrainGlassShardFlyingRightFirst,
    /// <summary>The second frame of the Mother Brain glass shard flying right.</summary>
    MotherBrainGlassShardFlyingRightSecond,
    /// <summary>The third frame of the Mother Brain glass shard flying right.</summary>
    MotherBrainGlassShardFlyingRightThird,
    /// <summary>The fourth frame of the Mother Brain glass shard flying right.</summary>
    MotherBrainGlassShardFlyingRightFourth,
    /// <summary>The fifth frame of the Mother Brain glass shard flying right.</summary>
    MotherBrainGlassShardFlyingRightFifth,
    /// <summary>The sixth frame of the Mother Brain glass shard flying right.</summary>
    MotherBrainGlassShardFlyingRightSixth,
    /// <summary>The seventh frame of the Mother Brain glass shard flying right.</summary>
    MotherBrainGlassShardFlyingRightSeventh,
    /// <summary>The eighth frame of the Mother Brain glass shard flying right.</summary>
    MotherBrainGlassShardFlyingRightEighth,
    /// <summary>The first frame of the Mother Brain glass shard flying down right.</summary>
    MotherBrainGlassShardFlyingDownRightFirst,
    /// <summary>The second frame of the Mother Brain glass shard flying down right.</summary>
    MotherBrainGlassShardFlyingDownRightSecond,
    /// <summary>The third frame of the Mother Brain glass shard flying down right.</summary>
    MotherBrainGlassShardFlyingDownRightThird,
    /// <summary>The fourth frame of the Mother Brain glass shard flying down right.</summary>
    MotherBrainGlassShardFlyingDownRightFourth,
    /// <summary>The fifth frame of the Mother Brain glass shard flying down right.</summary>
    MotherBrainGlassShardFlyingDownRightFifth,
    /// <summary>The sixth frame of the Mother Brain glass shard flying down right.</summary>
    MotherBrainGlassShardFlyingDownRightSixth,
    /// <summary>The seventh frame of the Mother Brain glass shard flying down right.</summary>
    MotherBrainGlassShardFlyingDownRightSeventh,
    /// <summary>The eighth frame of the Mother Brain glass shard flying down right.</summary>
    MotherBrainGlassShardFlyingDownRightEighth,
    /// <summary>The first frame of the Mother Brain glass shard flying down.</summary>
    MotherBrainGlassShardFlyingDownFirst,
    /// <summary>The second frame of the Mother Brain glass shard flying down.</summary>
    MotherBrainGlassShardFlyingDownSecond,
    /// <summary>The third frame of the Mother Brain glass shard flying down.</summary>
    MotherBrainGlassShardFlyingDownThird,
    /// <summary>The fourth frame of the Mother Brain glass shard flying down.</summary>
    MotherBrainGlassShardFlyingDownFourth,
    /// <summary>The fifth frame of the Mother Brain glass shard flying down.</summary>
    MotherBrainGlassShardFlyingDownFifth,
    /// <summary>The sixth frame of the Mother Brain glass shard flying down.</summary>
    MotherBrainGlassShardFlyingDownSixth,
    /// <summary>The seventh frame of the Mother Brain glass shard flying down.</summary>
    MotherBrainGlassShardFlyingDownSeventh,
    /// <summary>The eighth frame of the Mother Brain glass shard flying down.</summary>
    MotherBrainGlassShardFlyingDownEighth,
    /// <summary>The first frame of the Mother Brain glass shard flying down left.</summary>
    MotherBrainGlassShardFlyingDownLeftFirst,
    /// <summary>The second frame of the Mother Brain glass shard flying down left.</summary>
    MotherBrainGlassShardFlyingDownLeftSecond,
    /// <summary>The third frame of the Mother Brain glass shard flying down left.</summary>
    MotherBrainGlassShardFlyingDownLeftThird,
    /// <summary>The fourth frame of the Mother Brain glass shard flying down left.</summary>
    MotherBrainGlassShardFlyingDownLeftFourth,
    /// <summary>The fifth frame of the Mother Brain glass shard flying down left.</summary>
    MotherBrainGlassShardFlyingDownLeftFifth,
    /// <summary>The sixth frame of the Mother Brain glass shard flying down left.</summary>
    MotherBrainGlassShardFlyingDownLeftSixth,
    /// <summary>The seventh frame of the Mother Brain glass shard flying down left.</summary>
    MotherBrainGlassShardFlyingDownLeftSeventh,
    /// <summary>The eighth frame of the Mother Brain glass shard flying down left.</summary>
    MotherBrainGlassShardFlyingDownLeftEighth,
    /// <summary>The first frame of the Mother Brain glass shard flying left.</summary>
    MotherBrainGlassShardFlyingLeftFirst,
    /// <summary>The second frame of the Mother Brain glass shard flying left.</summary>
    MotherBrainGlassShardFlyingLeftSecond,
    /// <summary>The third frame of the Mother Brain glass shard flying left.</summary>
    MotherBrainGlassShardFlyingLeftThird,
    /// <summary>The fourth frame of the Mother Brain glass shard flying left.</summary>
    MotherBrainGlassShardFlyingLeftFourth,
    /// <summary>The fifth frame of the Mother Brain glass shard flying left.</summary>
    MotherBrainGlassShardFlyingLeftFifth,
    /// <summary>The sixth frame of the Mother Brain glass shard flying left.</summary>
    MotherBrainGlassShardFlyingLeftSixth,
    /// <summary>The seventh frame of the Mother Brain glass shard flying left.</summary>
    MotherBrainGlassShardFlyingLeftSeventh,
    /// <summary>The eighth frame of the Mother Brain glass shard flying left.</summary>
    MotherBrainGlassShardFlyingLeftEighth,
    /// <summary>The first frame of the Mother Brain glass shard flying up left.</summary>
    MotherBrainGlassShardFlyingUpLeftFirst,
    /// <summary>The second frame of the Mother Brain glass shard flying up left.</summary>
    MotherBrainGlassShardFlyingUpLeftSecond,
    /// <summary>The third frame of the Mother Brain glass shard flying up left.</summary>
    MotherBrainGlassShardFlyingUpLeftThird,
    /// <summary>The fourth frame of the Mother Brain glass shard flying up left.</summary>
    MotherBrainGlassShardFlyingUpLeftFourth,
    /// <summary>The fifth frame of the Mother Brain glass shard flying up left.</summary>
    MotherBrainGlassShardFlyingUpLeftFifth,
    /// <summary>The sixth frame of the Mother Brain glass shard flying up left.</summary>
    MotherBrainGlassShardFlyingUpLeftSixth,
    /// <summary>The seventh frame of the Mother Brain glass shard flying up left.</summary>
    MotherBrainGlassShardFlyingUpLeftSeventh,
    /// <summary>The eighth frame of the Mother Brain glass shard flying up left.</summary>
    MotherBrainGlassShardFlyingUpLeftEighth,
    /// <summary>The first frame of the Mother Brain glass sparkle.</summary>
    MotherBrainGlassSparkleFirst,
    /// <summary>The second frame of the Mother Brain glass sparkle.</summary>
    MotherBrainGlassSparkleSecond,
    /// <summary>The third frame of the Mother Brain glass sparkle.</summary>
    MotherBrainGlassSparkleThird,
    /// <summary>The fourth frame of the Mother Brain glass sparkle.</summary>
    MotherBrainGlassSparkleFourth,
    /// <summary>The first frame of the Ki-Hunter acid spit left.</summary>
    KiHunterAcidSpitLeftFirst,
    /// <summary>The second frame of the Ki-Hunter acid spit left.</summary>
    KiHunterAcidSpitLeftSecond,
    /// <summary>The third frame of the Ki-Hunter acid spit left.</summary>
    KiHunterAcidSpitLeftThird,
    /// <summary>The fourth frame of the Ki-Hunter acid spit left.</summary>
    KiHunterAcidSpitLeftFourth,
    /// <summary>The fifth frame of the Ki-Hunter acid spit left.</summary>
    KiHunterAcidSpitLeftFifth,
    /// <summary>The sixth frame of the Ki-Hunter acid spit left.</summary>
    KiHunterAcidSpitLeftSixth,
    /// <summary>The seventh frame of the Ki-Hunter acid spit left.</summary>
    KiHunterAcidSpitLeftSeventh,
    /// <summary>The first frame of the Ki-Hunter acid spit hit floor.</summary>
    KiHunterAcidSpitHitFloorFirst,
    /// <summary>The second frame of the Ki-Hunter acid spit hit floor.</summary>
    KiHunterAcidSpitHitFloorSecond,
    /// <summary>The third frame of the Ki-Hunter acid spit hit floor.</summary>
    KiHunterAcidSpitHitFloorThird,
    /// <summary>The fourth frame of the Ki-Hunter acid spit hit floor.</summary>
    KiHunterAcidSpitHitFloorFourth,
    /// <summary>The fifth frame of the Ki-Hunter acid spit hit floor.</summary>
    KiHunterAcidSpitHitFloorFifth,
    /// <summary>The first frame of the Ki-Hunter acid spit right.</summary>
    KiHunterAcidSpitRightFirst,
    /// <summary>The second frame of the Ki-Hunter acid spit right.</summary>
    KiHunterAcidSpitRightSecond,
    /// <summary>The third frame of the Ki-Hunter acid spit right.</summary>
    KiHunterAcidSpitRightThird,
    /// <summary>The fourth frame of the Ki-Hunter acid spit right.</summary>
    KiHunterAcidSpitRightFourth,
    /// <summary>The fifth frame of the Ki-Hunter acid spit right.</summary>
    KiHunterAcidSpitRightFifth,
    /// <summary>The sixth frame of the Ki-Hunter acid spit right.</summary>
    KiHunterAcidSpitRightSixth,
    /// <summary>The seventh frame of the Ki-Hunter acid spit right.</summary>
    KiHunterAcidSpitRightSeventh,
    /// <summary>The first frame of the Kago bug landed.</summary>
    KagoBugLandedFirst,
    /// <summary>The second frame of the Kago bug landed.</summary>
    KagoBugLandedSecond,
    /// <summary>The frame of the Kago bug falling.</summary>
    KagoBugFalling,
    /// <summary>The first frame of the Kago bug jump start.</summary>
    KagoBugJumpStartFirst,
    /// <summary>The second frame of the Kago bug jump start.</summary>
    KagoBugJumpStartSecond,
    /// <summary>The frame of the Kago bug jump loop.</summary>
    KagoBugJumpLoop,
    /// <summary>The first frame of the Kago bug shot.</summary>
    KagoBugShotFirst,
    /// <summary>The second frame of the Kago bug shot.</summary>
    KagoBugShotSecond,
    /// <summary>The third frame of the Kago bug shot.</summary>
    KagoBugShotThird,
    /// <summary>The fourth frame of the Kago bug shot.</summary>
    KagoBugShotFourth,
    /// <summary>The fifth frame of the Kago bug shot.</summary>
    KagoBugShotFifth,
    /// <summary>The first frame of the Powamp spike initial.</summary>
    PowampSpikeInitialFirst,
    /// <summary>The second frame of the Powamp spike initial.</summary>
    PowampSpikeInitialSecond,
    /// <summary>The third frame of the Powamp spike initial.</summary>
    PowampSpikeInitialThird,
    /// <summary>The first frame of the Work Robot laser initial.</summary>
    WorkRobotLaserInitialFirst,
    /// <summary>The second frame of the Work Robot laser initial.</summary>
    WorkRobotLaserInitialSecond,
    /// <summary>The third frame of the Work Robot laser initial.</summary>
    WorkRobotLaserInitialThird,
    /// <summary>The first frame of the Work Robot laser loop.</summary>
    WorkRobotLaserLoopFirst,
    /// <summary>The second frame of the Work Robot laser loop.</summary>
    WorkRobotLaserLoopSecond,
    /// <summary>The third frame of the Work Robot laser loop.</summary>
    WorkRobotLaserLoopThird,
    /// <summary>The fourth frame of the Work Robot laser loop.</summary>
    WorkRobotLaserLoopFourth,
    /// <summary>The first frame of the n00b tube crack spreading.</summary>
    NoobTubeCrackSpreadingFirst,
    /// <summary>The second frame of the n00b tube crack spreading.</summary>
    NoobTubeCrackSpreadingSecond,
    /// <summary>The third frame of the n00b tube crack spreading.</summary>
    NoobTubeCrackSpreadingThird,
    /// <summary>The fourth frame of the n00b tube crack spreading.</summary>
    NoobTubeCrackSpreadingFourth,
    /// <summary>The fifth frame of the n00b tube crack spreading.</summary>
    NoobTubeCrackSpreadingFifth,
    /// <summary>The sixth frame of the n00b tube crack spreading.</summary>
    NoobTubeCrackSpreadingSixth,
    /// <summary>The first frame of the n00b tube crack shattering.</summary>
    NoobTubeCrackShatteringFirst,
    /// <summary>The second frame of the n00b tube crack shattering.</summary>
    NoobTubeCrackShatteringSecond,
    /// <summary>The third frame of the n00b tube crack shattering.</summary>
    NoobTubeCrackShatteringThird,
    /// <summary>The fourth frame of the n00b tube crack shattering.</summary>
    NoobTubeCrackShatteringFourth,
    /// <summary>The fifth frame of the n00b tube crack shattering.</summary>
    NoobTubeCrackShatteringFifth,
    /// <summary>The sixth frame of the n00b tube crack shattering.</summary>
    NoobTubeCrackShatteringSixth,
    /// <summary>The seventh frame of the n00b tube crack shattering.</summary>
    NoobTubeCrackShatteringSeventh,
    /// <summary>The eighth frame of the n00b tube crack shattering.</summary>
    NoobTubeCrackShatteringEighth,
    /// <summary>The ninth frame of the n00b tube crack shattering.</summary>
    NoobTubeCrackShatteringNinth,
    /// <summary>The tenth frame of the n00b tube crack shattering.</summary>
    NoobTubeCrackShatteringTenth,
    /// <summary>The first frame of the n00b tube crack falling.</summary>
    NoobTubeCrackFallingFirst,
    /// <summary>The second frame of the n00b tube crack falling.</summary>
    NoobTubeCrackFallingSecond,
    /// <summary>The third frame of the n00b tube crack falling.</summary>
    NoobTubeCrackFallingThird,
    /// <summary>The fourth frame of the n00b tube crack falling.</summary>
    NoobTubeCrackFallingFourth,
    /// <summary>The fifth frame of the n00b tube crack falling.</summary>
    NoobTubeCrackFallingFifth,
    /// <summary>The sixth frame of the n00b tube crack falling.</summary>
    NoobTubeCrackFallingSixth,
    /// <summary>The seventh frame of the n00b tube crack falling.</summary>
    NoobTubeCrackFallingSeventh,
    /// <summary>The eighth frame of the n00b tube crack falling.</summary>
    NoobTubeCrackFallingEighth,
    /// <summary>The ninth frame of the n00b tube crack falling.</summary>
    NoobTubeCrackFallingNinth,
    /// <summary>The tenth frame of the n00b tube crack falling.</summary>
    NoobTubeCrackFallingTenth,
    /// <summary>The eleventh frame of the n00b tube crack falling.</summary>
    NoobTubeCrackFallingEleventh,
    /// <summary>The twelfth frame of the n00b tube crack falling.</summary>
    NoobTubeCrackFallingTwelfth,
    /// <summary>The thirteenth frame of the n00b tube crack falling.</summary>
    NoobTubeCrackFallingThirteenth,
    /// <summary>The fourteenth frame of the n00b tube crack falling.</summary>
    NoobTubeCrackFallingFourteenth,
    /// <summary>The fifteenth frame of the n00b tube crack falling.</summary>
    NoobTubeCrackFallingFifteenth,
    /// <summary>The sixteenth frame of the n00b tube crack falling.</summary>
    NoobTubeCrackFallingSixteenth,
    /// <summary>The seventeenth frame of the n00b tube crack falling.</summary>
    NoobTubeCrackFallingSeventeenth,
    /// <summary>The eighteenth frame of the n00b tube crack falling.</summary>
    NoobTubeCrackFallingEighteenth,
    /// <summary>The nineteenth frame of the n00b tube crack falling.</summary>
    NoobTubeCrackFallingNineteenth,
    /// <summary>The first frame of the n00b tube crack fallen.</summary>
    NoobTubeCrackFallenFirst,
    /// <summary>The second frame of the n00b tube crack fallen.</summary>
    NoobTubeCrackFallenSecond,
    /// <summary>The frame of the n00b tube shard down outward flying.</summary>
    NoobTubeShardDownOutwardFlying,
    /// <summary>The frame of the n00b tube shard down outward flying mirrored.</summary>
    NoobTubeShardDownOutwardFlyingMirrored,
    /// <summary>The frame of the n00b tube shard down outward falling.</summary>
    NoobTubeShardDownOutwardFalling,
    /// <summary>The frame of the n00b tube shard down outward falling mirrored.</summary>
    NoobTubeShardDownOutwardFallingMirrored,
    /// <summary>The frame of the n00b tube shard up outward flying.</summary>
    NoobTubeShardUpOutwardFlying,
    /// <summary>The frame of the n00b tube shard up outward flying mirrored.</summary>
    NoobTubeShardUpOutwardFlyingMirrored,
    /// <summary>The frame of the n00b tube shard up outward falling.</summary>
    NoobTubeShardUpOutwardFalling,
    /// <summary>The frame of the n00b tube shard up outward falling mirrored.</summary>
    NoobTubeShardUpOutwardFallingMirrored,
    /// <summary>The frame of the n00b tube shard up steep flying.</summary>
    NoobTubeShardUpSteepFlying,
    /// <summary>The frame of the n00b tube shard up steep flying mirrored.</summary>
    NoobTubeShardUpSteepFlyingMirrored,
    /// <summary>The frame of the n00b tube shard up steep falling.</summary>
    NoobTubeShardUpSteepFalling,
    /// <summary>The frame of the n00b tube shard up steep falling mirrored.</summary>
    NoobTubeShardUpSteepFallingMirrored,
    /// <summary>The frame of the n00b tube shard up diagonal inner flying.</summary>
    NoobTubeShardUpDiagonalInnerFlying,
    /// <summary>The frame of the n00b tube shard up diagonal inner flying mirrored.</summary>
    NoobTubeShardUpDiagonalInnerFlyingMirrored,
    /// <summary>The frame of the n00b tube shard up diagonal inner falling.</summary>
    NoobTubeShardUpDiagonalInnerFalling,
    /// <summary>The frame of the n00b tube shard up diagonal inner falling mirrored.</summary>
    NoobTubeShardUpDiagonalInnerFallingMirrored,
    /// <summary>The frame of the n00b tube shard up diagonal outer flying.</summary>
    NoobTubeShardUpDiagonalOuterFlying,
    /// <summary>The frame of the n00b tube shard up diagonal outer flying mirrored.</summary>
    NoobTubeShardUpDiagonalOuterFlyingMirrored,
    /// <summary>The frame of the n00b tube shard up diagonal outer falling.</summary>
    NoobTubeShardUpDiagonalOuterFalling,
    /// <summary>The frame of the n00b tube shard up diagonal outer falling mirrored.</summary>
    NoobTubeShardUpDiagonalOuterFallingMirrored,
    /// <summary>The frame of the n00b tube shard down diagonal flying.</summary>
    NoobTubeShardDownDiagonalFlying,
    /// <summary>The frame of the n00b tube shard down diagonal flying mirrored.</summary>
    NoobTubeShardDownDiagonalFlyingMirrored,
    /// <summary>The frame of the n00b tube shard down diagonal falling.</summary>
    NoobTubeShardDownDiagonalFalling,
    /// <summary>The frame of the n00b tube shard down diagonal falling mirrored.</summary>
    NoobTubeShardDownDiagonalFallingMirrored,
    /// <summary>The frame of the n00b tube shard down outer flying.</summary>
    NoobTubeShardDownOuterFlying,
    /// <summary>The frame of the n00b tube shard down outer flying mirrored.</summary>
    NoobTubeShardDownOuterFlyingMirrored,
    /// <summary>The frame of the n00b tube shard down outer falling.</summary>
    NoobTubeShardDownOuterFalling,
    /// <summary>The frame of the n00b tube shard down outer falling mirrored.</summary>
    NoobTubeShardDownOuterFallingMirrored,
    /// <summary>The frame of the n00b tube shard outward flying.</summary>
    NoobTubeShardOutwardFlying,
    /// <summary>The frame of the n00b tube shard outward flying mirrored.</summary>
    NoobTubeShardOutwardFlyingMirrored,
    /// <summary>The frame of the n00b tube shard outward falling.</summary>
    NoobTubeShardOutwardFalling,
    /// <summary>The frame of the n00b tube shard outward falling mirrored.</summary>
    NoobTubeShardOutwardFallingMirrored,
    /// <summary>The frame of the n00b tube shard up flying.</summary>
    NoobTubeShardUpFlying,
    /// <summary>The frame of the n00b tube shard up falling.</summary>
    NoobTubeShardUpFalling,
    /// <summary>The frame of the n00b tube shard down inner flying.</summary>
    NoobTubeShardDownInnerFlying,
    /// <summary>The frame of the n00b tube shard down inner flying mirrored.</summary>
    NoobTubeShardDownInnerFlyingMirrored,
    /// <summary>The frame of the n00b tube shard down inner falling.</summary>
    NoobTubeShardDownInnerFalling,
    /// <summary>The frame of the n00b tube shard down inner falling mirrored.</summary>
    NoobTubeShardDownInnerFallingMirrored,
    /// <summary>The first frame of the n00b tube bubble rising.</summary>
    NoobTubeBubbleRisingFirst,
    /// <summary>The second frame of the n00b tube bubble rising.</summary>
    NoobTubeBubbleRisingSecond,
    /// <summary>The third frame of the n00b tube bubble rising.</summary>
    NoobTubeBubbleRisingThird,
    /// <summary>The fourth frame of the n00b tube bubble rising.</summary>
    NoobTubeBubbleRisingFourth,
    /// <summary>The first frame of the n00b tube bubble falling.</summary>
    NoobTubeBubbleFallingFirst,
    /// <summary>The second frame of the n00b tube bubble falling.</summary>
    NoobTubeBubbleFallingSecond,
    /// <summary>The third frame of the n00b tube bubble falling.</summary>
    NoobTubeBubbleFallingThird,
    /// <summary>The fourth frame of the n00b tube bubble falling.</summary>
    NoobTubeBubbleFallingFourth,
    /// <summary>The fifth frame of the n00b tube bubble falling.</summary>
    NoobTubeBubbleFallingFifth,
    /// <summary>The sixth frame of the n00b tube bubble falling.</summary>
    NoobTubeBubbleFallingSixth,
    /// <summary>The seventh frame of the n00b tube bubble falling.</summary>
    NoobTubeBubbleFallingSeventh,
    /// <summary>The eighth frame of the n00b tube bubble falling.</summary>
    NoobTubeBubbleFallingEighth,
    /// <summary>The ninth frame of the n00b tube bubble falling.</summary>
    NoobTubeBubbleFallingNinth,
    /// <summary>The tenth frame of the n00b tube bubble falling.</summary>
    NoobTubeBubbleFallingTenth,
    /// <summary>The eleventh frame of the n00b tube bubble falling.</summary>
    NoobTubeBubbleFallingEleventh,
    /// <summary>The frame of the Cacatac spike left facing up.</summary>
    CacatacSpikeLeftFacingUp,
    /// <summary>The frame of the Cacatac spike up left.</summary>
    CacatacSpikeUpLeft,
    /// <summary>The frame of the Cacatac spike up.</summary>
    CacatacSpikeUp,
    /// <summary>The frame of the Cacatac spike up right.</summary>
    CacatacSpikeUpRight,
    /// <summary>The frame of the Cacatac spike right facing up.</summary>
    CacatacSpikeRightFacingUp,
    /// <summary>The frame of the Cacatac spike left facing down.</summary>
    CacatacSpikeLeftFacingDown,
    /// <summary>The frame of the Cacatac spike down left.</summary>
    CacatacSpikeDownLeft,
    /// <summary>The frame of the Cacatac spike down.</summary>
    CacatacSpikeDown,
    /// <summary>The frame of the Cacatac spike down right.</summary>
    CacatacSpikeDownRight,
    /// <summary>The frame of the Cacatac spike right facing down.</summary>
    CacatacSpikeRightFacingDown,
    /// <summary>The frame of the Spore Spawn spawner closed.</summary>
    SporeSpawnSpawnerClosed,
    /// <summary>The first frame of the Spore Spawn spawner release.</summary>
    SporeSpawnSpawnerReleaseFirst,
    /// <summary>The second frame of the Spore Spawn spawner release.</summary>
    SporeSpawnSpawnerReleaseSecond,
    /// <summary>The third frame of the Spore Spawn spawner release.</summary>
    SporeSpawnSpawnerReleaseThird,
    /// <summary>The fourth frame of the Spore Spawn spawner release.</summary>
    SporeSpawnSpawnerReleaseFourth,
    /// <summary>The fifth frame of the Spore Spawn spawner release.</summary>
    SporeSpawnSpawnerReleaseFifth,
    /// <summary>The first frame of the Spore Spawn Spore.</summary>
    SporeSpawnSporeFirst,
    /// <summary>The second frame of the Spore Spawn Spore.</summary>
    SporeSpawnSporeSecond,
    /// <summary>The third frame of the Spore Spawn Spore.</summary>
    SporeSpawnSporeThird,
    /// <summary>The frame of the Spore Spawn stalk.</summary>
    SporeSpawnStalk,
    /// <summary>The first frame of the Spore Spawn Spore shot.</summary>
    SporeSpawnSporeShotFirst,
    /// <summary>The second frame of the Spore Spawn Spore shot.</summary>
    SporeSpawnSporeShotSecond,
    /// <summary>The third frame of the Spore Spawn Spore shot.</summary>
    SporeSpawnSporeShotThird,
    /// <summary>The fourth frame of the Spore Spawn Spore shot.</summary>
    SporeSpawnSporeShotFourth,
    /// <summary>The fifth frame of the Spore Spawn Spore shot.</summary>
    SporeSpawnSporeShotFifth,
    /// <summary>The sixth frame of the Spore Spawn Spore shot.</summary>
    SporeSpawnSporeShotSixth,
    /// <summary>The seventh frame of the Spore Spawn Spore shot.</summary>
    SporeSpawnSporeShotSeventh,
    /// <summary>The first frame of the Fune Namihe fireball left.</summary>
    FuneNamiheFireballLeftFirst,
    /// <summary>The second frame of the Fune Namihe fireball left.</summary>
    FuneNamiheFireballLeftSecond,
    /// <summary>The third frame of the Fune Namihe fireball left.</summary>
    FuneNamiheFireballLeftThird,
    /// <summary>The first frame of the Fune Namihe fireball right.</summary>
    FuneNamiheFireballRightFirst,
    /// <summary>The second frame of the Fune Namihe fireball right.</summary>
    FuneNamiheFireballRightSecond,
    /// <summary>The third frame of the Fune Namihe fireball right.</summary>
    FuneNamiheFireballRightThird,
    /// <summary>The frame of the Magdollite lava left.</summary>
    MagdolliteLavaLeft,
    /// <summary>The frame of the Magdollite lava right.</summary>
    MagdolliteLavaRight,
    /// <summary>The first frame of the misc dust elbow charge particle.</summary>
    MiscDustElbowChargeParticleFirst,
    /// <summary>The second frame of the misc dust elbow charge particle.</summary>
    MiscDustElbowChargeParticleSecond,
    /// <summary>The third frame of the misc dust elbow charge particle.</summary>
    MiscDustElbowChargeParticleThird,
    /// <summary>The fourth frame of the misc dust elbow charge particle.</summary>
    MiscDustElbowChargeParticleFourth,
    /// <summary>The fifth frame of the misc dust elbow charge particle.</summary>
    MiscDustElbowChargeParticleFifth,
    /// <summary>The sixth frame of the misc dust elbow charge particle.</summary>
    MiscDustElbowChargeParticleSixth,
    /// <summary>The first frame of the misc dust elbow charge energy.</summary>
    MiscDustElbowChargeEnergyFirst,
    /// <summary>The second frame of the misc dust elbow charge energy.</summary>
    MiscDustElbowChargeEnergySecond,
    /// <summary>The third frame of the misc dust elbow charge energy.</summary>
    MiscDustElbowChargeEnergyThird,
    /// <summary>The fourth frame of the misc dust elbow charge energy.</summary>
    MiscDustElbowChargeEnergyFourth,
    /// <summary>The fifth frame of the misc dust elbow charge energy.</summary>
    MiscDustElbowChargeEnergyFifth,
    /// <summary>The sixth frame of the misc dust elbow charge energy.</summary>
    MiscDustElbowChargeEnergySixth,
    /// <summary>The seventh frame of the misc dust elbow charge energy.</summary>
    MiscDustElbowChargeEnergySeventh,
    /// <summary>The first frame of the misc dust small explosion.</summary>
    MiscDustSmallExplosionFirst,
    /// <summary>The second frame of the misc dust small explosion.</summary>
    MiscDustSmallExplosionSecond,
    /// <summary>The third frame of the misc dust small explosion.</summary>
    MiscDustSmallExplosionThird,
    /// <summary>The fourth frame of the misc dust small explosion.</summary>
    MiscDustSmallExplosionFourth,
    /// <summary>The fifth frame of the misc dust small explosion.</summary>
    MiscDustSmallExplosionFifth,
    /// <summary>The sixth frame of the misc dust small explosion.</summary>
    MiscDustSmallExplosionSixth,
    /// <summary>The first frame of the misc dust rainbow beam explosion.</summary>
    MiscDustRainbowBeamExplosionFirst,
    /// <summary>The second frame of the misc dust rainbow beam explosion.</summary>
    MiscDustRainbowBeamExplosionSecond,
    /// <summary>The third frame of the misc dust rainbow beam explosion.</summary>
    MiscDustRainbowBeamExplosionThird,
    /// <summary>The fourth frame of the misc dust rainbow beam explosion.</summary>
    MiscDustRainbowBeamExplosionFourth,
    /// <summary>The fifth frame of the misc dust rainbow beam explosion.</summary>
    MiscDustRainbowBeamExplosionFifth,
    /// <summary>The first frame of the misc dust dud shot.</summary>
    MiscDustDudShotFirst,
    /// <summary>The second frame of the misc dust dud shot.</summary>
    MiscDustDudShotSecond,
    /// <summary>The third frame of the misc dust dud shot.</summary>
    MiscDustDudShotThird,
    /// <summary>The fourth frame of the misc dust dud shot.</summary>
    MiscDustDudShotFourth,
    /// <summary>The fifth frame of the misc dust dud shot.</summary>
    MiscDustDudShotFifth,
    /// <summary>The sixth frame of the misc dust dud shot.</summary>
    MiscDustDudShotSixth,
    /// <summary>The first frame of the misc dust small dust cloud.</summary>
    MiscDustSmallDustCloudFirst,
    /// <summary>The second frame of the misc dust small dust cloud.</summary>
    MiscDustSmallDustCloudSecond,
    /// <summary>The third frame of the misc dust small dust cloud.</summary>
    MiscDustSmallDustCloudThird,
    /// <summary>The fourth frame of the misc dust small dust cloud.</summary>
    MiscDustSmallDustCloudFourth,
    /// <summary>The fifth frame of the misc dust small dust cloud.</summary>
    MiscDustSmallDustCloudFifth,
    /// <summary>The first frame of the misc dust corpse dust cloud.</summary>
    MiscDustCorpseDustCloudFirst,
    /// <summary>The second frame of the misc dust corpse dust cloud.</summary>
    MiscDustCorpseDustCloudSecond,
    /// <summary>The third frame of the misc dust corpse dust cloud.</summary>
    MiscDustCorpseDustCloudThird,
    /// <summary>The fourth frame of the misc dust corpse dust cloud.</summary>
    MiscDustCorpseDustCloudFourth,
    /// <summary>The first frame of the misc dust eye door smoke puff.</summary>
    MiscDustEyeDoorSmokePuffFirst,
    /// <summary>The second frame of the misc dust eye door smoke puff.</summary>
    MiscDustEyeDoorSmokePuffSecond,
    /// <summary>The third frame of the misc dust eye door smoke puff.</summary>
    MiscDustEyeDoorSmokePuffThird,
    /// <summary>The fourth frame of the misc dust eye door smoke puff.</summary>
    MiscDustEyeDoorSmokePuffFourth,
    /// <summary>The first frame of the misc dust smoke.</summary>
    MiscDustSmokeFirst,
    /// <summary>The second frame of the misc dust smoke.</summary>
    MiscDustSmokeSecond,
    /// <summary>The third frame of the misc dust smoke.</summary>
    MiscDustSmokeThird,
    /// <summary>The fourth frame of the misc dust smoke.</summary>
    MiscDustSmokeFourth,
    /// <summary>The first frame of the misc dust big explosion.</summary>
    MiscDustBigExplosionFirst,
    /// <summary>The second frame of the misc dust big explosion.</summary>
    MiscDustBigExplosionSecond,
    /// <summary>The third frame of the misc dust big explosion.</summary>
    MiscDustBigExplosionThird,
    /// <summary>The fourth frame of the misc dust big explosion.</summary>
    MiscDustBigExplosionFourth,
    /// <summary>The fifth frame of the misc dust big explosion.</summary>
    MiscDustBigExplosionFifth,
    /// <summary>The sixth frame of the misc dust big explosion.</summary>
    MiscDustBigExplosionSixth,
    /// <summary>The first frame of the misc dust rock particles.</summary>
    MiscDustRockParticlesFirst,
    /// <summary>The second frame of the misc dust rock particles.</summary>
    MiscDustRockParticlesSecond,
    /// <summary>The third frame of the misc dust rock particles.</summary>
    MiscDustRockParticlesThird,
    /// <summary>The fourth frame of the misc dust rock particles.</summary>
    MiscDustRockParticlesFourth,
    /// <summary>The fifth frame of the misc dust rock particles.</summary>
    MiscDustRockParticlesFifth,
    /// <summary>The sixth frame of the misc dust rock particles.</summary>
    MiscDustRockParticlesSixth,
    /// <summary>The seventh frame of the misc dust rock particles.</summary>
    MiscDustRockParticlesSeventh,
    /// <summary>The eighth frame of the misc dust rock particles.</summary>
    MiscDustRockParticlesEighth,
    /// <summary>The ninth frame of the misc dust rock particles.</summary>
    MiscDustRockParticlesNinth,
    /// <summary>The tenth frame of the misc dust rock particles.</summary>
    MiscDustRockParticlesTenth,
    /// <summary>The eleventh frame of the misc dust rock particles.</summary>
    MiscDustRockParticlesEleventh,
    /// <summary>The twelfth frame of the misc dust rock particles.</summary>
    MiscDustRockParticlesTwelfth,
    /// <summary>The thirteenth frame of the misc dust rock particles.</summary>
    MiscDustRockParticlesThirteenth,
    /// <summary>The fourteenth frame of the misc dust rock particles.</summary>
    MiscDustRockParticlesFourteenth,
    /// <summary>The fifteenth frame of the misc dust rock particles.</summary>
    MiscDustRockParticlesFifteenth,
    /// <summary>The sixteenth frame of the misc dust rock particles.</summary>
    MiscDustRockParticlesSixteenth,
    /// <summary>The first frame of the misc dust short big dust cloud.</summary>
    MiscDustShortBigDustCloudFirst,
    /// <summary>The second frame of the misc dust short big dust cloud.</summary>
    MiscDustShortBigDustCloudSecond,
    /// <summary>The third frame of the misc dust short big dust cloud.</summary>
    MiscDustShortBigDustCloudThird,
    /// <summary>The fourth frame of the misc dust short big dust cloud.</summary>
    MiscDustShortBigDustCloudFourth,
    /// <summary>The first frame of the misc dust big dust cloud.</summary>
    MiscDustBigDustCloudFirst,
    /// <summary>The second frame of the misc dust big dust cloud.</summary>
    MiscDustBigDustCloudSecond,
    /// <summary>The third frame of the misc dust big dust cloud.</summary>
    MiscDustBigDustCloudThird,
    /// <summary>The fourth frame of the misc dust big dust cloud.</summary>
    MiscDustBigDustCloudFourth,
    /// <summary>The fifth frame of the misc dust big dust cloud.</summary>
    MiscDustBigDustCloudFifth,
    /// <summary>The sixth frame of the misc dust big dust cloud.</summary>
    MiscDustBigDustCloudSixth,
    /// <summary>The seventh frame of the misc dust big dust cloud.</summary>
    MiscDustBigDustCloudSeventh,
    /// <summary>The eighth frame of the misc dust big dust cloud.</summary>
    MiscDustBigDustCloudEighth,
    /// <summary>The first frame of the misc dust Draygon breath bubbles.</summary>
    MiscDustDraygonBreathBubblesFirst,
    /// <summary>The second frame of the misc dust Draygon breath bubbles.</summary>
    MiscDustDraygonBreathBubblesSecond,
    /// <summary>The third frame of the misc dust Draygon breath bubbles.</summary>
    MiscDustDraygonBreathBubblesThird,
    /// <summary>The fourth frame of the misc dust Draygon breath bubbles.</summary>
    MiscDustDraygonBreathBubblesFourth,
    /// <summary>The fifth frame of the misc dust Draygon breath bubbles.</summary>
    MiscDustDraygonBreathBubblesFifth,
    /// <summary>The sixth frame of the misc dust Draygon breath bubbles.</summary>
    MiscDustDraygonBreathBubblesSixth,
    /// <summary>The seventh frame of the misc dust Draygon breath bubbles.</summary>
    MiscDustDraygonBreathBubblesSeventh,
    /// <summary>The eighth frame of the misc dust Draygon breath bubbles.</summary>
    MiscDustDraygonBreathBubblesEighth,
    /// <summary>The ninth frame of the misc dust Draygon breath bubbles.</summary>
    MiscDustDraygonBreathBubblesNinth,
    /// <summary>The frame of the downward gate moving.</summary>
    DownwardGateMoving,
    /// <summary>The downward gate lowered one block frame.</summary>
    DownwardGateLoweredOneBlock,
    /// <summary>The downward gate lowered two blocks frame.</summary>
    DownwardGateLoweredTwoBlocks,
    /// <summary>The downward gate lowered three blocks frame.</summary>
    DownwardGateLoweredThreeBlocks,
    /// <summary>The downward gate closed frame.</summary>
    DownwardGateClosed,
    /// <summary>The downward gate rising frame.</summary>
    DownwardGateRising,
    /// <summary>The downward gate risen one block frame.</summary>
    DownwardGateRisenOneBlock,
    /// <summary>The downward gate risen two blocks frame.</summary>
    DownwardGateRisenTwoBlocks,
    /// <summary>The downward gate risen three blocks frame.</summary>
    DownwardGateRisenThreeBlocks,
    /// <summary>The first frame of the save station electricity start.</summary>
    SaveStationElectricityStartFirst,
    /// <summary>The second frame of the save station electricity start.</summary>
    SaveStationElectricityStartSecond,
    /// <summary>The third frame of the save station electricity start.</summary>
    SaveStationElectricityStartThird,
    /// <summary>The fourth frame of the save station electricity start.</summary>
    SaveStationElectricityStartFourth,
    /// <summary>The fifth frame of the save station electricity start.</summary>
    SaveStationElectricityStartFifth,
    /// <summary>The sixth frame of the save station electricity start.</summary>
    SaveStationElectricityStartSixth,
    /// <summary>The seventh frame of the save station electricity start.</summary>
    SaveStationElectricityStartSeventh,
    /// <summary>The eighth frame of the save station electricity start.</summary>
    SaveStationElectricityStartEighth,
    /// <summary>The first frame of the Botwoon body up left.</summary>
    BotwoonBodyUpLeftFirst,
    /// <summary>The second frame of the Botwoon body up left.</summary>
    BotwoonBodyUpLeftSecond,
    /// <summary>The third frame of the Botwoon body up left.</summary>
    BotwoonBodyUpLeftThird,
    /// <summary>The fourth frame of the Botwoon body up left.</summary>
    BotwoonBodyUpLeftFourth,
    /// <summary>The first frame of the Botwoon body left.</summary>
    BotwoonBodyLeftFirst,
    /// <summary>The second frame of the Botwoon body left.</summary>
    BotwoonBodyLeftSecond,
    /// <summary>The third frame of the Botwoon body left.</summary>
    BotwoonBodyLeftThird,
    /// <summary>The fourth frame of the Botwoon body left.</summary>
    BotwoonBodyLeftFourth,
    /// <summary>The first frame of the Botwoon body down left.</summary>
    BotwoonBodyDownLeftFirst,
    /// <summary>The second frame of the Botwoon body down left.</summary>
    BotwoonBodyDownLeftSecond,
    /// <summary>The third frame of the Botwoon body down left.</summary>
    BotwoonBodyDownLeftThird,
    /// <summary>The fourth frame of the Botwoon body down left.</summary>
    BotwoonBodyDownLeftFourth,
    /// <summary>The first frame of the Botwoon body down.</summary>
    BotwoonBodyDownFirst,
    /// <summary>The second frame of the Botwoon body down.</summary>
    BotwoonBodyDownSecond,
    /// <summary>The third frame of the Botwoon body down.</summary>
    BotwoonBodyDownThird,
    /// <summary>The fourth frame of the Botwoon body down.</summary>
    BotwoonBodyDownFourth,
    /// <summary>The first frame of the Botwoon body down right.</summary>
    BotwoonBodyDownRightFirst,
    /// <summary>The second frame of the Botwoon body down right.</summary>
    BotwoonBodyDownRightSecond,
    /// <summary>The third frame of the Botwoon body down right.</summary>
    BotwoonBodyDownRightThird,
    /// <summary>The fourth frame of the Botwoon body down right.</summary>
    BotwoonBodyDownRightFourth,
    /// <summary>The first frame of the Botwoon body right.</summary>
    BotwoonBodyRightFirst,
    /// <summary>The second frame of the Botwoon body right.</summary>
    BotwoonBodyRightSecond,
    /// <summary>The third frame of the Botwoon body right.</summary>
    BotwoonBodyRightThird,
    /// <summary>The fourth frame of the Botwoon body right.</summary>
    BotwoonBodyRightFourth,
    /// <summary>The first frame of the Botwoon body up right.</summary>
    BotwoonBodyUpRightFirst,
    /// <summary>The second frame of the Botwoon body up right.</summary>
    BotwoonBodyUpRightSecond,
    /// <summary>The third frame of the Botwoon body up right.</summary>
    BotwoonBodyUpRightThird,
    /// <summary>The fourth frame of the Botwoon body up right.</summary>
    BotwoonBodyUpRightFourth,
    /// <summary>The first frame of the Botwoon body up.</summary>
    BotwoonBodyUpFirst,
    /// <summary>The second frame of the Botwoon body up.</summary>
    BotwoonBodyUpSecond,
    /// <summary>The third frame of the Botwoon body up.</summary>
    BotwoonBodyUpThird,
    /// <summary>The fourth frame of the Botwoon body up.</summary>
    BotwoonBodyUpFourth,
    /// <summary>The frame of the Botwoon tail up.</summary>
    BotwoonTailUp,
    /// <summary>The frame of the Botwoon tail up left.</summary>
    BotwoonTailUpLeft,
    /// <summary>The frame of the Botwoon tail left.</summary>
    BotwoonTailLeft,
    /// <summary>The frame of the Botwoon tail down left.</summary>
    BotwoonTailDownLeft,
    /// <summary>The frame of the Botwoon tail down.</summary>
    BotwoonTailDown,
    /// <summary>The frame of the Botwoon tail down right.</summary>
    BotwoonTailDownRight,
    /// <summary>The frame of the Botwoon tail right.</summary>
    BotwoonTailRight,
    /// <summary>The frame of the Botwoon tail up right.</summary>
    BotwoonTailUpRight,
    /// <summary>The frame of the Botwoon hidden up.</summary>
    BotwoonHiddenUp,
    /// <summary>The first frame of the Botwoon spit up.</summary>
    BotwoonSpitUpFirst,
    /// <summary>The second frame of the Botwoon spit up.</summary>
    BotwoonSpitUpSecond,
    /// <summary>The third frame of the Botwoon spit up.</summary>
    BotwoonSpitUpThird,
    /// <summary>The fourth frame of the Botwoon spit up.</summary>
    BotwoonSpitUpFourth,
    /// <summary>The fifth frame of the Botwoon spit up.</summary>
    BotwoonSpitUpFifth,
    /// <summary>The frame of the Yapping Maw facing down.</summary>
    YappingMawFacingDown,
    /// <summary>The frame of the Yapping Maw facing up.</summary>
    YappingMawFacingUp,
    /// <summary>The frame of the enemy death respawn tail.</summary>
    EnemyDeathRespawnTail,
    /// <summary>The frame of the enemy death big explosion.</summary>
    EnemyDeathBigExplosion,
    /// <summary>The frame of the enemy death Mini Kraid explosion.</summary>
    EnemyDeathMiniKraidExplosion,
    /// <summary>The first frame of the enemy death normal explosion.</summary>
    EnemyDeathNormalExplosionFirst,
    /// <summary>The second frame of the enemy death normal explosion.</summary>
    EnemyDeathNormalExplosionSecond,
    /// <summary>The third frame of the enemy death normal explosion.</summary>
    EnemyDeathNormalExplosionThird,
    /// <summary>The fourth frame of the enemy death normal explosion.</summary>
    EnemyDeathNormalExplosionFourth,
    /// <summary>The fifth frame of the enemy death normal explosion.</summary>
    EnemyDeathNormalExplosionFifth,
    /// <summary>The sixth frame of the enemy death normal explosion.</summary>
    EnemyDeathNormalExplosionSixth,
    /// <summary>The first frame of the enemy death small explosion.</summary>
    EnemyDeathSmallExplosionFirst,
    /// <summary>The second frame of the enemy death small explosion.</summary>
    EnemyDeathSmallExplosionSecond,
    /// <summary>The third frame of the enemy death small explosion.</summary>
    EnemyDeathSmallExplosionThird,
    /// <summary>The fourth frame of the enemy death small explosion.</summary>
    EnemyDeathSmallExplosionFourth,
    /// <summary>The fifth frame of the enemy death small explosion.</summary>
    EnemyDeathSmallExplosionFifth,
    /// <summary>The sixth frame of the enemy death small explosion.</summary>
    EnemyDeathSmallExplosionSixth,
    /// <summary>The first frame of the enemy pickup small energy.</summary>
    EnemyPickupSmallEnergyFirst,
    /// <summary>The second frame of the enemy pickup small energy.</summary>
    EnemyPickupSmallEnergySecond,
    /// <summary>The third frame of the enemy pickup small energy.</summary>
    EnemyPickupSmallEnergyThird,
    /// <summary>The fourth frame of the enemy pickup small energy.</summary>
    EnemyPickupSmallEnergyFourth,
    /// <summary>The first frame of the enemy pickup big energy.</summary>
    EnemyPickupBigEnergyFirst,
    /// <summary>The second frame of the enemy pickup big energy.</summary>
    EnemyPickupBigEnergySecond,
    /// <summary>The third frame of the enemy pickup big energy.</summary>
    EnemyPickupBigEnergyThird,
    /// <summary>The fourth frame of the enemy pickup big energy.</summary>
    EnemyPickupBigEnergyFourth,
    /// <summary>The first frame of the enemy pickup Missiles.</summary>
    EnemyPickupMissilesFirst,
    /// <summary>The second frame of the enemy pickup Missiles.</summary>
    EnemyPickupMissilesSecond,
    /// <summary>The first frame of the enemy pickup Super Missiles.</summary>
    EnemyPickupSuperMissilesFirst,
    /// <summary>The second frame of the enemy pickup Super Missiles.</summary>
    EnemyPickupSuperMissilesSecond,
    /// <summary>The first frame of the enemy pickup Power Bombs.</summary>
    EnemyPickupPowerBombsFirst,
    /// <summary>The second frame of the enemy pickup Power Bombs.</summary>
    EnemyPickupPowerBombsSecond,
    /// <summary>The third frame of the enemy pickup Power Bombs.</summary>
    EnemyPickupPowerBombsThird,
    /// <summary>The fourth frame of the enemy pickup Power Bombs.</summary>
    EnemyPickupPowerBombsFourth,
    /// <summary>The first frame of the enemy death killed by Samus contact.</summary>
    EnemyDeathKilledBySamusContactFirst,
    /// <summary>The second frame of the enemy death killed by Samus contact.</summary>
    EnemyDeathKilledBySamusContactSecond,
    /// <summary>The third frame of the enemy death killed by Samus contact.</summary>
    EnemyDeathKilledBySamusContactThird,
    /// <summary>The fourth frame of the enemy death killed by Samus contact.</summary>
    EnemyDeathKilledBySamusContactFourth,
    /// <summary>The fifth frame of the enemy death killed by Samus contact.</summary>
    EnemyDeathKilledBySamusContactFifth,
    /// <summary>The sixth frame of the enemy death killed by Samus contact.</summary>
    EnemyDeathKilledBySamusContactSixth,
    /// <summary>The seventh frame of the enemy death killed by Samus contact.</summary>
    EnemyDeathKilledBySamusContactSeventh,
    /// <summary>The eighth frame of the enemy death killed by Samus contact.</summary>
    EnemyDeathKilledBySamusContactEighth,
    /// <summary>The ninth frame of the enemy death killed by Samus contact.</summary>
    EnemyDeathKilledBySamusContactNinth,
    /// <summary>The tenth frame of the enemy death killed by Samus contact.</summary>
    EnemyDeathKilledBySamusContactTenth,
    /// <summary>The eleventh frame of the enemy death killed by Samus contact.</summary>
    EnemyDeathKilledBySamusContactEleventh,
    /// <summary>The twelfth frame of the enemy death killed by Samus contact.</summary>
    EnemyDeathKilledBySamusContactTwelfth,
    /// <summary>The thirteenth frame of the enemy death killed by Samus contact.</summary>
    EnemyDeathKilledBySamusContactThirteenth,
    /// <summary>The fourteenth frame of the enemy death killed by Samus contact.</summary>
    EnemyDeathKilledBySamusContactFourteenth,
    /// <summary>The fifteenth frame of the enemy death killed by Samus contact.</summary>
    EnemyDeathKilledBySamusContactFifteenth,
    /// <summary>The sixteenth frame of the enemy death killed by Samus contact.</summary>
    EnemyDeathKilledBySamusContactSixteenth,
    /// <summary>The first frame of the falling spark falling.</summary>
    FallingSparkFallingFirst,
    /// <summary>The second frame of the falling spark falling.</summary>
    FallingSparkFallingSecond,
    /// <summary>The third frame of the falling spark falling.</summary>
    FallingSparkFallingThird,
    /// <summary>The first frame of the falling spark hit floor.</summary>
    FallingSparkHitFloorFirst,
    /// <summary>The second frame of the falling spark hit floor.</summary>
    FallingSparkHitFloorSecond,
    /// <summary>The third frame of the falling spark hit floor.</summary>
    FallingSparkHitFloorThird,
    /// <summary>The fourth frame of the falling spark hit floor.</summary>
    FallingSparkHitFloorFourth,
    /// <summary>The fifth frame of the falling spark hit floor.</summary>
    FallingSparkHitFloorFifth,
    /// <summary>The sixth frame of the falling spark hit floor.</summary>
    FallingSparkHitFloorSixth,
    /// <summary>The seventh frame of the falling spark hit floor.</summary>
    FallingSparkHitFloorSeventh,
    /// <summary>The eighth frame of the falling spark hit floor.</summary>
    FallingSparkHitFloorEighth,
    /// <summary>The ninth frame of the falling spark hit floor.</summary>
    FallingSparkHitFloorNinth,
    /// <summary>The tenth frame of the falling spark hit floor.</summary>
    FallingSparkHitFloorTenth,
    /// <summary>The eleventh frame of the falling spark hit floor.</summary>
    FallingSparkHitFloorEleventh,
    /// <summary>The Ceres elevator pad first, one composed picture.</summary>
    CeresElevatorPadFirst,
    /// <summary>The Ceres elevator pad second, one composed picture.</summary>
    CeresElevatorPadSecond,
    /// <summary>The Ceres elevator platform, one composed picture.</summary>
    CeresElevatorPlatform,
}
