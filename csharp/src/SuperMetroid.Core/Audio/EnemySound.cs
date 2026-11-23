namespace SuperMetroid.Core.Audio;

/// <summary>
/// Every sound of the enemies' library (<see cref="SoundEffectLibrary.Enemies"/>): the enemies' and bosses' cries,
/// shots and attacks, the drops they leave and the impacts and explosions that kill them, and the blocks, water, lava
/// and stations around them. The library has its own queue: its sounds interrupt one another and never those of
/// Samus's or the room's library. The requests that stop its sound and play nothing are <see cref="EnemySoundStop"/>'s
/// (#2580).
/// </summary>
/// <remarks>
/// Members are identities. Their values are arbitrary and nothing reads them; the import pipeline alone knows which
/// installed sound effect each member requests.
/// <para>
/// Each different sound has one member (#2580), in this library or the room's, which names every use it serves. A request
/// merged into a member of this library had its priority and length; one that came from another library says what
/// interruption it gave up.
/// </para>
/// </remarks>
public enum EnemySound
{
    /// <summary>A small energy drop collected; a high-priority sound.</summary>
    SmallEnergyPickup,
    /// <summary>A big energy drop collected; a high-priority sound.</summary>
    BigEnergyPickup,
    /// <summary>A missile, Super Missile or Power Bomb drop collected; a high-priority sound.</summary>
    AmmoPickup,
    /// <summary>A block destroyed by contact damage.</summary>
    ContactBlockBreak,
    /// <summary>A missile or Super Missile striking a wall, or a Morph Ball bomb exploding.</summary>
    WeaponExplosion,
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
    /// <summary>Lava bubbling as a burst of four pops; one of the three the lava picks from at random.</summary>
    LavaBubblingBurst,
    /// <summary>Lava bubbling as two pops falling in pitch; one of the three the lava picks from at random.</summary>
    LavaBubblingFalling,
    /// <summary>Lava bubbling as two pops rising in pitch; one of the three the lava picks from at random.</summary>
    LavaBubblingRising,
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
    /// <summary>A two-pulse low clank near 125 Hz (0.44 s): a Kzan landing, or a Mama Turtle hitting a wall.</summary>
    HeavyClank,
    /// <summary>
    /// Three quick low pulses (0.25 s): a Chozo statue grabbing Samus, also each Crocomire fight projectile leaving his
    /// mouth; a high-priority sound.
    /// </summary>
    Rattle,
    /// <summary>The Dachora's cry.</summary>
    DachoraCry,
    /// <summary>
    /// A steady 2.2 kHz tone cut off by a low drop (0.31 s): a Fune or Namihe spitting; a high-priority sound.
    /// </summary>
    HighSpit,
    /// <summary>A shot fly hit.</summary>
    ShotFly,
    /// <summary>
    /// A two-pulse screech warbling between 1.9 and 3 kHz (0.39 s): a Skree, Metaree, wall or ninja Space Pirate hit, or
    /// a Skree or Metaree launching its dive.
    /// </summary>
    Screech,
    /// <summary>
    /// Three squeaks rising from 1.2 to 1.6 kHz (0.28 s): a pipe bug or Choot hit, a Golden Torizo egg hatching,
    /// Crocomire's acid burn.
    /// </summary>
    RisingSqueaks,
    /// <summary>Three squeaks between 580 and 780 Hz (0.28 s): a Zoomer, Zeela, Sova, Zero or Sidehopper hit.</summary>
    LowSqueaks,
    /// <summary>
    /// A small explosion, as an enemy dies; also a projectile of Mother Brain or a Torizo striking a surface, the Baby
    /// Metroid and Mother Brain exploding, and Kraid's rocks. Those last were the room library's requests: merged here
    /// (#2580), they wait in this library's queue, and interrupt and are interrupted by this library's sounds and stops,
    /// no longer the room library's.
    /// </summary>
    SmallExplosion,
    /// <summary>
    /// A big explosion: the Ceres door, Crocomire and its wall, Draygon's tail and Mother Brain; also a quake, as
    /// Crocomire or Kraid moves or Ridley's tail strikes the floor.
    /// </summary>
    BigExplosion,
    /// <summary>Bomb Torizo's explosive swipe.</summary>
    BombTorizoSwipe,
    /// <summary>A Torizo hit; a high-priority sound.</summary>
    ShotTorizo,
    /// <summary>A heavy rumble: Mother Brain rising, Crocomire's wall exploding, Spore Spawn hardening.</summary>
    HeavyRumble,
    /// <summary>
    /// A rolling rumble of a dozen low pulses near 90 Hz (1.13 s): Crocomire's hidden wall rumbling, Phantoon's death
    /// explosions.
    /// </summary>
    RollingRumble,
    /// <summary>Spore Spawn hit or opening up; a high-priority sound.</summary>
    ShotSporeSpawn,
    /// <summary>
    /// A roar rising from 2.9 to 4.1 kHz (0.64 s): Kraid's attack and dying roars, also the start of Crocomire's second
    /// melt; a high-priority sound.
    /// </summary>
    Roar,
    /// <summary>Kraid's dying cry; a high-priority sound.</summary>
    KraidDyingCry,
    /// <summary>A yapping maw.</summary>
    YappingMaw,
    /// <summary>
    /// A squeal stepping up from 2.6 to 4.6 kHz (0.54 s): a large Dessgeega hit, or Crocomire's spike wall breaking apart.
    /// </summary>
    SteppedSqueal,
    /// <summary>A Brinstar plant chewing.</summary>
    PlantChewing,
    /// <summary>An Etecoon's wall jump.</summary>
    EtecoonWallJump,
    /// <summary>An Etecoon's cry.</summary>
    EtecoonCry,
    /// <summary>
    /// A low thump swelling to its peak 0.19 s in, near 100 Hz (0.39 s): a Cacatac's spikes, a Golden Torizo egg
    /// released, or a Powamp hit.
    /// </summary>
    LowThump,
    /// <summary>The Etecoons' theme; a high-priority sound.</summary>
    EtecoonTheme,
    /// <summary>
    /// Three squeaks rising from 920 Hz to 1.5 kHz and falling back (0.36 s): a Rio, Geruta, Squeept or Dragon hit.
    /// </summary>
    Squeaks,
    /// <summary>A refill or map station's arm extending.</summary>
    StationEngaged,
    /// <summary>A refill or map station's arm retracting.</summary>
    StationDisengaged,
    /// <summary>A Mama Turtle spinning in her shell.</summary>
    MamaTurtleSpinning,
    /// <summary>A shinespark's launch, as the Dachora launches, also Crocomire's bridge collapsing under him.</summary>
    Shinespark,
    /// <summary>
    /// A short buzz near 220 Hz (0.25 s): an Owtch, Multiviola, Ripper, Tripper, suspensor platform, Yard, yapping maw or
    /// Atomic hit.
    /// </summary>
    LowBuzz,
    /// <summary>
    /// A rushing whoosh (#2079): an Alcoon spitting fire, Fake Kraid's lint launching, a ninja pirate's spin jump.
    /// </summary>
    Whoosh,
    /// <summary>A boulder bouncing.</summary>
    BoulderBounce,
    /// <summary>A boulder exploding.</summary>
    BoulderExplosion,
    /// <summary>A lavaquake, also the Tourian reveal's rumble; a high-priority sound.</summary>
    Lavaquake,
    /// <summary>A Waver hit.</summary>
    ShotWaver,
    /// <summary>A Torizo's sonic boom.</summary>
    TorizoSonicBoom,
    /// <summary>A two-pulse hiss near 6.5 kHz (0.32 s): a Skultera, Sciser or Zoa hit.</summary>
    Hiss,
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
    /// <summary>A Metroid draining Samus, or a Metroid's cry; a high-priority sound.</summary>
    MetroidDrain,
    /// <summary>A Coven hit; a high-priority sound.</summary>
    ShotCoven,
    /// <summary>The Baby Metroid's remorse.</summary>
    BabyMetroidRemorse,
    /// <summary>A Beetom hit.</summary>
    ShotBeetom,
    /// <summary>A suit acquired: the Varia or Gravity Suit's transformation.</summary>
    SuitAcquired,
    /// <summary>
    /// A shot doing nothing: a door, gate, eye door or the noob tube shot with a shot it ignores, or an Oum's shell shot.
    /// </summary>
    DudShot,
    /// <summary>A squeal rising from 3.5 to 5.4 kHz (0.43 s): a Mochtroid hit, or a Metroid's cry.</summary>
    RisingSqueal,
    /// <summary>Ridley's roar; a high-priority sound.</summary>
    RidleyRoar,
    /// <summary>A Metroid hit, or a Metroid's cry.</summary>
    ShotMetroid,
    /// <summary>A Skree or Metaree striking the ground as it burrows.</summary>
    Burrow,
    /// <summary>A Sidehopper jumping.</summary>
    SidehopperJump,
    /// <summary>
    /// A short low thud (#1842): a Sidehopper landing, a fire arc part spawning or a Nuclear Waffle turning, an Evir
    /// spitting, an Alcoon emerging.
    /// </summary>
    LowThud,
    /// <summary>
    /// A two-pulse warble near 520 to 600 Hz (0.33 s): a Holtz, Dessgeega, Viola, Alcoon or Botwoon hit.
    /// </summary>
    LowWarble,
    /// <summary>A Dragon's or Magdollite's fire spit, or a fire pillar.</summary>
    FireSpit,
    /// <summary>Mother Brain's hand beam.</summary>
    MotherBrainHandBeam,
    /// <summary>A Holtz's cry.</summary>
    HoltzCry,
    /// <summary>A Rio's cry.</summary>
    RioCry,
    /// <summary>
    /// A yell dipping from 2.2 to 1.2 kHz and back (0.33 s): a Ki-Hunter or walking Space Pirate hit, a ninja Pirate's claw
    /// throw or divekick, a wall Pirate's jump, a walking Pirate reflecting a shot.
    /// </summary>
    Yell,
    /// <summary>A laser of a space pirate, Mother Brain, a Torizo or a work robot.</summary>
    Laser,
    /// <summary>A work robot's step.</summary>
    WorkRobot,
    /// <summary>A Shaktool hit.</summary>
    ShotShaktool,
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
    /// <summary>The Baby Metroid's cry; a high-priority sound.</summary>
    BabyMetroidCry,
    /// <summary>
    /// A noisy shriek between 6.4 and 9.2 kHz (0.51 s): Phantoon's cry, or Draygon's roar; a high-priority sound.
    /// </summary>
    Shriek,
    /// <summary>Crocomire's cry; a high-priority sound.</summary>
    CrocomireCry,
    /// <summary>Crocomire's skeleton collapsing; a high-priority sound.</summary>
    CrocomireSkeletonCollapse,
    /// <summary>Crocomire's melting cry; a high-priority sound.</summary>
    CrocomireMeltingCry,
    /// <summary>The Baby Metroid draining.</summary>
    BabyMetroidDrain,
    /// <summary>Phantoon materialising, the swell that rises from the middle of his three pitches.</summary>
    PhantoonAppearsMiddle,
    /// <summary>Phantoon materialising, the swell that rises from the lowest of his three pitches.</summary>
    PhantoonAppearsLow,
    /// <summary>Phantoon materialising, the swell held at the highest of his three pitches.</summary>
    PhantoonAppearsHigh,
    /// <summary>Botwoon spitting; a high-priority sound.</summary>
    BotwoonSpit,
    /// <summary>The Baby Metroid's guilt; a high-priority sound.</summary>
    BabyMetroidGuilt,
    /// <summary>
    /// A wail falling from 1.3 kHz to 950 Hz and rising to 1.85 kHz (1.4 s): Mother Brain's high-pitched cry, also
    /// Phantoon's dying cry; it interrupts any sound of the library.
    /// </summary>
    LongWail,
    /// <summary>Mother Brain charging her rainbow beam.</summary>
    MotherBrainRainbowCharge,
}
