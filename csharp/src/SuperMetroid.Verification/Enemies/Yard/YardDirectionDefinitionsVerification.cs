using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Enemies.Yard;

/// <summary>Checks Yard direction tables and airborne transitions against the cartridge definitions and guarded runtime paths.</summary>
internal static class YardDirectionDefinitionsVerification
{
    /// <summary>Compares every direction and airborne-list word with ROM, then exercises initialization, turning, and launch paths without table reads.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Yard direction definitions")]
    internal static void VerifyYardDirectionDefinitions(SuperMetroidAddressSpace rom)
    {
        const int directionAddress = 0xa3cd42;
        const int oppositeAddress = 0xa3cdc2;
        const int movementAddress = 0xa3cdd2;
        int[] airborneAddresses = [0xa3d1ab, 0xa3d50f, 0xa3d5a4];

        for (ushort direction = 0; direction < 8; direction++)
        {
            YardDirectionDefinition definition =
                YardDirectionDefinitions.ForDirection(direction);
            int record = directionAddress + direction * 8;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(record), NativeEnemyProgramOracle.EntryOf(YardNativeListAddresses.Entries, definition.CrawlingProgram),
                $"Yard direction {direction} crawling list");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 2)), (ushort)definition.PropertyBits,
                $"Yard direction {direction} property bits");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 4)), NativeEnemyProgramOracle.EntryOf(YardNativeListAddresses.Entries, definition.HidingProgram),
                $"Yard direction {direction} hiding list");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 6)), definition.AirborneFacingDirection,
                $"Yard direction {direction} airborne facing");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((oppositeAddress + direction * 2)),
                definition.OppositeDirection,
                $"Yard direction {direction} opposite");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((movementAddress + direction * 2)),
                (ushort)definition.MovementFunction,
                $"Yard direction {direction} movement function");
        }

        foreach (int source in airborneAddresses)
        for (ushort facing = 0; facing < 2; facing++)
        {
            YardAirborneInstructionDefinition definition =
                YardDirectionDefinitions.ForAirborneFacing(facing);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((source + facing * 4)),
                NativeEnemyProgramOracle.EntryOf(YardNativeListAddresses.Entries, definition.VisibleProgram),
                $"Yard airborne ${source:X6} facing {facing} visible list");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((source + facing * 4 + 2)),
                NativeEnemyProgramOracle.EntryOf(YardNativeListAddresses.Entries, definition.HidingProgram),
                $"Yard airborne ${source:X6} facing {facing} hiding list");
        }

        AssertThrows<InvalidDataException>(
            () => YardDirectionDefinitions.ForDirection(8),
            "Yard invalid direction definition");
        AssertThrows<InvalidDataException>(
            () => YardDirectionDefinitions.ForAirborneFacing(2),
            "Yard invalid airborne facing definition");

        var guarded = NewYardDirectionReadGuard(rom);

        for (ushort direction = 0; direction < 8; direction++)
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.CurrentInstruction = direction;
            slot.SetSpawnParameter1(0);
            slot.Properties = EnemyPropertyWord.FromNativeWord(0x4000);
            slot.Bind(EnemyDefinitionId.Yard, slot.Definition);
            YardEnemyBehavior.InitializeYard(slot);

            YardEnemyState state = enemies.YardStates[0] ??
                throw new InvalidDataException("Yard initializer produced no typed state.");
            YardDirectionDefinition initial =
                YardDirectionDefinitions.ForDirection(direction);
            AssertYardDirectionState(slot, state, initial,
                $"Yard initializer direction {direction}",
                assertMovementFunction: false);
            AssertEqual(YardMovementFunction.InstructionPending, state.MovementFunction,
                $"Yard initializer direction {direction} pending function");

            // The population selector does not initialize the live direction word;
            // animation bytecode normally publishes it before a turn. Seed each of the
            // eight legal bytecode values to exercise the production turn dispatcher.
            state.Direction = direction;
            state.MovementFunction = initial.MovementFunction;
            bool turned = YardEnemyBehavior.TurnYardAround(slot, state);
            AssertTrue(turned, $"Yard direction {direction} production turn");
            YardDirectionDefinition opposite =
                YardDirectionDefinitions.ForDirection(initial.OppositeDirection);
            AssertEqual(initial.OppositeDirection, state.Direction,
                $"Yard direction {direction} production opposite");
            AssertYardDirectionState(slot, state, opposite,
                $"Yard turn direction {direction}",
                assertMovementFunction: true);
        }

        foreach (ushort direction in new ushort[] { 0, 1 })
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.CurrentInstruction = direction;
            slot.SetSpawnParameter1(0);
            slot.Bind(EnemyDefinitionId.Yard, slot.Definition);
            YardEnemyBehavior.InitializeYard(slot);
            YardEnemyState state = (enemies.YardStates[0] ?? throw new InvalidOperationException("Verification requires enemies.YardStates[0]."));
            int facing = state.AirborneFacingDirection;
            YardAirborneInstructionDefinition expected =
                YardDirectionDefinitions.ForAirborneFacing(facing);

            YardEnemyBehavior.DropYard(slot, state);
            AssertYardAirborneLists(state, expected,
                $"Yard detach facing {facing}");

            state.Behavior = YardBehavior.NormalCrawling;
            state.MovementFunction = YardMovementFunction.CrawlingUpsideUpMovingLeft;
            var samus = new SamusState
            {
                Pose = facing == 0
                    ? SamusPoseId.FacingLeftNormalPose
                    : SamusPoseId.FacingRightNormalPose,
                AbsoluteMovedLastFrameXFixed = 0x00018000,
            };
            YardEnemyBehavior.KickYardIntoAir(slot, state, samus);
            AssertYardAirborneLists(state, expected,
                $"Yard contact kick facing {facing}");

            YardEnemyBehavior.ShootYardIntoAir(slot, state, samus);
            AssertYardAirborneLists(state, expected,
                $"Yard shot launch facing {facing}");
        }

        Console.WriteLine(
            "Yard direction definitions: 48 direction words and all 24 duplicated airborne-list words match ROM; every real initializer, turn, detach, contact-kick and shot-launch handoff passes with the fixed tables forbidden.");
    }

    /// <summary>Checks the instruction, property, hiding, facing, and optionally movement values installed for a Yard direction.</summary>
    private static void AssertYardDirectionState(
        RoomEnemySlot slot,
        YardEnemyState state,
        YardDirectionDefinition expected,
        string name,
        bool assertMovementFunction)
    {
        AssertEqual(AnimationProgramCursor<YardProgram>.Start(expected.CrawlingProgram), state.Program,
            $"{name} crawling list");
        AssertEqual(expected.PropertyBits, (byte)(slot.Properties.FamilyData & 3),
            $"{name} property bits");
        AssertEqual((YardProgram?)expected.HidingProgram, state.HidingProgram,
            $"{name} hiding list");
        AssertEqual(expected.AirborneFacingDirection, state.AirborneFacingDirection,
            $"{name} airborne facing");
        if (assertMovementFunction)
        {
            AssertEqual(expected.MovementFunction, state.MovementFunction,
                $"{name} movement function");
        }
    }

    /// <summary>Checks the visible and hiding lists selected when a Yard becomes airborne.</summary>
    private static void AssertYardAirborneLists(
        YardEnemyState state,
        YardAirborneInstructionDefinition expected,
        string name)
    {
        AssertEqual(AnimationProgramCursor<YardProgram>.Start(expected.VisibleProgram), state.Program,
            $"{name} visible list");
        AssertEqual((YardProgram?)expected.HidingProgram, state.HidingProgram,
            $"{name} hiding list");
    }

    /// <summary>Rejects reads from the Yard direction and airborne-list ranges that the migrated definitions replace.</summary>
    private static CartridgeReadGuard NewYardDirectionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0xa3cd42 and < 0xa3cd82 or
                >= 0xa3cdc2 and < 0xa3cde2 or
                >= 0xa3d1ab and < 0xa3d1b3 or
                >= 0xa3d50f and < 0xa3d517 or
                >= 0xa3d5a4 and < 0xa3d5ac,
            address => $"Yard attempted migrated direction/list read ${address:X6}.");
}
