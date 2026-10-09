using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Loads the retail ROM and runs the Shitroid instruction-definition verification against its cartridge tables.
    /// </summary>
    private static void VerifyShitroidInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyShitroidInstructionProgramDefinitions), () => VerifyShitroidInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Compares compiled Shitroid mechanics and presentation data with the ROM, then exercises the production instruction runner while guarding migrated reads.
    /// </summary>
    /// <param name="rom">The retail cartridge address space used as the independent reference for original instruction data.</param>
    private static void VerifyShitroidInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < ShitroidInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                ShitroidInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadShitroidInstructionWord(rom, definition.Address),
                $"Shitroid mechanics word $A9:{definition.Address:X4}");
        }

        for (int index = 0; index < ShitroidInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = ShitroidInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(ShitroidVisualDefinitions.Bank, address, out ushort selector),
                "Shitroid presentation operand has an installed visual identity");
            AssertEqual(ReadShitroidInstructionWord(rom, address), selector,
                $"Shitroid native visual operand $A9:{address:X4}");
        }
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        ushort randomNumber = 0;
        var guard = new ShitroidInstructionReadGuard(rom);
        var executedOperands = new HashSet<ushort>();
        var enemies = new RoomEnemySystem { TileArtwork = RepositoryInstallation.EnemyTiles };
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        type.GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
        type.GetField("_readRandomNumber", flags)!.SetValue(
            enemies,
            (Func<ushort>)(() => randomNumber));
        var initialize = type.GetMethod("InitializeShitroid", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ushort>>(enemies);
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot shitroid = enemies.Slots[0];
        shitroid.EnemyDefinitionPointer = RoomEnemySystem.ShitroidDefinition;
        shitroid.Definition = default(RoomEnemyDefinition) with { Bank = 0xa9 };
        initialize(shitroid, 0);
        AssertEqual(ShitroidInstructionProgramDefinitions.Normal,
            shitroid.CurrentInstruction,
            "real Shitroid initializer installs compiled normal program");

        object?[] processArguments =
            [shitroid, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        ExecuteShitroidProgram(
            rom, executedOperands, enemies,
            process,
            processArguments,
            shitroid,
            ShitroidInstructionProgramDefinitions.FinishDraining,
            callCount: 2);
        AssertEqual(ShitroidInstructionProgramDefinitions.Normal,
            shitroid.CurrentInstruction,
            "finish-draining program falls through to normal program");

        ExecuteShitroidProgram(
            rom, executedOperands, enemies,
            process,
            processArguments,
            shitroid,
            ShitroidInstructionProgramDefinitions.Normal,
            callCount: 5);
        AssertEqual(unchecked((ushort)(ShitroidInstructionProgramDefinitions.Normal + 4)),
            shitroid.CurrentInstruction,
            "normal callback loops and installs its first repeated frame");

        ExecuteShitroidProgram(
            rom, executedOperands, enemies,
            process,
            processArguments,
            shitroid,
            ShitroidInstructionProgramDefinitions.LatchedOn,
            callCount: 5);
        AssertEqual(unchecked((ushort)(ShitroidInstructionProgramDefinitions.LatchedOn + 4)),
            shitroid.CurrentInstruction,
            "latched callback loops and installs its first repeated frame");

        randomNumber = 0;
        ExecuteShitroidProgram(
            rom, executedOperands, enemies,
            process,
            processArguments,
            shitroid,
            ShitroidInstructionProgramDefinitions.Remorse,
            callCount: 9);
        AssertEqual(unchecked((ushort)(ShitroidInstructionProgramDefinitions.Remorse + 4)),
            shitroid.CurrentInstruction,
            "clear-high-bit remorse branch follows its compiled loop target");

        randomNumber = 0x8000;
        ExecuteShitroidProgram(
            rom, executedOperands, enemies,
            process,
            processArguments,
            shitroid,
            ShitroidInstructionProgramDefinitions.Remorse,
            callCount: 21);
        AssertEqual(unchecked((ushort)(ShitroidInstructionProgramDefinitions.Remorse + 4)),
            shitroid.CurrentInstruction,
            "set-high-bit remorse path plays every trailing frame and loops");
        AssertEqual((ushort?)0x0052, enemies.LastShitroidSoundEffectLibrary2,
            "set-high-bit remorse branch publishes native Shitroid cry");

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Shitroid sprite selection performs zero live cartridge reads");
        AssertEqual(ShitroidInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "all Shitroid visual operands execute");
        for (int index = 0; index < ShitroidInstructionProgramDefinitions.PresentationWordCount; index++)
            AssertTrue(executedOperands.Contains(ShitroidInstructionProgramDefinitions.PresentationWordAddress(index)),
                "every Shitroid program frame executes with its native selector");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Shitroid mechanics byte");
        AssertThrows<InvalidDataException>(
            () => ShitroidInstructionProgramDefinitions.ReadMechanicsWord(
                ShitroidInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Shitroid spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => ShitroidInstructionProgramDefinitions.ReadMechanicsWord(
                ShitroidInstructionProgramDefinitions.FirstAdjacentCallbackCode),
            "adjacent Shitroid callback code is rejected as instruction mechanics");

        _ = ProbeShitroidInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeShitroidInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Shitroid allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Shitroid mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Shitroid instruction mechanics: thirty-five compiled words, the real " +
            "initializer, finish-draining fallthrough, normal/latched loops, both remorse " +
            "branches, native cry, and thirty native sprite selections pass with mechanics " +
            "bytes forbidden and zero live visual operand reads.");
    }

    /// <summary>
    /// Runs a selected Shitroid instruction sequence for a fixed number of callbacks and checks each executed visual operand against the cartridge.
    /// </summary>
    /// <param name="rom">The cartridge reference used to validate emitted spritemap pointers.</param>
    /// <param name="executedOperands">Collects the addresses of visual operands reached during execution.</param>
    /// <param name="enemies">The enemy system whose instruction processor will be invoked.</param>
    /// <param name="process">The instruction-processing method bound to <paramref name="enemies"/>.</param>
    /// <param name="processArguments">The argument array passed to each reflected processor invocation.</param>
    /// <param name="shitroid">The slot whose instruction pointer and timer are advanced.</param>
    /// <param name="program">The starting address of the compiled Shitroid sequence.</param>
    /// <param name="callCount">The number of processor callbacks to perform.</param>
    private static void ExecuteShitroidProgram(
        SuperMetroidAddressSpace rom,
        HashSet<ushort> executedOperands,
        RoomEnemySystem enemies,
        MethodInfo process,
        object?[] processArguments,
        RoomEnemySlot shitroid,
        ushort program,
        int callCount)
    {
        shitroid.CurrentInstruction = program;
        for (int call = 0; call < callCount; call++)
        {
            shitroid.InstructionTimer = 1;
            process.Invoke(enemies, processArguments);
            if (shitroid.InstructionTimer != 0)
            {
                ushort operand = unchecked((ushort)(shitroid.CurrentInstruction - 2));
                executedOperands.Add(operand);
                AssertEqual(ReadShitroidInstructionWord(rom, operand), shitroid.SpritemapPointer,
                    $"Shitroid executed visual operand $A9:{operand:X4} matches the cartridge");
            }
        }
    }

    /// <summary>
    /// Repeatedly reads compiled Shitroid mechanics words to provide a warmed, non-zero checksum for allocation measurement.
    /// </summary>
    /// <returns>The accumulated value, which keeps the lookup work observable to the caller.</returns>
    private static int ProbeShitroidInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ShitroidInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? ShitroidInstructionProgramDefinitions.Normal
                    : ShitroidInstructionProgramDefinitions.RemorseLoopOpcode);
        }
        return checksum;
    }

    /// <summary>
    /// Reads adjacent bytes in bank A9 and combines them using the cartridge's little-endian word layout.
    /// </summary>
    /// <param name="source">The cartridge address space to read.</param>
    /// <param name="address">The 16-bit bank-relative address of the word's low byte.</param>
    /// <returns>The unsigned 16-bit value stored at the requested bank-relative address.</returns>
    private static ushort ReadShitroidInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa90000 | address) |
            source.ReadByte(0xa90000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>
    /// Audits Shitroid execution by rejecting reads of compiled mechanics bytes and recording reads of native presentation operands.
    /// </summary>
    /// <param name="source">The underlying address space that receives all permitted cartridge accesses.</param>
    private sealed class ShitroidInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>
        /// Gets the compiled presentation-word addresses whose bytes have been requested during guarded execution.
        /// </summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>
        /// Gets the number of attempts to read a byte belonging to the compiled mechanics table.
        /// </summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>
        /// Routes an importer cartridge read through the Shitroid access audit.
        /// </summary>
        /// <param name="address">The cartridge address requested by the importer.</param>
        /// <returns>The byte at the address when it is not a forbidden compiled mechanics byte.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>
        /// Rejects reads from compiled mechanics data, tracks presentation operands in bank A9, and forwards allowed reads.
        /// </summary>
        /// <param name="address">The cartridge address to inspect and read.</param>
        /// <returns>The byte returned by the wrapped address space for an allowed access.</returns>
        public byte ReadByte(int address)
        {
            if (ShitroidInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Shitroid mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa90000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < ShitroidInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        ShitroidInstructionProgramDefinitions.PresentationWordAddress(index);
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

        /// <summary>
        /// Forwards a write to the wrapped address space without changing its address or value.
        /// </summary>
        /// <param name="address">The address receiving the write.</param>
        /// <param name="value">The byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
