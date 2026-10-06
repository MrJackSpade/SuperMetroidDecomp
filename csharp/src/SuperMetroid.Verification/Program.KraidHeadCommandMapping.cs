using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidHeadCommandMapping(SuperMetroidAddressSpace rom)
    {
        ushort Word(int pointer) => (ushort)(rom.ReadByte(0xa70000 | pointer)
            | rom.ReadByte(0xa70000 | (pointer + 1)) << 8);
        var expected = new List<KraidHeadInstructionDefinition>();
        // Decode the original stream independently: native opcodes determine record size.
        for (int pointer = 0x96d2; pointer < 0x9788;)
        {
            ushort opcode = Word(pointer);
            KraidHeadInstructionDefinition command = opcode switch
            {
                0xffff => new((ushort)pointer, KraidHeadInstructionKind.Terminate, 0, 0, 0, 0, 0),
                0xaf94 => new((ushort)pointer, KraidHeadInstructionKind.RoarSound, 0, 0, 0, 0, Word(0xaf96)),
                0xaf9f => new((ushort)pointer, KraidHeadInstructionKind.DyingSound, 0, 0, 0, 0, Word(0xafa1)),
                < 0x8000 => new((ushort)pointer, KraidHeadInstructionKind.Frame, opcode,
                    Word(pointer + 2), Word(pointer + 4), Word(pointer + 6), 0),
                _ => throw new InvalidDataException($"Unknown native Kraid head opcode {opcode:X4}."),
            };
            expected.Add(command);
            pointer += opcode < 0x8000 ? 8 : 2;
        }
        AssertEqual(28, expected.Count, "Independent Kraid head stream command count");
        AssertEqual(expected.Count, KraidHeadInstructionDefinitions.All.Length, "Calculated head command count");
        int index = 0;
        foreach (KraidHeadInstructionDefinition actual in KraidHeadInstructionDefinitions.All)
        {
            AssertEqual(expected[index], actual, "Head command enumeration matches native record");
            AssertEqual(expected[index], KraidHeadInstructionDefinitions.All[index], "Head command index");
            AssertEqual(expected[index], KraidHeadInstructionDefinitions.Resolve(expected[index].Pointer),
                "Head exact-address resolution");
            AssertEqual(Word(actual.Pointer + 2),
                KraidHeadInstructionDefinitions.ReadGrowthSelectionWord(rom, actual.Pointer),
                "Growth selection reads the native raw word for frame, sound and terminal cursors");
            index++;
        }
        AssertEqual(expected.Count, index, "Head enumeration ends at native stream boundary");
        AssertTrue(expected.SequenceEqual(KraidHeadInstructionDefinitions.All.ToArray()), "Head materialized asset view");
        var starts = expected.Select(command => command.Pointer).ToHashSet();
        for (int pointer = 0x96d1; pointer <= 0x9788; pointer++)
        {
            ushort address = (ushort)pointer;
            if (starts.Contains(address)) continue;
            AssertThrows<InvalidDataException>(() => KraidHeadInstructionDefinitions.Resolve(address),
                "Head resolver rejects operands, odd addresses and adjacent data");
        }
        AssertThrows<IndexOutOfRangeException>(() => KraidHeadInstructionDefinitions.Command(-1), "Head negative index");
        AssertThrows<IndexOutOfRangeException>(() => KraidHeadInstructionDefinitions.Command(28), "Head upper index");
        AssertThrows<InvalidDataException>(() => KraidHeadInstructionDefinitions.Resolve(0), "Head low-half command rejection");
        AssertThrows<InvalidDataException>(() => KraidHeadInstructionDefinitions.Resolve(0xffff), "Head upper command rejection");
    }
}
