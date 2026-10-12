using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

/// <summary>Represents shared crawler instruction program definitions verification.</summary>
internal static class SharedCrawlerInstructionProgramDefinitionsVerification
{
    /// <summary>Verifies shared crawler instruction program definitions.</summary>
    internal static void VerifySharedCrawlerInstructionProgramDefinitions()
    {
        Suite(nameof(VerifySharedCrawlerInstructionProgramDefinitions), () => VerifySharedCrawlerInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies shared crawler instruction program definitions.</summary>
    internal static void VerifySharedCrawlerInstructionProgramDefinitions(
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

        EnemyDefinitionId[] enemyDefinitions =
        [
            EnemyDefinitionId.Zeela,
            EnemyDefinitionId.Sova,
            EnemyDefinitionId.Zoomer,
            EnemyDefinitionId.MZoomer,
        ];
        var guard = NewSharedCrawlerInstructionReadGuard(
            rom, forbidPresentation: true);
        foreach (EnemyDefinitionId enemyDefinition in enemyDefinitions)
        foreach (CrawlerSurfaceOrientation orientation in Enum.GetValues<CrawlerSurfaceOrientation>())
        {
            var enemies = new RoomEnemySystem { TileArtwork = artwork };
            enemies.BindFixtureBus(guard);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = enemyDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            slot.CurrentInstruction = (ushort)orientation;
            slot.Parameter1 = 0;
            enemies.CrawlersBehavior.InitializeCrawler(slot, CrawlerAnimationFamily.Shared, null);

            ushort entry = CrawlerAnimationDefinitions.InitialInstruction(
                CrawlerAnimationFamily.Shared, orientation);
            AssertEqual(entry, slot.CurrentInstruction,
                $"shared crawler ${(int)enemyDefinition:X4} selects {orientation}");
            ExecuteSharedCrawlerProgram(enemies, slot, callCount: 6);
            CrawlerEnemyFunction expected = orientation is
                CrawlerSurfaceOrientation.UpsideRight or
                CrawlerSurfaceOrientation.UpsideLeft
                    ? CrawlerEnemyFunction.CrawlingVertically
                    : CrawlerEnemyFunction.CrawlingHorizontally;
            AssertEqual(expected, enemies.CrawlerStates[0]!.Function,
                $"shared crawler ${(int)enemyDefinition:X4}/{orientation} publishes native movement");
            AssertEqual(unchecked((ushort)(entry + 8)), slot.CurrentInstruction,
                $"shared crawler ${(int)enemyDefinition:X4}/{orientation} loops all five frames");
        }

        AssertEqual(0, guard.ObservedWords.Count,
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

    /// <summary>Execute shared crawler program.</summary>
    private static void ExecuteSharedCrawlerProgram(
        RoomEnemySystem enemies,
        RoomEnemySlot slot,
        int callCount)
    {
        object?[] arguments =
            [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        for (int call = 0; call < callCount; call++)
        {
            slot.InstructionTimer = 1;
            EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Probes shared crawler instruction mechanics allocation.</summary>
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

    /// <summary>Reads shared crawler instruction word.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for shared crawler instruction words")]
    internal static ushort ReadSharedCrawlerInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(0xa30000 | address);

    /// <summary>Creates shared crawler instruction read guard.</summary>
    private static CartridgeReadGuard NewSharedCrawlerInstructionReadGuard(ISnesAddressSpace source, bool forbidPresentation = false)
    {
        var guard = new CartridgeReadGuard(source).Forbid(SharedCrawlerInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
            address => $"Production read compiled shared-crawler mechanics byte ${address:X6}.");
        return forbidPresentation
            ? guard.ForbidWords(0xa30000, SharedCrawlerInstructionProgramDefinitionsTooling.PresentationWordCount, SharedCrawlerInstructionProgramDefinitionsTooling.PresentationWordAddress,
                address => $"Shared crawler read visual-selector byte ${address:X6}.")
            : guard.ObserveWords(0xa30000, SharedCrawlerInstructionProgramDefinitionsTooling.PresentationWordCount, SharedCrawlerInstructionProgramDefinitionsTooling.PresentationWordAddress);
    }

    /// <summary>Shared crawler instruction program definitions command.</summary>
    [VerificationCommand("--shared-crawler-instruction-program-definitions")]
    internal static int SharedCrawlerInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifySharedCrawlerInstructionProgramDefinitions), () => VerifySharedCrawlerInstructionProgramDefinitions());
        return 0;
    }
}
