using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidHealthEighths(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidHealthFraction), () => VerifyKraidHealthFraction(rom, 0xa7aa05, 3, 8, (state, index) => state.HealthEighthThreshold(index)));

    private static void VerifyKraidHealthQuarters(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidHealthFraction), () => VerifyKraidHealthFraction(rom, 0xa7aa26, 2, 4, (state, index) => state.HealthQuarterThreshold(index)));

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
