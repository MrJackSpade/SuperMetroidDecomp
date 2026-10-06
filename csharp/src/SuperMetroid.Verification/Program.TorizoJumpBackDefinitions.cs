using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyTorizoJumpBackDefinitions(ISnesAddressSpace rom)
    {
        const byte bank = TorizoCollisionDefinitions.Bank;
        var mechanicsAddresses = new HashSet<ushort>();
        for (int index = 0;
             index < TorizoJumpBackInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            TorizoJumpBackMechanicsWord word =
                TorizoJumpBackInstructionProgramDefinitions.MechanicsWord(index);
            AssertTrue(mechanicsAddresses.Add(word.Address),
                $"Torizo jump-back control $AA:{word.Address:X4} is unique");
            AssertTrue(word.Address >= TorizoJumpBackInstructionProgramDefinitions.Start &&
                       word.Address < TorizoJumpBackInstructionProgramDefinitions.End,
                $"Torizo jump-back control $AA:{word.Address:X4} is bounded");
            AssertEqual(ReadWord(word.Address), word.Value,
                $"Torizo jump-back control $AA:{word.Address:X4}");
            AssertTrue(TorizoJumpBackInstructionProgramDefinitions
                    .TryReadMechanicsWord(word.Address, out ushort selected) &&
                       selected == word.Value,
                $"Torizo jump-back control $AA:{word.Address:X4} lookup");
            AssertTrue(TorizoJumpBackInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) | word.Address) &&
                       TorizoJumpBackInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) |
                        unchecked((ushort)(word.Address + 1))),
                $"Torizo jump-back control $AA:{word.Address:X4} owns both bytes");
        }
        AssertEqual(52, mechanicsAddresses.Count,
            "Torizo paired jump-back control-word count");

        var selectedFrames = new HashSet<ushort>();
        for (int index = 0;
             index < TorizoJumpBackInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = TorizoJumpBackInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = ReadWord(address);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(bank, address,
                    out ushort compiled) && compiled == native,
                $"Torizo jump-back selector $AA:{address:X4}");
            AssertTrue(!mechanicsAddresses.Contains(address),
                $"Torizo jump-back selector $AA:{address:X4} is not mechanics");
            selectedFrames.Add(native);
        }
        AssertEqual(8, TorizoJumpBackInstructionProgramDefinitions.PresentationWordCount,
            "Torizo paired jump-back visual occurrence count");
        AssertEqual(3, selectedFrames.Count,
            "Torizo paired jump-back selects three distinct frames");

        var hitboxLists = new HashSet<ushort>();
        foreach (ushort frame in selectedFrames.Order())
        {
            AssertTrue(selectedFrames.Contains(frame),
                $"Torizo jump-back frame $AA:{frame:X4} is selected by its program");
            AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                    frame, out TorizoCollisionComponents components),
                $"Torizo jump-back frame $AA:{frame:X4} is compiled");
            AssertEqual((ushort)components.Length, ReadWord(frame),
                $"Torizo jump-back frame $AA:{frame:X4} component count");
            for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                GoldenTorizoCollisionComponent component = components[componentIndex];
                ushort address = unchecked((ushort)(frame + 2 + componentIndex * 8));
                AssertEqual(unchecked((ushort)component.X), ReadWord(address),
                    $"Torizo jump-back frame $AA:{frame:X4} component X {componentIndex}");
                AssertEqual(unchecked((ushort)component.Y),
                    ReadWord(unchecked((ushort)(address + 2))),
                    $"Torizo jump-back frame $AA:{frame:X4} component Y {componentIndex}");
                AssertEqual(component.HitboxList,
                    ReadWord(unchecked((ushort)(address + 6))),
                    $"Torizo jump-back frame $AA:{frame:X4} hitbox list {componentIndex}");
                hitboxLists.Add(component.HitboxList);
            }
        }
        AssertEqual(4, hitboxLists.Count,
            "Torizo paired jump-back references four physical hitbox lists");
        foreach (ushort pointer in hitboxLists)
        {
            ReadOnlySpan<GoldenTorizoCollisionHitbox> hitboxes =
                TorizoCollisionDefinitions.HitboxesAt(pointer);
            AssertEqual((ushort)hitboxes.Length, ReadWord(pointer),
                $"Torizo jump-back hitbox list $AA:{pointer:X4} count");
            for (int index = 0; index < hitboxes.Length; index++)
            {
                GoldenTorizoCollisionHitbox hitbox = hitboxes[index];
                ushort address = unchecked((ushort)(pointer + 2 + index * 12));
                AssertEqual(unchecked((ushort)hitbox.Left), ReadWord(address),
                    $"Torizo jump-back hitbox $AA:{pointer:X4} left");
                AssertEqual(unchecked((ushort)hitbox.Top),
                    ReadWord(unchecked((ushort)(address + 2))),
                    $"Torizo jump-back hitbox $AA:{pointer:X4} top");
                AssertEqual(unchecked((ushort)hitbox.Right),
                    ReadWord(unchecked((ushort)(address + 4))),
                    $"Torizo jump-back hitbox $AA:{pointer:X4} right");
                AssertEqual(unchecked((ushort)hitbox.Bottom),
                    ReadWord(unchecked((ushort)(address + 6))),
                    $"Torizo jump-back hitbox $AA:{pointer:X4} bottom");
                AssertEqual(hitbox.TouchAi,
                    ReadWord(unchecked((ushort)(address + 8))),
                    $"Torizo jump-back hitbox $AA:{pointer:X4} touch AI");
                AssertEqual(hitbox.ShotAi,
                    ReadWord(unchecked((ushort)(address + 10))),
                    $"Torizo jump-back hitbox $AA:{pointer:X4} shot AI");
            }
        }
        Console.WriteLine(
            "Paired Torizo jump-back lists: 52 control words, eight visual " +
            "occurrences, three physical frames and four hitbox lists match " +
            "the pinned cartridge.");

        VerifyTorizoJumpBackLeftDefinitions(rom);

        ushort ReadWord(ushort address) =>
            (ushort)(rom.ReadByte((bank << 16) | address) |
                rom.ReadByte((bank << 16) |
                    unchecked((ushort)(address + 1))) << 8);
    }

    private static void VerifyTorizoJumpBackLeftDefinitions(ISnesAddressSpace rom)
    {
        const byte bank = TorizoCollisionDefinitions.Bank;
        var mechanicsAddresses = new HashSet<ushort>();
        for (int index = 0;
             index < TorizoJumpBackLeftInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            TorizoJumpBackMechanicsWord word =
                TorizoJumpBackLeftInstructionProgramDefinitions.MechanicsWord(index);
            AssertTrue(mechanicsAddresses.Add(word.Address),
                $"left-facing Torizo jump-back control $AA:{word.Address:X4} is unique");
            AssertTrue(word.Address >= TorizoJumpBackLeftInstructionProgramDefinitions.Start &&
                       word.Address < TorizoJumpBackLeftInstructionProgramDefinitions.End,
                $"left-facing Torizo jump-back control $AA:{word.Address:X4} is bounded");
            AssertEqual(ReadWord(word.Address), word.Value,
                $"left-facing Torizo jump-back control $AA:{word.Address:X4}");
            AssertTrue(TorizoJumpBackLeftInstructionProgramDefinitions
                    .TryReadMechanicsWord(word.Address, out ushort selected) &&
                       selected == word.Value,
                $"left-facing Torizo jump-back control $AA:{word.Address:X4} lookup");
            AssertTrue(TorizoJumpBackLeftInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) | word.Address) &&
                       TorizoJumpBackLeftInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) |
                        unchecked((ushort)(word.Address + 1))),
                $"left-facing Torizo jump-back control $AA:{word.Address:X4} owns both bytes");
        }
        AssertEqual(52, mechanicsAddresses.Count,
            "left-facing Torizo paired jump-back control-word count");

        var frames = new HashSet<ushort>();
        for (int index = 0;
             index < TorizoJumpBackLeftInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = TorizoJumpBackLeftInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = ReadWord(address);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(bank, address,
                    out ushort compiled) && compiled == native,
                $"left-facing Torizo jump-back selector $AA:{address:X4}");
            AssertTrue(!mechanicsAddresses.Contains(address),
                $"left-facing Torizo jump-back selector $AA:{address:X4} is not mechanics");
            frames.Add(native);
        }
        AssertEqual(8, TorizoJumpBackLeftInstructionProgramDefinitions.PresentationWordCount,
            "left-facing Torizo paired jump-back visual occurrence count");
        AssertEqual(3, frames.Count,
            "left-facing Torizo paired jump-back selects three distinct frames");
        var hitboxLists = new HashSet<ushort>();
        foreach (ushort frame in frames.Order())
        {
            AssertTrue(frames.Contains(frame),
                $"left-facing Torizo jump-back frame $AA:{frame:X4} is selected");
            AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                    frame, out TorizoCollisionComponents components),
                $"left-facing Torizo jump-back frame $AA:{frame:X4} is compiled");
            AssertEqual((ushort)components.Length, ReadWord(frame),
                $"left-facing Torizo jump-back frame $AA:{frame:X4} component count");
            for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                GoldenTorizoCollisionComponent component = components[componentIndex];
                ushort address = unchecked((ushort)(frame + 2 + componentIndex * 8));
                AssertEqual(unchecked((ushort)component.X), ReadWord(address),
                    $"left-facing Torizo jump-back frame $AA:{frame:X4} X {componentIndex}");
                AssertEqual(unchecked((ushort)component.Y),
                    ReadWord(unchecked((ushort)(address + 2))),
                    $"left-facing Torizo jump-back frame $AA:{frame:X4} Y {componentIndex}");
                AssertEqual(component.HitboxList,
                    ReadWord(unchecked((ushort)(address + 6))),
                    $"left-facing Torizo jump-back frame $AA:{frame:X4} hitbox {componentIndex}");
                hitboxLists.Add(component.HitboxList);
            }
        }
        AssertEqual(4, hitboxLists.Count,
            "left-facing Torizo jump-back references four physical hitbox lists");
        foreach (ushort pointer in hitboxLists)
        {
            ReadOnlySpan<GoldenTorizoCollisionHitbox> hitboxes =
                TorizoCollisionDefinitions.HitboxesAt(pointer);
            AssertEqual((ushort)hitboxes.Length, ReadWord(pointer),
                $"left-facing Torizo jump-back hitbox list $AA:{pointer:X4} count");
            for (int index = 0; index < hitboxes.Length; index++)
            {
                GoldenTorizoCollisionHitbox hitbox = hitboxes[index];
                ushort address = unchecked((ushort)(pointer + 2 + index * 12));
                AssertEqual(unchecked((ushort)hitbox.Left), ReadWord(address),
                    $"left-facing Torizo jump-back hitbox $AA:{pointer:X4} left");
                AssertEqual(unchecked((ushort)hitbox.Top),
                    ReadWord(unchecked((ushort)(address + 2))),
                    $"left-facing Torizo jump-back hitbox $AA:{pointer:X4} top");
                AssertEqual(unchecked((ushort)hitbox.Right),
                    ReadWord(unchecked((ushort)(address + 4))),
                    $"left-facing Torizo jump-back hitbox $AA:{pointer:X4} right");
                AssertEqual(unchecked((ushort)hitbox.Bottom),
                    ReadWord(unchecked((ushort)(address + 6))),
                    $"left-facing Torizo jump-back hitbox $AA:{pointer:X4} bottom");
                AssertEqual(hitbox.TouchAi,
                    ReadWord(unchecked((ushort)(address + 8))),
                    $"left-facing Torizo jump-back hitbox $AA:{pointer:X4} touch AI");
                AssertEqual(hitbox.ShotAi,
                    ReadWord(unchecked((ushort)(address + 10))),
                    $"left-facing Torizo jump-back hitbox $AA:{pointer:X4} shot AI");
            }
        }
        Console.WriteLine(
            "Left-facing Torizo jump-back lists: 52 control words, eight visual " +
            "selectors, three physical frames and four hitbox lists match " +
            "the pinned cartridge.");

        ushort ReadWord(ushort address) =>
            (ushort)(rom.ReadByte((bank << 16) | address) |
                rom.ReadByte((bank << 16) |
                    unchecked((ushort)(address + 1))) << 8);
    }
}
