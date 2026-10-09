using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Runs the retail-ROM-backed checks for the compiled Metroid instruction programs and their runtime behavior.
    /// </summary>
    private static void VerifyMetroidInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyMetroidInstructionProgramDefinitions), () => VerifyMetroidInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Compares the compiled Metroid mechanics words with cartridge data and exercises both instruction loops,
    /// initialization, sound callbacks, and the no-cartridge-read guarantee.
    /// </summary>
    /// <param name="rom">Address space containing the retail cartridge bytes used as the reference.</param>
    private static void VerifyMetroidInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        AssertEqual(31, MetroidInstructionProgramDefinitionsTooling.MechanicsWordCount,
            "Metroid compiled mechanics word count");
        AssertEqual(25, MetroidInstructionProgramDefinitionsTooling.PresentationWordCount,
            "Metroid compiled presentation word count");
        for (int index = 0;
             index < MetroidInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MetroidInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadMetroidInstructionWord(rom, definition.Address),
                $"Metroid mechanics word $A3:{definition.Address:X4}");
        }

        var guard = new MetroidInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        int randomCalls = 0;
        (RoomEnemySystem chasingEnemies, RoomEnemySlot chasing) =
            NewMetroidInstructionSystem(
                guard,
                flags,
                MetroidInstructionProgramDefinitions.ChasingSamus,
                () =>
                {
                    randomCalls++;
                    return 5;
                });
        object?[] chasingArguments =
            [chasing, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        RunMetroidInstructionFrames(
            process,
            chasingEnemies,
            chasingArguments,
            chasing,
            count: 21);
        AssertEqual(unchecked((ushort)(MetroidInstructionProgramDefinitions.ChasingSamus + 4)),
            chasing.CurrentInstruction,
            "Metroid chasing program completes its twenty-frame loop");
        AssertEqual(1, randomCalls,
            "Metroid chasing callback advances the random generator once");
        AssertEqual((ushort?)MetroidBehaviorDefinitions.RandomCrySoundEffect(5),
            chasingEnemies.LastMetroidSoundEffectLibrary2,
            "Metroid chasing callback publishes its selected cry");

        (RoomEnemySystem drainingEnemies, RoomEnemySlot draining) =
            NewMetroidInstructionSystem(
                guard,
                flags,
                MetroidInstructionProgramDefinitions.DrainingSamus,
                () => throw new InvalidOperationException(
                    "Draining Metroid animation must not advance the random generator."));
        object?[] drainingArguments =
            [draining, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        RunMetroidInstructionFrames(
            process,
            drainingEnemies,
            drainingArguments,
            draining,
            count: 6);
        AssertEqual(unchecked((ushort)(MetroidInstructionProgramDefinitions.DrainingSamus + 4)),
            draining.CurrentInstruction,
            "Metroid draining program completes its five-frame loop");
        AssertEqual((ushort?)0x0050, drainingEnemies.LastMetroidSoundEffectLibrary2,
            "Metroid draining callback publishes the native sound");

        Suite(nameof(VerifyMetroidInstructionInitializer), () => VerifyMetroidInstructionInitializer(guard, flags));

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "both Metroid loops use compiled visual selectors, not cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Metroid mechanics byte");

        AssertThrows<InvalidDataException>(
            () => MetroidInstructionProgramDefinitions.ReadMechanicsWord(
                MetroidInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "Metroid spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MetroidInstructionProgramDefinitions.ReadMechanicsWord(
                MetroidInstructionProgramDefinitions.AdjacentBombedOffVelocities),
            "adjacent Metroid bombed-off velocity data is rejected as mechanics");

        _ = ProbeMetroidInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMetroidInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Metroid allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Metroid mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Metroid instruction mechanics: 31 compiled words, both complete animation " +
            "loops and sound callbacks, and 25 compiled visual selectors pass.");
    }

    /// <summary>
    /// Invokes Metroid initialization on a prepared room slot and checks its initial program and allocated bodies.
    /// </summary>
    /// <param name="bus">Address space used by the enemy system during initialization.</param>
    /// <param name="flags">Reflection flags that expose the system's private bus and initializer.</param>
    private static void VerifyMetroidInstructionInitializer(
        ISnesAddressSpace bus,
        BindingFlags flags)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.MetroidDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
        slot.XPosition = 128;
        slot.YPosition = 128;
        typeof(RoomEnemySystem).GetMethod("InitializeMetroid", flags)!.Invoke(enemies, [slot]);
        AssertEqual(MetroidInstructionProgramDefinitions.ChasingSamus,
            slot.CurrentInstruction,
            "Metroid initializer selects the chasing program");
        MetroidEnemyState state = enemies.MetroidStates[0] ??
            throw new InvalidOperationException(
                "Metroid initializer did not publish typed state.");
        AssertTrue(state.OuterBodyA.IsActive,
            "Metroid initializer allocates outer body A");
        AssertTrue(state.OuterBodyB.IsActive,
            "Metroid initializer allocates outer body B");
    }

    /// <summary>
    /// Creates a room enemy system with a Metroid slot positioned at a chosen instruction entry.
    /// </summary>
    /// <param name="bus">Address space wrapped by the read guard during instruction processing.</param>
    /// <param name="flags">Reflection flags used to set the system's private bus and random callback.</param>
    /// <param name="entry">Compiled instruction address from which the slot begins execution.</param>
    /// <param name="nextRandom">Callback supplied to the system for Metroid behavior that requests randomness.</param>
    /// <returns>The configured system and its active Metroid slot.</returns>
    private static (RoomEnemySystem Enemies, RoomEnemySlot Slot)
        NewMetroidInstructionSystem(
            ISnesAddressSpace bus,
            BindingFlags flags,
            ushort entry,
            Func<ushort> nextRandom)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!
            .SetValue(enemies, nextRandom);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.MetroidDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
        slot.CurrentInstruction = entry;
        slot.InstructionTimer = 1;
        return (enemies, slot);
    }

    /// <summary>
    /// Advances the reflected instruction processor a fixed number of calls, making each call eligible to execute.
    /// </summary>
    /// <param name="process">The room enemy instruction-processing method to invoke.</param>
    /// <param name="enemies">System instance on which each processing call runs.</param>
    /// <param name="arguments">Argument array passed unchanged to the reflected method.</param>
    /// <param name="slot">Metroid slot whose timer is reset before each call.</param>
    /// <param name="count">Number of processor calls to make.</param>
    private static void RunMetroidInstructionFrames(
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

    /// <summary>
    /// Repeatedly resolves the two Metroid mechanics entries so a warmed invocation can be checked for allocations.
    /// </summary>
    /// <returns>A checksum of the resolved words, ensuring the lookup results are consumed.</returns>
    private static int ProbeMetroidInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MetroidInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? MetroidInstructionProgramDefinitions.ChasingSamus
                    : MetroidInstructionProgramDefinitions.DrainingSamus);
        }
        return checksum;
    }

    /// <summary>
    /// Reads the little-endian word at a bank-$A3 offset from the retail address space.
    /// </summary>
    /// <param name="source">Cartridge address space containing the reference word.</param>
    /// <param name="address">Offset of the word within bank $A3.</param>
    /// <returns>The two adjacent cartridge bytes combined as a 16-bit value.</returns>
    private static ushort ReadMetroidInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa30000 | address) |
            source.ReadByte(0xa30000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>
    /// Wraps the address space to fail if production execution reads compiled Metroid mechanics or presentation data.
    /// </summary>
    /// <param name="source">Underlying address space used for reads and writes that are permitted by the guard.</param>
    private sealed class MetroidInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>
        /// Presentation-word offsets whose bytes production attempted to read before the guard rejected the access.
        /// </summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>
        /// Number of attempted reads rejected because they targeted compiled mechanics or presentation bytes.
        /// </summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>
        /// Routes the cartridge-import read through the same checks as ordinary address-space reads.
        /// </summary>
        /// <param name="address">SNES address requested by the importer.</param>
        /// <returns>The byte returned by the wrapped address space when the address is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>
        /// Rejects reads from compiled Metroid tables and delegates all other reads to the wrapped address space.
        /// </summary>
        /// <param name="address">SNES address requested by the caller.</param>
        /// <returns>The wrapped address-space byte when the address is outside guarded tables.</returns>
        /// <exception cref="InvalidOperationException">A compiled mechanics or presentation byte was requested.</exception>
        public byte ReadByte(int address)
        {
            if (MetroidInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Metroid mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < MetroidInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        MetroidInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                        ForbiddenReadAttempts++;
                        throw new InvalidOperationException(
                            $"Production read compiled Metroid presentation byte ${address:X6}.");
                    }
                }
            }
            return source.ReadByte(address);
        }

        /// <summary>
        /// Forwards writes unchanged because this guard only observes prohibited table reads.
        /// </summary>
        /// <param name="address">SNES address to write.</param>
        /// <param name="value">Byte stored at the requested address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
