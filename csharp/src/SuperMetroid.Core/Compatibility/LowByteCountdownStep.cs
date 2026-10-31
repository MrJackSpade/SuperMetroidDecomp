namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The outcome of taking one away from the eight-bit count an owner keeps in the lower half of a
/// <see cref="WrappingCountdown"/> (<see cref="WrappingCountdown.EightBitCount"/>). The count wraps from 0 to 255 on its
/// own and leaves the upper half of the countdown as it was; its tests describe the eight-bit count alone, so a count
/// at 128 or more reads as negative whatever the upper half holds.
/// <para>
/// <b>Frame point.</b> Made by the owner's step: once per slow cinematic fade dispatch for the screen fade's delay, and
/// once per afterburn spawned for the chain count of Ridley's fireball.
/// </para>
/// <para>
/// <b>Evidence.</b> The original game counts these down as single bytes beside unrelated upper halves (the slow screen
/// fade, Ridley's afterburn chain); the Ceres and intro cinematic fades and the Ridley full movie match the recording
/// with the upper halves kept.
/// </para>
/// </summary>
/// <param name="Remaining">The countdown after the step: its eight-bit count one lower (wrapping 0 to 255), the rest unchanged.</param>
[CompatibilityService("a countdown kept in the low half of a value whose high half is independent state, wrapping without disturbing it")]
[EncodingOwner("low-byte countdown decrement result")]
public readonly record struct LowByteCountdownStep(WrappingCountdown Remaining)
{
    /// <summary>The eight-bit count from which it reads as negative: it has run past zero.</summary>
    private const int NegativeFrom = 1 << 7;

    /// <summary>Whether the eight-bit count reached zero.</summary>
    public bool IsZero => Remaining.EightBitCount == 0;

    /// <summary>Whether the eight-bit count ran past zero (128 or more).</summary>
    public bool IsNegative => Remaining.EightBitCount >= NegativeFrom;

    /// <summary>Whether the eight-bit count reached zero or ran past it: the expiry of a count that ends on either.</summary>
    public bool IsZeroOrNegative => IsZero || IsNegative;
}
