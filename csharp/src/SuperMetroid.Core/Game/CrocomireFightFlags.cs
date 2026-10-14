namespace SuperMetroid.Core.Game;

/// <summary>
/// Crocomire's fight latches in body variable B. The cartridge sets, clears and tests each bit on
/// its own, so the bits compose. The low nibble is a separate saturating hit count stored in the
/// same word (see <see cref="CrocomireFightFlagBits"/>). The unused attack branches give some bits
/// meanings that the retail fight never reaches.
/// </summary>
[Flags]
public enum CrocomireFightFlags : ushort
{
    /// <summary>No latch is set.</summary>
    None = 0,
    /// <summary>The low-nibble hit count field. It is a counter, not a flag.</summary>
    HitCountField = 0x000f,
    /// <summary>Bit $0100: makes the unused attack chooser at <c>$A4:891B</c> take the move-until-claw-contact branch.</summary>
    UnusedPreferClawAttack = 0x0100,
    /// <summary>Bit $0200: set when the unused attack sequence at <c>$A4:88D2</c> restarts the art.</summary>
    UnusedAttackSequenceStarted = 0x0200,
    /// <summary>Bit $0400: set while the unused claw-moving branch at <c>$A4:89A8</c> runs.</summary>
    UnusedClawsMoving = 0x0400,
    /// <summary>Bit $0800: a mouth hit is waiting for the fight AI to start a backward step.</summary>
    DamageReactionPending = 0x0800,
    /// <summary>Bit $1000: kept by the unused near-spike-wall charge's reset mask, but no shipped routine sets or tests it.</summary>
    UnresolvedBit12 = 0x1000,
    /// <summary>Bit $2000: set when the unused forward steps at <c>$A4:8940</c> run out.</summary>
    UnusedForwardStepsDone = 0x2000,
    /// <summary>Bit $4000: a claw touched Samus since the claw-contact branch last cleared it.</summary>
    ClawTouchedSamus = 0x4000,
    /// <summary>Bit $8000: Crocomire has woken and engaged Samus, or a power bomb restarted that engagement.</summary>
    Engaged = 0x8000,
}
