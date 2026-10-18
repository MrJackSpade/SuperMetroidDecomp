using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Metroids.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Metroids;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Metroid's initial animation and both pulse loops with their closing sounds.</summary>
internal static class MetroidBehaviorVerification
{
    /// <summary>
    /// The real initializer starts the chasing pulse; the chasing loop's cue advances the RNG once for a random cry and
    /// the draining loop's cue plays the draining sound without touching the RNG; both loops run on time.
    /// </summary>
    internal static void VerifyMetroidBehavior()
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Metroid, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = MetroidFrameToolingDefinitions.Bank });
        // The room spawn primes the instruction timer before the species initializer runs.
        slot.InstructionTimer = NativeCountdown.Start(1);
        enemies.MetroidBehavior.InitializeMetroid(slot);
        MetroidEnemyState state = enemies.MetroidStates[0] ?? throw new InvalidOperationException("The Metroid has no state.");
        AssertEqual(AnimationProgramCursor<MetroidProgram>.Start(MetroidProgram.ChasingSamus), state.Program, "the Metroid starts chasing");

        foreach ((MetroidProgram loop, int cries) in new[] { (MetroidProgram.ChasingSamus, 1), (MetroidProgram.DrainingSamus, 0) })
        {
            int randomCalls = 0;
            enemies.VerificationServices().NextRandom = () => { randomCalls++; return 0; };
            state.Program = AnimationProgramCursor<MetroidProgram>.Start(loop);
            slot.InstructionTimer = NativeCountdown.Start(1);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(MetroidAnimations.Programs, loop, MetroidFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(MetroidAnimations.Programs, loop)), $"Metroid {loop} loop");
            AssertEqual(cries, randomCalls, $"Metroid {loop} loop's RNG use");
            AssertTrue(enemies.MetroidBehavior.LastMetroidSoundEffectLibrary2 is not null, $"Metroid {loop} loop plays its sound");
        }
        Console.WriteLine("  Metroid behavior: the chasing start and both pulse loops with their sound and RNG cues pass.");
    }

    /// <summary>Runs the Metroid behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--metroid-instruction-mechanics")]
    internal static int MetroidBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyMetroidBehavior), () => VerifyMetroidBehavior());
        Suite(nameof(MetroidAnimationsVerification.VerifyMetroidAnimations), () => MetroidAnimationsVerification.VerifyMetroidAnimations(rom));
        return 0;
    }
}
