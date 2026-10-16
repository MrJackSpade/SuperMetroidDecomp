using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Enemies.MaridiaSnails.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.MaridiaSnails;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Oum's animation selection, installs, and its animations' cues.</summary>
internal static class MaridiaLargeSnailInstructionDefinitionsVerification
{
    /// <summary>
    /// The eight-entry selector table picks the native lists and installing a changed index restarts that animation with
    /// both clocks reset; the initializer starts left-facing idle; each attack shows its poses, plays the splash and
    /// marks the attack finished; each roll opens the attack-rotation window for one pose and loops on time.
    /// </summary>
    internal static void VerifyMaridiaLargeSnailInstructionDefinitions(SuperMetroidAddressSpace rom)
    {
        for (ushort animationIndex = 0; animationIndex < 8; animationIndex++)
        {
            MaridiaLargeSnailProgram program = MaridiaLargeSnailInstructionDefinitions.Program(animationIndex);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(MaridiaLargeSnailNativeValues.SelectorTable + animationIndex * 2),
                NativeEnemyProgramOracle.EntryOf(MaridiaLargeSnailNativeListAddresses.Entries, program),
                $"Oum selector {animationIndex}");
            var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry);
            var state = new MaridiaLargeSnailEnemyState(slot)
            {
                RequestedInstructionListIndex = animationIndex,
                InstalledInstructionListIndex = ushort.MaxValue,
            };
            slot.InstructionTimer = NativeCountdown.Start(0x1234);
            slot.Timer = NativeCountdown.Start(0x5678);
            MaridiaLargeSnailBehavior.InstallMaridiaLargeSnailInstructionList(slot, state);
            AssertEqual(animationIndex, state.InstalledInstructionListIndex, $"Oum installed selector {animationIndex}");
            AssertEqual(AnimationProgramCursor<MaridiaLargeSnailProgram>.Start(program), state.Program, $"Oum installed animation {animationIndex}");
            AssertEqual((ushort)1, slot.InstructionTimer.RawValue, $"Oum instruction timer {animationIndex}");
            AssertEqual((ushort)0, slot.Timer.RawValue, $"Oum general timer {animationIndex}");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => MaridiaLargeSnailInstructionDefinitions.Program(8), "Oum selector past table");

        foreach (MaridiaLargeSnailProgram attack in new[] { MaridiaLargeSnailProgram.FacingLeftAttacking, MaridiaLargeSnailProgram.FacingRightAttacking })
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, MaridiaLargeSnailEnemyState state) = Create(rom);
            AssertEqual(AnimationProgramCursor<MaridiaLargeSnailProgram>.Start(MaridiaLargeSnailProgram.FacingLeftIdle), state.Program, "Oum starts left-facing idle");
            Start(slot, state, attack);
            int[] holds = [16, 16, 16, 2, 3, 4, 2, 3, 1, 3, 2, 1, 18];
            AssertEqual(EnemyInstructionRunner.ExpectedFrames(MaridiaLargeSnailAnimationsConstants.Spritemaps(attack), holds),
                EnemyInstructionRunner.ShownFrames(slot, holds.Sum()), $"Oum {attack} poses");
            AssertTrue(!state.AttackAnimationFinished, $"Oum {attack} is unfinished while its last pose holds");
            EnemyInstructionRunner.ShownFrames(slot, 1);
            AssertTrue(state.AttackAnimationFinished, $"Oum {attack} marks the attack finished");
            AssertEqual((ushort?)MaridiaLargeSnailNativeValues.SplashSound, enemies.MaridiaLargeSnailBehavior.LastMaridiaLargeSnailSoundEffect, $"Oum {attack} plays the splash");
        }

        foreach ((MaridiaLargeSnailProgram roll, int windowPose) in new[]
                 {
                     (MaridiaLargeSnailProgram.FacingLeftRollingForwards, 1), (MaridiaLargeSnailProgram.FacingRightRollingForwards, 1),
                     (MaridiaLargeSnailProgram.FacingLeftRollingBackwards, 7), (MaridiaLargeSnailProgram.FacingRightRollingBackwards, 7),
                 })
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, MaridiaLargeSnailEnemyState state) = Create(rom);
            Start(slot, state, roll);
            EnemyInstructionRunner.ShownFrames(slot, 7 * windowPose + 1);
            AssertTrue(state.AttackAllowsRotation, $"Oum {roll} opens the attack-rotation window");
            EnemyInstructionRunner.ShownFrames(slot, 7);
            AssertTrue(!state.AttackAllowsRotation, $"Oum {roll} closes the attack-rotation window one pose later");
            Start(slot, state, roll);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(MaridiaLargeSnailAnimations.Programs, roll, MaridiaLargeSnailFrameDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(MaridiaLargeSnailAnimations.Programs, roll)), $"Oum {roll} loop");
        }
        Console.WriteLine("Maridia Large Snail: eight native selectors, installs, both attacks' splash and completion, and all four rolls' rotation windows and loops pass.");
    }

    /// <summary>An Oum after its real initializer.</summary>
    private static (RoomEnemySystem, RoomEnemySlot, MaridiaLargeSnailEnemyState) Create(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(rom);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Oum, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = MaridiaLargeSnailFrameToolingDefinitions.Bank });
        MaridiaLargeSnailBehavior.InitializeMaridiaLargeSnail(slot);
        return (enemies, slot, enemies.MaridiaLargeSnailStates[0] ?? throw new InvalidOperationException("Oum has no state."));
    }

    /// <summary>Starts <paramref name="program"/> with its first step due on the next update.</summary>
    private static void Start(RoomEnemySlot slot, MaridiaLargeSnailEnemyState state, MaridiaLargeSnailProgram program)
    {
        state.Program = AnimationProgramCursor<MaridiaLargeSnailProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Runs the Oum checks and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--maridia-large-snail-instruction-definitions")]
    internal static int MaridiaLargeSnailInstructionDefinitionsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyMaridiaLargeSnailInstructionDefinitions), () => VerifyMaridiaLargeSnailInstructionDefinitions(rom));
        Suite(nameof(MaridiaLargeSnailAnimationsVerification.VerifyMaridiaLargeSnailAnimations), () => MaridiaLargeSnailAnimationsVerification.VerifyMaridiaLargeSnailAnimations(rom));
        return 0;
    }
}
