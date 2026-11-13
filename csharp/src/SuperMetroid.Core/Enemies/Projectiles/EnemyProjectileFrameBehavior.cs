namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// How an enemy projectile moves itself on each projectile pass, before its program advances. Its values are arbitrary.
/// Verification's view relates each member to its source.
/// </summary>
internal enum EnemyProjectileFrameBehavior
{
    /// <summary>
    /// The projectile does not move itself: it holds still while its program plays, or its owner positions it. A freed
    /// slot holds it too.
    /// </summary>
    Stationary,

    /// <summary>Slides the downward gate by its signed 8.8 vertical velocity.</summary>
    DownwardGateDescent,

    /// <summary>
    /// Flies an eye-door shot with block collision, accumulating its angle-selected acceleration, and starts its
    /// explosion on impact or once its door opens.
    /// </summary>
    EyeDoorShotFlight,

    /// <summary>Drops an eye-door sweat drop, accelerating twelve 8.8 units per pass, and splashes on the floor.</summary>
    EyeDoorSweatFall,

    /// <summary>Drops a Bomb Torizo statue fragment until it reaches the room's floor.</summary>
    BombTorizoStatueFragmentFall,

    /// <summary>Counts a dropped pickup's lifetime and collects it when Samus or her grapple touches it.</summary>
    PickupLifetime,

    /// <summary>Turns a Mother Brain room turret, fires it, or removes it once the room asks.</summary>
    MotherBrainTurretAiming,

    /// <summary>Flickers and flies a turret bullet until it strikes a solid block.</summary>
    MotherBrainTurretBulletFlight,

    /// <summary>Flies a shard of Mother Brain's glass in 8.8 steps under gravity, trailing sparkles.</summary>
    MotherBrainGlassShardFlight,

    /// <summary>Raises dust once, then drops a ceiling tube, accelerating.</summary>
    MotherBrainTubeFall,

    /// <summary>Keeps drool hanging from Mother Brain's mouth for its first five poses.</summary>
    MotherBrainDroolHanging,

    /// <summary>Drops released drool, accelerating, until it reaches the arena floor.</summary>
    MotherBrainDroolFall,

    /// <summary>Holds an onion ring at the mouth for its delay, then flies it with its own collision and arena cull.</summary>
    MotherBrainOnionRingFlight,

    /// <summary>Drops a Mother Brain bomb under gravity, bouncing in stages and testing Samus's bombs.</summary>
    MotherBrainBombBounce,

    /// <summary>Contracts the rainbow beam's charge around the moving brain.</summary>
    MotherBrainRainbowBeamCharge,

    /// <summary>Keeps a rainbow-beam impact at its offset from Samus as she moves.</summary>
    MotherBrainRainbowImpactTracking,

    /// <summary>Keeps a death explosion at its offset from Mother Brain's body each pass.</summary>
    MotherBrainDeathExplosionTracking,

    /// <summary>Drags and drops a door fragment under gravity for its fixed lifetime.</summary>
    MotherBrainDeathDoorFragmentFall,

    /// <summary>Pins the death subtitle to fixed screen coordinates.</summary>
    MotherBrainDeathSubtitleScreenPin,

    /// <summary>Keeps Draygon's goop stuck to Samus for its lifetime.</summary>
    DraygonGoopStuckToSamus,

    /// <summary>Flies a wall turret's aimed shot at full precision until it leaves the room.</summary>
    DraygonTurretShotFlight,

    /// <summary>Flies Draygon's goop, sticks it to Samus when she is near, and removes it outside the room.</summary>
    DraygonGoopFlight,

    /// <summary>Drifts a spore along its authored movement stream.</summary>
    SporeDrift,

    /// <summary>Releases spores from the ceiling spawner on a random cadence while Spore Spawn is closed.</summary>
    SporeSpawnerCadence,

    /// <summary>Orients a Botwoon body segment, shows its hurt colours, and runs its death sequence.</summary>
    BotwoonBodySegment,

    /// <summary>Flies Botwoon's spit by its 16.16 vector until it leaves the camera.</summary>
    BotwoonSpitFlight,

