namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The fixed words Torizo's movement routines and projectile pre-instructions add, mask or compare: native immediate
/// operands whose bit pattern, not their magnitude, carries the behavior.
/// </summary>
internal static class TorizoMotionDefinitions
{
    /// <summary>The high nibble of a raw 8.8 speed, which the runaway test examines.</summary>
    private const ushort RunawaySpeedNibble = 0xf000;

    /// <summary>A raw 8.8 speed of sixteen pixels per frame: a falling projectile's runaway speed.</summary>
    internal const ushort RunawaySpeed = 0x1000;

    /// <summary>The bits of a raw 8.8 speed above half a pixel per frame.</summary>
    private const ushort SettledBounceBits = 0xff80;

    /// <summary>Clears a whole pixel coordinate's position within its sixteen-pixel block.</summary>
    internal const ushort BlockMask = 0xfff0;

    /// <summary>The native <c>ADC #$FFFB</c> that lifts the initial drool five pixels, whose carry its fall speed adds.</summary>
    internal const ushort InitialDroolLift = 0xfffb;

    /// <summary>The native <c>ADC #$FFF8</c> that places a left-facing Torizo's initial drool eight pixels left.</summary>
    internal const ushort InitialDroolLeftOffset = 0xfff8;

    /// <summary>The angle bit that turns a thrown super missile's aim to the half of the circle away from Samus.</summary>
    internal const byte AwayHalfTurn = 0x80;

    /// <summary>The angle bits that keep a thrown super missile's aim in the half of the circle toward Samus.</summary>
    internal const byte TowardHalfTurn = 0x7f;

    /// <summary>
    /// Golden Torizo's egg hatch timer. $86:B001 masks the literal opcode byte $E2 rather than a random number, an
    /// authentic bug that fixes the timer at 66 updates.
    /// </summary>
    internal const ushort EggHatchTimer = (0x00e2 & 0x001f) + 64;

    /// <summary>
    /// Whether a falling projectile reached the runaway speed: its raw 8.8 speed's high nibble is one, sixteen to
    /// thirty-one pixels per frame.
    /// </summary>
    internal static bool IsRunaway(SnesSignedEightEight speed) => (speed.RawValue & RunawaySpeedNibble) == RunawaySpeed;

    /// <summary>Whether a bounced Chozo orb's rebound has settled: an upward speed under half a pixel per frame.</summary>
    internal static bool HasSettled(SnesSignedEightEight speed) => (speed.RawValue & SettledBounceBits) == SettledBounceBits;
}
