namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What an enemy projectile is. The values are arbitrary (#1452).
/// </summary>
public enum RoomEnemyProjectileKind
{
    /// <summary>No projectile: the projectile is gone or has freed its place, and the place is free for a spawn.</summary>
    None = 0,
    /// <summary>The pre-Phantoon room's background-offset keeper, spawned by the room's setup.</summary>
    PrePhantoonRoom = 1,
    /// <summary>The Maw's projectile-backed body.</summary>
    YappingMawBody = 2,
    /// <summary>Skree's downward-right burst particle.</summary>
    SkreeParticleDownRight = 3,
    /// <summary>Skree's upward-right burst particle.</summary>
    SkreeParticleUpRight = 4,
    /// <summary>Skree's downward-left burst particle.</summary>
    SkreeParticleDownLeft = 5,
    /// <summary>Skree's upward-left burst particle.</summary>
    SkreeParticleUpLeft = 6,
    /// <summary>Metaree's downward-right metal particle.</summary>
    MetareeParticleDownRight = 7,
    /// <summary>Metaree's upward-right metal particle.</summary>
    MetareeParticleUpRight = 8,
    /// <summary>Metaree's downward-left metal particle.</summary>
    MetareeParticleDownLeft = 9,
    /// <summary>Metaree's upward-left metal particle.</summary>
    MetareeParticleUpLeft = 10,
    /// <summary>Crocomire's fired attack projectile.</summary>
    CrocomireProjectile = 11,
    /// <summary>The collapsing bridge's debris actor.</summary>
    CrocomireBridgeCrumbling = 12,
    /// <summary>Fragments of Crocomire's spike wall.</summary>
    CrocomireSpikeWallPieces = 13,
    /// <summary>The rock emitted by Kraid's spit attack.</summary>
    KraidSpitRock = 14,
    /// <summary>Falling ceiling debris in Kraid's room.</summary>
    KraidCeilingRock = 15,
    /// <summary>The left member of Kraid's rising floor-rock pair.</summary>
    KraidRisingRockLeft = 16,
    /// <summary>The right member of Kraid's rising floor-rock pair.</summary>
    KraidRisingRockRight = 17,
    /// <summary>Phantoon's destructible flame actor.</summary>
    PhantoonDestroyableFlame = 18,
    /// <summary>Phantoon's opening flame actor.</summary>
    PhantoonStartingFlame = 19,
    /// <summary>Draygon's goop attack.</summary>
    DraygonGoop = 20,
    /// <summary>The projectile-backed turret in Draygon's room.</summary>
    DraygonWallTurret = 21,
    /// <summary>A Mother Brain room turret actor.</summary>
    MotherBrainRoomTurret = 22,
    /// <summary>A bullet fired by a Mother Brain room turret.</summary>
    MotherBrainRoomTurretBullet = 23,
    /// <summary>A shard from Mother Brain's broken glass.</summary>
    MotherBrainGlassShard = 24,
    /// <summary>A glass-shattering sparkle actor.</summary>
    MotherBrainGlassSparkle = 25,
    /// <summary>Mother Brain's ring-shaped attack.</summary>
    MotherBrainOnionRing = 26,
    /// <summary>The explosion of the Climb's escape-shaft fake wall.</summary>
    OldTourianEscapeShaftFakeWallExplosion = 27,
    /// <summary>Mother Brain's bomb attack.</summary>
    MotherBrainBomb = 28,
    /// <summary>The charging phase of Mother Brain's hand beam.</summary>
    MotherBrainHandBeamCharging = 29,
    /// <summary>The fired phase of Mother Brain's hand beam.</summary>
    MotherBrainHandBeamFired = 30,
    /// <summary>The rainbow beam's charging actor.</summary>
    MotherBrainRainbowBeamCharging = 31,
    /// <summary>Mother Brain's large purple breath projectile.</summary>
    MotherBrainPurpleBreathBig = 32,
    /// <summary>Mother Brain's small purple breath projectile.</summary>
    MotherBrainPurpleBreathSmall = 33,
    /// <summary>Drool emitted during Mother Brain's encounter.</summary>
    MotherBrainDrool = 34,
    /// <summary>Drool emitted during Mother Brain's death.</summary>
    MotherBrainDyingDrool = 35,
    /// <summary>A rainbow-beam explosion actor.</summary>
    MotherBrainRainbowBeamExplosion = 36,
    /// <summary>Mother Brain's body-relative death explosions, which follow the moving body.</summary>
    MotherBrainDeathExplosion = 37,
    /// <summary>A fragment of the opened escape door.</summary>
    MotherBrainEscapeDoorFragment = 38,
    /// <summary>The optional Japanese timebomb subtitle.</summary>
    MotherBrainEscapeSubtitle = 39,
    /// <summary>Mother Brain's falling top-right tube.</summary>
    MotherBrainTopRightTube = 40,
    /// <summary>Mother Brain's falling top-left tube.</summary>
    MotherBrainTopLeftTube = 41,
    /// <summary>Mother Brain's falling middle-left upper tube.</summary>
    MotherBrainTopMiddleLeftTube = 42,
    /// <summary>Mother Brain's falling middle-right upper tube.</summary>
    MotherBrainTopMiddleRightTube = 43,
    /// <summary>Ridley's aimed fireball, shared by Ceres and Zebes.</summary>
    CeresRidleyFireball = 44,
    /// <summary>The stationary center that spawns horizontal afterburns.</summary>
    CeresRidleyHorizontalAfterburnCenter = 45,
    /// <summary>The stationary center that spawns vertical afterburns.</summary>
    CeresRidleyVerticalAfterburnCenter = 46,
    /// <summary>A rightward afterburn and its continuation chain.</summary>
    CeresRidleyHorizontalAfterburnRight = 47,
    /// <summary>A leftward afterburn and its continuation chain.</summary>
    CeresRidleyHorizontalAfterburnLeft = 48,
    /// <summary>An upward afterburn and its continuation chain.</summary>
    CeresRidleyVerticalAfterburnUp = 49,
    /// <summary>A downward afterburn and its continuation chain.</summary>
    CeresRidleyVerticalAfterburnDown = 50,
    /// <summary>The light variant of Ceres falling debris.</summary>
    CeresFallingDebrisLight = 51,
    /// <summary>The dark variant of Ceres falling debris.</summary>
    CeresFallingDebrisDark = 52,
    /// <summary>Dust emitted during gunship liftoff.</summary>
    GunshipLiftoffDustCloud = 53,
    /// <summary>Alcoon's fireball attack.</summary>
    AlcoonFireball = 54,
    /// <summary>The spike actor emitted by Powamp.</summary>
    PowampSpike = 55,
    /// <summary>A Work Robot laser directed upward and left.</summary>
    WorkRobotLaserUpLeft = 56,
    /// <summary>The Work Robot's horizontal laser definition.</summary>
    WorkRobotLaserHorizontal = 57,
    /// <summary>A Work Robot laser directed downward and left.</summary>
    WorkRobotLaserDownLeft = 58,
    /// <summary>A Work Robot laser directed upward and right.</summary>
    WorkRobotLaserUpRight = 59,
    /// <summary>A Work Robot laser directed downward and right.</summary>
    WorkRobotLaserDownRight = 60,
    /// <summary>A falling spark actor.</summary>
    FallingSpark = 61,
    /// <summary>A persistent projectile-backed Nuclear Waffle body link.</summary>
    NuclearWaffleBody = 62,
    /// <summary>Fake Kraid's spit projectile.</summary>
    FakeKraidSpit = 63,
    /// <summary>Fake Kraid's left spike projectile.</summary>
    FakeKraidSpikeLeft = 64,
    /// <summary>Fake Kraid's right spike projectile.</summary>
    FakeKraidSpikeRight = 65,
    /// <summary>The left-facing Ki-Hunter acid-spit definition.</summary>
    KiHunterAcidSpitLeft = 66,
    /// <summary>The right-facing Ki-Hunter acid-spit definition.</summary>
    KiHunterAcidSpitRight = 67,
    /// <summary>The Space Pirate laser used in Mother Brain's room.</summary>
    PirateMotherBrainLaser = 68,
    /// <summary>A Space Pirate claw projectile.</summary>
    PirateClaw = 69,
    /// <summary>The rock actor emitted by Polyp.</summary>
    PolypRock = 70,
    /// <summary>A Cacatac's directional spike.</summary>
    CacatacSpike = 71,
    /// <summary>Namihe's fireball definition.</summary>
    NamiheFireball = 73,
    /// <summary>Fune's fireball definition.</summary>
    FuneFireball = 74,
    /// <summary>Lava thrown by Magdollite.</summary>
    LavaThrownByMagdollite = 75,
    /// <summary>Dragon's fireball attack.</summary>
    DragonFireball = 76,
    /// <summary>The Eye Door's aimed projectile.</summary>
    EyeDoorProjectile = 77,
    /// <summary>The Eye Door's sweat actor.</summary>
    EyeDoorSweat = 78,
    /// <summary>The room-object-spawned smoke actor used by Eye Doors.</summary>
    EyeDoorSmoke = 79,
    /// <summary>A parameter-selected dust or explosion effect.</summary>
    MiscDustExplosion = 80,
    /// <summary>An enemy drop with its pickup animation and collection behavior.</summary>
    EnemyDeathPickup = 81,
    /// <summary>An enemy-death explosion that can publish drops or rebuild an enemy slot.</summary>
    EnemyDeathExplosion = 82,
    /// <summary>A destructible bug released by Kago.</summary>
    KagoBug = 83,
    /// <summary>A body segment following Botwoon's stored trajectory.</summary>
    BotwoonBody = 84,
    /// <summary>Botwoon's angled spit projectile.</summary>
    BotwoonSpit = 85,
    /// <summary>Continuous drool from low-health Bomb Torizo.</summary>
    BombTorizoLowHealthDrool = 86,
    /// <summary>Bomb Torizo's initial drool actor.</summary>
    BombTorizoInitialDrool = 87,
    /// <summary>The projectile actor for Bomb Torizo's explosive swipe.</summary>
    BombTorizoExplosiveSwipe = 88,
    /// <summary>A low-health Bomb Torizo explosion.</summary>
    BombTorizoLowHealthExplosion = 89,
    /// <summary>An explosion during Bomb Torizo's death sequence.</summary>
    BombTorizoDeathExplosion = 90,
    /// <summary>A fragment effect from Bomb Torizo's breaking statue.</summary>
    BombTorizoStatueBreaking = 91,
    /// <summary>A Chozo orb fired by Bomb Torizo.</summary>
    BombTorizoChozoOrb = 92,
    /// <summary>Bomb Torizo's sonic-boom projectile.</summary>
    BombTorizoSonicBoom = 93,
    /// <summary>A Chozo orb fired by Golden Torizo.</summary>
    GoldenTorizoChozoOrb = 94,
    /// <summary>Golden Torizo's sonic-boom projectile.</summary>
    GoldenTorizoSonicBoom = 95,
    /// <summary>Dust from Torizo's right-foot landing.</summary>
    BombTorizoRightFootDust = 96,
    /// <summary>Dust from Torizo's left-foot landing.</summary>
    BombTorizoLeftFootDust = 97,
    /// <summary>Golden Torizo's egg projectile.</summary>
    GoldenTorizoEgg = 98,
    /// <summary>Golden Torizo's Super Missile projectile.</summary>
    GoldenTorizoSuperMissile = 99,
    /// <summary>Golden Torizo's eye beam.</summary>
    GoldenTorizoEyeBeam = 100,
    /// <summary>A water splash during the Tourian statue sequence.</summary>
    TourianStatueSplash = 101,
    /// <summary>A statue eye's unlocking glow.</summary>
    TourianStatueEyeGlow = 102,
    /// <summary>An unlocking particle from a Tourian statue.</summary>
    TourianStatueParticle = 103,
    /// <summary>The trail behind a statue unlocking particle.</summary>
    TourianStatueTail = 104,
    /// <summary>A soul actor in the statue-unlocking sequence.</summary>
    TourianStatueSoul = 105,
    /// <summary>The projectile-backed Ridley statue actor.</summary>
    TourianStatueRidley = 106,
    /// <summary>The projectile-backed Phantoon statue actor.</summary>
    TourianStatuePhantoon = 107,
    /// <summary>Decoration at the statue base.</summary>
    TourianStatueBaseDecoration = 108,
    /// <summary>A spike-clearing footstep effect.</summary>
    WreckedShipChozoSpikeFootstep = 109,
    /// <summary>The alternate unused spike-clearing explosion definition.</summary>
    WreckedShipChozoSpikeFootstepAlternate = 110,
    /// <summary>Dust emitted during the Tourian statue's descent.</summary>
    TourianStatueDescentDust = 111,
    /// <summary>The front circle of Shaktool's attack.</summary>
    ShaktoolAttackFrontCircle = 112,
    /// <summary>The middle circle of Shaktool's attack.</summary>
    ShaktoolAttackMiddleCircle = 113,
    /// <summary>The back circle of Shaktool's attack.</summary>
    ShaktoolAttackBackCircle = 114,
    /// <summary>A projectile-backed stalk segment.</summary>
    SporeSpawnStalk = 115,
    /// <summary>A falling Spore Spawn spore.</summary>
    SporeSpawnSpore = 116,
    /// <summary>The actor that schedules new spores.</summary>
    SporeSpawnSpawner = 117,
    /// <summary>Electricity anchored to the current save-station room object.</summary>
    SaveStationElectricity = 118,
    /// <summary>Gate actor spawned when a downward gate begins closing.</summary>
    DownwardGateMoving = 119,
    /// <summary>Gate actor parked at the bottom when a room initially loads.</summary>
    DownwardGateClosed = 120,
    /// <summary>The tube crack that flickers and falls during its destruction.</summary>
    NoobTubeCrack = 121,
    /// <summary>A flying shard from the broken Maridia tube.</summary>
    NoobTubeShard = 122,
    /// <summary>An air bubble released by the broken Maridia tube.</summary>
    NoobTubeReleasedAirBubble = 123,
}
