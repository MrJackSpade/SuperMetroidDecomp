using SuperMetroid.Core.Programs;
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
        foreach ((bool startsLeft, SkulteraProgram swim, SkulteraProgram loop, SkulteraProgram turn) in new[]
                 {
                     new VerifySkulteraBehaviorCase(true, SkulteraProgram.SwimmingLeft, SkulteraProgram.SwimmingLeftLoop, SkulteraProgram.TurningRight),
                     new VerifySkulteraBehaviorCase(false, SkulteraProgram.SwimmingRight, SkulteraProgram.SwimmingRightLoop, SkulteraProgram.TurningLeft),
                 })
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Skultera, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
            slot.SetSpawnParameter1(EnemyDefinitionId.Skultera, startsLeft ? SkulteraNativeValues.StartsLeftParameter : (ushort)0);
            // The room spawn primes the instruction timer before the species initializer runs.
            slot.FrameHold = WrappingCountdown.Start(1);
            SkulteraBehavior.InitializeSkultera(slot);
            SkulteraEnemyState state = enemies.SkulteraStates[0] ?? throw new InvalidOperationException("Skultera has no state.");
            AssertEqual(new AnimationPlayback<SkulteraProgram>(SkulteraAnimations.StartingWith(swim), AnimationPlace.StartOfAnimation), state.Playback, $"Skultera starts {swim}");
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(SkulteraAnimations.Segments, loop, SkulteraFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(SkulteraAnimations.Segments, loop)), $"Skultera {swim} loop");

            SkulteraBehavior.BeginSkulteraTurn(slot, state, startsLeft);
            AssertEqual(new AnimationPlayback<SkulteraProgram>(SkulteraAnimations.StartingWith(turn), AnimationPlace.StartOfAnimation), state.Playback, $"Skultera {swim} begins {turn}");
            int[] holds = [13, 10, 8, 6, 6, 8, 10, 13];
            AssertEqual(EnemyInstructionRunner.ExpectedFrames(SkulteraAnimationsConstants.Spritemaps(turn), holds),
                EnemyInstructionRunner.ShownFrames(slot, holds.Sum()), $"Skultera {turn} poses");
            AssertTrue(!state.TurnFinished, $"Skultera {turn} is unfinished while its last pose holds");
            EnemyInstructionRunner.ShownFrames(slot, 1);
            AssertTrue(state.TurnFinished, $"Skultera {turn} flags the turn finished");
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
