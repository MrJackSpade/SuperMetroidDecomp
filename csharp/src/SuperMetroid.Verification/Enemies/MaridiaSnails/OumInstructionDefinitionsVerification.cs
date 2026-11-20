using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Enemies.MaridiaSnails.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.MaridiaSnails;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Oum's animation selection, installs, and its animations' cues.</summary>
internal static class OumInstructionDefinitionsVerification
{
    /// <summary>
    /// The eight-entry selector table picks the native lists for the animations its indexes name, and starting a changed
    /// animation restarts it with both clocks reset; the behaviour's transitions move between the animations as the
    /// original index arithmetic does; the initializer starts left-facing idle; each attack shows its poses, plays the splash and
    /// marks the attack finished; each roll opens the attack-rotation window for one pose and loops on time.
    /// </summary>
    internal static void VerifyOumInstructionDefinitions(CartridgeImportAddressSpace rom)
    {
        for (ushort animationIndex = 0; animationIndex < 8; animationIndex++)
        {
            OumSegment animation = MaridiaSnailsNativeWords.OumAnimations.Decode(animationIndex, "Oum animation index");
            OumSegment program = animation;
            AssertEqual(rom.ReadCartridgeWord(OumNativeValues.SelectorTable + animationIndex * 2),
                NativeEnemyAnimationOracle.EntryOf(OumNativeListAddresses.Entries, program),
                $"Oum selector {animationIndex}");
            var place = new RoomEnemyPlace(0, EnemyPlaceFixtures.Registry, new RemovedEnemyLeftovers<EnemyOccupantStates>());
            var state = new OumEnemyState(OumAction.Idle, OumBouncePhase.Falling);
            place.AnimationClock.HoldFor(0x1234);
            OumBehavior.StartOumAnimation(place, state, animation);
            AssertTrue(ReferenceEquals(OumAnimations.StartingWith(program), state.Playback.Animation) &&
                state.Playback.Place == AnimationPlace.StartOfAnimation, $"Oum installed animation {animationIndex}");
            AssertEqual(1, place.AnimationClock.Hold, $"Oum instruction timer {animationIndex}");
        }
        AssertThrows<InvalidDataException>(() => MaridiaSnailsNativeWords.OumAnimations.Decode(8, "Oum animation index"), "Oum selector past table");

        // Each transition of the behaviour lands on the animation the original index arithmetic reaches: idle facing
        // Samus (0 or 1), a roll from idle (OR 2), an attack facing Samus (facing bit OR 4), the roll after an attack
        // (minus 2) and a reversed roll (XOR 4).
        int transitions = 0;
        foreach (bool left in new[] { false, true })
        {
            int facing = left ? 1 : 0;
            HorizontalDirection way = left ? HorizontalDirection.Left : HorizontalDirection.Right;
            AssertEqual(Index(facing), OumAnimationChoices.Idle(way), $"Oum idle facing left {left}");
            AssertEqual(Index(facing | 2), OumAnimationChoices.RollingForwards(way), $"Oum roll from idle facing left {left}");
            AssertEqual(Index((facing & 1) | 4), OumAnimationChoices.Attacking(way), $"Oum attack facing left {left}");
            AssertEqual(Index((facing | 4) - 2), OumAnimationChoices.RollingForwards(way), $"Oum roll after attack facing left {left}");
            foreach (bool backwards in new[] { false, true })
            {
                int roll = facing | (backwards ? 6 : 2);
                AssertEqual(Index(roll ^ 4), (backwards ? OumAnimationChoices.RollingForwards(way) : OumAnimationChoices.RollingBackwards(way)), $"Oum reversed roll from index {roll}");
            }
            transitions += 6;
        }
        static OumSegment Index(int index) => MaridiaSnailsNativeWords.OumAnimations.Decode(index, "Oum animation index");

        VerifyOumSpawnIdle();

        foreach (OumSegment attack in new[] { OumSegment.FacingLeftAttacking, OumSegment.FacingRightAttacking })
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place, OumEnemyState state) = Create();
            AssertTrue(ReferenceEquals(OumAnimations.StartingWith(OumSegment.FacingLeftIdle), state.Playback.Animation) &&
                state.Playback.Place == AnimationPlace.StartOfAnimation, "Oum starts left-facing idle");
            // Oum attacks as its AI starts the attack (#2439).
            state.Action = OumAction.Attacking;
            Start(place, state, attack);
            int[] holds = [16, 16, 16, 2, 3, 4, 2, 3, 1, 3, 2, 1, 18];
            AssertEqual(EnemyInstructionRunner.ExpectedFrames(OumAnimationsConstants.Spritemaps(attack), holds),
                EnemyInstructionRunner.ShownFrames(place, holds.Sum()), $"Oum {attack} poses");
            AssertEqual(OumAction.Attacking, state.Action, $"Oum {attack} is unfinished while its last pose holds");
            EnemyInstructionRunner.ShownFrames(place, 1);
            AssertEqual(OumAction.AttackFinished, state.Action, $"Oum {attack} finishes the attack");
            AssertEqual<SoundEffectId?>(OumNativeValues.SplashSound,
                enemies.Audio.LatestSound, $"Oum {attack} plays the library-two splash");
        }

        foreach ((OumSegment roll, int windowPose) in new[]
                 {
                     new VerifyOumInstructionDefinitionsCase(OumSegment.FacingLeftRollingForwards, 1), new VerifyOumInstructionDefinitionsCase(OumSegment.FacingRightRollingForwards, 1),
                     new VerifyOumInstructionDefinitionsCase(OumSegment.FacingLeftRollingBackwards, 7), new VerifyOumInstructionDefinitionsCase(OumSegment.FacingRightRollingBackwards, 7),
                 })
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place, OumEnemyState state) = Create();
            Start(place, state, roll);
            EnemyInstructionRunner.ShownFrames(place, 7 * windowPose + 1);
            AssertTrue(state.AttackAllowsRotation, $"Oum {roll} opens the attack-rotation window");
            EnemyInstructionRunner.ShownFrames(place, 7);
            AssertTrue(!state.AttackAllowsRotation, $"Oum {roll} closes the attack-rotation window one pose later");
            Start(place, state, roll);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(OumAnimations.Segments, roll, OumFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.LoopUpdates(OumAnimations.Segments, roll)), $"Oum {roll} loop");
        }
        Console.WriteLine($"Maridia Large Snail: eight native selectors, installs, {transitions} index-arithmetic transitions, both attacks' splash and completion, and all four rolls' rotation windows and loops pass.");
    }

    /// <summary>
    /// The spawned Oum shows its left-facing idle while it counts as facing right (#2145): asking for the right-facing idle
    /// keeps that pose and its frame timer, asking for the left-facing idle starts it afresh, and from then on asking for
    /// the animation playing leaves it running.
    /// </summary>
    private static void VerifyOumSpawnIdle()
    {
        (_, RoomEnemyPlace place, OumEnemyState state) = Create();
        Animation<OumSegment> leftIdle = OumAnimations.StartingWith(OumSegment.FacingLeftIdle);
        var spawned = new AnimationPlayback<OumSegment>(leftIdle, AnimationPlace.StartOfAnimation);
        AssertEqual(spawned, state.Playback, "Oum spawns showing its left-facing idle");
        place.AnimationClock.HoldFor(0x1234);
        OumBehavior.StartOumAnimation(place, state, OumSegment.FacingRightIdle);
        AssertEqual(spawned, state.Playback, "the spawned Oum keeps its left-facing pose for the right-facing idle it counts as");
        AssertEqual(0x1234, place.AnimationClock.Hold, "the spawned Oum's frame timer runs on for the right-facing idle");
        OumBehavior.StartOumAnimation(place, state, OumSegment.FacingLeftIdle);
        AssertEqual(spawned, state.Playback, "asking the spawned Oum for its left-facing idle starts that idle");
        AssertEqual(1, place.AnimationClock.Hold, "asking the spawned Oum for its left-facing idle starts it afresh");
        place.AnimationClock.HoldFor(0x1234);
        OumBehavior.StartOumAnimation(place, state, OumSegment.FacingLeftIdle);
        AssertEqual(0x1234, place.AnimationClock.Hold, "the started left-facing idle asked for again runs on");
    }

    /// <summary>An Oum after its real initializer.</summary>
    private static OumInstructionDefinitionsCreateResult Create()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Oum, place.Definition);
        place.Bind(place.DefinitionId, default(RoomEnemyDefinition));
        OumBehavior.InitializeOum(place);
        return new OumInstructionDefinitionsCreateResult(enemies, place, enemies.OumStates[0] ?? throw new InvalidOperationException("Oum has no state."));
    }

    /// <summary>Starts <paramref name="program"/> with its first step due on the next update.</summary>
    private static void Start(RoomEnemyPlace place, OumEnemyState state, OumSegment program)
    {
        state.Playback = new(OumAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
        place.AnimationClock.Restart();
    }

    /// <summary>Runs the Oum checks and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--maridia-large-snail-instruction-definitions")]
    internal static int OumInstructionDefinitionsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyOumInstructionDefinitions), () => VerifyOumInstructionDefinitions(rom));
        Suite(nameof(OumAnimationsVerification.VerifyOumAnimations), () => OumAnimationsVerification.VerifyOumAnimations(rom));
        return 0;
    }
}
