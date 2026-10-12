using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuiteTimingVerification;

internal static class TourianEntranceStatueInstructionProgramDefinitionsVerification
{
    internal static void VerifyTourianEntranceStatueInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyTourianEntranceStatueInstructionProgramDefinitions), () => VerifyTourianEntranceStatueInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    internal static void VerifyTourianEntranceStatueInstructionProgramDefinitions(
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
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xaa0000 | definition.Address),
                $"Tourian entrance-statue mechanics word $AA:{definition.Address:X4}");
        }

        using var paletteStream = new MemoryStream(
            SuperMetroid.AssetExtraction.TourianStatueColorExtractor.Extract(rom), writable: false);
        EnemyTileArtworkCatalog artwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(), TourianStatueColors = TourianStatueColorCatalog.Load(paletteStream) });
        var guard = NewTourianEntranceStatueInstructionReadGuard(rom);
        foreach (ushort parameter in new ushort[] { 0, 2, 4 })
        {
            var enemies = new RoomEnemySystem { TileArtwork = InstalledFixtures.EnemyTiles };
            enemies.BindFixtureBus(guard);
            enemies.Presentation._cgram = new SnesCgram();
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.TourianStatue;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xaa };
            slot.Parameter1 = parameter;
            enemies.TourianEntranceStatueBehavior.InitializeTourianEntranceStatue(slot);
            AssertEqual(
                TourianEntranceStatueInstructionProgramDefinitions.GetInitialInstruction(
                    parameter),
                slot.CurrentInstruction,
                $"Tourian entrance-statue initializer parameter {parameter}");

            enemies.Instructions.ProcessInstructions(slot, null, null, Position16<RoomPixels>.Zero, (ushort)0);
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

    private static CartridgeReadGuard NewTourianEntranceStatueInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(TourianEntranceStatueInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
            address => $"Production read compiled Tourian entrance-statue mechanics byte " +
                $"${address:X6}.");

    [VerificationCommand("--tourian-entrance-statue-instruction-program-definitions")]
    internal static int TourianEntranceStatueInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyTourianEntranceStatueInstructionProgramDefinitions), () => VerifyTourianEntranceStatueInstructionProgramDefinitions());
        return 0;
    }
}
