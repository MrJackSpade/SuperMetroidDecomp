namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The value of the game's shared random sequence: one sixteen-bit value every random consumer draws from. Advancing it
/// and exchanging its two bytes are its only operations; a sample is the state read as an int, and the game often draws
/// two independent random bytes from one sample, one per axis or choice. Which value each consumer draws is
/// gameplay-observable, so the advance is the original's exactly.
/// </summary>
/// <remarks>
/// Held by the session's <see cref="SharedRandom"/>, which advances it whenever a consumer draws. Evidence: the original's
/// random routine (<c>$80:8111</c>) and the SMV replays, which compare the random value on every update (the Ridley
/// full-movie check across door transitions).
/// </remarks>
[EncodingOwner("shared random state and the two random bytes of a sample")]
[CompatibilityService("the shared random sequence's value: its advance and its byte exchange, exactly as the original computed them")]
public readonly record struct SharedRandomState
{
    /// <summary>The bits of the lower random byte, below the upper one.</summary>
    private const int ByteBits = 8;

    /// <summary>The values one random byte holds: a byte's product moved to the upper byte is multiplied by this.</summary>
    private const int ByteSpan = 256;

    /// <summary>The values the state holds: an advance wraps at this.</summary>
    private const int StateSpan = 65536;

    /// <summary>Each byte of the state is multiplied by five.</summary>
    private const int Multiplier = 5;

    /// <summary>The 17 an advance adds after the products and their 256.</summary>
    private const int Increment = 17;

    /// <summary>Holds a state from 0 through 65535.</summary>
    public SharedRandomState(int value) => Value = RequireSample(value);

    /// <summary>The state as a sample, 0 through 65535.</summary>
    public int Value { get; }

    /// <summary>The random byte a sample, 0 through 65535, holds in its low eight bits.</summary>
    public static byte LowerByte(int sample) => unchecked((byte)RequireSample(sample));

    /// <summary>
    /// The random byte a sample, 0 through 65535, holds in its high eight bits: a second draw independent of
    /// <see cref="LowerByte"/>, which the game uses for another axis or another choice.
    /// </summary>
    public static byte UpperByte(int sample) => unchecked((byte)(RequireSample(sample) >> ByteBits));

    /// <summary>
    /// The state after one advance (<c>$80:8111</c>): roughly the state times five plus 273. Each byte of the state is
    /// multiplied by five on its own, and the upper byte's product keeps only its own low byte; the products plus 256
    /// form the next state, plus 17, plus one more when the products plus 256 pass the end of the state's sixteen bits.
    /// That extra one is why the sequence is not exactly the state times five plus 273.
    /// </summary>
    public SharedRandomState Next()
    {
        int products = LowerByte(Value) * Multiplier + LowerByte(UpperByte(Value) * Multiplier) * ByteSpan;
        int raised = products + ByteSpan;
        int pastSixteenBits = raised > ushort.MaxValue ? 1 : 0;
        return new SharedRandomState((raised + Increment + pastSixteenBits) % StateSpan);
    }

    /// <summary>The state with its two bytes exchanged, as the lava and acid FX routine stores it back.</summary>
    public SharedRandomState WithBytesSwapped() =>
        new(LowerByte(Value) << ByteBits | UpperByte(Value));

    /// <summary>A sample checked to lie in 0 through 65535.</summary>
    private static int RequireSample(int sample) => Ensure.BetweenInclusive(0, ushort.MaxValue, sample);
}
