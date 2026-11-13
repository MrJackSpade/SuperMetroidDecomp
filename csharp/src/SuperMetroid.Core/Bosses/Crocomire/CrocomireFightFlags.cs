namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's fight latches. The original game sets, clears and tests each one on its own, so they compose, and the
/// values here are arbitrary bits (#1413). The unused attack branches give some latches meanings that the retail fight
/// never reaches.
/// </summary>
[Flags]
public enum CrocomireFightFlags
{
    /// <summary>No latch is set.</summary>
    None = 0,
    /// <summary>Makes the unused attack chooser take the move-until-claw-contact branch.</summary>
    UnusedPreferClawAttack = 1,
    /// <summary>Set when the unused attack sequence restarts the art.</summary>
    UnusedAttackSequenceStarted = 2,
    /// <summary>Set while the unused claw-moving branch runs.</summary>
    UnusedClawsMoving = 4,
    /// <summary>A mouth hit is waiting for the fight AI to start a backward step.</summary>
    DamageReactionPending = 8,
    /// <summary>Kept by the unused near-spike-wall charge's reset mask, but no shipped routine sets or tests it.</summary>
    UnusedKeptByChargeReset = 16,
    /// <summary>Set when the unused forward steps run out.</summary>
    UnusedForwardStepsDone = 32,
    /// <summary>A claw touched Samus since the claw-contact branch last cleared it.</summary>
    ClawTouchedSamus = 64,
    /// <summary>Crocomire has woken and engaged Samus, or a power bomb restarted that engagement.</summary>
    Engaged = 128,
}
