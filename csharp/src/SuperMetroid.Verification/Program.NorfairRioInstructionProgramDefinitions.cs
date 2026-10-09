using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Loads the retail cartridge and runs the Norfair Rio instruction-definition verification suite.</summary>
    private static void VerifyNorfairRioInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyNorfairRioInstructionProgramDefinitions), () => VerifyNorfairRioInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled Norfair Rio mechanics and executed programs with cartridge behavior.</summary>
    /// <param name="rom">Cartridge address space used to check native instruction and presentation words.</param>
    private static void VerifyNorfairRioInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        AssertEqual(65, NorfairRioInstructionProgramDefinitions.MechanicsWordCount,
            "Norfair Rio compiled mechanics word count");
        AssertEqual(34, NorfairRioInstructionProgramDefinitions.PresentationWordCount,
            "Norfair Rio live presentation word count");
        for (int index = 0;
             index < NorfairRioInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                NorfairRioInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadNorfairRioInstructionWord(rom, definition.Address),
                $"Norfair Rio mechanics word $A2:{definition.Address:X4}");
        }

        var guard = new NorfairRioInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        (ushort Entry, int Calls, ushort Final, ushort Offset, bool Signal, string Name)[]
            programs =
        [
            (NorfairRioInstructionProgramDefinitions.Idle,
                5,
                unchecked((ushort)(NorfairRioInstructionProgramDefinitions.Idle + 6)),
                8, false, "idle"),
            (NorfairRioInstructionProgramDefinitions.StartDescending,
                7,
                unchecked((ushort)(
                    NorfairRioInstructionProgramDefinitions.StartDescending + 38)),
                unchecked((ushort)-16), true, "start descending"),
            (NorfairRioInstructionProgramDefinitions.Descending,
                5,
                unchecked((ushort)(NorfairRioInstructionProgramDefinitions.Descending + 6)),
                unchecked((ushort)-12), false, "descending"),
            (NorfairRioInstructionProgramDefinitions.StartAscending,
                9,
                unchecked((ushort)(
                    NorfairRioInstructionProgramDefinitions.StartAscending + 50)),
                12, true, "start ascending"),
            (NorfairRioInstructionProgramDefinitions.Ascending,
                5,
                unchecked((ushort)(NorfairRioInstructionProgramDefinitions.Ascending + 6)),
                12, false, "ascending"),
            (NorfairRioInstructionProgramDefinitions.FlamesAscending,
                5,
                unchecked((ushort)(
                    NorfairRioInstructionProgramDefinitions.FlamesAscending + 4)),
                0, false, "ascending flames"),
            (NorfairRioInstructionProgramDefinitions.FlamesDescending,
                5,
                unchecked((ushort)(
                    NorfairRioInstructionProgramDefinitions.FlamesDescending + 4)),
                0, false, "descending flames"),
        ];
        foreach ((ushort entry, int calls, ushort final, ushort offset, bool signal,
                  string name) in programs)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, NorfairRioEnemyState state) =
                NewNorfairRioInstructionSystem(guard, flags, entry);
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            RunNorfairRioInstructionFrames(
                process,
                enemies,
                arguments,
                slot,
                calls);
            AssertEqual(final, slot.CurrentInstruction,
                $"Norfair Rio {name} completes its program");
            AssertEqual(offset, state.FollowerYOffset,
                $"Norfair Rio {name} publishes its final follower offset");
            AssertEqual(signal, state.AnimationSignal,
                $"Norfair Rio {name} animation signal");
        }

        Suite(nameof(VerifyNorfairRioInitializerSelections), () => VerifyNorfairRioInitializerSelections(guard, flags));

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Norfair Rio presentation selectors are compiled, not ROM reads");
        for (int index = 0;
             index < NorfairRioInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                NorfairRioInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertEqual(ReadNorfairRioInstructionWord(rom, address),
                EnemySpritemapDefinitions.NorfairRioFrameAt(address),
                $"compiled Norfair Rio selector $A2:{address:X4} matches ROM");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Norfair Rio mechanics byte");

        AssertThrows<InvalidDataException>(
            () => NorfairRioInstructionProgramDefinitions.ReadMechanicsWord(
                NorfairRioInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Norfair Rio spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => NorfairRioInstructionProgramDefinitions.ReadMechanicsWord(
                NorfairRioInstructionProgramDefinitions.AdjacentMovementDefinitions),
            "adjacent Norfair Rio movement definitions are rejected as mechanics");

        _ = ProbeNorfairRioInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeNorfairRioInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Norfair Rio allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Norfair Rio mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Norfair Rio instruction mechanics: 65 compiled words, all seven parent/" +
            "flame programs, eleven callbacks, and 34 compiled spritemap selectors pass.");
    }

    /// <summary>Checks that parent and flame-follower parameters select their corresponding native program entries.</summary>
    /// <param name="bus">Address space supplied to the enemy system during initialization.</param>
    /// <param name="flags">Reflection flags used to access the system's private initializer and bus field.</param>
    private static void VerifyNorfairRioInitializerSelections(
        ISnesAddressSpace bus,
        BindingFlags flags)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeNorfairRio",
            flags)!;

        RoomEnemySlot parent = enemies.Slots[0];
        parent.EnemyDefinitionPointer = RoomEnemySystem.NorfairRioDefinition;
        parent.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        initialize.Invoke(enemies, [parent]);
        AssertEqual(NorfairRioInstructionProgramDefinitions.Idle,
            parent.CurrentInstruction,
            "Norfair Rio parent initializer selects idle");

        RoomEnemySlot follower = enemies.Slots[1];
        follower.EnemyDefinitionPointer = RoomEnemySystem.NorfairRioDefinition;
        follower.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        follower.Parameter1 = 0x8000;
        initialize.Invoke(enemies, [follower]);
        AssertEqual(NorfairRioInstructionProgramDefinitions.FlamesAscending,
            follower.CurrentInstruction,
            "Norfair Rio follower initializer selects ascending flames");
    }

    /// <summary>Builds a one-slot enemy system positioned at a chosen Norfair Rio instruction entry.</summary>
    /// <param name="bus">Address space installed as the system's cartridge bus.</param>
    /// <param name="flags">Reflection flags used to install the bus and seed the private per-slot state array.</param>
    /// <param name="entry">Instruction address at which the slot begins execution.</param>
    /// <param name="art">Optional artwork catalog made available to the enemy system.</param>
    /// <returns>The configured system, its active slot, and the slot's Norfair Rio runtime state.</returns>
    private static (
        RoomEnemySystem Enemies,
        RoomEnemySlot Slot,
        NorfairRioEnemyState State) NewNorfairRioInstructionSystem(
            ISnesAddressSpace bus,
            BindingFlags flags,
            ushort entry,
            EnemyTileArtworkCatalog? art = null)
    {
        var enemies = new RoomEnemySystem { TileArtwork = art };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.NorfairRioDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        slot.CurrentInstruction = entry;
        slot.InstructionTimer = 1;
        var state = new NorfairRioEnemyState(slot);
        var states = (NorfairRioEnemyState?[])typeof(RoomEnemySystem)
            .GetField("_norfairRioStates", flags)!
            .GetValue(enemies)!;
        states[0] = state;
        return (enemies, slot, state);
    }

    /// <summary>Invokes the real instruction dispatcher a fixed number of times with the slot timer ready each time.</summary>
    /// <param name="process">Reflected instruction-dispatch method for the enemy system.</param>
    /// <param name="enemies">System instance that owns the slot and interpreter.</param>
    /// <param name="arguments">Reflection argument array passed to the dispatcher.</param>
    /// <param name="slot">Slot whose instruction timer is reset before each dispatch.</param>
    /// <param name="count">Number of dispatcher calls to execute.</param>
    private static void RunNorfairRioInstructionFrames(
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

    /// <summary>Consumes repeated compiled mechanics lookups for the warmed allocation check.</summary>
    /// <returns>A checksum of the selected instruction words so the lookups remain observable.</returns>
    private static int ProbeNorfairRioInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += NorfairRioInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? NorfairRioInstructionProgramDefinitions.Idle
                    : NorfairRioInstructionProgramDefinitions.StartAscending);
        }
        return checksum;
    }

    /// <summary>Reads a little-endian word from bank $A2 at a 16-bit instruction address.</summary>
    /// <param name="source">Cartridge address space containing the native program bytes.</param>
    /// <param name="address">Bank-relative address of the low byte.</param>
    /// <returns>The native word formed from the addressed byte and its successor.</returns>
    private static ushort ReadNorfairRioInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa20000 | address) |
            source.ReadByte(0xa20000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Tracks permitted Norfair Rio selector reads and throws if runtime execution reads compiled mechanics bytes.</summary>
    /// <param name="source">Underlying address space used for reads and writes allowed by the guard.</param>
    /// <param name="forbidPresentation">When true, selector reads also throw instead of being recorded.</param>
    private sealed class NorfairRioInstructionReadGuard(
        ISnesAddressSpace source, bool forbidPresentation = false) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Bank-relative presentation words observed during execution when selector reads are permitted.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        /// <summary>Count of attempted reads from instruction bytes expected to be served by compiled definitions.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge-import reads through the guard's runtime read checks.</summary>
        /// <param name="address">Cartridge byte address requested by the importer.</param>
        /// <returns>The source byte if the guarded read is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics reads, records or rejects presentation selectors, and forwards other bytes.</summary>
        /// <param name="address">Cartridge byte address requested by the instruction dispatcher.</param>
        /// <returns>The source byte when the read is permitted.</returns>
        public byte ReadByte(int address)
        {
            if (NorfairRioInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Norfair Rio mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < NorfairRioInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = NorfairRioInstructionProgramDefinitions
                        .PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        if (forbidPresentation)
                            throw new InvalidOperationException(
                                $"Installed Norfair Rio read visual selector " +
                                $"$A2:{presentation:X4}.");
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards writes unchanged; this fixture guard observes instruction-data reads.</summary>
        /// <param name="address">Destination cartridge byte address.</param>
        /// <param name="value">Byte written to the underlying address space.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
