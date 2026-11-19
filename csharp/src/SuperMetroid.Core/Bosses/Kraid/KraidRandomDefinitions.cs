namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The random choices of Kraid's fight and of Fake Kraid: delays, movement, nails and the rising rocks.</summary>
internal static class KraidRandomDefinitions
{
    /// <summary>
    /// Fake Kraid's random starting delay, 0 through 3 updates added to <see cref="FakeKraidStartDelayBase"/>.
    /// </summary>
    internal static readonly RandomSampleField FakeKraidStartDelay = new(0, 2);

    /// <summary>The fewest updates Fake Kraid waits before its first walk turn and first spit decision: two.</summary>
    internal const int FakeKraidStartDelayBase = 2;

    /// <summary>
    /// The random part of a Fake Kraid spike row's reload, 0 through 63 updates added to
    /// <see cref="FakeKraidSpikeReloadBase"/>.
    /// </summary>
    internal static readonly RandomSampleField FakeKraidSpikeReloadSpread = new(0, 6);

    /// <summary>The fewest updates a Fake Kraid spike row waits after it fires before it can fire again: sixteen.</summary>
    internal const int FakeKraidSpikeReloadBase = 16;

    /// <summary>
    /// The random part of how many walk steps Fake Kraid takes before it turns round, 0 through 3 added to
    /// <see cref="FakeKraidWalkStepBase"/>.
    /// </summary>
    internal static readonly RandomSampleField FakeKraidWalkStepSpread = new(0, 2);

    /// <summary>The fewest walk steps Fake Kraid takes one way before it turns round: seven.</summary>
    internal const int FakeKraidWalkStepBase = 7;

    /// <summary>
    /// The random part of how many walk steps Fake Kraid takes between spits, 0 through 3 added to
    /// <see cref="FakeKraidSpitDecisionBase"/>.
    /// </summary>
    internal static readonly RandomSampleField FakeKraidSpitDecisionSpread = new(0, 2);

    /// <summary>The fewest walk steps Fake Kraid takes between one spit and the next: three.</summary>
    internal const int FakeKraidSpitDecisionBase = 3;

    /// <summary>
    /// The random part of a falling ceiling rock's speed in Kraid's room added to <see cref="CeilingRockSlowestFall"/>: 0
    /// through 63, in 256ths of a pixel an update.
    /// </summary>
    internal static readonly RandomSampleField CeilingRockFallSpeedSpread = new(0, 6);

    /// <summary>The slowest a ceiling rock falls in Kraid's room: a quarter pixel an update.</summary>
    internal static readonly SignedEightEight CeilingRockSlowestFall = SignedEightEight.FromParts(0, 64);

    /// <summary>
    /// Kraid's thinking delay in steps of <see cref="KraidArenaDefinitions.ThinkingDelayStepUpdates"/>, chosen from eight rolls (#2510): a roll of one through seven takes that
    /// many steps and a roll of none <see cref="KraidArenaDefinitions.ThinkingDelayStepsOnNoRoll"/>.
    /// </summary>
    internal static readonly WeightedRandomChoice<int> ThinkingDelaySteps = WeightedRandomChoice<int>.InTurn(
        new RandomSampleField(0, 3), KraidArenaDefinitions.ThinkingDelayStepsOnNoRoll, 1, 2, 3, 4, 5, 6, 7);

    /// <summary>
    /// The part of the sample Kraid's second-phase walk is chosen by, eight values his destination's walks share
    /// (<see cref="KraidMovementDefinitions.Rows"/>).
    /// </summary>
    internal static readonly RandomSampleField MovementChoice = new(2, 3);

    /// <summary>
    /// Whether a launching nail waits for the lint at its parked position instead of launching beside Kraid's body (#2479):
    /// one launch in two, unless its sibling took the alternate spawn last.
    /// </summary>
    internal static readonly RandomChance NailWaitsForLint = RandomChance.Highest(1, new RandomSampleField(0, 1));

    /// <summary>
    /// Whether a rising rock takes its pose flying to the right (#2479): an even choice, taken by half the random
    /// numbers.
    /// </summary>
    internal static readonly RandomChance RisingRockFliesRight = RandomChance.Highest(1, new RandomSampleField(4, 1));

    /// <summary>A rising rock's offset from Kraid's body X, 0 through 63 pixels.</summary>
    internal static readonly RandomSampleField RisingRockOffset = new(0, 6);

    /// <summary>
    /// Whether a rising rock keeps its offset to the right of Kraid's body (#2479): one rock in two;
    /// otherwise the rock rises to the left, <see cref="RisingRockLeftExtraPixels"/> further from the body than its offset.
    /// </summary>
    internal static readonly RandomChance RisingRockStartsOnRight = RandomChance.Highest(1, new RandomSampleField(0, 1));

    /// <summary>
    /// How much further from Kraid's body a rising rock on the left starts than one on the right with the same offset:
    /// one pixel, so a rock on the left starts 1 through 64 pixels from the body's X.
    /// </summary>
    internal const int RisingRockLeftExtraPixels = 1;

    /// <summary>
    /// A rising rock's horizontal speed in sixteenths of a pixel an update, 0 through 63.
    /// </summary>
    internal static readonly RandomSampleField RisingRockXSpeedSixteenths = new(4, 6);
}
