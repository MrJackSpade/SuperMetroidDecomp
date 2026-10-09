using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyGoldenTorizoEyeBeamAttackDefinitions(ISnesAddressSpace rom)
    {
        const byte bank = 0xaa;
        var addresses = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoEyeBeamAttackInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord word =
                GoldenTorizoEyeBeamAttackInstructionProgramDefinitions.MechanicsWord(index);
            AssertTrue(addresses.Add(word.Address),
                $"Golden Torizo eye-beam control $AA:{word.Address:X4} is unique");
            AssertTrue(word.Address is >= GoldenTorizoEyeBeamAttackInstructionProgramDefinitions.Start and
                       < GoldenTorizoEyeBeamAttackInstructionProgramDefinitions.End,
                $"Golden Torizo eye-beam control $AA:{word.Address:X4} is bounded");
            AssertEqual(ReadWord(word.Address), word.Value,
                $"Golden Torizo eye-beam control $AA:{word.Address:X4}");
            AssertTrue(GoldenTorizoEyeBeamAttackInstructionProgramDefinitions
                    .TryReadMechanicsWord(word.Address, out ushort selected) &&
                       selected == word.Value,
                $"Golden Torizo eye-beam control $AA:{word.Address:X4} lookup");
            AssertTrue(GoldenTorizoEyeBeamAttackInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) | word.Address) &&
                       GoldenTorizoEyeBeamAttackInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) |
                        unchecked((ushort)(word.Address + 1))),
                $"Golden Torizo eye-beam control $AA:{word.Address:X4} owns both bytes");
        }
        AssertEqual(41, addresses.Count,
            "Golden Torizo callable eye-beam control-word count");

        foreach (ushort transfer in new ushort[] { 0xd137, 0xd144, 0xd151, 0xd15e })
        {
            AssertTrue(TorizoInstructionVramTransferDefinitions.TryGet(
                    transfer, out TorizoInstructionVramTransferDefinition descriptor),
                $"Golden Torizo eye-beam transfer $AA:{transfer:X4} is compiled");
            AssertEqual(CommonEnemyInstructionCodes.CopyToVram, ReadWord(transfer),
                $"Golden Torizo eye-beam transfer $AA:{transfer:X4} opcode");
            AssertEqual(descriptor.ByteCount,
                ReadWord(unchecked((ushort)(transfer + 2))),
                $"Golden Torizo eye-beam transfer $AA:{transfer:X4} length");
            int source = rom.ReadByte((bank << 16) | (transfer + 4)) |
                rom.ReadByte((bank << 16) | (transfer + 5)) << 8 |
                rom.ReadByte((bank << 16) | (transfer + 6)) << 16;
            AssertEqual(descriptor.SourceAddress, source,
                $"Golden Torizo eye-beam transfer $AA:{transfer:X4} source");
            AssertEqual(descriptor.DestinationWord,
                ReadWord(unchecked((ushort)(transfer + 7))),
                $"Golden Torizo eye-beam transfer $AA:{transfer:X4} destination");
            for (int byteOffset = 2; byteOffset < 9; byteOffset++)
                AssertTrue(!GoldenTorizoEyeBeamAttackInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) | (transfer + byteOffset)),
                    $"Golden Torizo eye-beam transfer $AA:{transfer:X4} descriptor is not an opcode");
        }
        Console.WriteLine(
            "Golden Torizo callable eye beam: 41 control words and four separate " +
            "VRAM descriptors match the pinned cartridge; no spritemap selectors.");

        ushort ReadWord(ushort address) =>
            (ushort)(rom.ReadByte((bank << 16) | address) |
                rom.ReadByte((bank << 16) |
                    unchecked((ushort)(address + 1))) << 8);
    }
}
