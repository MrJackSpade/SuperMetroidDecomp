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
        VerifyGoldenTorizoAwakeningDefinitions(rom);
        VerifyGoldenTorizoWalkingDefinitions(rom);
        VerifyGoldenTorizoRightwardDefinitions(rom);
        VerifyTorizoJumpBackDefinitions(rom);
        VerifyGoldenTorizoJumpLandingDefinitions(rom);
        VerifyGoldenTorizoRightOrbDefinitions(rom);
        VerifyGoldenTorizoRightSonicDefinitions(rom);

        ushort ReadWord(ushort address) =>
            (ushort)(rom.ReadByte((bank << 16) | address) |
                rom.ReadByte((bank << 16) |
                    unchecked((ushort)(address + 1))) << 8);
    }

    private static void VerifyGoldenTorizoAwakeningDefinitions(ISnesAddressSpace rom)
    {
        const byte bank = GoldenTorizoAwakeningCollisionDefinitions.Bank;
        var mechanicsAddresses = new HashSet<ushort>();
        int transfers = 0;
        for (int index = 0;
             index < GoldenTorizoAwakeningInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            GoldenTorizoAwakeningMechanicsWord word =
                GoldenTorizoAwakeningInstructionProgramDefinitions.MechanicsWord(index);
            AssertTrue(mechanicsAddresses.Add(word.Address),
                $"Golden Torizo awakening mechanics $AA:{word.Address:X4} is unique");
            AssertTrue(word.Address >= GoldenTorizoAwakeningInstructionProgramDefinitions.Start &&
                       word.Address < GoldenTorizoAwakeningInstructionProgramDefinitions.End,
                $"Golden Torizo awakening mechanics $AA:{word.Address:X4} is in the bounded program");
            AssertEqual(ReadWord(word.Address), word.Value,
                $"Golden Torizo awakening mechanics $AA:{word.Address:X4}");
            AssertTrue(GoldenTorizoAwakeningInstructionProgramDefinitions
                    .TryReadMechanicsWord(word.Address, out ushort selected) &&
                       selected == word.Value,
                $"Golden Torizo awakening word $AA:{word.Address:X4} lookup");
            AssertTrue(GoldenTorizoAwakeningInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) | word.Address) &&
                       GoldenTorizoAwakeningInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) |
                        unchecked((ushort)(word.Address + 1))),
                $"Golden Torizo awakening word $AA:{word.Address:X4} owns both bytes");
            if (word.Value == CommonEnemyInstructionCodes.CopyToVram)
                transfers++;
        }
        AssertEqual(8, transfers, "Golden Torizo awakening transfer opcodes");
        AssertEqual(21,
            GoldenTorizoAwakeningInstructionProgramDefinitions.PresentationWordCount,
            "Golden Torizo awakening presentation-operand count");
        var selectedFrames = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoAwakeningInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = GoldenTorizoAwakeningInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = ReadWord(address);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(bank, address,
                    out ushort compiled) && compiled == native,
                $"Golden Torizo awakening visual selector $AA:{address:X4}");
            AssertTrue(!mechanicsAddresses.Contains(address),
                $"Golden Torizo awakening visual selector $AA:{address:X4} is not mechanics");
            selectedFrames.Add(native);
        }
        AssertEqual(7, selectedFrames.Count,
            "Golden Torizo awakening selects all seven seated/standing frames");
        AssertEqual(7, GoldenTorizoAwakeningCollisionDefinitions.FrameCount,
            "Golden Torizo awakening physical-frame count");
        var hitboxLists = new HashSet<ushort>();
        for (int index = 0; index < GoldenTorizoAwakeningCollisionDefinitions.FrameCount; index++)
        {
            ushort frame = GoldenTorizoAwakeningCollisionDefinitions.FramePointer(index);
            AssertTrue(selectedFrames.Contains(frame),
                $"Golden Torizo physical frame $AA:{frame:X4} is selected by the script");
            AssertTrue(GoldenTorizoAwakeningCollisionDefinitions.TryGetComponents(
                    frame, out var components),
                $"Golden Torizo physical frame $AA:{frame:X4} is compiled");
            AssertEqual((ushort)components.Length, ReadWord(frame),
                $"Golden Torizo frame $AA:{frame:X4} component count");
            for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                GoldenTorizoCollisionComponent component = components.Span[componentIndex];
                ushort address = unchecked((ushort)(frame + 2 + componentIndex * 8));
                AssertEqual(unchecked((ushort)component.X), ReadWord(address),
                    $"Golden Torizo frame $AA:{frame:X4} component X {componentIndex}");
                AssertEqual(unchecked((ushort)component.Y), ReadWord(unchecked((ushort)(address + 2))),
                    $"Golden Torizo frame $AA:{frame:X4} component Y {componentIndex}");
                AssertEqual(component.HitboxList, ReadWord(unchecked((ushort)(address + 6))),
                    $"Golden Torizo frame $AA:{frame:X4} hitbox list {componentIndex}");
                hitboxLists.Add(component.HitboxList);
            }
        }
        AssertEqual(9, hitboxLists.Count,
            "Golden Torizo awakening has nine distinct physical hitbox lists");
        foreach (ushort pointer in hitboxLists)
        {
            ReadOnlySpan<GoldenTorizoCollisionHitbox> hitboxes =
                GoldenTorizoAwakeningCollisionDefinitions.HitboxesAt(pointer);
            AssertEqual((ushort)hitboxes.Length, ReadWord(pointer),
                $"Golden Torizo hitbox list $AA:{pointer:X4} count");
            for (int index = 0; index < hitboxes.Length; index++)
            {
                GoldenTorizoCollisionHitbox hitbox = hitboxes[index];
                ushort address = unchecked((ushort)(pointer + 2 + index * 12));
                AssertEqual(unchecked((ushort)hitbox.Left), ReadWord(address),
                    $"Golden Torizo hitbox $AA:{pointer:X4} left");
                AssertEqual(unchecked((ushort)hitbox.Top), ReadWord(unchecked((ushort)(address + 2))),
                    $"Golden Torizo hitbox $AA:{pointer:X4} top");
                AssertEqual(unchecked((ushort)hitbox.Right), ReadWord(unchecked((ushort)(address + 4))),
                    $"Golden Torizo hitbox $AA:{pointer:X4} right");
                AssertEqual(unchecked((ushort)hitbox.Bottom), ReadWord(unchecked((ushort)(address + 6))),
                    $"Golden Torizo hitbox $AA:{pointer:X4} bottom");
                AssertEqual(hitbox.TouchAi, ReadWord(unchecked((ushort)(address + 8))),
                    $"Golden Torizo hitbox $AA:{pointer:X4} touch AI");
                AssertEqual(hitbox.ShotAi, ReadWord(unchecked((ushort)(address + 10))),
                    $"Golden Torizo hitbox $AA:{pointer:X4} shot AI");
            }
        }
        Console.WriteLine(
            $"Golden Torizo awakening: {mechanicsAddresses.Count} control words, 21 selectors, " +
            "seven physical frames and nine hitbox lists match the pinned cartridge.");

        ushort ReadWord(ushort address) =>
            (ushort)(rom.ReadByte((bank << 16) | address) |
                rom.ReadByte((bank << 16) |
                    unchecked((ushort)(address + 1))) << 8);
    }
}
