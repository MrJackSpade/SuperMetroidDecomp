using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.Core.Enemies.Skultera.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Skultera;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Skultera's swims and its turns' timing and completion cue.</summary>
internal static class SkulteraBehaviorVerification
{
    /// <summary>
    /// The real initializer starts the swim its parameter faces, each looping on time; each turn shows its eight mirrored
    /// poses and then flags the turn finished.
    /// </summary>
    internal static void VerifySkulteraBehavior()
    {
        foreach ((bool startsLeft, SkulteraSegment swim, SkulteraSegment loop, SkulteraSegment turn) in new[]
                 {
                     new VerifySkulteraBehaviorCase(true, SkulteraSegment.SwimmingLeft, SkulteraSegment.SwimmingLeft, SkulteraSegment.TurningRight),
                     new VerifySkulteraBehaviorCase(false, SkulteraSegment.SwimmingRight, SkulteraSegment.SwimmingRight, SkulteraSegment.TurningLeft),
                 })
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Bind(EnemyDefinitionId.Skultera, place.KeptDefinition);
            place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
            place.SetSpawnParameter1(EnemyDefinitionId.Skultera, startsLeft ? SkulteraNativeValues.StartsLeftParameter : (ushort)0);
            // The room spawn primes the instruction timer before the species initializer runs.
            place.AnimationClock.StepOnNextUpdate();
            SkulteraBehavior.InitializeSkultera(place);
            SkulteraEnemyState state = enemies.SkulteraStates[0] ?? throw new InvalidOperationException("Skultera has no state.");
            AssertEqual(new AnimationPlayback<SkulteraSegment>(SkulteraAnimations.StartingWith(swim), AnimationPlace.StartOfAnimation), state.Playback, $"Skultera starts {swim}");
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(SkulteraAnimations.Segments, loop, SkulteraFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.LoopUpdates(SkulteraAnimations.Segments, loop)), $"Skultera {swim} loop");

            SkulteraBehavior.BeginSkulteraTurn(place, state, startsLeft ? HorizontalDirection.Left : HorizontalDirection.Right);
            AssertEqual(new AnimationPlayback<SkulteraSegment>(SkulteraAnimations.StartingWith(turn), AnimationPlace.StartOfAnimation), state.Playback, $"Skultera {swim} begins {turn}");
            int[] holds = [13, 10, 8, 6, 6, 8, 10, 13];
            AssertEqual(EnemyInstructionRunner.ExpectedFrames(SkulteraAnimationsConstants.Spritemaps(turn), holds),
                EnemyInstructionRunner.ShownFrames(place, holds.Sum()), $"Skultera {turn} poses");
            AssertEqual(SkulteraAction.Turning, state.Action, $"Skultera {turn} is unfinished while its last pose holds");
            EnemyInstructionRunner.ShownFrames(place, 1);
            AssertEqual(SkulteraAction.TurnFinished, state.Action, $"Skultera {turn} finishes the turn");
        }
        Console.WriteLine("  Skultera behavior: both swims with their layers and both turns with their completion cue pass.");
    }

    /// <summary>Runs the Skultera behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--skultera-instruction-mechanics")]
    internal static int SkulteraBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifySkulteraBehavior), () => VerifySkulteraBehavior());
        Suite(nameof(SkulteraAnimationsVerification.VerifySkulteraAnimations), () => SkulteraAnimationsVerification.VerifySkulteraAnimations(rom));
        return 0;
    }
}
