using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Animals;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>Checks the Etecoon's typed animations.</summary>
internal static class EtecoonInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for the Etecoon animation checks.</summary>
    internal static void VerifyEtecoonInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyEtecoonInstructionProgramDefinitions), () => VerifyEtecoonInstructionProgramDefinitions(
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// The real initializer starts the initial loop; every looping program runs on time; the four flex cycles count
    /// down; the programs show exactly the installed Etecoon frames; the program oracle matches bank $A7.
    /// </summary>
    internal static void VerifyEtecoonInstructionProgramDefinitions(CartridgeImportAddressSpace rom)
    {
        foreach (EtecoonSegment program in Enum.GetValues<EtecoonSegment>())
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, EtecoonEnemyState state) = NewEtecoon();
            AssertEqual(new AnimationPlayback<EtecoonSegment>(EtecoonAnimations.StartingWith(EtecoonSegment.Initial), AnimationPlace.StartOfAnimation), state.Playback, "real Etecoon initializer");
            // The self-looping animations: one phase of their own segment, played again and again.
            if (!EtecoonAnimations.StartingAt.TryGetValue(program, out Animation<EtecoonSegment>? animation) ||
                animation is not { Phases: [{ Plays: 1 } only], End: AnimationEnd.Loop { Phase: 0 } } || only.Segment != program)
                continue;
            state.Playback = new(EtecoonAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            slot.AnimationClock.StepOnNextUpdate();
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(EtecoonAnimations.Segments, program, EtecoonFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(EtecoonAnimations.Segments, program)),
                $"Etecoon {program} loop");
        }

        {
            // Four six-pose flex cycles, then two held poses: 4 x 6 x 8 + 2 x 32 updates reach the final sleep.
            (RoomEnemySystem enemies, RoomEnemySlot slot, EtecoonEnemyState state) = NewEtecoon();
            state.Playback = new(EtecoonAnimations.StartingWith(EtecoonSegment.Flexing), AnimationPlace.StartOfAnimation);
            slot.AnimationClock.StepOnNextUpdate();
            _ = EnemyInstructionRunner.ShownFrames(slot, 4 * 6 * 8 + 2 * 32);
            AnimationPlayback<EtecoonSegment> held = state.Playback;
            _ = EnemyInstructionRunner.ShownFrames(slot, 8);
            // The flex's last pose plays out, then the animation holds in its last phase for good.
            AssertEqual(new AnimationPlayback<EtecoonSegment>(held.Animation,
                    AnimationPlace.HoldingIn(held.Animation.Phases.Count - 1, held.Animation.Phases[^1].BeatCount)),
                state.Playback, "Etecoon flex ends asleep");
            _ = EnemyInstructionRunner.ShownFrames(slot, 8);
            AssertEqual(AnimationPlace.HoldingIn(held.Animation.Phases.Count - 1, held.Animation.Phases[^1].BeatCount), state.Playback.Place,
                "Etecoon flex stays asleep");
            AssertEqual(EtecoonAnimationsTooling.Spritemaps(EtecoonSegment.FlexingLoop).Last(), slot.SpritemapPointer, "Etecoon flex ends on its last pose");
            AssertEqual((ushort)0, slot.AnimationClock.Repeats.RawValue, "Etecoon flex loop count expires");
        }

        AssertTrue(EtecoonAnimationsTooling.Spritemaps(Enum.GetValues<EtecoonSegment>()).Where(pointer => pointer != 0x804d).ToHashSet().SetEquals(
                EnemySpritemapDefinitions.Frames.Where(frame => frame.Name.StartsWith("etecoon_", StringComparison.Ordinal)).Select(frame => frame.Pointer)),
            "the Etecoon's programs show exactly its installed frames");
        Suite(nameof(EtecoonAnimationsVerification.VerifyEtecoonAnimations), () => EtecoonAnimationsVerification.VerifyEtecoonAnimations(rom));

        Console.WriteLine("Etecoon: the real initializer, every loop and the four-cycle flex pass.");
    }

    /// <summary>An initialized Etecoon in the first enemy slot.</summary>
    private static NewEtecoonResult NewEtecoon()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Etecoon, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SetSpawnWords(EnemyDefinitionId.Etecoon, words => words);
        EtecoonBehavior.InitializeEtecoon(slot);
        return new NewEtecoonResult(enemies, slot, enemies.EtecoonStates[0] ?? throw new InvalidOperationException("The Etecoon has no state."));
    }

    /// <summary>Runs the Etecoon animation verification.</summary>
    [VerificationCommand("--etecoon-instruction-program-definitions")]
    internal static int EtecoonInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyEtecoonInstructionProgramDefinitions), () => VerifyEtecoonInstructionProgramDefinitions());
        return 0;
    }
}
