using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifySamusDeathExplosionTimingDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (ushort index = 0;
             index < SamusDeathExplosionTimingDefinitions.RecordCount;
             index++)
        {
            int address = SamusDeathExplosionTimingDefinitions.NativeFirstTimerAddress +
                index * SamusDeathExplosionTimingDefinitions.RecordByteCount;
            AssertEqual(
                rom.ReadByte(address),
                SamusDeathExplosionTimingDefinitions.DurationForIndex(index),
                $"Samus death-explosion duration {index}");
        }

        AssertThrows<InvalidDataException>(
            () => SamusDeathExplosionTimingDefinitions.DurationForIndex(
                SamusDeathExplosionTimingDefinitions.RecordCount),
            "Samus death-explosion timing rejects a post-sequence index");

        Console.WriteLine(
            "Samus death-explosion timing: all nine native durations match the " +
            "compiled sequence and invalid restored indexes fail loudly.");
    }

    private static CartridgeReadGuard NewSamusDeathExplosionTimingReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
            {
                int relative = address -
                    SamusDeathExplosionTimingDefinitions.NativeFirstTimerAddress;
                return relative >= 0 &&
                    relative < SamusDeathExplosionTimingDefinitions.RecordCount *
                        SamusDeathExplosionTimingDefinitions.RecordByteCount &&
                    relative % SamusDeathExplosionTimingDefinitions.RecordByteCount == 0;
            },
            address => $"Samus death sequence attempted timer read ${address:X6}.");
}
