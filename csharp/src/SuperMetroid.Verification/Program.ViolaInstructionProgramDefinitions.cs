using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Registers the retail-ROM verification suite for Viola's compiled instruction mechanics.</summary>
    private static void VerifyViolaInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyViolaInstructionProgramDefinitions), () => VerifyViolaInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks compiled Viola mechanics against cartridge words and executes each surface orientation through the production initializer and instruction processor.</summary>
    /// <param name="rom">Retail address space used for reference words and visual-selector verification.</param>
    private static void VerifyViolaInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < ViolaInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                ViolaInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadViolaInstructionWord(rom, definition.Address),
                $"Viola mechanics word $A3:{definition.Address:X4}");
        }

        var guard = new ViolaInstructionReadGuard(rom);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod("InitializeCrawler", flags)!;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        foreach (CrawlerSurfaceOrientation orientation in Enum.GetValues<CrawlerSurfaceOrientation>())
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.ViolaDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            // InitAI_Viola ($A3:B678) selects with the property bits; a different
            // population parameter proves the parameter is not the source.
            slot.CurrentInstruction = (ushort)((ushort)orientation ^ 1);
            slot.Properties = (ushort)orientation;
            slot.Parameter1 = 0;
            initialize.Invoke(enemies,
                [slot, CrawlerAnimationFamily.Viola, (ushort?)6]);

            ushort entry = CrawlerAnimationDefinitions.InitialInstruction(
                CrawlerAnimationFamily.Viola, orientation);
            AssertEqual(entry, slot.CurrentInstruction,
                $"Viola real initializer selects {orientation}");
            ExecuteViolaProgram(enemies, process, slot, callCount: 15);
            CrawlerEnemyFunction expected = orientation is
                CrawlerSurfaceOrientation.UpsideRight or
                CrawlerSurfaceOrientation.UpsideLeft
                    ? CrawlerEnemyFunction.CrawlingVertically
                    : CrawlerEnemyFunction.CrawlingHorizontally;
            AssertEqual(expected, enemies.CrawlerStates[0]!.Function,
                $"Viola {orientation} publishes native movement function");
            AssertEqual(unchecked((ushort)(ViolaInstructionProgramDefinitions.NormalLoop + 4)),
                slot.CurrentInstruction,
                $"Viola {orientation} enters and loops all fourteen shared frames");
        }

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Viola execution uses compiled spritemap selectors");
        for (int index = 0;
             index < ViolaInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                ViolaInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertCompiledEnemyVisualSelector(rom, RoomEnemySystem.ViolaDefinition,
                0xa3, address, $"Viola $A3:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Viola mechanics byte");

        AssertThrows<InvalidDataException>(
            () => ViolaInstructionProgramDefinitions.ReadMechanicsWord(
                ViolaInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Viola spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => ViolaInstructionProgramDefinitions.ReadMechanicsWord(
                ViolaInstructionProgramDefinitions.UnusedXFlipped),
            "retail-unused Viola program is outside production mechanics");

        _ = ProbeViolaInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeViolaInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Viola allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Viola mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Viola instruction mechanics: thirty compiled words, all four surface entries, " +
            "the shared fourteen-frame loop, and fourteen compiled spritemap selectors pass with " +
            "mechanics bytes forbidden.");
    }

    /// <summary>Advances a Viola instruction program through the production processor for a fixed number of eligible calls.</summary>
    /// <param name="enemies">Enemy system whose production instruction processor is invoked.</param>
    /// <param name="process">Reflected production instruction-processing method.</param>
    /// <param name="slot">Viola slot whose instruction timer and current program are advanced.</param>
    /// <param name="callCount">Number of processor calls to perform.</param>
    private static void ExecuteViolaProgram(
        RoomEnemySystem enemies,
        MethodInfo process,
        RoomEnemySlot slot,
        int callCount)
    {
        object?[] arguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int call = 0; call < callCount; call++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
    }

    /// <summary>Warms and repeatedly reads compiled Viola mechanics entries so steady-state lookup allocations can be measured.</summary>
    /// <returns>A checksum that keeps the lookup results observable.</returns>
    private static int ProbeViolaInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ViolaInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? ViolaInstructionProgramDefinitions.UpsideRight
                    : ViolaInstructionProgramDefinitions.NormalLoop);
        }
        return checksum;
    }

    /// <summary>Reads a little-endian instruction mechanics word from bank $A3 for comparison with compiled data.</summary>
    /// <param name="source">Retail address space containing the original instruction program.</param>
    /// <param name="address">Bank-relative address of the low byte.</param>
    /// <returns>The adjacent cartridge bytes combined as an unsigned 16-bit word.</returns>
    private static ushort ReadViolaInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa30000 | address) |
            source.ReadByte(0xa30000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Wraps cartridge access to reject reads of compiled Viola mechanics and track any reads of compiled presentation operands.</summary>
    /// <param name="source">Underlying address space for permitted reads and writes.</param>
    private sealed class ViolaInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Unique presentation operand addresses observed during production execution.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of attempted reads rejected for targeting compiled mechanics bytes.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge-import reads through the same mechanics guard and presentation tracking as ordinary byte reads.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The underlying byte when the address is not a compiled mechanics byte.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics reads, records presentation operand reads, and forwards all other byte requests.</summary>
        /// <param name="address">Address-space location requested by production code.</param>
        /// <returns>The underlying byte when the address is outside the compiled mechanics range.</returns>
        /// <exception cref="InvalidOperationException">The request targets a compiled Viola mechanics byte.</exception>
        public byte ReadByte(int address)
        {
            if (ViolaInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Viola mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < ViolaInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        ViolaInstructionProgramDefinitions.PresentationWordAddress(index);
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

        /// <summary>Forwards writes unchanged; this wrapper guards and observes reads only.</summary>
        /// <param name="address">Address-space location to write.</param>
        /// <param name="value">Byte passed to the underlying source.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
