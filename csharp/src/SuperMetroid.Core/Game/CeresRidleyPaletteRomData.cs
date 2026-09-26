namespace SuperMetroid.Core.Game;

/// <summary>Authored bank-$A6 Ceres Ridley palette sources and native CGRAM destinations.</summary>
public static class CeresRidleyPaletteRomData
{
    /// <summary>Bank $A6, which owns Ceres Ridley's wrapped Mode-7 palette source.</summary>
    public const int Bank = 0xa60000;
    /// <summary>Door and Baby-container target colors loaded during Ceres Ridley initialization at $A6:E16F.</summary>
    public const int StartColors = 0xa6e16f;
    public const int StartColorCount = 32;
    public const int StartCgramIndex = 0x140 / 2;

    /// <summary>
    /// Four fifteen-color Ceres Baby shades at $A6:E1F1-$E268. The private
    /// $BFE1 draw callback copies one row to OBJ palette one, colors 1..15.
    /// </summary>
    public const int BabyColors = 0xa6e1f1;
    public const int BabyRowCount = 4;
    public const int BabyColorCount = 15;
    public const int BabyCgramIndex = 0x162 / 2;

    /// <summary>Sixteen three-color eye-fade rows beginning at $A6:E2AA.</summary>
    public const int EyeFadeColors = EnemyRomTablePointers.Ceres.RidleyEyeFadePaletteRows;
    public const int EyeFadeRowCount = 16;
    public const int EyeFadeColorCount = 3;
    public const int EyeFadeCgramIndex = 252;

    /// <summary>Sixteen eleven-color body-fade rows at $A6:E30A-$E469.</summary>
    public const int BodyFadeColors = 0xa6e30a;
    public const int BodyFadeRowCount = 16;
    public const int BodyFadeColorCount = 11;
    public const int BodyFadeBgCgramIndex = 0x122 / 2;
    public const int BodyFadeObjCgramIndex = 0x1e2 / 2;

    /// <summary>Three fourteen-color health shades at $A6:E46A, shared by Ceres and Norfair Ridley.</summary>
    public const int HealthColors = 0xa6e46a;
    public const int HealthRowCount = 3;
    public const int HealthColorCount = 14;
    public const int HealthMidRow = 0;
    public const int HealthLateRow = 2;
    public const int HealthCgramIndex = 0x1e2 / 2;

    /// <summary>Sixteen three-color Ceres self-destruct alarm frames at $A6:C1DF.</summary>
    public const int AlarmColors = 0xa6c1df;
    public const int AlarmRowCount = 16;
    public const int AlarmColorCount = 3;
    public const int AlarmCgramIndex = 97;

    /// <summary>Retreat BG colors one through fifteen at $A6:A9E3.</summary>
    public const int RetreatBgColors = 0xa6a9e3;
    public const int RetreatBgColorCount = 15;
    public const int RetreatBgCgramIndex = 0x0a2 / 2;
    /// <summary>Eight retreat colors at $A6:AA01 copied to both BG and OBJ palettes.</summary>
    public const int RetreatSharedColors = 0xa6aa01;
    public const int RetreatSharedColorCount = 8;
    public const int RetreatSharedBgCgramIndex = 0x042 / 2;
    public const int RetreatSharedObjCgramIndex = 0x1e2 / 2;

    /// <summary>Nine Mode-7 zoom shades at $A6:B107, selected by zoom high byte zero through eight.</summary>
    public const int Mode7ZoomColors = 0xa6b107;
    public const int Mode7ZoomRowCount = 9;
    public const int Mode7ZoomColorCount = 15;
    /// <summary>Each zoom shade occupies sixteen words; the final word is not copied.</summary>
    public const int Mode7ZoomRowByteStride = 32;
    public const int Mode7ZoomCgramIndex = 0x0a2 / 2;
}
