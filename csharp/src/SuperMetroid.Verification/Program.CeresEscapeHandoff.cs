using System.Reflection;
using System.Text;
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
        Suite(nameof(VerifyCeresRidleyEjectionHandler), () => VerifyCeresRidleyEjectionHandler());
        Suite(nameof(VerifyLegacyCeresRidleyEjectionSnapshots), () => VerifyLegacyCeresRidleyEjectionSnapshots());
        Suite(nameof(VerifyCeresElevatorShaftRoomMain), () => VerifyCeresElevatorShaftRoomMain());
        Suite(nameof(VerifyCeresDepartureDispatcherTiming), () => VerifyCeresDepartureDispatcherTiming());
        Console.WriteLine("  Ceres escape handoff: ejection, shaft rotation, trigger, hold, and blackout agree.");
    }

    /// <summary>
    /// Snapshots from before the getaway moved to room main carry a retired pending flag.
    /// An idle request restores; one captured mid-deferral has no current equivalent.
    /// </summary>
    private static void VerifyLegacyCeresRidleyEjectionSnapshots()
    {
        var idle = new SamusCeresRidleyEjectionState();
        var restored = RestoreLegacyEjection(idle, pending: false);
        AssertTrue(!restored.IsActive && !restored.InitializationPending,
            "idle legacy ejection snapshot restores without the retired pending flag");
        AssertThrows<InvalidDataException>(() => RestoreLegacyEjection(idle, pending: true),
            "legacy snapshot captured mid-deferral fails loudly");

        static SamusCeresRidleyEjectionState RestoreLegacyEjection(SamusCeresRidleyEjectionState state, bool pending)
        {
            // The prior field envelope: every current field plus `<IsPending>k__BackingField`,
            // with primitive values written by the production codec.
            const byte newObjectMarker = 2, fieldPayloadKind = 5;
            Type type = typeof(SamusCeresRidleyEjectionState);
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field => !field.IsDefined(typeof(NonSerializedAttribute))).ToArray();
            using var data = new MemoryStream();
            using (var writer = new BinaryWriter(data, Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(newObjectMarker);
                writer.Write(1);
                writer.Write(SuperMetroid.Desktop.DebuggerStateTypeIdentity.GetSerializedName(type));
                writer.Write(fieldPayloadKind);
                writer.Write(fields.Length + 1);
                writer.Write(SuperMetroid.Desktop.DebuggerStateTypeIdentity.GetSerializedName(type));
                writer.Write("<IsPending>k__BackingField");
                writer.Flush();
                SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(data, pending);
                foreach (FieldInfo field in fields)
                {
                    writer.Write(SuperMetroid.Desktop.DebuggerStateTypeIdentity.GetSerializedName(field.DeclaringType!));
                    writer.Write(field.Name);
                    writer.Flush();
                    SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(data, field.GetValue(state)!);
                }
            }
            data.Position = 0;
            return SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SamusCeresRidleyEjectionState>(data);
        }
    }

    private static void VerifyCeresRidleyEjectionHandler()
    {
        var bus = new TestAddressSpace();
        SeedPoseOneSamusData(bus);

        // Pose $53 is the right-facing knockback body installed by `$90:E12E`. Only the
        // definition and first delay byte are consumed by this focused handler fixture.
        WritePoseDefinition(
            bus,
            SamusPoseIds.KnockbackRightPose,
            [0x08, 0x0a, 0xff, 0xff, 0x00, 0x00, 0x15, 0x00]);
        WriteTestWord(
            bus,
            0x91b010 + SamusPoseIds.KnockbackRightPose * sizeof(ushort),
            0xc000);
        bus.WriteByte(0x91c000, 4);

        // `$90:DE57` selects this ordinary falling record when the shove reaches a wall.
        // Its radius is two pixels shorter than `$53`, making bottom-edge alignment an
        // observable part of the shared knockback-finish path rather than just a pose ID.
        WritePoseDefinition(
            bus,
            SamusPoseIds.FallingRightPose,
            [0x08, 0x06, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]);
        WriteTestWord(
            bus,
            0x91b010 + SamusPoseIds.FallingRightPose * sizeof(ushort),
            0xc010);
        bus.WriteBytes(0x91c010, [0x01, 0xff]);

        SamusState samus = CreateSamus(
            SamusPoseIds.FacingRightNormalPose,
            xPosition: 100,
            yPosition: 100);
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        RoomLevelData emptyRoom = CreateEmptyRoom(32, 32);

        samus.PoseHistory.PreviousPose = SamusPoseIds.FacingRightNormalPose;
        samus.PoseHistory.PreviousDirectionAndMovement = 8;
        samus.PoseHistory.LastDifferentPose = SamusPoseIds.SpinJumpLeftPose;
        samus.PoseHistory.LastDifferentDirectionAndMovement = 0x0304;
        samus.CeresRidleyEjection.Request(samus);
        AssertEqual(SamusPoseIds.SpinJumpLeftPose, samus.PoseHistory.LastDifferentPose,
            "installing the ejection handler does not publish pose history early");
        AssertEqual(SamusPoseIds.FacingRightNormalPose, samus.Pose, "request does not execute gamma early");
        AssertTrue(samus.CeresRidleyEjection.IsActive, "room-main request installs Ridley ejection");
        AssertTrue(!samus.InputLocked, "installed ejection replaces movement but not pose input");

        bool initializationWasPending = samus.CeresRidleyEjection.InitializationPending;
        uint xBeforeInitialization = samus.Kinematics.XFixed;
        uint yBeforeInitialization = samus.Kinematics.YFixed;
        samus.CeresRidleyEjection.Step(
            bus,
            emptyRoom,
            samus,
            layer1X: 0,
            nmiFrameCounter: 0);
        AssertTrue(initializationWasPending && !samus.CeresRidleyEjection.InitializationPending,
            "first ejection gamma initializes state");
        AssertEqual(SamusPoseIds.FacingRightNormalPose, samus.PoseHistory.LastDifferentPose, "ejection initialization shifts prior pose");
        AssertEqual(8, samus.PoseHistory.LastDifferentDirectionAndMovement, "ejection initialization shifts prior metadata");
        AssertEqual(SamusPoseIds.KnockbackRightPose, samus.PoseHistory.PreviousPose, "ejection initialization publishes hurt pose");
        AssertEqual(0x0a08, samus.PoseHistory.PreviousDirectionAndMovement, "ejection initialization publishes hurt metadata");
        AssertEqual(xBeforeInitialization, samus.Kinematics.XFixed, "first gamma performs no horizontal movement");
        AssertEqual(yBeforeInitialization, samus.Kinematics.YFixed, "first gamma performs no vertical movement");
        AssertEqual(SamusPoseIds.KnockbackRightPose, samus.Pose, "ejection selects pose from old facing");
        AssertEqual(1, samus.CeresRidleyEjection.PushDirection, "left-half Samus is pushed left");
        AssertEqual(5, samus.Kinematics.YSpeed, "ejection installs terminal downward speed");
        AssertSamusPosition(100, 100, samus, "first ejection gamma leaves world position unchanged");

        // The type-$0A air record supplies one pixel/frame after the first acceleration.
        // Put the radius-eight body flush against the room's left boundary so the very next
        // translated `$90:E1FD` call takes its real horizontal-collision termination path.
        WriteTestWords(
            bus,
            SamusMovementRomData.Banks.Movement |
                (SamusMovementRomData.HorizontalMotion.NormalAirSpeedTable +
                0x0a * SpeedTableEntry.ByteCount),
            1, 0, 1, 0, 0, 0);
        samus.Kinematics.XPosition = samus.Kinematics.XRadius;
        // Leftover extra-run speed from before the shove (retail 100% movie: `.4000`).
        samus.HorizontalSpeed.ExtraRunSubspeed = 0x4000;
        samus.CeresRidleyEjection.Step(
            bus,
            emptyRoom,
            samus,
            layer1X: 0,
            nmiFrameCounter: 1);
        AssertEqual(0, samus.CeresRidleyEjection.PushDirection, "room-wall contact terminates Ceres ejection");
        AssertTrue(!samus.CeresRidleyEjection.IsActive, "wall contact restores normal movement");
        AssertTrue(!samus.InputLocked, "wall contact leaves ordinary pose input available");
        // The push never sets knockback direction `$0A52`, so `$90:DDE9` has no knockback
        // to finish: the hurt pose remains for the ordinary movement handler (retail 100%
        // movie: `$53` on the wall frame, then landing `$A4`).
        AssertEqual(SamusPoseIds.KnockbackRightPose, samus.Pose,
            "wall handoff keeps the hurt pose for ordinary movement");
        AssertEqual(0, samus.KnockbackDirection, "Ceres push never publishes knockback direction");
        AssertEqual(0, samus.Kinematics.YSpeed, "$90:DF85 clears Y speed");
        AssertEqual(0, samus.HorizontalSpeed.ExtraRunSubspeed,
            "Kill_SamusXSpeed_IfCollisionDetected clears extra-run speed at the wall");
        AssertEqual(0, samus.Kinematics.YDirection, "$90:DF88 clears Y direction");
        AssertEqual(100, samus.Kinematics.YPosition,
            "bottom alignment against the previous hurt pose leaves Y unchanged");

        // #1275: InitializeSamusPose_1 does not publish the hurt pose's radius, so a shove
        // that interrupts a radius-$13 airborne body raises it by 21-19 = 2 pixels and keeps
        // the live radius until the following frame's pose epilogue.
        SamusState airborne = CreateSamus(SamusPoseIds.FallingRightPose, xPosition: 100, yPosition: 100);
        airborne.RefreshCollisionRadii(bus);
        airborne.InitializeAnimation(bus);
        airborne.PoseHistory.PreviousPose = SamusPoseIds.FallingRightPose;
        airborne.CeresRidleyEjection.Request(airborne);
        airborne.CeresRidleyEjection.Step(bus, emptyRoom, airborne, layer1X: 0, nmiFrameCounter: 0);
        AssertEqual(98, airborne.Kinematics.YPosition, "ejection alignment uses the interrupted pose's live radius");
        AssertEqual(0x13, airborne.Kinematics.YRadius, "ejection initialization leaves the live radius for the pose epilogue");
    }

    private static void VerifyCeresElevatorShaftRoomMain()
    {
        var bus = new TestAddressSpace();
        SeedPoseOneSamusData(bus);

        // No rotation bytes are installed in this address space. The live matrix
        // lifecycle consumes application-owned records, not synthetic ROM artwork.

        var state = new CeresElevatorShaftRoomMainState();
        var scratch = new RoomMainScratchState();
        state.Reset(active: true, scratch);
        SamusState outsideTrigger = CreateSamus(
            SamusPoseIds.FacingRightNormalPose,
            xPosition: 32,
            yPosition: 100);

        StepFrames(60, _ => state.Step(bus, outsideTrigger, 0x8000, allowDeparture: true, scratch));
        AssertEqual(0, state.RotationTimer, "shaft door delay retains zero for one room-main call");
        AssertEqual(
            CeresElevatorShaftRoomMainState.InitialRotationIndex,
            scratch.Var1,
            "shaft index waits through first 60 calls");

        CeresElevatorShaftRoomMainResult firstMatrix =
            state.Step(bus, outsideTrigger, 0x8000, allowDeparture: true, scratch);
        AssertTrue(firstMatrix.MatrixChanged, "61st shaft call underflows and consumes first matrix record");
        AssertEqual(35, scratch.Var1, "shaft index advances after record 34");
        AssertEqual(0x0100, state.Transform.MatrixA, "shaft cosine comes from record 34");
        AssertEqual(0, state.Transform.MatrixB, "shaft sine comes from record 34");
        AssertEqual(
            0,
            state.Transform.MatrixC,
            "shaft C matrix is native negated sine");

        // Consume records 35..67. The final forward phase is encoded as $8044 rather
        // than 68; one more call proves the wrapped multiplication selects record 68.
        for (int i = 0; i < 33; i++)
            StepFrames(state.RotationTimer + 1, _ => state.Step(bus, outsideTrigger, 0x8000, allowDeparture: true, scratch));
        AssertEqual(0x8044, scratch.Var1, "shaft forward sweep enters encoded reverse phase");
        StepFrames(state.RotationTimer + 1, _ => state.Step(bus, outsideTrigger, 0x8000, allowDeparture: true, scratch));
        AssertEqual(0x8043, scratch.Var1, "shaft encoded reverse phase decrements");
        AssertEqual(0x00fe, state.Transform.MatrixA, "encoded phase maps to record 68");
        AssertEqual(34, state.Transform.MatrixB, "reverse endpoint sine");
        Suite(nameof(VerifyCeresShaftCompiledRotation), () => VerifyCeresShaftCompiledRotation());

        // State $20/$21 still run room main but fail its explicit game-state-eight gate.
        var trigger = new CeresElevatorShaftRoomMainState();
        var triggerScratch = new RoomMainScratchState();
        trigger.Reset(active: true, triggerScratch);
        SamusState samus = CreateSamus(
            SamusPoseIds.FacingRightNormalPose,
            xPosition: 113,
            yPosition: 75);
        trigger.Step(bus, samus, 0x8000, allowDeparture: false, triggerScratch);
        AssertTrue(!trigger.DepartureRequested, "non-gameplay dispatcher cannot trigger departure");

        CeresElevatorShaftRoomMainResult requested =
            trigger.Step(bus, samus, 0x8000, allowDeparture: true, triggerScratch);
        AssertTrue(requested.DepartureRequestedThisFrame, "inclusive lower Y/exclusive lower X trigger admits Samus");
        AssertTrue(samus.InputLocked, "departure trigger installs SamusCode_00 lock");
        AssertEqual(SamusPoseIds.FacingRightNormalPose, samus.Pose, "departure keeps right-facing standing pose");

        CeresElevatorShaftRoomMainResult repeated =
            trigger.Step(bus, samus, 0x8000, allowDeparture: true, triggerScratch);
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
