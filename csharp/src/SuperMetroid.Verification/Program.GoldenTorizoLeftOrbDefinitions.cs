using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Checks the left-orb control words and visual selectors against the pinned cartridge,
    /// then validates the selected collision frames and their physical hitbox lists.
    /// </summary>
    /// <param name="rom">Pinned retail ROM address space supplying the bank-$AA words.</param>
    private static void VerifyGoldenTorizoLeftOrbDefinitions(ISnesAddressSpace rom)
    {
        const byte bank = TorizoCollisionDefinitions.Bank;
        var controls = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoLeftOrbInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord word =
                GoldenTorizoLeftOrbInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertTrue(controls.Add(word.Address),
                $"Golden Torizo left-orb control $AA:{word.Address:X4} is unique");
            AssertTrue(word.Address >= GoldenTorizoLeftOrbInstructionProgramDefinitions.Start &&
                       word.Address < GoldenTorizoLeftOrbInstructionProgramDefinitions.End,
                $"Golden Torizo left-orb control $AA:{word.Address:X4} is bounded");
            AssertEqual(ReadWord(word.Address), word.Value,
                $"Golden Torizo left-orb control $AA:{word.Address:X4}");
            AssertTrue(GoldenTorizoLeftOrbInstructionProgramDefinitions
                    .TryReadMechanicsWord(word.Address, out ushort selected) &&
                       selected == word.Value,
                $"Golden Torizo left-orb control $AA:{word.Address:X4} lookup");
            AssertTrue(GoldenTorizoLeftOrbInstructionProgramDefinitionsTooling
                    .IsCompiledMechanicsByte((bank << 16) | word.Address) &&
                       GoldenTorizoLeftOrbInstructionProgramDefinitionsTooling
                    .IsCompiledMechanicsByte((bank << 16) |
                        unchecked((ushort)(word.Address + 1))),
                $"Golden Torizo left-orb control $AA:{word.Address:X4} owns both bytes");
        }
        AssertEqual(46, controls.Count,
            "two Golden Torizo left-orb lists contain 46 control words");

        var selectedFrames = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoLeftOrbInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = GoldenTorizoLeftOrbInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = ReadWord(operand);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(bank, operand,
                    out ushort compiled) && compiled == native,
                $"Golden Torizo left-orb selector $AA:{operand:X4}");
            AssertTrue(!controls.Contains(operand),
                $"Golden Torizo left-orb selector $AA:{operand:X4} is not mechanics");
            selectedFrames.Add(native);
        }
        AssertEqual(20, GoldenTorizoLeftOrbInstructionProgramDefinitions.PresentationWordCount,
            "Golden Torizo left-orb visual occurrence count");

        var hitboxLists = new HashSet<ushort>();
        foreach (ushort frame in selectedFrames.Order())
        {
            AssertTrue(selectedFrames.Contains(frame),
                $"Golden Torizo left-orb frame $AA:{frame:X4} is selected");
            AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                    frame, out TorizoCollisionComponents components),
                $"Golden Torizo left-orb frame $AA:{frame:X4} is compiled");
            AssertEqual((ushort)components.Length, ReadWord(frame),
                $"Golden Torizo left-orb frame $AA:{frame:X4} component count");
            for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                GoldenTorizoCollisionComponent component = components[componentIndex];
                ushort address = unchecked((ushort)(frame + 2 + componentIndex * 8));
                AssertEqual(unchecked((ushort)component.X), ReadWord(address),
                    $"Golden Torizo left-orb frame $AA:{frame:X4} X {componentIndex}");
                AssertEqual(unchecked((ushort)component.Y),
                    ReadWord(unchecked((ushort)(address + 2))),
                    $"Golden Torizo left-orb frame $AA:{frame:X4} Y {componentIndex}");
                AssertEqual(component.HitboxList,
                    ReadWord(unchecked((ushort)(address + 6))),
                    $"Golden Torizo left-orb frame $AA:{frame:X4} hitbox {componentIndex}");
                hitboxLists.Add(component.HitboxList);
            }
        }
        AssertEqual(8, hitboxLists.Count,
            "Golden Torizo left-orb references eight physical hitbox lists");
        foreach (ushort pointer in hitboxLists)
        {
            ReadOnlySpan<GoldenTorizoCollisionHitbox> hitboxes =
                TorizoCollisionDefinitions.HitboxesAt(pointer);
            AssertEqual((ushort)hitboxes.Length, ReadWord(pointer),
                $"Golden Torizo left-orb hitbox list $AA:{pointer:X4} count");
            for (int index = 0; index < hitboxes.Length; index++)
            {
                GoldenTorizoCollisionHitbox hitbox = hitboxes[index];
                ushort address = unchecked((ushort)(pointer + 2 + index * 12));
                AssertEqual(unchecked((ushort)hitbox.Left), ReadWord(address),
                    $"Golden Torizo left-orb hitbox $AA:{pointer:X4} left");
                AssertEqual(unchecked((ushort)hitbox.Top),
                    ReadWord(unchecked((ushort)(address + 2))),
                    $"Golden Torizo left-orb hitbox $AA:{pointer:X4} top");
                AssertEqual(unchecked((ushort)hitbox.Right),
                    ReadWord(unchecked((ushort)(address + 4))),
                    $"Golden Torizo left-orb hitbox $AA:{pointer:X4} right");
                AssertEqual(unchecked((ushort)hitbox.Bottom),
                    ReadWord(unchecked((ushort)(address + 6))),
                    $"Golden Torizo left-orb hitbox $AA:{pointer:X4} bottom");
                AssertEqual(hitbox.TouchAi,
                    ReadWord(unchecked((ushort)(address + 8))),
                    $"Golden Torizo left-orb hitbox $AA:{pointer:X4} touch AI");
                AssertEqual(hitbox.ShotAi,
                    ReadWord(unchecked((ushort)(address + 10))),
                    $"Golden Torizo left-orb hitbox $AA:{pointer:X4} shot AI");
            }
        }
        Console.WriteLine(
            "Golden Torizo left-orb attacks: 46 control words, 20 selectors, " +
            "12 physical frames and eight hitbox lists match the pinned cartridge.");

        ushort ReadWord(ushort address) =>
            (ushort)(rom.ReadByte((bank << 16) | address) |
                rom.ReadByte((bank << 16) |
                    unchecked((ushort)(address + 1))) << 8);
    }
}
