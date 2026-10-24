namespace SuperMetroid.Core.Audio;

/// <summary>
/// Every sound of the second sound-effect library: pickups, impacts and explosions, blocks, water and lava, stations,
/// and the enemies' and bosses' sounds. The library is its own voice queue: its sounds interrupt one another and never
/// those of the other two libraries.
/// </summary>
/// <remarks>
/// Members are identities. Their values are arbitrary and nothing reads them; the import pipeline alone knows which
/// request each member is in the installed sound program.
/// </remarks>
public enum Library2Sound
{
    /// <summary>A small energy drop collected; a high-priority sound.</summary>
    SmallEnergyPickup,
    /// <summary>A big energy drop collected; a high-priority sound.</summary>
    BigEnergyPickup,
    /// <summary>A missile drop collected; a high-priority sound.</summary>
    MissilePickup,
    /// <summary>A Super Missile drop collected; a high-priority sound.</summary>
    SuperMissilePickup,
    /// <summary>A Power Bomb drop collected; a high-priority sound.</summary>
    PowerBombPickup,
    /// <summary>A block destroyed by contact damage.</summary>
    ContactBlockBreak,
    /// <summary>A missile or Super Missile striking a wall.</summary>
    MissileImpact,
    /// <summary>A Morph Ball bomb exploding.</summary>
    BombExplosion,
    /// <summary>An enemy killed.</summary>
    EnemyKilled,
    /// <summary>A block crumbling.</summary>
    BlockCrumble,
    /// <summary>An enemy killed or damaged by Samus's contact damage.</summary>
    ContactKill,
    /// <summary>A beam striking a wall, or a Torizo statue crumbling.</summary>
    BeamImpact,
    /// <summary>A splash into water.</summary>
    SplashIn,
    /// <summary>A splash out of water.</summary>
    SplashOut,
    /// <summary>Low-pitched air bubbles.</summary>
    LowBubbles,
    /// <summary>Lava or acid damaging Samus.</summary>
    LiquidDamage,
    /// <summary>High-pitched air bubbles.</summary>
    HighBubbles,
    /// <summary>The first lava bubbling sound.</summary>
    LavaBubbling1,
    /// <summary>The second lava bubbling sound.</summary>
    LavaBubbling2,
    /// <summary>The third lava bubbling sound.</summary>
    LavaBubbling3,
    /// <summary>The Maridia elevatube.</summary>
    Elevatube,
    /// <summary>Fake Kraid's cry; a high-priority sound.</summary>
    FakeKraidCry,
    /// <summary>A morph ball eye's ray; a high-priority sound.</summary>
    MorphBallEyeRay,
    /// <summary>A beacon, as in the hall before Tourian.</summary>
    Beacon,
    /// <summary>A Tourian statue's unlocking particle.</summary>
    TourianStatueParticle,
    /// <summary>The glass tube in Maridia shattering.</summary>
    GlassTubeShatter,
    /// <summary>A spike platform stopping, or a Tatori hitting a wall.</summary>
    SpikePlatformStop,
    /// <summary>A Chozo statue grabbing Samus; a high-priority sound.</summary>
    ChozoGrab,
    /// <summary>The Dachora's cry.</summary>
    DachoraCry,
    /// <summary>The library's first unused sound; a high-priority sound.</summary>
    Unused1,
    /// <summary>A Fune spitting; a high-priority sound.</summary>
    FuneSpit,
    /// <summary>A shot fly hit.</summary>
    ShotFly,
    /// <summary>A Skree or a wall or ninja space pirate hit.</summary>
    ShotSkree,
    /// <summary>A pipe bug or a Choot hit, also a Golden Torizo egg hatching.</summary>
    ShotPipeBug,
    /// <summary>A Zero, Sidehopper or Zoomer hit.</summary>
    ShotZoomer,
    /// <summary>A small explosion, as an enemy dies.</summary>
    SmallExplosion,
    /// <summary>A big explosion: the Ceres door, Crocomire and its wall, Draygon's tail and Mother Brain.</summary>
    BigExplosion,
    /// <summary>Bomb Torizo's explosive swipe.</summary>
    BombTorizoSwipe,
    /// <summary>A Torizo hit; a high-priority sound.</summary>
    ShotTorizo,
    /// <summary>The library's second unused sound.</summary>
    Unused2,
    /// <summary>A heavy rumble: Mother Brain rising, Crocomire's wall exploding, Spore Spawn hardening.</summary>
    HeavyRumble,
    /// <summary>The library's third unused sound.</summary>
    Unused3,
    /// <summary>Ridley's fireball striking a surface, Crocomire's post-death rumble, Phantoon exploding.</summary>
    FireballImpact,
    /// <summary>Spore Spawn hit or opening up; a high-priority sound.</summary>
    ShotSporeSpawn,
    /// <summary>Kraid's roar, also Crocomire's dying cry; a high-priority sound.</summary>
    KraidRoar,
    /// <summary>Kraid's dying cry; a high-priority sound.</summary>
    KraidDyingCry,
    /// <summary>A yapping maw.</summary>
    YappingMaw,
    /// <summary>A super Desgeega hit, or Crocomire destroying its wall.</summary>
    ShotSuperDesgeega,
    /// <summary>A Brinstar plant chewing.</summary>
    PlantChewing,
    /// <summary>An Etecoon's wall jump.</summary>
    EtecoonWallJump,
    /// <summary>An Etecoon's cry.</summary>
    EtecoonCry,
    /// <summary>A Cacatac's spikes, or a Golden Torizo egg released.</summary>
    CacatacSpikes,
    /// <summary>The Etecoons' theme; a high-priority sound.</summary>
    EtecoonTheme,
    /// <summary>A Rio, Squeept or Dragon hit.</summary>
    ShotRio,
    /// <summary>A refill or map station's arm extending.</summary>
    StationEngaged,
    /// <summary>A refill or map station's arm retracting.</summary>
    StationDisengaged,
    /// <summary>A Dachora's Speed Booster.</summary>
    DachoraSpeedBooster,
    /// <summary>A Tatori spinning.</summary>
    TatoriSpinning,
    /// <summary>A Dachora's shinespark.</summary>
    DachoraShinespark,
    /// <summary>A Dachora's shinespark ending.</summary>
    DachoraShinesparkEnd,
    /// <summary>A Dachora storing a shinespark.</summary>
    DachoraStoredShinespark,
    /// <summary>An Owtch, Viola, Ripper, Tripper, suspensor platform, Yard, yapping maw or Atomic hit.</summary>
    ShotRipper,
    /// <summary>An Alcoon spitting, Fake Kraid's lint, or a ninja pirate's spin jump.</summary>
    AlcoonSpit,
    /// <summary>The library's fourth unused sound.</summary>
    Unused4,
    /// <summary>The library's first empty sound; it plays nothing.</summary>
    Empty1,
    /// <summary>A boulder bouncing.</summary>
    BoulderBounce,
    /// <summary>A boulder exploding.</summary>
    BoulderExplosion,
    /// <summary>The library's second empty sound; it plays nothing.</summary>
    Empty2,
    /// <summary>A typewriter stroke of Ceres's self-destruct sequence.</summary>
    CeresTypewriter,
    /// <summary>A lavaquake, also the Tourian reveal's rumble; a high-priority sound.</summary>
    Lavaquake,
    /// <summary>A Waver hit.</summary>
    ShotWaver,
    /// <summary>A Torizo's sonic boom.</summary>
    TorizoSonicBoom,
    /// <summary>A Skultera, Sciser or Zoa hit.</summary>
    ShotSkultera,
    /// <summary>An Evir hit.</summary>
    ShotEvir,
    /// <summary>A Chozo statue's or a Torizo's footsteps.</summary>
    ChozoFootsteps,
    /// <summary>A Ki-Hunter's or an eye door's acid spit, or Draygon's goop.</summary>
    AcidSpit,
    /// <summary>The gunship hovering.</summary>
    GunshipHover,
    /// <summary>Ceres Ridley's getaway; a high-priority sound.</summary>
    CeresRidleyGetaway,
    /// <summary>The library's fifth unused sound.</summary>
    Unused5,
    /// <summary>A Metroid draining Samus, or a Metroid's cry; a high-priority sound.</summary>
    MetroidDrain,
    /// <summary>A Coven hit; a high-priority sound.</summary>
    ShotCoven,
    /// <summary>The Baby Metroid's remorse.</summary>
    BabyMetroidRemorse,
    /// <summary>A mini-Crocomire hit.</summary>
    ShotMiniCrocomire,
    /// <summary>Crocomire hit; no code plays it; a high-priority sound.</summary>
    ShotCrocomire,
    /// <summary>A Beetom hit.</summary>
    ShotBeetom,
    /// <summary>A suit acquired: the Varia or Gravity Suit's transformation.</summary>
    SuitAcquired,
    /// <summary>A door or gate shot with a shot it ignores, or a Reflec or Oum hit.</summary>
    DudShotOnDoor,
    /// <summary>A Mochtroid hit, or a Metroid's cry.</summary>
    ShotMochtroid,
    /// <summary>Ridley's roar; a high-priority sound.</summary>
    RidleyRoar,
    /// <summary>A Metroid hit, or a Metroid's cry.</summary>
    ShotMetroid,
    /// <summary>A Skree launching its dive.</summary>
    SkreeDive,
    /// <summary>A Skree striking the ground.</summary>
    SkreeBurrow,
    /// <summary>A Sidehopper jumping.</summary>
    SidehopperJump,
    /// <summary>A Sidehopper landing, a fire arc part spawning, an Evir spitting, an Alcoon spawning.</summary>
    SidehopperLanding,
    /// <summary>A Holtz, Desgeega, Viola, Alcoon or Botwoon hit.</summary>
    ShotHoltz,
    /// <summary>The library's sixth unused sound.</summary>
    Unused6,
    /// <summary>A Dragon's or Magdollite's fire spit, or a fire pillar.</summary>
    FireSpit,
    /// <summary>The library's seventh unused sound.</summary>
    Unused7,
    /// <summary>Mother Brain's hand beam.</summary>
    MotherBrainHandBeam,
    /// <summary>A Holtz's cry.</summary>
    HoltzCry,
    /// <summary>A Rio's cry.</summary>
    RioCry,
    /// <summary>A Ki-Hunter or walking space pirate hit, or a space pirate attacking.</summary>
    ShotSpacePirate,
    /// <summary>A laser of a space pirate, Mother Brain, a Torizo or a work robot.</summary>
    Laser,
    /// <summary>A work robot's step.</summary>
    WorkRobot,
    /// <summary>A Shaktool hit.</summary>
    ShotShaktool,
    /// <summary>A Powamp hit.</summary>
    ShotPowamp,
    /// <summary>The library's eighth unused sound.</summary>
    Unused8,
    /// <summary>A Kago bug.</summary>
    KagoBug,
    /// <summary>A Ceres tile falling from the ceiling and landing.</summary>
    CeresTileFall,
    /// <summary>Mother Brain's first phase hit; a high-priority sound.</summary>
    ShotMotherBrain,
    /// <summary>Mother Brain's low-pitched cry; a high-priority sound.</summary>
    MotherBrainLowCry,
    /// <summary>A Yard bouncing.</summary>
    YardBounce,
    /// <summary>Silences the library; it interrupts any sound of the library.</summary>
    Silence,
    /// <summary>The Baby Metroid's cry; a high-priority sound.</summary>
    BabyMetroidCry,
    /// <summary>Phantoon's or Draygon's cry; a high-priority sound.</summary>
    PhantoonCry,
    /// <summary>Crocomire's cry; a high-priority sound.</summary>
    CrocomireCry,
    /// <summary>Crocomire's skeleton collapsing; a high-priority sound.</summary>
    CrocomireSkeletonCollapse,
    /// <summary>A quake: Crocomire or Kraid moving, or Ridley's tail striking the floor.</summary>
    Quake,
    /// <summary>Crocomire's melting cry; a high-priority sound.</summary>
    CrocomireMeltingCry,
    /// <summary>The Baby Metroid draining.</summary>
    BabyMetroidDrain,
    /// <summary>Phantoon's first appearance sound.</summary>
    PhantoonAppears1,
    /// <summary>Phantoon's second appearance sound.</summary>
    PhantoonAppears2,
    /// <summary>Phantoon's third appearance sound.</summary>
    PhantoonAppears3,
    /// <summary>Botwoon spitting; a high-priority sound.</summary>
    BotwoonSpit,
    /// <summary>The Baby Metroid's guilt; a high-priority sound.</summary>
    BabyMetroidGuilt,
    /// <summary>Mother Brain's high-pitched cry, also Phantoon's dying cry; it interrupts any sound of the library.</summary>
    MotherBrainHighCry,
    /// <summary>Mother Brain charging her rainbow beam.</summary>
    MotherBrainRainbowCharge,
}
