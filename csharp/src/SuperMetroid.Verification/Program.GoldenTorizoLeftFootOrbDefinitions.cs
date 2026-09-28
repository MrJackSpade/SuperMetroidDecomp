using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyGoldenTorizoLeftFootOrbDefinitions(ISnesAddressSpace rom)
    {
        const byte bank = GoldenTorizoLeftFootOrbCollisionDefinitions.Bank;
        var mechanicsAddresses = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoLeftFootOrbInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            GoldenTorizoLeftFootOrbMechanicsWord word =
                GoldenTorizoLeftFootOrbInstructionProgramDefinitions.MechanicsWord(index);
            AssertTrue(mechanicsAddresses.Add(word.Address),
                $"Golden Torizo left-foot orb control $AA:{word.Address:X4} is unique");
            AssertTrue(word.Address >= GoldenTorizoLeftFootOrbInstructionProgramDefinitions.Start &&
                       word.Address < GoldenTorizoLeftFootOrbInstructionProgramDefinitions.End,
                $"Golden Torizo left-foot orb control $AA:{word.Address:X4} is bounded");
            AssertEqual(ReadWord(word.Address), word.Value,
                $"Golden Torizo left-foot orb control $AA:{word.Address:X4}");
            AssertTrue(GoldenTorizoLeftFootOrbInstructionProgramDefinitions
                    .TryReadMechanicsWord(word.Address, out ushort selected) &&
                       selected == word.Value,
                $"Golden Torizo left-foot orb control $AA:{word.Address:X4} lookup");
            AssertTrue(GoldenTorizoLeftFootOrbInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) | word.Address) &&
                       GoldenTorizoLeftFootOrbInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) |
                        unchecked((ushort)(word.Address + 1))),
                $"Golden Torizo left-foot orb control $AA:{word.Address:X4} owns both bytes");
        }
        AssertEqual(23, mechanicsAddresses.Count,
            "Golden Torizo left-foot orb control-word count");

        var selectedFrames = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoLeftFootOrbInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = GoldenTorizoLeftFootOrbInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = ReadWord(address);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(bank, address,
                    out ushort compiled) && compiled == native,
                $"Golden Torizo left-foot orb selector $AA:{address:X4}");
            AssertTrue(!mechanicsAddresses.Contains(address),
                $"Golden Torizo left-foot orb selector $AA:{address:X4} is not mechanics");
            selectedFrames.Add(native);
        }
        AssertEqual(10, GoldenTorizoLeftFootOrbInstructionProgramDefinitions.PresentationWordCount,
            "Golden Torizo left-foot orb visual occurrence count");
        AssertEqual(6, selectedFrames.Count,
            "Golden Torizo left-foot orb selects six distinct frames");
        AssertTrue(selectedFrames.Contains(0xabec) &&
                   GoldenTorizoRightSonicCollisionDefinitions.HasFrame(0xabec),
            "left-foot orb reuses the existing ABEC sonic frame");
        AssertEqual(selectedFrames.Count - 1,
            GoldenTorizoLeftFootOrbCollisionDefinitions.FrameCount,
            "Golden Torizo left-foot orb has five new physical frames");

        var hitboxLists = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoLeftFootOrbCollisionDefinitions.FrameCount;
             index++)
        {
            ushort frame = GoldenTorizoLeftFootOrbCollisionDefinitions.FramePointer(index);
            AssertTrue(selectedFrames.Contains(frame),
                $"Golden Torizo left-foot orb frame $AA:{frame:X4} is selected");
            AssertTrue(GoldenTorizoLeftFootOrbCollisionDefinitions.TryGetComponents(
                    frame, out ReadOnlyMemory<GoldenTorizoCollisionComponent> components),
                $"Golden Torizo left-foot orb frame $AA:{frame:X4} is compiled");
            AssertEqual((ushort)components.Length, ReadWord(frame),
                $"Golden Torizo left-foot orb frame $AA:{frame:X4} component count");
            for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                GoldenTorizoCollisionComponent component = components.Span[componentIndex];
                ushort address = unchecked((ushort)(frame + 2 + componentIndex * 8));
                AssertEqual(unchecked((ushort)component.X), ReadWord(address),
                    $"Golden Torizo left-foot orb frame $AA:{frame:X4} X {componentIndex}");
                AssertEqual(unchecked((ushort)component.Y),
                    ReadWord(unchecked((ushort)(address + 2))),
                    $"Golden Torizo left-foot orb frame $AA:{frame:X4} Y {componentIndex}");
                AssertEqual(component.HitboxList,
                    ReadWord(unchecked((ushort)(address + 6))),
                    $"Golden Torizo left-foot orb frame $AA:{frame:X4} hitbox {componentIndex}");
                hitboxLists.Add(component.HitboxList);
            }
        }
        AssertEqual(6, hitboxLists.Count,
            "Golden Torizo left-foot orb references six hitbox lists");
        foreach (ushort pointer in hitboxLists)
        {
            ReadOnlySpan<GoldenTorizoCollisionHitbox> hitboxes =
                GoldenTorizoLeftFootOrbCollisionDefinitions.HitboxesAt(pointer);
            AssertEqual((ushort)hitboxes.Length, ReadWord(pointer),
                $"Golden Torizo left-foot orb hitbox $AA:{pointer:X4} count");
            for (int index = 0; index < hitboxes.Length; index++)
            {
                GoldenTorizoCollisionHitbox hitbox = hitboxes[index];
                ushort address = unchecked((ushort)(pointer + 2 + index * 12));
                AssertEqual(unchecked((ushort)hitbox.Left), ReadWord(address),
                    $"Golden Torizo left-foot orb hitbox $AA:{pointer:X4} left");
                AssertEqual(unchecked((ushort)hitbox.Top),
                    ReadWord(unchecked((ushort)(address + 2))),
                    $"Golden Torizo left-foot orb hitbox $AA:{pointer:X4} top");
                AssertEqual(unchecked((ushort)hitbox.Right),
                    ReadWord(unchecked((ushort)(address + 4))),
                    $"Golden Torizo left-foot orb hitbox $AA:{pointer:X4} right");
                AssertEqual(unchecked((ushort)hitbox.Bottom),
                    ReadWord(unchecked((ushort)(address + 6))),
                    $"Golden Torizo left-foot orb hitbox $AA:{pointer:X4} bottom");
                AssertEqual(hitbox.TouchAi,
                    ReadWord(unchecked((ushort)(address + 8))),
                    $"Golden Torizo left-foot orb hitbox $AA:{pointer:X4} touch AI");
                AssertEqual(hitbox.ShotAi,
                    ReadWord(unchecked((ushort)(address + 10))),
                    $"Golden Torizo left-foot orb hitbox $AA:{pointer:X4} shot AI");
            }
        }
        Console.WriteLine(
            "Golden Torizo left-foot orb: 23 control words, ten visual " +
            "selectors, five new physical frames and six hitbox lists match the cartridge.");

        ushort ReadWord(ushort address) =>
            (ushort)(rom.ReadByte((bank << 16) | address) |
                rom.ReadByte((bank << 16) |
                    unchecked((ushort)(address + 1))) << 8);
    }
}
