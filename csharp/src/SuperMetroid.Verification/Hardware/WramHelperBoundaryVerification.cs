using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Hardware;
using SuperMetroid.Verification.Harness;

/// <summary>Checks migrated WRAM helpers with an address space that has no cartridge.</summary>
internal static class WramHelperBoundaryVerification
{
    /// <summary>Checks the statically identified WRAM-only helpers without importing a cartridge or running gameplay.</summary>
    internal static void VerifyWramHelperBoundary()
    {
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        VerifyWramWordMapping(memory);
        Console.WriteLine("WRAM helper boundary: bank wrapping and source rejection pass without a cartridge.");
    }

    /// <summary>Checks word wrapping, system-bank mirrors, and rejection of non-WRAM addresses.</summary>
    private static void VerifyWramWordMapping(SuperMetroidAddressSpace memory)
    {
        memory.WriteByte(0x7effff, 0x12);
        memory.WriteByte(0x7e0000, 0x34);
        memory.WriteByte(0x7f0000, 0x56);
        AssertEqual((ushort)0x3412, memory.ReadWorkRamWordFixedBank(0x7effff),
            "WRAM word wraps its offset inside the same bank");
        AssertEqual((ushort)0x0034, memory.ReadWorkRamWordFixedBank(0x800000),
            "WRAM word accepts native system-bank mirrors");
        foreach (int address in new[] { 0x908000, 0x700000, 0x002140 })
            AssertThrows<ArgumentOutOfRangeException>(() => memory.ReadWorkRamWordFixedBank(address),
                "WRAM helper rejects cartridge, SRAM and peripheral sources");
    }

    /// <summary>Runs the cartridge-free WRAM helper checks.</summary>
    [VerificationCommand("--wram-helper-boundary")]
    internal static int WramHelperBoundaryCommand()
    {
        Suite(nameof(VerifyWramHelperBoundary), () => VerifyWramHelperBoundary());
        return 0;
    }
}
