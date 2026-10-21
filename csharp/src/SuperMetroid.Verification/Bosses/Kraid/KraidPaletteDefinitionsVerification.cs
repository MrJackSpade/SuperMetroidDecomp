using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks the native source addresses and extents used by Kraid's installed palettes.</summary>
internal static class KraidPaletteDefinitionsVerification
{
    /// <summary>Reads five indexed-load operands from bank $A7 and confirms each compiled Kraid palette source address and invalid-identity rejection.</summary>
    internal static void VerifyKraidPaletteSourceAddresses(SuperMetroidAddressSpace rom)
    {
        NativeLoad[] nativeLoads =
        [
            new NativeLoad(KraidPaletteSource.RoomBackdrop, 0xa7a981),
            new NativeLoad(KraidPaletteSource.InitialTarget, 0xa7aa97),
            new NativeLoad(KraidPaletteSource.Health, 0xa7b3bb),
            new NativeLoad(KraidPaletteSource.Secondary, 0xa7b3c2),
            new NativeLoad(KraidPaletteSource.DeathArm, 0xa7c37b),
        ];
        foreach (var (source, instruction) in nativeLoads)
        {
            byte opcode = KraidPaletteSourceAddressesRomByte(rom, instruction);
            AssertTrue(opcode is 0xb9 or 0xbd, "Native palette source is indexed absolute LDA");
            int expected = 0xa70000 | KraidPaletteSourceAddressesRomByte(rom, instruction + 1) | KraidPaletteSourceAddressesRomByte(rom, instruction + 2) << 8;
            AssertEqual(expected, KraidPaletteRomData.SourceAddress(source), "Native palette source operand");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 5, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => KraidPaletteRomData.SourceAddress((KraidPaletteSource)invalid),
                "Unknown palette address source rejected");
    }

    /// <summary>Reads a byte of retail bank-$A7 code solely for the source-address oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid palette source addresses")]
    private static byte KraidPaletteSourceAddressesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Uses adjacent bank-$A7 symbol boundaries to check complete backdrop, target, health, secondary, and death-arm palette lengths.</summary>
    internal static void VerifyKraidPaletteSourceLengths()
    {
        Extent[] extents =
        [
            new Extent(KraidPaletteSource.RoomBackdrop, 0x86c7, 0x86e7),
            new Extent(KraidPaletteSource.InitialTarget, 0xaaa6, 0xaac6),
            new Extent(KraidPaletteSource.Health, 0xb3d3, 0xb4f3),
            new Extent(KraidPaletteSource.Secondary, 0xb513, 0xb633),
            new Extent(KraidPaletteSource.DeathArm, 0xb4f3, 0xb513),
        ];
        foreach (var (source, start, end) in extents)
            AssertEqual((end - start) / 2, KraidPaletteRomData.ColorCount(source), "Complete native palette extent");
        foreach (int invalid in new[] { int.MinValue, -1, 5, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => KraidPaletteRomData.ColorCount((KraidPaletteSource)invalid),
                "Unknown palette size source rejected");
    }
}
