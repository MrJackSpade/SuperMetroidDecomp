namespace SuperMetroid.Core.Audio;

/// <summary>
/// The music data sets: the songs and instruments one stretch of the game plays from, each installed as a clip set.
/// Exactly one is loaded at a time. Changing it is a clip-set switch: the song stops, music and sounds play from the new
/// set's clips, and the sound libraries take no requests for the downtime
/// <see cref="AudioQueueDefinitions.MusicChangeSoundDowntimePasses"/> describes (every library's request is withdrawn for
/// eight sound-queue passes, and the sound acceptance clock drops the requests not yet taken,
/// <see cref="SoundAcceptanceClock{TLibrary, TSound}.PauseForMusicLoad"/>). Before any is loaded only the shared sounds play,
/// which is represented as no data set (null).
/// </summary>
/// <remarks>Values are arbitrary; the import pipeline decodes the game's data set numbering into these names once. Each set is named for its content (#1748).</remarks>
public enum MusicDataSet
{
    /// <summary>The title-sequence music data set.</summary>
    TitleSequence,
    /// <summary>The empty-Crateria music data set.</summary>
    EmptyCrateria,
    /// <summary>The lower-Crateria music data set.</summary>
    LowerCrateria,
    /// <summary>The upper-Crateria music data set.</summary>
    UpperCrateria,
    /// <summary>The green-Brinstar music data set.</summary>
    GreenBrinstar,
    /// <summary>The red-Brinstar music data set.</summary>
    RedBrinstar,
    /// <summary>The upper-Norfair music data set.</summary>
    UpperNorfair,
    /// <summary>The lower-Norfair music data set.</summary>
    LowerNorfair,
    /// <summary>The Maridia music data set.</summary>
    Maridia,
    /// <summary>The Tourian music data set.</summary>
    Tourian,
    /// <summary>The Mother Brain music data set.</summary>
    MotherBrain,
    /// <summary>The battle music data set of the Torizos, Draygon and Ridley, with the Torizo's awakening and the escapes.</summary>
    RidleyAndDraygonBattles,
    /// <summary>The battle music data set of Kraid, Phantoon and Crocomire, with the boss rooms' tension.</summary>
    KraidAndPhantoonBattles,
    /// <summary>The miniboss music data set.</summary>
    MiniBossFight,
    /// <summary>The Ceres music data set.</summary>
    Ceres,
    /// <summary>The Wrecked Ship music data set.</summary>
    WreckedShip,
    /// <summary>The escape and Zebes-explosion music data set.</summary>
    ZebesExplosion,
    /// <summary>The narrated-introduction music data set.</summary>
    Intro,
    /// <summary>The death-sequence music data set.</summary>
    Death,
    /// <summary>The ending and credits music data set.</summary>
    Credits,
    /// <summary>The music data set of the first spoken opening line.</summary>
    LastMetroidInCaptivity,
    /// <summary>The music data set of the second spoken opening line.</summary>
    GalaxyAtPeace,
    /// <summary>The Baby Metroid encounter's music data set: its theme, its attack, played to Kraid's and Phantoon's battle music, and the silence after it.</summary>
    BabyMetroidEncounter,
    /// <summary>The Samus-theme upper-Crateria music data set.</summary>
    SamusThemeUpperCrateria,
}
