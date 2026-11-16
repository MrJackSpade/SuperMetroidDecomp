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
        AssertEqual(new AnimationPlayback<FirefleaSegment>(FirefleaAnimations.StartingWith(FirefleaSegment.Loop), AnimationPlace.StartOfAnimation), (enemies.Places.InUpdateOrder[0].EntityState<FirefleaEntity, FirefleaEnemyState>() ?? throw new InvalidOperationException("The Fireflea has no state.")).Playback,
            "Fireflea initializer animation");
        place.AnimationClock.StepOnNextUpdate();
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(FirefleaAnimations.Segments, FirefleaSegment.Loop, FirefleaFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.LoopUpdates(FirefleaAnimations.Segments, FirefleaSegment.Loop)),
            "Fireflea runs its loop and returns through its goto");
        Console.WriteLine("  Fireflea behavior: the initializer starts and runs its loop.");
    }

    /// <summary>
    /// A vertically moving Fireflea spawned four pixels below the room's top edge has radius eight, so its upper extent
    /// wraps to 65532. The production main AI compares the position with both extents by the wrapped spatial difference:
    /// at its spawn Y it is inside both and keeps its speed entry; one pixel above the wrapped upper extent (65531), or at
    /// the lower extent (12), it reverses to the other half of the speed entry.
    /// </summary>
    internal static void VerifyFirefleaVerticalExtentsWrap()
    {
        var frame = new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false },
            new FrameCounters(0));
        foreach ((ushort y, EnemySpeedSign expectedSpeedSign) in new VerifyFirefleaVerticalExtentsWrapCase[] { new VerifyFirefleaVerticalExtentsWrapCase(4, EnemySpeedSign.Negative), new VerifyFirefleaVerticalExtentsWrapCase(65531, EnemySpeedSign.Positive), new VerifyFirefleaVerticalExtentsWrapCase(12, EnemySpeedSign.Positive) })
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Bind(EnemyDefinitionId.Fireflea, place.Definition);
            place.SetSpawnParameter1(EnemyDefinitionId.Fireflea, 0);
            place.SetSpawnParameter2(EnemyDefinitionId.Fireflea, 0);
            place.Position = place.Position.WithWhole(new(128, 4));
            FirefleaBehavior.InitializeFireflea(place);
            BobbingFirefleaState state = place.RequireSpeciesState<FirefleaEnemyState>().Flight as BobbingFirefleaState
                ?? throw new InvalidOperationException("Verification requires a bobbing Fireflea.");
            AssertEqual(65532, state.MinimumY, "the upper extent wraps past the room's top edge");
            AssertEqual(12, state.MaximumY, "the lower extent is the spawn Y plus the radius");
            place.Position = place.Position.WithWholeY(y);

            FirefleaBehavior.RunFirefleaMainAi(place, frame);

            AssertEqual(expectedSpeedSign, state.SpeedSign, $"Fireflea at Y {y} keeps or reverses its speed");
        }
        Console.WriteLine("  Fireflea vertical extents: positions compare with wrapped extents by the wrapped spatial difference.");
    }

    /// <summary>Runs the Fireflea behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--fireflea-instruction-program-definitions")]
    internal static int FirefleaBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyFirefleaBehavior), () => VerifyFirefleaBehavior());
        Suite(nameof(VerifyFirefleaVerticalExtentsWrap), () => VerifyFirefleaVerticalExtentsWrap());
        Suite(nameof(FirefleaAnimationsVerification.VerifyFirefleaAnimations), () => FirefleaAnimationsVerification.VerifyFirefleaAnimations(rom));
        return 0;
    }
}
