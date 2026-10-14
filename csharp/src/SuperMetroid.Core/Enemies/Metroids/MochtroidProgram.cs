namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// Mochtroid's free-flight and attached pulse loops as typed programs (#641), one per bank-$A3 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="MochtroidAnimations"/>.
/// </summary>
internal enum MochtroidProgram
{
    /// <summary><c>InstList_Mochtroid_NotTouchingSamus</c> at $A3:A745.</summary>
    FreeFlight,
    /// <summary><c>InstList_Mochtroid_TouchingSamus</c> at $A3:A759.</summary>
    Attached,
}
