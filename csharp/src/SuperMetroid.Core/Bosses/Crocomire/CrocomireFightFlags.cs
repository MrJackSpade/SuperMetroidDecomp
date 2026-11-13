namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's fight latches. The game sets, clears and tests each one on its own, so they compose, and the
/// values here are arbitrary bits (#1413).
/// </summary>
[Flags]
public enum CrocomireFightFlags
{
    /// <summary>No latch is set.</summary>
    None = 0,
    /// <summary>A mouth hit is waiting for the fight AI to start a backward step.</summary>
    DamageReactionPending = 1,
    /// <summary>Crocomire has woken and engaged Samus, or a power bomb restarted that engagement.</summary>
    Engaged = 2,
}
