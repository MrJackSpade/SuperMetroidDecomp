using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// Verifies Ridley's delayed ejection handler, the shaft's ROM-driven Mode 7 room main,
    /// and bank-$82's exact 60-frame hold plus fifteen-step forced-blank transition.
    /// </summary>
    static void VerifyCeresEscapeHandoff()
    {
        VerifyCeresRidleyEjectionHandler();
        VerifyCeresElevatorShaftRoomMain();
        VerifyCeresDepartureDispatcherTiming();
        Console.WriteLine("  Ceres escape handoff: ejection, shaft rotation, trigger, hold, and blackout agree.");
    }

    private static void VerifyCeresRidleyEjectionHandler()
    {
        var bus = new TestAddressSpace();
        SeedPoseOneSamusData(bus);

        // Pose $53 is the right-facing knockback body installed by `$90:E12E`. Only the
        // definition and first delay byte are consumed by this focused handler fixture.
        WritePoseDefinition(
            bus,
            SamusState.KnockbackRightPose,
            [0x08, 0x0a, 0xff, 0xff, 0x00, 0x00, 0x15, 0x00]);
        WriteTestWord(
            bus,
            0x91b010 + SamusState.KnockbackRightPose * sizeof(ushort),
            0xc000);
        bus.WriteByte(0x91c000, 4);

        // `$90:DE57` selects this ordinary falling record when the shove reaches a wall.
        // Its radius is two pixels shorter than `$53`, making bottom-edge alignment an
        // observable part of the shared knockback-finish path rather than just a pose ID.
        WritePoseDefinition(
            bus,
            SamusState.FallingRightPose,
            [0x08, 0x06, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]);
        WriteTestWord(
            bus,
            0x91b010 + SamusState.FallingRightPose * sizeof(ushort),
            0xc010);
        bus.WriteBytes(0x91c010, [0x01, 0xff]);

        SamusState samus = CreateSamus(
            SamusState.FacingRightNormalPose,
            xPosition: 100,
            yPosition: 100);
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        RoomLevelData emptyRoom = CreateEmptyRoom(32, 32);

        samus.CeresRidleyEjection.Request();
        AssertTrue(samus.CeresRidleyEjection.IsPending, "Ridley ejection request is pending");
        AssertTrue(!samus.CeresRidleyEjection.IsActive, "request does not execute gamma early");
        AssertTrue(!samus.InputLocked, "request frame retains ordinary Samus input handler");

        samus.CeresRidleyEjection.BeginFrame(samus);
        AssertTrue(samus.CeresRidleyEjection.IsActive, "next frame promotes Ridley ejection");
        AssertTrue(!samus.InputLocked, "promoted ejection replaces movement but not pose input");

        CeresRidleyEjectionResult initialized = samus.CeresRidleyEjection.Step(
            bus,
            emptyRoom,
            samus,
            layer1X: 0,
            nmiFrameCounter: 0);
        AssertTrue(initialized.Initialized, "first ejection gamma initializes state");
        AssertTrue(initialized.Horizontal is null, "first gamma performs no horizontal movement");
        AssertTrue(initialized.Vertical is null, "first gamma performs no vertical movement");
        AssertEqual(SamusState.KnockbackRightPose, samus.Pose, "ejection selects pose from old facing");
        AssertEqual(1, samus.CeresRidleyEjection.PushDirection, "left-half Samus is pushed left");
        AssertEqual(5, samus.Kinematics.YSpeed, "ejection installs terminal downward speed");
        AssertSamusPosition(100, 100, samus, "first ejection gamma leaves world position unchanged");

        // The type-$0A air record supplies one pixel/frame after the first acceleration.
        // Put the radius-eight body flush against the room's left boundary so the very next
        // translated `$90:E1FD` call takes its real horizontal-collision termination path.
        WriteTestWords(
            bus,
            0x900000 | (SamusHorizontalSpeedState.NormalAirSpeedTableBaseAddress +
                0x0a * SpeedTableEntry.ByteCount),
            1, 0, 1, 0, 0, 0);
        samus.Kinematics.XPosition = samus.Kinematics.XRadius;
        CeresRidleyEjectionResult wallCollision = samus.CeresRidleyEjection.Step(
            bus,
            emptyRoom,
            samus,
            layer1X: 0,
            nmiFrameCounter: 1);
        AssertTrue(wallCollision.Ended, "room-wall contact terminates Ceres ejection");
        AssertTrue(!samus.CeresRidleyEjection.IsActive, "wall contact restores normal movement");
        AssertTrue(!samus.InputLocked, "wall contact leaves ordinary pose input available");
        AssertEqual(SamusState.FallingRightPose, samus.Pose,
            "neutral wall handoff consumes ordinary knockback-finished pose");
        AssertEqual(0, samus.KnockbackDirection,
            "shared knockback finish clears direction");
        AssertTrue(!samus.KnockbackActive,
            "shared knockback finish leaves no special movement owner");
        AssertEqual(2, samus.Kinematics.YDirection,
            "shared knockback finish publishes downward falling direction");
        AssertEqual(102, samus.Kinematics.YPosition,
            "falling radius keeps the scripted body's feet aligned");
    }

    private static void VerifyCeresElevatorShaftRoomMain()
    {
        var bus = new TestAddressSpace();
        SeedPoseOneSamusData(bus);

        // Every table record gets a zero timer so, after the door ASM's initial 60-frame
        // delay, each subsequent call exposes one phase. Distinct sine/cosine words make
        // the exact current-record selection observable without copying the retail table.
        for (int record = 0; record <= 68; record++)
        {
            WriteTestWords(
                bus,
                CeresElevatorShaftRoomMainState.RotationTableAddress + record * 6,
                0,
                unchecked((ushort)(0x0100 + record)),
                unchecked((ushort)(0x0200 + record)));
        }

        var state = new CeresElevatorShaftRoomMainState();
        state.Reset(active: true);
        SamusState outsideTrigger = CreateSamus(
            SamusState.FacingRightNormalPose,
            xPosition: 32,
            yPosition: 100);

        StepFrames(60, _ => state.Step(bus, outsideTrigger, 0x8000, allowDeparture: true));
        AssertEqual(0, state.RotationTimer, "shaft door delay retains zero for one room-main call");
        AssertEqual(
            CeresElevatorShaftRoomMainState.InitialRotationIndex,
            state.RotationIndex,
            "shaft index waits through first 60 calls");

        CeresElevatorShaftRoomMainResult firstMatrix =
            state.Step(bus, outsideTrigger, 0x8000, allowDeparture: true);
        AssertTrue(firstMatrix.MatrixChanged, "61st shaft call underflows and consumes first matrix record");
        AssertEqual(35, state.RotationIndex, "shaft index advances after record 34");
        AssertEqual(0x0222, state.Transform.MatrixA, "shaft cosine comes from record 34");
        AssertEqual(0x0122, state.Transform.MatrixB, "shaft sine comes from record 34");
        AssertEqual(
            unchecked((ushort)-0x0122),
            state.Transform.MatrixC,
            "shaft C matrix is native negated sine");

        // Consume records 35..67. The final forward phase is encoded as $8044 rather
        // than 68; one more call proves the wrapped multiplication selects record 68.
        StepFrames(33, _ => state.Step(bus, outsideTrigger, 0x8000, allowDeparture: true));
        AssertEqual(0x8044, state.RotationIndex, "shaft forward sweep enters encoded reverse phase");
        state.Step(bus, outsideTrigger, 0x8000, allowDeparture: true);
        AssertEqual(0x8043, state.RotationIndex, "shaft encoded reverse phase decrements");
        AssertEqual(0x0244, state.Transform.MatrixA, "encoded phase maps to record 68");

        // State $20/$21 still run room main but fail its explicit game-state-eight gate.
        var trigger = new CeresElevatorShaftRoomMainState();
        trigger.Reset(active: true);
        SamusState samus = CreateSamus(
            SamusState.FacingRightNormalPose,
            xPosition: 113,
            yPosition: 75);
        trigger.Step(bus, samus, 0x8000, allowDeparture: false);
        AssertTrue(!trigger.DepartureRequested, "non-gameplay dispatcher cannot trigger departure");

        CeresElevatorShaftRoomMainResult requested =
            trigger.Step(bus, samus, 0x8000, allowDeparture: true);
        AssertTrue(requested.DepartureRequestedThisFrame, "inclusive lower Y/exclusive lower X trigger admits Samus");
        AssertTrue(samus.InputLocked, "departure trigger installs SamusCode_00 lock");
        AssertEqual(SamusState.FacingRightNormalPose, samus.Pose, "departure keeps right-facing standing pose");

        CeresElevatorShaftRoomMainResult repeated =
            trigger.Step(bus, samus, 0x8000, allowDeparture: true);
        AssertTrue(!repeated.DepartureRequestedThisFrame, "departure request is a one-frame publication");
    }

    private static void VerifyCeresDepartureDispatcherTiming()
    {
        var departure = new CeresDepartureState();
        departure.Begin();
        AssertEqual(60, departure.HoldFramesRemaining, "Ceres state 20 starts at 60");

        StepFrames(59, _ =>
            AssertTrue(!departure.StepHoldAfterGameplay(), "first 59 state-20 calls retain hold"));
        AssertEqual(1, departure.HoldFramesRemaining, "state 20 has one call remaining");
        AssertTrue(departure.StepHoldAfterGameplay(), "60th state-20 call enters blackout");
        AssertEqual(CeresDeparturePhase.FadingToBlack, departure.Phase, "state 21 is active");

        StepFrames(14, _ =>
            AssertTrue(!departure.StepFadeAfterGameplay(), "first 14 fade calls are visible"));
        AssertEqual(1, departure.Brightness, "14 fade calls leave brightness one");
        AssertTrue(departure.StepFadeAfterGameplay(), "15th fade call reaches forced blank");
        AssertEqual(CeresDeparturePhase.Complete, departure.Phase, "Ceres blackout completes");

        Rgba32[] pixel = [new Rgba32(255, 120, 60, 255)];
        departure.ApplyBrightness(pixel);
        AssertEqual(new Rgba32(0, 0, 0, 255), pixel[0], "forced blank produces black software output");
    }
}
