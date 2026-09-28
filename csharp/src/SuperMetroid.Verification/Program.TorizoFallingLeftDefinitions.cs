using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyTorizoFallingLeftDefinitions(ISnesAddressSpace rom)
    {
        const byte bank = TorizoFallingLeftCollisionDefinitions.Bank;
        var addresses = new HashSet<ushort>();
        for (int index = 0;
             index < TorizoFallingLeftInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            TorizoFallingLeftMechanicsWord word =
                TorizoFallingLeftInstructionProgramDefinitions.MechanicsWord(index);
            AssertTrue(addresses.Add(word.Address),
                $"Torizo falling-left control $AA:{word.Address:X4} is unique");
            AssertTrue(word.Address >= TorizoFallingLeftInstructionProgramDefinitions.Start &&
                       word.Address < TorizoFallingLeftInstructionProgramDefinitions.End,
                $"Torizo falling-left control $AA:{word.Address:X4} is bounded");
            AssertEqual(ReadWord(word.Address), word.Value,
                $"Torizo falling-left control $AA:{word.Address:X4}");
            AssertTrue(TorizoFallingLeftInstructionProgramDefinitions
                    .TryReadMechanicsWord(word.Address, out ushort selected) &&
                       selected == word.Value,
                $"Torizo falling-left control $AA:{word.Address:X4} lookup");
            AssertTrue(TorizoFallingLeftInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) | word.Address) &&
                       TorizoFallingLeftInstructionProgramDefinitions
                    .IsCompiledMechanicsByte((bank << 16) |
                        unchecked((ushort)(word.Address + 1))),
                $"Torizo falling-left control $AA:{word.Address:X4} owns both bytes");
        }
        AssertEqual(14, addresses.Count, "Torizo falling-left control-word count");
        AssertEqual(1, TorizoFallingLeftInstructionProgramDefinitions.PresentationWordCount,
            "Torizo falling-left visual occurrence count");
        ushort operand = TorizoFallingLeftInstructionProgramDefinitions
            .PresentationWordAddress(0);
        AssertEqual(TorizoFallingLeftInstructionProgramDefinitions.FallingFrame,
            ReadWord(operand), "Torizo falling-left visual operand");
        AssertTrue(CompiledEnemyVisualSelectors.TryGet(bank, operand,
                out ushort selectedFrame) &&
                   selectedFrame == TorizoFallingLeftCollisionDefinitions.Frame &&
                   !addresses.Contains(operand),
            "Torizo falling-left visual operand is separate from mechanics");

        AssertTrue(TorizoFallingLeftCollisionDefinitions.TryGetComponents(
                selectedFrame, out ReadOnlyMemory<GoldenTorizoCollisionComponent> components),
            "Torizo falling-left physical frame is compiled");
        AssertEqual((ushort)components.Length, ReadWord(selectedFrame),
            "Torizo falling-left physical component count");
        foreach (int index in Enumerable.Range(0, components.Length))
        {
            GoldenTorizoCollisionComponent component = components.Span[index];
            ushort address = unchecked((ushort)(selectedFrame + 2 + index * 8));
            AssertEqual(unchecked((ushort)component.X), ReadWord(address),
                $"Torizo falling-left component {index} X");
            AssertEqual(unchecked((ushort)component.Y),
                ReadWord(unchecked((ushort)(address + 2))),
                $"Torizo falling-left component {index} Y");
            AssertEqual(component.HitboxList,
                ReadWord(unchecked((ushort)(address + 6))),
                $"Torizo falling-left component {index} hitbox list");
            ReadOnlySpan<GoldenTorizoCollisionHitbox> hitboxes =
                TorizoFallingLeftCollisionDefinitions.HitboxesAt(component.HitboxList);
            AssertEqual((ushort)hitboxes.Length, ReadWord(component.HitboxList),
                $"Torizo falling-left hitbox list $AA:{component.HitboxList:X4} count");
            for (int hitboxIndex = 0; hitboxIndex < hitboxes.Length; hitboxIndex++)
            {
                GoldenTorizoCollisionHitbox hitbox = hitboxes[hitboxIndex];
                ushort hitboxAddress = unchecked((ushort)(component.HitboxList + 2 + hitboxIndex * 12));
                AssertEqual(unchecked((ushort)hitbox.Left), ReadWord(hitboxAddress),
                    "Torizo falling-left hitbox left");
                AssertEqual(unchecked((ushort)hitbox.Top),
                    ReadWord(unchecked((ushort)(hitboxAddress + 2))),
                    "Torizo falling-left hitbox top");
                AssertEqual(unchecked((ushort)hitbox.Right),
                    ReadWord(unchecked((ushort)(hitboxAddress + 4))),
                    "Torizo falling-left hitbox right");
                AssertEqual(unchecked((ushort)hitbox.Bottom),
                    ReadWord(unchecked((ushort)(hitboxAddress + 6))),
                    "Torizo falling-left hitbox bottom");
                AssertEqual(hitbox.TouchAi,
                    ReadWord(unchecked((ushort)(hitboxAddress + 8))),
                    "Torizo falling-left hitbox touch AI");
                AssertEqual(hitbox.ShotAi,
                    ReadWord(unchecked((ushort)(hitboxAddress + 10))),
                    "Torizo falling-left hitbox shot AI");
            }
        }
        Console.WriteLine(
            "Torizo falling left: 14 control words, one visual selector and " +
            "one three-component physical frame match the pinned cartridge.");

        ushort ReadWord(ushort address) =>
            (ushort)(rom.ReadByte((bank << 16) | address) |
                rom.ReadByte((bank << 16) |
                    unchecked((ushort)(address + 1))) << 8);
    }
}
