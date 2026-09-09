using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyMorphedSpikeRelease()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(runtime.ActiveRoom!.Pointer, 0, 0);
        var level = runtime.LevelData!;
        for (int y = 0; y <= 16; y++)
        for (int x = 0; x < level.WidthInBlocks; x++)
            level.SetForegroundEntry(y * level.WidthInBlocks + x,
                y is 0 or 16 || x is 0 or 15 ? (ushort)0x8000 : (ushort)0);
        int hazardBlock = 10 * level.WidthInBlocks + 8;
        level.SetForegroundEntry(hazardBlock, 0x2000);
        level.SetBehavior(hazardBlock, SamusTerrainHazardRomData.DamagingSpikeAirBehavior);
        runtime.InitializeDebugGroundedSamus(128, 160, 16);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Health = 99;
        samus.EquippedItems = (ushort)SamusEquipmentFlags.MorphBall;
        samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.SetAnimationFrameFromSpecialHandler(0, 1);
        samus.XPosition = 128;
        samus.YPosition = 160;
        samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;

        // Original CPU contact=2, right-facing ball, lower inside point, delay zero.
        // Frame three releases direction with subpixel base speed still present. Type
        // four's command six selects the stationary pose and clears momentum that frame.
        (uint X, uint Y, byte Pose, uint Base)[] expected =
        [
            (0x00800000, 0x00a10000, SamusPoseIds.MorphBallGroundRightPose, 0),
            (0x007f4000, 0x009c0000, SamusPoseIds.MorphBallMovingLeftPose, 0xc000),
            (0x007f0000, 0x00971c00, SamusPoseIds.MorphBallMovingLeftPose, 0x4000),
            (0x007f0000, 0x00925400, SamusPoseIds.MorphBallGroundLeftPose, 0),
            (0x007e4000, 0x008da800, SamusPoseIds.MorphBallGroundLeftPose, 0),
            (0x007d8000, 0x00891800, SamusPoseIds.MorphBallGroundLeftPose, 0),
            (0x007cc000, 0x0084a400, SamusPoseIds.MorphBallGroundLeftPose, 0),
        ];
        for (int frame = 0; frame < expected.Length; frame++)
        {
            runtime.StepFrame(frame < 3 ? (ushort)0x0280 : (ushort)0x0080);
            AssertEqual(expected[frame], (samus.Kinematics.XFixed, samus.Kinematics.YFixed,
                samus.Pose, samus.HorizontalSpeed.BaseFixed), $"morphed spike release frame {frame}");
            AssertEqual(83, samus.Health, "spike contact damage");
            AssertEqual(9 - frame, samus.KnockbackTimer, "spike hurt timer");
        }

        // Independent native seed: forward-held spike contact, left-facing, delay four,
        // frame 27. Hurt just expired above the floor; normal type-$0A probes downward.
        runtime.InitializeDebugGroundedSamus(83, 232, 16);
        samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Pose = SamusPoseIds.KnockbackRightPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.XPosition = 83;
        samus.Kinematics.XSubposition = 0x4000;
        samus.YPosition = 232;
        samus.Kinematics.YSubposition = 0xe000;
        samus.Kinematics.YSpeed = samus.Kinematics.YSubspeed = 0;
        samus.Kinematics.YDirection = 2;
        samus.HorizontalSpeed.BaseSpeed = 5;
        samus.HorizontalSpeed.BaseSubspeed = 0;
        samus.HorizontalSpeed.CalculateTotalSpeed(samus.HorizontalSpeed.BaseFixed);
        samus.KnockbackActive = false;
        samus.KnockbackTimer = samus.KnockbackDirection = 0;
        runtime.Controller1.Latch(0x0180);
        runtime.StepFrame(0x0180);
        AssertEqual(SamusPoseIds.NormalLandingRightPose, samus.Pose, "expired hurt floor probe selects landing");
        AssertEqual(0x00534000u, samus.Kinematics.XFixed, "expired hurt floor probe keeps X");
        AssertEqual(0x00ebffffu, samus.Kinematics.YFixed, "expired hurt floor alignment");
        AssertEqual(0u, samus.HorizontalSpeed.BaseFixed, "landing clears residual hurt momentum");
        AssertEqual(0, samus.Kinematics.YDirection, "landing clears vertical direction");
        samus.Kinematics.YDirection = 2;
        runtime.StepFrame(0x0180);
        AssertEqual(2, samus.Kinematics.YDirection, "landing movement preserves post-landing hurt-expiry direction");

        // Native carry trace contact=5, delay=2, immediately after the contact frame.
        // One neutral hurt frame must cancel momentum without clearing its numeric
        // extra speed; the next normal movement frame consumes that cancellation.
        level.SetForegroundEntry(hazardBlock, 0);
        samus.Pose = SamusPoseIds.SpinJumpRightPose;
        samus.KnockbackActive = false;
        samus.KnockbackDirection = 0;
        SamusKnockbackMovement.Start(bus, samus, 0, 0, 4);
        samus.XPosition = 131;
        samus.Kinematics.XSubposition = 0x6000;
        samus.YPosition = 160;
        samus.Kinematics.YSubposition = 0;
        samus.HorizontalSpeed.BaseSpeed = 1;
        samus.HorizontalSpeed.BaseSubspeed = 0x6000;
        samus.HorizontalSpeed.ExtraRunSpeed = 2;
        samus.HorizontalSpeed.ExtraRunSubspeed = 0;
        samus.HorizontalSpeed.HasRunningMomentum = true;
        samus.HorizontalSpeed.AccelerationMode = 1;
        runtime.Controller1.Latch(0);
        (ushort Input, uint X, uint Y, uint Base, ushort Extra)[] carriedFrames =
        [
            (0, 0x00808000, 0x009b0000, 0x0000e000, 2),
            (0x0280, 0x007c2000, 0x00961c00, 0x00026000, 2),
            (0x0280, 0x00790000, 0x00915400, 0x00032000, 0),
        ];
        foreach (var expectedFrame in carriedFrames)
        {
            runtime.StepFrame(expectedFrame.Input);
            AssertEqual((expectedFrame.X, expectedFrame.Y, expectedFrame.Base, expectedFrame.Extra),
                (samus.Kinematics.XFixed, samus.Kinematics.YFixed,
                    samus.HorizontalSpeed.BaseFixed, samus.HorizontalSpeed.ExtraRunSpeed),
                "native carried-speed hurt fallback trajectory");
            AssertEqual(false, samus.HorizontalSpeed.HasRunningMomentum, "hurt fallback cancels momentum flag");
        }

        samus.KnockbackActive = false;
        samus.KnockbackDirection = samus.KnockbackTimer = 0;
        samus.HorizontalSpeed.ClearHorizontalMomentum(samus.ReadFacingDirection(bus));
        foreach (byte retainedPose in new byte[]
        {
            SamusPoseIds.FacingRightNormalPose, SamusPoseIds.FacingLeftNormalPose,
            SamusPoseIds.SpinJumpRightPose, SamusPoseIds.SpinJumpLeftPose,
            SamusPoseIds.NeutralJumpTransitionRightPose, SamusPoseIds.NeutralJumpTransitionLeftPose,
        })
        {
            samus.Pose = retainedPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 5);
            samus.YPosition = 200;
            samus.Kinematics.YSubposition = 0;
            samus.Kinematics.YDirection = 2;
            samus.PoseHistory.PreviousPose = retainedPose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(
                ((byte)samus.ReadMovementType(bus) << 8) | (samus.IsFacingLeft(bus) ? 4 : 8));
            samus.PoseHistory.LastDifferentPose = SamusPoseIds.NormalLandingRightPose;
            // Standing must have floor support; spin stays above that floor. Held Jump
            // without a new edge selects definition fallback rather than a fresh jump.
            if (retainedPose is SamusPoseIds.FacingRightNormalPose or SamusPoseIds.FacingLeftNormalPose)
                samus.YPosition = 235;
            runtime.Controller1.Latch(0x0080);
            runtime.StepFrame(0x0080);
            AssertEqual(retainedPose, samus.Pose, "fallback retains visible standing/spin pose");
            AssertEqual(retainedPose, samus.PoseHistory.LastDifferentPose, "same-pose fallback shifts history");
        }

        // Native carry mode 6 / water / forward / delay 11, end of frame 25.
        // Contact with the floor and the final F8 turn command coincide next frame.
        samus.Pose = SamusPoseIds.TurningRightToLeftFallingPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.SetAnimationFrameFromSpecialHandler(2, 1);
        samus.LiquidPhysics.ConfigureWater(8);
        samus.XPosition = 99;
        samus.Kinematics.XSubposition = 0x3800;
        samus.YPosition = 237;
        samus.Kinematics.YSubposition = 0xb800;
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0x9800;
        samus.Kinematics.YDirection = 2;
        samus.HorizontalSpeed.BaseSpeed = 0;
        samus.HorizontalSpeed.BaseSubspeed = 0x9000;
        samus.HorizontalSpeed.AccelerationMode = 1;
        runtime.Controller1.Latch(0x0280);
        runtime.StepFrame(0x0280);
        AssertEqual((0x0063c000u, 0x00edffffu, 0x0000a000u),
            (samus.Kinematics.XFixed, samus.Kinematics.YFixed, samus.Kinematics.VerticalSpeedFixed),
            "underwater turn floor contact preserves native accumulated speed");
        AssertEqual(SamusPoseIds.FallingLeftPose, samus.Pose, "turn animation completes at floor");
        AssertEqual(false, runtime.LastAerialSamusMovement!.Value.Landed,
            "turn suppresses collision-owned landing presentation");
        runtime.StepFrame(0x0280);
        AssertEqual((0x0062c000u, 0x00ebffffu, 0u),
            (samus.Kinematics.XFixed, samus.Kinematics.YFixed, samus.Kinematics.VerticalSpeedFixed),
            "following normal fall performs actual landing");
        AssertEqual(SamusPoseIds.NormalLandingLeftPose, samus.Pose, "next frame selects normal landing");
    }
}
