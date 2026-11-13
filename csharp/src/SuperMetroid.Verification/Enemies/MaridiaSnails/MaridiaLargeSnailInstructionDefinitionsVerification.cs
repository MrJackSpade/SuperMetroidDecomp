using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Programs;
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
    /// The eight-entry selector table picks the native lists and installing a changed index restarts that animation with
    /// both clocks reset; the initializer starts left-facing idle; each attack shows its poses, plays the splash and
    /// marks the attack finished; each roll opens the attack-rotation window for one pose and loops on time.
    /// </summary>
    internal static void VerifyMaridiaLargeSnailInstructionDefinitions(CartridgeImportAddressSpace rom)
    {
        for (ushort animationIndex = 0; animationIndex < 8; animationIndex++)
        {
            MaridiaLargeSnailProgram program = MaridiaLargeSnailPrograms.Program(animationIndex);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(MaridiaLargeSnailNativeValues.SelectorTable + animationIndex * 2),
                NativeEnemyAnimationOracle.EntryOf(MaridiaLargeSnailNativeListAddresses.Entries, program),
                $"Oum selector {animationIndex}");
            var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry);
            var state = new MaridiaLargeSnailEnemyState()
            {
                RequestedAnimationScriptIndex = animationIndex,
                InstalledAnimationScriptIndex = -1,
            };
            slot.FrameHold = WrappingCountdown.Start(0x1234);
            slot.Timer = WrappingCountdown.Start(0x5678);
            MaridiaLargeSnailBehavior.InstallMaridiaLargeSnailAnimationScript(slot, state);
            AssertEqual(animationIndex, state.InstalledAnimationScriptIndex, $"Oum installed selector {animationIndex}");
            AssertTrue(ReferenceEquals(MaridiaLargeSnailAnimations.StartingWith(program), state.Playback.Animation) &&
                state.Playback.Place == AnimationPlace.StartOfAnimation, $"Oum installed animation {animationIndex}");
            AssertEqual((ushort)1, slot.FrameHold.RawValue, $"Oum instruction timer {animationIndex}");
            AssertEqual((ushort)0, slot.Timer.RawValue, $"Oum general timer {animationIndex}");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => MaridiaLargeSnailPrograms.Program(8), "Oum selector past table");

        foreach (MaridiaLargeSnailProgram attack in new[] { MaridiaLargeSnailProgram.FacingLeftAttacking, MaridiaLargeSnailProgram.FacingRightAttacking })
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, MaridiaLargeSnailEnemyState state) = Create();
            AssertTrue(ReferenceEquals(MaridiaLargeSnailAnimations.StartingWith(MaridiaLargeSnailProgram.FacingLeftIdle), state.Playback.Animation) &&
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

        foreach ((MaridiaLargeSnailProgram roll, int windowPose) in new[]
                 {
                     new VerifyMaridiaLargeSnailInstructionDefinitionsCase(MaridiaLargeSnailProgram.FacingLeftRollingForwards, 1), new VerifyMaridiaLargeSnailInstructionDefinitionsCase(MaridiaLargeSnailProgram.FacingRightRollingForwards, 1),
                     new VerifyMaridiaLargeSnailInstructionDefinitionsCase(MaridiaLargeSnailProgram.FacingLeftRollingBackwards, 7), new VerifyMaridiaLargeSnailInstructionDefinitionsCase(MaridiaLargeSnailProgram.FacingRightRollingBackwards, 7),
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
        Console.WriteLine("Maridia Large Snail: eight native selectors, installs, both attacks' splash and completion, and all four rolls' rotation windows and loops pass.");
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
    private static void Start(RoomEnemySlot slot, MaridiaLargeSnailEnemyState state, MaridiaLargeSnailProgram program)
    {
        state.Playback = new(MaridiaLargeSnailAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
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
