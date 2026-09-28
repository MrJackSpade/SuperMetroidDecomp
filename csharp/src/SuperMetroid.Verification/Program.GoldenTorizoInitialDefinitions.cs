using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyGoldenTorizoInitialDefinitions(ISnesAddressSpace rom)
    {
        const byte bank = GoldenTorizoInitialFrameDefinitions.Bank;
        foreach (GoldenTorizoInitialMechanicsWord word in
                 Enumerable.Range(0,
                     GoldenTorizoInitialInstructionProgramDefinitions.MechanicsWordCount)
                     .Select(GoldenTorizoInitialInstructionProgramDefinitions.MechanicsWord))
        {
            AssertEqual(ReadWord(word.Address), word.Value,
                $"Golden Torizo initial mechanics $AA:{word.Address:X4}");
            AssertTrue(GoldenTorizoInitialInstructionProgramDefinitions
                    .TryReadMechanicsWord(word.Address, out ushort selected) &&
                       selected == word.Value,
                $"Golden Torizo initial word $AA:{word.Address:X4} lookup");
            AssertTrue(GoldenTorizoInitialInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) | word.Address) &&
                       GoldenTorizoInitialInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) |
                        unchecked((ushort)(word.Address + 1))),
                $"Golden Torizo initial word $AA:{word.Address:X4} owns both bytes");
        }

        ushort visualOperand = GoldenTorizoInitialInstructionProgramDefinitions
            .PresentationWordAddress(0);
        AssertEqual(GoldenTorizoInitialFrameDefinitions.Frame,
            ReadWord(visualOperand),
            "Golden Torizo initial frame selection matches the cartridge");
        AssertTrue(CompiledEnemyVisualSelectors.TryGet(bank, visualOperand,
                out ushort selectedFrame) &&
                   selectedFrame == GoldenTorizoInitialFrameDefinitions.Frame,
            "Golden Torizo initial frame selector is compiled");
        AssertTrue(!GoldenTorizoInitialInstructionProgramDefinitions
                .TryReadMechanicsWord(visualOperand, out _),
            "Golden Torizo visual operand is not executable mechanics");
        AssertTrue(!GoldenTorizoInitialInstructionProgramDefinitions
                .TryReadMechanicsWord(0xc9e2, out _),
            "later Golden Torizo program is not invented by initial catalog");

        ushort frame = GoldenTorizoInitialFrameDefinitions.Frame;
        AssertEqual(GoldenTorizoInitialFrameDefinitions.ComponentCount,
            ReadWord(frame), "Golden Torizo initial extended component count");
        AssertEqual((ushort)0, ReadWord(unchecked((ushort)(frame + 2))),
            "Golden Torizo initial component X offset");
        AssertEqual((ushort)0, ReadWord(unchecked((ushort)(frame + 4))),
            "Golden Torizo initial component Y offset");
        AssertEqual(GoldenTorizoInitialFrameDefinitions.HitboxList,
            ReadWord(unchecked((ushort)(frame + 8))),
            "Golden Torizo initial component hitbox pointer");
        ushort hitbox = GoldenTorizoInitialFrameDefinitions.HitboxList;
        AssertEqual(GoldenTorizoInitialFrameDefinitions.HitboxCount,
            ReadWord(hitbox), "Golden Torizo initial hitbox count");
        AssertEqual(unchecked((ushort)GoldenTorizoInitialFrameDefinitions.Left),
            ReadWord(unchecked((ushort)(hitbox + 2))),
            "Golden Torizo initial hitbox left");
        AssertEqual(unchecked((ushort)GoldenTorizoInitialFrameDefinitions.Top),
            ReadWord(unchecked((ushort)(hitbox + 4))),
            "Golden Torizo initial hitbox top");
        AssertEqual(unchecked((ushort)GoldenTorizoInitialFrameDefinitions.Right),
            ReadWord(unchecked((ushort)(hitbox + 6))),
            "Golden Torizo initial hitbox right");
        AssertEqual(unchecked((ushort)GoldenTorizoInitialFrameDefinitions.Bottom),
            ReadWord(unchecked((ushort)(hitbox + 8))),
            "Golden Torizo initial hitbox bottom");
        AssertEqual(GoldenTorizoInitialFrameDefinitions.TouchAi,
            ReadWord(unchecked((ushort)(hitbox + 10))),
            "Golden Torizo initial hitbox touch callback");
        AssertEqual(GoldenTorizoInitialFrameDefinitions.ShotAi,
            ReadWord(unchecked((ushort)(hitbox + 12))),
            "Golden Torizo initial hitbox shot callback");
        Console.WriteLine(
            "Golden Torizo initial entry: seven mechanics words, one visual frame, " +
            "and one native collision rectangle match the pinned cartridge.");

        ushort ReadWord(ushort address) =>
            (ushort)(rom.ReadByte((bank << 16) | address) |
                rom.ReadByte((bank << 16) |
                    unchecked((ushort)(address + 1))) << 8);
    }
}
