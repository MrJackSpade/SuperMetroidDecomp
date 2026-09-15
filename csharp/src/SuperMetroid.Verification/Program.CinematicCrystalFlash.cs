using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyCinematicCrystalFlash(string nativeTracePath)
    {
        VerifyNativeCinematicFlashOwnership(nativeTracePath);

        // Mother Brain command `$18` preserves the Flash pointer but replaces frame-handler
        // beta with a no-op. The movement owner must therefore remain visible without being
        // called until command one restores the ordinary beta dispatcher.
        foreach (int movementCalls in new[] { 0, 5, 12 })
        {
            (SuperMetroidAddressSpace bus, SuperMetroidRuntime runtime, SamusState samus) =
                CreateCinematicFlashRuntime(movementCalls);
            CrystalFlashPhase phaseBefore = samus.CrystalFlash.Phase;
            ushort yBefore = samus.YPosition;

            var rainbow = new MotherBrainRainbowBeamAttackSequence();
            rainbow.StartActiveBeam(bus, samus);
            AssertEqual(DrainedSamusPhase.RainbowBeamLocked, samus.Drained.Phase,
                $"Mother Brain command $18 installs drained lock after {movementCalls} Flash calls");
            AssertEqual(phaseBefore, samus.CrystalFlash.Phase,
                $"Mother Brain command $18 retains Flash pointer after {movementCalls} calls");

            runtime.StepFrame(0);
            AssertTrue(runtime.LastCrystalFlashMovement is null,
                $"Mother Brain's locked beta suspends retained Flash after {movementCalls} calls");
            AssertEqual(phaseBefore, samus.CrystalFlash.Phase,
                "locked beta does not discard the suspended movement pointer");
            AssertEqual(yBefore, samus.YPosition,
                "locked beta does not execute Crystal Flash movement");
        }

        // Drive the translated Mother Brain state machine through its real command-one
        // unlock. The first restored beta pass must immediately resume the same Flash
        // pointer. A large legitimate maximum-energy value prevents the isolated sequence
        // from entering the unrelated death frontend while its 300 damage ticks run.
        (SuperMetroidAddressSpace motherBus, SuperMetroidRuntime motherRuntime,
            SamusState motherSamus) = CreateCinematicFlashRuntime(movementCalls: 12);
        motherSamus.MaxHealth = 9999;
        motherSamus.Health = 699;
        var motherRainbow = new MotherBrainRainbowBeamAttackSequence();
        motherRainbow.StartActiveBeam(motherBus, motherSamus);
        bool motherUnlocked = false;
        for (ushort frame = 0; frame < 700; frame++)
        {
            MotherBrainRainbowBeamAttackStepResult step = motherRainbow.Step(
                motherBus,
                motherSamus,
                enemyFrameCounter: frame,
                mainEnemyExecutionCounter: frame);
            motherRuntime.StepFrame(0);
            if (!step.UnlockedSamus)
                continue;

            motherUnlocked = true;
            AssertTrue(motherRuntime.LastCrystalFlashMovement is not null,
                "Mother Brain command-one unlock resumes retained Flash on the same beta pass");
            break;
        }
        AssertTrue(motherUnlocked, "Mother Brain isolated rainbow sequence reaches command-one unlock");

        // `$A9:F20E/$F21B` likewise leaves the movement pointer alone. At the terminal
        // health boundary its controller call changes Samus to drained crouching/falling
        // art; the Flash pointer nevertheless remains the beta owner and eventually gets
        // stranded in its finish handler, which is the Super Metroid *CF suit state.
        (SuperMetroidAddressSpace superBus, SuperMetroidRuntime superRuntime,
            SamusState superSamus) = CreateCinematicFlashRuntime(movementCalls: 12);
        superSamus.Drained.LetFall(superBus, superSamus);
        AssertEqual(DrainedSamusPhase.WaitingForFallingCommand, superSamus.Drained.Phase,
            "Super Metroid terminal drain installs crouching/falling controller state");
        superRuntime.StepFrame(0);
        AssertTrue(superRuntime.LastCrystalFlashMovement is not null,
            "Super Metroid drained pose does not suppress retained Flash movement");
        AssertEqual(CrystalFlashPhase.DrainingAmmo, superSamus.CrystalFlash.Phase,
            "Super Metroid interruption retains Flash ammo handler");

        // Drained animation command `$F7` later replaces the same physical movement word.
        // This is the successful suit outcome: Flash movement is gone, but its palette and
        // RTS input handler remain, making Samus immobile after the actor releases her.
        bool superInstalledFalling = false;
        for (int frame = 0; frame < 96; frame++)
        {
            superRuntime.StepFrame(0);
            if (superSamus.CrystalFlash.Phase != CrystalFlashPhase.Inactive)
                continue;
            superInstalledFalling = true;
            break;
        }
        AssertTrue(superInstalledFalling,
            "Super Metroid drained command replaces retained Flash movement");
        AssertEqual(SamusSpecialPaletteType.CrystalFlash,
            superSamus.CrystalFlash.SpecialPaletteKind,
            "successful Super Metroid timing retains Flash palette");
        AssertTrue(superSamus.CrystalFlashPoseInputLocked,
            "successful Super Metroid timing retains immobile RTS input");

        // The adjacent failure reverses the ordering: drained crouching is installed first,
        // then Flash admission replaces it. Flash completes normally, clears its input lock,
        // and therefore frees the Super Metroid stun instead of yielding a suit.
        (SuperMetroidAddressSpace failedBus, SuperMetroidRuntime failedRuntime,
            SamusState failedSamus) = CreateCinematicFlashRuntime(0, admitFlash: false);
        failedSamus.Drained.LetFall(failedBus, failedSamus);
        bool failedTimingAdmitted = failedSamus.CrystalFlash.TryBegin(
            failedBus,
            failedSamus,
            (ushort)(SnesButton.Down | SnesButton.L | SnesButton.R | SnesButton.X));
        AssertTrue(failedTimingAdmitted,
            "post-crouch adjacent timing admits the delayed Crystal Flash");
        AssertEqual(DrainedSamusPhase.Inactive, failedSamus.Drained.Phase,
            "delayed Flash replaces the earlier drained movement owner");
        for (int frame = 0; frame < 400 &&
            failedSamus.CrystalFlash.Phase != CrystalFlashPhase.Inactive; frame++)
        {
            failedRuntime.StepFrame(0);
        }
        AssertEqual(CrystalFlashPhase.Inactive, failedSamus.CrystalFlash.Phase,
            "post-crouch Flash timing completes normally");
        AssertTrue(!failedSamus.CrystalFlashPoseInputLocked,
            "post-crouch Flash timing restores input instead of retaining a suit");

        // Release the successful drained actor, then use the production X-Ray admission and
        // teardown. X-Ray replaces the retained RTS input word and must not resurrect that
        // lock after the visor closes.
        superSamus.Drained.Release(superBus, superSamus);
        for (int frame = 0; frame < 128 &&
            superSamus.Drained.Phase != DrainedSamusPhase.Inactive; frame++)
        {
            superRuntime.StepFrame(0);
        }
        AssertEqual(DrainedSamusPhase.Inactive, superSamus.Drained.Phase,
            "Super Metroid release reaches an ordinary posture");
        SamusMovementType releasedMovement = superSamus.ReadMovementType(superBus);
        bool xrayAdmitted = superSamus.Xray.TryBegin(
            superBus,
            superSamus,
            releasedMovement);
        AssertTrue(xrayAdmitted, "X-Ray admits from the released immobile suit posture");
        AssertTrue(!superSamus.CrystalFlashPoseInputLocked,
            "X-Ray replaces the retained Crystal Flash input handler");
        for (int frame = 0; frame < 16 && superSamus.Xray.IsActive; frame++)
            superSamus.Xray.StepBeam(superBus, superSamus, controllerInput: 0);
        AssertTrue(!superSamus.Xray.IsActive, "X-Ray recovery reaches ordinary teardown");
        AssertTrue(!superSamus.CrystalFlashPoseInputLocked,
            "X-Ray teardown does not restore the discarded Flash input lock");

        // Ceres is intentionally different. `$90:E119` writes `$E90E` to the physical
        // movement pointer, so all three Flash phases are permanently displaced while the
        // independent palette handler and RTS pose-input pointer survive.
        foreach (int movementCalls in new[] { 0, 5, 12 })
        {
            (_, _, SamusState ceresSamus) = CreateCinematicFlashRuntime(movementCalls);
            ceresSamus.CeresRidleyEjection.Request();
            AssertTrue(ceresSamus.CeresRidleyEjection.IsPending,
                "Ceres request remains delayed until the next frame boundary");
            AssertTrue(ceresSamus.CrystalFlash.Phase != CrystalFlashPhase.Inactive,
                "request frame still owns Flash movement");

            ceresSamus.CeresRidleyEjection.BeginFrame(ceresSamus);
            AssertEqual(CrystalFlashPhase.Inactive, ceresSamus.CrystalFlash.Phase,
                $"Ceres ejection replaces Flash movement after {movementCalls} calls");
            AssertEqual(SamusSpecialPaletteType.CrystalFlash,
                ceresSamus.CrystalFlash.SpecialPaletteKind,
                "Ceres ejection retains Crystal Flash palette ownership");
            AssertTrue(ceresSamus.CrystalFlashPoseInputLocked,
                "Ceres ejection does not replace Flash's RTS pose-input pointer");
        }

        (_, _, SamusState noFlash) = CreateCinematicFlashRuntime(
            movementCalls: 0,
            admitFlash: false);
        noFlash.CeresRidleyEjection.Request();
        noFlash.CeresRidleyEjection.BeginFrame(noFlash);
        AssertEqual(SamusSpecialPaletteType.None, noFlash.CrystalFlash.SpecialPaletteKind,
            "adjacent no-Flash Ceres route invents no retained palette");
        AssertTrue(!noFlash.CrystalFlashPoseInputLocked,
            "adjacent no-Flash Ceres route invents no input lock");

        Console.WriteLine(
            "Cinematic Crystal Flash: native ownership, Mother Brain beta suspension/resume, Super Metroid success/failure, immobility/X-Ray recovery and Ceres replacement pass.");
    }

    private static void VerifyNativeCinematicFlashOwnership(string path)
    {
        string[] lines = File.ReadAllLines(path);
        AssertEqual(42, lines.Length, "cinematic Flash native trace row count");
        var rows = lines.Skip(1)
            .Select(line => line.Split(','))
            .ToArray();

        foreach (string[] row in rows)
        {
            string encounter = row[0];
            bool flash = row[1] == "1";
            int movementCalls = int.Parse(row[2]);
            ushort before = Convert.ToUInt16(row[3], 16);
            ushort after = Convert.ToUInt16(row[4], 16);
            ushort palette = Convert.ToUInt16(row[13], 16);

            if (encounter is "mother-brain" or "super-metroid")
                AssertEqual(before, after,
                    $"native {encounter} retains movement pointer at call {movementCalls}");
            else if (encounter is "ceres-request" or "ceres-first-frame")
                AssertEqual((ushort)0xe90e, after,
                    $"native {encounter} replaces movement pointer at call {movementCalls}");

            AssertEqual(flash ? 7 : 0, palette,
                $"native {encounter} palette ownership at call {movementCalls}");
        }

        string[] nativeShitroid = rows.Single(row =>
            row[0] == "super-metroid" && row[1] == "0" && row[2] == "0");
        AssertEqual((ushort)45, Convert.ToUInt16(nativeShitroid[10], 16),
            "native unsuited Super Metroid drain subtracts four energy");

        string[] activeTiming = rows.Single(row =>
            row[0] == "mother-brain" && row[1] == "1" && row[2] == "240");
        AssertEqual((ushort)0xd6ce, Convert.ToUInt16(activeTiming[3], 16),
            "native call 240 still exposes an interruptible Flash movement pointer");
        AssertEqual((ushort)0xe90e, Convert.ToUInt16(activeTiming[5], 16),
            "native active timing retains Flash's RTS input handler");

        string[] completedTiming = rows.Single(row =>
            row[0] == "mother-brain" && row[1] == "1" && row[2] == "250");
        AssertEqual((ushort)0xa337, Convert.ToUInt16(completedTiming[3], 16),
            "native call 250 is the adjacent completed-Flash timing");
        AssertEqual((ushort)0xe913, Convert.ToUInt16(completedTiming[5], 16),
            "native completed timing has already restored ordinary input");
        AssertEqual((ushort)0xffff, Convert.ToUInt16(completedTiming[12], 16),
            "native completed timing exposes only the pending palette-restore sentinel");

        string[] postCrouchTiming = rows.Single(row =>
            row[0] == "super-metroid-before-flash" && row[2] == "400");
        AssertEqual((ushort)0xa337, Convert.ToUInt16(postCrouchTiming[4], 16),
            "native post-crouch Flash completes to ordinary movement");
        AssertEqual((ushort)0xe913, Convert.ToUInt16(postCrouchTiming[5], 16),
            "native post-crouch Flash restores ordinary input");
    }

    private static (SuperMetroidAddressSpace Bus, SuperMetroidRuntime Runtime, SamusState Samus)
        CreateCinematicFlashRuntime(int movementCalls, bool admitFlash = true)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite, cameraX: 0, cameraY: 0);
        SamusState samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.XPosition = 128;
        samus.YPosition = 128;
        samus.Health = 49;
        samus.MaxHealth = 99;
        samus.ReserveEnergy = 0;
        samus.Missiles = samus.MaxMissiles = 10;
        samus.SuperMissiles = samus.MaxSuperMissiles = 10;
        samus.PowerBombs = samus.MaxPowerBombs = 10;
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        bool admitted = samus.CrystalFlash.TryBegin(
            bus,
            samus,
            admitFlash
                ? (ushort)(SnesButton.Down | SnesButton.L | SnesButton.R | SnesButton.X)
                : (ushort)(SnesButton.Down | SnesButton.L | SnesButton.R));
        AssertEqual(admitFlash, admitted, "cinematic Flash fixture admission");
        for (ushort frame = 0; frame < movementCalls; frame++)
            samus.CrystalFlash.Step(bus, samus, frame);
        return (bus, runtime, samus);
    }
}
