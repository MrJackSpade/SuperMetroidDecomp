namespace SuperMetroid.Core.Progression;

/// <summary>
/// The story and progression events the game keeps, in the order it lists them. Their values are arbitrary and nothing
/// reads them; the number the game gives each event is known only to the import pipeline's progression codec.
/// </summary>
/// <remarks>
/// These are story events, not composable masks: <see cref="PersistentProgress.Events"/> records which have happened,
/// and saves keep them, by name, independently of the area bosses.
/// </remarks>
public enum StoryEvent
{
    /// <summary>Zebes is awake, set when the initial enemy-death quota unlocks its grey door and used by room progression.</summary>
    ZebesAwake,
    /// <summary>The giant Metroid has drained the Sidehopper, preserving that Tourian encounter milestone.</summary>
    ShitroidAteSidehopper,
    /// <summary>Mother Brain's glass-breaking room object has completed destruction; room setup and Mother Brain initialization observe the broken-glass state.</summary>
    MotherBrainGlassDestroyed,
    /// <summary>Phantoon's entrance statue has completed its grey transformation; one of four retained prerequisites for Tourian descent.</summary>
    PhantoonStatueGrey,
    /// <summary>Ridley's entrance statue has completed its grey transformation; one of four retained prerequisites for Tourian descent.</summary>
    RidleyStatueGrey,
    /// <summary>Draygon's entrance statue has completed its grey transformation; one of four retained prerequisites for Tourian descent.</summary>
    DraygonStatueGrey,
    /// <summary>Kraid's entrance statue has completed its grey transformation; one of four retained prerequisites for Tourian descent.</summary>
    KraidStatueGrey,
    /// <summary>The statue descent has opened Tourian access; later room entries enable the completed sequence's scrolling.</summary>
    TourianUnlocked,
    /// <summary>The Maridia glass-tube destruction room object has broken the Noob Tube, selecting its permanently broken room state on later visits.</summary>
    MaridiaNoobTubeBroken,
    /// <summary>The Lower Norfair Chozo sequence has lowered the acid, allowing its room-object and enemy owners to recognize the completed activation.</summary>
    LowerNorfairChozoLoweredAcid,
    /// <summary>Shaktool's room mutation has cleared the sand path, selecting the cleared room layout on subsequent visits.</summary>
    ShaktoolClearedPath,
    /// <summary>Zebes's escape timebomb is armed; escape setup persists it and room selection uses the resulting escape states.</summary>
    ZebesTimebombSet,
    /// <summary>The animal-rescue room object has released the critters; their enemy behavior, grey-door condition, and ending observe this rescue.</summary>
    CrittersEscaped,
    /// <summary>The first Tourian Metroid hall has met its clear condition; the metroids-cleared room object records it for later room-state selection.</summary>
    FirstMetroidHallCleared,
    /// <summary>The first Tourian Metroid shaft has met its clear condition; the metroids-cleared room object records it for later room-state selection.</summary>
    FirstMetroidShaftCleared,
    /// <summary>The second Tourian Metroid hall has met its clear condition; the metroids-cleared room object records it for later room-state selection.</summary>
    SecondMetroidHallCleared,
    /// <summary>The second Tourian Metroid shaft has met its clear condition; the metroids-cleared room object records it for later room-state selection.</summary>
    SecondMetroidShaftCleared,
    /// <summary>
    /// The rooms leading to the Shitroid skip its encounter music. Nothing in the game ever sets this event. Only the room-state tests of the three rooms leading to the Shitroid read it: when it is set, the
    /// Blue Hopper room keeps the music already playing and the Dust Torizo room plays Tourian's theme, where both
    /// otherwise start the Baby Metroid encounter's music, and the Big Boy room selects a state identical to its default.
    /// It is named for those alternate states.
    /// </summary>
    ShitroidEncounterMusicSkipped,
    /// <summary>The Speed Booster escape room object has detected that Samus outran the rising-lava earthquake sequence, suppressing that hazard on later activation.</summary>
    OutranSpeedBoosterLavaquake,
}
