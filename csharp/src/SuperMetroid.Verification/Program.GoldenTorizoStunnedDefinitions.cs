using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Represents golden torizo stunned definitions verification.</summary>
internal static class GoldenTorizoStunnedDefinitionsVerification
{
    /// <summary>verify golden torizo stunned definitions.</summary>
    internal static void VerifyGoldenTorizoStunnedDefinitions(ISnesAddressSpace rom)
    {
        const byte bank = 0xaa;
        var addresses = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoStunnedInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord word =
                GoldenTorizoStunnedInstructionProgramDefinitions.MechanicsWord(index);
            AssertTrue(addresses.Add(word.Address),
                $"Golden Torizo stunned control $AA:{word.Address:X4} is unique");
            AssertTrue(word.Address is >= GoldenTorizoStunnedInstructionProgramDefinitions.Start and
                       < GoldenTorizoStunnedInstructionProgramDefinitions.End,
                $"Golden Torizo stunned control $AA:{word.Address:X4} is bounded");
            AssertEqual(ReadWord(word.Address), word.Value,
                $"Golden Torizo stunned control $AA:{word.Address:X4}");
            AssertTrue(GoldenTorizoStunnedInstructionProgramDefinitions
                    .TryReadMechanicsWord(word.Address, out ushort selected) &&
                       selected == word.Value,
                $"Golden Torizo stunned control $AA:{word.Address:X4} lookup");
            AssertTrue(GoldenTorizoStunnedInstructionProgramDefinitions
                    .IsCompiledMechanicsByte(((int)new SnesAddress(bank, word.Address))) &&
                       GoldenTorizoStunnedInstructionProgramDefinitions
                    .IsCompiledMechanicsByte(((int)new SnesAddress(bank, unchecked((ushort)(word.Address + 1))))),
                $"Golden Torizo stunned control $AA:{word.Address:X4} owns both bytes");
        }
        AssertEqual(28, addresses.Count,
            "Golden Torizo callable stunned control-word count");

        foreach (ushort transfer in new ushort[] { 0xd1a5, 0xd1b2, 0xd1bf, 0xd1cc })
        {
            AssertTrue(TorizoInstructionVramTransferDefinitions.TryGet(
                    transfer, out TorizoInstructionVramTransferDefinition descriptor),
                $"Golden Torizo stunned transfer $AA:{transfer:X4} is compiled");
            AssertEqual((ushort)CommonEnemyInstruction.CopyToVram, ReadWord(transfer),
                $"Golden Torizo stunned transfer $AA:{transfer:X4} opcode");
            AssertEqual(descriptor.ByteCount,
                ReadWord(unchecked((ushort)(transfer + 2))),
                $"Golden Torizo stunned transfer $AA:{transfer:X4} length");
            int source = ((int)new SnesAddress(                GoldenTorizoStunnedDefinitionsRomByte(rom, (bank << 16) | (transfer + 6)), checked((ushort)(GoldenTorizoStunnedDefinitionsRomByte(rom, (bank << 16) | (transfer + 4)) |
                GoldenTorizoStunnedDefinitionsRomByte(rom, (bank << 16) | (transfer + 5)) << 8))));
            AssertEqual(descriptor.SourceAddress, source,
                $"Golden Torizo stunned transfer $AA:{transfer:X4} source");
            AssertEqual(descriptor.DestinationWord,
                ReadWord(unchecked((ushort)(transfer + 7))),
                $"Golden Torizo stunned transfer $AA:{transfer:X4} destination");
            for (int byteOffset = 2; byteOffset < 9; byteOffset++)
                AssertTrue(!GoldenTorizoStunnedInstructionProgramDefinitions
                    .IsCompiledMechanicsByte(((int)new SnesAddress(bank, checked((ushort)(transfer + byteOffset))))),
                    $"Golden Torizo stunned transfer $AA:{transfer:X4} descriptor is not an opcode");
        }
        Console.WriteLine(
            "Golden Torizo callable stun: 28 control words and four separate " +
            "VRAM descriptors match the pinned cartridge; no spritemap selectors.");

        ushort ReadWord(ushort address) =>
            (ushort)(SnesLittleEndian.Word(GoldenTorizoStunnedDefinitionsRomByte(rom, (bank << 16) | address), GoldenTorizoStunnedDefinitionsRomByte(rom, (bank << 16) |
                    unchecked((ushort)(address + 1)))));
    }

    /// <summary>golden torizo stunned definitions rom byte.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Golden Torizo stunned definitions")]
    private static byte GoldenTorizoStunnedDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
