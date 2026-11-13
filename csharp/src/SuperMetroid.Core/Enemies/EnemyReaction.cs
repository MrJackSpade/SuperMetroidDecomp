namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The named reactions an enemy has to Samus's touch, to her shots and bombs, and to her Power Bombs. A definition names one
/// for each kind of contact, and an extended hitbox names one for touch and one for shot; the import decides each from
/// the game once. The values are arbitrary.
/// </summary>
public enum EnemyReaction
{
    /// <summary>The ordinary contact reaction: Samus takes the definition's damage, or her contact attack damages the enemy.</summary>
    NormalEnemyTouch,
    /// <summary>The ordinary shot reaction: the enemy takes the weapon's damage by its vulnerabilities.</summary>
    NormalEnemyShot,
    /// <summary>The shot glances off as a dud.</summary>
    DudShot,
    /// <summary>
    /// The reaction that does nothing. Every reaction that returns at once is this one reaction, whichever enemy names
    /// it; whether the engine still tests an enemy's extended hitboxes is the definition's separate
    /// <see cref="EnemyHitboxScan"/>, not a property of this reaction.
    /// </summary>
    Ignore,
    /// <summary>
    /// A reaction of the species' own that has no name of its own: the species' handlers dispatch it by the enemy's
    /// definition. A Power Bomb reaction of this kind that no handler claims fails loudly.
    /// </summary>
    SpeciesOwn,
    /// <summary>Owtch shot reaction.</summary>
    OwtchShot,
    /// <summary>Dragon touch reaction.</summary>
    DragonTouch,
    /// <summary>Dragon shot reaction.</summary>
    DragonShot,
    /// <summary>Dragon Power Bomb reaction.</summary>
    DragonPowerBomb,
    /// <summary>Shared G/Ripper 2 shot reaction.</summary>
    GRipperRipper2Shot,
    /// <summary>Mama Turtle touch reaction.</summary>
    MamaTurtleTouch,
    /// <summary>Baby Turtle touch reaction.</summary>
    BabyTurtleTouch,
    /// <summary>Baby Turtle shot reaction.</summary>
    BabyTurtleShot,
    /// <summary>Maridia large snail's damaging touch reaction.</summary>
    MaridiaLargeSnailDamagingTouch,
    /// <summary>Maridia large snail's harmless touch reaction.</summary>
    MaridiaLargeSnailNonDamagingTouch,
    /// <summary>Maridia large snail's shot reaction.</summary>
    MaridiaLargeSnailShot,
    /// <summary>Rinka touch reaction.</summary>
    RinkaTouch,
    /// <summary>Rinka shot reaction.</summary>
    RinkaShot,
    /// <summary>Rinka Power Bomb reaction.</summary>
    RinkaPowerBomb,
    /// <summary>Vertical shutter touch reaction.</summary>
    VerticalShutterTouch,
    /// <summary>Shootable vertical shutter shot reaction.</summary>
    ShootableVerticalShutterShot,
    /// <summary>Destroyable vertical shutter shot reaction.</summary>
    DestroyableVerticalShutterShot,
    /// <summary>Vertical shutter Power Bomb reaction.</summary>
    VerticalShutterPowerBomb,
    /// <summary>Skree shot reaction.</summary>
    SkreeShot,
    /// <summary>Metaree shot reaction.</summary>
    MetareeShot,
    /// <summary>Fireflea touch reaction.</summary>
    FirefleaTouch,
    /// <summary>Fireflea Power Bomb reaction.</summary>
    FirefleaPowerBomb,
    /// <summary>Fireflea shot reaction.</summary>
    FirefleaShot,
    /// <summary>Mochtroid touch reaction.</summary>
    MochtroidTouch,
    /// <summary>Mochtroid shot reaction.</summary>
    MochtroidShot,
    /// <summary>Metroid touch reaction.</summary>
    MetroidTouch,
    /// <summary>Metroid shot reaction.</summary>
    MetroidShot,
    /// <summary>Metroid Power Bomb reaction.</summary>
    MetroidPowerBomb,
    /// <summary>Yard touch reaction.</summary>
    YardTouch,
    /// <summary>Yard shot reaction.</summary>
    YardShot,
    /// <summary>Tripper shot reaction.</summary>
    TripperShot,
    /// <summary>Crocomire claw touch reaction.</summary>
    CrocomireClawTouch,
    /// <summary>Crocomire counts the hit in its fight flags without other effect.</summary>
    CrocomireHitCountShot,
    /// <summary>Crocomire dust hitbox shot reaction.</summary>
    CrocomireDustHitboxShot,
    /// <summary>Crocomire mouth shot reaction.</summary>
    CrocomireMouthShot,
    /// <summary>Alternate Crocomire dust hitbox shot reaction.</summary>
    CrocomireAlternateDustHitboxShot,
    /// <summary>Crocomire Power Bomb reaction.</summary>
    CrocomirePowerBomb,
    /// <summary>Spore Spawn touch reaction.</summary>
    SporeSpawnTouch,
    /// <summary>Spore Spawn shot reaction.</summary>
    SporeSpawnShot,
    /// <summary>Draygon touch reaction.</summary>
    DraygonTouch,
    /// <summary>Draygon shot reaction.</summary>
    DraygonShot,
    /// <summary>Draygon Power Bomb reaction.</summary>
    DraygonPowerBomb,
    /// <summary>Fake Kraid touch reaction.</summary>
    FakeKraidTouch,
    /// <summary>Fake Kraid shot reaction.</summary>
    FakeKraidShot,
    /// <summary>Ceres steam touch reaction.</summary>
    CeresSteamTouch,
    /// <summary>Ridley's frame-hitbox touch reaction.</summary>
    RidleyFrameHitboxTouch,
    /// <summary>Ridley shot reaction.</summary>
    RidleyShot,
    /// <summary>Ridley Power Bomb reaction.</summary>
    RidleyPowerBomb,
    /// <summary>Zebetite touch reaction.</summary>
    ZebetiteTouch,
    /// <summary>Zebetite shot reaction.</summary>
    ZebetiteShot,
    /// <summary>Kraid background/foot touch reaction.</summary>
    KraidBackgroundTouch,
    /// <summary>Kraid arm touch reaction.</summary>
    KraidArmTouch,
    /// <summary>Kraid arm shot reaction.</summary>
    KraidArmShot,
    /// <summary>Kraid's nail touch: the ordinary contact reaction, then the nail dies.</summary>
    KraidNailTouch,
    /// <summary>Kraid's bad nail touch: the ordinary contact reaction, then the nail dies.</summary>
    KraidBadNailTouch,
    /// <summary>Evir touch reaction.</summary>
    EvirTouch,
    /// <summary>Evir Power Bomb reaction.</summary>
    EvirPowerBomb,
    /// <summary>Evir shot reaction.</summary>
    EvirShot,
    /// <summary>Magdollite Power Bomb reaction.</summary>
    MagdollitePowerBomb,
    /// <summary>Magdollite touch reaction.</summary>
    MagdolliteTouch,
    /// <summary>Magdollite shot reaction.</summary>
    MagdolliteShot,
    /// <summary>Beetom touch reaction.</summary>
    BeetomTouch,
    /// <summary>Beetom shot reaction.</summary>
    BeetomShot,
    /// <summary>Powamp touch reaction.</summary>
    PowampTouch,
    /// <summary>Powamp shot reaction.</summary>
    PowampShot,
    /// <summary>Powamp Power Bomb reaction.</summary>
    PowampPowerBomb,
    /// <summary>Work robot touch reaction.</summary>
    WorkRobotTouch,
    /// <summary>Work robot no-power shot reaction.</summary>
    WorkRobotNoPowerShot,
    /// <summary>Work robot powered shot reaction.</summary>
    WorkRobotShot,
    /// <summary>Bull shot reaction.</summary>
    BullShot,
    /// <summary>Kago shot reaction.</summary>
    KagoShot,
    /// <summary>Ki-Hunter shot reaction.</summary>
    KiHunterShot,
    /// <summary>Spark shot reaction.</summary>
    SparkShot,
    /// <summary>Blue Brinstar face block's shot reaction.</summary>
    BlueBrinstarFaceBlockShot,
    /// <summary>Yapping Maw touch reaction.</summary>
    YappingMawTouch,
    /// <summary>Yapping Maw shot reaction.</summary>
    YappingMawShot,
    /// <summary>Mother Brain body shot reaction.</summary>
    MotherBrainBodyShot,
    /// <summary>Mother Brain head shot reaction.</summary>
    MotherBrainHeadShot,
    /// <summary>Mother Brain head touch reaction.</summary>
    MotherBrainHeadTouch,
    /// <summary>Dead Torizo touch/shot reaction.</summary>
    DeadTorizoTouchAndShot,
    /// <summary>Dead Torizo Power Bomb reaction.</summary>
    DeadTorizoPowerBomb,
    /// <summary>Dead Sidehopper touch reaction.</summary>
    DeadSidehopperTouch,
    /// <summary>Dead Sidehopper shot reaction.</summary>
    DeadSidehopperShot,
    /// <summary>Dead Sidehopper Power Bomb reaction.</summary>
    DeadSidehopperPowerBomb,
    /// <summary>Shitroid touch reaction.</summary>
    ShitroidTouch,
    /// <summary>Shitroid shot reaction.</summary>
    ShitroidShot,
    /// <summary>Shitroid Power Bomb reaction.</summary>
    ShitroidPowerBomb,
    /// <summary>Bomb Torizo touch reaction.</summary>
    BombTorizoTouch,
    /// <summary>Bomb Torizo shot reaction.</summary>
    BombTorizoShot,
    /// <summary>Torizo stand-up/sit-down shot reaction.</summary>
    TorizoStandUpSitDownShot,
    /// <summary>Golden Torizo shot reaction.</summary>
    GoldenTorizoShot,
    /// <summary>Shaktool touch reaction.</summary>
    ShaktoolTouch,
    /// <summary>Shaktool shot reaction.</summary>
    ShaktoolShot,
    /// <summary>Space Pirate Power Bomb reaction.</summary>
    SpacePiratePowerBomb,
    /// <summary>Space Pirate touch reaction.</summary>
    SpacePirateTouch,
    /// <summary>Space Pirate shot reaction.</summary>
    SpacePirateShot,
    /// <summary>Gold ninja Space Pirate vulnerable-hitbox shot reaction.</summary>
    GoldNinjaVulnerableHitboxShot,
    /// <summary>Gold ninja Space Pirate invincible-hitbox shot reaction.</summary>
    GoldNinjaInvincibleHitboxShot,
    /// <summary>Botwoon touch reaction.</summary>
    BotwoonTouch,
    /// <summary>Botwoon shot reaction.</summary>
    BotwoonShot,
    /// <summary>Botwoon Power Bomb reaction.</summary>
    BotwoonPowerBomb,
    /// <summary>Phantoon body touch reaction.</summary>
    PhantoonTouch,
    /// <summary>Phantoon body shot reaction.</summary>
    PhantoonShot,
    /// <summary>Dead Zoomer's touch and shot reaction, which starts the corpse rotting.</summary>
    DeadZoomerTouchAndShot,
    /// <summary>Dead Zoomer's Power Bomb reaction, which starts the corpse rotting.</summary>
    DeadZoomerPowerBomb,
    /// <summary>Dead Ripper's touch and shot reaction, which starts the corpse rotting.</summary>
    DeadRipperTouchAndShot,
    /// <summary>Dead Ripper's Power Bomb reaction, which starts the corpse rotting.</summary>
    DeadRipperPowerBomb,
    /// <summary>Dead Skree's touch and shot reaction, which starts the corpse rotting.</summary>
    DeadSkreeTouchAndShot,
    /// <summary>Dead Skree's Power Bomb reaction, which starts the corpse rotting.</summary>
    DeadSkreePowerBomb,
}
