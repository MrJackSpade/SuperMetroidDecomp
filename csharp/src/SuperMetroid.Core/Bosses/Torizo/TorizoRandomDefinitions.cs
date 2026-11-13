
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The fields of the room's random numbers Torizo's AI and spawns read. A decision that needs several single-bit fields
/// is taken when every one of them reads zero, so the number of fields sets its odds.
/// </summary>
internal static class TorizoRandomDefinitions
{
    /// <summary>Bomb Torizo drools while hurt when four single-bit fields of the sample all read zero: one time in sixteen.</summary>
    internal static readonly TorizoRandomDecision LowHealthDrool = TorizoRandomDecision.WhenAllClear(
        new RandomSampleField(1, 1), new RandomSampleField(6, 1), new RandomSampleField(8, 1), new RandomSampleField(15, 1));

    /// <summary>Golden Torizo's low-health call is taken when two single-bit fields of the sample both read zero: one time in four.</summary>
    internal static readonly TorizoRandomDecision LowHealthCall = TorizoRandomDecision.WhenAllClear(
        new RandomSampleField(1, 1), new RandomSampleField(8, 1));

    /// <summary>Golden Torizo's medium-range attack is taken when two single-bit fields of the sample both read zero: one time in four.</summary>
    internal static readonly TorizoRandomDecision MediumRangeAttack = TorizoRandomDecision.WhenAllClear(
        new RandomSampleField(4, 1), new RandomSampleField(8, 1));

    /// <summary>Golden Torizo's forward leap goes while held directions draw a sample whose two single-bit fields both read zero: one time in four.</summary>
    internal static readonly TorizoRandomDecision ForwardLeap = TorizoRandomDecision.WhenAllClear(
        new RandomSampleField(0, 1), new RandomSampleField(8, 1));

    /// <summary>The initial drool's added fall speed, 0 through 31 in raw 8.8 units: the selection modulo 32.</summary>
    internal static readonly RandomSampleField DroolFallJitter = new(0, 5);

    /// <summary>
    /// A turning Torizo's drool angle, any of the 256 table angles: the selection divided by two, modulo 256.
    /// </summary>
    internal static readonly RandomSampleField DroolTurningAngle = new(1, 8);

    /// <summary>A facing Torizo's drool angle spread about its facing angle, 0 through 15: the selection modulo 16.</summary>
    internal static readonly RandomSampleField DroolFacingSpread = new(0, 4);

    /// <summary>
    /// Golden Torizo's eye-beam angle spread, 0 through 15 table angles: the selection divided by two, modulo 16.
    /// </summary>
    internal static readonly RandomSampleField EyeBeamSpread = new(1, 4);

    /// <summary>The initial drool's height jitter below Torizo's Y, 0 through 3 pixels: the sample modulo 4.</summary>
    internal static readonly RandomSampleField InitialDroolYJitter = new(0, 2);

    /// <summary>The initial drool's side jitter to the right of Torizo's X, 0 through 3 pixels: the sample modulo 4.</summary>
    internal static readonly RandomSampleField InitialDroolXJitter = new(0, 2);

    /// <summary>
    /// The low-health drool's delay entry, 0 through 7 of the eight-entry list whose entries repeat the no-delay,
    /// two-frame and four-frame lists: the sample divided by four, modulo 8.
    /// </summary>
    internal static readonly RandomSampleField LowHealthDroolDelay = new(2, 3);

    /// <summary>
    /// The drool each <see cref="LowHealthDroolDelay"/> entry starts, in entry order: the no-delay, two-frame and
    /// four-frame drools in turn, so the first two kinds are each drawn three times in eight and the last twice.
    /// </summary>
    internal static readonly TorizoProjectileSegment[] LowHealthDroolAnimations =
    [
        TorizoProjectileSegment.DroolNoDelay, TorizoProjectileSegment.DroolTwoFrameDelay, TorizoProjectileSegment.DroolFourFrameDelay,
        TorizoProjectileSegment.DroolNoDelay, TorizoProjectileSegment.DroolTwoFrameDelay, TorizoProjectileSegment.DroolFourFrameDelay,
        TorizoProjectileSegment.DroolNoDelay, TorizoProjectileSegment.DroolTwoFrameDelay,
    ];

    /// <summary>Whether a sonic boom flies high, twelve pixels above Torizo's Y rather than twenty below: the sample's bit zero.</summary>
    internal static readonly RandomSampleField SonicBoomHigh = new(0, 1);

    /// <summary>A randomized projectile's jitter on one axis, less 128, in 256ths of a pixel: 0 through 255 from the sample.</summary>
    internal static readonly RandomSampleField VelocityJitter = new(0, 8);
}
