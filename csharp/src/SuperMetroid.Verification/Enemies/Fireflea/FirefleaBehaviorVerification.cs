using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.Core.Enemies.Fireflea.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Fireflea;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that Fireflea starts and runs its typed animation (#641).</summary>
internal static class FirefleaBehaviorVerification
{
    /// <summary>The real initializer starts the Loop program, which the runtime then plays through once and back.</summary>
    internal static void VerifyFirefleaBehavior()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Fireflea, place.Definition);
        place.Bind(place.DefinitionId, default(RoomEnemyDefinition));
        place.SetSpawnParameter1(EnemyDefinitionId.Fireflea, 0);
        place.SetSpawnParameter2(EnemyDefinitionId.Fireflea, 0);
        FirefleaBehavior.InitializeFireflea(place);
        AssertEqual(new AnimationPlayback<FirefleaSegment>(FirefleaAnimations.StartingWith(FirefleaSegment.BrightestWingbeat), AnimationPlace.StartOfAnimation), (enemies.Places.InUpdateOrder[0].EntityState<FirefleaEntity, FirefleaEnemyState>() ?? throw new InvalidOperationException("The Fireflea has no state.")).Playback,
            "Fireflea initializer animation");
        place.AnimationClock.StepOnNextUpdate();
        AssertEqual(EnemyInstructionRunner.ExpectedAnimationLoop(FirefleaAnimations.Segments, FirefleaAnimations.StartingWith(FirefleaSegment.BrightestWingbeat), FirefleaFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.AnimationLoopUpdates(FirefleaAnimations.Segments, FirefleaAnimations.StartingWith(FirefleaSegment.BrightestWingbeat))),
            "Fireflea runs its loop and returns through its goto");
        Console.WriteLine("  Fireflea behavior: the initializer starts and runs its loop.");
    }

    /// <summary>
    /// A vertically moving Fireflea spawned at row 100 has radius eight, so its extents are rows 92 and 108 (#2369: no
    /// placement bobs within its radius of the room's top, so the extents are plain rows). The production main AI compares
    /// the position with both: at its spawn Y it is inside both and keeps moving up; one pixel above the upper extent
    /// (91), or at the lower extent (108), it turns down.
    /// </summary>
    internal static void VerifyFirefleaVerticalExtents()
    {
        var frame = new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false },
            new FrameCounters(0));
        foreach ((ushort y, VerticalDirection expectedDirection) in new VerifyFirefleaVerticalExtentsCase[] { new(100, VerticalDirection.Up), new(91, VerticalDirection.Down), new(108, VerticalDirection.Down) })
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Bind(EnemyDefinitionId.Fireflea, place.Definition);
            place.SetSpawnParameter1(EnemyDefinitionId.Fireflea, 0);
            place.SetSpawnParameter2(EnemyDefinitionId.Fireflea, 0);
            place.Position = place.Position.WithWhole(new(128, 100));
            FirefleaBehavior.InitializeFireflea(place);
            BobbingFirefleaState state = place.RequireSpeciesState<FirefleaEnemyState>().Flight as BobbingFirefleaState
                ?? throw new InvalidOperationException("Verification requires a bobbing Fireflea.");
            AssertEqual(92, state.MinimumY, "the upper extent is the spawn Y minus the radius");
            AssertEqual(108, state.MaximumY, "the lower extent is the spawn Y plus the radius");
            place.Position = place.Position.WithWholeY(y);

            FirefleaBehavior.RunFirefleaMainAi(place, frame);

            AssertEqual(expectedDirection, state.Direction, $"Fireflea at Y {y} keeps or reverses its way");
        }
        Console.WriteLine("  Fireflea vertical extents: the Fireflea turns past its upper extent and at its lower one.");
    }

    /// <summary>Runs the Fireflea behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--fireflea-instruction-program-definitions")]
    internal static int FirefleaBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyFirefleaBehavior), () => VerifyFirefleaBehavior());
        Suite(nameof(VerifyFirefleaVerticalExtents), () => VerifyFirefleaVerticalExtents());
        Suite(nameof(FirefleaAnimationsVerification.VerifyFirefleaAnimations), () => FirefleaAnimationsVerification.VerifyFirefleaAnimations(rom));
        return 0;
    }
}
