using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyGoldenTorizoRightSonicDefinitions(ISnesAddressSpace rom)
    {
        const byte bank = TorizoCollisionDefinitions.Bank;
        var mechanicsAddresses = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoRightSonicInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord word =
                GoldenTorizoRightSonicInstructionProgramDefinitions.MechanicsWord(index);
            AssertTrue(mechanicsAddresses.Add(word.Address),
                $"Golden Torizo right-sonic control $AA:{word.Address:X4} is unique");
            AssertTrue(word.Address >= GoldenTorizoRightSonicInstructionProgramDefinitions.Start &&
                       word.Address < GoldenTorizoRightSonicInstructionProgramDefinitions.End,
                $"Golden Torizo right-sonic control $AA:{word.Address:X4} is bounded");
            AssertEqual(ReadWord(word.Address), word.Value,
                $"Golden Torizo right-sonic control $AA:{word.Address:X4}");
            AssertTrue(GoldenTorizoRightSonicInstructionProgramDefinitions
                    .TryReadMechanicsWord(word.Address, out ushort selected) &&
                       selected == word.Value,
                $"Golden Torizo right-sonic control $AA:{word.Address:X4} lookup");
            AssertTrue(GoldenTorizoRightSonicInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) | word.Address) &&
                       GoldenTorizoRightSonicInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) |
                        unchecked((ushort)(word.Address + 1))),
                $"Golden Torizo right-sonic control $AA:{word.Address:X4} owns both bytes");
        }
        AssertEqual(66, mechanicsAddresses.Count,
            "Golden Torizo paired right-sonic control-word count");

        var selectedFrames = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoRightSonicInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = GoldenTorizoRightSonicInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = ReadWord(address);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(bank, address,
                    out ushort compiled) && compiled == native,
                $"Golden Torizo right-sonic selector $AA:{address:X4}");
            AssertTrue(!mechanicsAddresses.Contains(address),
                $"Golden Torizo right-sonic selector $AA:{address:X4} is not mechanics");
            selectedFrames.Add(native);
        }
        AssertEqual(40, GoldenTorizoRightSonicInstructionProgramDefinitions.PresentationWordCount,
            "Golden Torizo paired right-sonic visual occurrence count");
        AssertEqual(22, selectedFrames.Count,
            "Golden Torizo paired right-sonic selects 22 distinct frames");
        AssertTrue(selectedFrames.Contains(0xac88) &&
                   TorizoCollisionDefinitions.HasFrame(0xac88),
            "right-sonic list shares the right-orb frame AC88 without duplicating its owner");

        var hitboxLists = new HashSet<ushort>();
        // 0xac88 is the shared frame checked above; these are this program's own frames.
        foreach (ushort frame in selectedFrames.Order().Where(frame => frame != 0xac88))
        {
            AssertTrue(selectedFrames.Contains(frame),
                $"Golden Torizo right-sonic frame $AA:{frame:X4} is selected");
            AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                    frame, out TorizoCollisionComponents components),
                $"Golden Torizo right-sonic frame $AA:{frame:X4} is compiled");
            AssertEqual((ushort)components.Length, ReadWord(frame),
                $"Golden Torizo right-sonic frame $AA:{frame:X4} component count");
            for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                GoldenTorizoCollisionComponent component = components[componentIndex];
                ushort address = unchecked((ushort)(frame + 2 + componentIndex * 8));
                AssertEqual(unchecked((ushort)component.X), ReadWord(address),
                    $"Golden Torizo right-sonic frame $AA:{frame:X4} X {componentIndex}");
                AssertEqual(unchecked((ushort)component.Y),
                    ReadWord(unchecked((ushort)(address + 2))),
                    $"Golden Torizo right-sonic frame $AA:{frame:X4} Y {componentIndex}");
                AssertEqual(component.HitboxList,
                    ReadWord(unchecked((ushort)(address + 6))),
                    $"Golden Torizo right-sonic frame $AA:{frame:X4} hitbox {componentIndex}");
                hitboxLists.Add(component.HitboxList);
            }
        }
        AssertEqual(14, hitboxLists.Count,
            "Golden Torizo paired right-sonic references 14 hitbox lists");
        foreach (ushort pointer in hitboxLists)
        {
            ReadOnlySpan<GoldenTorizoCollisionHitbox> hitboxes =
                TorizoCollisionDefinitions.HitboxesAt(pointer);
            AssertEqual((ushort)hitboxes.Length, ReadWord(pointer),
                $"Golden Torizo right-sonic hitbox $AA:{pointer:X4} count");
            for (int index = 0; index < hitboxes.Length; index++)
            {
                GoldenTorizoCollisionHitbox hitbox = hitboxes[index];
                ushort address = unchecked((ushort)(pointer + 2 + index * 12));
                AssertEqual(unchecked((ushort)hitbox.Left), ReadWord(address),
                    $"Golden Torizo right-sonic hitbox $AA:{pointer:X4} left");
                AssertEqual(unchecked((ushort)hitbox.Top),
                    ReadWord(unchecked((ushort)(address + 2))),
                    $"Golden Torizo right-sonic hitbox $AA:{pointer:X4} top");
                AssertEqual(unchecked((ushort)hitbox.Right),
                    ReadWord(unchecked((ushort)(address + 4))),
                    $"Golden Torizo right-sonic hitbox $AA:{pointer:X4} right");
                AssertEqual(unchecked((ushort)hitbox.Bottom),
                    ReadWord(unchecked((ushort)(address + 6))),
                    $"Golden Torizo right-sonic hitbox $AA:{pointer:X4} bottom");
                AssertEqual(hitbox.TouchAi,
                    ReadWord(unchecked((ushort)(address + 8))),
                    $"Golden Torizo right-sonic hitbox $AA:{pointer:X4} touch AI");
                AssertEqual(hitbox.ShotAi,
                    ReadWord(unchecked((ushort)(address + 10))),
                    $"Golden Torizo right-sonic hitbox $AA:{pointer:X4} shot AI");
            }
        }
        Console.WriteLine(
            "Golden Torizo paired right-sonic attacks: 66 control words, 40 " +
            "visual selections, 21 new physical frames and 14 hitbox lists " +
            "match the pinned cartridge.");

        ushort ReadWord(ushort address) =>
            (ushort)(rom.ReadByte((bank << 16) | address) |
                rom.ReadByte((bank << 16) |
                    unchecked((ushort)(address + 1))) << 8);
    }
}
