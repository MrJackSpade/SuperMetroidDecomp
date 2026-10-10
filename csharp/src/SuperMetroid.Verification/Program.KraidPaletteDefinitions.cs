using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Validates the five Kraid palette source addresses against the indexed loads in the retail AI code.</summary>
    /// <param name="rom">Address space containing Kraid's native bank-$A7 instructions and palette operands.</param>
    private static void VerifyKraidPaletteSourceAddresses(SuperMetroidAddressSpace rom)
    {
        (KraidPaletteSource Source, int Instruction)[] nativeLoads =
        [
            (KraidPaletteSource.RoomBackdrop, 0xa7a981),
            (KraidPaletteSource.InitialTarget, 0xa7aa97),
            (KraidPaletteSource.Health, 0xa7b3bb),
            (KraidPaletteSource.Secondary, 0xa7b3c2),
            (KraidPaletteSource.DeathArm, 0xa7c37b),
        ];
        foreach (var (source, instruction) in nativeLoads)
        {
            byte opcode = rom.ReadByte(instruction);
            AssertTrue(opcode is 0xb9 or 0xbd, "Native palette source is indexed absolute LDA");
            int expected = 0xa70000 | rom.ReadByte(instruction + 1) | rom.ReadByte(instruction + 2) << 8;
            AssertEqual(expected, KraidPaletteRomData.SourceAddress(source), "Native palette source operand");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 5, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => KraidPaletteRomData.SourceAddress((KraidPaletteSource)invalid),
                "Unknown palette address source rejected");
    }

    /// <summary>Checks that each compiled Kraid palette length matches its complete native bank-$A7 data extent.</summary>
    private static void VerifyKraidPaletteSourceLengths()
    {
        // Independent bank_A7 symbol extents: next definition starts immediately
        // after each complete source. Health sources include flash plus eight bands.
        (KraidPaletteSource Source, int Start, int End)[] extents =
        [
            (KraidPaletteSource.RoomBackdrop, 0x86c7, 0x86e7),
            (KraidPaletteSource.InitialTarget, 0xaaa6, 0xaac6),
            (KraidPaletteSource.Health, 0xb3d3, 0xb4f3),
            (KraidPaletteSource.Secondary, 0xb513, 0xb633),
            (KraidPaletteSource.DeathArm, 0xb4f3, 0xb513),
        ];
        foreach (var (source, start, end) in extents)
            AssertEqual((end - start) / 2, KraidPaletteRomData.ColorCount(source), "Complete native palette extent");
        foreach (int invalid in new[] { int.MinValue, -1, 5, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => KraidPaletteRomData.ColorCount((KraidPaletteSource)invalid),
                "Unknown palette size source rejected");
    }
}
