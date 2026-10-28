using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>Where the cartridge keeps every retail non-PAL SPC upload stream: the import pipeline's sources.</summary>
/// <remarks>
/// Values are cartridge source addresses, not SPC RAM destinations or track selectors.
/// Each stream contains length/destination block headers and a zero-length terminator; its length is read from that structure,
/// not inferred from the next address. Music-data selectors are byte offsets into <c>Music_Pointers</c> at <c>$8F:E7E1</c>.
/// Import validates mapped source addresses and bounded termination; these values themselves perform no reads or validation.
/// </remarks>
public static class AudioUploadSourceDefinitions
{
    /// <summary><c>SPC_Engine</c> at <c>$CF:8000</c>; common upload containing the resident driver, sound-effect programs, and shared samples, catalog data selector $00.</summary>
    public static readonly SnesAddress SpcEngine = new(0xCF, 0x8000);
    /// <summary><c>Music_TitleSequence</c> at <c>$D0:E20D</c>; music-data upload selected by $03 for the title sequence, distinct from its track commands.</summary>
    public static readonly SnesAddress TitleSequence = new(0xD0, 0xE20D);
    /// <summary><c>Music_EmptyCrateria</c> at <c>$D1:B62A</c>; empty-Crateria music-data upload selected by $06, distinct from the later regional Crateria banks.</summary>
    public static readonly SnesAddress EmptyCrateria = new(0xD1, 0xB62A);
    /// <summary><c>Music_LowerCrateria</c> at <c>$D2:88CA</c>; lower-Crateria music-data upload selected by $09.</summary>
    public static readonly SnesAddress LowerCrateria = new(0xD2, 0x88CA);
    /// <summary><c>Music_UpperCrateria</c> at <c>$D2:D9B6</c>; upper-Crateria music-data upload selected by $0C, separate from the Samus-theme variant at $48.</summary>
    public static readonly SnesAddress UpperCrateria = new(0xD2, 0xD9B6);
    /// <summary><c>Music_GreenBrinstar</c> at <c>$D3:933C</c>; green-Brinstar music-data upload selected by $0F.</summary>
    public static readonly SnesAddress GreenBrinstar = new(0xD3, 0x933C);
    /// <summary><c>Music_RedBrinstar</c> at <c>$D3:E812</c>; red-Brinstar music-data upload selected by $12.</summary>
    public static readonly SnesAddress RedBrinstar = new(0xD3, 0xE812);
    /// <summary><c>Music_UpperNorfair</c> at <c>$D4:B86C</c>; upper-Norfair music-data upload selected by $15.</summary>
    public static readonly SnesAddress UpperNorfair = new(0xD4, 0xB86C);
    /// <summary><c>Music_LowerNorfair</c> at <c>$D4:F420</c>; lower-Norfair music-data upload selected by $18.</summary>
    public static readonly SnesAddress LowerNorfair = new(0xD4, 0xF420);
    /// <summary><c>Music_Maridia</c> at <c>$D5:C844</c>; Maridia music-data upload selected by $1B.</summary>
    public static readonly SnesAddress Maridia = new(0xD5, 0xC844);
    /// <summary><c>Music_Tourian</c> at <c>$D6:98B7</c>; Tourian music-data upload selected by $1E, separate from the Mother Brain bank.</summary>
    public static readonly SnesAddress Tourian = new(0xD6, 0x98B7);
    /// <summary><c>Music_MotherBrain</c> at <c>$D6:EF9D</c>; Mother Brain music-data upload selected by $21.</summary>
    public static readonly SnesAddress MotherBrain = new(0xD6, 0xEF9D);
    /// <summary><c>Music_BossFight1</c> at <c>$D7:BF73</c>; first boss-fight music-data upload selected by $24, not a numbered track within the second boss bank.</summary>
    public static readonly SnesAddress BossFight1 = new(0xD7, 0xBF73);
    /// <summary><c>Music_BossFight2</c> at <c>$D8:99B2</c>; second boss-fight music-data upload selected by $27, separate from its Baby Metroid variant at $45.</summary>
    public static readonly SnesAddress BossFight2 = new(0xD8, 0x99B2);
    /// <summary><c>Music_MiniBossFight</c> at <c>$D8:EA8B</c>; miniboss music-data upload selected by $2A.</summary>
    public static readonly SnesAddress MiniBossFight = new(0xD8, 0xEA8B);
    /// <summary><c>Music_Ceres</c> at <c>$D9:B67B</c>; Ceres music-data upload selected by $2D, distinct from the narrated introduction bank.</summary>
    public static readonly SnesAddress Ceres = new(0xD9, 0xB67B);
    /// <summary><c>Music_WreckedShip</c> at <c>$D9:F5DD</c>; Wrecked Ship music-data upload selected by $30.</summary>
    public static readonly SnesAddress WreckedShip = new(0xD9, 0xF5DD);
    /// <summary><c>Music_ZebesExplosion</c> at <c>$DA:B650</c>; escape/explosion cinematic music-data upload selected by $33, separate from the subsequent credits bank.</summary>
    public static readonly SnesAddress ZebesExplosion = new(0xDA, 0xB650);
    /// <summary><c>Music_Intro</c> at <c>$DA:D63B</c>; narrated-introduction music-data upload selected by $36, distinct from the two spoken opening-line banks.</summary>
    public static readonly SnesAddress Intro = new(0xDA, 0xD63B);
    /// <summary><c>Music_Death</c> at <c>$DB:A40F</c>; death-sequence music-data upload selected by $39.</summary>
    public static readonly SnesAddress Death = new(0xDB, 0xA40F);
    /// <summary><c>Music_Credits</c> at <c>$DB:DF4F</c>; ending/credits music-data upload selected by $3C.</summary>
    public static readonly SnesAddress Credits = new(0xDB, 0xDF4F);
    /// <summary><c>Music_TheLastMetroidIsInCaptivity</c> at <c>$DC:AF6C</c>; first spoken opening-line upload selected by $3F.</summary>
    public static readonly SnesAddress LastMetroidInCaptivity = new(0xDC, 0xAF6C);
    /// <summary><c>Music_TheGalaxyIsAtPeace</c> at <c>$DC:FAC7</c>; second spoken opening-line upload selected by $42.</summary>
    public static readonly SnesAddress GalaxyAtPeace = new(0xDC, 0xFAC7);
    /// <summary><c>Music_BabyMetroid_BossFight2</c> at <c>$DD:B104</c>; Baby Metroid encounter upload selected by $45, sharing the boss-fight-two theme but retaining its own stream identity.</summary>
    public static readonly SnesAddress BabyMetroidBossFight2 = new(0xDD, 0xB104);
    /// <summary><c>Music_SamusTheme_UpperCrateria</c> at <c>$DE:81C1</c>; Samus-theme upload selected by $48, sharing the upper-Crateria theme but retaining its own stream identity.</summary>
    public static readonly SnesAddress SamusThemeUpperCrateria = new(0xDE, 0x81C1);
}
