namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The random choices of Kraid's fight and of Fake Kraid: delays, movement, nails and the rising rocks.</summary>
internal static class KraidRandomDefinitions
{
    /// <summary>
    /// Fake Kraid's random starting delay, 0 through 3 updates added to <see cref="FakeKraidStartDelayBase"/>: the
    /// sample modulo 4.
    /// </summary>
    internal static readonly RandomSampleField FakeKraidStartDelay = new(0, 2);

    /// <summary>The fewest updates Fake Kraid waits before its first walk turn and first spit decision: two.</summary>
    internal const int FakeKraidStartDelayBase = 2;

    /// <summary>
    /// The random part of a Fake Kraid spike row's reload, 0 through 63 updates added to
    /// <see cref="FakeKraidSpikeReloadBase"/>: the sample modulo 64.
    /// </summary>
    internal static readonly RandomSampleField FakeKraidSpikeReloadSpread = new(0, 6);

    /// <summary>The fewest updates a Fake Kraid spike row waits after it fires before it can fire again: sixteen.</summary>
    internal const int FakeKraidSpikeReloadBase = 16;

    /// <summary>
    /// The random part of how many walk steps Fake Kraid takes before it turns round, 0 through 3 added to
    /// <see cref="FakeKraidWalkStepBase"/>: the sample modulo 4.
    /// </summary>
    internal static readonly RandomSampleField FakeKraidWalkStepSpread = new(0, 2);

    /// <summary>The fewest walk steps Fake Kraid takes one way before it turns round: seven.</summary>
    internal const int FakeKraidWalkStepBase = 7;

    /// <summary>
    /// The random part of how many walk steps Fake Kraid takes between spits, 0 through 3 added to
    /// <see cref="FakeKraidSpitDecisionBase"/>: the sample modulo 4.
    /// </summary>
    internal static readonly RandomSampleField FakeKraidSpitDecisionSpread = new(0, 2);

    /// <summary>The fewest walk steps Fake Kraid takes between one spit and the next: three.</summary>
    internal const int FakeKraidSpitDecisionBase = 3;

    /// <summary>
    /// The random part of a falling ceiling rock's speed in Kraid's room, 0 through 63 256ths of a pixel an update added
    /// to <see cref="CeilingRockFallSpeedBase"/>: the sample modulo 64.
    /// </summary>
    internal static readonly RandomSampleField CeilingRockFallSpeedSpread = new(0, 6);

    /// <summary>The slowest a ceiling rock falls in Kraid's room: 64 256ths of a pixel an update, a quarter pixel.</summary>
    internal const int CeilingRockFallSpeedBase = 64;

    /// <summary>
    /// Kraid's thinking delay in steps of 64 updates, 0 through 7 with a zero taken as two: the sample modulo 8.
    /// </summary>
    internal static readonly RandomSampleField ThinkingDelaySteps = new(0, 3);

    /// <summary>
    /// Kraid's second-phase movement choice, 0 through 7 with every choice past four taken as four: the sample divided by 4, modulo 8.
    /// </summary>
    internal static readonly RandomSampleField MovementChoice = new(2, 3);

    /// <summary>
    /// Whether a launching nail waits for the lint at its parked position instead of launching beside Kraid's body: an
    /// odd sample waits, unless its sibling took the alternate spawn last.
    /// </summary>
    internal static readonly RandomSampleField NailWaitsForLint = new(0, 1);

    /// <summary>Whether a rising rock flies to the right: taken when the sample divided by 16 is odd.</summary>
    internal static readonly RandomSampleField RisingRockRight = new(4, 1);

    /// <summary>A rising rock's offset from Kraid's body X, 0 through 63 pixels: the sample modulo 64.</summary>
    internal static readonly RandomSampleField RisingRockOffset = new(0, 6);

    /// <summary>
    /// Whether a rising rock keeps its offset to the right of Kraid's body: taken by an odd sample; otherwise the rock
    /// rises to the left, <see cref="RisingRockLeftExtraPixels"/> further from the body than its offset.
    /// </summary>
    internal static readonly RandomSampleField RisingRockOffsetRight = new(0, 1);

    /// <summary>
    /// How much further from Kraid's body a rising rock on the left starts than one on the right with the same offset:
    /// one pixel, so a rock on the left starts 1 through 64 pixels from the body's X.
    /// </summary>
    internal const int RisingRockLeftExtraPixels = 1;

    /// <summary>
    /// A rising rock's horizontal speed in steps of <see cref="RisingRockXSpeedStep"/>, 0 through 63: the sample
    /// divided by 16, modulo 64, which the game reads without shifting it down.
    /// </summary>
    internal static readonly RandomSampleField RisingRockXSpeedSteps = new(4, 6);

    /// <summary>One <see cref="RisingRockXSpeedSteps"/> step: sixteen 256ths of a pixel an update.</summary>
    internal const int RisingRockXSpeedStep = 16;
}
