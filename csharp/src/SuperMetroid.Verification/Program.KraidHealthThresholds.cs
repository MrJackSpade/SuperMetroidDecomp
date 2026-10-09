using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Verifies Kraid's eight health thresholds against the native initializer and its
    /// repeated right-shift-and-accumulate calculation.
    /// </summary>
    /// <param name="rom">Address space used to inspect the native initializer instructions.</param>
    private static void VerifyKraidHealthEighths(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidHealthFraction), () => VerifyKraidHealthFraction(rom, 0xa7aa05, 3, 8, (state, index) => state.HealthEighthThreshold(index)));

    /// <summary>
    /// Verifies Kraid's four health thresholds against the native initializer and its
    /// repeated right-shift-and-accumulate calculation.
    /// </summary>
    /// <param name="rom">Address space used to inspect the native initializer instructions.</param>
    private static void VerifyKraidHealthQuarters(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidHealthFraction), () => VerifyKraidHealthFraction(rom, 0xa7aa26, 2, 4, (state, index) => state.HealthQuarterThreshold(index)));

    /// <summary>
    /// Checks the native shift instructions and compares each generated health fraction against
    /// the shift-and-accumulate recurrence for every possible initial 16-bit health value; also
    /// verifies that invalid threshold indices preserve the array bounds failure.
    /// </summary>
    /// <param name="rom">Address space containing the native initializer instructions.</param>
    /// <param name="shiftAddress">Address of the first native accumulator right-shift instruction.</param>
    /// <param name="shifts">Number of right shifts applied before accumulating the fraction.</param>
    /// <param name="count">Number of cumulative thresholds produced by the recurrence.</param>
    /// <param name="actual">Accessor that returns the implementation's threshold at a given index.</param>
    private static void VerifyKraidHealthFraction(SuperMetroidAddressSpace rom, int shiftAddress,
        int shifts, int count, Func<KraidEnemyState, int, ushort> actual)
    {
        for (int instruction = 0; instruction < shifts; instruction++)
            AssertEqual((byte)0x4a, rom.ReadByte(shiftAddress + instruction), "Native initializer uses accumulator LSR");
        var state = new KraidEnemyState();
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            state.InitialHealth = (ushort)raw;
            // Execute the native shift/add recurrence, independently of the
            // replacement's indexed multiplication. Truncate before accumulation.
            ushort increment = (ushort)raw;
            for (int instruction = 0; instruction < shifts; instruction++) increment >>= 1;
            ushort accumulated = 0;
            for (int index = 0; index < count; index++)
            {
                accumulated = unchecked((ushort)(accumulated + increment));
                AssertEqual(accumulated, actual(state, index), "Native generated threshold recurrence");
            }
        }
        foreach (int invalid in new[] { int.MinValue, -1, count, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => actual(state, invalid), "Threshold index preserves array bounds");
    }
}
