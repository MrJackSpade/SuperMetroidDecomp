namespace SuperMetroid.Core.Audio;

/// <summary>
/// Every sound of the room's library (<see cref="SoundEffectLibrary.Room"/>): the room's doors, gates, elevators and
/// gunship, the bosses' larger cries and quakes, Samus's landings, footsteps, Speed Booster and shinespark, and the
/// low-health beep. The library has its own queue: its sounds interrupt one another and never those of Samus's or
/// the enemies' library, and its low-health beep holds the library against every other sound. The requests that stop its
/// sound and play nothing, one of which also ends the beep, are <see cref="RoomSoundStop"/>'s (#2580).
/// </summary>
/// <remarks>
/// Members are identities. Their values are arbitrary and nothing reads them; the import pipeline alone knows which
/// installed sound effect each member requests.
/// <para>
/// Each different sound has one member (#2580), in this library or the enemies', which names every use it serves. A
/// request merged into a member of this library from the enemies' library says what interruption it gave up.
/// </para>
/// </remarks>
public enum RoomSound
{
    /// <summary>The low-health beep; it interrupts any sound of the library except itself.</summary>
    LowHealthBeep,
    /// <summary>
    /// The Speed Booster reaching full speed, Samus's or the Dachora's. The Dachora's run was the enemies' library's request:
    /// merged here (#2580), it waits in this library's queue, interrupts and is interrupted by Samus's room sounds instead of
    /// the enemies' sounds, and ends at its wall turn by this library's
    /// <see cref="RoomSoundStop.SpeedBoosterOrElevator"/>, which also ends Samus's run and an elevator's sound.
    /// </summary>
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
    /// <summary>
    /// A shinespark stored, warning before it runs out, or the Dachora charging its shinespark. The Dachora's was the
    /// enemies' library's request: merged here (#2580), it interrupts and is interrupted by this library's sounds instead of
    /// the enemies'.
    /// </summary>
    StoredShinespark,
    /// <summary>
    /// A typewriter stroke of the intro text, the Zebes escape's warning or Ceres's self-destruct warning. Ceres's was the
    /// enemies' library's request: merged here (#2580), it interrupts and is interrupted by this library's sounds instead of
    /// the enemies'.
    /// </summary>
    Typewriter,
    /// <summary>A gate opening or closing; a high-priority sound.</summary>
    GateMoving,
    /// <summary>A shinespark launched.</summary>
    Shinespark,
    /// <summary>
    /// A shinespark ended against a wall, or the Dachora's striking the ceiling. The Dachora's was the enemies' library's
    /// request: merged here (#2580), it interrupts and is interrupted by this library's sounds instead of the enemies'.
    /// </summary>
    ShinesparkEnd,
    /// <summary>A shorter shinespark end.</summary>
    ShinesparkEndShort,
    /// <summary>The gunship's entrance pad opening; a high-priority sound.</summary>
    GunshipPadOpening,
    /// <summary>The gunship's entrance pad closing; a high-priority sound.</summary>
    GunshipPadClosing,
    /// <summary>The crunchy footstep of Mother Brain's third form.</summary>
    MotherBrainFootstep,
    /// <summary>Mother Brain's blue rings.</summary>
    MotherBrainBlueRings,
    /// <summary>The Baby Metroid dying; a high-priority sound.</summary>
    BabyMetroidDeath,
    /// <summary>Draygon's dying cry; a high-priority sound.</summary>
    DraygonDyingCry,
    /// <summary>Crocomire spitting.</summary>
    CrocomireSpit,
    /// <summary>One of Phantoon's flames.</summary>
    PhantoonFlame,
    /// <summary>Kraid firing lint.</summary>
    KraidLint,
    /// <summary>Ridley whipping its tail; a high-priority sound.</summary>
    RidleyTailWhip,
    /// <summary>Crocomire's acid damage.</summary>
    CrocomireAcidDamage,
    /// <summary>The Baby Metroid's high cry, one held note.</summary>
    BabyMetroidHighCry,
    /// <summary>The Baby Metroid's cry on Ceres; a high-priority sound.</summary>
    CeresBabyMetroidCry,
    /// <summary>The Baby Metroid's hitched cry, a short catch before a held note.</summary>
    BabyMetroidHitchedCry,
    /// <summary>The Baby Metroid's low cry, one louder, lower held note.</summary>
    BabyMetroidLowCry,
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
}
