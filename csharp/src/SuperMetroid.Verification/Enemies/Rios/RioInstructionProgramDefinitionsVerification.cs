using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Rios;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

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
    internal static void VerifyRioInstructionProgramDefinitions(CartridgeImportAddressSpace rom)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace rio = enemies.Places.InUpdateOrder[0];
        rio.Bind(EnemyDefinitionId.Rio, rio.Definition);
        rio.Bind(rio.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        // The room spawn primes the instruction timer before the species initializer runs.
        rio.AnimationClock.StepOnNextUpdate();
        RioBehavior.InitializeRio(rio);
        RioEnemyState state = enemies.RioStates[0] ?? throw new InvalidOperationException("The Rio has no state.");
        AssertEqual(new AnimationPlayback<RioSegment>(RioAnimations.StartingWith(RioSegment.TuckedWingbeat), AnimationPlace.StartOfAnimation), state.Playback, "real Rio initializer starts idling");

        // Idle shows its tucked wingbeat, then runs into the post-swoop hover's tucked wingbeat three times and its hanging
        // wingbeat twice, which return to idle (#2451, #2512).
        IEnumerable<ushort> idlePass = RioAnimationsConstants.Spritemaps(RioSegment.TuckedWingbeat,
            RioSegment.TuckedWingbeat, RioSegment.TuckedWingbeat, RioSegment.TuckedWingbeat, RioSegment.HangingWingbeat, RioSegment.HangingWingbeat);
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. idlePass, RioAnimationsConstants.Spritemaps(RioSegment.TuckedWingbeat).First()], Enumerable.Repeat(4, 13)),
            EnemyInstructionRunner.ShownFrames(rio, 12 * 4 + 1),
            "idle runs into the post-swoop idle and returns");

        foreach (RioSegment swoop in new[] { RioSegment.SwoopOpening, RioSegment.SwoopCooldown })
        {
            state.Finish.Forget();
            state.Playback = new(RioAnimations.StartingWith(swoop), AnimationPlace.StartOfAnimation);
            rio.AnimationClock.StepOnNextUpdate();
            AssertEqual(EnemyInstructionRunner.ExpectedFrames(RioAnimationsConstants.Spritemaps(swoop), Enumerable.Repeat(3, 5)),
                EnemyInstructionRunner.ShownFrames(rio, 5 * 3),
                $"Rio {swoop} shows its five poses");
            AssertTrue(!state.Finish.IsRaised, $"Rio {swoop} is still showing its last pose");
            // The finished cue runs when the last pose's time runs out, on the next update.
            _ = EnemyInstructionRunner.ShownFrames(rio, 1);
            AssertTrue(state.Finish.IsRaised, $"Rio {swoop} tells main AI it has finished");
        }

        state.Playback = new(RioAnimations.StartingWith(RioSegment.SwoopLoop), AnimationPlace.StartOfAnimation);
        rio.AnimationClock.StepOnNextUpdate();
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(RioAnimations.Segments, RioSegment.SwoopLoop, RioFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(rio, EnemyInstructionRunner.LoopUpdates(RioAnimations.Segments, RioSegment.SwoopLoop)),
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
