using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The bank-$86 definition pointer occupying one live room-enemy projectile slot. Keeping
/// the cartridge pointer as the enum value makes debugger state directly comparable with
/// <c>eproj_id</c> rather than inventing a host-only identity layer.
/// </summary>
public enum RoomEnemyProjectileKind : ushort
{
    /// <summary>Zero in <c>eproj_id</c>; the physical projectile slot is available for allocation.</summary>
    None = 0,
    /// <summary>$86:A3B0, the pre-Phantoon room's BG2-offset keeper spawned by setup ASM $8F:C8C8.</summary>
    PrePhantoonRoom = 0xa3b0,
    /// <summary><c>$86:EC95 EnemyProjectile_YappingMawsBody</c>: the Maw's projectile-backed body.</summary>
    YappingMawBody = 0xec95,
    /// <summary><c>$86:8BC2 EnemyProjectile_SkreeParticles_DownRight</c>: Skree's downward-right burst particle.</summary>
    SkreeParticleDownRight = 0x8bc2,
    /// <summary><c>$86:8BD0 EnemyProjectile_SkreeParticles_UpRight</c>: Skree's upward-right burst particle.</summary>
    SkreeParticleUpRight = 0x8bd0,
    /// <summary><c>$86:8BDE EnemyProjectile_SkreeParticles_DownLeft</c>: Skree's downward-left burst particle.</summary>
    SkreeParticleDownLeft = 0x8bde,
    /// <summary><c>$86:8BEC EnemyProjectile_SkreeParticles_UpLeft</c>: Skree's upward-left burst particle.</summary>
    SkreeParticleUpLeft = 0x8bec,
    /// <summary><c>$86:8BFA EnemyProjectile_MetalSkreeParticles_DownRight</c>: Metaree's downward-right metal particle.</summary>
    MetareeParticleDownRight = 0x8bfa,
    /// <summary><c>$86:8C08 EnemyProjectile_MetalSkreeParticles_UpRight</c>: Metaree's upward-right metal particle.</summary>
    MetareeParticleUpRight = 0x8c08,
    /// <summary><c>$86:8C16 EnemyProjectile_MetalSkreeParticles_DownLeft</c>: Metaree's downward-left metal particle.</summary>
    MetareeParticleDownLeft = 0x8c16,
    /// <summary><c>$86:8C24 EnemyProjectile_MetalSkreeParticles_UpLeft</c>: Metaree's upward-left metal particle.</summary>
    MetareeParticleUpLeft = 0x8c24,
    /// <summary><c>$86:8F8F EnemyProjectile_CrocomiresProjectile</c>: Crocomire's fired attack projectile.</summary>
    CrocomireProjectile = 0x8f8f,
    /// <summary><c>$86:8F9D EnemyProjectile_CrocomireBridgeCrumbling</c>: the collapsing bridge's debris actor.</summary>
    CrocomireBridgeCrumbling = 0x8f9d,
    /// <summary><c>$86:90C1 EnemyProjectile_CrocomireSpikeWallPieces</c>: fragments of Crocomire's spike wall.</summary>
    CrocomireSpikeWallPieces = 0x90c1,
    /// <summary><c>$86:9C45 EnemyProjectile_KraidRockSpit</c>: the rock emitted by Kraid's spit attack.</summary>
    KraidSpitRock = 0x9c45,
    /// <summary><c>$86:9C53 EnemyProjectile_KraidCeilingRocks</c>: falling ceiling debris in Kraid's room.</summary>
    KraidCeilingRock = 0x9c53,
    /// <summary><c>$86:9C61 EnemyProjectile_KraidFloorRocks_Left</c>: the left member of Kraid's rising floor-rock pair.</summary>
    KraidRisingRockLeft = 0x9c61,
    /// <summary><c>$86:9C6F EnemyProjectile_KraidFloorRocks_Right</c>: the right member of Kraid's rising floor-rock pair.</summary>
    KraidRisingRockRight = 0x9c6f,
    /// <summary><c>$86:9C29 EnemyProjectile_PhantoonDestroyableFlames</c>: Phantoon's destructible flame actor.</summary>
    PhantoonDestroyableFlame = 0x9c29,
    /// <summary><c>$86:9C37 EnemyProjectile_PhantoonStartingFlames</c>: Phantoon's opening flame actor.</summary>
    PhantoonStartingFlame = 0x9c37,
    /// <summary><c>$86:8E50 EnemyProjectile_DraygonGoop</c>: Draygon's goop attack.</summary>
    DraygonGoop = 0x8e50,
    /// <summary><c>$86:8E5E EnemyProjectile_DraygonWallTurret</c>: the projectile-backed turret in Draygon's room.</summary>
    DraygonWallTurret = 0x8e5e,
    /// <summary><c>$86:C17E EnemyProjectile_MotherBrainTurret</c>: a Mother Brain room turret actor.</summary>
    MotherBrainRoomTurret = 0xc17e,
    /// <summary><c>$86:C18C EnemyProjectile_MotherBrainTurretBullets</c>: a bullet fired by a Mother Brain room turret.</summary>
    MotherBrainRoomTurretBullet = 0xc18c,
    /// <summary><c>$86:CEFC EnemyProjectile_MotherBrainGlassShattering_Shard</c>: a shard from Mother Brain's broken glass.</summary>
    MotherBrainGlassShard = 0xcefc,
    /// <summary><c>$86:CF0A EnemyProjectile_MotherBrainGlassShattering_Sparkle</c>: a glass-shattering sparkle actor.</summary>
    MotherBrainGlassSparkle = 0xcf0a,
    /// <summary><c>$86:CB4B EnemyProjectile_MotherBrainOnionRings</c>: Mother Brain's ring-shaped attack.</summary>
    MotherBrainOnionRing = 0xcb4b,
    /// <summary>$86:B4B1: the explosion of the Climb's escape-shaft fake wall.</summary>
    OldTourianEscapeShaftFakeWallExplosion = 0xb4b1,
    /// <summary><c>$86:CB59 EnemyProjectile_MotherBrainBomb</c>: Mother Brain's bomb attack.</summary>
    MotherBrainBomb = 0xcb59,
    /// <summary><c>$86:CB67 EnemyProjectile_MotherBrainRedBeam_Charging</c>: the charging phase of Mother Brain's hand beam.</summary>
    MotherBrainHandBeamCharging = 0xcb67,
    /// <summary><c>$86:CB75 EnemyProjectile_MotherBrainRedBeam_Fired</c>: the fired phase of Mother Brain's hand beam.</summary>
    MotherBrainHandBeamFired = 0xcb75,
    /// <summary><c>$86:CB83 EnemyProjectile_MotherBrainRainbowBeam_Charging</c>: the rainbow beam's charging actor.</summary>
    MotherBrainRainbowBeamCharging = 0xcb83,
    /// <summary><c>$86:CB2F EnemyProjectile_MotherBrainPurpleBreath_Big</c>: Mother Brain's large purple breath projectile.</summary>
    MotherBrainPurpleBreathBig = 0xcb2f,
    /// <summary>$86:CB3D: Mother Brain's small purple breath projectile.</summary>
    MotherBrainPurpleBreathSmall = 0xcb3d,
    /// <summary><c>$86:CB91 EnemyProjectile_MotherBrainDrool</c>: drool emitted during Mother Brain's encounter.</summary>
    MotherBrainDrool = 0xcb91,
    /// <summary><c>$86:CB9F EnemyProjectile_MotherBrainDyingDrool</c>: drool emitted during Mother Brain's death.</summary>
    MotherBrainDyingDrool = 0xcb9f,
    /// <summary><c>$86:CBAD EnemyProjectile_MotherBrainRainbowBeam_Explosion</c>: a rainbow-beam explosion actor.</summary>
    MotherBrainRainbowBeamExplosion = 0xcbad,
    /// <summary>$86:CB13 body-relative death explosions; C914 follows the moving body.</summary>
    MotherBrainDeathExplosion = MotherBrainDeathRomData.ExplosionDefinition,
    /// <summary><c>$86:CB21 EnemyProjectile_MotherBrainExplodedEscapeDoorParticles</c>: a fragment of the opened escape door.</summary>
    MotherBrainEscapeDoorFragment = MotherBrainDeathRomData.DoorFragmentDefinition,
    /// <summary><c>$86:CBBB EnemyProjectile_TimeBombSetSubtitle</c>: the optional Japanese timebomb subtitle.</summary>
    MotherBrainEscapeSubtitle = MotherBrainDeathRomData.SubtitleDefinition,
    /// <summary><c>$86:CC5B EnemyProjectile_MotherBrainTubeFalling_TopRight</c>: Mother Brain's falling top-right tube.</summary>
    MotherBrainTopRightTube = 0xcc5b,
    /// <summary><c>$86:CC69 EnemyProjectile_MotherBrainTubeFalling_TopLeft</c>: Mother Brain's falling top-left tube.</summary>
    MotherBrainTopLeftTube = 0xcc69,
    /// <summary><c>$86:CC77 EnemyProjectile_MotherBrainTubeFalling_TopMiddleLeft</c>: Mother Brain's falling middle-left upper tube.</summary>
    MotherBrainTopMiddleLeftTube = 0xcc77,
    /// <summary><c>$86:CC85 EnemyProjectile_MotherBrainTubeFalling_TopMiddleRight</c>: Mother Brain's falling middle-right upper tube.</summary>
    MotherBrainTopMiddleRightTube = 0xcc85,
    /// <summary><c>$86:9642 EnemyProjectile_RidleysFireball</c>: Ridley's aimed fireball, shared by Ceres and Zebes.</summary>
    CeresRidleyFireball = 0x9642,
    /// <summary><c>$86:9650 EnemyProjectile_RidleyHorizontalAfterburn_Center</c>: the stationary center that spawns horizontal afterburns.</summary>
    CeresRidleyHorizontalAfterburnCenter = 0x9650,
    /// <summary><c>$86:965E EnemyProjectile_RidleyVerticalAfterburn_Center</c>: the stationary center that spawns vertical afterburns.</summary>
    CeresRidleyVerticalAfterburnCenter = 0x965e,
    /// <summary><c>$86:966C EnemyProjectile_RidleyHorizontalAfterburn_Right</c>: a rightward afterburn and its continuation chain.</summary>
    CeresRidleyHorizontalAfterburnRight = 0x966c,
    /// <summary><c>$86:967A EnemyProjectile_RidleyHorizontalAfterburn_Left</c>: a leftward afterburn and its continuation chain.</summary>
    CeresRidleyHorizontalAfterburnLeft = 0x967a,
    /// <summary><c>$86:9688 EnemyProjectile_RidleyVerticalAfterburn_Up</c>: an upward afterburn and its continuation chain.</summary>
    CeresRidleyVerticalAfterburnUp = 0x9688,
    /// <summary><c>$86:9696 EnemyProjectile_RidleyVerticalAfterburn_Down</c>: a downward afterburn and its continuation chain.</summary>
    CeresRidleyVerticalAfterburnDown = 0x9696,
    /// <summary><c>$86:9734 EnemyProjectile_CeresFallingTile_Light</c>: the light palette variant of Ceres falling debris.</summary>
    CeresFallingDebrisLight = 0x9734,
    /// <summary><c>$86:9742 EnemyProjectile_CeresFallingTile_Dark</c>: the dark palette variant of Ceres falling debris.</summary>
    CeresFallingDebrisDark = 0x9742,
    /// <summary><c>$86:A379 EnemyProjectile_GunShipLiftoffDustClouds</c>: dust emitted during gunship liftoff.</summary>
    GunshipLiftoffDustCloud = 0xa379,
    /// <summary><c>$86:9E90 EnemyProjectile_AlcoonFireball</c>: Alcoon's fireball attack.</summary>
    AlcoonFireball = 0x9e90,
    /// <summary><c>$86:D298 EnemyProjectile_Powamp</c>: the spike actor emitted by Powamp.</summary>
    PowampSpike = 0xd298,
    /// <summary><c>$86:D2A6 EnemyProjectile_RobotLaser_UpLeft</c>: a Work Robot laser directed upward and left.</summary>
    WorkRobotLaserUpLeft = 0xd2a6,
    /// <summary><c>$86:D2B4 EnemyProjectile_RobotLaser_Horizontal</c>: the Work Robot's horizontal laser definition.</summary>
    WorkRobotLaserHorizontal = 0xd2b4,
    /// <summary><c>$86:D2C2 EnemyProjectile_RobotLaser_DownLeft</c>: a Work Robot laser directed downward and left.</summary>
    WorkRobotLaserDownLeft = 0xd2c2,
    /// <summary><c>$86:D2D0 EnemyProjectile_RobotLaser_UpRight</c>: a Work Robot laser directed upward and right.</summary>
    WorkRobotLaserUpRight = 0xd2d0,
    /// <summary><c>$86:D2DE EnemyProjectile_RobotLaser_DownRight</c>: a Work Robot laser directed downward and right.</summary>
    WorkRobotLaserDownRight = 0xd2de,
    /// <summary><c>$86:F498 EnemyProjectile_FallingSpark</c>: a falling spark actor.</summary>
    FallingSpark = 0xf498,
    /// <summary><c>$86:BBC7 EnemyProjectile_Puromi</c>: a persistent projectile-backed Nuclear Waffle body link.</summary>
    NuclearWaffleBody = 0xbbc7,
    /// <summary><c>$86:9DB0 EnemyProjectile_MiniKraidSpit</c>: Fake Kraid's spit projectile.</summary>
    FakeKraidSpit = 0x9db0,
    /// <summary><c>$86:9DBE EnemyProjectile_MiniKraidSpikes_Left</c>: Fake Kraid's left spike projectile.</summary>
    FakeKraidSpikeLeft = 0x9dbe,
    /// <summary><c>$86:9DCC EnemyProjectile_MiniKraidSpikes_Right</c>: Fake Kraid's right spike projectile.</summary>
    FakeKraidSpikeRight = 0x9dcc,
    /// <summary><c>$86:CF18 EnemyProjectile_KiHunterAcidSpit_Left</c>: the left-facing Ki-Hunter acid-spit definition.</summary>
    KiHunterAcidSpitLeft = 0xcf18,
    /// <summary><c>$86:CF26 EnemyProjectile_KiHunterAcidSpit_Right</c>: the right-facing Ki-Hunter acid-spit definition.</summary>
    KiHunterAcidSpitRight = 0xcf26,
    /// <summary><c>$86:A17B EnemyProjectile_PirateMotherBrainLaser</c>: the Space Pirate laser used in Mother Brain's room.</summary>
    PirateMotherBrainLaser = 0xa17b,
    /// <summary><c>$86:A189 EnemyProjectile_PirateClaw</c>: a Space Pirate claw projectile.</summary>
    PirateClaw = 0xa189,
    /// <summary><c>$86:BD5A EnemyProjectile_LavaquakeRocks</c>: the rock actor emitted by Polyp.</summary>
    PolypRock = 0xbd5a,
    /// <summary><c>$86:DAFE EnemyProjectile_Cacatac</c>: a Cacatac's directional spike.</summary>
    CacatacSpike = 0xdafe,
    /// <summary><c>$86:DBF2 UNUSED_EnemyProjectile_Stoke_86DBF2</c>: the native unused mini-Crocomire/Stoke projectile definition.</summary>
    StokeProjectile = 0xdbf2,
    /// <summary><c>$86:DFBC EnemyProjectile_NamiheFireball</c>: Namihe's fireball definition.</summary>
    NamiheFireball = 0xdfbc,
    /// <summary><c>$86:DFCA EnemyProjectile_FuneFireball</c>: Fune's fireball definition.</summary>
    FuneFireball = 0xdfca,
    /// <summary><c>$86:E0E0 EnemyProjectile_Magdollite</c>: lava thrown by Magdollite.</summary>
    LavaThrownByMagdollite = 0xe0e0,
    /// <summary><c>$86:B5CB EnemyProjectile_DragonFireball</c>: Dragon's fireball attack.</summary>
    DragonFireball = 0xb5cb,
    /// <summary><c>$86:B743 EnemyProjectile_EyeDoorProjectile</c>: the Eye Door's aimed projectile.</summary>
    EyeDoorProjectile = 0xb743,
    /// <summary><c>$86:B751 EnemyProjectile_EyeDoorSweat</c>: the Eye Door's sweat actor.</summary>
    EyeDoorSweat = 0xb751,
    /// <summary><c>$86:E517 EnemyProjectile_MiscDustPLM</c>: the PLM-spawned smoke actor used by Eye Doors.</summary>
    EyeDoorSmoke = 0xe517,
    /// <summary><c>$86:E509 EnemyProjectile_MiscDust</c>: a parameter-selected dust or explosion effect.</summary>
    MiscDustExplosion = 0xe509,
    /// <summary><c>$86:F337 EnemyProjectile_EnemyDeathPickup</c>: an enemy drop with its pickup animation and collection behavior.</summary>
    EnemyDeathPickup = 0xf337,
    /// <summary><c>$86:F345 EnemyProjectile_EnemyDeathExplosion</c>: an enemy-death explosion that can publish drops or rebuild an enemy slot.</summary>
    EnemyDeathExplosion = 0xf345,
    /// <summary><c>$86:D02E EnemyProjectile_KagoBug</c>: a destructible bug released by Kago.</summary>
    KagoBug = 0xd02e,
    /// <summary><c>$86:EBA0 EnemyProjectile_BotwoonsBody</c>: a body segment following Botwoon's stored trajectory.</summary>
    BotwoonBody = 0xeba0,
    /// <summary><c>$86:EC48 EnemyProjectile_BotwoonsSpit</c>: Botwoon's angled spit projectile.</summary>
    BotwoonSpit = 0xec48,
    /// <summary><c>$86:A95B EnemyProjectile_BombTorizoContinuousDrool</c>: continuous drool from low-health Bomb Torizo.</summary>
    BombTorizoLowHealthDrool = 0xa95b,
    /// <summary><c>$86:A969 EnemyProjectile_BombTorizoInitialDrool</c>: Bomb Torizo's initial drool actor.</summary>
    BombTorizoInitialDrool = 0xa969,
    /// <summary><c>$86:A985 EnemyProjectile_BombTorizoExplosiveSwipe</c>: the projectile actor for Bomb Torizo's explosive swipe.</summary>
    BombTorizoExplosiveSwipe = 0xa985,
    /// <summary><c>$86:A9A1 EnemyProjectile_BombTorizoLowHealthExplosion</c>: a low-health Bomb Torizo explosion.</summary>
    BombTorizoLowHealthExplosion = 0xa9a1,
    /// <summary><c>$86:A9AF EnemyProjectile_BombTorizoDeathExplosion</c>: an explosion during Bomb Torizo's death sequence.</summary>
    BombTorizoDeathExplosion = 0xa9af,
    /// <summary><c>$86:A993 EnemyProjectile_BombTorizoStatueBreaking</c>: a fragment effect from Bomb Torizo's breaking statue.</summary>
    BombTorizoStatueBreaking = 0xa993,
    /// <summary><c>$86:AD5E EnemyProjectile_BombTorizoChozoOrbs</c>: a Chozo orb fired by Bomb Torizo.</summary>
    BombTorizoChozoOrb = 0xad5e,
    /// <summary><c>$86:AEA8 EnemyProjectile_BombTorizoSonicBoom</c>: Bomb Torizo's sonic-boom projectile.</summary>
    BombTorizoSonicBoom = 0xaea8,
    /// <summary><c>$86:AD7A EnemyProjectile_GoldenTorizoChozoOrbs</c>: a Chozo orb fired by Golden Torizo.</summary>
    GoldenTorizoChozoOrb = 0xad7a,
    /// <summary><c>$86:AEB6 EnemyProjectile_GoldenTorizoSonicBoom</c>: Golden Torizo's sonic-boom projectile.</summary>
    GoldenTorizoSonicBoom = 0xaeb6,
    /// <summary><c>$86:AFE5 EnemyProjectile_TorizoLandingDustCloud_RightFoot</c>: dust from Torizo's right-foot landing.</summary>
    BombTorizoRightFootDust = 0xafe5,
    /// <summary><c>$86:AFF3 EnemyProjectile_TorizoLandingDustCloud_LeftFoot</c>: dust from Torizo's left-foot landing.</summary>
    BombTorizoLeftFootDust = 0xaff3,
    /// <summary><c>$86:B1C0 EnemyProjectile_GoldenTorizoEgg</c>: Golden Torizo's egg projectile.</summary>
    GoldenTorizoEgg = 0xb1c0,
    /// <summary><c>$86:B31A EnemyProjectile_GoldenTorizoSuperMissile</c>: Golden Torizo's Super Missile projectile.</summary>
    GoldenTorizoSuperMissile = 0xb31a,
    /// <summary><c>$86:B428 EnemyProjectile_GoldenTorizoEyeBeam</c>: Golden Torizo's eye beam.</summary>
    GoldenTorizoEyeBeam = 0xb428,
    /// <summary><c>$86:BA5C EnemyProjectile_TourianStatueWaterSplash</c>: a water splash during the Tourian statue sequence.</summary>
    TourianStatueSplash = 0xba5c,
    /// <summary><c>$86:BA6A EnemyProjectile_TourianStatueEyeGlow</c>: a statue eye's unlocking glow.</summary>
    TourianStatueEyeGlow = 0xba6a,
    /// <summary><c>$86:BA78 EnemyProjectile_TourianStatueUnlockingParticle</c>: an unlocking particle from a Tourian statue.</summary>
    TourianStatueParticle = 0xba78,
    /// <summary><c>$86:BA86 EnemyProjectile_TourianStatueUnlockingParticleTail</c>: the trail behind a statue unlocking particle.</summary>
    TourianStatueTail = 0xba86,
    /// <summary><c>$86:BA94 EnemyProjectile_TourianStatueSoul</c>: a soul actor in the statue-unlocking sequence.</summary>
    TourianStatueSoul = 0xba94,
    /// <summary><c>$86:BAA2 EnemyProjectile_TourianStatueRidley</c>: the projectile-backed Ridley statue actor.</summary>
    TourianStatueRidley = 0xbaa2,
    /// <summary><c>$86:BAB0 EnemyProjectile_TourianStatuePhantoon</c>: the projectile-backed Phantoon statue actor.</summary>
    TourianStatuePhantoon = 0xbab0,
    /// <summary><c>$86:BABE EnemyProjectile_TourianStatueBaseDecoration</c>: decoration at the statue base.</summary>
    TourianStatueBaseDecoration = 0xbabe,
    /// <summary><c>$86:AF68 EnemyProjectile_WreckedShipChozoSpikeClearingFootsteps</c>: a spike-clearing footstep effect.</summary>
    WreckedShipChozoSpikeFootstep = 0xaf68,
    /// <summary><c>$86:AF76 UNUSED_EnemyProjectile_SpikeClearingExplosions_86AF76</c>: the alternate unused spike-clearing explosion definition.</summary>
    WreckedShipChozoSpikeFootstepAlternate = 0xaf76,
    /// <summary><c>$86:AF84 EnemyProjectile_TourianStatueDustClouds</c>: dust emitted during the Tourian statue's descent.</summary>
    TourianStatueDescentDust = 0xaf84,
    /// <summary><c>$86:BE25 EnemyProjectile_ShaktoolFrontCircle</c>: the front circle of Shaktool's attack.</summary>
    ShaktoolAttackFrontCircle = 0xbe25,
    /// <summary><c>$86:BE33 EnemyProjectile_ShaktoolMiddleCircle</c>: the middle circle of Shaktool's attack.</summary>
    ShaktoolAttackMiddleCircle = 0xbe33,
    /// <summary><c>$86:BE41 EnemyProjectile_ShaktoolBackCircle</c>: the back circle of Shaktool's attack.</summary>
    ShaktoolAttackBackCircle = 0xbe41,
    /// <summary><c>$86:DE6C EnemyProjectile_SporeSpawnStalk</c>: a projectile-backed stalk segment.</summary>
    SporeSpawnStalk = 0xde6c,
    /// <summary><c>$86:DE7A EnemyProjectile_SporeSpawnSpores</c>: a falling Spore Spawn spore.</summary>
    SporeSpawnSpore = 0xde7a,
    /// <summary><c>$86:DE88 EnemyProjectile_SporeSpawnSporeSpawner</c>: the actor that schedules new spores.</summary>
    SporeSpawnSpawner = 0xde88,
    /// <summary><c>$86:E6D2 EnemyProjectile_SaveStationElectricity</c>: electricity anchored to the current save-station PLM.</summary>
    SaveStationElectricity = 0xe6d2,
    /// <summary>Gate actor spawned when a downward gate begins closing.</summary>
    DownwardGateMoving = 0xe64b,
    /// <summary>Gate actor parked at the bottom when a room initially loads.</summary>
    DownwardGateClosed = 0xe659,
    /// <summary><c>$86:D904 EnemyProjectile_NoobTubeCrack</c>: the tube crack that flickers and falls during its destruction.</summary>
    NoobTubeCrack = 0xd904,
    /// <summary><c>$86:D912 EnemyProjectile_NoobTubeShard</c>: a flying shard from the broken Maridia tube.</summary>
    NoobTubeShard = 0xd912,
    /// <summary><c>$86:D920 EnemyProjectile_NoobTubeAirBubbles</c>: an air bubble released by the broken Maridia tube.</summary>
    NoobTubeReleasedAirBubble = 0xd920,
}
