namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The fixed speeds, offsets and turns Torizo's movement routines and projectile pre-instructions use.</summary>
internal static class TorizoMotionDefinitions
{
    /// <summary>
    /// A raw 8.8 speed of sixteen pixels per frame: a falling projectile at this speed and below twice it has run away.
    /// </summary>
    internal const ushort RunawaySpeed = 4096;

    /// <summary>
    /// The upward raw 8.8 speed, half a pixel per frame, below which a bounced Chozo orb's rebound has settled.
    /// </summary>
    internal const int SettledBounceSpeed = 128;

    /// <summary>The pixels of one room block: a landing projectile is placed relative to its block's top.</summary>
    internal const int BlockPixels = 16;

    /// <summary>
    /// The pixels the initial drool is lifted above the jittered spawn height. The cartridge adds this as
    /// <c>ADC #$FFFB</c>, whose carry (set unless the lift wraps) its fall speed adds.
    /// </summary>
    internal const int InitialDroolLift = 5;

    /// <summary>
    /// The pixels the initial drool is placed to the facing side of the jittered spawn position (the cartridge's
    /// <c>ADC #$0008</c> facing right and <c>ADC #$FFF8</c> facing left).
    /// </summary>
    internal const int InitialDroolSideOffset = 8;

    /// <summary>The angle bit that turns a thrown super missile's aim to the half of the circle away from Samus.</summary>
    internal const byte AwayHalfTurn = 0x80;

    /// <summary>The angle bits that keep a thrown super missile's aim in the half of the circle toward Samus.</summary>
    internal const byte TowardHalfTurn = 0x7f;

    /// <summary>
    /// Golden Torizo's egg hatch timer. $86:B001 masks the literal opcode byte $E2 rather than a random number, an
    /// authentic bug that fixes the timer at 66 updates.
    /// </summary>
    internal const ushort EggHatchTimer = (0x00e2 & 0x001f) + 64;
}
