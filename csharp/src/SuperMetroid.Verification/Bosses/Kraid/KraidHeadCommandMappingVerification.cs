using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks the compiled Kraid head command stream against independently decoded native records.</summary>
internal static class KraidHeadCommandMappingVerification
{
    /// <summary>Checks all 28 command records, exact address resolution, growth reads, and rejection of operand addresses.</summary>
    internal static void VerifyKraidHeadCommandMapping(SuperMetroidAddressSpace rom)
    {
        ushort Word(int pointer) => (ushort)(SnesLittleEndian.Word(KraidHeadCommandMappingRomByte(rom, 0xa70000 | pointer), KraidHeadCommandMappingRomByte(rom, 0xa70000 | (pointer + 1))));
        var expected = new List<KraidHeadInstructionDefinition>();
        // Decode the original stream independently: native opcodes determine record size.
        for (int pointer = 0x96d2; pointer < 0x9788;)
        {
            ushort opcode = Word(pointer);
            KraidHeadInstructionDefinition command = opcode switch
            {
                0xffff => new((ushort)pointer, KraidHeadInstructionKind.Terminate, 0, 0, 0, 0, 0, 0),
                0xaf94 => new((ushort)pointer, KraidHeadInstructionKind.RoarSound, 0, 0, 0, 0, Word(0xaf96), 0),
                0xaf9f => new((ushort)pointer, KraidHeadInstructionKind.DyingSound, 0, 0, 0, 0, Word(0xafa1), 0),
                < 0x8000 => new((ushort)pointer, KraidHeadInstructionKind.Frame, opcode,
                    Word(pointer + 2), Word(pointer + 4), Word(pointer + 6), 0, 0),
                _ => throw new InvalidDataException($"Unknown native Kraid head opcode {opcode:X4}."),
            };
            expected.Add(command);
            pointer += opcode < 0x8000 ? 8 : 2;
        }
        AssertEqual(28, expected.Count, "Independent Kraid head stream command count");
        // A frame's mouth stage is its tilemap's rank among the head's distinct tilemaps: the closed mouth is the
        // lowest, and each further stage of opening is the next.
        List<ushort> stageTilemaps = [.. expected.Where(command => command.Kind == KraidHeadInstructionKind.Frame)
            .Select(command => command.Tilemap).Distinct().Order()];
        AssertEqual(KraidHeadInstructionDefinitions.FullyOpenMouthStage + 1, stageTilemaps.Count,
            "Independent Kraid head stream shows one tilemap per mouth stage");
        for (int command = 0; command < expected.Count; command++)
        {
            if (expected[command].Kind == KraidHeadInstructionKind.Frame)
                expected[command] = expected[command] with { MouthStage = stageTilemaps.IndexOf(expected[command].Tilemap) };
        }
        AssertEqual(expected.Count, KraidHeadInstructionDefinitions.All.Length, "Calculated head command count");
        int index = 0;
        foreach (KraidHeadInstructionDefinition actual in KraidHeadInstructionDefinitions.All)
        {
            AssertEqual(expected[index], actual, "Head command enumeration matches native record");
            AssertEqual(expected[index], KraidHeadInstructionDefinitions.All[index], "Head command index");
            AssertEqual(expected[index], KraidHeadInstructionDefinitions.Resolve(expected[index].Pointer),
                "Head exact-address resolution");
            AssertEqual(Word(actual.Pointer + 2),
                KraidHeadInstructionDefinitions.ReadGrowthSelectionWord(actual.Pointer),
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

    /// <summary>Reads one native bank-$A7 byte solely for the head-command oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid head command mapping")]
    private static byte KraidHeadCommandMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
