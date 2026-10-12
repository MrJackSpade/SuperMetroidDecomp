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

/// <summary>Represents hzoomer instruction program definitions verification.</summary>
internal static class HZoomerInstructionProgramDefinitionsVerification
{
    /// <summary>Verifies hzoomer instruction program definitions.</summary>
    internal static void VerifyHZoomerInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyHZoomerInstructionProgramDefinitions), () => VerifyHZoomerInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies hzoomer instruction program definitions.</summary>
    internal static void VerifyHZoomerInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog? artwork = null)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < HZoomerInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                HZoomerInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadHZoomerInstructionWord(rom, definition.Address),
                $"HZoomer mechanics word $A3:{definition.Address:X4}");
        }

        var guard = NewHZoomerInstructionGuard(rom);
        (CrawlerSurfaceOrientation Orientation, CrawlerEnemyFunction Function)[] cases =
        [
            (CrawlerSurfaceOrientation.UpsideRight,
                CrawlerEnemyFunction.HZoomerCrawlingVertically),
            (CrawlerSurfaceOrientation.UpsideLeft,
                CrawlerEnemyFunction.HZoomerCrawlingVertically),
            (CrawlerSurfaceOrientation.UpsideDown,
                CrawlerEnemyFunction.HZoomerCrawlingHorizontally),
            (CrawlerSurfaceOrientation.UpsideUp,
                CrawlerEnemyFunction.HZoomerCrawlingHorizontally),
        ];

        foreach ((CrawlerSurfaceOrientation orientation, CrawlerEnemyFunction function) in cases)
        {
            var enemies = new RoomEnemySystem { TileArtwork = artwork };
            enemies.BindFixtureBus(guard);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.HZoomer;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            slot.CurrentInstruction = (ushort)orientation;
            slot.Parameter1 = 0;
            enemies.CrawlersBehavior.InitializeHZoomer(slot);

            ushort entry = CrawlerAnimationDefinitions.InitialInstruction(
                CrawlerAnimationFamily.HZoomer, orientation);
            AssertEqual(entry, slot.CurrentInstruction,
                $"HZoomer real initializer selects {orientation}");
            ExecuteHZoomerProgram(enemies, slot, callCount: 6);
            AssertEqual(function, enemies.CrawlerStates[0]!.Function,
                $"HZoomer {orientation} publishes native movement function");
            AssertEqual(unchecked((ushort)(entry + 8)), slot.CurrentInstruction,
                $"HZoomer {orientation} completes and loops all five frames");
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "HZoomer production loops avoid cartridge visual selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled HZoomer mechanics byte");

        AssertThrows<InvalidDataException>(
            () => HZoomerInstructionProgramDefinitions.ReadMechanicsWord(
                HZoomerInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "HZoomer spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => HZoomerInstructionProgramDefinitions.ReadMechanicsWord(
                HZoomerInstructionProgramDefinitions.AdjacentFunctionCode),
            "adjacent HZoomer callback implementation is rejected as mechanics");

        _ = ProbeHZoomerInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeHZoomerInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "HZoomer allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed HZoomer mechanics lookups allocate no per-frame storage");

        Console.WriteLine(artwork is null
            ? "HZoomer instruction mechanics: thirty-six compiled words, four surface " +
              "loops and movement callbacks, and twenty compiled visual selectors pass."
            : "Installed HZoomer: four surface loops execute with cartridge visual " +
              "selector reads forbidden.");
    }

    /// <summary>Execute hzoomer program.</summary>
    private static void ExecuteHZoomerProgram(
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

    /// <summary>Probes hzoomer instruction mechanics allocation.</summary>
    private static int ProbeHZoomerInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += HZoomerInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? HZoomerInstructionProgramDefinitions.UpsideRight
                    : HZoomerInstructionProgramDefinitions.UpsideUp);
        }
        return checksum;
    }

    /// <summary>Reads hzoomer instruction word.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for H Zoomer instruction words")]
    internal static ushort ReadHZoomerInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(0xa30000 | address);

    /// <summary>Creates hzoomer instruction guard.</summary>
    private static CartridgeReadGuard NewHZoomerInstructionGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(HZoomerInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled HZoomer mechanics byte ${address:X6}.")
            .ForbidWords(0xa30000, HZoomerInstructionProgramDefinitionsTooling.PresentationWordCount,
                HZoomerInstructionProgramDefinitionsTooling.PresentationWordAddress,
                address => $"HZoomer read visual-selector byte ${address:X6}.");

    /// <summary>Hzoomer instruction program definitions command.</summary>
    [VerificationCommand("--hzoomer-instruction-program-definitions")]
    internal static int HzoomerInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyHZoomerInstructionProgramDefinitions), () => VerifyHZoomerInstructionProgramDefinitions());
        return 0;
    }
}
