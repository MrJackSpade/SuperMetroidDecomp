using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static ImportedSpritemapsVerification;
using static SuiteTimingVerification;

/// <summary>Represents escape dachora instruction program definitions verification.</summary>
internal static class EscapeDachoraInstructionProgramDefinitionsVerification
{
    /// <summary>Verifies escape dachora instruction program definitions.</summary>
    internal static void VerifyEscapeDachoraInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyEscapeDachoraInstructionProgramDefinitions), () => VerifyEscapeDachoraInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies escape dachora instruction program definitions.</summary>
    internal static void VerifyEscapeDachoraInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < EscapeDachoraInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                EscapeDachoraInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb30000 | definition.Address)),
                $"escape Dachora mechanics word $B3:{definition.Address:X4}");
        }

        var executedOperands = new HashSet<ushort>();
        var guard = NewEscapeDachoraInstructionReadGuard(rom);
        RoomEnemySystem enemies = CreateEscapeDachoraProgramSystem(guard);
        RoomEnemySlot dachora = enemies.SlotTable.Slots[0];

        ushort initialX = dachora.Position.X.Whole;
        dachora.CurrentInstruction =
            EscapeDachoraInstructionProgramDefinitions.RunningAroundLowTide;
        RunForcedEscapeDachoraInstructions(rom, executedOperands, enemies, dachora, 70);
        AssertTrue(dachora.Position.X.Whole != initialX,
            "escape Dachora low-tide pacing runs movement callbacks");

        initialX = dachora.Position.X.Whole;
        dachora.CurrentInstruction =
            EscapeDachoraInstructionProgramDefinitions.RunningAroundHighTide;
        RunForcedEscapeDachoraInstructions(rom, executedOperands, enemies, dachora, 70);
        AssertTrue(dachora.Position.X.Whole != initialX,
            "escape Dachora high-tide pacing runs movement callbacks");

        initialX = dachora.Position.X.Whole;
        dachora.CurrentInstruction =
            EscapeDachoraInstructionProgramDefinitions.RunningForEscape;
        RunForcedEscapeDachoraInstructions(rom, executedOperands, enemies, dachora, 30);
        AssertTrue(unchecked((short)(dachora.Position.X.Whole - initialX)) > 0,
            "escape Dachora departure moves right through its accelerating callbacks");
        AssertTrue(dachora.CurrentInstruction is
                >= EscapeDachoraInstructionProgramDefinitions.RunningForEscapeMaximumSpeed and
                <= EscapeDachoraInstructionProgramDefinitions.RunningForEscapeMaximumSpeed +
                    0x0024,
            "escape Dachora departure reaches its maximum-speed loop");

        AssertEqual(EscapeDachoraInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count,
            "all executed visual selectors match the cartridge");
        for (int index = 0;
             index < EscapeDachoraInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                EscapeDachoraInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"production covers escape-Dachora presentation word $B3:{address:X4}");
            AssertThrows<InvalidDataException>(
                () => EscapeDachoraInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"escape-Dachora spritemap $B3:{address:X4} is rejected as mechanics");
        }
        AssertEqual(0, guard.ObservedWords.Count,
            "compiled visual selectors require no runtime cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled escape-Dachora mechanics byte");
        AssertThrows<InvalidDataException>(
            () => EscapeDachoraInstructionProgramDefinitions.ReadMechanicsWord(
                EscapeDachoraInstructionProgramDefinitions.FirstAdjacentCodeRoutine),
            "adjacent escape-Dachora callback code is rejected as mechanics");

        _ = ProbeEscapeDachoraInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeEscapeDachoraInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "escape-Dachora allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - allocatedBefore,
            "warmed escape-Dachora mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Escape Dachora instruction mechanics: 119 compiled words, low/high-tide " +
            "pacing, accelerating departure and 43 executed selectors match cartridge data with runtime " +
            "ROM reads forbidden.");
    }

    /// <summary>Creates escape dachora program system.</summary>
    internal static RoomEnemySystem CreateEscapeDachoraProgramSystem(
        ISnesAddressSpace bus)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(bus);
        enemies.VerificationServices().HasEvent = (Func<EventNumber, bool>)(_ => false);
        RoomEnemySlot dachora = enemies.SlotTable.Slots[0];
        dachora.EnemyDefinitionPointer = EnemyDefinitionId.DachoraEscape;
        dachora.Definition = default(RoomEnemyDefinition) with { Bank = 0xb3 };
        dachora.Position = dachora.Position.WithWhole(new(0x0100, 0x00c8));
        enemies.EscapeAnimalsBehavior.InitializeEscapeDachora(dachora);
        return enemies;
    }

    /// <summary>Runs forced escape dachora instructions.</summary>
    private static void RunForcedEscapeDachoraInstructions(
        ISnesAddressSpace rom,
        HashSet<ushort> executedOperands,
        RoomEnemySystem enemies,
        RoomEnemySlot dachora,
        int steps)
    {
        object?[] arguments =
            [dachora, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        for (int step = 0; step < steps; step++)
        {
            dachora.InstructionTimer = 1;
            EnemyInstructionRunner.Process(enemies, arguments);
            VerifyExecutedEnemySelector(rom, dachora, executedOperands);
        }
    }

    /// <summary>Probe escape dachora instruction mechanics allocation.</summary>
    private static int ProbeEscapeDachoraInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += EscapeDachoraInstructionProgramDefinitions.ReadMechanicsWord(
                EscapeDachoraInstructionProgramDefinitions.RunningForEscapeMaximumSpeed);
        }
        return checksum;
    }

    /// <summary>New escape dachora instruction read guard.</summary>
    internal static CartridgeReadGuard NewEscapeDachoraInstructionReadGuard(ISnesAddressSpace source)
    {
        var guard = new CartridgeReadGuard(source).Forbid(
            EscapeDachoraInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
            address => $"Production read compiled escape-Dachora mechanics byte ${address:X6}.");
        // Every presentation word whose low or high byte is read is recorded, not just the first.
        guard.Observe(address => (SnesAddressMath.BankBase(address)) == 0xb30000, address =>
        {
            ushort bankAddress = unchecked((ushort)address);
            for (int index = 0; index < EscapeDachoraInstructionProgramDefinitions.PresentationWordCount; index++)
            {
                ushort presentation = EscapeDachoraInstructionProgramDefinitions.PresentationWordAddress(index);
                if (bankAddress == presentation || bankAddress == unchecked((ushort)(presentation + 1)))
                    guard.ObservedWords.Add(presentation);
            }
        });
        return guard;
    }

    /// <summary>Escape dachora instruction mechanics command.</summary>
    [VerificationCommand("--escape-dachora-instruction-mechanics")]
    internal static int EscapeDachoraInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyEscapeDachoraInstructionProgramDefinitions), () => VerifyEscapeDachoraInstructionProgramDefinitions());
        return 0;
    }
}
