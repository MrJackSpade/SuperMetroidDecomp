using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Rios;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Rio's typed idle, swoop and cooldown animations.</summary>
internal static class RioInstructionProgramDefinitionsVerification
{
    /// <summary>Loads retail cartridge data for the Rio animation checks.</summary>
    internal static void VerifyRioInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyRioInstructionProgramDefinitions), () => VerifyRioInstructionProgramDefinitions(
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// The real initializer starts idling with nothing installed; the idle program runs into the post-swoop idle and back;
    /// both one-shot swoop programs end by telling main AI the animation has finished; the second swoop phase loops; the
    /// program oracle matches bank $A2.
    /// </summary>
    internal static void VerifyRioInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(rom);
        RoomEnemySlot rio = enemies.SlotTable.Slots[0];
        rio.EnemyDefinitionPointer = EnemyDefinitionId.Rio;
        rio.Definition = default(RoomEnemyDefinition) with { Bank = RioFrameDefinitions.Bank };
        // The room spawn primes the instruction timer before the species initializer runs.
        rio.InstructionTimer = NativeCountdown.Start(1);
        enemies.RioBehavior.InitializeRio(rio);
        RioEnemyState state = enemies.RioStates[0] ?? throw new InvalidOperationException("The Rio has no state.");
        AssertEqual(AnimationProgramCursor<RioProgram>.Start(RioProgram.Idle), state.Program, "real Rio initializer starts idling");
        AssertEqual((RioProgram?)null, state.InstalledProgram, "real Rio initializer leaves nothing installed");

        // Idle shows its two poses, then runs into the ten-pose post-swoop idle, which returns to idle.
        IEnumerable<ushort> idlePass = RioAnimations.Spritemaps(RioProgram.Idle, RioProgram.PostSwoopIdle);
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. idlePass, RioAnimations.Spritemaps(RioProgram.Idle).First()], Enumerable.Repeat(4, 13)),
            EnemyInstructionRunner.ShownFrames(enemies, rio, 12 * 4 + 1),
            "idle runs into the post-swoop idle and returns");

        foreach (RioProgram swoop in new[] { RioProgram.SwoopingPart1, RioProgram.SwoopCooldown })
        {
            state.AnimationFinished = false;
            state.Program = AnimationProgramCursor<RioProgram>.Start(swoop);
            rio.InstructionTimer = NativeCountdown.Start(1);
            AssertEqual(EnemyInstructionRunner.ExpectedFrames(RioAnimations.Spritemaps(swoop), Enumerable.Repeat(3, 5)),
                EnemyInstructionRunner.ShownFrames(enemies, rio, 5 * 3),
                $"Rio {swoop} shows its five poses");
            AssertTrue(!state.AnimationFinished, $"Rio {swoop} is still showing its last pose");
            // The finished cue runs when the last pose's time runs out, on the next update.
            _ = EnemyInstructionRunner.ShownFrames(enemies, rio, 1);
            AssertTrue(state.AnimationFinished, $"Rio {swoop} tells main AI it has finished");
        }

        state.Program = AnimationProgramCursor<RioProgram>.Start(RioProgram.SwoopingPart2);
        rio.InstructionTimer = NativeCountdown.Start(1);
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(RioAnimations.Programs, RioProgram.SwoopingPart2, RioFrameDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(enemies, rio, EnemyInstructionRunner.LoopUpdates(RioAnimations.Programs, RioProgram.SwoopingPart2)),
            "second swoop phase loops");

        Suite(nameof(RioAnimationsVerification.VerifyRioAnimations), () => RioAnimationsVerification.VerifyRioAnimations(rom));
        Console.WriteLine(
            "Rio: the real initializer, idle fallthrough and return, both one-shot swoop animations with their finished " +
            "cue, and the looping second swoop phase pass.");
    }

    /// <summary>Runs the Rio animation verification command.</summary>
    [VerificationCommand("--rio-instruction-mechanics")]
    internal static int RioInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyRioInstructionProgramDefinitions), () => VerifyRioInstructionProgramDefinitions());
        return 0;
    }
}
