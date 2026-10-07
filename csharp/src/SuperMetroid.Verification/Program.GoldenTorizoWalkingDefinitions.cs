using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;

internal static partial class Program
{
    private static void VerifyGoldenTorizoWalkingDefinitions(ISnesAddressSpace rom)
    {
        const byte bank = TorizoCollisionDefinitions.Bank;
        var mechanicsAddresses = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoWalkingInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord word =
                GoldenTorizoWalkingInstructionProgramDefinitions.MechanicsWord(index);
            AssertTrue(mechanicsAddresses.Add(word.Address),
                $"Golden Torizo walking control $AA:{word.Address:X4} is unique");
            AssertTrue(word.Address >= GoldenTorizoWalkingInstructionProgramDefinitions.Start &&
                       word.Address < GoldenTorizoWalkingInstructionProgramDefinitions.End,
                $"Golden Torizo walking control $AA:{word.Address:X4} is bounded");
            AssertEqual(ReadWord(word.Address), word.Value,
                $"Golden Torizo walking control $AA:{word.Address:X4}");
            AssertTrue(GoldenTorizoWalkingInstructionProgramDefinitions
                    .TryReadMechanicsWord(word.Address, out ushort selected) &&
                       selected == word.Value,
                $"Golden Torizo walking control $AA:{word.Address:X4} lookup");
            AssertTrue(GoldenTorizoWalkingInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) | word.Address) &&
                       GoldenTorizoWalkingInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) |
                        unchecked((ushort)(word.Address + 1))),
                $"Golden Torizo walking control $AA:{word.Address:X4} owns both bytes");
        }
        AssertEqual(70, mechanicsAddresses.Count,
            "Golden Torizo walking linked-list control-word count");

        var selectedFrames = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoWalkingInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = GoldenTorizoWalkingInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = ReadWord(address);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(bank, address,
                    out ushort compiled) && compiled == native,
                $"Golden Torizo walking selector $AA:{address:X4}");
            AssertTrue(!mechanicsAddresses.Contains(address),
                $"Golden Torizo walking selector $AA:{address:X4} is not mechanics");
            selectedFrames.Add(native);
        }
        AssertEqual(10, selectedFrames.Count,
            "Golden Torizo walking selects ten distinct frames");

        var hitboxLists = new HashSet<ushort>();
        foreach (ushort frame in selectedFrames.Order())
        {
            AssertTrue(selectedFrames.Contains(frame),
                $"Golden Torizo walking frame $AA:{frame:X4} selected by the program");
            AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                    frame, out TorizoCollisionComponents components),
                $"Golden Torizo walking frame $AA:{frame:X4} is compiled");
            AssertEqual((ushort)components.Length, ReadWord(frame),
                $"Golden Torizo walking frame $AA:{frame:X4} component count");
            for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                GoldenTorizoCollisionComponent component = components[componentIndex];
                ushort address = unchecked((ushort)(frame + 2 + componentIndex * 8));
                AssertEqual(unchecked((ushort)component.X), ReadWord(address),
                    $"Golden Torizo walking frame $AA:{frame:X4} component X {componentIndex}");
                AssertEqual(unchecked((ushort)component.Y),
                    ReadWord(unchecked((ushort)(address + 2))),
                    $"Golden Torizo walking frame $AA:{frame:X4} component Y {componentIndex}");
                AssertEqual(component.HitboxList,
                    ReadWord(unchecked((ushort)(address + 6))),
                    $"Golden Torizo walking frame $AA:{frame:X4} hitbox {componentIndex}");
                hitboxLists.Add(component.HitboxList);
            }
        }
        AssertEqual(11, hitboxLists.Count,
            "Golden Torizo walking references eleven distinct physical hitbox lists");
        foreach (ushort pointer in hitboxLists)
        {
            ReadOnlySpan<GoldenTorizoCollisionHitbox> hitboxes =
                TorizoCollisionDefinitions.HitboxesAt(pointer);
            AssertEqual((ushort)hitboxes.Length, ReadWord(pointer),
                $"Golden Torizo walking hitbox list $AA:{pointer:X4} count");
            for (int index = 0; index < hitboxes.Length; index++)
            {
                GoldenTorizoCollisionHitbox hitbox = hitboxes[index];
                ushort address = unchecked((ushort)(pointer + 2 + index * 12));
                AssertEqual(unchecked((ushort)hitbox.Left), ReadWord(address),
                    $"Golden Torizo walking hitbox $AA:{pointer:X4} left");
                AssertEqual(unchecked((ushort)hitbox.Top),
                    ReadWord(unchecked((ushort)(address + 2))),
                    $"Golden Torizo walking hitbox $AA:{pointer:X4} top");
                AssertEqual(unchecked((ushort)hitbox.Right),
                    ReadWord(unchecked((ushort)(address + 4))),
                    $"Golden Torizo walking hitbox $AA:{pointer:X4} right");
                AssertEqual(unchecked((ushort)hitbox.Bottom),
                    ReadWord(unchecked((ushort)(address + 6))),
                    $"Golden Torizo walking hitbox $AA:{pointer:X4} bottom");
                AssertEqual(hitbox.TouchAi,
                    ReadWord(unchecked((ushort)(address + 8))),
                    $"Golden Torizo walking hitbox $AA:{pointer:X4} touch AI");
                AssertEqual(hitbox.ShotAi,
                    ReadWord(unchecked((ushort)(address + 10))),
                    $"Golden Torizo walking hitbox $AA:{pointer:X4} shot AI");
            }
        }
        Console.WriteLine(
            "Golden Torizo linked walking-left lists: 70 control words, ten selectors, " +
            "ten physical frames and eleven hitbox lists match the pinned cartridge.");

        ushort ReadWord(ushort address) =>
            (ushort)(rom.ReadByte((bank << 16) | address) |
                rom.ReadByte((bank << 16) |
                    unchecked((ushort)(address + 1))) << 8);
    }
}
