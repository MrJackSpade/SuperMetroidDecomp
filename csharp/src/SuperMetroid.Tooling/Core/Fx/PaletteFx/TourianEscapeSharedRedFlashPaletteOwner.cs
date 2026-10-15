namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The mutually exclusive Tourian escape entry into the shared red-flash loop.</summary>
public enum TourianEscapeSharedRedFlashPaletteOwner
{
    /// <summary>PalFxDef_Tourian20 ($8D:FFD1), entering at $F941 to select CGRAM byte $A8 and branch over the adjacent setup into the shared $F94D loop.</summary>
    GeneralLevel,
    /// <summary>PalFxDef_Tourian40 ($8D:FFD5), entering at $F949 to select CGRAM byte $E8 for Arkanoid blocks/red orbs and fall through into the shared $F94D loop.</summary>
    ArkanoidBlocksAndRedOrbs,
}
