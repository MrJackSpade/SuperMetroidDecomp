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
internal static class MaridiaLargeSnailInstructionDefinitionsVerification
{
    /// <summary>
    /// The eight-entry selector table picks the native lists for the animations its indexes name, and starting a changed
    /// animation restarts it with both clocks reset; the behaviour's transitions move between the animations as the
    /// original index arithmetic does; the initializer starts left-facing idle; each attack shows its poses, plays the splash and
    /// marks the attack finished; each roll opens the attack-rotation window for one pose and loops on time.
    /// </summary>
    internal static void VerifyMaridiaLargeSnailInstructionDefinitions(CartridgeImportAddressSpace rom)
    {
        for (ushort animationIndex = 0; animationIndex < 8; animationIndex++)
        {
            MaridiaLargeSnailSegment animation = MaridiaSnailsNativeWords.MaridiaLargeSnailAnimations.Decode(animationIndex, "Oum animation index");
            MaridiaLargeSnailSegment program = animation;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(MaridiaLargeSnailNativeValues.SelectorTable + animationIndex * 2),
                NativeEnemyAnimationOracle.EntryOf(MaridiaLargeSnailNativeListAddresses.Entries, program),
                $"Oum selector {animationIndex}");
            var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry);
            var state = new MaridiaLargeSnailEnemyState()
            {
                RequestedAnimation = animation,
                InstalledAnimation = animation == MaridiaLargeSnailSegment.FacingRightIdle
                    ? MaridiaLargeSnailSegment.FacingLeftIdle
                    : MaridiaLargeSnailSegment.FacingRightIdle,
            };
            slot.AnimationClock.HoldFor(0x1234);
            slot.AnimationClock.Repeats = WrappingCountdown.Start(0x5678);
            MaridiaLargeSnailBehavior.StartMaridiaLargeSnailAnimation(slot, state);
            AssertEqual(animation, state.InstalledAnimation, $"Oum installed selector {animationIndex}");
            AssertTrue(ReferenceEquals(MaridiaLargeSnailAnimations.StartingWith(program), state.Playback.Animation) &&
                state.Playback.Place == AnimationPlace.StartOfAnimation, $"Oum installed animation {animationIndex}");
            AssertEqual((ushort)1, slot.AnimationClock.Hold.RawValue, $"Oum instruction timer {animationIndex}");
            AssertEqual((ushort)0, slot.AnimationClock.Repeats.RawValue, $"Oum general timer {animationIndex}");
        }
        AssertThrows<InvalidDataException>(() => MaridiaSnailsNativeWords.MaridiaLargeSnailAnimations.Decode(8, "Oum animation index"), "Oum selector past table");

        // Each transition of the behaviour lands on the animation the original index arithmetic reaches: idle facing
        // Samus (0 or 1), a roll from idle (OR 2), an attack facing Samus (facing bit OR 4), the roll after an attack
        // (minus 2) and a reversed roll (XOR 4).
        int transitions = 0;
        foreach (bool left in new[] { false, true })
        {
            int facing = left ? 1 : 0;
            AssertEqual(Index(facing), MaridiaLargeSnailAnimationChoices.Idle(left), $"Oum idle facing left {left}");
            AssertEqual(Index(facing | 2), MaridiaLargeSnailAnimationChoices.RollingFrom(Index(facing)), $"Oum roll from idle facing left {left}");
            AssertEqual(Index((facing & 1) | 4), MaridiaLargeSnailAnimationChoices.Attacking(left), $"Oum attack facing left {left}");
            AssertEqual(Index((facing | 4) - 2), MaridiaLargeSnailAnimationChoices.RollingAfter(Index(facing | 4)), $"Oum roll after attack facing left {left}");
            foreach (int roll in new[] { facing | 2, facing | 6 })
                AssertEqual(Index(roll ^ 4), MaridiaLargeSnailAnimationChoices.Reversed(Index(roll)), $"Oum reversed roll from index {roll}");
            transitions += 6;
        }
        foreach (MaridiaLargeSnailSegment notIdle in new[] { Index(2), Index(4), Index(6) })
            AssertThrows<InvalidOperationException>(() => MaridiaLargeSnailAnimationChoices.RollingFrom(notIdle), $"Oum starts rolling only from idle, not {notIdle}");
        foreach (MaridiaLargeSnailSegment notAttack in new[] { Index(0), Index(2), Index(6) })
            AssertThrows<InvalidOperationException>(() => MaridiaLargeSnailAnimationChoices.RollingAfter(notAttack), $"Oum resumes rolling only after an attack, not {notAttack}");
        foreach (MaridiaLargeSnailSegment notRoll in new[] { Index(0), Index(5) })
            AssertThrows<InvalidOperationException>(() => MaridiaLargeSnailAnimationChoices.Reversed(notRoll), $"Only a rolling Oum reverses, not {notRoll}");
        static MaridiaLargeSnailSegment Index(int index) => MaridiaSnailsNativeWords.MaridiaLargeSnailAnimations.Decode(index, "Oum animation index");

        foreach (MaridiaLargeSnailSegment attack in new[] { MaridiaLargeSnailSegment.FacingLeftAttacking, MaridiaLargeSnailSegment.FacingRightAttacking })
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, MaridiaLargeSnailEnemyState state) = Create();
            AssertTrue(ReferenceEquals(MaridiaLargeSnailAnimations.StartingWith(MaridiaLargeSnailSegment.FacingLeftIdle), state.Playback.Animation) &&
                state.Playback.Place == AnimationPlace.StartOfAnimation, "Oum starts left-facing idle");
            Start(slot, state, attack);
            int[] holds = [16, 16, 16, 2, 3, 4, 2, 3, 1, 3, 2, 1, 18];
            AssertEqual(EnemyInstructionRunner.ExpectedFrames(MaridiaLargeSnailAnimationsConstants.Spritemaps(attack), holds),
                EnemyInstructionRunner.ShownFrames(slot, holds.Sum()), $"Oum {attack} poses");
            AssertTrue(!state.AttackAnimationFinished, $"Oum {attack} is unfinished while its last pose holds");
            EnemyInstructionRunner.ShownFrames(slot, 1);
            AssertTrue(state.AttackAnimationFinished, $"Oum {attack} marks the attack finished");
            AssertEqual<SoundEffectId?>(MaridiaLargeSnailNativeValues.SplashSound,
                enemies.Audio.LatestSound, $"Oum {attack} plays the library-two splash");
        }

        foreach ((MaridiaLargeSnailSegment roll, int windowPose) in new[]
                 {
                     new VerifyMaridiaLargeSnailInstructionDefinitionsCase(MaridiaLargeSnailSegment.FacingLeftRollingForwards, 1), new VerifyMaridiaLargeSnailInstructionDefinitionsCase(MaridiaLargeSnailSegment.FacingRightRollingForwards, 1),
                     new VerifyMaridiaLargeSnailInstructionDefinitionsCase(MaridiaLargeSnailSegment.FacingLeftRollingBackwards, 7), new VerifyMaridiaLargeSnailInstructionDefinitionsCase(MaridiaLargeSnailSegment.FacingRightRollingBackwards, 7),
                 })
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, MaridiaLargeSnailEnemyState state) = Create();
            Start(slot, state, roll);
            EnemyInstructionRunner.ShownFrames(slot, 7 * windowPose + 1);
            AssertTrue(state.AttackAllowsRotation, $"Oum {roll} opens the attack-rotation window");
            EnemyInstructionRunner.ShownFrames(slot, 7);
            AssertTrue(!state.AttackAllowsRotation, $"Oum {roll} closes the attack-rotation window one pose later");
            Start(slot, state, roll);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(MaridiaLargeSnailAnimations.Segments, roll, MaridiaLargeSnailFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(MaridiaLargeSnailAnimations.Segments, roll)), $"Oum {roll} loop");
        }
        Console.WriteLine($"Maridia Large Snail: eight native selectors, installs, {transitions} index-arithmetic transitions, both attacks' splash and completion, and all four rolls' rotation windows and loops pass.");
    }

    /// <summary>An Oum after its real initializer.</summary>
    private static MaridiaLargeSnailInstructionDefinitionsCreateResult Create()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Oum, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        MaridiaLargeSnailBehavior.InitializeMaridiaLargeSnail(slot);
        return new MaridiaLargeSnailInstructionDefinitionsCreateResult(enemies, slot, enemies.MaridiaLargeSnailStates[0] ?? throw new InvalidOperationException("Oum has no state."));
    }

    /// <summary>Starts <paramref name="program"/> with its first step due on the next update.</summary>
    private static void Start(RoomEnemySlot slot, MaridiaLargeSnailEnemyState state, MaridiaLargeSnailSegment program)
    {
        state.Playback = new(MaridiaLargeSnailAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
        slot.AnimationClock.Restart();
    }

    /// <summary>Runs the Oum checks and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--maridia-large-snail-instruction-definitions")]
    internal static int MaridiaLargeSnailInstructionDefinitionsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyMaridiaLargeSnailInstructionDefinitions), () => VerifyMaridiaLargeSnailInstructionDefinitions(rom));
        Suite(nameof(MaridiaLargeSnailAnimationsVerification.VerifyMaridiaLargeSnailAnimations), () => MaridiaLargeSnailAnimationsVerification.VerifyMaridiaLargeSnailAnimations(rom));
        return 0;
    }
}
