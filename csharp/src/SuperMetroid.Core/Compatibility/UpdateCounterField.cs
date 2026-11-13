namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A phase an update counter selects (#1606): the bits of the counter that one alternation or cycle reads, by their
/// position and width. A field of width one at position <c>n</c> alternates every <c>2^n</c> updates (set for
/// <c>2^n</c> updates, then clear for as many); a wider field steps through its values, holding each for
/// <c>2^Shift</c> updates. The game read these phases by masking and shifting its counters; a catalog names the
/// field a phase reads instead of the mask, and this type is the one place that reads a field from a counter.
/// </summary>
/// <remarks>
/// Kept apart from <see cref="RandomSampleField"/>: a counter field is a schedule the counter steps through, not a
/// random choice, and naming it for the random sample would misname every counter it reads. A counter read here may be
/// signed; its field is read in two's complement, exactly as the game's bits read it. Read at every alternating or
/// cycling choice, through the named field its family's catalog gives for what the phase shows or decides. Evidence:
/// each phase's original masking and shifting of its counter, which a field reproduces bit for bit; the SMV replays
/// compare the counters and what their phases show and decide on every update.
/// </remarks>
/// <param name="Shift">The position of the field's lowest bit in the counter.</param>
/// <param name="Width">How many bits the field holds, 1 through 15.</param>
[EncodingOwner("update counter phase fields")]
[CompatibilityService("which phase of an alternation or cycle an update counter selects, exactly as the original masked and shifted its counters")]
public readonly record struct UpdateCounterField(int Shift, int Width)
{
    /// <summary>The field's value in <paramref name="counter"/>, 0 through <see cref="Outcomes"/> minus one.</summary>
    /// <param name="counter">The counter that paces the phase, as the game holds it.</param>
    public int Of(int counter) => (counter >> Shift) & (Outcomes - 1);

    /// <summary>How many values the field can take.</summary>
    public int Outcomes => 1 << Width;

    /// <summary>Whether the field, a single bit, is set in <paramref name="counter"/>.</summary>
    /// <param name="counter">The counter that paces the phase, as the game holds it.</param>
    /// <exception cref="InvalidOperationException">The field holds more than one bit.</exception>
    public bool IsSetIn(int counter) => Width == 1
        ? Of(counter) != 0
        : throw new InvalidOperationException($"A {Width}-bit counter field is a cycle, not an alternation.");
}
