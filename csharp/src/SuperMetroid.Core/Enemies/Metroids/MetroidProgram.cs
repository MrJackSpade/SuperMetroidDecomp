namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// The Metroid's chasing and draining pulse loops as typed programs (#641), one per bank-$A3 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="MetroidAnimations"/>.
/// </summary>
internal enum MetroidProgram
{
    /// <summary><c>InstList_Metroid_ChasingSamus</c> at $A3:E9CF.</summary>
    ChasingSamus,
    /// <summary><c>InstList_Metroid_DrainingSamus</c> at $A3:EA25.</summary>
    DrainingSamus,
}
