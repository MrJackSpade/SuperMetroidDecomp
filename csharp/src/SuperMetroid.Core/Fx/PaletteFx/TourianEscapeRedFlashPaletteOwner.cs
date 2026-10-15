namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The mutually exclusive early Tourian escape red-flash program.</summary>
public enum TourianEscapeRedFlashPaletteOwner
{
    /// <summary>PalFxDef_Tourian8 ($8D:FFC9), flashing six shutter colors from CGRAM byte $0132 through the fourteen two-tick records at $F7A9.</summary>
    Shutter,
    /// <summary>PalFxDef_Tourian10 ($8D:FFCD), flashing four background colors from CGRAM byte $0070 through the fourteen four-tick records at $F891.</summary>
    Background,
}
