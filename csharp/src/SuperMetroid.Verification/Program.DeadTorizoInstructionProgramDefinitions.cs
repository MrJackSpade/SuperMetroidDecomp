using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Verifies the compiled Dead Torizo instruction definitions against the retail ROM
    /// and exercises their use by the production enemy instruction processor.
    /// </summary>
    private static void VerifyDeadTorizoInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyDeadTorizoInstructionProgramDefinitions), () => VerifyDeadTorizoInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Checks compiled mechanics words against the cartridge, then confirms that the real
    /// Dead Torizo initializer and instruction loop use the compiled program without reading
    /// those mechanics bytes at runtime.
    /// </summary>
    /// <param name="rom">Retail address space supplying the reference mechanics words.</param>
    private static void VerifyDeadTorizoInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < DeadTorizoInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                DeadTorizoInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadDeadTorizoInstructionWord(rom, definition.Address),
                $"Dead Torizo mechanics word $A9:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new DeadTorizoInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem { TileArtwork = RepositoryInstallation.EnemyTiles };
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = type.GetMethod("InitializeDeadTorizo", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot corpse = enemies.Slots[0];
        corpse.EnemyDefinitionPointer = RoomEnemySystem.DeadTorizoDefinition;
        corpse.Definition = default(RoomEnemyDefinition) with { Bank = 0xa9 };
        initialize(corpse);
        AssertEqual(DeadTorizoInstructionProgramDefinitions.Stationary,
            corpse.CurrentInstruction,
            "real Dead Torizo initializer installs compiled stationary program");

        object?[] processArguments =
            [corpse, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        corpse.InstructionTimer = 1;
        process.Invoke(enemies, processArguments);
        corpse.InstructionTimer = 1;
        process.Invoke(enemies, processArguments);
        AssertEqual(DeadTorizoInstructionProgramDefinitions.SleepOpcode,
            corpse.CurrentInstruction,
            "Dead Torizo program reaches terminal sleep");
        AssertEqual(ReadDeadTorizoInstructionWord(
                rom, DeadTorizoInstructionProgramDefinitionsTooling.PresentationWord),
            corpse.SpritemapPointer,
            "compiled Dead Torizo spritemap selector matches the cartridge");
        AssertTrue(!guard.SawPresentationWord,
            "Dead Torizo execution uses the compiled spritemap selector");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids both compiled Dead Torizo mechanics words");
        AssertThrows<InvalidDataException>(
            () => DeadTorizoInstructionProgramDefinitions.ReadMechanicsWord(
                DeadTorizoInstructionProgramDefinitionsTooling.PresentationWord),
            "Dead Torizo spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => DeadTorizoInstructionProgramDefinitions.ReadMechanicsWord(
                DeadTorizoInstructionProgramDefinitions.FirstAdjacentPresentationData),
            "adjacent Dead Torizo spritemap data is rejected as instruction mechanics");

        _ = ProbeDeadTorizoInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeDeadTorizoInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Dead Torizo allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Dead Torizo mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Dead Torizo instruction mechanics: two compiled words, the real initializer, " +
            "terminal sleep, and the compiled spritemap selector pass with mechanics bytes forbidden.");
    }

    /// <summary>
    /// Repeatedly reads both supported instruction words so the verification can measure
    /// whether warmed mechanics lookups allocate on the current thread.
    /// </summary>
    /// <returns>A checksum that keeps the lookup results observable to the caller.</returns>
    private static int ProbeDeadTorizoInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += DeadTorizoInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? DeadTorizoInstructionProgramDefinitions.Stationary
                    : DeadTorizoInstructionProgramDefinitions.SleepOpcode);
        }
        return checksum;
    }

    /// <summary>Reads a little-endian instruction word from the Dead Torizo bank-$A9 address space.</summary>
    /// <param name="source">Cartridge address space containing the native instruction bytes.</param>
    /// <param name="address">Bank-local address of the word's low byte.</param>
    /// <returns>The low byte followed by the next address's byte as a 16-bit value.</returns>
    private static ushort ReadDeadTorizoInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa90000 | address) |
            source.ReadByte(0xa90000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>
    /// Wraps the cartridge bus to detect forbidden production reads of compiled mechanics
    /// words and to record any access to the presentation selector.
    /// </summary>
    /// <param name="source">Underlying address space used for permitted reads and all writes.</param>
    private sealed class DeadTorizoInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets whether production requested either byte of the presentation word.</summary>
        internal bool SawPresentationWord { get; private set; }

        /// <summary>Gets the number of attempted reads from mechanics bytes guarded against runtime access.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Forwards an import-time cartridge read through the same guarded address-space path.</summary>
        /// <param name="address">Absolute address requested by the cartridge importer.</param>
        /// <returns>The byte returned by the wrapped address space, unless the address is forbidden.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>
        /// Rejects accesses to mechanics bytes that should come from compiled definitions,
        /// records presentation-selector reads, and forwards other reads to the source bus.
        /// </summary>
        /// <param name="address">Absolute address requested by the production code.</param>
        /// <returns>The byte at <paramref name="address"/> when the read is permitted.</returns>
        /// <exception cref="InvalidOperationException">The read targets a compiled mechanics byte.</exception>
        public byte ReadByte(int address)
        {
            if (DeadTorizoInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Dead Torizo mechanics byte ${address:X6}.");
            }

            int presentation =
                0xa90000 | DeadTorizoInstructionProgramDefinitionsTooling.PresentationWord;
            if (address == presentation || address == presentation + 1)
                SawPresentationWord = true;
            return source.ReadByte(address);
        }

        /// <summary>Forwards a write unchanged to the wrapped address space.</summary>
        /// <param name="address">Absolute destination address.</param>
        /// <param name="value">Byte to write.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
