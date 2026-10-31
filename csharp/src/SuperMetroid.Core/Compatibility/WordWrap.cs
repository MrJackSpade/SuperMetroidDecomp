namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The wrap of the game's sixteen- and eight-bit quantities: a value computed past either end of its width continues from
/// the other end, and a value whose top bit is set reads as negative where the game tests its sign.
/// <para>
/// <b>Gameplay behaviour.</b> The original game computes speeds, accelerations, counters, phases, health products and
/// table offsets in sixteen- or eight-bit quantities with no clamp, and the result it keeps is the wrapped one: a speed
/// negated at its most negative value stays most negative, a phase stepped past its top restarts from the bottom, a
/// difference tested for its sign reads a large word as negative, and a byte taken from a word keeps only its low eight
/// bits. Values that are coordinates wrap through <c>CoordinateAxis</c> and the spatial types instead; values with a
/// richer owner (a countdown, an 8.8 velocity, a table angle, the shared random number) wrap through that owner.
/// </para>
/// <para>
/// <b>Frame point.</b> None of its own: each owner wraps a value where its update computes it.
/// </para>
/// <para>
/// <b>Evidence.</b> The original routines keep these quantities in sixteen- and eight-bit registers and memory with no
/// clamp; the full-movie replays (the Ridley full movie and the 13% SMV) compute every wrapped value here and match the
/// recording on every update.
/// </para>
/// </summary>
[CompatibilityService("sixteen- and eight-bit quantities wrap at their width, and a set top bit reads as negative")]
public static class WordWrap
{
    /// <summary>The value wrapped to a sixteen-bit quantity, 0 through 65535.</summary>
    /// <param name="value">The computed value, which may lie outside sixteen bits.</param>
    public static int Unsigned(int value) => unchecked((ushort)value);

    /// <summary>The value wrapped to a sixteen-bit quantity and read as signed, -32768 through 32767.</summary>
    /// <param name="value">The computed value, which may lie outside sixteen bits.</param>
    public static int Signed(int value) => unchecked((short)value);

    /// <summary>The value's low eight bits, 0 through 255: an eight-bit quantity computed past either end wraps.</summary>
    /// <param name="value">The computed value, which may lie outside eight bits.</param>
    public static int LowByte(int value) => unchecked((byte)value);

    /// <summary>The value's low eight bits read as signed, -128 through 127.</summary>
    /// <param name="value">The computed value, which may lie outside eight bits.</param>
    public static int SignedLowByte(int value) => unchecked((sbyte)value);

    /// <summary>Whether a sixteen-bit quantity reads as negative: its top bit is set, whatever higher bits it carries.</summary>
    /// <param name="value">The computed value; only its low sixteen bits are read.</param>
    public static bool IsNegative(int value) => Signed(value) < 0;
}
