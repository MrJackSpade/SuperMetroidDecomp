using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.MorphBallEye.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.MorphBallEye;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.Artwork;

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
            AssertEqual(new AnimationPlayback<MorphBallEyeProgram>(MorphBallEyeAnimations.StartingWith(closed), AnimationPlace.StartOfAnimation), body.Playback, $"eye body parameter {parameter1} starts closed");
        }
        MorphBallEyeProgram[] mounts = [MorphBallEyeProgram.MountFacingLeft, MorphBallEyeProgram.MountFacingRight, MorphBallEyeProgram.MountFacingUp, MorphBallEyeProgram.MountFacingDown];
        for (ushort direction = 0; direction < mounts.Length; direction++)
        {
            (_, _, MorphBallEyeEnemyState mount) = Create(parameter1: 0, parameter2: (ushort)(MorphBallEyeNativeValues.MountParameter | direction));
            AssertEqual(new AnimationPlayback<MorphBallEyeProgram>(MorphBallEyeAnimations.StartingWith(mounts[direction]), AnimationPlace.StartOfAnimation), mount.Playback, $"eye mount direction {direction}");
        }

        foreach (MorphBallEyeProgram closing in new[] { MorphBallEyeProgram.FacingRightDeactivating, MorphBallEyeProgram.FacingLeftDeactivating })
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, MorphBallEyeEnemyState state) = Create(parameter1: 0, parameter2: 0);
            state.Playback = new(MorphBallEyeAnimations.StartingWith(closing), AnimationPlace.StartOfAnimation);
            MorphBallEyeProgram closed = closing == MorphBallEyeProgram.FacingRightDeactivating ? MorphBallEyeProgram.FacingRightClosed : MorphBallEyeProgram.FacingLeftClosed;
            ushort shut = MorphBallEyeAnimationsConstants.Spritemaps(closed).Single();
            int[] holds = [8, 48, 5, 48];
            AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. MorphBallEyeAnimationsConstants.Spritemaps(closing), shut], holds),
                EnemyInstructionRunner.ShownFrames(slot, holds.Sum() + 1), $"eye {closing} closes and holds");
        }

        (RoomEnemySystem tracker, RoomEnemySlot eye, MorphBallEyeEnemyState tracking) = Create(parameter1: 0, parameter2: 0);
        tracking.Playback = new(MorphBallEyeAnimations.StartingWith(MorphBallEyeProgram.Active), AnimationPlace.StartOfAnimation);
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(MorphBallEyeAnimations.Segments, MorphBallEyeProgram.Active, MorphBallEyeFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(eye, EnemyInstructionRunner.LoopUpdates(MorphBallEyeAnimations.Segments, MorphBallEyeProgram.Active)),
            "eye tracking loop");
        ushort[] active = [.. MorphBallEyeAnimationsConstants.Spritemaps(MorphBallEyeProgram.Active)];
        for (int pose = 0; pose < active.Length; pose++)
        {
            tracking.Playback = new(MorphBallEyeAnimations.StartingWith(MorphBallEyeProgram.Active),
                AnimationSegments<MorphBallEyeProgram, EnemyFrameId, NoAnimationCue>.PlaceOf(
                    MorphBallEyeAnimations.StartingWith(MorphBallEyeProgram.Active), MorphBallEyeProgram.Active, pose));
            eye.FrameHold = WrappingCountdown.Start(1);
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
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
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
        AssertEqual(SignedSixteenSixteen.FromParts(MorphBallEyeBeamDefinitions.FullAngularWidth, MorphBallEyeBeamDefinitions.WidthGrowthStep.Fraction),
            beam.AngularHalfWidth, "a full beam keeps the fraction it overshot with");

        beam.Phase = MorphBallEyeBeamPhase.Deactivating;
        beam.Color = MorphBallEyeBeamDefinitions.InitialColor;
        int fadeUpdates = MorphBallEyeBeamDefinitions.InitialColor.FadeLevels.Green;
        for (int update = 1; update <= fadeUpdates; update++)
        {
            enemies.MorphBallEyeBehavior.StepMorphBallEyeBeam();
            AssertEqual(Math.Max(MorphBallEyeBeamDefinitions.InitialColor.FadeLevels.Red - update, 0), beam.Color.FadeLevels.Red,
                $"eye beam red intensity after fading update {update}");
            AssertEqual(MorphBallEyeBeamDefinitions.InitialColor.FadeLevels.Green - update, beam.Color.FadeLevels.Green, $"eye beam green intensity after fading update {update}");
            AssertEqual(0, beam.Color.FadeLevels.Blue, $"eye beam blue intensity after fading update {update}");
            AssertEqual(MorphBallEyeBeamPhase.Deactivating, beam.Phase, $"eye beam still fading after update {update}");
        }
        enemies.MorphBallEyeBehavior.StepMorphBallEyeBeam();
        AssertEqual(MorphBallEyeBeamPhase.Inactive, beam.Phase, "eye beam is removed on the update after green has faded");
        Console.WriteLine("  Morph Ball eye beam: six widening updates reach full width and a full fade removes the beam.");
    }

    /// <summary>An eye body or mount after its real initializer.</summary>
    private static MorphBallEyeBehaviorCreateResult Create(ushort parameter1, ushort parameter2)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Eye, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SetSpawnParameter1(EnemyDefinitionId.Eye, parameter1);
        slot.SetSpawnParameter2(EnemyDefinitionId.Eye, parameter2);
        enemies.MorphBallEyeBehavior.InitializeMorphBallEye(slot);
        return new MorphBallEyeBehaviorCreateResult(enemies, slot, slot.SpeciesState<MorphBallEyeEnemyState>() ?? throw new InvalidOperationException("The eye has no state."));
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
