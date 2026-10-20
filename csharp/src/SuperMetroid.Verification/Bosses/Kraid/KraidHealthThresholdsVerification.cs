using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Compares Kraid health thresholds with the cartridge's shift-and-accumulate setup.</summary>
internal static class KraidHealthThresholdsVerification
{
    /// <summary>Checks eight thresholds generated from one-eighth initial-health increments.</summary>
    internal static void VerifyKraidHealthEighths(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidHealthFraction), () => VerifyKraidHealthFraction(rom, 0xa7aa05, 3, 8, (state, index) => state.HealthEighthThreshold(index)));

    /// <summary>Checks four thresholds generated from one-quarter initial-health increments.</summary>
    internal static void VerifyKraidHealthQuarters(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidHealthFraction), () => VerifyKraidHealthFraction(rom, 0xa7aa26, 2, 4, (state, index) => state.HealthQuarterThreshold(index)));

    /// <summary>Runs the native right-shift and wrapped addition recurrence over every 16-bit starting health.</summary>
    private static void VerifyKraidHealthFraction(SuperMetroidAddressSpace rom, int shiftAddress,
        int shifts, int count, Func<KraidEnemyState, int, int> actual)
    {
        for (int instruction = 0; instruction < shifts; instruction++)
            AssertEqual((byte)0x4a, KraidHealthFractionRomByte(rom, shiftAddress + instruction), "Native initializer uses accumulator LSR");
        var state = new KraidEnemyState();
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            state.InitialHealth = (ushort)raw;
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
            AssertThrows<ArgumentOutOfRangeException>(() => actual(state, invalid), "Threshold index preserves array bounds");
    }

    /// <summary>Reads one retail instruction byte to confirm the initializer uses accumulator LSR.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid health fraction")]
    private static byte KraidHealthFractionRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
