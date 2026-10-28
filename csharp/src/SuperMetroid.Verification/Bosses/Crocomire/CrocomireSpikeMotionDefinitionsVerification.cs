using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>Compares compiled spike motion limits for all eighteen physical slots with bank-$86 retail data.</summary>
internal static class CrocomireSpikeMotionDefinitionsVerification
{
    /// <summary>Checks each slot's acceleration increment and rejects indexes outside the native table.</summary>
    internal static void VerifyCrocomireSpikeAccelerationDelta(SuperMetroidAddressSpace rom)
    {
        for (int slot = 0; slot < 18; slot++)
        {
            int address = 0x8691c3 + 2 * slot;
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(expected, CrocomireSpikeMotion.AccelerationDelta(slot),
                $"Crocomire spike acceleration increment, physical slot {slot}");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireSpikeMotion.AccelerationDelta(-1), "negative increment slot");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireSpikeMotion.AccelerationDelta(18), "increment slot past end");
    }

    /// <summary>Checks each slot's acceleration cap and native table bounds.</summary>
    internal static void VerifyCrocomireSpikeMaximumAcceleration(SuperMetroidAddressSpace rom)
    {
        for (int slot = 0; slot < 18; slot++)
        {
            int address = 0x8691e7 + 2 * slot;
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(expected, CrocomireSpikeMotion.MaximumAcceleration(slot),
                $"Crocomire spike acceleration limit, physical slot {slot}");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireSpikeMotion.MaximumAcceleration(-1), "negative acceleration limit slot");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireSpikeMotion.MaximumAcceleration(18), "acceleration limit slot past end");
    }

    /// <summary>Checks the low byte of each slot's velocity cap and native table bounds.</summary>
    internal static void VerifyCrocomireSpikeMaximumVelocity(SuperMetroidAddressSpace rom)
    {
        for (int slot = 0; slot < 18; slot++)
            AssertEqual(rom.ReadCartridgeByte(0x86920b + 2 * slot), CrocomireSpikeMotion.MaximumVelocity(slot),
                $"Crocomire spike velocity limit low byte, physical slot {slot}");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireSpikeMotion.MaximumVelocity(-1), "negative velocity limit slot");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireSpikeMotion.MaximumVelocity(18), "velocity limit slot past end");
    }
}
