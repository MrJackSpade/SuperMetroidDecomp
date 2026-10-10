using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// The dormant entry is a bounded bank-$AA program segment. Match every
    /// compiled control word and its visual selector against the pinned cartridge.
    /// </summary>
    private static void VerifyBombTorizoDormantDefinitions(ISnesAddressSpace rom)
    {
        Suite(nameof(VerifyBombTorizoDormantControlMapping), () => VerifyBombTorizoDormantControlMapping(rom));
        const byte bank = 0xaa;
        for (int index = 0;
             index < BombTorizoDormantInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord word =
                BombTorizoDormantInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = ReadWord(word.Address);
            AssertEqual(native, word.Value,
                $"Bomb Torizo dormant mechanics $AA:{word.Address:X4}");
            AssertTrue(BombTorizoDormantInstructionProgramDefinitions
                    .TryReadMechanicsWord(word.Address, out ushort selected) &&
                       selected == native,
                $"Bomb Torizo dormant word $AA:{word.Address:X4} selects native value");
            AssertTrue(BombTorizoDormantInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) | word.Address) &&
                       BombTorizoDormantInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) |
                        unchecked((ushort)(word.Address + 1))),
                $"Bomb Torizo dormant word $AA:{word.Address:X4} owns both bytes");
        }

        ushort visualOperand = BombTorizoDormantInstructionProgramDefinitions
            .PresentationWordAddress(0);
        AssertTrue(CompiledEnemyVisualSelectors.TryGet(bank, visualOperand,
                out ushort installedFrame) &&
                   installedFrame == ReadWord(visualOperand),
            "Bomb Torizo dormant visual selector is compiled from the cartridge");
        AssertTrue(!BombTorizoDormantInstructionProgramDefinitions
                .TryReadMechanicsWord(visualOperand, out _),
            "Bomb Torizo visual operand is not executable mechanics");
        AssertTrue(!BombTorizoDormantInstructionProgramDefinitions
                .TryReadMechanicsWord(0xb887, out _),
            "later awakening instructions are not invented by dormant catalog");

        Console.WriteLine(
            "Bomb Torizo dormant entry: six mechanics words and one visual selector " +
            "match the pinned cartridge.");

        ushort ReadWord(ushort address) =>
            (ushort)(rom.ReadByte((bank << 16) | address) |
                rom.ReadByte((bank << 16) |
                    unchecked((ushort)(address + 1))) << 8);
    }
    /// <summary>Checks the six dormant mechanics words and byte ownership against ROM, keeps the visual operand separate, and rejects unsupported addresses and ordinals.</summary>
    /// <param name="rom">Pinned cartridge address space supplying the expected bank-$AA words.</param>
    private static void VerifyBombTorizoDormantControlMapping(ISnesAddressSpace rom)
    {
        ushort[] expected = [0xb879,0xb87b,0xb87d,0xb881,0xb883,0xb885];
        AssertEqual(expected.Length, BombTorizoDormantInstructionProgramDefinitions.MechanicsWordCount, "dormant native control count");
        var words = expected.ToHashSet();
        var bytes = expected.SelectMany(address => new[] {(int)address, address + 1}).ToHashSet();
        for (int i = 0; i < expected.Length; i++)
            AssertEqual(expected[i], BombTorizoDormantInstructionProgramDefinitions.MechanicsWord(i).Address, "dormant native enumeration position");
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = BombTorizoDormantInstructionProgramDefinitions.TryReadMechanicsWord((ushort)address, out ushort value);
            AssertEqual(words.Contains((ushort)address), found, "dormant full control membership");
            ushort native = found ? (ushort)(rom.ReadByte(0xaa0000 | address) | rom.ReadByte(0xaa0000 | (address + 1)) << 8) : (ushort)0;
            AssertEqual(native, value, "dormant original control value or cleared missing output");
            AssertEqual(bytes.Contains(address), BombTorizoDormantInstructionProgramDefinitions.IsCompiledMechanicsByte(0xaa0000 | address), "dormant exact byte ownership");
            AssertEqual(bytes.Contains(address), BombTorizoDormantInstructionProgramDefinitions.IsCompiledMechanicsByte(0x1aa0000 | address), "dormant existing high-bit alias");
            AssertTrue(!BombTorizoDormantInstructionProgramDefinitions.IsCompiledMechanicsByte(0xab0000 | address), "dormant other bank rejected");
        }
        foreach (int index in new[] {int.MinValue,-1,6,int.MaxValue})
            AssertThrows<IndexOutOfRangeException>(() => BombTorizoDormantInstructionProgramDefinitions.MechanicsWord(index), "dormant control ordinal bounds");
        AssertEqual(1, BombTorizoDormantInstructionProgramDefinitions.PresentationWordCount, "dormant one visual operand");
        AssertEqual((ushort)0xb87f, BombTorizoDormantInstructionProgramDefinitions.PresentationWordAddress(0), "dormant native visual operand position");
        foreach (int index in new[] {int.MinValue,-1,1,int.MaxValue})
            AssertThrows<ArgumentOutOfRangeException>(() => BombTorizoDormantInstructionProgramDefinitions.PresentationWordAddress(index), "dormant existing visual ordinal exception");
    }
}
