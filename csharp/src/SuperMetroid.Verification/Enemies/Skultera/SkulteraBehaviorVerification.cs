using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.Core.Enemies.Skultera.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Skultera;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Skultera's swims, the layer each sets, and its turns' timing and completion cue.</summary>
internal static class SkulteraBehaviorVerification
{
    /// <summary>
    /// The real initializer starts the swim its parameter faces; the left swim draws on layer two and the right on
    /// layer six, each looping on time; each turn shows its eight mirrored poses and then flags the turn finished.
    /// </summary>
    internal static void VerifySkulteraBehavior()
    {
        foreach ((bool startsLeft, SkulteraProgram swim, SkulteraProgram loop, ushort layer, SkulteraProgram turn) in new[]
                 {
                     (true, SkulteraProgram.SwimmingLeft, SkulteraProgram.SwimmingLeftLoop, (ushort)2, SkulteraProgram.TurningRight),
                     (false, SkulteraProgram.SwimmingRight, SkulteraProgram.SwimmingRightLoop, (ushort)6, SkulteraProgram.TurningLeft),
                 })
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Skultera, slot.Definition);
            slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = SkulteraFrameToolingDefinitions.Bank });
            slot.SetSpawnParameter1(startsLeft ? SkulteraNativeValues.StartsLeftParameter : (ushort)0);
            // The room spawn primes the instruction timer before the species initializer runs.
            slot.InstructionTimer = NativeCountdown.Start(1);
            SkulteraBehavior.InitializeSkultera(slot);
            SkulteraEnemyState state = enemies.SkulteraStates[0] ?? throw new InvalidOperationException("Skultera has no state.");
            AssertEqual(AnimationProgramCursor<SkulteraProgram>.Start(swim), state.Program, $"Skultera starts {swim}");
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(SkulteraAnimations.Programs, loop, SkulteraFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(SkulteraAnimations.Programs, loop)), $"Skultera {swim} loop");
            AssertEqual(layer, slot.Layer, $"Skultera {swim} draws on layer {layer}");

            SkulteraBehavior.BeginSkulteraTurn(slot, state, startsLeft);
            AssertEqual(AnimationProgramCursor<SkulteraProgram>.Start(turn), state.Program, $"Skultera {swim} begins {turn}");
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
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifySkulteraBehavior), () => VerifySkulteraBehavior());
        Suite(nameof(SkulteraAnimationsVerification.VerifySkulteraAnimations), () => SkulteraAnimationsVerification.VerifySkulteraAnimations(rom));
        return 0;
    }
}
