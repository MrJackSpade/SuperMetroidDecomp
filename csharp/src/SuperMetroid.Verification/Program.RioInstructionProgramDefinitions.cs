using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Loads the retail ROM and verifies the compiled Rio instruction definitions against native data and execution.</summary>
    private static void VerifyRioInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyRioInstructionProgramDefinitions), () => VerifyRioInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled mechanics with bank A2 and runs Rio's production initializer and instruction programs.</summary>
    /// <param name="rom">Retail address space containing Rio's native instruction and presentation data.</param>
    private static void VerifyRioInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < RioInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                RioInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadRioInstructionWord(rom, definition.Address),
                $"Rio mechanics word $A2:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new RioInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = type.GetMethod("InitializeRio", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot rio = enemies.Slots[0];
        rio.EnemyDefinitionPointer = RoomEnemySystem.RioDefinition;
        rio.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        initialize(rio);
        AssertEqual(RioInstructionProgramDefinitions.Idle,
            rio.CurrentInstruction,
            "real Rio initializer installs compiled idle program");

        object?[] processArguments =
            [rio, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        ExecuteRioProgram(
            enemies,
            process,
            processArguments,
            rio,
            RioInstructionProgramDefinitions.Idle,
            callCount: 13);
        AssertEqual(unchecked((ushort)(RioInstructionProgramDefinitions.Idle + 4)),
            rio.CurrentInstruction,
            "idle fallthrough and post-swoop program return to first repeated idle frame");

        RioEnemyState state = enemies.RioStates[0]!;
        state.AnimationFinished = false;
        ExecuteRioProgram(
            enemies,
            process,
            processArguments,
            rio,
            RioInstructionProgramDefinitions.SwoopingPart1,
            callCount: 6);
        AssertEqual(unchecked((ushort)(RioInstructionProgramDefinitions.SwoopingPart1 + 22)),
            rio.CurrentInstruction,
            "first swoop program reaches terminal sleep");
        AssertTrue(state.AnimationFinished,
            "first swoop program publishes animation completion");

        ExecuteRioProgram(
            enemies,
            process,
            processArguments,
            rio,
            RioInstructionProgramDefinitions.SwoopingPart2,
            callCount: 3);
        AssertEqual(unchecked((ushort)(RioInstructionProgramDefinitions.SwoopingPart2 + 4)),
            rio.CurrentInstruction,
            "second swoop program loops and installs its first repeated frame");

        state.AnimationFinished = false;
        ExecuteRioProgram(
            enemies,
            process,
            processArguments,
            rio,
            RioInstructionProgramDefinitions.SwoopCooldown,
            callCount: 6);
        AssertEqual(unchecked((ushort)(RioInstructionProgramDefinitions.SwoopCooldown + 22)),
            rio.CurrentInstruction,
            "swoop cooldown reaches terminal sleep");
        AssertTrue(state.AnimationFinished,
            "swoop cooldown publishes animation completion");

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Rio presentation selectors are compiled, not ROM reads");
        for (int index = 0;
             index < RioInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = RioInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertEqual(ReadRioInstructionWord(rom, address),
                EnemySpritemapDefinitions.RioFrameAt(address),
                $"compiled Rio selector $A2:{address:X4} matches ROM");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Rio mechanics byte");
        AssertThrows<InvalidDataException>(
            () => RioInstructionProgramDefinitions.ReadMechanicsWord(
                RioInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Rio spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => RioInstructionProgramDefinitions.ReadMechanicsWord(
                RioInstructionProgramDefinitions.FirstAdjacentMechanicsData),
            "adjacent Rio launch data is rejected as instruction mechanics");

        _ = ProbeRioInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeRioInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Rio allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Rio mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Rio instruction mechanics: thirty-two compiled words, the real initializer, " +
            "idle fallthrough/return, both swoop phases, cooldown completion, and twenty-four " +
            "compiled spritemap selectors pass with mechanics bytes forbidden.");
    }

    /// <summary>Advances a selected Rio instruction list through the production instruction processor.</summary>
    /// <param name="enemies">Room enemy system containing Rio and its processor state.</param>
    /// <param name="process">Reflected production instruction-processing method.</param>
    /// <param name="processArguments">Arguments passed to each processor invocation.</param>
    /// <param name="rio">Rio slot whose instruction pointer and timer are advanced.</param>
    /// <param name="program">Instruction-list entry address installed before the first call.</param>
    /// <param name="callCount">Number of processor calls to perform.</param>
    private static void ExecuteRioProgram(
        RoomEnemySystem enemies,
        MethodInfo process,
        object?[] processArguments,
        RoomEnemySlot rio,
        ushort program,
        int callCount)
    {
        rio.CurrentInstruction = program;
        for (int call = 0; call < callCount; call++)
        {
            rio.InstructionTimer = 1;
            process.Invoke(enemies, processArguments);
        }
    }

    /// <summary>Repeatedly reads representative compiled Rio mechanics words for the warmed-allocation check.</summary>
    /// <returns>A checksum that keeps the repeated lookups observable.</returns>
    private static int ProbeRioInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += RioInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? RioInstructionProgramDefinitions.Idle
                    : unchecked((ushort)(RioInstructionProgramDefinitions.SwoopCooldown + 22)));
        }
        return checksum;
    }

    /// <summary>Reads a little-endian instruction word from Rio's bank A2.</summary>
    /// <param name="source">Retail address space containing bank A2.</param>
    /// <param name="address">Offset of the low byte within bank A2.</param>
    /// <returns>The word formed by the addressed byte and its successor.</returns>
    private static ushort ReadRioInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa20000 | address) |
            source.ReadByte(0xa20000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Detects production reads of compiled Rio mechanics and optionally rejects presentation-selector reads.</summary>
    /// <param name="source">Wrapped address space used for reads and writes permitted by the guard.</param>
    /// <param name="forbidPresentation">Whether reads of compiled presentation words should throw instead of being recorded.</param>
    private sealed class RioInstructionReadGuard(
        ISnesAddressSpace source, bool forbidPresentation = false) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets presentation-word offsets whose bytes were requested through this wrapper.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Gets the number of attempts to read bytes owned by compiled Rio mechanics.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes a cartridge-byte request through the guard's read policy.</summary>
        /// <param name="address">Cartridge bus address to read.</param>
        /// <returns>The byte returned by the wrapped source when allowed.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled-mechanics reads, records or rejects presentation reads, and forwards other bytes.</summary>
        /// <param name="address">Bus address of the requested byte.</param>
        /// <returns>The requested byte when the guard permits the read.</returns>
        public byte ReadByte(int address)
        {
            if (RioInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Rio mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < RioInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        RioInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        if (forbidPresentation)
                            throw new InvalidOperationException(
                                $"Installed Rio read visual selector $A2:{presentation:X4}.");
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }

            return source.ReadByte(address);
        }

        /// <summary>Forwards a write unchanged to the wrapped address space.</summary>
        /// <param name="address">Bus address receiving the write.</param>
        /// <param name="value">Byte written at <paramref name="address"/>.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
