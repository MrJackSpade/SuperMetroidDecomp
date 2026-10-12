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
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class DraygonInstructionProgramDefinitionsVerification
{
    /// <summary>
    /// Proves that Draygon's four physical enemy records share one complete compiled
    /// mechanics owner and retain the cartridge's atomic four-list installation command.
    /// </summary>
    internal static void VerifyDraygonInstructionProgramDefinitions()
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));

        for (int index = 0;
             index < DraygonInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                DraygonInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa50000 | definition.Address)),
                definition.Value,
                $"Draygon mechanics word $A5:{definition.Address:X4}");
        }

        EnemyDefinitionId[] definitions =
        [
            EnemyDefinitionId.DraygonBody,
            EnemyDefinitionId.DraygonEye,
            EnemyDefinitionId.DraygonTail,
            EnemyDefinitionId.DraygonArms,
        ];
        var enemies = new RoomEnemySystem();
        var guard = NewDraygonInstructionReadGuard();
        enemies.BindFixtureBus(guard);
        foreach (EnemyDefinitionId enemyDefinition in definitions)
        {
            var slot = new RoomEnemySlot(0)
            {
                EnemyDefinitionPointer = enemyDefinition,
                Definition = default(RoomEnemyDefinition) with { Bank = 0xa5 },
            };
            for (int index = 0;
                 index < DraygonInstructionProgramDefinitionsTooling.MechanicsWordCount;
                 index++)
            {
                InstructionMechanicsWord word =
                    DraygonInstructionProgramDefinitionsTooling.MechanicsWord(index);
                AssertEqual(
                    word.Value,
                    (ushort)EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(slot, word.Address),
                    $"Draygon definition ${(int)enemyDefinition:X4} owns $A5:{word.Address:X4}");
            }
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "all four Draygon definitions avoid cartridge mechanics reads");

        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        RoomEnemySlot eye = enemies.SlotTable.Slots[1];
        RoomEnemySlot tail = enemies.SlotTable.Slots[2];
        RoomEnemySlot arms = enemies.SlotTable.Slots[3];
        body.EnemyDefinitionPointer = definitions[0];
        eye.EnemyDefinitionPointer = definitions[1];
        tail.EnemyDefinitionPointer = definitions[2];
        arms.EnemyDefinitionPointer = definitions[3];
        var state = new DraygonEnemyState(body)
        {
            Eye = eye,
            Tail = tail,
            Arms = arms,
            RoomLoadingIrqCommand = 0,
        };
        enemies.DraygonBehavior.Draygon = state;
        ushort resetCursor = DraygonInstructionProgramDefinitions.BodyFacingLeftReset;
        AssertTrue(enemies.DraygonBehavior.TryProcessDraygonInstruction(
                body, null, (ushort)DraygonInstruction.Draygon_SetInstList_Body_Eye_Tail_Arms, ref resetCursor),
            "Draygon four-list reset command is translated");
        AssertEqual(DraygonInstructionProgramDefinitions.BodyFacingLeftIdle,
            body.CurrentInstruction, "Draygon reset installs body list");
        AssertEqual(DraygonInstructionProgramDefinitions.EyeFacingLeftIdle,
            eye.CurrentInstruction, "Draygon reset installs eye list");
        AssertEqual(0x99c6, tail.CurrentInstruction,
            "Draygon reset installs internal tail idle list");
        AssertEqual(DraygonInstructionProgramDefinitions.ArmsFacingLeftIdle,
            arms.CurrentInstruction, "Draygon reset installs arms list");
        AssertEqual(0x97c5, resetCursor,
            "Draygon reset consumes all four list operands atomically");

        ushort duplicateIrqCursor = 0x9c8a;
        AssertTrue(enemies.DraygonBehavior.TryProcessDraygonInstruction(
                body, null, (ushort)DraygonInstruction.Draygon_RoomLoadingInterruptCmd_BeginHUDDrawDuplicate,
                ref duplicateIrqCursor),
            "right-facing duplicate HUD IRQ command is translated");
        AssertEqual(0x000c, state.RoomLoadingIrqCommand,
            "duplicate HUD IRQ command publishes the native room-loading command");
        AssertEqual(0x9c8c, duplicateIrqCursor,
            "duplicate HUD IRQ command advances one instruction word");

        AssertThrows<InvalidDataException>(
            () => DraygonInstructionProgramDefinitions.ReadMechanicsWord(
                DraygonInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "Draygon extended-spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => DraygonInstructionProgramDefinitions.ReadMechanicsWord(0x8000),
            "foreign bank-A5 data is outside the compiled Draygon family");

        _ = ProbeDraygonInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeDraygonInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Draygon allocation probe consumes mechanics data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Draygon mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            $"Draygon instruction mechanics: " +
            $"{DraygonInstructionProgramDefinitionsTooling.MechanicsWordCount} compiled words, " +
            $"{DraygonInstructionProgramDefinitionsTooling.PresentationWordCount} live presentation " +
            "operands, four physical owners, atomic list handoff, and both HUD IRQ opcodes pass.");
    }

    private static int ProbeDraygonInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += DraygonInstructionProgramDefinitions.ReadMechanicsWord(
                DraygonInstructionProgramDefinitions.BodyFacingLeftIdle);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewDraygonInstructionReadGuard() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Draygon mechanics attempted cartridge read ${address:X6}.")
            .RejectWrites(address => $"Draygon mechanics attempted cartridge write ${address:X6}.");

    [VerificationCommand("--draygon-instruction-mechanics")]
    internal static int DraygonInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyDraygonInstructionProgramDefinitions), () => VerifyDraygonInstructionProgramDefinitions());
        return 0;
    }
}
