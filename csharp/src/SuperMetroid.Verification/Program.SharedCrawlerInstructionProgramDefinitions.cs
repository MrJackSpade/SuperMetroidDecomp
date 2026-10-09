using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Loads the retail ROM and verifies the compiled shared-crawler mechanics and movement loops.</summary>
    private static void VerifySharedCrawlerInstructionProgramDefinitions()
    {
        Suite(nameof(VerifySharedCrawlerInstructionProgramDefinitions), () => VerifySharedCrawlerInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks bank-A3 mechanics and runs each crawler definition over every surface orientation.</summary>
    /// <param name="rom">Retail address space containing the native shared-crawler instruction data.</param>
    /// <param name="artwork">Optional installed artwork catalog supplied to the production enemy system.</param>
    private static void VerifySharedCrawlerInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog? artwork = null)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < SharedCrawlerInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                SharedCrawlerInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadSharedCrawlerInstructionWord(rom, definition.Address),
                $"shared-crawler mechanics word $A3:{definition.Address:X4}");
        }

        ushort[] enemyDefinitions =
        [
            RoomEnemySystem.ZeelaDefinition,
            RoomEnemySystem.SovaDefinition,
            RoomEnemySystem.ZoomerDefinition,
            RoomEnemySystem.StoneZoomerDefinition,
        ];
        var guard = new SharedCrawlerInstructionReadGuard(
            rom, forbidPresentation: true);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod("InitializeCrawler", flags)!;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        foreach (ushort enemyDefinition in enemyDefinitions)
        foreach (CrawlerSurfaceOrientation orientation in Enum.GetValues<CrawlerSurfaceOrientation>())
        {
            var enemies = new RoomEnemySystem { TileArtwork = artwork };
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = enemyDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            slot.CurrentInstruction = (ushort)orientation;
            slot.Parameter1 = 0;
            initialize.Invoke(enemies,
                [slot, CrawlerAnimationFamily.Shared, null]);

            ushort entry = CrawlerAnimationDefinitions.InitialInstruction(
                CrawlerAnimationFamily.Shared, orientation);
            AssertEqual(entry, slot.CurrentInstruction,
                $"shared crawler ${enemyDefinition:X4} selects {orientation}");
            ExecuteSharedCrawlerProgram(enemies, process, slot, callCount: 6);
            CrawlerEnemyFunction expected = orientation is
                CrawlerSurfaceOrientation.UpsideRight or
                CrawlerSurfaceOrientation.UpsideLeft
                    ? CrawlerEnemyFunction.CrawlingVertically
                    : CrawlerEnemyFunction.CrawlingHorizontally;
            AssertEqual(expected, enemies.CrawlerStates[0]!.Function,
                $"shared crawler ${enemyDefinition:X4}/{orientation} publishes native movement");
            AssertEqual(unchecked((ushort)(entry + 8)), slot.CurrentInstruction,
                $"shared crawler ${enemyDefinition:X4}/{orientation} loops all five frames");
        }

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "all shared-crawler loops avoid cartridge visual selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled shared-crawler mechanics byte");

        AssertThrows<InvalidDataException>(
            () => SharedCrawlerInstructionProgramDefinitions.ReadMechanicsWord(
                SharedCrawlerInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "shared-crawler spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => SharedCrawlerInstructionProgramDefinitions.ReadMechanicsWord(
                SharedCrawlerInstructionProgramDefinitions.AdjacentInitialSelectorTable),
            "adjacent shared-crawler selector table is rejected as mechanics");

        _ = ProbeSharedCrawlerInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeSharedCrawlerInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "shared-crawler allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed shared-crawler mechanics lookups allocate no per-frame storage");

        Console.WriteLine(artwork is null
            ? "Shared-crawler instruction mechanics: thirty-six compiled words, four enemy " +
              "definitions across every surface loop, and twenty compiled visual selectors."
            : "Installed shared crawlers: four definitions across four surface loops " +
              "execute without cartridge visual-selector reads.");
    }

    /// <summary>Advances one initialized crawler instruction list through the production processor.</summary>
    /// <param name="enemies">Room enemy system containing the crawler and processor.</param>
    /// <param name="process">Reflected production instruction-processing method.</param>
    /// <param name="slot">Crawler slot whose instruction timer is advanced.</param>
    /// <param name="callCount">Number of processor invocations to perform.</param>
    private static void ExecuteSharedCrawlerProgram(
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

    /// <summary>Repeatedly reads representative compiled words for the warmed-allocation check.</summary>
    /// <returns>A checksum that keeps the repeated lookups observable.</returns>
    private static int ProbeSharedCrawlerInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += SharedCrawlerInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? SharedCrawlerInstructionProgramDefinitions.UpsideRight
                    : SharedCrawlerInstructionProgramDefinitions.UpsideUp);
        }
        return checksum;
    }

    /// <summary>Reads a little-endian instruction word from the shared-crawler bank A3.</summary>
    /// <param name="source">Retail address space containing bank A3.</param>
    /// <param name="address">Offset of the low byte within bank A3.</param>
    /// <returns>The word formed by the addressed byte and its successor.</returns>
    private static ushort ReadSharedCrawlerInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa30000 | address) |
            source.ReadByte(0xa30000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Detects reads of compiled crawler mechanics and records or rejects cartridge presentation reads.</summary>
    /// <param name="source">Address space receiving reads and writes permitted by the guard.</param>
    /// <param name="forbidPresentation">Whether presentation-selector reads should throw and count as forbidden.</param>
    private sealed class SharedCrawlerInstructionReadGuard(
        ISnesAddressSpace source, bool forbidPresentation = false) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets presentation-word offsets whose bytes were requested while reads were allowed.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Gets attempts to read compiled mechanics or presentation bytes configured as forbidden.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes a cartridge-byte request through the guard's read policy.</summary>
        /// <param name="address">Cartridge bus address to read.</param>
        /// <returns>The byte returned by the wrapped source when permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled-mechanics reads, records or rejects presentation reads, and forwards other bytes.</summary>
        /// <param name="address">Bus address of the requested byte.</param>
        /// <returns>The byte returned by the wrapped source when the guard permits the read.</returns>
        public byte ReadByte(int address)
        {
            if (SharedCrawlerInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled shared-crawler mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < SharedCrawlerInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        SharedCrawlerInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        if (forbidPresentation)
                        {
                            ForbiddenReadAttempts++;
                            throw new InvalidOperationException(
                                $"Shared crawler read visual-selector byte ${address:X6}.");
                        }
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
