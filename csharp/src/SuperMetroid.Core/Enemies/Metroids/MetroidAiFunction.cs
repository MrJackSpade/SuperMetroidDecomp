namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// Native dispatch-table indexes stored in ordinary Metroid variable F. These are indexes,
/// not bank-$A3 function addresses: main AI uses the word directly to select one of four
/// movement routines at <c>$A3:EC09</c>.
/// </summary>
public enum MetroidAiFunction
{
    /// <summary>Index 0 selects $A3:EC11, Function_Metroid_0_ChaseSamus: proportional 16.16 attraction toward Samus's X and eight pixels above her Y, capped at three pixels per update.</summary>
    Homing = 0,
    /// <summary>Index 1 selects $A3:ECDC, Function_Metroid_1_LatchOntoSamus: byte-oriented close-range seeking entered on touch before the attachment threshold is reached.</summary>
    ClosingOnSamus = 1,
    /// <summary>Index 2 selects $A3:ED8F, Function_Metroid_2_LatchedOntoSamus: hard-lock position to Samus's X and Y minus eight pixels, clearing movement velocities.</summary>
    AttachedToSamus = 2,
    /// <summary>Index 3 selects $A3:EDAB, Function_Metroid_3_BombedOffSamus: perform four displacement steps after bomb collision detaches the Metroid, then resume homing.</summary>
    PowerBombEscape = 3,
}
