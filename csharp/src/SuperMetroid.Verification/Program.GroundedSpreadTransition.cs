using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyGroundedSpreadTransition(string? tracePath = null)
    {
        using var trace = tracePath is null ? null : File.OpenText(tracePath);
        if (trace is not null) AssertEqual("left,scenario,frame,input,pose,y,charge,spread,bombs", trace.ReadLine(), "native transition header");
        foreach (bool left in new[] { false, true })
        for (int scenario = 0; scenario < 4; scenario++)
            VerifyGroundedSpreadTransitionCase(left, scenario, trace);
        if (trace is not null) AssertTrue(trace.ReadLine() is null, "native transition trace fully consumed");
        Console.WriteLine("Grounded spread runtime: earned charge, hold, unmorph and release boundaries pass in both directions (960 frames).");
    }

    private static void VerifyGroundedSpreadTransitionCase(bool left, int scenario, StreamReader? trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        var level = runtime.LevelData!;
        // A local flat clearing isolates the input/pose/charge path from slopes,
        // gunship interaction and scenery. The actual runtime still drives each tick.
        for (int y = 24; y < 36; y++)
        for (int x = 24; x < 40; x++)
        {
            int index = y * level.WidthInBlocks + x;
            level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0,
                y >= 32 ? RoomCollisionType.SolidBlock : RoomCollisionType.Air).Raw);
            level.SetBehavior(index, 0);
        }
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
        samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
        samus.EquippedBeams = (ushort)SamusBeamFlags.Charge;
        samus.XPosition = 512;
        samus.YPosition = 490;
        AssertEqual((ushort)0, samus.Kinematics.XSubposition, "fixture initial X fraction matches native cleared RAM");
        AssertEqual((ushort)0, samus.Kinematics.YSubposition, "fixture initial Y fraction matches native cleared RAM");
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        for (int frame = 0; frame < 120; frame++)
        {
            ushort input = runtime.ControllerBindings.Shoot;
            if (frame is >= 70 and < 110 && frame != 75) input |= (ushort)SnesButton.Down;
            if (frame == 100)
            {
                if (scenario != 3) input |= runtime.ControllerBindings.Jump;
                if (scenario == 1) input &= unchecked((ushort)~SnesButton.Down);
                if (scenario == 2) input &= unchecked((ushort)~runtime.ControllerBindings.Shoot);
            }
            runtime.StepFrame(input);
            if (trace is not null)
                AssertEqual(trace.ReadLine(), $"{(left ? 1 : 0)},{scenario},{frame},{input:X4},{samus.Pose:X4},{samus.YPosition:X4},{samus.ProjectileFlareCounter:X4},{samus.BombSpreadChargeTimeoutCounter:X4},{runtime.BombProjectiles.BombCounter:X4}",
                    $"native/runtime charge-preserving unmorph frame {frame}");
            AssertEqual(samus.ProjectileFlareCounter, runtime.Projectiles.FlareCounter, "runtime charge mirror stays synchronized");
            if (frame == 99)
            {
                AssertEqual((ushort)77, samus.ProjectileFlareCounter, "beam charge earned before morph stays intact while holding Down");
                AssertEqual((ushort)17, samus.BombSpreadChargeTimeoutCounter, "hold counter advances only after morph animation");
            }
            if (frame is >= 100 and <= 105 && scenario != 3)
                AssertEqual(left ? SamusPoseIds.UnmorphingTransitionLeftPose : SamusPoseIds.UnmorphingTransitionRightPose,
                    samus.Pose, "native posture input dispatcher cannot interrupt the six-frame unmorph");
            if (frame == 100)
            {
                AssertEqual((ushort)(scenario is 1 or 2 ? 0 : 77), samus.ProjectileFlareCounter, "same-frame release decides charge consumption before unmorph");
                AssertEqual((ushort)(scenario == 1 ? 5 : 0), runtime.BombProjectiles.BombCounter, "releasing Down creates spread even on the unmorph input frame");
            }
            if (frame == 106 && scenario != 3)
            {
                AssertEqual(left ? SamusPoseIds.CrouchingLeftPose : SamusPoseIds.CrouchingRightPose, samus.Pose, "unmorph finishes in crouch rather than falling");
                AssertEqual((ushort)496, samus.YPosition, "unmorph maintains flat-floor alignment");
            }
        }
    }
}
