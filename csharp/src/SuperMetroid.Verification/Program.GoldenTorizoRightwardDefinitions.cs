using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyGoldenTorizoRightwardDefinitions(ISnesAddressSpace rom)
    {
        const byte bank = GoldenTorizoRightwardCollisionDefinitions.Bank;
        var mechanicsAddresses = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoRightwardInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            GoldenTorizoRightwardMechanicsWord word =
                GoldenTorizoRightwardInstructionProgramDefinitions.MechanicsWord(index);
            AssertTrue(mechanicsAddresses.Add(word.Address),
                $"Golden Torizo rightward control $AA:{word.Address:X4} is unique");
            AssertTrue(word.Address >= GoldenTorizoRightwardInstructionProgramDefinitions.Start &&
                       word.Address < GoldenTorizoRightwardInstructionProgramDefinitions.End,
                $"Golden Torizo rightward control $AA:{word.Address:X4} is bounded");
            AssertEqual(ReadWord(word.Address), word.Value,
                $"Golden Torizo rightward control $AA:{word.Address:X4}");
            AssertTrue(GoldenTorizoRightwardInstructionProgramDefinitions
                    .TryReadMechanicsWord(word.Address, out ushort selected) &&
                       selected == word.Value,
                $"Golden Torizo rightward control $AA:{word.Address:X4} lookup");
            AssertTrue(GoldenTorizoRightwardInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) | word.Address) &&
                       GoldenTorizoRightwardInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) |
                        unchecked((ushort)(word.Address + 1))),
                $"Golden Torizo rightward control $AA:{word.Address:X4} owns both bytes");
        }
        AssertEqual(82, mechanicsAddresses.Count,
            "Golden Torizo rightward control-word count");

        var selectedFrames = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoRightwardInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = GoldenTorizoRightwardInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = ReadWord(address);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(bank, address,
                    out ushort compiled) && compiled == native,
                $"Golden Torizo rightward selector $AA:{address:X4}");
            AssertTrue(!mechanicsAddresses.Contains(address),
                $"Golden Torizo rightward selector $AA:{address:X4} is not mechanics");
            selectedFrames.Add(native);
        }
        AssertEqual(12,
            GoldenTorizoRightwardInstructionProgramDefinitions.PresentationWordCount,
            "Golden Torizo rightward has twelve visual occurrences");
        AssertEqual(11, selectedFrames.Count,
            "Golden Torizo rightward selects eleven distinct frames");
        AssertEqual(selectedFrames.Count,
            GoldenTorizoRightwardCollisionDefinitions.FrameCount,
            "Golden Torizo rightward compiled physical-frame count");

        var hitboxLists = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoRightwardCollisionDefinitions.FrameCount; index++)
        {
            ushort frame = GoldenTorizoRightwardCollisionDefinitions.FramePointer(index);
            AssertTrue(selectedFrames.Contains(frame),
                $"Golden Torizo rightward frame $AA:{frame:X4} selected by the program");
            AssertTrue(GoldenTorizoRightwardCollisionDefinitions.TryGetComponents(
                    frame, out ReadOnlyMemory<GoldenTorizoCollisionComponent> components),
                $"Golden Torizo rightward frame $AA:{frame:X4} is compiled");
            AssertEqual((ushort)components.Length, ReadWord(frame),
                $"Golden Torizo rightward frame $AA:{frame:X4} component count");
            for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                GoldenTorizoCollisionComponent component = components.Span[componentIndex];
                ushort address = unchecked((ushort)(frame + 2 + componentIndex * 8));
                AssertEqual(unchecked((ushort)component.X), ReadWord(address),
                    $"Golden Torizo rightward frame $AA:{frame:X4} component X {componentIndex}");
                AssertEqual(unchecked((ushort)component.Y),
                    ReadWord(unchecked((ushort)(address + 2))),
                    $"Golden Torizo rightward frame $AA:{frame:X4} component Y {componentIndex}");
                AssertEqual(component.HitboxList,
                    ReadWord(unchecked((ushort)(address + 6))),
                    $"Golden Torizo rightward frame $AA:{frame:X4} hitbox {componentIndex}");
                hitboxLists.Add(component.HitboxList);
            }
        }
        AssertEqual(11, hitboxLists.Count,
            "Golden Torizo rightward references eleven physical hitbox lists");
        foreach (ushort pointer in hitboxLists)
        {
            ReadOnlySpan<GoldenTorizoCollisionHitbox> hitboxes =
                GoldenTorizoRightwardCollisionDefinitions.HitboxesAt(pointer);
            AssertEqual((ushort)hitboxes.Length, ReadWord(pointer),
                $"Golden Torizo rightward hitbox list $AA:{pointer:X4} count");
            for (int index = 0; index < hitboxes.Length; index++)
            {
                GoldenTorizoCollisionHitbox hitbox = hitboxes[index];
                ushort address = unchecked((ushort)(pointer + 2 + index * 12));
                AssertEqual(unchecked((ushort)hitbox.Left), ReadWord(address),
                    $"Golden Torizo rightward hitbox $AA:{pointer:X4} left");
                AssertEqual(unchecked((ushort)hitbox.Top),
                    ReadWord(unchecked((ushort)(address + 2))),
                    $"Golden Torizo rightward hitbox $AA:{pointer:X4} top");
                AssertEqual(unchecked((ushort)hitbox.Right),
                    ReadWord(unchecked((ushort)(address + 4))),
                    $"Golden Torizo rightward hitbox $AA:{pointer:X4} right");
                AssertEqual(unchecked((ushort)hitbox.Bottom),
                    ReadWord(unchecked((ushort)(address + 6))),
                    $"Golden Torizo rightward hitbox $AA:{pointer:X4} bottom");
                AssertEqual(hitbox.TouchAi,
                    ReadWord(unchecked((ushort)(address + 8))),
                    $"Golden Torizo rightward hitbox $AA:{pointer:X4} touch AI");
                AssertEqual(hitbox.ShotAi,
                    ReadWord(unchecked((ushort)(address + 10))),
                    $"Golden Torizo rightward hitbox $AA:{pointer:X4} shot AI");
            }
        }
        Console.WriteLine(
            "Golden Torizo turning/walking-right lists: 82 control words, " +
            "twelve selectors, eleven physical frames and eleven hitbox lists " +
            "match the pinned cartridge.");

        ushort ReadWord(ushort address) =>
            (ushort)(rom.ReadByte((bank << 16) | address) |
                rom.ReadByte((bank << 16) |
                    unchecked((ushort)(address + 1))) << 8);
    }
}
