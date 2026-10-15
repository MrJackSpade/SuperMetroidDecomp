namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Norfair Rio's idle and swoop animations and its flames' animations as typed programs (#641), one per bank-$A2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="NorfairRioAnimations"/>.
/// </summary>
internal enum NorfairRioProgram
{
    /// <summary><c>InstList_Geruta_Main_Idle</c> at $A2:C0F1.</summary>
    Idle,
    /// <summary><c>InstList_Geruta_Main_Swoop_StartDescending</c> at $A2:C107.</summary>
    StartDescending,
    /// <summary><c>InstList_Geruta_Main_Swoop_Descending</c> at $A2:C12F.</summary>
    Descending,
    /// <summary><c>InstList_Geruta_Main_Swoop_StartAscending</c> at $A2:C145.</summary>
    StartAscending,
    /// <summary><c>InstList_Geruta_Main_Swoop_Ascending</c> at $A2:C179.</summary>
    Ascending,
    /// <summary><c>InstList_Geruta_Flames_Ascending</c> at $A2:C18F.</summary>
    FlamesAscending,
    /// <summary><c>InstList_Geruta_Flames_Descending</c> at $A2:C1A3.</summary>
    FlamesDescending,
}
