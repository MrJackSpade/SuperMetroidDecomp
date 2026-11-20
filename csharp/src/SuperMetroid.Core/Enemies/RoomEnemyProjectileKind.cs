namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What an enemy projectile is.
/// </summary>
public enum RoomEnemyProjectileKind
{
    /// <summary>
    /// No enemy's projectile: the place the pre-Phantoon room's setup holds in the pool for its first projectile updates
    /// (<see cref="Compatibility.PrePhantoonRoomHeldProjectilePlace"/>). It shows nothing, touches nothing and frees the
    /// place when its hold ends.
    /// </summary>
    PrePhantoonRoomHeldPlace,
    /// <summary>A Yapping Maw's body.</summary>
    YappingMawBody,
    /// <summary>
    /// One of the four particles a Skree or Metaree bursts into (#2278): its launch velocity is its way, and its animation
    /// shows the enemy's own look.
    /// </summary>
    SkreeMetareeParticle,
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
    /// <summary>A rock Kraid's rise throws up from the floor (#2278): its animation shows the pose its random side chooses.</summary>
    KraidRisingRock,
    /// <summary>Phantoon's destructible flame actor.</summary>
    PhantoonDestroyableFlame,
    /// <summary>Phantoon's opening flame actor.</summary>
    PhantoonStartingFlame,
    /// <summary>Draygon's goop attack.</summary>
    DraygonGoop,
    /// <summary>A wall turret in Draygon's room.</summary>
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
    RidleyFireball,
    /// <summary>
    /// The stationary centre a fireball's impact leaves, which spawns a pair of afterburns; its animation names the axis
    /// the pair flies along.
    /// </summary>
    RidleyAfterburnCenter,
    /// <summary>An afterburn flying sideways, and its continuation chain; its velocity holds the way it flies.</summary>
    RidleyHorizontalAfterburn,
    /// <summary>An afterburn flying up or down, and its continuation chain; its velocity holds the way it flies.</summary>
    RidleyVerticalAfterburn,
    /// <summary>A ceiling tile falling in Ceres station's escape; its animation shows it light or dark.</summary>
    CeresFallingDebris,
    /// <summary>Dust emitted during gunship liftoff.</summary>
    GunshipLiftoffDustCloud,
    /// <summary>Alcoon's fireball attack.</summary>
    AlcoonFireball,
    /// <summary>The spike actor emitted by Powamp.</summary>
    PowampSpike,
    /// <summary>A Work Robot laser fired up or down ahead of the robot (#2278): its velocity is its way.</summary>
    WorkRobotLaserDiagonal,
    /// <summary>A Work Robot laser fired straight ahead of the robot: its velocity is its way.</summary>
    WorkRobotLaserHorizontal,
    /// <summary>A falling spark actor.</summary>
    FallingSpark,
    /// <summary>A link of a Nuclear Waffle's body, lasting as long as the Waffle.</summary>
    NuclearWaffleBody,
    /// <summary>Fake Kraid's spit projectile.</summary>
    FakeKraidSpit,
    /// <summary>A spike Fake Kraid fires the way he faces (#2278): its velocity and pose are its way.</summary>
    FakeKraidSpike,
    /// <summary>Acid a Ki-Hunter spits (#2278): its velocity and animation are its way.</summary>
    KiHunterAcidSpit,
    /// <summary>The Space Pirate laser used in Mother Brain's room.</summary>
    PirateMotherBrainLaser,
    /// <summary>A Space Pirate claw projectile.</summary>
    PirateClaw,
    /// <summary>The rock actor emitted by Polyp.</summary>
    PolypRock,
    /// <summary>A Cacatac's directional spike.</summary>
    CacatacSpike,
    /// <summary>A fireball a Namihe spits.</summary>
    NamiheFireball,
    /// <summary>A fireball a Fune spits.</summary>
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
    /// <summary>A dust cloud or explosion, its animation chosen by its spawner.</summary>
    DustOrExplosion,
    /// <summary>An enemy drop with its pickup animation and collection behavior.</summary>
    EnemyDeathPickup,
    /// <summary>An enemy-death explosion that can publish drops or rebuild the enemy in its place.</summary>
    EnemyDeathExplosion,
    /// <summary>A destructible bug released by Kago.</summary>
    KagoBug,
    /// <summary>A body segment following Botwoon's stored trajectory.</summary>
    BotwoonBody,
    /// <summary>Botwoon's angled spit projectile.</summary>
    BotwoonSpit,
    /// <summary>Continuous drool from low-health Bomb Torizo.</summary>
    BombTorizoLowHealthDrool,
    /// <summary>Either Torizo's initial drool actor (#2240).</summary>
    TorizoInitialDrool,
    /// <summary>The projectile actor for Bomb Torizo's explosive swipe.</summary>
    BombTorizoExplosiveSwipe,
    /// <summary>A low-health Bomb Torizo explosion.</summary>
    BombTorizoLowHealthExplosion,
    /// <summary>An explosion during either Torizo's death sequence (#2240).</summary>
    TorizoDeathExplosion,
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
    /// <summary>Dust from a Torizo's landing step; its animation and place show which foot raised it.</summary>
    TorizoLandingDust,
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
    /// <summary>Ridley's figure on the Tourian statue.</summary>
    TourianStatueRidley,
    /// <summary>Phantoon's figure on the Tourian statue.</summary>
    TourianStatuePhantoon,
    /// <summary>Decoration at the statue base.</summary>
    TourianStatueBaseDecoration,
    /// <summary>A spike-clearing footstep effect.</summary>
    WreckedShipChozoSpikeFootstep,
    /// <summary>Dust emitted during the Tourian statue's descent.</summary>
    TourianStatueDescentDust,
    /// <summary>A segment of Spore Spawn's stalk.</summary>
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
