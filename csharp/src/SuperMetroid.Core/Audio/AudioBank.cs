namespace SuperMetroid.Core.Audio;

/// <summary>
/// Stable identity of one retail SPC upload stream: the resident driver or one of the 24
/// music-data banks (#1304). Members follow the cartridge's music-data pointer-table order.
/// </summary>
/// <remarks>
/// The banks are mutually exclusive; an upload installs exactly one of them. Cartridge
/// source addresses and room-header data indexes are mapped from this identity only by
/// <see cref="AudioAssetCatalogDefinitions"/>, at the extraction boundary.
/// </remarks>
public enum AudioBank : byte
{
    /// <summary><c>SPC_Engine</c>: the resident driver, sound-effect programs and shared samples, uploaded at reset.</summary>
    SpcEngine,
    /// <summary><c>Music_TitleSequence</c>: the title-sequence music data.</summary>
    TitleSequence,
    /// <summary><c>Music_EmptyCrateria</c>: the empty-Crateria music data.</summary>
    EmptyCrateria,
    /// <summary><c>Music_LowerCrateria</c>: the lower-Crateria music data.</summary>
    LowerCrateria,
    /// <summary><c>Music_UpperCrateria</c>: the upper-Crateria music data.</summary>
    UpperCrateria,
    /// <summary><c>Music_GreenBrinstar</c>: the green-Brinstar music data.</summary>
    GreenBrinstar,
    /// <summary><c>Music_RedBrinstar</c>: the red-Brinstar music data.</summary>
    RedBrinstar,
    /// <summary><c>Music_UpperNorfair</c>: the upper-Norfair music data.</summary>
    UpperNorfair,
    /// <summary><c>Music_LowerNorfair</c>: the lower-Norfair music data.</summary>
    LowerNorfair,
    /// <summary><c>Music_Maridia</c>: the Maridia music data.</summary>
    Maridia,
    /// <summary><c>Music_Tourian</c>: the Tourian music data.</summary>
    Tourian,
    /// <summary><c>Music_MotherBrain</c>: the Mother Brain music data.</summary>
    MotherBrain,
    /// <summary><c>Music_BossFight1</c>: the first boss-fight music data.</summary>
    BossFight1,
    /// <summary><c>Music_BossFight2</c>: the second boss-fight music data.</summary>
    BossFight2,
    /// <summary><c>Music_MiniBossFight</c>: the miniboss music data.</summary>
    MiniBossFight,
    /// <summary><c>Music_Ceres</c>: the Ceres music data.</summary>
    Ceres,
    /// <summary><c>Music_WreckedShip</c>: the Wrecked Ship music data.</summary>
    WreckedShip,
    /// <summary><c>Music_ZebesExplosion</c>: the escape and Zebes-explosion music data.</summary>
    ZebesExplosion,
    /// <summary><c>Music_Intro</c>: the narrated-introduction music data.</summary>
    Intro,
    /// <summary><c>Music_Death</c>: the death-sequence music data.</summary>
    Death,
    /// <summary><c>Music_Credits</c>: the ending and credits music data.</summary>
    Credits,
    /// <summary><c>Music_TheLastMetroidIsInCaptivity</c>: the first spoken opening line.</summary>
    LastMetroidInCaptivity,
    /// <summary><c>Music_TheGalaxyIsAtPeace</c>: the second spoken opening line.</summary>
    GalaxyAtPeace,
    /// <summary><c>Music_BabyMetroid_BossFight2</c>: the Baby Metroid encounter's music data.</summary>
    BabyMetroidBossFight2,
    /// <summary><c>Music_SamusTheme_UpperCrateria</c>: the Samus-theme upper-Crateria music data.</summary>
    SamusThemeUpperCrateria,
}
