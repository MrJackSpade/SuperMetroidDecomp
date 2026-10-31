namespace SuperMetroid.Core.Progression;

/// <summary>
/// Known story/progression event numbers consumed by <c>$80:81FA-$80:824E</c>.
/// The backing table has room for 64 event bits, but the original game names only these.
/// </summary>
/// <remarks>
/// These are story events, not composable masks: <see cref="PersistentProgress.Events"/> records which have happened,
/// by the number the game gives each (<see cref="EventNumberCodec"/>), and saves keep them independently of the area
/// bosses.
/// </remarks>
public enum EventNumber
{
    /// <summary>Native event $00: Zebes is awake, set when the initial enemy-death quota unlocks its grey door and used by room progression.</summary>
    ZebesAwake = 0,
    /// <summary>Native event $01: the giant Metroid has drained the Sidehopper, preserving that Tourian encounter milestone.</summary>
    ShitroidAteSidehopper = 1,
    /// <summary>Native event $02: Mother Brain's glass-breaking PLM has completed destruction; room setup and Mother Brain initialization observe the broken-glass state.</summary>
    MotherBrainGlassDestroyed = 2,
    /// <summary>Native event $03: bit 0 of the persistent Zebetite destruction counter, set for counts 1 and 3 and rewritten as the counter advances.</summary>
    ZebetiteDestroyedBit0 = 3,
    /// <summary>Native event $04: bit 1 of the persistent Zebetite destruction counter, set for counts 2 and 3 and rewritten as the counter advances.</summary>
    ZebetiteDestroyedBit1 = 4,
    /// <summary>Native event $05: bit 2 of the persistent Zebetite destruction counter, set when all four Zebetites have been destroyed.</summary>
    ZebetiteDestroyedBit2 = 5,
    /// <summary>Native event $06: Phantoon's entrance statue has completed its grey transformation; one of four retained prerequisites for Tourian descent.</summary>
    PhantoonStatueGrey = 6,
    /// <summary>Native event $07: Ridley's entrance statue has completed its grey transformation; one of four retained prerequisites for Tourian descent.</summary>
    RidleyStatueGrey = 7,
    /// <summary>Native event $08: Draygon's entrance statue has completed its grey transformation; one of four retained prerequisites for Tourian descent.</summary>
    DraygonStatueGrey = 8,
    /// <summary>Native event $09: Kraid's entrance statue has completed its grey transformation; one of four retained prerequisites for Tourian descent.</summary>
    KraidStatueGrey = 9,
    /// <summary>Native event $0A: the statue descent has opened Tourian access; later room entries enable the completed sequence's scrolling.</summary>
    TourianUnlocked = 10,
    /// <summary>Native event $0B: the Maridia glass-tube destruction PLM has broken the Noob Tube, selecting its permanently broken room state on later visits.</summary>
    MaridiaNoobTubeBroken = 11,
    /// <summary>Native event $0C: the Lower Norfair Chozo sequence has lowered the acid, allowing its PLM and enemy owners to recognize the completed activation.</summary>
    LowerNorfairChozoLoweredAcid = 12,
    /// <summary>Native event $0D: Shaktool's room mutation has cleared the sand path, selecting the cleared room layout on subsequent visits.</summary>
    ShaktoolClearedPath = 13,
    /// <summary>Native event $0E: Zebes's escape timebomb is armed; escape setup persists it and room selection uses the resulting escape states.</summary>
    ZebesTimebombSet = 14,
    /// <summary>Native event $0F: the animal-rescue PLM has released the critters; their enemy behavior, grey-door condition, and ending observe this rescue.</summary>
    CrittersEscaped = 15,
    /// <summary>Native event $10: the first Tourian Metroid hall has met its clear condition; the metroids-cleared PLM records it for later room-state selection.</summary>
    FirstMetroidHallCleared = 16,
    /// <summary>Native event $11: the first Tourian Metroid shaft has met its clear condition; the metroids-cleared PLM records it for later room-state selection.</summary>
    FirstMetroidShaftCleared = 17,
    /// <summary>Native event $12: the second Tourian Metroid hall has met its clear condition; the metroids-cleared PLM records it for later room-state selection.</summary>
    SecondMetroidHallCleared = 18,
    /// <summary>Native event $13: the second Tourian Metroid shaft has met its clear condition; the metroids-cleared PLM records it for later room-state selection.</summary>
    SecondMetroidShaftCleared = 19,
    /// <summary>Native event $14: unused in the bank-$80 event list, but retained because three Tourian room-state selectors still test this persistent bit.</summary>
    Unused14 = 20,
    /// <summary>Native event $15: the Speed Booster escape PLM has detected that Samus outran the rising-lava earthquake sequence, suppressing that hazard on later activation.</summary>
    OutranSpeedBoosterLavaquake = 21,
}
