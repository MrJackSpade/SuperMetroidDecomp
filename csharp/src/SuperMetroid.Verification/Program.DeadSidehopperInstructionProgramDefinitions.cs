using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Loads the retail ROM and verifies the compiled Dead sidehopper instruction programs against it.</summary>
    private static void VerifyDeadSidehopperInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyDeadSidehopperInstructionProgramDefinitions), () => VerifyDeadSidehopperInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled mechanics to bank A9 and executes each production Dead sidehopper program.</summary>
    /// <param name="rom">Retail address space containing the original Dead sidehopper mechanics and selectors.</param>
    private static void VerifyDeadSidehopperInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < DeadSidehopperInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                DeadSidehopperInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadDeadSidehopperInstructionWord(rom, definition.Address),
                $"Dead sidehopper mechanics word $A9:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new DeadSidehopperInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem { TileArtwork = RepositoryInstallation.EnemyTiles };
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = type.GetMethod("InitializeDeadSidehopper", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot corpse = enemies.Slots[0];
        corpse.EnemyDefinitionPointer = RoomEnemySystem.DeadSidehopperDefinition;
        corpse.Definition = default(RoomEnemyDefinition) with { Bank = 0xa9 };
        corpse.Parameter1 = 0;
        initialize(corpse);
        AssertEqual(DeadSidehopperInstructionProgramDefinitions.AliveIdle,
            corpse.CurrentInstruction,
            "real Dead sidehopper initializer installs compiled alive-idle program");

        object?[] processArguments =
            [corpse, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        ExecuteDeadSidehopperProgram(
            enemies,
            process,
            processArguments,
            corpse,
            DeadSidehopperInstructionProgramDefinitions.AliveIdle,
            frameCount: 1);
        ExecuteDeadSidehopperProgram(
            enemies,
            process,
            processArguments,
            corpse,
            DeadSidehopperInstructionProgramDefinitions.AliveCorpse,
            frameCount: 1);
        ExecuteDeadSidehopperProgram(
            enemies,
            process,
            processArguments,
            corpse,
            DeadSidehopperInstructionProgramDefinitions.InitiallyDead,
            frameCount: 1);
        ExecuteDeadSidehopperProgram(
            enemies,
            process,
            processArguments,
            corpse,
            DeadSidehopperInstructionProgramDefinitions.AliveHopping,
            frameCount: 8);

        AssertEqual(DeadSidehopperInstructionProgramDefinitions.HoppingSleepOpcode,
            corpse.CurrentInstruction,
            "Dead sidehopper hopping program executes end-hop callback and reaches sleep");
        AssertEqual(DeadSidehopperAiFunction.BeginPostLandingDelay,
            enemies.DeadSidehoppers[0]!.Function,
            "Dead sidehopper end-hop callback returns ownership to main AI");
        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Dead sidehopper execution uses compiled spritemap selectors");
        for (int index = 0;
             index < DeadSidehopperInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort operand = DeadSidehopperInstructionProgramDefinitionsTooling
                .PresentationWordAddress(index);
            AssertCompiledEnemyVisualSelector(rom, RoomEnemySystem.DeadSidehopperDefinition,
                0xa9, operand, $"Dead sidehopper $A9:{operand:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Dead sidehopper mechanics byte");
        AssertThrows<InvalidDataException>(
            () => DeadSidehopperInstructionProgramDefinitions.ReadMechanicsWord(
                DeadSidehopperInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "Dead sidehopper spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => DeadSidehopperInstructionProgramDefinitions.ReadMechanicsWord(
                DeadSidehopperInstructionProgramDefinitions.FirstAdjacentProgram),
            "adjacent corpse program is rejected as Dead sidehopper mechanics");

        _ = ProbeDeadSidehopperInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeDeadSidehopperInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "Dead sidehopper allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Dead sidehopper mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Dead sidehopper instruction mechanics: sixteen compiled words, the real " +
            "initializer, all four programs, end-hop handoff, and eleven compiled " +
            "spritemap selectors pass with mechanics bytes forbidden.");
    }

    /// <summary>Runs a selected instruction list through the production processor for its requested frame count.</summary>
    /// <param name="enemies">Room enemy system whose instruction processor executes the program.</param>
    /// <param name="process">Reflected production instruction-processing method.</param>
    /// <param name="processArguments">Argument array passed to each processor invocation.</param>
    /// <param name="corpse">Dead sidehopper slot whose instruction pointer and timer are advanced.</param>
    /// <param name="program">Entry address installed before execution.</param>
    /// <param name="frameCount">Number of program frames; one additional processor call consumes the terminal instruction.</param>
    private static void ExecuteDeadSidehopperProgram(
        RoomEnemySystem enemies,
        MethodInfo process,
        object?[] processArguments,
        RoomEnemySlot corpse,
        ushort program,
        int frameCount)
    {
        corpse.CurrentInstruction = program;
        for (int frame = 0; frame <= frameCount; frame++)
        {
            corpse.InstructionTimer = 1;
            process.Invoke(enemies, processArguments);
        }
    }

    /// <summary>Repeatedly reads representative mechanics words so warmed lookup allocation can be measured.</summary>
    /// <returns>A checksum that keeps the repeated reads observable.</returns>
    private static int ProbeDeadSidehopperInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += DeadSidehopperInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? DeadSidehopperInstructionProgramDefinitions.AliveHopping
                    : DeadSidehopperInstructionProgramDefinitions.HoppingSleepOpcode);
        }
        return checksum;
    }

    /// <summary>Reads a little-endian word from the Dead sidehopper's bank A9 instruction data.</summary>
    /// <param name="source">Retail address space containing bank A9.</param>
    /// <param name="address">Offset of the word's low byte within bank A9.</param>
    /// <returns>The word formed from the addressed byte and its successor.</returns>
    private static ushort ReadDeadSidehopperInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa90000 | address) |
            source.ReadByte(0xa90000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Guards production execution against reads of compiled mechanics and records presentation reads.</summary>
    /// <param name="source">Address space receiving reads that pass the mechanics guard.</param>
    private sealed class DeadSidehopperInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets presentation-word offsets whose bytes production execution requested.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Gets the number of attempts to read bytes represented by compiled mechanics.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes a cartridge-byte request through the guard's mechanics check.</summary>
        /// <param name="address">Cartridge bus address to read.</param>
        /// <returns>The byte returned by the wrapped address space if allowed.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled-mechanics reads, records presentation reads, and forwards permitted bytes.</summary>
        /// <param name="address">Bus address of the requested byte.</param>
        /// <returns>The requested byte when the guard permits the read.</returns>
        public byte ReadByte(int address)
        {
            if (DeadSidehopperInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Dead sidehopper mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa90000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < DeadSidehopperInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        DeadSidehopperInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }

            return source.ReadByte(address);
        }

        /// <summary>Forwards a write unchanged to the wrapped address space.</summary>
        /// <param name="address">Bus address receiving the write.</param>
        /// <param name="value">Byte to store at <paramref name="address"/>.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