    /// <summary>Drops a Bomb Torizo orb under gravity until it strikes the room.</summary>
    BombTorizoOrbFall,

    /// <summary>Bounces a Golden Torizo orb off walls and, damped, off the floor.</summary>
    GoldenTorizoOrbBounce,

    /// <summary>Flies a Torizo sonic boom horizontally, accelerating, until it strikes the room.</summary>
    TorizoSonicBoomFlight,

    /// <summary>Drags and drops Bomb Torizo's drool under gravity, splashing on impact.</summary>
    BombTorizoDroolFall,

    /// <summary>Bounces a Golden Torizo egg for its timer, then launches it horizontally.</summary>
    GoldenTorizoEggBounce,

    /// <summary>Accelerates a hatched Golden Torizo egg toward a wall.</summary>
    GoldenTorizoEggCharge,

    /// <summary>Drops a Golden Torizo egg that struck a wall to the floor, where it hatches or bursts.</summary>
    GoldenTorizoEggDrop,

    /// <summary>Keeps a held super missile at Golden Torizo's hand.</summary>
    GoldenTorizoSuperMissileHeld,

    /// <summary>Drops a thrown super missile under gravity until it strikes the room.</summary>
    GoldenTorizoSuperMissileThrown,

    /// <summary>Flies Golden Torizo's eye beam until it strikes the room.</summary>
    GoldenTorizoEyeBeamFlight,

    /// <summary>Keeps a Tourian entrance statue actor on the statue's line-by-line reveal.</summary>
    TourianStatueRevealFollow,

    /// <summary>Keeps a finished statue decoration at its place.</summary>
    TourianStatueDecorationSettled,

    /// <summary>Flies Shaktool's front circle with independent horizontal and vertical room collision.</summary>
    ShaktoolFrontCircleFlight,

    /// <summary>Flies Shaktool's middle and back circles while their owner lives.</summary>
    ShaktoolTrailingCircleFlight,

    /// <summary>Removes dust or an explosion once it leaves the camera.</summary>
    DustCameraCull,

    /// <summary>Flies a Dragon fireball in a signed 8.8 arc under gravity, removing it below the screen.</summary>
    DragonFireballArc,

    /// <summary>Flies a Ridley fireball.</summary>
    RidleyFireballFlight,

    /// <summary>Flickers the n00b-tube crack.</summary>
    NoobTubeCrackFlickering,

    /// <summary>Drops the detached n00b-tube crack.</summary>
    NoobTubeCrackFalling,

    /// <summary>Flies a newly emitted n00b-tube shard.</summary>
    NoobTubeShardFlying,

    /// <summary>Sways and drops a n00b-tube shard after its first flight.</summary>
    NoobTubeShardFalling,

    /// <summary>Sways and drops a released-air bubble.</summary>
    NoobTubeBubbleFalling,

    /// <summary>Moves a released-air bubble vertically.</summary>
    NoobTubeBubbleFlying,

    /// <summary>Flies one of Ridley's horizontal afterburns by its 8.8 velocity.</summary>
    HorizontalAfterburnFlight,

    /// <summary>Flies one of Ridley's vertical afterburns by its 8.8 velocity.</summary>
    VerticalAfterburnFlight,

    /// <summary>Flies a Skree particle in signed 8.8 steps under gravity, removing it outside the camera.</summary>
    SkreeParticleFlight,

    /// <summary>Moves Crocomire's shot once, then derives its fired vector.</summary>
    CrocomireShotAiming,

    /// <summary>Flies Crocomire's shot until it strikes the room horizontally or vertically.</summary>
    CrocomireShotFlight,

    /// <summary>Accelerates and drops each spike-wall piece by its own slot's rule.</summary>
    CrocomireSpikeWallCollapse,

    /// <summary>Flies one of Kraid's rocks with room collision, drag and gravity.</summary>
    KraidRockFlight,

    /// <summary>Drops a ceiling rock with vertical room collision and gravity.</summary>
    KraidCeilingRockFall,

    /// <summary>Switches a shot spat rock to the common explosion colours.</summary>
    KraidShotRockRecolor,

