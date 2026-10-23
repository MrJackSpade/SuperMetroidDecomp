using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Cartridge values and helpers of <see cref="CeresRidleyPaletteRomData"/> that only verification reads.</summary>
internal static class CeresRidleyPaletteRomDataConstants
{
    /// <summary>CGRAM color index 160 (native byte offset $0140), the start of OBJ palette 2; the 32-color initialization also fills OBJ palette 3.</summary>
    public const int StartCgramIndex = 0x140 / 2;
    /// <summary>CGRAM color index 177 (native byte offset $0162), OBJ palette 3 color 1, written by palette callback $A6:BFE1.</summary>
    public const int BabyCgramIndex = 0x162 / 2;
    /// <summary>CGRAM color index 252 (native byte offset $01F8), OBJ palette 7 color $C; each eye row updates indices 252..254.</summary>
    public const int EyeFadeCgramIndex = 252;
    /// <summary>CGRAM color index 145 (native byte offset $0122), actually OBJ palette 1 color 1 despite the legacy Bg name; receives the body fade alongside OBJ palette 7.</summary>
    public const int BodyFadeBgCgramIndex = 0x122 / 2;
    /// <summary>CGRAM color index 241 (native byte offset $01E2), OBJ palette 7 color 1; each body-fade row updates indices 241..251.</summary>
    public const int BodyFadeObjCgramIndex = 0x1e2 / 2;
    /// <summary>CGRAM color index 241 (native byte offset $01E2), OBJ palette 7 color 1; a health shade replaces indices 241..254.</summary>
    public const int HealthCgramIndex = 0x1e2 / 2;
    /// <summary>CGRAM color index 81 (native byte offset $00A2), BG palette 5 color 1, for the getaway backdrop.</summary>
    public const int RetreatBgCgramIndex = 0x0a2 / 2;
    /// <summary>CGRAM color index 33 (native byte offset $0042), BG palette 2 color 1, receiving the shared retreat shade.</summary>
    public const int RetreatSharedBgCgramIndex = 0x042 / 2;
    /// <summary>CGRAM color index 241 (native byte offset $01E2), OBJ palette 7 color 1, receiving the same eight retreat colors as BG palette 2.</summary>
    public const int RetreatSharedObjCgramIndex = 0x1e2 / 2;
    /// <summary>CGRAM color index 81 (native byte offset $00A2), BG palette 5 color 1, updated by native zoom-palette routine $A6:B0EF.</summary>
    public const int Mode7ZoomCgramIndex = 0x0a2 / 2;
}
