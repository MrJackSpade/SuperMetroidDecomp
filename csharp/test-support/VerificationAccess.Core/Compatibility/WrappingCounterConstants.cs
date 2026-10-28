namespace SuperMetroid.Core.Compatibility;

/// <summary>Cartridge values and helpers of <see cref="WrappingCounter"/> that only verification reads.</summary>
internal static class WrappingCounterConstants
{
    /// <summary>
    /// Executes one 8-bit <c>DEC</c> of a byte held as an int, 0 through 255, wrapping <c>$00</c> to <c>$FF</c>.
    /// </summary>
    public static int DecrementByte(int value) => (Ensure.BetweenInclusive(0, byte.MaxValue, value) - 1) & byte.MaxValue;
}
