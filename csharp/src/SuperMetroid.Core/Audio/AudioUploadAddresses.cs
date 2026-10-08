namespace SuperMetroid.Core.Audio;

/// <summary>LoROM source addresses of every retail non-PAL SPC upload stream.</summary>
/// <remarks>
/// Values are full 24-bit cartridge source addresses, not SPC RAM destinations or track selectors.
/// Each stream contains length/destination block headers and a zero-length terminator; its length is read from that structure,
/// not inferred from the next address. Music-data selectors are byte offsets into <c>Music_Pointers</c> at <c>$8F:E7E1</c>.
/// Import validates mapped source addresses and bounded termination; these constants themselves perform no reads or validation.
/// </remarks>
public static class AudioUploadAddresses
{
    /// <summary><c>SPC_Engine</c> at <c>$CF:8000</c>; common upload containing the resident driver, sound-effect programs, and shared samples, catalog data selector $00.</summary>
    public const int SpcEngine = 0xCF8000;
    /// <summary><c>Music_TitleSequence</c> at <c>$D0:E20D</c>; music-data upload selected by $03 for the title sequence, distinct from its track commands.</summary>
    public const int TitleSequence = 0xD0E20D;
    /// <summary><c>Music_EmptyCrateria</c> at <c>$D1:B62A</c>; empty-Crateria music-data upload selected by $06, distinct from the later regional Crateria banks.</summary>
    public const int EmptyCrateria = 0xD1B62A;
    /// <summary><c>Music_LowerCrateria</c> at <c>$D2:88CA</c>; lower-Crateria music-data upload selected by $09.</summary>
    public const int LowerCrateria = 0xD288CA;
    /// <summary><c>Music_UpperCrateria</c> at <c>$D2:D9B6</c>; upper-Crateria music-data upload selected by $0C, separate from the Samus-theme variant at $48.</summary>
    public const int UpperCrateria = 0xD2D9B6;
    /// <summary><c>Music_GreenBrinstar</c> at <c>$D3:933C</c>; green-Brinstar music-data upload selected by $0F.</summary>
    public const int GreenBrinstar = 0xD3933C;
    /// <summary><c>Music_RedBrinstar</c> at <c>$D3:E812</c>; red-Brinstar music-data upload selected by $12.</summary>
    public const int RedBrinstar = 0xD3E812;
    /// <summary><c>Music_UpperNorfair</c> at <c>$D4:B86C</c>; upper-Norfair music-data upload selected by $15.</summary>
    public const int UpperNorfair = 0xD4B86C;
    /// <summary><c>Music_LowerNorfair</c> at <c>$D4:F420</c>; lower-Norfair music-data upload selected by $18.</summary>
    public const int LowerNorfair = 0xD4F420;
    /// <summary><c>Music_Maridia</c> at <c>$D5:C844</c>; Maridia music-data upload selected by $1B.</summary>
    public const int Maridia = 0xD5C844;
    /// <summary><c>Music_Tourian</c> at <c>$D6:98B7</c>; Tourian music-data upload selected by $1E, separate from the Mother Brain bank.</summary>
    public const int Tourian = 0xD698B7;
    /// <summary><c>Music_MotherBrain</c> at <c>$D6:EF9D</c>; Mother Brain music-data upload selected by $21.</summary>
    public const int MotherBrain = 0xD6EF9D;
    /// <summary><c>Music_BossFight1</c> at <c>$D7:BF73</c>; first boss-fight music-data upload selected by $24, not a numbered track within the second boss bank.</summary>
    public const int BossFight1 = 0xD7BF73;
    /// <summary><c>Music_BossFight2</c> at <c>$D8:99B2</c>; second boss-fight music-data upload selected by $27, separate from its Baby Metroid variant at $45.</summary>
    public const int BossFight2 = 0xD899B2;
    /// <summary><c>Music_MiniBossFight</c> at <c>$D8:EA8B</c>; miniboss music-data upload selected by $2A.</summary>
    public const int MiniBossFight = 0xD8EA8B;
    /// <summary><c>Music_Ceres</c> at <c>$D9:B67B</c>; Ceres music-data upload selected by $2D, distinct from the narrated introduction bank.</summary>
    public const int Ceres = 0xD9B67B;
    /// <summary><c>Music_WreckedShip</c> at <c>$D9:F5DD</c>; Wrecked Ship music-data upload selected by $30.</summary>
    public const int WreckedShip = 0xD9F5DD;
    /// <summary><c>Music_ZebesExplosion</c> at <c>$DA:B650</c>; escape/explosion cinematic music-data upload selected by $33, separate from the subsequent credits bank.</summary>
    public const int ZebesExplosion = 0xDAB650;
    /// <summary><c>Music_Intro</c> at <c>$DA:D63B</c>; narrated-introduction music-data upload selected by $36, distinct from the two spoken opening-line banks.</summary>
    public const int Intro = 0xDAD63B;
    /// <summary><c>Music_Death</c> at <c>$DB:A40F</c>; death-sequence music-data upload selected by $39.</summary>
    public const int Death = 0xDBA40F;
    /// <summary><c>Music_Credits</c> at <c>$DB:DF4F</c>; ending/credits music-data upload selected by $3C.</summary>
    public const int Credits = 0xDBDF4F;
    /// <summary><c>Music_TheLastMetroidIsInCaptivity</c> at <c>$DC:AF6C</c>; first spoken opening-line upload selected by $3F.</summary>
    public const int LastMetroidInCaptivity = 0xDCAF6C;
    /// <summary><c>Music_TheGalaxyIsAtPeace</c> at <c>$DC:FAC7</c>; second spoken opening-line upload selected by $42.</summary>
    public const int GalaxyAtPeace = 0xDCFAC7;
    /// <summary><c>Music_BabyMetroid_BossFight2</c> at <c>$DD:B104</c>; Baby Metroid encounter upload selected by $45, sharing the boss-fight-two theme but retaining its own stream identity.</summary>
    public const int BabyMetroidBossFight2 = 0xDDB104;
    /// <summary><c>Music_SamusTheme_UpperCrateria</c> at <c>$DE:81C1</c>; Samus-theme upload selected by $48, sharing the upper-Crateria theme but retaining its own stream identity.</summary>
    public const int SamusThemeUpperCrateria = 0xDE81C1;
}
