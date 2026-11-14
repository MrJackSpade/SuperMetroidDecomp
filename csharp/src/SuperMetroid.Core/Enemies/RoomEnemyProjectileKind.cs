namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What an enemy projectile is.
/// </summary>
public enum RoomEnemyProjectileKind
{
    /// <summary>No projectile: the projectile is gone or has freed its place, and the place is free for a spawn.</summary>
    None,
    /// <summary>The pre-Phantoon room's background-offset keeper, spawned by the room's setup.</summary>
    PrePhantoonRoom,
    /// <summary>The Maw's projectile-backed body.</summary>
    YappingMawBody,
    /// <summary>Skree's downward-right burst particle.</summary>
    SkreeParticleDownRight,
    /// <summary>Skree's upward-right burst particle.</summary>
    SkreeParticleUpRight,
    /// <summary>Skree's downward-left burst particle.</summary>
    SkreeParticleDownLeft,
    /// <summary>Skree's upward-left burst particle.</summary>
    SkreeParticleUpLeft,
    /// <summary>Metaree's downward-right metal particle.</summary>
    MetareeParticleDownRight,
    /// <summary>Metaree's upward-right metal particle.</summary>
    MetareeParticleUpRight,
    /// <summary>Metaree's downward-left metal particle.</summary>
    MetareeParticleDownLeft,
    /// <summary>Metaree's upward-left metal particle.</summary>
    MetareeParticleUpLeft,
    /// <summary>Crocomire's fired attack projectile.</summary>
    CrocomireProjectile,
    /// <summary>The collapsing bridge's debris actor.</summary>
    CrocomireBridgeCrumbling,
    /// <summary>Fragments of Crocomire's spike wall.</summary>
    CrocomireSpikeWallPieces,
    /// <summary>The rock emitted by Kraid's spit attack.</summary>
    KraidSpitRock,
    /// <summary>Falling ceiling debris in Kraid's room.</summary>
    KraidCeilingRock,
    /// <summary>The left member of Kraid's rising floor-rock pair.</summary>
    KraidRisingRockLeft,
    /// <summary>The right member of Kraid's rising floor-rock pair.</summary>
    KraidRisingRockRight,
    /// <summary>Phantoon's destructible flame actor.</summary>
    PhantoonDestroyableFlame,
    /// <summary>Phantoon's opening flame actor.</summary>
    PhantoonStartingFlame,
    /// <summary>Draygon's goop attack.</summary>
    DraygonGoop,
    /// <summary>The projectile-backed turret in Draygon's room.</summary>
    DraygonWallTurret,
    /// <summary>A Mother Brain room turret actor.</summary>
    MotherBrainRoomTurret,
    /// <summary>A bullet fired by a Mother Brain room turret.</summary>
    MotherBrainRoomTurretBullet,
    /// <summary>A shard from Mother Brain's broken glass.</summary>
    MotherBrainGlassShard,
    /// <summary>A glass-shattering sparkle actor.</summary>
    MotherBrainGlassSparkle,
    /// <summary>Mother Brain's ring-shaped attack.</summary>
    MotherBrainOnionRing,
    /// <summary>The explosion of the Climb's escape-shaft fake wall.</summary>
    OldTourianEscapeShaftFakeWallExplosion,
    /// <summary>Mother Brain's bomb attack.</summary>
    MotherBrainBomb,
    /// <summary>The charging phase of Mother Brain's hand beam.</summary>
    MotherBrainHandBeamCharging,
    /// <summary>The fired phase of Mother Brain's hand beam.</summary>
    MotherBrainHandBeamFired,
    /// <summary>The rainbow beam's charging actor.</summary>
    MotherBrainRainbowBeamCharging,
    /// <summary>Mother Brain's large purple breath projectile.</summary>
    MotherBrainPurpleBreathBig,
    /// <summary>Mother Brain's small purple breath projectile.</summary>
    MotherBrainPurpleBreathSmall,
    /// <summary>Drool emitted during Mother Brain's encounter.</summary>
    MotherBrainDrool,
    /// <summary>Drool emitted during Mother Brain's death.</summary>
    MotherBrainDyingDrool,
    /// <summary>A rainbow-beam explosion actor.</summary>
    MotherBrainRainbowBeamExplosion,
    /// <summary>Mother Brain's body-relative death explosions, which follow the moving body.</summary>
    MotherBrainDeathExplosion,
    /// <summary>A fragment of the opened escape door.</summary>
    MotherBrainEscapeDoorFragment,
    /// <summary>The optional Japanese timebomb subtitle.</summary>
    MotherBrainEscapeSubtitle,
    /// <summary>Mother Brain's falling top-right tube.</summary>
    MotherBrainTopRightTube,
    /// <summary>Mother Brain's falling top-left tube.</summary>
    MotherBrainTopLeftTube,
    /// <summary>Mother Brain's falling middle-left upper tube.</summary>
    MotherBrainTopMiddleLeftTube,
    /// <summary>Mother Brain's falling middle-right upper tube.</summary>
    MotherBrainTopMiddleRightTube,
    /// <summary>Ridley's aimed fireball, shared by Ceres and Zebes.</summary>
    CeresRidleyFireball,
    /// <summary>The stationary center that spawns horizontal afterburns.</summary>
    CeresRidleyHorizontalAfterburnCenter,
    /// <summary>The stationary center that spawns vertical afterburns.</summary>
    CeresRidleyVerticalAfterburnCenter,
    /// <summary>A rightward afterburn and its continuation chain.</summary>
    CeresRidleyHorizontalAfterburnRight,
    /// <summary>A leftward afterburn and its continuation chain.</summary>
    CeresRidleyHorizontalAfterburnLeft,
    /// <summary>An upward afterburn and its continuation chain.</summary>
    CeresRidleyVerticalAfterburnUp,
    /// <summary>A downward afterburn and its continuation chain.</summary>
    CeresRidleyVerticalAfterburnDown,
    /// <summary>The light variant of Ceres falling debris.</summary>
    CeresFallingDebrisLight,
    /// <summary>The dark variant of Ceres falling debris.</summary>
    CeresFallingDebrisDark,
    /// <summary>Dust emitted during gunship liftoff.</summary>
    GunshipLiftoffDustCloud,
    /// <summary>Alcoon's fireball attack.</summary>
    AlcoonFireball,
    /// <summary>The spike actor emitted by Powamp.</summary>
    PowampSpike,
    /// <summary>A Work Robot laser directed upward and left.</summary>
    WorkRobotLaserUpLeft,
    /// <summary>The Work Robot's horizontal laser definition.</summary>
    WorkRobotLaserHorizontal,
    /// <summary>A Work Robot laser directed downward and left.</summary>
    WorkRobotLaserDownLeft,
    /// <summary>A Work Robot laser directed upward and right.</summary>
    WorkRobotLaserUpRight,
    /// <summary>A Work Robot laser directed downward and right.</summary>
    WorkRobotLaserDownRight,
    /// <summary>A falling spark actor.</summary>
    FallingSpark,
    /// <summary>A persistent projectile-backed Nuclear Waffle body link.</summary>
    NuclearWaffleBody,
    /// <summary>Fake Kraid's spit projectile.</summary>
    FakeKraidSpit,
    /// <summary>Fake Kraid's left spike projectile.</summary>
    FakeKraidSpikeLeft,
    /// <summary>Fake Kraid's right spike projectile.</summary>
    FakeKraidSpikeRight,
    /// <summary>The left-facing Ki-Hunter acid-spit definition.</summary>
    KiHunterAcidSpitLeft,
    /// <summary>The right-facing Ki-Hunter acid-spit definition.</summary>
    KiHunterAcidSpitRight,
    /// <summary>The Space Pirate laser used in Mother Brain's room.</summary>
    PirateMotherBrainLaser,
    /// <summary>A Space Pirate claw projectile.</summary>
    PirateClaw,
    /// <summary>The rock actor emitted by Polyp.</summary>
    PolypRock,
    /// <summary>A Cacatac's directional spike.</summary>
    CacatacSpike,
    /// <summary>Namihe's fireball definition.</summary>
    NamiheFireball,
    /// <summary>Fune's fireball definition.</summary>
    FuneFireball,
    /// <summary>Lava thrown by Magdollite.</summary>
    LavaThrownByMagdollite,
    /// <summary>Dragon's fireball attack.</summary>
    DragonFireball,
    /// <summary>The Eye Door's aimed projectile.</summary>
    EyeDoorProjectile,
    /// <summary>The Eye Door's sweat actor.</summary>
    EyeDoorSweat,
    /// <summary>The room-object-spawned smoke actor used by Eye Doors.</summary>
    EyeDoorSmoke,
    /// <summary>A parameter-selected dust or explosion effect.</summary>
    MiscDustExplosion,
    /// <summary>An enemy drop with its pickup animation and collection behavior.</summary>
    EnemyDeathPickup,
    /// <summary>An enemy-death explosion that can publish drops or rebuild an enemy slot.</summary>
    EnemyDeathExplosion,
    /// <summary>A destructible bug released by Kago.</summary>
    KagoBug,
    /// <summary>A body segment following Botwoon's stored trajectory.</summary>
    BotwoonBody,
    /// <summary>Botwoon's angled spit projectile.</summary>
    BotwoonSpit,
    /// <summary>Continuous drool from low-health Bomb Torizo.</summary>
    BombTorizoLowHealthDrool,
    /// <summary>Bomb Torizo's initial drool actor.</summary>
    BombTorizoInitialDrool,
    /// <summary>The projectile actor for Bomb Torizo's explosive swipe.</summary>
    BombTorizoExplosiveSwipe,
    /// <summary>A low-health Bomb Torizo explosion.</summary>
    BombTorizoLowHealthExplosion,
    /// <summary>An explosion during Bomb Torizo's death sequence.</summary>
    BombTorizoDeathExplosion,
    /// <summary>A fragment effect from Bomb Torizo's breaking statue.</summary>
    BombTorizoStatueBreaking,
    /// <summary>A Chozo orb fired by Bomb Torizo.</summary>
    BombTorizoChozoOrb,
    /// <summary>Bomb Torizo's sonic-boom projectile.</summary>
    BombTorizoSonicBoom,
    /// <summary>A Chozo orb fired by Golden Torizo.</summary>
    GoldenTorizoChozoOrb,
    /// <summary>Golden Torizo's sonic-boom projectile.</summary>
    GoldenTorizoSonicBoom,
    /// <summary>Dust from Torizo's right-foot landing.</summary>
    BombTorizoRightFootDust,
    /// <summary>Dust from Torizo's left-foot landing.</summary>
    BombTorizoLeftFootDust,
    /// <summary>Golden Torizo's egg projectile.</summary>
    GoldenTorizoEgg,
    /// <summary>Golden Torizo's Super Missile projectile.</summary>
    GoldenTorizoSuperMissile,
    /// <summary>Golden Torizo's eye beam.</summary>
    GoldenTorizoEyeBeam,
    /// <summary>A water splash during the Tourian statue sequence.</summary>
    TourianStatueSplash,
    /// <summary>A statue eye's unlocking glow.</summary>
    TourianStatueEyeGlow,
    /// <summary>An unlocking particle from a Tourian statue.</summary>
    TourianStatueParticle,
    /// <summary>The trail behind a statue unlocking particle.</summary>
    TourianStatueTail,
    /// <summary>A soul actor in the statue-unlocking sequence.</summary>
    TourianStatueSoul,
    /// <summary>The projectile-backed Ridley statue actor.</summary>
    TourianStatueRidley,
    /// <summary>The projectile-backed Phantoon statue actor.</summary>
    TourianStatuePhantoon,
    /// <summary>Decoration at the statue base.</summary>
    TourianStatueBaseDecoration,
    /// <summary>A spike-clearing footstep effect.</summary>
    WreckedShipChozoSpikeFootstep,
    /// <summary>The alternate unused spike-clearing explosion definition.</summary>
    WreckedShipChozoSpikeFootstepAlternate,
    /// <summary>Dust emitted during the Tourian statue's descent.</summary>
    TourianStatueDescentDust,
    /// <summary>The front circle of Shaktool's attack.</summary>
    ShaktoolAttackFrontCircle,
    /// <summary>The middle circle of Shaktool's attack.</summary>
    ShaktoolAttackMiddleCircle,
    /// <summary>The back circle of Shaktool's attack.</summary>
    ShaktoolAttackBackCircle,
    /// <summary>A projectile-backed stalk segment.</summary>
    SporeSpawnStalk,
    /// <summary>A falling Spore Spawn spore.</summary>
    SporeSpawnSpore,
    /// <summary>The actor that schedules new spores.</summary>
    SporeSpawnSpawner,
    /// <summary>Electricity anchored to the current save-station room object.</summary>
    SaveStationElectricity,
    /// <summary>Gate actor spawned when a downward gate begins closing.</summary>
    DownwardGateMoving,
    /// <summary>Gate actor parked at the bottom when a room initially loads.</summary>
    DownwardGateClosed,
    /// <summary>The tube crack that flickers and falls during its destruction.</summary>
    NoobTubeCrack,
    /// <summary>A flying shard from the broken Maridia tube.</summary>
    NoobTubeShard,
    /// <summary>An air bubble released by the broken Maridia tube.</summary>
    NoobTubeReleasedAirBubble,
}
