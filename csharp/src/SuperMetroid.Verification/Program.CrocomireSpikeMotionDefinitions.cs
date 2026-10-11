using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire spike acceleration delta")]
    private static void VerifyCrocomireSpikeAccelerationDelta(SuperMetroidAddressSpace rom)
    {
        for (int slot = 0; slot < 18; slot++)
        {
            int address = 0x8691c3 + 2 * slot;
            ushort expected = (ushort)(rom.ReadCartridgeByte(address) | rom.ReadCartridgeByte(address + 1) << 8);
            AssertEqual(expected, CrocomireSpikeMotionDefinitions.AccelerationDelta(slot),
                $"Crocomire spike acceleration increment, physical slot {slot}");
        }
        AssertThrows<IndexOutOfRangeException>(() => CrocomireSpikeMotionDefinitions.AccelerationDelta(-1), "negative increment slot");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireSpikeMotionDefinitions.AccelerationDelta(18), "increment slot past end");
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire spike maximum acceleration")]
    private static void VerifyCrocomireSpikeMaximumAcceleration(SuperMetroidAddressSpace rom)
    {
        for (int slot = 0; slot < 18; slot++)
        {
            int address = 0x8691e7 + 2 * slot;
            ushort expected = (ushort)(rom.ReadCartridgeByte(address) | rom.ReadCartridgeByte(address + 1) << 8);
            AssertEqual(expected, CrocomireSpikeMotionDefinitions.MaximumAcceleration(slot),
                $"Crocomire spike acceleration limit, physical slot {slot}");
        }
        AssertThrows<IndexOutOfRangeException>(() => CrocomireSpikeMotionDefinitions.MaximumAcceleration(-1), "negative acceleration limit slot");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireSpikeMotionDefinitions.MaximumAcceleration(18), "acceleration limit slot past end");
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire spike maximum velocity")]
    private static void VerifyCrocomireSpikeMaximumVelocity(SuperMetroidAddressSpace rom)
    {
        for (int slot = 0; slot < 18; slot++)
            AssertEqual(rom.ReadCartridgeByte(0x86920b + 2 * slot), CrocomireSpikeMotionDefinitions.MaximumVelocity(slot),
                $"Crocomire spike velocity limit low byte, physical slot {slot}");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireSpikeMotionDefinitions.MaximumVelocity(-1), "negative velocity limit slot");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireSpikeMotionDefinitions.MaximumVelocity(18), "velocity limit slot past end");
    }
}
