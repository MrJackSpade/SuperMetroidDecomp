namespace SuperMetroid.Core.Progression;

/// <summary>
/// Known story/progression event numbers consumed by <c>$80:81FA-$80:824E</c>.
/// The backing table has room for 64 event bits, but the original game names only these.
/// </summary>
/// <remarks>
/// These values are bit indices in the persistent eight-byte event table at
/// <c>$7E:D820-$7E:D827</c>, not composable masks. <c>MarkEvent_inA</c> sets the
/// corresponding bit; save/load preserves it independently of the area boss bits.
/// </remarks>
public enum EventNumber
{
    /// <summary>Native event $00: Zebes is awake, set when the initial enemy-death quota unlocks its grey door and used by room progression.</summary>
    ZebesAwake = 0x00,
    /// <summary>Native event $01: the giant Metroid has drained the Sidehopper, preserving that Tourian encounter milestone.</summary>
    ShitroidAteSidehopper = 0x01,
    /// <summary>Native event $02: Mother Brain's glass-breaking PLM has completed destruction; room setup and Mother Brain initialization observe the broken-glass state.</summary>
    MotherBrainGlassDestroyed = 0x02,
    /// <summary>Native event $03: bit 0 of the persistent Zebetite destruction counter, set for counts 1 and 3 and rewritten as the counter advances.</summary>
    ZebetiteDestroyedBit0 = 0x03,
    /// <summary>Native event $04: bit 1 of the persistent Zebetite destruction counter, set for counts 2 and 3 and rewritten as the counter advances.</summary>
    ZebetiteDestroyedBit1 = 0x04,
    /// <summary>Native event $05: bit 2 of the persistent Zebetite destruction counter, set when all four Zebetites have been destroyed.</summary>
    ZebetiteDestroyedBit2 = 0x05,
    /// <summary>Native event $06: Phantoon's entrance statue has completed its grey transformation; one of four retained prerequisites for Tourian descent.</summary>
    PhantoonStatueGrey = 0x06,
    /// <summary>Native event $07: Ridley's entrance statue has completed its grey transformation; one of four retained prerequisites for Tourian descent.</summary>
    RidleyStatueGrey = 0x07,
    /// <summary>Native event $08: Draygon's entrance statue has completed its grey transformation; one of four retained prerequisites for Tourian descent.</summary>
    DraygonStatueGrey = 0x08,
    /// <summary>Native event $09: Kraid's entrance statue has completed its grey transformation; one of four retained prerequisites for Tourian descent.</summary>
    KraidStatueGrey = 0x09,
    /// <summary>Native event $0A: the statue descent has opened Tourian access; later room entries enable the completed sequence's scrolling.</summary>
    TourianUnlocked = 0x0a,
    /// <summary>Native event $0B: the Maridia glass-tube destruction PLM has broken the Noob Tube, selecting its permanently broken room state on later visits.</summary>
    MaridiaNoobTubeBroken = 0x0b,
    /// <summary>Native event $0C: the Lower Norfair Chozo sequence has lowered the acid, allowing its PLM and enemy owners to recognize the completed activation.</summary>
    LowerNorfairChozoLoweredAcid = 0x0c,
    /// <summary>Native event $0D: Shaktool's room mutation has cleared the sand path, selecting the cleared room layout on subsequent visits.</summary>
    ShaktoolClearedPath = 0x0d,
    /// <summary>Native event $0E: Zebes's escape timebomb is armed; escape setup persists it and room selection uses the resulting escape states.</summary>
    ZebesTimebombSet = 0x0e,
    /// <summary>Native event $0F: the animal-rescue PLM has released the critters; their enemy behavior, grey-door condition, and ending observe this rescue.</summary>
    CrittersEscaped = 0x0f,
    /// <summary>Native event $10: the first Tourian Metroid hall has met its clear condition; the metroids-cleared PLM records it for later room-state selection.</summary>
    FirstMetroidHallCleared = 0x10,
    /// <summary>Native event $11: the first Tourian Metroid shaft has met its clear condition; the metroids-cleared PLM records it for later room-state selection.</summary>
    FirstMetroidShaftCleared = 0x11,
    /// <summary>Native event $12: the second Tourian Metroid hall has met its clear condition; the metroids-cleared PLM records it for later room-state selection.</summary>
    SecondMetroidHallCleared = 0x12,
    /// <summary>Native event $13: the second Tourian Metroid shaft has met its clear condition; the metroids-cleared PLM records it for later room-state selection.</summary>
    SecondMetroidShaftCleared = 0x13,
    /// <summary>Native event $14: unused in the bank-$80 event list, but retained because three Tourian room-state selectors still test this persistent bit.</summary>
    Unused14 = 0x14,
    /// <summary>Native event $15: the Speed Booster escape PLM has detected that Samus outran the rising-lava earthquake sequence, suppressing that hazard on later activation.</summary>
    OutranSpeedBoosterLavaquake = 0x15,
}
