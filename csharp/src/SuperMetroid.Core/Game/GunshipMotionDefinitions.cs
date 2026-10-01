namespace SuperMetroid.Core.Game;

/// <summary>One timer and signed whole-pixel delta in the gunship idle-bob cycle.</summary>
internal readonly record struct GunshipIdleBobDefinition(byte Timer, sbyte YDelta);

/// <summary>Exact bounded motion policies for the Landing Site gunship.</summary>
internal static class GunshipMotionDefinitions
{
    /// <summary>$A2:A622 ShipBrakesMovementData: seventeen signed word Y deltas.</summary>
    internal const int BrakeReferenceAddress = 0xa2a622;
    /// <summary>$A2:A7CF ProcessShipHover.timer: four unsigned bytes at stride two.</summary>
    internal const int HoverTimerReferenceAddress = 0xa2a7cf;
    /// <summary>$A2:A7D0 ProcessShipHover.YVelocity: four signed bytes at stride two.</summary>
    internal const int HoverDeltaReferenceAddress = 0xa2a7d0;

    /// <summary>Returns +1 for frames 0..5, zero for 6..10, and -1 for 11..16.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against all NTSC J/U v1.0 words and pinned
    /// bank_A2.asm. These are explicit six/five/six braking intervals, not a sampled
    /// physical acceleration curve. A range switch expresses the authored schedule
    /// more directly than the older clamp/truncated-division hypothesis.
    /// Callers add the signed whole-pixel delta with word wrapping, then advance.
    /// </remarks>
    internal static short LandingBrakeYDelta(ushort frameIndex)
    {
        if (frameIndex >= 17)
            throw new InvalidDataException(
                $"Gunship landing-brake frame {frameIndex} exceeds its 17-frame schedule.");
        return frameIndex switch { < 6 => 1, < 11 => 0, _ => -1 };
    }

    /// <summary>Returns timer 16 and signed delta +1,-1,-1,+1 for phases 0..3.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against each of the two byte fields in
    /// NTSC J/U v1.0 and pinned ProcessShipHover. Timer and delta have separate
    /// original-data proofs. Endpoint phases move down, middle phases move up;
    /// this case policy is clearer than the older Gray-code sign expression.
    /// Validate before selecting. The caller owns expiration and phase wrap.
    /// </remarks>
    internal static GunshipIdleBobDefinition IdleBob(ushort phase)
    {
        if (phase >= 4)
            throw new InvalidDataException(
                $"Gunship idle-bob phase {phase} exceeds its four-record cycle.");
        return new(16, phase is 0 or 3 ? (sbyte)1 : (sbyte)-1);
    }
}
