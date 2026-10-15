using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.MorphBallEye.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.MorphBallEye;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Morph Ball eye's initial animations and how its eyelid and tracking animations run.</summary>
internal static class MorphBallEyeBehaviorVerification
{
    /// <summary>
    /// The real initializer starts a body's closed eyelid on its mounted side and each mount's directional pose; the
    /// eyelid closes over 8, 48 and 5 updates and then holds the closed pose; the tracking loop runs its sixteen ten-update poses, and
    /// a tracking cursor placed at an angle's pose shows that pose first.
    /// </summary>
    internal static void VerifyMorphBallEyeBehavior(SuperMetroidAddressSpace rom)
    {
        foreach ((ushort parameter1, MorphBallEyeProgram closed) in new[]
                 {
                     ((ushort)0, MorphBallEyeProgram.FacingRightClosed),
                     ((ushort)1, MorphBallEyeProgram.FacingLeftClosed),
                 })
        {
            (_, _, MorphBallEyeEnemyState body) = Create(rom, parameter1, parameter2: 0);
            AssertEqual(AnimationProgramCursor<MorphBallEyeProgram>.Start(closed), body.Program, $"eye body parameter {parameter1} starts closed");
        }
        MorphBallEyeProgram[] mounts = [MorphBallEyeProgram.MountFacingLeft, MorphBallEyeProgram.MountFacingRight, MorphBallEyeProgram.MountFacingUp, MorphBallEyeProgram.MountFacingDown];
        for (ushort direction = 0; direction < mounts.Length; direction++)
        {
            (_, _, MorphBallEyeEnemyState mount) = Create(rom, parameter1: 0, parameter2: (ushort)(MorphBallEyeNativeValues.MountParameter | direction));
            AssertEqual(AnimationProgramCursor<MorphBallEyeProgram>.Start(mounts[direction]), mount.Program, $"eye mount direction {direction}");
        }

        foreach (MorphBallEyeProgram closing in new[] { MorphBallEyeProgram.FacingRightDeactivating, MorphBallEyeProgram.FacingLeftDeactivating })
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, MorphBallEyeEnemyState state) = Create(rom, parameter1: 0, parameter2: 0);
            state.Program = AnimationProgramCursor<MorphBallEyeProgram>.Start(closing);
            MorphBallEyeProgram closed = closing == MorphBallEyeProgram.FacingRightDeactivating ? MorphBallEyeProgram.FacingRightClosed : MorphBallEyeProgram.FacingLeftClosed;
            ushort shut = MorphBallEyeAnimations.Spritemaps(closed).Single();
            int[] holds = [8, 48, 5, 48];
            AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. MorphBallEyeAnimations.Spritemaps(closing), shut], holds),
                EnemyInstructionRunner.ShownFrames(enemies, slot, holds.Sum() + 1), $"eye {closing} closes and holds");
        }

        (RoomEnemySystem tracker, RoomEnemySlot eye, MorphBallEyeEnemyState tracking) = Create(rom, parameter1: 0, parameter2: 0);
        tracking.Program = AnimationProgramCursor<MorphBallEyeProgram>.Start(MorphBallEyeProgram.Active);
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(MorphBallEyeAnimations.Programs, MorphBallEyeProgram.Active, MorphBallEyeFrameDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(tracker, eye, EnemyInstructionRunner.LoopUpdates(MorphBallEyeAnimations.Programs, MorphBallEyeProgram.Active)),
            "eye tracking loop");
        ushort[] active = [.. MorphBallEyeAnimations.Spritemaps(MorphBallEyeProgram.Active)];
        for (int pose = 0; pose < active.Length; pose++)
        {
            tracking.Program = new AnimationProgramCursor<MorphBallEyeProgram>(MorphBallEyeProgram.Active, pose);
            eye.InstructionTimer = NativeCountdown.Start(1);
            AssertEqual(EnemyInstructionRunner.ExpectedFrames([active[pose]], [1]), EnemyInstructionRunner.ShownFrames(tracker, eye, 1),
                $"eye tracking pose {pose}");
        }
        Console.WriteLine("  Morph Ball eye behavior: initial eyelid and mount poses, both closings and every tracking pose pass.");
    }

    /// <summary>An eye body or mount after its real initializer.</summary>
    private static (RoomEnemySystem Enemies, RoomEnemySlot Slot, MorphBallEyeEnemyState State) Create(SuperMetroidAddressSpace rom, ushort parameter1, ushort parameter2)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(rom);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Eye;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = MorphBallEyeFrameDefinitions.Bank };
        slot.Parameter1 = parameter1;
        slot.Parameter2 = parameter2;
        enemies.MorphBallEyeBehavior.InitializeMorphBallEye(slot);
        return (enemies, slot, enemies.MorphBallEyeBehavior.MorphBallEyeStates[0] ?? throw new InvalidOperationException("The eye has no state."));
    }

    /// <summary>Runs the Morph Ball eye behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--morph-ball-eye-instruction-program-definitions")]
    internal static int MorphBallEyeBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyMorphBallEyeBehavior), () => VerifyMorphBallEyeBehavior(rom));
        Suite(nameof(MorphBallEyeAnimationsVerification.VerifyMorphBallEyeAnimations), () => MorphBallEyeAnimationsVerification.VerifyMorphBallEyeAnimations(rom));
        return 0;
    }
}
