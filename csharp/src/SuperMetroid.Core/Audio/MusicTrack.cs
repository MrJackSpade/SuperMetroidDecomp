namespace SuperMetroid.Core.Audio;

/// <summary>
/// One song of the game, named for what it is (#1748): the four shared songs every music data set holds (the fanfares
/// and the elevator and statue-hall tunes), the areas' themes, the boss battles, and the scenes' music. A song plays from
/// the loaded data set, and the clip library finds each set's songs by these names (<see cref="MusicTrackKey"/>); a song
/// two sets hold plays from whichever is loaded. A room that keeps the loaded set plays its area theme
/// (<see cref="AreaThemeDefinitions"/>).
/// </summary>
/// <remarks>Values are arbitrary; the import pipeline decodes each data set's track numbering into these names once.</remarks>
public enum MusicTrack
{
    /// <summary>Samus's fanfare, played as a game begins.</summary>
    SamusFanfare,
    /// <summary>The fanfare of a permanent item collected.</summary>
    ItemFanfare,
    /// <summary>The elevator tune, also played in item rooms and in a boss's room once the boss has fallen.</summary>
    Elevator,
    /// <summary>The tense statue-hall tune: the hall before Tourian's statues, Ridley's lair before he rises, and the game-over menu.</summary>
    StatueHall,

    /// <summary>The title sequence's opening, played as the logo appears.</summary>
    TitleOpening,
    /// <summary>The title screen's music, which the title sequence's skip selects before it rebuilds the screen.</summary>
    TitleScreen,

    /// <summary>Crateria on Samus's arrival: the Landing Site and the elevator room down to Blue Brinstar before Zebes awakes.</summary>
    CrateriaArrival,
    /// <summary>The Landing Site's music once Zebes has awoken, before Samus holds power bombs.</summary>
    CrateriaArrivalZebesAwake,
    /// <summary>The Morph Ball's room in Blue Brinstar before Zebes awakes.</summary>
    BlueBrinstarArrival,
    /// <summary>Lower Crateria's theme once Zebes has awoken, also Blue Brinstar's.</summary>
    LowerCrateria,
    /// <summary>The room of the statues before Tourian.</summary>
    TourianStatues,
    /// <summary>The Samus theme: Upper Crateria's theme once Samus holds power bombs, and the Hyper Beam's arrival at the end of Mother Brain's battle.</summary>
    SamusTheme,
    /// <summary>Green Brinstar's theme.</summary>
    GreenBrinstar,
    /// <summary>Red Brinstar's theme.</summary>
    RedBrinstar,
    /// <summary>Upper Norfair's theme.</summary>
    UpperNorfair,
    /// <summary>Lower Norfair's theme.</summary>
    LowerNorfair,
    /// <summary>East Maridia's theme: the sand-filled halls from the Forgotten Highway's elevator to Botwoon and Draygon.</summary>
    EastMaridia,
    /// <summary>West Maridia's theme: Main Street and the shafts and tunnels from Red Brinstar's glass tunnel.</summary>
    WestMaridia,
    /// <summary>The Wrecked Ship's theme while its power is off, before Phantoon falls.</summary>
    WreckedShipPoweredDown,
    /// <summary>The Wrecked Ship's theme once its power is back on, after Phantoon falls.</summary>
    WreckedShipPoweredUp,
    /// <summary>Tourian's theme.</summary>
    Tourian,
    /// <summary>
    /// A song that plays only silence: Mother Brain's room holds the music silent with it once her first form falls, and the
    /// Baby Metroid's encounter once the Baby Metroid lets Samus go.
    /// </summary>
    SilentSong,

    /// <summary>The battle with Mother Brain's second form.</summary>
    MotherBrainBattle,
    /// <summary>The battle music of the Bomb and Golden Torizos, Draygon, and Ridley on Ceres and in Norfair.</summary>
    RidleyAndDraygonBattle,
    /// <summary>The Bomb Torizo's awakening, as its statue comes to life.</summary>
    TorizoAwakening,
    /// <summary>The escape: Ceres after Ridley flees with the Metroid, and Tourian and Crateria after Mother Brain falls.</summary>
    Escape,
    /// <summary>The battle music of Kraid, Phantoon and Crocomire, also played as the Baby Metroid attacks.</summary>
    KraidAndPhantoonBattle,
    /// <summary>A boss room's tension: Kraid's and Phantoon's rooms before they appear, Mini-Kraid's room, and Crocomire's fall.</summary>
    BossRoomTension,
    /// <summary>The battle music of the minibosses Spore Spawn and Botwoon.</summary>
    MiniBossBattle,
    /// <summary>The Baby Metroid's theme, played as Samus approaches its encounter in Tourian.</summary>
    BabyMetroidTheme,

    /// <summary>The gunship's flight to the Ceres space station.</summary>
    CeresFlight,
    /// <summary>The Ceres space station's theme.</summary>
    CeresStation,
    /// <summary>The station's destruction once Samus has escaped it.</summary>
    CeresExplosion,
    /// <summary>The station's destruction with Samus still aboard.</summary>
    CeresExplosionWithSamusAboard,
    /// <summary>Zebes's destruction as the ending's gunship escapes it.</summary>
    ZebesExplosion,
    /// <summary>The narrated introduction's music.</summary>
    Introduction,
    /// <summary>Samus's death.</summary>
    SamusDeath,
    /// <summary>The ending's credits.</summary>
    Credits,
    /// <summary>The opening's first spoken line, "the last Metroid is in captivity".</summary>
    LastMetroidInCaptivity,
    /// <summary>The opening's second spoken line, "the galaxy is at peace".</summary>
    GalaxyAtPeace,
}
