namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The random choices of Kraid's fight and of Fake Kraid: delays, movement, nails and the rising rocks.</summary>
internal static class KraidRandomDefinitions
{
    /// <summary>Fake Kraid's starting delay, 0 through 3 updates added to two: the sample's bits 0-1.</summary>
    internal static readonly RandomSampleField FakeKraidStartDelay = new(0, 2);

    /// <summary>
    /// Kraid's thinking delay in steps of 64 updates, 0 through 7 with a zero taken as two: the sample's bits 0-2.
    /// </summary>
    internal static readonly RandomSampleField ThinkingDelaySteps = new(0, 3);

    /// <summary>
    /// Kraid's second-phase movement choice, 0 through 7 with every choice past four taken as four: the sample's bits 2-4.
    /// </summary>
    internal static readonly RandomSampleField MovementChoice = new(2, 3);

    /// <summary>
    /// Whether a launching nail waits for the lint at its parked position instead of launching beside Kraid's body: the
    /// sample's bit zero, set to wait, unless its sibling took the alternate spawn last.
    /// </summary>
    internal static readonly RandomSampleField NailWaitsForLint = new(0, 1);

    /// <summary>Whether a rising rock flies to the right: the sample's bit four.</summary>
    internal static readonly RandomSampleField RisingRockRight = new(4, 1);

    /// <summary>A rising rock's offset from Kraid's body X, 0 through 63 pixels: the sample's bits 0-5.</summary>
    internal static readonly RandomSampleField RisingRockOffset = new(0, 6);

    /// <summary>
    /// Whether a rising rock keeps its offset to the right of Kraid's body: the sample's bit zero; when clear the offset
    /// is complemented to the left.
    /// </summary>
    internal static readonly RandomSampleField RisingRockOffsetRight = new(0, 1);

    /// <summary>
    /// A rising rock's horizontal speed in steps of <see cref="RisingRockXSpeedStep"/>, 0 through 63: the sample's
    /// bits 4-9, which the original keeps in place.
    /// </summary>
    internal static readonly RandomSampleField RisingRockXSpeedSteps = new(4, 6);

    /// <summary>One <see cref="RisingRockXSpeedSteps"/> step: sixteen 256ths of a pixel an update.</summary>
    internal const int RisingRockXSpeedStep = 16;
}
