using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>Checks Kraid-family visual selector domains and values against native timed frames.</summary>
internal static class KraidVisualSelectorsVerification
{
    /// <summary>Checks the twenty-four Mini Kraid timed-frame visual operands and surrounding holes.</summary>
    internal static void VerifyFakeKraidVisualSelectors(SuperMetroidAddressSpace rom)
    {
        ushort[] operands =
        [
            0x99b0, 0x99b4, 0x99b8, 0x99bc, 0x99c8, 0x99ce, 0x99d2, 0x99d6,
            0x99de, 0x99e4, 0x99ea, 0x99ee, 0x99fe, 0x9a02, 0x9a06, 0x9a0a,
            0x9a16, 0x9a1c, 0x9a20, 0x9a24, 0x9a2c, 0x9a32, 0x9a38, 0x9a3c,
        ];
        Suite(nameof(VerifyKraidVisualSelectorDomain), () => VerifyKraidVisualSelectorDomain(rom, 0xa60000, EnemyDefinitionId.MiniKraid, operands));
    }

    /// <summary>Compares full membership and native values through family and shared selector catalogs.</summary>
    private static void VerifyKraidVisualSelectorDomain(SuperMetroidAddressSpace rom, int bank,
        EnemyDefinitionId enemy, ushort[] operands)
    {
        // Operand identities are independently recorded from native timed-frame instructions.
        var valid = operands.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(valid.Contains((ushort)address), FakeKraidInstructionProgramDefinitions.IsPresentationWord((ushort)address),
                "Kraid-family complete presentation membership domain");
        for (int address = operands[0] - 2; address <= operands[^1] + 8; address++)
        {
            ushort selected = (ushort)address;
            if (!operands.Contains(selected))
            {
                AssertThrows<InvalidDataException>(() => KraidVisualDefinitions.FrameAt(enemy, selected),
                    "Nonvisual instruction bytes and unused program gap rejected");
                AssertTrue(!CompiledEnemyVisualSelectors.TryGet(SnesLittleEndian.BankByte(bank), selected, out ushort missing),
                    "Kraid-family shared selector rejects holes");
                AssertEqual((ushort)0, missing, "Kraid-family missing shared selector clears output");
                continue;
            }
            ushort expected = (ushort)(SnesLittleEndian.Word(KraidVisualSelectorDomainRomByte(rom, bank | address), KraidVisualSelectorDomainRomByte(rom, bank | (address + 1))));
            AssertEqual(expected, KraidVisualDefinitions.FrameAt(enemy, selected), "Original native visual selector word");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(SnesLittleEndian.BankByte(bank), selected, out ushort shared),
                "Kraid-family shared calculated selector");
            AssertEqual(expected, shared, "Kraid-family shared native visual value");
        }
        foreach (ushort invalid in new ushort[] { 0, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => KraidVisualDefinitions.FrameAt(enemy, invalid), "Outside visual program bounds");
        AssertThrows<InvalidDataException>(() => KraidVisualDefinitions.FrameAt(0, operands[0]), "Unknown visual family");
    }

    /// <summary>Reads a cartridge byte for the independent Kraid visual-operand oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid visual selector domain")]
    private static byte KraidVisualSelectorDomainRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
