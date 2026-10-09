using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Registers the retail-ROM verification suite for the Yapping Maw instruction program definitions.</summary>
    private static void VerifyYappingMawInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyYappingMawInstructionProgramDefinitions), () => VerifyYappingMawInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks compiled Yapping Maw mechanics against cartridge words, then exercises attack, cooldown, initialization, and allocation behavior through guarded production access.</summary>
    /// <param name="rom">Retail address space providing reference instruction words and other permitted cartridge data.</param>
    private static void VerifyYappingMawInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        AssertEqual(96, YappingMawInstructionProgramDefinitions.MechanicsWordCount,
            "Yapping Maw compiled mechanics word count");
        AssertEqual(52, YappingMawInstructionProgramDefinitions.PresentationWordCount,
            "Yapping Maw live presentation word count");
        for (int index = 0;
             index < YappingMawInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                YappingMawInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadYappingMawInstructionWord(rom, definition.Address),
                $"Yapping Maw mechanics word $A8:{definition.Address:X4}");
        }

        var guard = new YappingMawInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        for (int direction = 0; direction < 8; direction++)
        {
            ushort entry = YappingMawInstructionProgramDefinitions
                .AttackForDirection(direction);
            (RoomEnemySystem enemies, RoomEnemySlot slot, _) =
                NewYappingMawInstructionSystem(guard, flags, entry);
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            RunYappingMawInstructionFrames(process, enemies, arguments, slot, count: 5);
            AssertEqual(unchecked((ushort)(entry + 4)), slot.CurrentInstruction,
                $"Yapping Maw attack direction {direction} completes its loop");
            AssertEqual((ushort?)0x002f, enemies.LastYappingMawSoundEffect,
                $"Yapping Maw attack direction {direction} publishes its sound");
        }

        (ushort Entry, int Calls, ushort Final, short X, short Y, string Name)[] cooldowns =
        [
            (YappingMawInstructionProgramDefinitions.CooldownFacingUpRight,
                6,
                unchecked((ushort)(
                    YappingMawInstructionProgramDefinitions.CooldownFacingUp + 6)),
                8, -8, "up-right"),
            (YappingMawInstructionProgramDefinitions.CooldownFacingUp,
                5,
                unchecked((ushort)(
                    YappingMawInstructionProgramDefinitions.CooldownFacingUp + 6)),
                0, -16, "up"),
            (YappingMawInstructionProgramDefinitions.CooldownFacingUpLeft,
                6,
                unchecked((ushort)(
                    YappingMawInstructionProgramDefinitions.CooldownFacingUpLeft + 12)),
                -8, -8, "up-left"),
            (YappingMawInstructionProgramDefinitions.CooldownFacingDownRight,
                6,
                unchecked((ushort)(
                    YappingMawInstructionProgramDefinitions.CooldownFacingDown + 6)),
                8, 8, "down-right"),
            (YappingMawInstructionProgramDefinitions.CooldownFacingDown,
                5,
                unchecked((ushort)(
                    YappingMawInstructionProgramDefinitions.CooldownFacingDown + 6)),
                0, 16, "down"),
            (YappingMawInstructionProgramDefinitions.CooldownFacingDownLeft,
                6,
                unchecked((ushort)(
                    YappingMawInstructionProgramDefinitions.CooldownFacingDownLeft + 12)),
                -8, 8, "down-left"),
        ];
        foreach ((ushort entry, int calls, ushort final, short x, short y, string name)
                 in cooldowns)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, YappingMawEnemyState state) =
                NewYappingMawInstructionSystem(guard, flags, entry);
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            RunYappingMawInstructionFrames(process, enemies, arguments, slot, count: 1);
            AssertEqual(unchecked((ushort)x), state.HeldSamusXOffset,
                $"Yapping Maw {name} callback X offset");
            AssertEqual(unchecked((ushort)y), state.HeldSamusYOffset,
                $"Yapping Maw {name} callback Y offset");
            RunYappingMawInstructionFrames(
                process,
                enemies,
                arguments,
                slot,
                count: calls - 1);
            AssertEqual(final, slot.CurrentInstruction,
                $"Yapping Maw {name} cooldown completes its loop");
            AssertEqual((ushort?)0x002f, enemies.LastYappingMawSoundEffect,
                $"Yapping Maw {name} cooldown publishes its sound");
        }

        Suite(nameof(VerifyYappingMawInitializerSelections), () => VerifyYappingMawInitializerSelections(guard, flags));

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids all compiled Yapping Maw mechanics and visual bytes");

        AssertThrows<InvalidDataException>(
            () => YappingMawInstructionProgramDefinitions.ReadMechanicsWord(
                YappingMawInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Yapping Maw spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => YappingMawInstructionProgramDefinitions.ReadMechanicsWord(
                YappingMawInstructionProgramDefinitions.AdjacentAttackSelectorTable),
            "adjacent Yapping Maw direction table is rejected as mechanics");

        _ = ProbeYappingMawInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeYappingMawInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Yapping Maw allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Yapping Maw mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Yapping Maw instruction mechanics: 96 compiled words, all fourteen attack/" +
            "cooldown entries, seven callbacks, and 52 compiled visual selectors pass.");
    }

    /// <summary>Confirms the enemy initializer selects the expected attack program for ordinary and alternate Yapping Maw variants.</summary>
    /// <param name="bus">Address space assigned to the enemy system under verification.</param>
    /// <param name="flags">Reflection binding flags used to access the production initializer and bus field.</param>
    private static void VerifyYappingMawInitializerSelections(
        ISnesAddressSpace bus,
        BindingFlags flags)
    {
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeYappingMaw",
            flags)!;
        foreach ((ushort parameter2, ushort expected, string name) in new[]
        {
            ((ushort)0, YappingMawInstructionProgramDefinitions.AttackingFacingDown,
                "alternate/down"),
            ((ushort)1, YappingMawInstructionProgramDefinitions.AttackingFacingUp,
                "ordinary/up"),
        })
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.YappingMawDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
            slot.XPosition = 128;
            slot.YPosition = 128;
            slot.Parameter1 = 128;
            slot.Parameter2 = parameter2;
            initialize.Invoke(enemies, [slot]);
            AssertEqual(expected, slot.CurrentInstruction,
                $"Yapping Maw {name} initializer program");
            YappingMawEnemyState state = enemies.YappingMawStates[0] ??
                throw new InvalidOperationException(
                    $"Yapping Maw {name} initializer did not publish typed state.");
            AssertEqual(4, state.BodyProjectiles.Count(projectile => projectile is not null),
                $"Yapping Maw {name} initializer body links");
            AssertTrue(state.RootSpriteObject is not null,
                $"Yapping Maw {name} initializer root sprite object");
        }
    }

    /// <summary>Builds an enemy system with a typed Yapping Maw state and instruction slot positioned at a chosen program entry.</summary>
    /// <param name="bus">Address space installed on the new enemy system.</param>
    /// <param name="flags">Reflection binding flags used to install the bus and publish typed state.</param>
    /// <param name="entry">Initial instruction pointer to execute.</param>
    /// <returns>The system, its configured slot, and the typed state associated with that slot.</returns>
    private static (
        RoomEnemySystem Enemies,
        RoomEnemySlot Slot,
        YappingMawEnemyState State) NewYappingMawInstructionSystem(
            ISnesAddressSpace bus,
            BindingFlags flags,
            ushort entry)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.YappingMawDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        slot.CurrentInstruction = entry;
        slot.InstructionTimer = 1;
        var state = new YappingMawEnemyState(slot);
        var states = (YappingMawEnemyState?[])typeof(RoomEnemySystem)
            .GetField("_yappingMawStates", flags)!
            .GetValue(enemies)!;
        states[0] = state;
        return (enemies, slot, state);
    }

    /// <summary>Invokes the production instruction processor the requested number of times, making the instruction timer eligible on each call.</summary>
    /// <param name="process">Bound production instruction-processing method.</param>
    /// <param name="enemies">Enemy system receiving each instruction step.</param>
    /// <param name="arguments">Reflection argument array passed unchanged to the processor.</param>
    /// <param name="slot">Instruction slot whose timer is reset before every call.</param>
    /// <param name="count">Number of processor calls to perform.</param>
    private static void RunYappingMawInstructionFrames(
        MethodInfo process,
        RoomEnemySystem enemies,
        object?[] arguments,
        RoomEnemySlot slot,
        int count)
    {
        for (int call = 0; call < count; call++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
    }

    /// <summary>Warms and repeatedly reads compiled mechanics entries so the caller can measure steady-state lookup allocations.</summary>
    /// <returns>A checksum that keeps each lookup result observable to the caller.</returns>
    private static int ProbeYappingMawInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += YappingMawInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? YappingMawInstructionProgramDefinitions.AttackingFacingUp
                    : YappingMawInstructionProgramDefinitions.CooldownFacingDownLeft);
        }
        return checksum;
    }

    /// <summary>Reads a little-endian mechanics word from bank $A8 for comparison with its compiled definition.</summary>
    /// <param name="source">Retail address space containing the original instruction program.</param>
    /// <param name="address">Bank-relative address of the low byte.</param>
    /// <returns>The two cartridge bytes combined as an unsigned 16-bit word.</returns>
    private static ushort ReadYappingMawInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa80000 | address) |
            source.ReadByte(0xa80000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Wraps cartridge access for production execution and rejects reads of the Yapping Maw mechanics or presentation bytes being verified as compiled data.</summary>
    /// <param name="source">Underlying address space for permitted reads and writes.</param>
    private sealed class YappingMawInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Number of attempted reads rejected because they target compiled Yapping Maw mechanics or presentation data.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes import-source reads through the same compiled-data checks as other byte reads.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The underlying byte when the address is not owned by the compiled definitions.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics and presentation ranges, counting each attempt, and forwards all other reads.</summary>
        /// <param name="address">Cartridge address requested by production code.</param>
        /// <returns>The underlying byte when the address is outside guarded definition data.</returns>
        /// <exception cref="InvalidOperationException">The address identifies a compiled Yapping Maw mechanics or presentation byte.</exception>
        public byte ReadByte(int address)
        {
            if (YappingMawInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Yapping Maw mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < YappingMawInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = YappingMawInstructionProgramDefinitions
                        .PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ForbiddenReadAttempts++;
                        throw new InvalidOperationException(
                            $"Production read compiled Yapping Maw visual byte ${address:X6}.");
                    }
                }
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards writes unchanged; this guard prevents reads from compiled data without restricting address-space mutation.</summary>
        /// <param name="address">Cartridge address to write.</param>
        /// <param name="value">Byte passed through to the underlying address space.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
