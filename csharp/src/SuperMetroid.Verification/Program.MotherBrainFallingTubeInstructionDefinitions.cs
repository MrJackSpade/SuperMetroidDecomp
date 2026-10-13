using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

/// <summary>Checks compiled Mother Brain falling-tube frame lists and ordinary-enemy execution.</summary>
internal static class MotherBrainFallingTubeInstructionDefinitionsVerification
{
    /// <summary>Compares five tube programs to native words, then confirms each shows its own composition and sleeps at its terminal opcode.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Mother Brain falling-tube instruction words")]
    internal static void VerifyMotherBrainFallingTubeInstructionDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (int index = 0; index < MotherBrainFallingTubeInstructionDefinitionsTooling.ListCount;
            index++)
        {
            ushort start = unchecked((ushort)(
                MotherBrainFallingTubeInstructionDefinitions.FirstList +
                index * MotherBrainFallingTubeInstructionDefinitions.ListStride));
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), 0xa90000 | start),
                MotherBrainFallingTubeInstructionDefinitions.ReadMechanicsWord(start),
                $"Mother Brain falling tube {index} duration");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), 0xa90000 | (start + 2)),
                MotherBrainFallingTubeInstructionDefinitions.ReadVisualSelector(
                    unchecked((ushort)(start + 2))),
                $"Mother Brain falling tube {index} visual identity");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), 0xa90000 | (start + 4)),
                MotherBrainFallingTubeInstructionDefinitions.ReadMechanicsWord(
                    unchecked((ushort)(start + 4))),
                $"Mother Brain falling tube {index} terminal opcode");

            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(NewMotherBrainFallingTubeReadGuard(rom));
            var tube = new RoomEnemySlot(0)
            {
                EnemyDefinitionPointer = EnemyDefinitionId.MotherBrainTubes,
                Definition = default(RoomEnemyDefinition) with { Bank = 0xa9 },
                CurrentInstruction = start,
                InstructionTimer = NativeCountdown.Start(1),
            };
            object?[] args = [tube, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            EnemyInstructionRunner.Process(enemies, args);
            AssertEqual((ushort)(start + 4), tube.CurrentInstruction,
                $"falling tube {index} advances from frame to terminal opcode");
            AssertEqual((ushort)1, tube.InstructionTimer.RawValue,
                $"falling tube {index} retains its native one-frame duration");
            AssertEqual(MotherBrainFallingTubeInstructionDefinitions.ReadVisualSelector(
                    unchecked((ushort)(start + 2))), tube.SpritemapPointer,
                $"falling tube {index} publishes its own selected composition");
            EnemyInstructionRunner.Process(enemies, args);
            AssertEqual((ushort)(start + 4), tube.CurrentInstruction,
                $"falling tube {index} sleeps on the native terminal word");
        }

        AssertThrows<InvalidDataException>(
            () => MotherBrainFallingTubeInstructionDefinitions.ReadMechanicsWord(0x8c6b),
            "tube visual selector is not read as mechanics");
        AssertThrows<InvalidDataException>(
            () => MotherBrainFallingTubeInstructionDefinitions.ReadVisualSelector(0x8c69),
            "tube duration is not read as presentation");
        AssertThrows<InvalidDataException>(
            () => MotherBrainFallingTubeInstructionDefinitions.ReadMechanicsWord(0x8c87),
            "native code after the tube lists is not interpreted as list data");

        Console.WriteLine(
            "Mother Brain falling tubes: five native frame/sleep lists, selected art, " +
            "and guarded ordinary-enemy execution pass.");
    }

    /// <summary>Rejects runtime rereads of the compiled tube list region while allowing other fixture bus access.</summary>
    private static CartridgeReadGuard NewMotherBrainFallingTubeReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa98c69 and <= 0xa98c86,
                address => $"Mother Brain tube reread compiled source ${address:X6}.");
}
