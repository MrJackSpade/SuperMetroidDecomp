using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.MorphBallEye.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.MorphBallEye;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Morph Ball eye's initial animations and how its eyelid and tracking animations run.</summary>
internal static class MorphBallEyeBehaviorVerification
{
    /// <summary>
    /// The real initializer starts a body's closed eyelid on its mounted side and each mount's directional pose; the
    /// eyelid closes over 8, 48 and 5 updates and then holds the closed pose; the tracking loop runs its sixteen ten-update poses, and
    /// a tracking cursor placed at an angle's pose shows that pose first.
    /// </summary>
    internal static void VerifyMorphBallEyeBehavior()
    {
        foreach ((ushort parameter1, MorphBallEyeProgram closed) in new[]
                 {
                     new VerifyMorphBallEyeBehaviorCase(0, MorphBallEyeProgram.FacingRightClosed),
                     new VerifyMorphBallEyeBehaviorCase(1, MorphBallEyeProgram.FacingLeftClosed),
                 })
        {
            (_, _, MorphBallEyeEnemyState body) = Create(parameter1, parameter2: 0);
            AssertEqual(AnimationProgramCursor<MorphBallEyeProgram>.Start(closed), body.Program, $"eye body parameter {parameter1} starts closed");
        }
        MorphBallEyeProgram[] mounts = [MorphBallEyeProgram.MountFacingLeft, MorphBallEyeProgram.MountFacingRight, MorphBallEyeProgram.MountFacingUp, MorphBallEyeProgram.MountFacingDown];
        for (ushort direction = 0; direction < mounts.Length; direction++)
        {
            (_, _, MorphBallEyeEnemyState mount) = Create(parameter1: 0, parameter2: (ushort)(MorphBallEyeNativeValues.MountParameter | direction));
            AssertEqual(AnimationProgramCursor<MorphBallEyeProgram>.Start(mounts[direction]), mount.Program, $"eye mount direction {direction}");
        }

        foreach (MorphBallEyeProgram closing in new[] { MorphBallEyeProgram.FacingRightDeactivating, MorphBallEyeProgram.FacingLeftDeactivating })
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, MorphBallEyeEnemyState state) = Create(parameter1: 0, parameter2: 0);
            state.Program = AnimationProgramCursor<MorphBallEyeProgram>.Start(closing);
            MorphBallEyeProgram closed = closing == MorphBallEyeProgram.FacingRightDeactivating ? MorphBallEyeProgram.FacingRightClosed : MorphBallEyeProgram.FacingLeftClosed;
            ushort shut = MorphBallEyeAnimationsConstants.Spritemaps(closed).Single();
            int[] holds = [8, 48, 5, 48];
            AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. MorphBallEyeAnimationsConstants.Spritemaps(closing), shut], holds),
                EnemyInstructionRunner.ShownFrames(slot, holds.Sum() + 1), $"eye {closing} closes and holds");
        }

        (RoomEnemySystem tracker, RoomEnemySlot eye, MorphBallEyeEnemyState tracking) = Create(parameter1: 0, parameter2: 0);
        tracking.Program = AnimationProgramCursor<MorphBallEyeProgram>.Start(MorphBallEyeProgram.Active);
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(MorphBallEyeAnimations.Programs, MorphBallEyeProgram.Active, MorphBallEyeFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(eye, EnemyInstructionRunner.LoopUpdates(MorphBallEyeAnimations.Programs, MorphBallEyeProgram.Active)),
            "eye tracking loop");
        ushort[] active = [.. MorphBallEyeAnimationsConstants.Spritemaps(MorphBallEyeProgram.Active)];
        for (int pose = 0; pose < active.Length; pose++)
        {
            tracking.Program = new AnimationProgramCursor<MorphBallEyeProgram>(MorphBallEyeProgram.Active, pose);
            eye.InstructionTimer = WrappingCountdown.Start(1);
            AssertEqual(EnemyInstructionRunner.ExpectedFrames([active[pose]], [1]), EnemyInstructionRunner.ShownFrames(eye, 1),
                $"eye tracking pose {pose}");
        }
        Console.WriteLine("  Morph Ball eye behavior: initial eyelid and mount poses, both closings and every tracking pose pass.");
    }

    /// <summary>
    /// The beam's half-width grows a quarter table unit faster each update, so its whole width reads 0, 0, 1, 2, 3 and
    /// then reaches four on the sixth update, which keeps the fraction and makes the beam full; a fading beam steps red
    /// and green one intensity per update toward their faded levels and is removed on the update after green arrives.
    /// </summary>
    internal static void VerifyMorphBallEyeBeamWideningAndFade()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        MorphBallEyeBeamState beam = enemies.MorphBallEyeBehavior.MorphBallEyeBeam;
        beam.Phase = MorphBallEyeBeamPhase.Widening;
        int[] widths = [0, 0, 1, 2, 3, 4];
        for (int update = 0; update < widths.Length; update++)
        {
            enemies.MorphBallEyeBehavior.StepMorphBallEyeBeam();
            AssertEqual(widths[update], beam.AngularWidth, $"eye beam whole width after widening update {update + 1}");
            AssertEqual(update == widths.Length - 1 ? MorphBallEyeBeamPhase.Full : MorphBallEyeBeamPhase.Widening, beam.Phase,
                $"eye beam phase after widening update {update + 1}");
        }
        AssertEqual(SnesSignedSixteenSixteen.FromParts(MorphBallEyeBeamDefinitions.FullAngularWidth, MorphBallEyeBeamDefinitions.WidthGrowthStep.Fraction),
            beam.AngularHalfWidth, "a full beam keeps the fraction it overshot with");

        beam.Phase = MorphBallEyeBeamPhase.Deactivating;
        beam.Red = MorphBallEyeBeamDefinitions.InitialRed;
        beam.Green = MorphBallEyeBeamDefinitions.InitialGreen;
        beam.Blue = MorphBallEyeBeamDefinitions.Blue;
        int fadeUpdates = MorphBallEyeBeamDefinitions.InitialGreen - MorphBallEyeBeamDefinitions.FadedGreen;
        for (int update = 1; update <= fadeUpdates; update++)
        {
            enemies.MorphBallEyeBehavior.StepMorphBallEyeBeam();
            AssertEqual(Math.Max(MorphBallEyeBeamDefinitions.InitialRed - update, MorphBallEyeBeamDefinitions.FadedRed), beam.Red,
                $"eye beam red after fading update {update}");
            AssertEqual(MorphBallEyeBeamDefinitions.InitialGreen - update, beam.Green, $"eye beam green after fading update {update}");
            AssertEqual(MorphBallEyeBeamDefinitions.Blue, beam.Blue, $"eye beam blue after fading update {update}");
            AssertEqual(MorphBallEyeBeamPhase.Deactivating, beam.Phase, $"eye beam still fading after update {update}");
        }
        enemies.MorphBallEyeBehavior.StepMorphBallEyeBeam();
        AssertEqual(MorphBallEyeBeamPhase.Inactive, beam.Phase, "eye beam is removed on the update after green has faded");
        Console.WriteLine("  Morph Ball eye beam: six widening updates reach full width and a full fade removes the beam.");
    }

    /// <summary>An eye body or mount after its real initializer.</summary>
    private static MorphBallEyeBehaviorCreateResult Create(ushort parameter1, ushort parameter2)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Eye, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition) with { Bank = MorphBallEyeFrameToolingDefinitions.Bank });
        slot.SetSpawnParameter1(parameter1);
        slot.SetSpawnParameter2(parameter2);
        enemies.MorphBallEyeBehavior.InitializeMorphBallEye(slot);
        return new MorphBallEyeBehaviorCreateResult(enemies, slot, enemies.MorphBallEyeBehavior.MorphBallEyeStates[0] ?? throw new InvalidOperationException("The eye has no state."));
    }

    /// <summary>Runs the Morph Ball eye behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--morph-ball-eye-instruction-program-definitions")]
    internal static int MorphBallEyeBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyMorphBallEyeBehavior), () => VerifyMorphBallEyeBehavior());
        Suite(nameof(VerifyMorphBallEyeBeamWideningAndFade), () => VerifyMorphBallEyeBeamWideningAndFade());
        Suite(nameof(MorphBallEyeAnimationsVerification.VerifyMorphBallEyeAnimations), () => MorphBallEyeAnimationsVerification.VerifyMorphBallEyeAnimations(rom));
        return 0;
    }
}
