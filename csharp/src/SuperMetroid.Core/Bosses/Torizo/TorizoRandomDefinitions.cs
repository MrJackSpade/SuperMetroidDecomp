namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The masks bank $AA and Torizo's bank-$86 spawns apply to the room's random numbers. A decision that tests a mask is
/// taken when every masked bit is clear, so each mask's bit count sets its odds.
/// </summary>
internal static class TorizoRandomDefinitions
{
    /// <summary>Bomb Torizo drools while hurt when random bits 1, 6, 8 and 15 are clear: one time in sixteen.</summary>
    internal static readonly TorizoRandomBits LowHealthDrool = TorizoRandomBits.At(1, 6, 8, 15);

    /// <summary>Golden Torizo's low-health call is taken when random bits 1 and 8 are clear: one time in four.</summary>
    internal static readonly TorizoRandomBits LowHealthCall = TorizoRandomBits.At(1, 8);

    /// <summary>Golden Torizo's medium-range attack is taken when random bits 4 and 8 are clear: one time in four.</summary>
    internal static readonly TorizoRandomBits MediumRangeAttack = TorizoRandomBits.At(4, 8);

    /// <summary>Golden Torizo's forward leap goes while held directions draw random bits 0 and 8 clear: one time in four.</summary>
    internal static readonly TorizoRandomBits ForwardLeap = TorizoRandomBits.At(0, 8);

    /// <summary>The initial drool's added fall speed: five low bits of raw 8.8 speed.</summary>
    internal const ushort DroolFallJitterMask = 0x001f;

    /// <summary>A turning Torizo's drool angle: the low byte of a word index into the full circle.</summary>
    internal const ushort DroolTurningAngleMask = 0x01fe;

    /// <summary>A facing Torizo's drool angle spread: four low bits about its facing angle.</summary>
    internal const ushort DroolFacingAngleMask = 0x000f;

    /// <summary>Golden Torizo's eye-beam angle spread: bits 1-4, an even step of up to thirty.</summary>
    internal const ushort EyeBeamSpreadMask = 0x001e;

    /// <summary>The nine-bit word index of a full circle of eye-beam angles.</summary>
    internal const int EyeBeamAngleMask = 0x01ff;

    /// <summary>Golden Torizo's dying drop scatter: the low byte across its 256-pixel strip.</summary>
    internal const ushort GoldenDropXMask = 0x00ff;

    /// <summary>Bomb Torizo's dying drop scatter: seven low bits across its 128-pixel region.</summary>
    internal const ushort BombDropXMask = 0x007f;

    /// <summary>Both statues' dying drop scatter: six bits of the sample's upper random byte down a 64-pixel band.</summary>
    internal const ushort DropYMask = 0x003f;
}
