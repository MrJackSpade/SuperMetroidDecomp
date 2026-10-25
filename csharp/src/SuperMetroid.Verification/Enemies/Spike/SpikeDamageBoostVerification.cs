using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Verification.Enemies.Spike;

/// <summary>Checks Samus's spike knockback release, residual speed, and landing transitions.</summary>
internal static class SpikeDamageBoostVerification
{
    /// <summary>Replays focused spike and hurt-expiry trajectories through the runtime movement path.</summary>
    internal static void VerifyMorphedSpikeRelease()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(runtime.Room.LoadedRoom.Definition, new Position16<RoomPixels>(0, 0));
        var level = runtime.Room.LoadedLevelData;
        for (int y = 0; y <= 16; y++)
        for (int x = 0; x < level.Size.X; x++)
            level.SetForegroundEntry(y * level.Size.X + x,
                y is 0 or 16 || x is 0 or 15 ? (ushort)0x8000 : (ushort)0);
        int hazardBlock = 10 * level.Size.X + 8;
        level.SetForegroundEntry(hazardBlock, 0x2000);
        level.SetBehavior(hazardBlock, SpikeHazardDefinitions.DamagingSpikeAirBehavior);
        runtime.InitializeDebugGroundedSamus(128, 160, 16);
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.Health = 99;
        samus.EquippedItems = SamusEquipmentFlags.MorphBall;
        samus.Pose = SamusPoseId.MorphBallGroundRightPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, 0, 1);
        samus.Position = samus.Position.WithWhole(new(128, 160));
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0).WithFractionY(0);

        // Original CPU contact=2, right-facing ball, lower inside point, delay zero.
        // Frame three releases direction with subpixel base speed still present. Type
        // four's command six selects the stationary pose and clears momentum that frame.
        SpikeDamageBoostExpected[] expected =
        [
            new SpikeDamageBoostExpected(0x00800000, 0x00a10000, SamusPoseId.MorphBallGroundRightPose, 0),
            new SpikeDamageBoostExpected(0x007f4000, 0x009c0000, SamusPoseId.MorphBallMovingLeftPose, 0xc000),
            new SpikeDamageBoostExpected(0x007f0000, 0x00971c00, SamusPoseId.MorphBallMovingLeftPose, 0x4000),
            new SpikeDamageBoostExpected(0x007f0000, 0x00925400, SamusPoseId.MorphBallGroundLeftPose, 0),
            new SpikeDamageBoostExpected(0x007e4000, 0x008da800, SamusPoseId.MorphBallGroundLeftPose, 0),
            new SpikeDamageBoostExpected(0x007d8000, 0x00891800, SamusPoseId.MorphBallGroundLeftPose, 0),
            new SpikeDamageBoostExpected(0x007cc000, 0x0084a400, SamusPoseId.MorphBallGroundLeftPose, 0),
        ];
        for (int frame = 0; frame < expected.Length; frame++)
        {
            runtime.StepFrame(frame < 3 ? ControllerButton.Left | ControllerButton.A : ControllerButton.A);
            AssertEqual(expected[frame], new SpikeDamageBoostExpected(samus.Kinematics.Position.X.RawValue, samus.Kinematics.Position.Y.RawValue,
                samus.Pose, samus.HorizontalSpeed.BaseFixed), $"morphed spike release frame {frame}");
            AssertEqual(83, samus.Health, "spike contact damage");
            AssertEqual(9 - frame, samus.KnockbackTimer.RawValue, "spike hurt timer");
        }

        // Independent native seed: forward-held spike contact, left-facing, delay four,
        // frame 27. Hurt just expired above the floor; normal type-$0A probes downward.
        runtime.InitializeDebugGroundedSamus(83, 232, 16);
        samus = runtime.Player.Samus;
        (samus ?? throw new InvalidOperationException("Verification requires samus.")).Control.Unlock();
        samus.Pose = SamusPoseId.KnockbackRightPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        samus.Position = samus.Position.WithWholeX(83);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0x4000);
        samus.Position = samus.Position.WithWholeY(232);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0xe000);
        samus.Kinematics.YSpeed = default;
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        samus.HorizontalSpeed.BaseSpeed = new UnsignedSixteenSixteen(5, 0);
        samus.HorizontalSpeed.CalculateTotalSpeed(samus.HorizontalSpeed.BaseFixed);
        samus.KnockbackActive = false;
        samus.KnockbackDirection = 0;
        samus.KnockbackTimer = WrappingCountdown.Start(0);
        runtime.Controller1.Latch((ControllerButton.Right | ControllerButton.A));
        runtime.StepFrame((ControllerButton.Right | ControllerButton.A));
        AssertEqual(SamusPoseId.NormalLandingRightPose, samus.Pose, "expired hurt floor probe selects landing");
        AssertEqual(0x00534000u, samus.Kinematics.Position.X.RawValue, "expired hurt floor probe keeps X");
        AssertEqual(0x00ebffffu, samus.Kinematics.Position.Y.RawValue, "expired hurt floor alignment");
        AssertEqual(0u, samus.HorizontalSpeed.BaseFixed, "landing clears residual hurt momentum");
        AssertEqual(SamusVerticalDirection.None, samus.Kinematics.YDirection, "landing clears vertical direction");
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        runtime.StepFrame((ControllerButton.Right | ControllerButton.A));
        AssertEqual(SamusVerticalDirection.Down, samus.Kinematics.YDirection, "landing movement preserves post-landing hurt-expiry direction");

        // Native carry trace contact=5, delay=2, immediately after the contact frame.
        // One neutral hurt frame must cancel momentum without clearing its numeric
        // extra speed; the next normal movement frame consumes that cancellation.
        level.SetForegroundEntry(hazardBlock, 0);
        samus.Pose = SamusPoseId.SpinJumpRightPose;
        samus.KnockbackActive = false;
        samus.KnockbackDirection = 0;
        SamusKnockbackMovement.Start(samus, 0, new KnockbackLaunch { KnockbackXDirection = KnockbackHorizontalDirection.Left, KnockbackTimer = WrappingCountdown.Start(4) });
        samus.Position = samus.Position.WithWholeX(131);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0x6000);
        samus.Position = samus.Position.WithWholeY(160);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0);
        samus.HorizontalSpeed.BaseSpeed = new UnsignedSixteenSixteen(1, 0x6000);
        samus.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen(2, 0);
        samus.HorizontalSpeed.HasRunningMomentum = true;
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Turning;
        runtime.Controller1.Latch(0);
        CarriedFrame[] carriedFrames =
        [
            new CarriedFrame(0, 0x00808000, 0x009b0000, 0x0000e000, 2),
            new CarriedFrame(0x0280, 0x007c2000, 0x00961c00, 0x00026000, 2),
            new CarriedFrame(0x0280, 0x00790000, 0x00915400, 0x00032000, 0),
        ];
        foreach (var expectedFrame in carriedFrames)
        {
            runtime.StepFrame(ControllerWordCodec.FromRaw(expectedFrame.Input, "spike fixture frame"));
            AssertEqual(new VerifyMorphedSpikeReleaseCase(expectedFrame.X, expectedFrame.Y, expectedFrame.Base, expectedFrame.Extra),
                new VerifyMorphedSpikeReleaseCase(samus.Kinematics.Position.X.RawValue, samus.Kinematics.Position.Y.RawValue,
                    samus.HorizontalSpeed.BaseFixed, samus.HorizontalSpeed.ExtraRunSpeed.Whole),
                "native carried-speed hurt fallback trajectory");
            AssertEqual(false, samus.HorizontalSpeed.HasRunningMomentum, "hurt fallback cancels momentum flag");
        }

        samus.KnockbackActive = false;
        samus.KnockbackTimer = WrappingCountdown.Start(0);
        samus.KnockbackDirection = 0;
        samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        foreach (SamusPoseId retainedPose in new SamusPoseId[]
        {
            SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose,
            SamusPoseId.SpinJumpRightPose, SamusPoseId.SpinJumpLeftPose,
            SamusPoseId.NeutralJumpTransitionRightPose, SamusPoseId.NeutralJumpTransitionLeftPose,
            SamusPoseId.TurningRightToLeftPose, SamusPoseId.TurningLeftToRightPose,
        })
        {
            samus.Pose = retainedPose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, 0, 5);
            samus.Position = samus.Position.WithWholeY(200);
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0);
            samus.Kinematics.YDirection = SamusVerticalDirection.Down;
            samus.PoseHistory.PreviousPose = retainedPose;
            samus.PoseHistory.PreviousDirectionAndMovement =
                SnesLittleEndian.Word((byte)(SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left) ? 4 : 8), (byte)SamusPoseMetadata.ReadMovementType(samus));
            samus.PoseHistory.LastDifferentPose = SamusPoseId.NormalLandingRightPose;
            // Standing must have floor support; spin stays above that floor. Held Jump
            // without a new edge selects definition fallback rather than a fresh jump.
            if (retainedPose is SamusPoseId.FacingRightNormalPose or SamusPoseId.FacingLeftNormalPose or
                SamusPoseId.TurningRightToLeftPose or SamusPoseId.TurningLeftToRightPose)
                samus.Position = samus.Position.WithWholeY(235);
            runtime.Controller1.Latch(ControllerButton.A);
            runtime.StepFrame(ControllerButton.A);
            AssertEqual(retainedPose, samus.Pose, "fallback retains visible standing/spin/turn pose");
            AssertEqual(retainedPose, samus.PoseHistory.LastDifferentPose, "same-pose fallback shifts history");
        }

        // Native carry mode 6 / water / forward / delay 11, end of frame 25.
        // Contact with the floor and the final F8 turn command coincide next frame.
        samus.Pose = SamusPoseId.TurningRightToLeftFallingPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, 2, 1);
        samus.LiquidPhysics.ConfigureWater(8);
        samus.Position = samus.Position.WithWholeX(99);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0x3800);
        samus.Position = samus.Position.WithWholeY(237);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0xb800);
        samus.Kinematics.YSpeed = new UnsignedSixteenSixteen(0, 0x9800);
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        samus.HorizontalSpeed.BaseSpeed = new UnsignedSixteenSixteen(0, 0x9000);
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Turning;
        runtime.Controller1.Latch((ControllerButton.Left | ControllerButton.A));
        runtime.StepFrame((ControllerButton.Left | ControllerButton.A));
        AssertEqual(new SpikeDamageBoostVerifyMorphedSpikeReleaseCase(0x0063c000u, 0x00edffffu, 0x0000a000u),
            new SpikeDamageBoostVerifyMorphedSpikeReleaseCase(samus.Kinematics.Position.X.RawValue, samus.Kinematics.Position.Y.RawValue, samus.Kinematics.VerticalSpeedFixed),
            "underwater turn floor contact preserves native accumulated speed");
        AssertEqual(SamusPoseId.FallingLeftPose, samus.Pose, "turn animation completes at floor");
        AssertEqual(false, (runtime.Player.LastAerialSamusMovement ?? throw new InvalidOperationException("Verification requires runtime.Player.LastAerialSamusMovement.")).Landed,
            "turn suppresses collision-owned landing presentation");
        runtime.StepFrame((ControllerButton.Left | ControllerButton.A));
        AssertEqual(new SpikeDamageBoostVerifyMorphedSpikeReleaseCase(0x0062c000u, 0x00ebffffu, 0u),
            new SpikeDamageBoostVerifyMorphedSpikeReleaseCase(samus.Kinematics.Position.X.RawValue, samus.Kinematics.Position.Y.RawValue, samus.Kinematics.VerticalSpeedFixed),
            "following normal fall performs actual landing");
        AssertEqual(SamusPoseId.NormalLandingLeftPose, samus.Pose, "next frame selects normal landing");
    }
}
