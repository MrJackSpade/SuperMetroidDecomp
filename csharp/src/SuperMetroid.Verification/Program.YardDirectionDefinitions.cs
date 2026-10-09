using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Compares the Yard direction and airborne instruction tables with cartridge data,
    /// then exercises the production paths that consume their migrated definitions.
    /// </summary>
    /// <param name="rom">Address space containing the cartridge tables used as the reference.</param>
    private static void VerifyYardDirectionDefinitions(SuperMetroidAddressSpace rom)
    {
        const int directionAddress = 0xa3cd42;
        const int oppositeAddress = 0xa3cdc2;
        const int movementAddress = 0xa3cdd2;
        int[] airborneAddresses = [0xa3d1ab, 0xa3d50f, 0xa3d5a4];
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;

        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        for (ushort direction = 0; direction < 8; direction++)
        {
            YardDirectionDefinition definition =
                YardDirectionDefinitions.ForDirection(direction);
            int record = directionAddress + direction * 8;
            AssertEqual(ReadWord(rom, record), definition.CrawlingInstructionList,
                $"Yard direction {direction} crawling list");
            AssertEqual(ReadWord(rom, record + 2), definition.PropertyBits,
                $"Yard direction {direction} property bits");
            AssertEqual(ReadWord(rom, record + 4), definition.HidingInstructionList,
                $"Yard direction {direction} hiding list");
            AssertEqual(ReadWord(rom, record + 6), definition.AirborneFacingDirection,
                $"Yard direction {direction} airborne facing");
            AssertEqual(ReadWord(rom, oppositeAddress + direction * 2),
                definition.OppositeDirection,
                $"Yard direction {direction} opposite");
            AssertEqual(ReadWord(rom, movementAddress + direction * 2),
                (ushort)definition.MovementFunction,
                $"Yard direction {direction} movement function");
        }

        foreach (int source in airborneAddresses)
        for (ushort facing = 0; facing < 2; facing++)
        {
            YardAirborneInstructionDefinition definition =
                YardDirectionDefinitions.ForAirborneFacing(facing);
            AssertEqual(ReadWord(rom, source + facing * 4),
                definition.VisibleInstructionList,
                $"Yard airborne ${source:X6} facing {facing} visible list");
            AssertEqual(ReadWord(rom, source + facing * 4 + 2),
                definition.HidingInstructionList,
                $"Yard airborne ${source:X6} facing {facing} hiding list");
        }

        AssertThrows<InvalidDataException>(
            () => YardDirectionDefinitions.ForDirection(8),
            "Yard invalid direction definition");
        AssertThrows<InvalidDataException>(
            () => YardDirectionDefinitions.ForAirborneFacing(2),
            "Yard invalid airborne facing definition");

        var guarded = new YardDirectionReadGuard(rom);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeYard",
            instanceFlags)!;
        MethodInfo turn = typeof(RoomEnemySystem).GetMethod(
            "TurnYardAround",
            staticFlags)!;
        MethodInfo drop = typeof(RoomEnemySystem).GetMethod(
            "DropYard",
            staticFlags)!;
        MethodInfo kick = typeof(RoomEnemySystem).GetMethod(
            "KickYardIntoAir",
            instanceFlags)!;
        MethodInfo shoot = typeof(RoomEnemySystem).GetMethod(
            "ShootYardIntoAir",
            instanceFlags)!;

        for (ushort direction = 0; direction < 8; direction++)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!
                .SetValue(enemies, guarded);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.CurrentInstruction = direction;
            slot.Parameter1 = 0;
            slot.Properties = 0x4000;
            initialize.Invoke(enemies, [slot]);

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
            bool turned = (bool)(turn.Invoke(null, [slot, state]) ?? false);
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
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!
                .SetValue(enemies, guarded);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.CurrentInstruction = direction;
            slot.Parameter1 = 0;
            initialize.Invoke(enemies, [slot]);
            YardEnemyState state = enemies.YardStates[0]!;
            ushort facing = state.AirborneFacingDirection;
            YardAirborneInstructionDefinition expected =
                YardDirectionDefinitions.ForAirborneFacing(facing);

            drop.Invoke(null, [slot, state]);
            AssertYardAirborneLists(slot, state, expected,
                $"Yard detach facing {facing}");

            state.Behavior = 0;
            state.MovementFunction = YardMovementFunction.CrawlingUpsideUpMovingLeft;
            var samus = new SamusState
            {
                Pose = facing == 0
                    ? SamusPoseIds.FacingLeftNormalPose
                    : SamusPoseIds.FacingRightNormalPose,
                AbsoluteMovedLastFrameXFixed = 0x00018000,
            };
            kick.Invoke(enemies, [slot, state, samus]);
            AssertYardAirborneLists(slot, state, expected,
                $"Yard contact kick facing {facing}");

            shoot.Invoke(enemies, [slot, state, samus]);
            AssertYardAirborneLists(slot, state, expected,
                $"Yard shot launch facing {facing}");
        }

        Console.WriteLine(
            "Yard direction definitions: 48 direction words and all 24 duplicated airborne-list words match ROM; every real initializer, turn, detach, contact-kick and shot-launch handoff passes with the fixed tables forbidden.");
    }

    /// <summary>
    /// Checks the slot and typed Yard state fields selected for one direction transition.
    /// </summary>
    /// <param name="slot">Enemy slot whose active instruction list and property bits are checked.</param>
    /// <param name="state">Typed state holding the Yard hiding list, airborne facing, and movement function.</param>
    /// <param name="expected">Definition whose direction-specific values should be installed.</param>
    /// <param name="name">Context included in assertion messages to identify the transition under check.</param>
    /// <param name="assertMovementFunction">Whether the transition is expected to install its movement function.</param>
    private static void AssertYardDirectionState(
        RoomEnemySlot slot,
        YardEnemyState state,
        YardDirectionDefinition expected,
        string name,
        bool assertMovementFunction)
    {
        AssertEqual(expected.CrawlingInstructionList, slot.CurrentInstruction,
            $"{name} crawling list");
        AssertEqual(expected.PropertyBits, (ushort)(slot.Properties & 3),
            $"{name} property bits");
        AssertEqual(expected.HidingInstructionList, state.HidingInstructionList,
            $"{name} hiding list");
        AssertEqual(expected.AirborneFacingDirection, state.AirborneFacingDirection,
            $"{name} airborne facing");
        if (assertMovementFunction)
        {
            AssertEqual(expected.MovementFunction, state.MovementFunction,
                $"{name} movement function");
        }
    }

    /// <summary>
    /// Checks that a detach or launch copied the facing-specific visible and hiding lists.
    /// </summary>
    /// <param name="slot">Enemy slot containing the active visible instruction list.</param>
    /// <param name="state">Typed Yard state containing the selected hiding instruction list.</param>
    /// <param name="expected">Airborne list pair selected by the launch facing.</param>
    /// <param name="name">Context included in assertion messages to identify the handoff under check.</param>
    private static void AssertYardAirborneLists(
        RoomEnemySlot slot,
        YardEnemyState state,
        YardAirborneInstructionDefinition expected,
        string name)
    {
        AssertEqual(expected.VisibleInstructionList, slot.CurrentInstruction,
            $"{name} visible list");
        AssertEqual(expected.HidingInstructionList, state.HidingInstructionList,
            $"{name} hiding list");
    }

    /// <summary>
    /// Forwards cartridge access while failing if gameplay tries to read the migrated Yard tables.
    /// </summary>
    /// <param name="source">Underlying address space used for accesses outside the guarded table ranges.</param>
    private sealed class YardDirectionReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>
        /// Routes an imported cartridge read through the same guard applied to normal bus reads.
        /// </summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The byte at <paramref name="address"/> when it is outside the migrated tables.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>
        /// Rejects reads from the migrated Yard direction and airborne-list tables.
        /// </summary>
        /// <param name="address">Cartridge address to read.</param>
        /// <returns>The byte at <paramref name="address"/> when it is not in a guarded table.</returns>
        /// <exception cref="InvalidOperationException">The requested address belongs to a migrated Yard table.</exception>
        public byte ReadByte(int address) =>
            address is >= 0xa3cd42 and < 0xa3cd82 or
                >= 0xa3cdc2 and < 0xa3cde2 or
                >= 0xa3d1ab and < 0xa3d1b3 or
                >= 0xa3d50f and < 0xa3d517 or
                >= 0xa3d5a4 and < 0xa3d5ac
                ? throw new InvalidOperationException(
                    $"Yard attempted migrated direction/list read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>
        /// Preserves writes by forwarding them to the wrapped address space.
        /// </summary>
        /// <param name="address">Cartridge address to write.</param>
        /// <param name="value">Byte to store at <paramref name="address"/>.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
