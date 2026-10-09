using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Loads the retail ROM and runs the Dead Tourian corpse instruction-definition verification against its cartridge data.
    /// </summary>
    private static void VerifyDeadTourianCorpseInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyDeadTourianCorpseInstructionProgramDefinitions), () => VerifyDeadTourianCorpseInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Checks compiled mechanics against ROM words and executes each real corpse variant while guarding migrated mechanics reads.
    /// </summary>
    /// <param name="rom">The retail cartridge address space used as the reference for original instruction data and visual selectors.</param>
    private static void VerifyDeadTourianCorpseInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < DeadTourianCorpseInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                DeadTourianCorpseInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadDeadTourianCorpseInstructionWord(rom, definition.Address),
                $"Dead Tourian corpse mechanics word $A9:{definition.Address:X4}");
        }

        (DeadTourianCorpseSpecies Species, ushort EnemyDefinition, int VariantCount)[] families =
        [
            (DeadTourianCorpseSpecies.Zoomer, RoomEnemySystem.DeadZoomerDefinition, 3),
            (DeadTourianCorpseSpecies.Ripper, RoomEnemySystem.DeadRipperDefinition, 2),
            (DeadTourianCorpseSpecies.Skree, RoomEnemySystem.DeadSkreeDefinition, 3),
        ];

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new DeadTourianCorpseInstructionReadGuard(rom);
        int programIndex = 0;
        foreach (var family in families)
        {
            for (int variantIndex = 0; variantIndex < family.VariantCount; variantIndex++)
            {
                var enemies = new RoomEnemySystem { TileArtwork = LookupStream5CorpseFixtureArtwork(rom) };
                Type type = typeof(RoomEnemySystem);
                type.GetField("_bus", flags)!.SetValue(enemies, guard);
                var initialize = type.GetMethod("InitializeDeadTourianCorpse", flags)!
                    .CreateDelegate<Action<RoomEnemySlot>>(enemies);
                MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

                RoomEnemySlot corpse = enemies.Slots[0];
                corpse.EnemyDefinitionPointer = family.EnemyDefinition;
                corpse.Definition = default(RoomEnemyDefinition) with { Bank = 0xa9 };
                corpse.Parameter1 = checked((ushort)(variantIndex * 2));
                initialize(corpse);

                ushort program = DeadTourianCorpseInstructionProgramDefinitions.Program(
                    programIndex);
                AssertEqual(program, corpse.CurrentInstruction,
                    $"real dead {family.Species} variant {variantIndex} initializer program");

                object?[] processArguments =
                    [corpse, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
                for (int call = 0; call < 2; call++)
                {
                    corpse.InstructionTimer = 1;
                    process.Invoke(enemies, processArguments);
                }
                AssertEqual(
                    DeadTourianCorpseInstructionProgramDefinitions.SleepWordAddress(programIndex),
                    corpse.CurrentInstruction,
                    $"dead {family.Species} variant {variantIndex} reaches terminal sleep");
                ushort operand = DeadTourianCorpseInstructionProgramDefinitionsTooling
                    .PresentationWordAddress(programIndex);
                AssertCompiledEnemyVisualSelector(rom, family.EnemyDefinition, 0xa9,
                    operand, $"dead {family.Species} variant {variantIndex}");
                programIndex++;
            }
        }

        AssertEqual(DeadTourianCorpseInstructionProgramDefinitions.ProgramCount,
            programIndex,
            "every Dead Tourian corpse program executes");
        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Dead Tourian corpse execution uses compiled spritemap selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Dead Tourian corpse mechanics byte");
        AssertThrows<InvalidDataException>(
            () => DeadTourianCorpseInstructionProgramDefinitions.ReadMechanicsWord(
                DeadTourianCorpseInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "Dead Tourian corpse spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => DeadTourianCorpseInstructionProgramDefinitions.ReadMechanicsWord(
                DeadTourianCorpseInstructionProgramDefinitions.FirstAdjacentPresentationData),
            "adjacent corpse spritemap data is rejected as instruction mechanics");

        _ = ProbeDeadTourianCorpseInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeDeadTourianCorpseInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "Dead Tourian corpse allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Dead Tourian corpse mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Dead Tourian corpse instruction mechanics: sixteen compiled words, all eight " +
            "real Zoomer/Ripper/Skree initializers and terminal sleeps, and eight compiled " +
            "spritemap selectors pass with mechanics bytes forbidden.");
    }

    /// <summary>
    /// Repeatedly looks up compiled corpse mechanics words to warm the lookup path and produce a non-zero allocation-probe checksum.
    /// </summary>
    /// <returns>The accumulated mechanics values consumed by the probe.</returns>
    private static int ProbeDeadTourianCorpseInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += DeadTourianCorpseInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? DeadTourianCorpseInstructionProgramDefinitions.Zoomer0
                    : DeadTourianCorpseInstructionProgramDefinitions.SleepWordAddress(7));
        }
        return checksum;
    }

    /// <summary>
    /// Reads adjacent bytes in bank A9 and combines them as one little-endian corpse instruction word.
    /// </summary>
    /// <param name="source">The cartridge address space that supplies the reference bytes.</param>
    /// <param name="address">The bank-relative address of the word's low byte.</param>
    /// <returns>The unsigned 16-bit word stored at the requested address.</returns>
    private static ushort ReadDeadTourianCorpseInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa90000 | address) |
            source.ReadByte(0xa90000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>
    /// Audits corpse execution by rejecting reads from compiled mechanics data and recording reads of presentation selector words.
    /// </summary>
    /// <param name="source">The wrapped address space used for all permitted cartridge reads and writes.</param>
    private sealed class DeadTourianCorpseInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>
        /// Routes an importer read through the corpse mechanics guard.
        /// </summary>
        /// <param name="address">The cartridge address requested by the importer.</param>
        /// <returns>The byte at the address when it is not part of compiled mechanics data.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>
        /// Gets the compiled presentation-word addresses observed while the production instruction runner is executing.
        /// </summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>
        /// Gets the number of attempts to read a byte in the compiled corpse mechanics range.
        /// </summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>
        /// Rejects compiled mechanics accesses, records presentation-word reads, and forwards permitted addresses to the wrapped bus.
        /// </summary>
        /// <param name="address">The bus address being read.</param>
        /// <returns>The byte returned by the wrapped address space for an allowed read.</returns>
        public byte ReadByte(int address)
        {
            if (DeadTourianCorpseInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Dead Tourian corpse mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa90000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < DeadTourianCorpseInstructionProgramDefinitions.ProgramCount;
                     index++)
                {
                    ushort presentation =
                        DeadTourianCorpseInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
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
        /// Forwards a write to the wrapped address space without changing its destination or value.
        /// </summary>
        /// <param name="address">The destination bus address.</param>
        /// <param name="value">The byte to write.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
