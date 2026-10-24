namespace SuperMetroid.Core.Audio;

/// <summary>
/// The music data sets: the songs and instruments one stretch of the game plays from. Exactly one is loaded at a time;
/// changing it switches the clips music and sounds play from, and costs the sound libraries the downtime
/// <see cref="AudioQueueDefinitions.MusicChangeSoundDowntimeCalls"/> describes. Before any is loaded only the shared
/// sounds play, which is represented as no data set (null).
/// </summary>
/// <remarks>Values are arbitrary; the import pipeline decodes the cartridge's data set numbering into these names once.</remarks>
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
    /// <summary>The first boss-fight music data set.</summary>
    BossFight1,
    /// <summary>The second boss-fight music data set.</summary>
    BossFight2,
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
    /// <summary>The Baby Metroid encounter's music data set.</summary>
    BabyMetroidBossFight2,
    /// <summary>The Samus-theme upper-Crateria music data set.</summary>
    SamusThemeUpperCrateria,
}
