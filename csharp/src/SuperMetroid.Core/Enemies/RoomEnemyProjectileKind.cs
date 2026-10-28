namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The definition occupying one live room-enemy projectile slot. The values are arbitrary (#1452); verification's
/// native view names each member's bank-$86 definition word, which a native snapshot's <c>eproj_id</c> holds.
/// </summary>
public enum RoomEnemyProjectileKind
{
    /// <summary>No projectile: the physical slot is available for allocation.</summary>
    None = 0,
    /// <summary>The pre-Phantoon room's BG2-offset keeper, spawned by the room's setup.</summary>
    PrePhantoonRoom = 1,
    /// <summary><c>EnemyProjectile_YappingMawsBody</c>: the Maw's projectile-backed body.</summary>
    YappingMawBody = 2,
    /// <summary><c>EnemyProjectile_SkreeParticles_DownRight</c>: Skree's downward-right burst particle.</summary>
    SkreeParticleDownRight = 3,
    /// <summary><c>EnemyProjectile_SkreeParticles_UpRight</c>: Skree's upward-right burst particle.</summary>
    SkreeParticleUpRight = 4,
    /// <summary><c>EnemyProjectile_SkreeParticles_DownLeft</c>: Skree's downward-left burst particle.</summary>
    SkreeParticleDownLeft = 5,
    /// <summary><c>EnemyProjectile_SkreeParticles_UpLeft</c>: Skree's upward-left burst particle.</summary>
    SkreeParticleUpLeft = 6,
    /// <summary><c>EnemyProjectile_MetalSkreeParticles_DownRight</c>: Metaree's downward-right metal particle.</summary>
    MetareeParticleDownRight = 7,
    /// <summary><c>EnemyProjectile_MetalSkreeParticles_UpRight</c>: Metaree's upward-right metal particle.</summary>
    MetareeParticleUpRight = 8,
    /// <summary><c>EnemyProjectile_MetalSkreeParticles_DownLeft</c>: Metaree's downward-left metal particle.</summary>
    MetareeParticleDownLeft = 9,
    /// <summary><c>EnemyProjectile_MetalSkreeParticles_UpLeft</c>: Metaree's upward-left metal particle.</summary>
    MetareeParticleUpLeft = 10,
    /// <summary><c>EnemyProjectile_CrocomiresProjectile</c>: Crocomire's fired attack projectile.</summary>
    CrocomireProjectile = 11,
    /// <summary><c>EnemyProjectile_CrocomireBridgeCrumbling</c>: the collapsing bridge's debris actor.</summary>
    CrocomireBridgeCrumbling = 12,
    /// <summary><c>EnemyProjectile_CrocomireSpikeWallPieces</c>: fragments of Crocomire's spike wall.</summary>
    CrocomireSpikeWallPieces = 13,
    /// <summary><c>EnemyProjectile_KraidRockSpit</c>: the rock emitted by Kraid's spit attack.</summary>
    KraidSpitRock = 14,
    /// <summary><c>EnemyProjectile_KraidCeilingRocks</c>: falling ceiling debris in Kraid's room.</summary>
    KraidCeilingRock = 15,
    /// <summary><c>EnemyProjectile_KraidFloorRocks_Left</c>: the left member of Kraid's rising floor-rock pair.</summary>
    KraidRisingRockLeft = 16,
    /// <summary><c>EnemyProjectile_KraidFloorRocks_Right</c>: the right member of Kraid's rising floor-rock pair.</summary>
    KraidRisingRockRight = 17,
    /// <summary><c>EnemyProjectile_PhantoonDestroyableFlames</c>: Phantoon's destructible flame actor.</summary>
    PhantoonDestroyableFlame = 18,
    /// <summary><c>EnemyProjectile_PhantoonStartingFlames</c>: Phantoon's opening flame actor.</summary>
    PhantoonStartingFlame = 19,
    /// <summary><c>EnemyProjectile_DraygonGoop</c>: Draygon's goop attack.</summary>
    DraygonGoop = 20,
    /// <summary><c>EnemyProjectile_DraygonWallTurret</c>: the projectile-backed turret in Draygon's room.</summary>
    DraygonWallTurret = 21,
    /// <summary><c>EnemyProjectile_MotherBrainTurret</c>: a Mother Brain room turret actor.</summary>
    MotherBrainRoomTurret = 22,
    /// <summary><c>EnemyProjectile_MotherBrainTurretBullets</c>: a bullet fired by a Mother Brain room turret.</summary>
    MotherBrainRoomTurretBullet = 23,
    /// <summary><c>EnemyProjectile_MotherBrainGlassShattering_Shard</c>: a shard from Mother Brain's broken glass.</summary>
    MotherBrainGlassShard = 24,
    /// <summary><c>EnemyProjectile_MotherBrainGlassShattering_Sparkle</c>: a glass-shattering sparkle actor.</summary>
    MotherBrainGlassSparkle = 25,
    /// <summary><c>EnemyProjectile_MotherBrainOnionRings</c>: Mother Brain's ring-shaped attack.</summary>
    MotherBrainOnionRing = 26,
    /// <summary>The explosion of the Climb's escape-shaft fake wall.</summary>
    OldTourianEscapeShaftFakeWallExplosion = 27,
    /// <summary><c>EnemyProjectile_MotherBrainBomb</c>: Mother Brain's bomb attack.</summary>
    MotherBrainBomb = 28,
    /// <summary><c>EnemyProjectile_MotherBrainRedBeam_Charging</c>: the charging phase of Mother Brain's hand beam.</summary>
    MotherBrainHandBeamCharging = 29,
    /// <summary><c>EnemyProjectile_MotherBrainRedBeam_Fired</c>: the fired phase of Mother Brain's hand beam.</summary>
    MotherBrainHandBeamFired = 30,
    /// <summary><c>EnemyProjectile_MotherBrainRainbowBeam_Charging</c>: the rainbow beam's charging actor.</summary>
    MotherBrainRainbowBeamCharging = 31,
    /// <summary><c>EnemyProjectile_MotherBrainPurpleBreath_Big</c>: Mother Brain's large purple breath projectile.</summary>
    MotherBrainPurpleBreathBig = 32,
    /// <summary>Mother Brain's small purple breath projectile.</summary>
    MotherBrainPurpleBreathSmall = 33,
    /// <summary><c>EnemyProjectile_MotherBrainDrool</c>: drool emitted during Mother Brain's encounter.</summary>
    MotherBrainDrool = 34,
    /// <summary><c>EnemyProjectile_MotherBrainDyingDrool</c>: drool emitted during Mother Brain's death.</summary>
    MotherBrainDyingDrool = 35,
    /// <summary><c>EnemyProjectile_MotherBrainRainbowBeam_Explosion</c>: a rainbow-beam explosion actor.</summary>
    MotherBrainRainbowBeamExplosion = 36,
    /// <summary>Mother Brain's body-relative death explosions, which follow the moving body.</summary>
    MotherBrainDeathExplosion = 37,
    /// <summary><c>EnemyProjectile_MotherBrainExplodedEscapeDoorParticles</c>: a fragment of the opened escape door.</summary>
    MotherBrainEscapeDoorFragment = 38,
    /// <summary><c>EnemyProjectile_TimeBombSetSubtitle</c>: the optional Japanese timebomb subtitle.</summary>
    MotherBrainEscapeSubtitle = 39,
    /// <summary><c>EnemyProjectile_MotherBrainTubeFalling_TopRight</c>: Mother Brain's falling top-right tube.</summary>
    MotherBrainTopRightTube = 40,
    /// <summary><c>EnemyProjectile_MotherBrainTubeFalling_TopLeft</c>: Mother Brain's falling top-left tube.</summary>
    MotherBrainTopLeftTube = 41,
    /// <summary><c>EnemyProjectile_MotherBrainTubeFalling_TopMiddleLeft</c>: Mother Brain's falling middle-left upper tube.</summary>
    MotherBrainTopMiddleLeftTube = 42,
    /// <summary><c>EnemyProjectile_MotherBrainTubeFalling_TopMiddleRight</c>: Mother Brain's falling middle-right upper tube.</summary>
    MotherBrainTopMiddleRightTube = 43,
    /// <summary><c>EnemyProjectile_RidleysFireball</c>: Ridley's aimed fireball, shared by Ceres and Zebes.</summary>
    CeresRidleyFireball = 44,
    /// <summary><c>EnemyProjectile_RidleyHorizontalAfterburn_Center</c>: the stationary center that spawns horizontal afterburns.</summary>
    CeresRidleyHorizontalAfterburnCenter = 45,
    /// <summary><c>EnemyProjectile_RidleyVerticalAfterburn_Center</c>: the stationary center that spawns vertical afterburns.</summary>
    CeresRidleyVerticalAfterburnCenter = 46,
    /// <summary><c>EnemyProjectile_RidleyHorizontalAfterburn_Right</c>: a rightward afterburn and its continuation chain.</summary>
    CeresRidleyHorizontalAfterburnRight = 47,
    /// <summary><c>EnemyProjectile_RidleyHorizontalAfterburn_Left</c>: a leftward afterburn and its continuation chain.</summary>
    CeresRidleyHorizontalAfterburnLeft = 48,
    /// <summary><c>EnemyProjectile_RidleyVerticalAfterburn_Up</c>: an upward afterburn and its continuation chain.</summary>
    CeresRidleyVerticalAfterburnUp = 49,
    /// <summary><c>EnemyProjectile_RidleyVerticalAfterburn_Down</c>: a downward afterburn and its continuation chain.</summary>
    CeresRidleyVerticalAfterburnDown = 50,
    /// <summary><c>EnemyProjectile_CeresFallingTile_Light</c>: the light palette variant of Ceres falling debris.</summary>
    CeresFallingDebrisLight = 51,
    /// <summary><c>EnemyProjectile_CeresFallingTile_Dark</c>: the dark palette variant of Ceres falling debris.</summary>
    CeresFallingDebrisDark = 52,
    /// <summary><c>EnemyProjectile_GunShipLiftoffDustClouds</c>: dust emitted during gunship liftoff.</summary>
    GunshipLiftoffDustCloud = 53,
    /// <summary><c>EnemyProjectile_AlcoonFireball</c>: Alcoon's fireball attack.</summary>
    AlcoonFireball = 54,
    /// <summary><c>EnemyProjectile_Powamp</c>: the spike actor emitted by Powamp.</summary>
    PowampSpike = 55,
    /// <summary><c>EnemyProjectile_RobotLaser_UpLeft</c>: a Work Robot laser directed upward and left.</summary>
    WorkRobotLaserUpLeft = 56,
    /// <summary><c>EnemyProjectile_RobotLaser_Horizontal</c>: the Work Robot's horizontal laser definition.</summary>
    WorkRobotLaserHorizontal = 57,
    /// <summary><c>EnemyProjectile_RobotLaser_DownLeft</c>: a Work Robot laser directed downward and left.</summary>
    WorkRobotLaserDownLeft = 58,
    /// <summary><c>EnemyProjectile_RobotLaser_UpRight</c>: a Work Robot laser directed upward and right.</summary>
    WorkRobotLaserUpRight = 59,
    /// <summary><c>EnemyProjectile_RobotLaser_DownRight</c>: a Work Robot laser directed downward and right.</summary>
    WorkRobotLaserDownRight = 60,
    /// <summary><c>EnemyProjectile_FallingSpark</c>: a falling spark actor.</summary>
    FallingSpark = 61,
    /// <summary><c>EnemyProjectile_Puromi</c>: a persistent projectile-backed Nuclear Waffle body link.</summary>
    NuclearWaffleBody = 62,
    /// <summary><c>EnemyProjectile_MiniKraidSpit</c>: Fake Kraid's spit projectile.</summary>
    FakeKraidSpit = 63,
    /// <summary><c>EnemyProjectile_MiniKraidSpikes_Left</c>: Fake Kraid's left spike projectile.</summary>
    FakeKraidSpikeLeft = 64,
    /// <summary><c>EnemyProjectile_MiniKraidSpikes_Right</c>: Fake Kraid's right spike projectile.</summary>
    FakeKraidSpikeRight = 65,
    /// <summary><c>EnemyProjectile_KiHunterAcidSpit_Left</c>: the left-facing Ki-Hunter acid-spit definition.</summary>
    KiHunterAcidSpitLeft = 66,
    /// <summary><c>EnemyProjectile_KiHunterAcidSpit_Right</c>: the right-facing Ki-Hunter acid-spit definition.</summary>
    KiHunterAcidSpitRight = 67,
    /// <summary><c>EnemyProjectile_PirateMotherBrainLaser</c>: the Space Pirate laser used in Mother Brain's room.</summary>
    PirateMotherBrainLaser = 68,
    /// <summary><c>EnemyProjectile_PirateClaw</c>: a Space Pirate claw projectile.</summary>
    PirateClaw = 69,
    /// <summary><c>EnemyProjectile_LavaquakeRocks</c>: the rock actor emitted by Polyp.</summary>
    PolypRock = 70,
    /// <summary><c>EnemyProjectile_Cacatac</c>: a Cacatac's directional spike.</summary>
    CacatacSpike = 71,
    /// <summary><c>UNUSED_EnemyProjectile_Stoke_86DBF2</c>: the native unused mini-Crocomire/Stoke projectile definition.</summary>
    StokeProjectile = 72,
    /// <summary><c>EnemyProjectile_NamiheFireball</c>: Namihe's fireball definition.</summary>
    NamiheFireball = 73,
    /// <summary><c>EnemyProjectile_FuneFireball</c>: Fune's fireball definition.</summary>
    FuneFireball = 74,
    /// <summary><c>EnemyProjectile_Magdollite</c>: lava thrown by Magdollite.</summary>
    LavaThrownByMagdollite = 75,
    /// <summary><c>EnemyProjectile_DragonFireball</c>: Dragon's fireball attack.</summary>
    DragonFireball = 76,
    /// <summary><c>EnemyProjectile_EyeDoorProjectile</c>: the Eye Door's aimed projectile.</summary>
    EyeDoorProjectile = 77,
    /// <summary><c>EnemyProjectile_EyeDoorSweat</c>: the Eye Door's sweat actor.</summary>
    EyeDoorSweat = 78,
    /// <summary><c>EnemyProjectile_MiscDustPLM</c>: the PLM-spawned smoke actor used by Eye Doors.</summary>
    EyeDoorSmoke = 79,
    /// <summary><c>EnemyProjectile_MiscDust</c>: a parameter-selected dust or explosion effect.</summary>
    MiscDustExplosion = 80,
    /// <summary><c>EnemyProjectile_EnemyDeathPickup</c>: an enemy drop with its pickup animation and collection behavior.</summary>
    EnemyDeathPickup = 81,
    /// <summary><c>EnemyProjectile_EnemyDeathExplosion</c>: an enemy-death explosion that can publish drops or rebuild an enemy slot.</summary>
    EnemyDeathExplosion = 82,
    /// <summary><c>EnemyProjectile_KagoBug</c>: a destructible bug released by Kago.</summary>
    KagoBug = 83,
    /// <summary><c>EnemyProjectile_BotwoonsBody</c>: a body segment following Botwoon's stored trajectory.</summary>
    BotwoonBody = 84,
    /// <summary><c>EnemyProjectile_BotwoonsSpit</c>: Botwoon's angled spit projectile.</summary>
    BotwoonSpit = 85,
    /// <summary><c>EnemyProjectile_BombTorizoContinuousDrool</c>: continuous drool from low-health Bomb Torizo.</summary>
    BombTorizoLowHealthDrool = 86,
    /// <summary><c>EnemyProjectile_BombTorizoInitialDrool</c>: Bomb Torizo's initial drool actor.</summary>
    BombTorizoInitialDrool = 87,
    /// <summary><c>EnemyProjectile_BombTorizoExplosiveSwipe</c>: the projectile actor for Bomb Torizo's explosive swipe.</summary>
    BombTorizoExplosiveSwipe = 88,
    /// <summary><c>EnemyProjectile_BombTorizoLowHealthExplosion</c>: a low-health Bomb Torizo explosion.</summary>
    BombTorizoLowHealthExplosion = 89,
    /// <summary><c>EnemyProjectile_BombTorizoDeathExplosion</c>: an explosion during Bomb Torizo's death sequence.</summary>
    BombTorizoDeathExplosion = 90,
    /// <summary><c>EnemyProjectile_BombTorizoStatueBreaking</c>: a fragment effect from Bomb Torizo's breaking statue.</summary>
    BombTorizoStatueBreaking = 91,
    /// <summary><c>EnemyProjectile_BombTorizoChozoOrbs</c>: a Chozo orb fired by Bomb Torizo.</summary>
    BombTorizoChozoOrb = 92,
    /// <summary><c>EnemyProjectile_BombTorizoSonicBoom</c>: Bomb Torizo's sonic-boom projectile.</summary>
    BombTorizoSonicBoom = 93,
    /// <summary><c>EnemyProjectile_GoldenTorizoChozoOrbs</c>: a Chozo orb fired by Golden Torizo.</summary>
    GoldenTorizoChozoOrb = 94,
    /// <summary><c>EnemyProjectile_GoldenTorizoSonicBoom</c>: Golden Torizo's sonic-boom projectile.</summary>
    GoldenTorizoSonicBoom = 95,
    /// <summary><c>EnemyProjectile_TorizoLandingDustCloud_RightFoot</c>: dust from Torizo's right-foot landing.</summary>
    BombTorizoRightFootDust = 96,
    /// <summary><c>EnemyProjectile_TorizoLandingDustCloud_LeftFoot</c>: dust from Torizo's left-foot landing.</summary>
    BombTorizoLeftFootDust = 97,
    /// <summary><c>EnemyProjectile_GoldenTorizoEgg</c>: Golden Torizo's egg projectile.</summary>
    GoldenTorizoEgg = 98,
    /// <summary><c>EnemyProjectile_GoldenTorizoSuperMissile</c>: Golden Torizo's Super Missile projectile.</summary>
    GoldenTorizoSuperMissile = 99,
    /// <summary><c>EnemyProjectile_GoldenTorizoEyeBeam</c>: Golden Torizo's eye beam.</summary>
    GoldenTorizoEyeBeam = 100,
    /// <summary><c>EnemyProjectile_TourianStatueWaterSplash</c>: a water splash during the Tourian statue sequence.</summary>
    TourianStatueSplash = 101,
    /// <summary><c>EnemyProjectile_TourianStatueEyeGlow</c>: a statue eye's unlocking glow.</summary>
    TourianStatueEyeGlow = 102,
    /// <summary><c>EnemyProjectile_TourianStatueUnlockingParticle</c>: an unlocking particle from a Tourian statue.</summary>
    TourianStatueParticle = 103,
    /// <summary><c>EnemyProjectile_TourianStatueUnlockingParticleTail</c>: the trail behind a statue unlocking particle.</summary>
    TourianStatueTail = 104,
    /// <summary><c>EnemyProjectile_TourianStatueSoul</c>: a soul actor in the statue-unlocking sequence.</summary>
    TourianStatueSoul = 105,
    /// <summary><c>EnemyProjectile_TourianStatueRidley</c>: the projectile-backed Ridley statue actor.</summary>
    TourianStatueRidley = 106,
    /// <summary><c>EnemyProjectile_TourianStatuePhantoon</c>: the projectile-backed Phantoon statue actor.</summary>
    TourianStatuePhantoon = 107,
    /// <summary><c>EnemyProjectile_TourianStatueBaseDecoration</c>: decoration at the statue base.</summary>
    TourianStatueBaseDecoration = 108,
    /// <summary><c>EnemyProjectile_WreckedShipChozoSpikeClearingFootsteps</c>: a spike-clearing footstep effect.</summary>
    WreckedShipChozoSpikeFootstep = 109,
    /// <summary><c>UNUSED_EnemyProjectile_SpikeClearingExplosions_86AF76</c>: the alternate unused spike-clearing explosion definition.</summary>
    WreckedShipChozoSpikeFootstepAlternate = 110,
    /// <summary><c>EnemyProjectile_TourianStatueDustClouds</c>: dust emitted during the Tourian statue's descent.</summary>
    TourianStatueDescentDust = 111,
    /// <summary><c>EnemyProjectile_ShaktoolFrontCircle</c>: the front circle of Shaktool's attack.</summary>
    ShaktoolAttackFrontCircle = 112,
    /// <summary><c>EnemyProjectile_ShaktoolMiddleCircle</c>: the middle circle of Shaktool's attack.</summary>
    ShaktoolAttackMiddleCircle = 113,
    /// <summary><c>EnemyProjectile_ShaktoolBackCircle</c>: the back circle of Shaktool's attack.</summary>
    ShaktoolAttackBackCircle = 114,
    /// <summary><c>EnemyProjectile_SporeSpawnStalk</c>: a projectile-backed stalk segment.</summary>
    SporeSpawnStalk = 115,
    /// <summary><c>EnemyProjectile_SporeSpawnSpores</c>: a falling Spore Spawn spore.</summary>
    SporeSpawnSpore = 116,
    /// <summary><c>EnemyProjectile_SporeSpawnSporeSpawner</c>: the actor that schedules new spores.</summary>
    SporeSpawnSpawner = 117,
    /// <summary><c>EnemyProjectile_SaveStationElectricity</c>: electricity anchored to the current save-station PLM.</summary>
    SaveStationElectricity = 118,
    /// <summary>Gate actor spawned when a downward gate begins closing.</summary>
    DownwardGateMoving = 119,
    /// <summary>Gate actor parked at the bottom when a room initially loads.</summary>
    DownwardGateClosed = 120,
    /// <summary><c>EnemyProjectile_NoobTubeCrack</c>: the tube crack that flickers and falls during its destruction.</summary>
    NoobTubeCrack = 121,
    /// <summary><c>EnemyProjectile_NoobTubeShard</c>: a flying shard from the broken Maridia tube.</summary>
    NoobTubeShard = 122,
    /// <summary><c>EnemyProjectile_NoobTubeAirBubbles</c>: an air bubble released by the broken Maridia tube.</summary>
    NoobTubeReleasedAirBubble = 123,
}
