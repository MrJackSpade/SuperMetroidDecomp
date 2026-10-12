using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Represents torizo falling left definitions verification.</summary>
internal static class TorizoFallingLeftDefinitionsVerification
{
    /// <summary>Verifies torizo falling left definitions.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Torizo falling left definitions")]
    internal static void VerifyTorizoFallingLeftDefinitions(ISnesAddressSpace rom)
    {
        const byte bank = TorizoCollisionDefinitions.Bank;
        var addresses = new HashSet<ushort>();
        for (int index = 0;
             index < TorizoFallingLeftInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord word =
                TorizoFallingLeftInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertTrue(addresses.Add(word.Address),
                $"Torizo falling-left control $AA:{word.Address:X4} is unique");
            AssertTrue(word.Address >= TorizoFallingLeftInstructionProgramDefinitions.Start &&
                       word.Address < TorizoFallingLeftInstructionProgramDefinitions.End,
                $"Torizo falling-left control $AA:{word.Address:X4} is bounded");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, word.Address))), word.Value,
                $"Torizo falling-left control $AA:{word.Address:X4}");
            AssertTrue(TorizoFallingLeftInstructionProgramDefinitions
                    .TryReadMechanicsWord(word.Address, out ushort selected) &&
                       selected == word.Value,
                $"Torizo falling-left control $AA:{word.Address:X4} lookup");
            AssertTrue(TorizoFallingLeftInstructionProgramDefinitionsTooling
                    .IsCompiledMechanicsByte(((int)new SnesAddress(bank, word.Address))) &&
                       TorizoFallingLeftInstructionProgramDefinitionsTooling
                    .IsCompiledMechanicsByte(((int)new SnesAddress(bank, unchecked((ushort)(word.Address + 1))))),
                $"Torizo falling-left control $AA:{word.Address:X4} owns both bytes");
        }
        AssertEqual(14, addresses.Count, "Torizo falling-left control-word count");
        AssertEqual(1, TorizoFallingLeftInstructionProgramDefinitionsTooling.PresentationWordCount,
            "Torizo falling-left visual occurrence count");
        ushort operand = TorizoFallingLeftInstructionProgramDefinitions
            .PresentationWordAddress(0);
        AssertEqual(TorizoFallingLeftInstructionProgramDefinitions.FallingFrame,
            CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, operand))), "Torizo falling-left visual operand");
        AssertTrue(CompiledEnemyVisualSelectors.TryGet(bank, operand,
                out ushort selectedFrame) &&
                   selectedFrame == TorizoFallingLeftInstructionProgramDefinitions.FallingFrame &&
                   !addresses.Contains(operand),
            "Torizo falling-left visual operand is separate from mechanics");

        AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                selectedFrame, out TorizoCollisionComponents components),
            "Torizo falling-left physical frame is compiled");
        AssertEqual((ushort)components.Length, CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, selectedFrame))),
            "Torizo falling-left physical component count");
        foreach (int index in Enumerable.Range(0, components.Length))
        {
            GoldenTorizoCollisionComponent component = components[index];
            ushort address = unchecked((ushort)(selectedFrame + 2 + index * 8));
            AssertEqual(unchecked((ushort)component.Offset.X), CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, address))),
                $"Torizo falling-left component {index} X");
            AssertEqual(unchecked((ushort)component.Offset.Y),
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, unchecked((ushort)(address + 2))))),
                $"Torizo falling-left component {index} Y");
            AssertEqual(component.HitboxList,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, unchecked((ushort)(address + 6))))),
                $"Torizo falling-left component {index} hitbox list");
            ReadOnlySpan<GoldenTorizoCollisionHitbox> hitboxes =
                TorizoCollisionDefinitions.HitboxesAt(component.HitboxList);
            AssertEqual((ushort)hitboxes.Length, CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, component.HitboxList))),
                $"Torizo falling-left hitbox list $AA:{component.HitboxList:X4} count");
            for (int hitboxIndex = 0; hitboxIndex < hitboxes.Length; hitboxIndex++)
            {
                GoldenTorizoCollisionHitbox hitbox = hitboxes[hitboxIndex];
                ushort hitboxAddress = unchecked((ushort)(component.HitboxList + 2 + hitboxIndex * 12));
                AssertEqual(unchecked((ushort)hitbox.Left), CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, hitboxAddress))),
                    "Torizo falling-left hitbox left");
                AssertEqual(unchecked((ushort)hitbox.Top),
                    CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, unchecked((ushort)(hitboxAddress + 2))))),
                    "Torizo falling-left hitbox top");
                AssertEqual(unchecked((ushort)hitbox.Right),
                    CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, unchecked((ushort)(hitboxAddress + 4))))),
                    "Torizo falling-left hitbox right");
                AssertEqual(unchecked((ushort)hitbox.Bottom),
                    CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, unchecked((ushort)(hitboxAddress + 6))))),
                    "Torizo falling-left hitbox bottom");
                AssertEqual(hitbox.TouchAi,
                    CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, unchecked((ushort)(hitboxAddress + 8))))),
                    "Torizo falling-left hitbox touch AI");
                AssertEqual(hitbox.ShotAi,
                    CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, unchecked((ushort)(hitboxAddress + 10))))),
                    "Torizo falling-left hitbox shot AI");
            }
        }
        Console.WriteLine(
            "Torizo falling left: 14 control words, one visual selector and " +
            "one three-component physical frame match the pinned cartridge.");
    }
}
