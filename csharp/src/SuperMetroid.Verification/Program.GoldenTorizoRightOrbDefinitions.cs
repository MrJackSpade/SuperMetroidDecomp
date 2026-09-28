using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyGoldenTorizoRightOrbDefinitions(ISnesAddressSpace rom)
    {
        const byte bank = GoldenTorizoRightOrbCollisionDefinitions.Bank;
        var mechanicsAddresses = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoRightOrbInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            GoldenTorizoRightOrbMechanicsWord word =
                GoldenTorizoRightOrbInstructionProgramDefinitions.MechanicsWord(index);
            AssertTrue(mechanicsAddresses.Add(word.Address),
                $"Golden Torizo right-orb control $AA:{word.Address:X4} is unique");
            AssertTrue(word.Address >= GoldenTorizoRightOrbInstructionProgramDefinitions.Start &&
                       word.Address < GoldenTorizoRightOrbInstructionProgramDefinitions.End,
                $"Golden Torizo right-orb control $AA:{word.Address:X4} is bounded");
            AssertEqual(ReadWord(word.Address), word.Value,
                $"Golden Torizo right-orb control $AA:{word.Address:X4}");
            AssertTrue(GoldenTorizoRightOrbInstructionProgramDefinitions
                    .TryReadMechanicsWord(word.Address, out ushort selected) &&
                       selected == word.Value,
                $"Golden Torizo right-orb control $AA:{word.Address:X4} lookup");
            AssertTrue(GoldenTorizoRightOrbInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) | word.Address) &&
                       GoldenTorizoRightOrbInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) |
                        unchecked((ushort)(word.Address + 1))),
                $"Golden Torizo right-orb control $AA:{word.Address:X4} owns both bytes");
        }
        AssertEqual(23, mechanicsAddresses.Count,
            "Golden Torizo right-orb control-word count");

        var selectedFrames = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoRightOrbInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = GoldenTorizoRightOrbInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = ReadWord(address);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(bank, address,
                    out ushort compiled) && compiled == native,
                $"Golden Torizo right-orb selector $AA:{address:X4}");
            AssertTrue(!mechanicsAddresses.Contains(address),
                $"Golden Torizo right-orb selector $AA:{address:X4} is not mechanics");
            selectedFrames.Add(native);
        }
        AssertEqual(10, GoldenTorizoRightOrbInstructionProgramDefinitions.PresentationWordCount,
            "Golden Torizo right-orb visual occurrence count");
        AssertEqual(6, selectedFrames.Count,
            "Golden Torizo right-orb selects six distinct frames");
        AssertEqual(selectedFrames.Count, GoldenTorizoRightOrbCollisionDefinitions.FrameCount,
            "Golden Torizo right-orb compiled physical-frame count");

        var hitboxLists = new HashSet<ushort>();
        for (int index = 0; index < GoldenTorizoRightOrbCollisionDefinitions.FrameCount; index++)
        {
            ushort frame = GoldenTorizoRightOrbCollisionDefinitions.FramePointer(index);
            AssertTrue(selectedFrames.Contains(frame),
                $"Golden Torizo right-orb frame $AA:{frame:X4} is selected by its program");
            AssertTrue(GoldenTorizoRightOrbCollisionDefinitions.TryGetComponents(
                    frame, out ReadOnlyMemory<GoldenTorizoCollisionComponent> components),
                $"Golden Torizo right-orb frame $AA:{frame:X4} is compiled");
            AssertEqual((ushort)components.Length, ReadWord(frame),
                $"Golden Torizo right-orb frame $AA:{frame:X4} component count");
            for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                GoldenTorizoCollisionComponent component = components.Span[componentIndex];
                ushort address = unchecked((ushort)(frame + 2 + componentIndex * 8));
                AssertEqual(unchecked((ushort)component.X), ReadWord(address),
                    $"Golden Torizo right-orb frame $AA:{frame:X4} component X {componentIndex}");
                AssertEqual(unchecked((ushort)component.Y),
                    ReadWord(unchecked((ushort)(address + 2))),
                    $"Golden Torizo right-orb frame $AA:{frame:X4} component Y {componentIndex}");
                AssertEqual(component.HitboxList,
                    ReadWord(unchecked((ushort)(address + 6))),
                    $"Golden Torizo right-orb frame $AA:{frame:X4} hitbox list {componentIndex}");
                hitboxLists.Add(component.HitboxList);
            }
        }
        AssertEqual(7, hitboxLists.Count,
            "Golden Torizo right-orb references seven physical hitbox lists");
        foreach (ushort pointer in hitboxLists)
        {
            ReadOnlySpan<GoldenTorizoCollisionHitbox> hitboxes =
                GoldenTorizoRightOrbCollisionDefinitions.HitboxesAt(pointer);
            AssertEqual((ushort)hitboxes.Length, ReadWord(pointer),
                $"Golden Torizo right-orb hitbox list $AA:{pointer:X4} count");
            for (int index = 0; index < hitboxes.Length; index++)
            {
                GoldenTorizoCollisionHitbox hitbox = hitboxes[index];
                ushort address = unchecked((ushort)(pointer + 2 + index * 12));
                AssertEqual(unchecked((ushort)hitbox.Left), ReadWord(address),
                    $"Golden Torizo right-orb hitbox $AA:{pointer:X4} left");
                AssertEqual(unchecked((ushort)hitbox.Top),
                    ReadWord(unchecked((ushort)(address + 2))),
                    $"Golden Torizo right-orb hitbox $AA:{pointer:X4} top");
                AssertEqual(unchecked((ushort)hitbox.Right),
                    ReadWord(unchecked((ushort)(address + 4))),
                    $"Golden Torizo right-orb hitbox $AA:{pointer:X4} right");
                AssertEqual(unchecked((ushort)hitbox.Bottom),
                    ReadWord(unchecked((ushort)(address + 6))),
                    $"Golden Torizo right-orb hitbox $AA:{pointer:X4} bottom");
                AssertEqual(hitbox.TouchAi,
                    ReadWord(unchecked((ushort)(address + 8))),
                    $"Golden Torizo right-orb hitbox $AA:{pointer:X4} touch AI");
                AssertEqual(hitbox.ShotAi,
                    ReadWord(unchecked((ushort)(address + 10))),
                    $"Golden Torizo right-orb hitbox $AA:{pointer:X4} shot AI");
            }
        }
        Console.WriteLine(
            "Golden Torizo right-orb attack: 23 control words, ten visual " +
            "occurrences, six physical frames and seven hitbox lists match " +
            "the pinned cartridge.");

        ushort ReadWord(ushort address) =>
            (ushort)(rom.ReadByte((bank << 16) | address) |
                rom.ReadByte((bank << 16) |
                    unchecked((ushort)(address + 1))) << 8);
    }
}
