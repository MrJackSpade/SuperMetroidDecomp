using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;

/// <summary>Checks charged bomb-spread input through grounded morph and unmorph transitions.</summary>
internal static class GroundedSpreadTransitionVerification
{
    /// <summary>Exercises both facings and four release scenarios, optionally comparing each frame to a native trace.</summary>
    internal static void VerifyGroundedSpreadTransition(string? tracePath = null)
    {
        using var trace = tracePath is null ? null : File.OpenText(tracePath);
        if (trace is not null) AssertEqual("left,scenario,frame,input,pose,y,charge,spread,bombs", trace.ReadLine(), "native transition header");
        foreach (bool left in new[] { false, true })
        for (int scenario = 0; scenario < 4; scenario++)
            VerifyGroundedSpreadTransitionCase(left, scenario, trace);
        if (trace is not null) AssertTrue(trace.ReadLine() is null, "native transition trace fully consumed");
        Console.WriteLine("Grounded spread runtime: earned charge, hold, unmorph and release boundaries pass in both directions (960 frames).");
    }

    /// <summary>Runs one 120-frame input path through the retail runtime in an isolated flat clearing.</summary>
    private static void VerifyGroundedSpreadTransitionCase(bool left, int scenario, StreamReader? trace)
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LandingSite);
        var level = runtime.Room.LoadedLevelData;
        // A local flat clearing isolates the input/pose/charge path from slopes,
        // gunship interaction and scenery. The actual runtime still drives each tick.
        for (int y = 24; y < 36; y++)
        for (int x = 24; x < 40; x++)
        {
            int index = y * level.Size.X + x;
            level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0,
                y >= 32 ? RoomCollisionType.SolidBlock : RoomCollisionType.Air).Raw);
            level.SetBehavior(index, 0);
        }
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.Pose = left ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose;
        samus.EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
        samus.EquippedBeams = SamusBeamFlags.Charge;
        samus.Position = samus.Position.WithWhole(new(512, 490));
        AssertEqual((ushort)0, samus.Kinematics.Position.X.Fraction, "fixture initial X fraction matches native cleared RAM");
        AssertEqual((ushort)0, samus.Kinematics.Position.Y.Fraction, "fixture initial Y fraction matches native cleared RAM");
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation();
        for (int frame = 0; frame < 120; frame++)
        {
            SnesButton input = runtime.Options.ControllerBindings.Shoot;
            if (frame is >= 70 and < 110 and not 75) input |= SnesButton.Down;
            if (frame == 100)
            {
                if (scenario != 3) input |= runtime.Options.ControllerBindings.Jump;
                if (scenario == 1) input &= ~SnesButton.Down;
                if (scenario == 2) input &= ~runtime.Options.ControllerBindings.Shoot;
            }
            runtime.StepFrame(input);
            if (trace is not null)
                AssertEqual(trace.ReadLine(), $"{(left ? 1 : 0)},{scenario},{frame},{input:X},{(int)samus.Pose:X4},{samus.Position.Y.Whole:X4},{samus.ProjectileFlareCounter:X4},{samus.BombSpreadChargeTimeoutCounter:X4},{runtime.BombProjectiles.BombCounter:X4}",
                    $"native/runtime charge-preserving unmorph frame {frame}");
            AssertEqual(samus.ProjectileFlareCounter, runtime.Projectiles.Charge.FlareCounter, "runtime charge mirror stays synchronized");
            if (frame == 99)
            {
                AssertEqual((ushort)77, samus.ProjectileFlareCounter, "beam charge earned before morph stays intact while holding Down");
                AssertEqual((ushort)17, samus.BombSpreadChargeTimeoutCounter, "hold counter advances only after morph animation");
            }
            if (frame is >= 100 and <= 105 && scenario != 3)
                AssertEqual(left ? SamusPoseId.UnmorphingTransitionLeftPose : SamusPoseId.UnmorphingTransitionRightPose,
                    samus.Pose, "native posture input dispatcher cannot interrupt the six-frame unmorph");
            if (frame == 100)
            {
                AssertEqual((ushort)(scenario is 1 or 2 ? 0 : 77), samus.ProjectileFlareCounter, "same-frame release decides charge consumption before unmorph");
                AssertEqual((ushort)(scenario == 1 ? 5 : 0), runtime.BombProjectiles.BombCounter, "releasing Down creates spread even on the unmorph input frame");
            }
            if (frame == 106 && scenario != 3)
            {
                AssertEqual(left ? SamusPoseId.CrouchingLeftPose : SamusPoseId.CrouchingRightPose, samus.Pose, "unmorph finishes in crouch rather than falling");
                AssertEqual((ushort)496, samus.Position.Y.Whole, "unmorph maintains flat-floor alignment");
            }
        }
    }

    /// <summary>Runs the grounded spread check without a native frame trace.</summary>
    [VerificationCommand("--grounded-spread-transition")]
    internal static int GroundedSpreadTransitionCommand()
    {
        Suite(nameof(VerifyGroundedSpreadTransition), () => VerifyGroundedSpreadTransition());
        return 0;
    }

    /// <summary>Runs the grounded spread check with per-frame native transition comparisons.</summary>
    [VerificationCommand("--grounded-spread-transition", "transitionTrace")]
    internal static int GroundedSpreadTransitionCommand2(string transitionTrace)
    {
        Suite(nameof(VerifyGroundedSpreadTransition), () => VerifyGroundedSpreadTransition(transitionTrace));
        return 0;
    }
}
