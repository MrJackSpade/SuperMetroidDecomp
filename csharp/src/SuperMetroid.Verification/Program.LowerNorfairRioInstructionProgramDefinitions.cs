using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Loads the retail ROM and runs the complete Lower Norfair Rio instruction-definition verification.</summary>
    private static void VerifyLowerNorfairRioInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyLowerNorfairRioInstructionProgramDefinitions), () => VerifyLowerNorfairRioInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks compiled mechanics words, all seven instruction sequences and initializer choices,
    /// then confirms production avoids reads from compiled mechanics and presentation data.</summary>
    /// <param name="rom">Retail address space used for expected instruction words and visual selectors.</param>
    private static void VerifyLowerNorfairRioInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        AssertEqual(51, LowerNorfairRioInstructionProgramDefinitionsTooling.MechanicsWordCount,
            "Lower Norfair Rio compiled mechanics word count");
        AssertEqual(32, LowerNorfairRioInstructionProgramDefinitionsTooling.PresentationWordCount,
            "Lower Norfair Rio live presentation word count");
        for (int index = 0;
             index < LowerNorfairRioInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                LowerNorfairRioInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadLowerNorfairRioInstructionWord(rom, definition.Address),
                $"Lower Norfair Rio mechanics word $A2:{definition.Address:X4}");
        }

        var guard = new LowerNorfairRioInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        (ushort Entry, int Calls, ushort Final, bool Visible, bool Signal, string Name)[]
            programs =
        [
            (LowerNorfairRioInstructionProgramDefinitions.Idle,
                5,
                unchecked((ushort)(LowerNorfairRioInstructionProgramDefinitions.Idle + 6)),
                false, false, "idle"),
            (LowerNorfairRioInstructionProgramDefinitions.PrepareToSwoop,
                10,
                unchecked((ushort)(
                    LowerNorfairRioInstructionProgramDefinitions.PrepareToSwoop + 40)),
                false, true, "prepare to swoop"),
            (LowerNorfairRioInstructionProgramDefinitions.Descending,
                2,
                unchecked((ushort)(
                    LowerNorfairRioInstructionProgramDefinitions.Descending + 6)),
                false, false, "descending"),
            (LowerNorfairRioInstructionProgramDefinitions.AscendingPart1,
                4,
                unchecked((ushort)(
                    LowerNorfairRioInstructionProgramDefinitions.AscendingPart1 + 16)),
                false, true, "ascending part one"),
            (LowerNorfairRioInstructionProgramDefinitions.AscendingPart2,
                4,
                unchecked((ushort)(
                    LowerNorfairRioInstructionProgramDefinitions.AscendingPart2 + 6)),
                true, false, "ascending part two"),
            (LowerNorfairRioInstructionProgramDefinitions.Cooldown,
                10,
                unchecked((ushort)(
                    LowerNorfairRioInstructionProgramDefinitions.Cooldown + 40)),
                true, true, "cooldown"),
            (LowerNorfairRioInstructionProgramDefinitions.Flames,
                4,
                unchecked((ushort)(
                    LowerNorfairRioInstructionProgramDefinitions.Flames + 4)),
                false, false, "flames"),
        ];
        foreach ((ushort entry, int calls, ushort final, bool visible, bool signal,
                  string name) in programs)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, LowerNorfairRioEnemyState state) =
                NewLowerNorfairRioInstructionSystem(guard, flags, entry);
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            RunLowerNorfairRioInstructionFrames(
                process,
                enemies,
                arguments,
                slot,
                calls);
            AssertEqual(final, slot.CurrentInstruction,
                $"Lower Norfair Rio {name} completes its program");
            AssertEqual(visible, state.FollowerVisible,
                $"Lower Norfair Rio {name} publishes follower visibility");
            AssertEqual(signal, state.AnimationSignal,
                $"Lower Norfair Rio {name} animation signal");
        }

        Suite(nameof(VerifyLowerNorfairRioInitializerSelections), () => VerifyLowerNorfairRioInitializerSelections(guard, flags));

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Lower Norfair Rio presentation selectors are compiled, not ROM reads");
        for (int index = 0;
             index < LowerNorfairRioInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address =
                LowerNorfairRioInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertEqual(ReadLowerNorfairRioInstructionWord(rom, address),
                EnemySpritemapDefinitions.LowerNorfairRioFrameAt(address),
                $"compiled Lower Norfair Rio selector $A2:{address:X4} matches ROM");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Lower Norfair Rio mechanics byte");

        AssertThrows<InvalidDataException>(
            () => LowerNorfairRioInstructionProgramDefinitions.ReadMechanicsWord(
                LowerNorfairRioInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "Lower Norfair Rio spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => LowerNorfairRioInstructionProgramDefinitions.ReadMechanicsWord(
                LowerNorfairRioInstructionProgramDefinitions.AdjacentMovementDefinitions),
            "adjacent Lower Norfair Rio movement definitions are rejected as mechanics");

        _ = ProbeLowerNorfairRioInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeLowerNorfairRioInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "Lower Norfair Rio allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Lower Norfair Rio mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Lower Norfair Rio instruction mechanics: 51 compiled words, all seven " +
            "parent/flame programs, three callbacks, and 32 compiled spritemap selectors pass.");
    }

    /// <summary>Checks that room initialization selects the idle program for the parent and the flame program for a follower.</summary>
    /// <param name="bus">Address space installed on the room enemy system during initialization.</param>
    /// <param name="flags">Reflection flags used to invoke the private initializer.</param>
    private static void VerifyLowerNorfairRioInitializerSelections(
        ISnesAddressSpace bus,
        BindingFlags flags)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeLowerNorfairRio",
            flags)!;

        RoomEnemySlot parent = enemies.Slots[0];
        parent.EnemyDefinitionPointer = RoomEnemySystem.LowerNorfairRioDefinition;
        parent.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        initialize.Invoke(enemies, [parent]);
        AssertEqual(LowerNorfairRioInstructionProgramDefinitions.Idle,
            parent.CurrentInstruction,
            "Lower Norfair Rio parent initializer selects idle");

        RoomEnemySlot follower = enemies.Slots[1];
        follower.EnemyDefinitionPointer = RoomEnemySystem.LowerNorfairRioDefinition;
        follower.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        follower.Parameter1 = 0x8000;
        initialize.Invoke(enemies, [follower]);
        AssertEqual(LowerNorfairRioInstructionProgramDefinitions.Flames,
            follower.CurrentInstruction,
            "Lower Norfair Rio follower initializer selects flames");
    }

    /// <summary>Creates a room system with one Rio slot and its initialized extension state for instruction tests.</summary>
    /// <param name="bus">Address space assigned to the room system.</param>
    /// <param name="flags">Reflection flags used to install the private state and address space.</param>
    /// <param name="entry">Instruction-list address assigned to the test slot.</param>
    /// <param name="art">Optional installed tile artwork for instruction paths that use presentation data.</param>
    /// <returns>The room system, configured slot, and matching Rio state.</returns>
    private static (
        RoomEnemySystem Enemies,
        RoomEnemySlot Slot,
        LowerNorfairRioEnemyState State) NewLowerNorfairRioInstructionSystem(
            ISnesAddressSpace bus,
            BindingFlags flags,
            ushort entry,
            EnemyTileArtworkCatalog? art = null)
    {
        var enemies = new RoomEnemySystem { TileArtwork = art };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.LowerNorfairRioDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        slot.CurrentInstruction = entry;
        slot.InstructionTimer = 1;
        var state = new LowerNorfairRioEnemyState(slot);
        var states = (LowerNorfairRioEnemyState?[])typeof(RoomEnemySystem)
            .GetField("_lowerNorfairRioStates", flags)!
            .GetValue(enemies)!;
        states[0] = state;
        return (enemies, slot, state);
    }

    /// <summary>Advances a private instruction dispatcher a fixed number of calls, resetting its timer to one before each call.</summary>
    /// <param name="process">Reflected instruction-processing method to invoke.</param>
    /// <param name="enemies">Room system that owns the test slot and instruction state.</param>
    /// <param name="arguments">Argument array passed to the reflected dispatcher on each call.</param>
    /// <param name="slot">Slot whose instruction timer is reset before dispatch.</param>
    /// <param name="count">Number of dispatcher calls to perform.</param>
    private static void RunLowerNorfairRioInstructionFrames(
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

    /// <summary>Repeatedly resolves two compiled instruction words and accumulates their values for an allocation probe.</summary>
    /// <returns>Checksum keeping the repeated lookup results observable.</returns>
    private static int ProbeLowerNorfairRioInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += LowerNorfairRioInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? LowerNorfairRioInstructionProgramDefinitions.Idle
                    : LowerNorfairRioInstructionProgramDefinitions.Cooldown);
        }
        return checksum;
    }

    /// <summary>Reads one little-endian instruction word from bank $A2 for cartridge comparisons.</summary>
    /// <param name="source">Retail cartridge address space.</param>
    /// <param name="address">Bank-relative address of the word's low byte.</param>
    /// <returns>The word formed from the addressed byte and its successor.</returns>
    private static ushort ReadLowerNorfairRioInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa20000 | address) |
            source.ReadByte(0xa20000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Address-space proxy that counts forbidden mechanics reads and tracks or rejects presentation reads.</summary>
    /// <param name="source">Underlying address space for reads that pass the guard and for writes.</param>
    /// <param name="forbidPresentation">Whether compiled visual-selector reads throw instead of being recorded.</param>
    private sealed class LowerNorfairRioInstructionReadGuard(
        ISnesAddressSpace source, bool forbidPresentation = false) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Distinct compiled presentation operands observed through the guard when observation is allowed.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        /// <summary>Number of attempts to read bytes owned by the compiled mechanics table.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes importer cartridge reads through mechanics rejection and presentation tracking.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The underlying byte when the address passes the guard.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics reads and optionally rejects presentation reads before forwarding others.</summary>
        /// <param name="address">Address requested from the wrapped cartridge space.</param>
        /// <returns>The underlying byte for an address allowed by this guard.</returns>
        /// <exception cref="InvalidOperationException">The address is compiled mechanics data or a forbidden presentation selector.</exception>
        public byte ReadByte(int address)
        {
            if (LowerNorfairRioInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Lower Norfair Rio mechanics byte " +
                    $"${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < LowerNorfairRioInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation = LowerNorfairRioInstructionProgramDefinitionsTooling
                        .PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        if (forbidPresentation)
                            throw new InvalidOperationException(
                                $"Installed Lower Norfair Rio read visual selector " +
                                $"$A2:{presentation:X4}.");
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards writes without applying the read guard.</summary>
        /// <param name="address">Address receiving the write.</param>
        /// <param name="value">Byte stored at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
