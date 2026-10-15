namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The masks bank $AA and Torizo's bank-$86 spawns apply to the room's random numbers. A decision that tests a mask is
/// taken when every masked bit is clear, so each mask's bit count sets its odds.
/// </summary>
internal static class TorizoRandomDefinitions
{
    /// <summary>Bomb Torizo drools while hurt when none of these bits is set: one time in sixteen.</summary>
    internal const ushort LowHealthDroolMask = 0x8142;

    /// <summary>Golden Torizo's low-health call is skipped when either of these bits is set: taken one time in four.</summary>
    internal const ushort LowHealthCallMask = 0x0102;

    /// <summary>Golden Torizo's medium-range attack is skipped when either of these bits is set: taken one time in four.</summary>
    internal const ushort MediumRangeAttackMask = 0x0110;

    /// <summary>Golden Torizo's forward leap waits while held directions draw neither of these bits: one time in four.</summary>
    internal const ushort ForwardLeapMask = 0x0101;

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

    /// <summary>Both statues' dying drop scatter: six high-byte bits down a 64-pixel band.</summary>
    internal const ushort DropYMask = 0x3f00;
}
