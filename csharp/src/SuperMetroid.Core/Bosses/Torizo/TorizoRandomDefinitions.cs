using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The fields of the room's random numbers Torizo's AI and spawns read. A decision that needs several single-bit fields
/// is a <see cref="RandomClearBitsChance"/>, taken when every one of them reads zero, so the number of fields sets its odds.
/// </summary>
internal static class TorizoRandomDefinitions
{

    /// <summary>Golden Torizo's forward leap goes while held directions draw a sample whose two single-bit fields both read zero: one time in four.</summary>
    internal static readonly RandomClearBitsChance ForwardLeap = RandomClearBitsChance.WhenAllClear(
        RandomSampleField.OneOf(2), RandomSampleField.SecondOf(2));

    /// <summary>
    /// The initial drool's fall speed: its slowest (<see cref="TorizoMotionDefinitions.InitialDroolSlowestFall"/>), raised
    /// by up to thirty-one of the finest steps a speed takes, a 256th of a pixel an update each.
    /// </summary>
    internal static readonly RandomSpeed InitialDroolFall =
        new(RandomSampleField.OneOf(32), TorizoMotionDefinitions.InitialDroolSlowestFall, SignedEightEight.FromPixelFraction(1, 256));

    /// <summary>A facing Torizo's drool angle spread about its facing angle, 0 through 15.</summary>
    internal static readonly RandomSampleField DroolFacingSpread = RandomSampleField.OneOf(16);

    /// <summary>The initial drool's height jitter below Torizo's Y, 0 through 3 pixels.</summary>
    internal static readonly RandomSampleField InitialDroolYJitter = RandomSampleField.OneOf(4);

    /// <summary>The initial drool's side jitter to the right of Torizo's X, 0 through 3 pixels.</summary>
    internal static readonly RandomSampleField InitialDroolXJitter = RandomSampleField.OneOf(4);

    /// <summary>
    /// The drool a low-health drool starts, chosen from eight rolls of its draw (#2510): the drools released at once, formed
    /// for two updates and formed for four in turn, so the first two kinds are each drawn three times in eight and the last
    /// twice.
    /// </summary>
    internal static readonly WeightedRandomChoice<Animation<TorizoProjectileSegment>> LowHealthDroolForm =
        WeightedRandomChoice<Animation<TorizoProjectileSegment>>.InTurn(TorizoRandomRolls.LowHealthDroolForm,
            TorizoProjectileAnimations.StartingWith(TorizoProjectileSegment.DroolNoDelay), TorizoProjectileAnimations.DroolFormingShort,
            TorizoProjectileAnimations.DroolFormingLong);

    /// <summary>
    /// A sonic boom flying high, twelve pixels above Torizo's Y rather than twenty below (#2410): one time in two.
    /// </summary>
    internal static readonly RandomChance SonicBoomFliesHigh = RandomChance.Highest(1, RandomSampleField.OneOf(2));

    /// <summary>
    /// A randomized projectile's jitter on one axis (#2232): from half a pixel an update back up to one 256th short of half a
    /// pixel on, in 256 of the finest steps a speed takes, so the jitter lies either side of its base speed.
    /// </summary>
    internal static readonly RandomSpeed VelocityJitter =
        new(RandomSampleField.OneOf(256), SignedEightEight.FromPixelFraction(-1, 2), SignedEightEight.FromPixelFraction(1, 256));
}
