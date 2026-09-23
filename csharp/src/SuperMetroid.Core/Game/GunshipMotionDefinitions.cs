namespace SuperMetroid.Core.Game;

/// <summary>One timer and signed whole-pixel delta in the gunship idle-bob cycle.</summary>
internal readonly record struct GunshipIdleBobDefinition(byte Timer, sbyte YDelta);

/// <summary>Compiled physical motion schedules for the Landing Site gunship.</summary>
internal static class GunshipMotionDefinitions
{
    /// <summary>
    /// <c>ShipBrakesMovementData</c> at <c>$A2:A622-$A2:A643</c>. The native brake
    /// timer is a word index that advances once per frame through all seventeen deltas.
    /// </summary>
    /// <remarks>Issues #625 and #945 exact run model, i=0..16: +1 for i&lt;6, zero for 6..10, -1 for 11..16.
    /// Equivalently clamp(truncate((8-i)/3), -1, +1), truncating toward zero rather than floor.
    /// LookupTableResearch checks all seventeen signed words against ROM, pinned assembly, and this table.
    /// The six/five/six authored phase lengths are retained; no physical deceleration law is inferred.</remarks>
    private static ReadOnlySpan<short> LandingBrakeYDeltas =>
        [1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1];

    /// <summary>
    /// <c>Function_Ship_Hover.timer/YVelocity</c> at <c>$A2:A7CF-$A2:A7D6</c>.
    /// The four records are byte-packed timer/signed-Y pairs.
    /// </summary>
    /// <remarks>Issues #625 and #946 exact four-phase model: timer=16; Y=1-2*((i XOR (i&gt;&gt;1)) &amp; 1), i=0..3.
    /// This Gray-code sign pattern gives +1,-1,-1,+1 without four records. All eight bytes are
    /// checked against ROM, pinned assembly, and compiled data by LookupTableResearch.
    /// Input bounds and the caller's timer expiration behavior remain unchanged.</remarks>
    private static readonly GunshipIdleBobDefinition[] IdleBobCycle =
    [
        new(0x10, 1),
        new(0x10, -1),
        new(0x10, -1),
        new(0x10, 1),
    ];

    /// <summary>Returns the signed Y delta for one authored landing-brake frame.</summary>
    internal static short LandingBrakeYDelta(ushort frameIndex)
    {
        if (frameIndex >= LandingBrakeYDeltas.Length)
        {
            throw new InvalidDataException(
                $"Gunship landing-brake frame {frameIndex} exceeds its 17-frame schedule.");
        }

        return LandingBrakeYDeltas[frameIndex];
    }

    /// <summary>Returns one of the four authored idle-bob phases.</summary>
    internal static GunshipIdleBobDefinition IdleBob(ushort phase)
    {
        if (phase >= IdleBobCycle.Length)
        {
            throw new InvalidDataException(
                $"Gunship idle-bob phase {phase} exceeds its four-record cycle.");
        }

        return IdleBobCycle[phase];
    }
}
