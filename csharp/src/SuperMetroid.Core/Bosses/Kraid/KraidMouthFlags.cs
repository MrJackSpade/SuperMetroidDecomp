namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's mouth shot latches, each set, cleared and tested on its own. The mouth's pending reopens are
/// <see cref="KraidEnemyState.PendingMouthReopens"/>.
/// </summary>
[Flags]
public enum KraidMouthFlags
{
    /// <summary>No latch is set and no reopen is pending.</summary>
    None = 0,
    /// <summary>A charged shot hit the mouth or body this pass.</summary>
    Hit = 1 << 0,
    /// <summary>A body hit started the eye glow with a charged shot, arming the reopen.</summary>
    ReopenArmed = 1 << 1,
    /// <summary>A mouth hit landed while the reopen was armed.</summary>
    ReopenPending = 1 << 2,
}
