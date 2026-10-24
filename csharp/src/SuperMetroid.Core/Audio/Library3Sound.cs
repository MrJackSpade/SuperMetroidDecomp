namespace SuperMetroid.Core.Audio;

/// <summary>
/// Every sound of the third sound-effect library: Samus's landings, footsteps, Speed Booster and shinespark, the
/// low-health beep, doors, gates and elevators, and the bosses' larger cries. The library is its own voice queue: its
/// sounds interrupt one another and never those of the other two libraries, and its low-health beep holds the library
/// against every other sound but silence.
/// </summary>
/// <remarks>
/// Members are identities. Their values are arbitrary and nothing reads them; the import pipeline alone knows which
/// request each member is in the installed sound program.
/// </remarks>
public enum Library3Sound
{
    /// <summary>Silences the library; it interrupts any sound of the library.</summary>
    Silence,
    /// <summary>The low-health beep; it interrupts any sound of the library except itself.</summary>
    LowHealthBeep,
    /// <summary>The Speed Booster reaching full speed.</summary>
    SpeedBooster,
    /// <summary>Samus landing hard.</summary>
    HardLanding,
    /// <summary>Samus landing, or wall-jumping.</summary>
    Landing,
    /// <summary>Samus's footsteps.</summary>
    Footsteps,
    /// <summary>A door opened; a high-priority sound.</summary>
    DoorOpened,
    /// <summary>A door closed; a high-priority sound.</summary>
    DoorClosed,
    /// <summary>A missile door hit by a missile, or a Zebetite shot.</summary>
    MissileDoorHit,
    /// <summary>An enemy frozen; a high-priority sound.</summary>
    EnemyFrozen,
    /// <summary>An elevator moving.</summary>
    Elevator,
    /// <summary>A shinespark stored, warning before it runs out.</summary>
    StoredShinespark,
    /// <summary>A typewriter stroke of the intro text.</summary>
    IntroTypewriter,
    /// <summary>A gate opening or closing; a high-priority sound.</summary>
    GateMoving,
    /// <summary>A shinespark launched.</summary>
    Shinespark,
    /// <summary>A shinespark ended against a wall.</summary>
    ShinesparkEnd,
    /// <summary>A shorter shinespark end.</summary>
    ShinesparkEndShort,
    /// <summary>The library's first empty sound; a high-priority sound that plays nothing.</summary>
    Empty1,
    /// <summary>
    /// A projectile of Mother Brain or a Torizo striking a surface, also the Baby Metroid and Mother Brain exploding.
    /// </summary>
    ProjectileImpact,
    /// <summary>The gunship's entrance pad opening; a high-priority sound.</summary>
    GunshipPadOpening,
    /// <summary>The gunship's entrance pad closing; a high-priority sound.</summary>
    GunshipPadClosing,
    /// <summary>The crunchy footstep of Mother Brain's third form.</summary>
    MotherBrainFootstep,
    /// <summary>Mother Brain's blue rings.</summary>
    MotherBrainBlueRings,
    /// <summary>The library's second empty sound; it plays nothing.</summary>
    Empty2,
    /// <summary>The Baby Metroid dying; a high-priority sound.</summary>
    BabyMetroidDeath,
    /// <summary>The library's third empty sound; it plays nothing.</summary>
    Empty3,
    /// <summary>Draygon's dying cry; a high-priority sound.</summary>
    DraygonDyingCry,
    /// <summary>Crocomire spitting.</summary>
    CrocomireSpit,
    /// <summary>One of Phantoon's flames.</summary>
    PhantoonFlame,
    /// <summary>Kraid's earthquake.</summary>
    KraidEarthquake,
    /// <summary>Kraid firing lint.</summary>
    KraidLint,
    /// <summary>The library's fourth empty sound; a high-priority sound that plays nothing.</summary>
    Empty4,
    /// <summary>Ridley whipping its tail; a high-priority sound.</summary>
    RidleyTailWhip,
    /// <summary>Crocomire's acid damage.</summary>
    CrocomireAcidDamage,
    /// <summary>The Baby Metroid's first cry.</summary>
    BabyMetroidCry1,
    /// <summary>The Baby Metroid's cry on Ceres; a high-priority sound.</summary>
    CeresBabyMetroidCry,
    /// <summary>Silences the Speed Booster or an elevator.</summary>
    SpeedBoosterEnd,
    /// <summary>The Baby Metroid's second cry.</summary>
    BabyMetroidCry2,
    /// <summary>The Baby Metroid's third cry.</summary>
    BabyMetroidCry3,
    /// <summary>Phantoon materialising for an attack.</summary>
    PhantoonMaterialise,
    /// <summary>Phantoon's attack after a Super Missile hit.</summary>
    PhantoonSuperMissiled,
    /// <summary>The pause menu's ambient beep.</summary>
    PauseBeep,
    /// <summary>The Speed Booster or a shinespark resumed.</summary>
    ResumedSpeedBooster,
    /// <summary>A Ceres door opening; a high-priority sound.</summary>
    CeresDoorOpening,
    /// <summary>Energy rising or falling a little at a time: reserve refills, drains, heat and Mother Brain's energy.</summary>
    IncrementalEnergy,
    /// <summary>Mother Brain's glass shattering; a high-priority sound.</summary>
    MotherBrainGlassShatter,
    /// <summary>The library's fifth empty sound; it plays nothing.</summary>
    Empty5,
}
