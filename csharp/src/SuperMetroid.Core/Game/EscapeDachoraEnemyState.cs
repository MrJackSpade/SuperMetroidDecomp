namespace SuperMetroid.Core.Game;

/// <summary>
/// Debugger-visible typed marker for escape Dachora enemy <c>$F313</c>. Dachora owns no
/// private variables: all of its movement is authored directly in the three ROM instruction
/// lists at <c>$B3:E964-$EAA7</c>. These read-only properties expose that live script state
/// without inventing a parallel host state machine.
/// </summary>
public sealed class EscapeDachoraEnemyState
{
    /// <summary>Native Dachora enemy slot; movement remains in its bank-$B3 instruction lists.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Retains the underlying slot for debugger inspection of script-driven Dachora state.</summary>
    internal EscapeDachoraEnemyState(RoomEnemySlot slot) => _slot = slot;
}
