using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Input;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Areas.Tourian;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Tourian entrance statue delete programs and their initializer selections.</summary>
internal static class TourianEntranceStatueInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for entrance statue instruction checks.</summary>
    internal static void VerifyTourianEntranceStatueInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyTourianEntranceStatueInstructionProgramDefinitions), () => VerifyTourianEntranceStatueInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares three native words and executes each statue variant with mechanics reads blocked.</summary>
    internal static void VerifyTourianEntranceStatueInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        using var paletteStream = new MemoryStream(
            SuperMetroid.AssetExtraction.TourianStatueColorExtractor.Extract(rom), writable: false);
        EnemyTileArtworkCatalog artwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(), TourianStatueColors = TourianStatueColorCatalog.Load(paletteStream) });
        foreach (ushort parameter in new ushort[] { 0, 2, 4 })
        {
            var enemies = new RoomEnemySystem { TileArtwork = InstalledFixtures.EnemyTiles };
            enemies.BindFixtureBus(rom);
            enemies.Presentation._cgram = new SnesCgram();
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.TourianStatue, slot.Definition);
            slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = 0xaa });
            slot.Parameter1 = parameter;
            enemies.TourianEntranceStatueBehavior.InitializeTourianEntranceStatue(slot);
            AssertEqual(
                AnimationProgramCursor<TourianEntranceStatueProgram>.Start(TourianEntranceStatueAnimationDefinitions.InitialProgram(parameter)),
                enemies.TourianEntranceStatueBehavior._extensions[slot.SlotIndex].Program,
                $"Tourian entrance-statue initializer parameter {parameter}");

            EnemyInstructionInterpreter.ProcessInstructions(slot, null, null, Position16<RoomPixels>.Zero, (ushort)0);
            AssertTrue(slot.Properties.HasAny(EnemyProperties.Deleted),
                $"Tourian entrance-statue parameter {parameter} executes native delete");
        }
        Suite(nameof(TourianEntranceStatueAnimationsVerification.VerifyTourianEntranceStatueAnimations),
            () => TourianEntranceStatueAnimationsVerification.VerifyTourianEntranceStatueAnimations(rom));

        Console.WriteLine(
            "Tourian entrance statue: all three typed delete programs and the production initializer selections pass.");
    }

    /// <summary>Runs the entrance statue instruction-definition command.</summary>
    [VerificationCommand("--tourian-entrance-statue-instruction-program-definitions")]
    internal static int TourianEntranceStatueInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyTourianEntranceStatueInstructionProgramDefinitions), () => VerifyTourianEntranceStatueInstructionProgramDefinitions());
        return 0;
    }
}
