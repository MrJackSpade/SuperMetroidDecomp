namespace SuperMetroid.Core.Game;

/// <summary>Authored bank-$A6 Ceres Ridley palette sources and native CGRAM destinations.</summary>
public static class CeresRidleyPaletteRomData
{
    /// <summary>Door and Baby-container target colors loaded during Ceres Ridley initialization at $A6:E16F.</summary>
    public const int StartColors = 0xa6e16f;
    /// <summary>Thirty-two RGB555 color words spanning the complete Ceres door/container and initial Baby palettes at $A6:E16F-$E1AE.</summary>
    public const int StartColorCount = 32;
    /// <summary>CGRAM color index 160 (native byte offset $0140), the start of OBJ palette 2; the 32-color initialization also fills OBJ palette 3.</summary>
    public const int StartCgramIndex = 0x140 / 2;

    /// <summary>
    /// Four fifteen-color Ceres Baby shades at $A6:E1F1-$E268. The private
    /// $BFE1 draw callback copies one row to OBJ palette three, colors 1..15.
    /// </summary>
    public const int BabyColors = 0xa6e1f1;
    /// <summary>Four ordered Baby color rows: end-of-list/resting, horizontal squish, round, and vertical squish, selected by its animation instructions.</summary>
    public const int BabyRowCount = 4;
    /// <summary>Fifteen RGB555 words per Baby shade, excluding the palette's transparent color zero.</summary>
    public const int BabyColorCount = 15;
    /// <summary>CGRAM color index 177 (native byte offset $0162), OBJ palette 3 color 1, written by palette callback $A6:BFE1.</summary>
    public const int BabyCgramIndex = 0x162 / 2;

    /// <summary>Sixteen three-color eye-fade rows beginning at $A6:E2AA.</summary>
    public const int EyeFadeColors = EnemyRomTablePointers.Ceres.RidleyEyeFadePaletteRows;
    /// <summary>Sixteen eye shades, traversed from row 15 to row 0 and then held at row 0 by the native per-AI-update fade-index program.</summary>
    public const int EyeFadeRowCount = 16;
    /// <summary>Three RGB555 eye-color words per row, occupying palette colors $C..$E.</summary>
    public const int EyeFadeColorCount = 3;
    /// <summary>CGRAM color index 252 (native byte offset $01F8), OBJ palette 7 color $C; each eye row updates indices 252..254.</summary>
    public const int EyeFadeCgramIndex = 252;

    /// <summary>Sixteen eleven-color body-fade rows at $A6:E30A-$E469.</summary>
    public const int BodyFadeColors = 0xa6e30a;
    /// <summary>Sixteen black-to-visible body shades consumed in ascending order, one row every two Ceres Ridley AI updates.</summary>
    public const int BodyFadeRowCount = 16;
    /// <summary>Eleven RGB555 body-color words per fade row ($16 bytes), filling colors 1..$B without overwriting the eye colors.</summary>
    public const int BodyFadeColorCount = 11;
    /// <summary>CGRAM color index 145 (native byte offset $0122), actually OBJ palette 1 color 1 despite the legacy Bg name; receives the body fade alongside OBJ palette 7.</summary>
    public const int BodyFadeBgCgramIndex = 0x122 / 2;
    /// <summary>CGRAM color index 241 (native byte offset $01E2), OBJ palette 7 color 1; each body-fade row updates indices 241..251.</summary>
    public const int BodyFadeObjCgramIndex = 0x1e2 / 2;

    /// <summary>Three fourteen-color health shades at $A6:E46A, shared by Ceres and Norfair Ridley.</summary>
    public const int HealthColors = 0xa6e46a;
    /// <summary>Three shared Ridley damage shades, selected by health thresholds in Norfair but by accepted-hit count in Ceres.</summary>
    public const int HealthRowCount = 3;
    /// <summary>Fourteen RGB555 words per damage shade ($1C bytes), covering OBJ palette 7 colors 1..$E.</summary>
    public const int HealthColorCount = 14;
    /// <summary>Row 0, the native below-9000-health shade, selected for 50..69 accepted Ceres hits while fight mode is active.</summary>
    public const int HealthMidRow = 0;
    /// <summary>Row 2, the native below-1800-health shade, selected for every Ceres hit count from 70 onward because the cartridge omits the branch after its 90-hit comparison.</summary>
    public const int HealthLateRow = 2;
    /// <summary>CGRAM color index 241 (native byte offset $01E2), OBJ palette 7 color 1; a health shade replaces indices 241..254.</summary>
    public const int HealthCgramIndex = 0x1e2 / 2;

    /// <summary>Sixteen three-color Ceres self-destruct alarm frames at $A6:C1DF.</summary>
    public const int AlarmColors = 0xa6c1df;
    /// <summary>Sixteen cyclic EMERGENCY-text shades, advancing every fourth frame-counter tick and wrapping with mask $0F.</summary>
    public const int AlarmRowCount = 16;
    /// <summary>Three RGB555 text colors per alarm frame ($06 bytes), excluding the transparent palette entry.</summary>
    public const int AlarmColorCount = 3;
    /// <summary>CGRAM color index 97 (native byte offset $00C2), BG palette 6 color 1; the alarm cycle updates indices 97..99.</summary>
    public const int AlarmCgramIndex = 97;

    /// <summary>Retreat BG colors one through fifteen at $A6:A9E3.</summary>
    public const int RetreatBgColors = 0xa6a9e3;
    /// <summary>Fifteen RGB555 words installed by real-retreat setup $A6:A9A0, covering BG palette 5 colors 1..$F.</summary>
    public const int RetreatBgColorCount = 15;
    /// <summary>CGRAM color index 81 (native byte offset $00A2), BG palette 5 color 1, for the getaway backdrop.</summary>
    public const int RetreatBgCgramIndex = 0x0a2 / 2;
    /// <summary>Eight retreat colors at $A6:AA01 copied to both BG and OBJ palettes.</summary>
    public const int RetreatSharedColors = 0xa6aa01;
    /// <summary>Eight RGB555 words shared between BG palette 2 and OBJ palette 7 colors 1..8 during real-retreat setup.</summary>
    public const int RetreatSharedColorCount = 8;
    /// <summary>CGRAM color index 33 (native byte offset $0042), BG palette 2 color 1, receiving the shared retreat shade.</summary>
    public const int RetreatSharedBgCgramIndex = 0x042 / 2;
    /// <summary>CGRAM color index 241 (native byte offset $01E2), OBJ palette 7 color 1, receiving the same eight retreat colors as BG palette 2.</summary>
    public const int RetreatSharedObjCgramIndex = 0x1e2 / 2;

    /// <summary>Nine Mode-7 zoom shades at $A6:B107, selected by zoom high byte zero through eight.</summary>
    public const int Mode7ZoomColors = 0xa6b107;
    /// <summary>Nine Mode-7 getaway shades selected by zoom high-byte values 0..8, rather than by an independent animation timer.</summary>
    public const int Mode7ZoomRowCount = 9;
    /// <summary>Fifteen RGB555 color words copied per zoom shade; the sixteenth stored word is padding and does not replace palette color zero.</summary>
    public const int Mode7ZoomColorCount = 15;
    /// <summary>Each zoom shade occupies sixteen words; the final word is not copied.</summary>
    public const int Mode7ZoomRowByteStride = 32;
    /// <summary>CGRAM color index 81 (native byte offset $00A2), BG palette 5 color 1, updated by native zoom-palette routine $A6:B0EF.</summary>
    public const int Mode7ZoomCgramIndex = 0x0a2 / 2;
}
