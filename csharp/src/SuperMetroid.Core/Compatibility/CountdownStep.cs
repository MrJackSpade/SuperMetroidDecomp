namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The outcome of taking one frame away from a <see cref="WrappingCountdown"/>: the countdown left, and the expiry tests
/// its owner chooses between. The tests describe the countdown after the step, so a countdown stepped from zero has
/// wrapped to 65535 and reads as negative, not as zero.
/// <para>
/// <b>Frame point.</b> Made by the owner's step, once per call of the owner's update; the owner stores
/// <see cref="Remaining"/> and tests the step in the same call.
/// </para>
/// <para>
/// <b>Evidence.</b> The original game tests each timer, right after decrementing it, by zero or by sign; the full-movie
/// replays (the Ridley full movie and the 13% SMV) match the recording on every update with each owner's test taken
/// from here.
/// </para>
/// </summary>
/// <param name="Remaining">The countdown left after the step; a countdown stepped from zero has wrapped to 65535.</param>
[CompatibilityService("the outcome of one countdown step, tested the way each timer's expiry was originally tested (reached zero, went negative, or either)")]
[EncodingOwner("wrapping countdown decrement result")]
public readonly record struct CountdownStep(WrappingCountdown Remaining)
{
    /// <summary>Whether the step left no frames: the expiry of a timer that ends on reaching zero.</summary>
    public bool IsZero => Remaining.IsZero;

    /// <summary>Whether the step ran the countdown past zero: the expiry of a timer that ends on running past zero.</summary>
    public bool IsNegative => Remaining.IsNegative;

    /// <summary>Whether the countdown has not yet run past zero: a timer that ends on running past zero is still waiting.</summary>
    public bool IsNonNegative => !IsNegative;

    /// <summary>Whether the step left no frames or ran past zero: the expiry of a timer that ends on either.</summary>
    public bool IsZeroOrNegative => Remaining.IsZeroOrNegative;
}
