using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Crocomire;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>Compares compiled spike motion limits for all eighteen physical slots with bank-$86 retail data.</summary>
internal static class CrocomireSpikeMotionDefinitionsVerification
{
    /// <summary>Checks each slot's acceleration increment and rejects indexes outside the native table.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire spike acceleration delta")]
    internal static void VerifyCrocomireSpikeAccelerationDelta(SuperMetroidAddressSpace rom)
    {
        for (int slot = 0; slot < 18; slot++)
        {
            int address = 0x8691c3 + 2 * slot;
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(expected, CrocomireSpikeMotionDefinitions.AccelerationDelta(slot),
                $"Crocomire spike acceleration increment, physical slot {slot}");
        }
        AssertThrows<IndexOutOfRangeException>(() => CrocomireSpikeMotionDefinitions.AccelerationDelta(-1), "negative increment slot");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireSpikeMotionDefinitions.AccelerationDelta(18), "increment slot past end");
    }

    /// <summary>Checks each slot's acceleration cap and native table bounds.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire spike maximum acceleration")]
    internal static void VerifyCrocomireSpikeMaximumAcceleration(SuperMetroidAddressSpace rom)
    {
        for (int slot = 0; slot < 18; slot++)
        {
            int address = 0x8691e7 + 2 * slot;
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(expected, CrocomireSpikeMotionDefinitions.MaximumAcceleration(slot),
                $"Crocomire spike acceleration limit, physical slot {slot}");
        }
        AssertThrows<IndexOutOfRangeException>(() => CrocomireSpikeMotionDefinitions.MaximumAcceleration(-1), "negative acceleration limit slot");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireSpikeMotionDefinitions.MaximumAcceleration(18), "acceleration limit slot past end");
    }

    /// <summary>Checks the low byte of each slot's velocity cap and native table bounds.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire spike maximum velocity")]
    internal static void VerifyCrocomireSpikeMaximumVelocity(SuperMetroidAddressSpace rom)
    {
        for (int slot = 0; slot < 18; slot++)
            AssertEqual(rom.ReadCartridgeByte(0x86920b + 2 * slot), CrocomireSpikeMotionDefinitions.MaximumVelocity(slot),
                $"Crocomire spike velocity limit low byte, physical slot {slot}");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireSpikeMotionDefinitions.MaximumVelocity(-1), "negative velocity limit slot");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireSpikeMotionDefinitions.MaximumVelocity(18), "velocity limit slot past end");
    }
}
