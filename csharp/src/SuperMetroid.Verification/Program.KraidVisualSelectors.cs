using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidNailVisualSelectors(SuperMetroidAddressSpace rom)
    {
        ushort[] operands = [0x8b0c, 0x8b10, 0x8b14, 0x8b18, 0x8b1c, 0x8b20, 0x8b24, 0x8b28];
        foreach (ushort enemy in new[] { RoomEnemySystem.KraidGoodNailDefinition, RoomEnemySystem.KraidBadNailDefinition })
            VerifyKraidVisualSelectorDomain(rom, 0xa70000, enemy, operands);
    }

    private static void VerifyFakeKraidVisualSelectors(SuperMetroidAddressSpace rom)
    {
        ushort[] operands =
        [
            0x99b0, 0x99b4, 0x99b8, 0x99bc, 0x99c8, 0x99ce, 0x99d2, 0x99d6,
            0x99de, 0x99e4, 0x99ea, 0x99ee, 0x99fe, 0x9a02, 0x9a06, 0x9a0a,
            0x9a16, 0x9a1c, 0x9a20, 0x9a24, 0x9a2c, 0x9a32, 0x9a38, 0x9a3c,
        ];
        Suite(nameof(VerifyKraidVisualSelectorDomain), () => VerifyKraidVisualSelectorDomain(rom, 0xa60000, RoomEnemySystem.FakeKraidDefinition, operands));
    }

    private static void VerifyKraidVisualSelectorDomain(SuperMetroidAddressSpace rom, int bank,
        ushort enemy, ushort[] operands)
    {
        // Operand identities are independently recorded from native timed-frame instructions.
        var valid = operands.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(valid.Contains((ushort)address), bank == 0xa60000
                ? FakeKraidInstructionProgramDefinitions.IsPresentationWord((ushort)address)
                : KraidNailInstructionProgramDefinitions.IsPresentationWord((ushort)address),
                "Kraid-family complete presentation membership domain");
        for (int address = operands[0] - 2; address <= operands[^1] + 8; address++)
        {
            ushort selected = (ushort)address;
            if (!operands.Contains(selected))
            {
                AssertThrows<InvalidDataException>(() => KraidVisualDefinitions.FrameAt(enemy, selected),
                    "Nonvisual instruction bytes and unused program gap rejected");
                AssertTrue(!CompiledEnemyVisualSelectors.TryGet((byte)(bank >> 16), selected, out ushort missing),
                    "Kraid-family shared selector rejects holes");
                AssertEqual((ushort)0, missing, "Kraid-family missing shared selector clears output");
                continue;
            }
            ushort expected = (ushort)(rom.ReadByte(bank | address) | rom.ReadByte(bank | (address + 1)) << 8);
            AssertEqual(expected, KraidVisualDefinitions.FrameAt(enemy, selected), "Original native visual selector word");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet((byte)(bank >> 16), selected, out ushort shared),
                "Kraid-family shared calculated selector");
            AssertEqual(expected, shared, "Kraid-family shared native visual value");
        }
        foreach (ushort invalid in new ushort[] { 0, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => KraidVisualDefinitions.FrameAt(enemy, invalid), "Outside visual program bounds");
        AssertThrows<InvalidDataException>(() => KraidVisualDefinitions.FrameAt(0, operands[0]), "Unknown visual family");
    }
}
