using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// The dormant entry is a bounded bank-$AA program segment. Match every
    /// compiled control word and the deliberately empty hitbox against the
    /// pinned cartridge; later fight programs still need their own catalog.
    /// </summary>
    private static void VerifyBombTorizoDormantDefinitions(ISnesAddressSpace rom)
    {
        const byte bank = BombTorizoDormantFrameDefinitions.Bank;
        for (int index = 0;
             index < BombTorizoDormantInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            BombTorizoDormantMechanicsWord word =
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
        AssertEqual(BombTorizoDormantFrameDefinitions.Frame, ReadWord(visualOperand),
            "Bomb Torizo dormant script selects the shared extended frame");
        AssertTrue(CompiledEnemyVisualSelectors.TryGet(bank, visualOperand,
                out ushort installedFrame) &&
                   installedFrame == BombTorizoDormantFrameDefinitions.Frame,
            "Bomb Torizo dormant visual selector is compiled");
        AssertTrue(!BombTorizoDormantInstructionProgramDefinitions
                .TryReadMechanicsWord(visualOperand, out _),
            "Bomb Torizo visual operand is not executable mechanics");
        AssertTrue(!BombTorizoDormantInstructionProgramDefinitions
                .TryReadMechanicsWord(0xb887, out _),
            "later awakening instructions are not invented by dormant catalog");

        ushort frame = BombTorizoDormantFrameDefinitions.Frame;
        AssertEqual(BombTorizoDormantFrameDefinitions.ComponentCount,
            ReadWord(frame), "dormant Torizo extended frame has one component");
        AssertEqual(BombTorizoDormantFrameDefinitions.EmptyHitboxList,
            ReadWord(unchecked((ushort)(frame + 8))),
            "dormant Torizo component selects the blank hitbox list");
        AssertEqual(BombTorizoDormantFrameDefinitions.HitboxCount,
            ReadWord(BombTorizoDormantFrameDefinitions.EmptyHitboxList),
            "dormant Torizo has no touch or shot hitbox");
        Console.WriteLine(
            "Bomb Torizo dormant entry: six mechanics words, one visual selector, " +
            "and the empty collision list match the pinned cartridge.");

        ushort ReadWord(ushort address) =>
            (ushort)(rom.ReadByte((bank << 16) | address) |
                rom.ReadByte((bank << 16) |
                    unchecked((ushort)(address + 1))) << 8);
    }
}
