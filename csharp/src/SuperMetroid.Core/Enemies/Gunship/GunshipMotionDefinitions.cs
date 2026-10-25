namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>Exact bounded motion policies for the Landing Site gunship.</summary>
internal static class GunshipMotionDefinitions
{

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

    /// <summary>Above this Y the descending ship moves $4.8000 pixels per call; below it, $2.8000.</summary>
    internal const ushort SlowDescentY = 0x0300;

    /// <summary>The descending ship's speed above <see cref="SlowDescentY"/>, 16.16 pixels per call.</summary>
    internal const uint FastDescentSpeed = 0x0004_8000u;

    /// <summary>The descending ship's speed below <see cref="SlowDescentY"/>, 16.16 pixels per call.</summary>
    internal const uint SlowDescentSpeed = 0x0002_8000u;

    /// <summary>The landed top hull's Y, where the descent clamps.</summary>
    internal const ushort LandedTopY = 0x045f;

    /// <summary>The landed bottom hull's Y.</summary>
    internal const ushort LandedBottomY = 0x0487;

    /// <summary>The landed entrance pad's Y.</summary>
    internal const ushort LandedPadY = 0x045e;

    /// <summary>The landed ship's X, where Samus stands to board.</summary>
    internal const ushort LandedX = 0x0480;

    /// <summary>The escaping ship's starting 8.8 upward velocity.</summary>
    internal const ushort EscapeStartVelocity = 0x0200;

    /// <summary>The escaping ship's 8.8 acceleration per call.</summary>
    internal const ushort EscapeAcceleration = 0x0040;

    /// <summary>The escaping velocity whose whole byte reaching ten caps it.</summary>
    internal const ushort EscapeVelocityCapTrigger = 0x0a00;

    /// <summary>The escaping ship's capped 8.8 velocity.</summary>
    internal const ushort EscapeVelocityCap = 0x0900;
}
