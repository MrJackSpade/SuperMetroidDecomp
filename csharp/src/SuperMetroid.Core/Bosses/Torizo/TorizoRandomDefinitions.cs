using SuperMetroid.Core.Enemies;

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

    /// <summary>The initial drool's added fall speed, 0 through 31 in raw 8.8 units: the selection of bits 0-4.</summary>
    internal static readonly RandomSampleField DroolFallJitter = new(0, 5);

    /// <summary>
    /// A turning Torizo's drool angle, any of the 256 table angles: the selection of bits 1-8, which the cartridge
    /// reads as a word index into the full circle.
    /// </summary>
    internal static readonly RandomSampleField DroolTurningAngle = new(1, 8);

    /// <summary>A facing Torizo's drool angle spread about its facing angle, 0 through 15: the selection of bits 0-3.</summary>
    internal static readonly RandomSampleField DroolFacingSpread = new(0, 4);

    /// <summary>
    /// Golden Torizo's eye-beam angle spread, 0 through 15 table angles: the selection of bits 1-4, which the
    /// cartridge reads as an even word-index step of up to thirty.
    /// </summary>
    internal static readonly RandomSampleField EyeBeamSpread = new(1, 4);

    /// <summary>Golden Torizo's dying drop scatter: the low byte across its 256-pixel strip.</summary>
    internal const int GoldenDropXMask = 0x00ff;

    /// <summary>Bomb Torizo's dying drop scatter: seven low bits across its 128-pixel region.</summary>
    internal const int BombDropXMask = 0x007f;

    /// <summary>Both statues' dying drop scatter: six bits of the sample's upper random byte down a 64-pixel band.</summary>
    internal const int DropYMask = 0x003f;
}
