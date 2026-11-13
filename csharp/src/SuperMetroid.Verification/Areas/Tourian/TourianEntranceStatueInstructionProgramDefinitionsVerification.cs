using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Animation;
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
        CartridgeImportAddressSpace rom)
    {
        foreach (ushort parameter in new ushort[] { 0, 2, 4 })
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = InstalledFixtures.BossArtwork };
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.TourianStatue, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
            slot.SetSpawnParameter1(EnemyDefinitionId.TourianStatue, parameter);
            enemies.TourianEntranceStatueBehavior.InitializeTourianEntranceStatue(slot);
            // The statues' animations stop at once and so compare equal; the one started is told apart by identity.
            AnimationPlayback<TourianEntranceStatueSegment> started = slot.RequireSpeciesState<TourianEntranceStatueState>().Playback;
            AssertTrue(ReferenceEquals(TourianEntranceStatueAnimations.StartingWith(SuperMetroid.AssetExtraction.Enemies.SpawnWordDecodes.TourianStatue(parameter)), started.Animation)
                && started.Place == AnimationPlace.StartOfAnimation,
                $"Tourian entrance-statue initializer parameter {parameter}");

            EnemyAnimationPlayer.AdvanceAnimation(slot, null, null, Position16<RoomPixels>.Zero, 0);
            AssertTrue(slot.Presence.HasAny(EnemyPresence.Deleted),
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
