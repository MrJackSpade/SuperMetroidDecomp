using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    /// <summary>Verifies the Tourian entrance-statue instruction definitions against the retail ROM.</summary>
    private static void VerifyTourianEntranceStatueInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyTourianEntranceStatueInstructionProgramDefinitions), () => VerifyTourianEntranceStatueInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks compiled mechanics data and confirms production selects and executes the expected native delete programs.</summary>
    /// <param name="rom">Retail address space used to compare the compiled words with their cartridge bytes.</param>
    private static void VerifyTourianEntranceStatueInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        AssertEqual(3,
            TourianEntranceStatueInstructionProgramDefinitions.MechanicsWordCount,
            "Tourian entrance-statue mechanics word count");
        for (int index = 0;
             index < TourianEntranceStatueInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                TourianEntranceStatueInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadTourianEntranceStatueInstructionWord(rom, definition.Address),
                $"Tourian entrance-statue mechanics word $AA:{definition.Address:X4}");
        }

        using var paletteStream = new MemoryStream(
            SuperMetroid.AssetExtraction.TourianStatueColorExtractor.Extract(rom), writable: false);
        EnemyTileArtworkCatalog artwork = EnemyTileArtworkCatalog.FromArtworkForVerification(
            new Dictionary<ushort, RoomCharacterAtlas>(),
            new Dictionary<ushort, EnemyPaletteSheet>(),
            tourianStatueColors: TourianStatueColorCatalog.Load(paletteStream));
        var guard = new TourianEntranceStatueInstructionReadGuard(rom);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeTourianEntranceStatue",
            flags)!;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        foreach (ushort parameter in new ushort[] { 0, 2, 4 })
        {
            var enemies = new RoomEnemySystem { TileArtwork = RepositoryInstallation.EnemyTiles };
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.TourianEntranceStatueDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xaa };
            slot.Parameter1 = parameter;
            initialize.Invoke(enemies, [slot]);
            AssertEqual(
                TourianEntranceStatueInstructionProgramDefinitions.GetInitialInstruction(
                    parameter),
                slot.CurrentInstruction,
                $"Tourian entrance-statue initializer parameter {parameter}");

            process.Invoke(
                enemies,
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0]);
            AssertTrue(slot.Properties.HasAny(EnemyProperties.Deleted),
                $"Tourian entrance-statue parameter {parameter} executes native delete");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Tourian entrance-statue production execution avoids compiled mechanics bytes");

        AssertThrows<InvalidDataException>(
            () => TourianEntranceStatueInstructionProgramDefinitions.ReadMechanicsWord(
                TourianEntranceStatueInstructionProgramDefinitions.AdjacentUnusedProgram),
            "unused Tourian entrance-statue presentation program is rejected as mechanics");

        _ = ProbeTourianEntranceStatueInstructionAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeTourianEntranceStatueInstructionAllocation();
        AssertTrue(checksum != 0,
            "Tourian entrance-statue allocation probe consumes compiled data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Tourian entrance-statue mechanics lookups allocate no storage");

        Console.WriteLine(
            "Tourian entrance-statue instruction mechanics: three compiled delete " +
            "programs and all three production initializer selections pass.");
    }

    /// <summary>Exercises warmed compiled mechanics lookups so their checksum and managed allocation cost can be checked.</summary>
    /// <returns>The accumulated value of alternating Ridley and base-decoration mechanics words.</returns>
    private static int ProbeTourianEntranceStatueInstructionAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += TourianEntranceStatueInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? TourianEntranceStatueInstructionProgramDefinitions.Ridley
                    : TourianEntranceStatueInstructionProgramDefinitions.BaseDecoration);
        }
        return checksum;
    }

    /// <summary>Reads one little-endian mechanics word from the Tourian entrance-statue program bank.</summary>
    /// <param name="source">Address space containing the retail program bytes.</param>
    /// <param name="address">The low-byte address of the word within bank $AA.</param>
    /// <returns>The two consecutive bytes combined with the low byte first.</returns>
    private static ushort ReadTourianEntranceStatueInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xaa0000 | address) |
            source.ReadByte(0xaa0000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Wraps cartridge access to detect production reads from bytes represented by compiled mechanics data.</summary>
    /// <param name="source">Underlying address space used for reads that are permitted and for all writes.</param>
    private sealed class TourianEntranceStatueInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets the number of attempted reads from bytes that should be supplied by compiled mechanics data.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Reads a byte through the same compiled-mechanics access guard as ordinary address-space reads.</summary>
        /// <param name="address">The cartridge address requested by the importer.</param>
        /// <returns>The byte from the underlying source when the address is not guarded.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads of compiled mechanics bytes and delegates all other addresses to the wrapped source.</summary>
        /// <param name="address">The address to inspect and, when allowed, read.</param>
        /// <returns>The byte returned by the wrapped source for an allowed address.</returns>
        public byte ReadByte(int address)
        {
            if (TourianEntranceStatueInstructionProgramDefinitionsTooling
                .IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Tourian entrance-statue mechanics byte " +
                    $"${address:X6}.");
            }
            return source.ReadByte(address);
        }

        /// <summary>Passes every write through unchanged because this guard only observes reads.</summary>
        /// <param name="address">The destination address.</param>
        /// <param name="value">The byte to write at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
