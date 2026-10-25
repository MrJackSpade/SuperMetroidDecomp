using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Areas.Tourian;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

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
        foreach (ushort parameter in new ushort[] { 0, 2, 4 })
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = InstalledFixtures.EnemyTiles, BossArtwork = InstalledFixtures.BossArtwork, AreaArtwork = InstalledFixtures.AreaArtwork };
            enemies.Presentation.BindColours(new GameplayColours(new SpriteColourSets()));
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.TourianStatue, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
            slot.SetSpawnParameter1(parameter);
            enemies.TourianEntranceStatueBehavior.InitializeTourianEntranceStatue(slot);
            AssertEqual(
                AnimationProgramCursor<TourianEntranceStatueProgram>.Start(TourianEntranceStatueAnimationDefinitions.InitialProgram(parameter)),
                enemies.TourianEntranceStatueBehavior.Extensions[slot.SlotIndex].Program,
                $"Tourian entrance-statue initializer parameter {parameter}");

            EnemyInstructionInterpreter.ProcessInstructions(slot, null, null, Position16<RoomPixels>.Zero, 0);
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