    /// <summary>Holds an intro flame until Phantoon's body activates it.</summary>
    PhantoonIntroFlameWaiting,

    /// <summary>Orbits an intro flame while its radius contracts.</summary>
    PhantoonIntroFlameOrbit,

    /// <summary>Drops a casual flame until it first strikes the room.</summary>
    PhantoonCasualFlameFall,

    /// <summary>Holds a landed casual flame for its impact pause.</summary>
    PhantoonCasualFlameLanding,

    /// <summary>Bounces a casual flame twice, each bounce lower.</summary>
    PhantoonCasualFlameBounce,

    /// <summary>Orbits a rage flame outward around Phantoon's body.</summary>
    PhantoonRageFlameOrbit,

    /// <summary>Drops a rain flame after its delay until it strikes the room.</summary>
    PhantoonFlameRainFall,

    /// <summary>Spirals a flame outward around Phantoon's body.</summary>
    PhantoonSpiralFlameOrbit,

    /// <summary>Drops a crumbling bridge fragment under gravity until it strikes the room.</summary>
    CrocomireBridgeFragmentFall,

    /// <summary>Flies an Alcoon fireball with vertical, then horizontal, room collision and horizontal drag.</summary>
    AlcoonFireballFlight,

    /// <summary>Flies Ki-Hunter acid under gravity until it strikes the room.</summary>
    KiHunterAcidFlight,

    /// <summary>Launches Ki-Hunter acid leftward.</summary>
    KiHunterAcidLaunchLeft,

    /// <summary>Launches Ki-Hunter acid rightward.</summary>
    KiHunterAcidLaunchRight,

    /// <summary>Flies a Powamp spike outward, accelerating, with horizontal-then-vertical room collision.</summary>
    PowampSpikeFlight,

    /// <summary>Flies a Work Robot laser with room collision.</summary>
    WorkRobotLaserFlight,

    /// <summary>Flies a Stoke fireball.</summary>
    StokeFireballFlight,

    /// <summary>Flies a Cacatac spike.</summary>
    CacatacSpikeFlight,

    /// <summary>Flies a Polyp rock.</summary>
    PolypRockFlight,

    /// <summary>Flies a Fune or Namihe fireball.</summary>
    FuneNamiheFireballFlight,

    /// <summary>Flies Magdollite lava.</summary>
    MagdolliteLavaFlight,

    /// <summary>Holds a Kago bug for its idle time, then starts its jump.</summary>
    KagoBugWaiting,

    /// <summary>Raises a jumping Kago bug.</summary>
    KagoBugJump,

    /// <summary>Drops a Kago bug until it lands.</summary>
    KagoBugFall,

    /// <summary>Drops a falling spark in 16.16 steps under gravity, bouncing on the floor and leaving a trail.</summary>
    FallingSparkBounce,

    /// <summary>Drops a Ceres tile, accelerating, and raises dust where it lands.</summary>
    CeresTileFall,

    /// <summary>Flies Fake Kraid's spit with room collision, then capped gravity.</summary>
    FakeKraidSpitFlight,

    /// <summary>Flies a Fake Kraid spike horizontally until it strikes a wall.</summary>
    FakeKraidSpikeFlight,

    /// <summary>Flies a Space Pirate or Mother Brain laser left until it leaves the camera.</summary>
    LaserFlightLeft,

    /// <summary>Flies a Space Pirate or Mother Brain laser right until it leaves the camera.</summary>
    LaserFlightRight,

    /// <summary>Flies a Ninja Space Pirate claw thrown left out, then back right.</summary>
    PirateClawThrownLeft,

    /// <summary>Flies a Ninja Space Pirate claw thrown right out, then back left.</summary>
    PirateClawThrownRight,

    /// <summary>Holds a Tourian statue splash at the water surface.</summary>
    TourianStatueSplashMotion,

    /// <summary>Drops a Tourian statue particle, splashing as it crosses the water.</summary>
    TourianStatueParticleMotion,

    /// <summary>Raises a Tourian statue soul out of the room.</summary>
    TourianStatueSoulMotion,
}
