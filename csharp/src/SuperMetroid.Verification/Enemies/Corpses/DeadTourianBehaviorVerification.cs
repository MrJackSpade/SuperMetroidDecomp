using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Corpses;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the dead Sidehopper's typed animations and the landing cue that returns it to main AI.</summary>
internal static class DeadTourianBehaviorVerification
{
    /// <summary>
    /// The real initializer starts the alive-idle pose. The idle, corpse and initially-dead poses show once and hold (the
    /// update that reaches the native sleep still reports the one-update pose as new); the hop shows its eight frames and
    /// its landing cue hands the Sidehopper back to main AI's post-landing delay.
    /// </summary>
    internal static void VerifyDeadSidehopperBehavior()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = InstalledFixtures.BossArtwork };
        RoomEnemySlot corpse = enemies.SlotTable.Slots[0];
        corpse.Bind(EnemyDefinitionId.CorpseSidehopper, corpse.Definition);
        corpse.Bind(corpse.DefinitionId, default(RoomEnemyDefinition));
        corpse.SetSpawnParameter1(EnemyDefinitionId.CorpseSidehopper, 0);
        enemies.DeadSidehopperBehavior.InitializeDeadSidehopper(corpse);
        AssertEqual(new AnimationPlayback<DeadTourianSegment>(DeadTourianAnimations.StartingWith(DeadTourianSegment.SidehopperIdle), AnimationPlace.StartOfAnimation),
            corpse.RequireSpeciesState<DeadSidehopperEnemyState>().Playback, "real dead Sidehopper initializer starts the alive-idle pose");

        foreach (DeadTourianSegment pose in new[] { DeadTourianSegment.SidehopperIdle, DeadTourianSegment.SidehopperCorpse, DeadTourianSegment.SidehopperInitiallyDead })
        {
            DeadSidehopperBehavior.SetDeadSidehopperAnimation(corpse, corpse.RequireSpeciesState<DeadSidehopperEnemyState>(), pose);
            ushort frame = DeadTourianAnimationsConstants.Spritemaps(pose).Single();
            AssertEqual(EnemyInstructionRunner.ExpectedFrames([frame, frame], [1, 1]),
                EnemyInstructionRunner.ShownFrames(corpse, 3), $"dead Sidehopper {pose} pose");
        }

        DeadSidehopperBehavior.SetDeadSidehopperAnimation(corpse, corpse.RequireSpeciesState<DeadSidehopperEnemyState>(), DeadTourianSegment.SidehopperHopping);
        ushort[] hop = DeadTourianAnimationsConstants.Spritemaps(DeadTourianSegment.SidehopperHopping).ToArray();
        int[] holds = [2, 4, 5, 48, 5, 4, 5, 4];
        AssertEqual(EnemyInstructionRunner.ExpectedFrames(hop, holds),
            EnemyInstructionRunner.ShownFrames(corpse, holds.Sum() + 1), "dead Sidehopper hop");
        AssertEqual(DeadSidehopperAction.BeginPostLandingDelay,
            (enemies.DeadSidehoppers[0] ?? throw new InvalidOperationException("Verification requires enemies.DeadSidehoppers[0].")).Action,
            "the landing cue returns ownership to main AI");
        Console.WriteLine("  Dead Sidehopper behavior: the poses hold and the hop's landing cue reaches main AI.");
    }

    /// <summary>Runs the dead Sidehopper behavior check and the dead Tourian program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--dead-sidehopper-instruction-mechanics")]
    internal static int DeadSidehopperBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyDeadSidehopperBehavior), () => VerifyDeadSidehopperBehavior());
        Suite(nameof(DeadTourianAnimationsVerification.VerifyDeadTourianAnimations), () => DeadTourianAnimationsVerification.VerifyDeadTourianAnimations(rom));
        return 0;
    }
}
