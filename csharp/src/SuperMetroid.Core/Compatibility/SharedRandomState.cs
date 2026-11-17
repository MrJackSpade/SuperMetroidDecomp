namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The value of the game's shared random sequence: one value, 0 through 65535, every random consumer draws from.
/// Advancing it and swapping its two halves, each 0 through 255, are its only operations; a sample is the state read as
/// an int, and the game often draws two independent random halves from one sample, one per axis or choice. Which value
/// each consumer draws is gameplay-observable, so the advance is the game's exactly.
/// </summary>
/// <remarks>
/// Held by the session's <see cref="SharedRandom"/>, which advances it whenever a consumer draws. Evidence: the original's
/// random routine (<c>$80:8111</c>) and the SMV replays, which compare the random value on every update (the Ridley
/// full-movie check across door transitions).
/// </remarks>
[EncodingOwner("shared random state and the two random halves of a sample")]
[CompatibilityService("the shared random sequence's value: how each value follows the last, and the swap of its two halves, exactly as the game made them")]
public readonly record struct SharedRandomState
{
    /// <summary>The binary places of the lower random half, below the upper one.</summary>
    private const int HalfBits = 8;

    /// <summary>The values one random half holds: a half's product moved to the upper half is multiplied by this.</summary>
    private const int HalfSpan = 256;

    /// <summary>The values the state holds: an advance wraps at this.</summary>
    private const int StateSpan = 65536;

    /// <summary>
    /// The largest value the state holds, and so the largest sample a consumer copies from it: an advance wraps past it
    /// (#1627).
    /// </summary>
    public const int LargestValue = StateSpan - 1;

    /// <summary>Each half of the state is multiplied by five.</summary>
    private const int Multiplier = 5;

    /// <summary>The 17 an advance adds after the products and their 256.</summary>
    private const int Increment = 17;

    /// <summary>Holds a state from 0 through 65535.</summary>
    public SharedRandomState(int value) => Value = RequireSample(value);

    /// <summary>The state as a sample, 0 through 65535.</summary>
    public int Value { get; }

    /// <summary>The lower random half, 0 through 255, of a sample, 0 through 65535: its remainder past a multiple of 256.</summary>
    public static byte LowerHalf(int sample) => unchecked((byte)RequireSample(sample));

    /// <summary>
    /// The upper random half, 0 through 255, of a sample, 0 through 65535: how many whole 256s it holds; a second draw
    /// independent of
    /// <see cref="LowerHalf"/>, which the game uses for another axis or another choice.
    /// </summary>
    public static byte UpperHalf(int sample) => unchecked((byte)(RequireSample(sample) >> HalfBits));

    /// <summary>
    /// The state after one advance: roughly the state times five plus 273. Each half of the state is multiplied by five on
    /// its own, and the upper half's product keeps only its part below 256; the products plus 256 form the next state,
    /// plus 17, plus one more when the products plus 256 reach 65536. That extra one is why the sequence is not exactly
    /// the state times five plus 273.
    /// </summary>
    public SharedRandomState Next()
    {
        int products = LowerHalf(Value) * Multiplier + LowerHalf(UpperHalf(Value) * Multiplier) * HalfSpan;
        int raised = products + HalfSpan;
        int pastSixteenBits = raised > LargestValue ? 1 : 0;
        return new SharedRandomState((raised + Increment + pastSixteenBits) % StateSpan);
    }

    /// <summary>The state with its two halves swapped, as the lava and acid effect stores it back.</summary>
    public SharedRandomState WithHalvesSwapped() =>
        new(LowerHalf(Value) << HalfBits | UpperHalf(Value));

    /// <summary>A sample checked to lie in 0 through 65535.</summary>
    private static int RequireSample(int sample) => Ensure.BetweenInclusive(0, LargestValue, sample);
}
