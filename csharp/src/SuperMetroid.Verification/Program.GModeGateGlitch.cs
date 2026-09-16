using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyGModeGateGlitch()
    {
        string[] nativeRows = File.ReadAllLines(
            "csharp/test-fixtures/movement-release/gmode-gate-406.csv");
        AssertEqual(
            "reactivateFrame,passedGateFrame,activationFrame,finalX,finalY,plmFlag,activePlms",
            nativeRows[0],
            "G-Mode native fixture schema remains explicit");
        AssertEqual(4, nativeRows.Length,
            "G-Mode native fixture contains no-cancel, success, and adjacent-failure rows");
        foreach (string row in nativeRows.Skip(1))
        {
            string[] columns = row.Split(',');
            VerifyDirectGModeGateTiming(
                reactivateFrame: int.Parse(columns[0]),
                expectedPassedFrame: int.Parse(columns[1]),
                expectedOpenFrame: int.Parse(columns[2]));
        }
        VerifyIndirectGModeOmitsGateActor();
        Console.WriteLine(
            "  G-Mode gate: native full-pool passage, 4/5 reactivation boundary, and indirect actor omission pass.");
    }

    private static void VerifyDirectGModeGateTiming(
        int reactivateFrame,
        int expectedPassedFrame,
        int expectedOpenFrame)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.KronicBoost);

        RoomLevelData level = runtime.LevelData!;
        RoomPlmSystem plms = runtime.Plms;
        RoomPlmSlotSnapshot gate = plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == RoomPlmHeaders.DownwardGate);
        for (int warm = 0; warm < 2; warm++)
            plms.Step(bus, level, runtime.BackgroundStreamer!, 0, 224, 0);
        gate = plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == RoomPlmHeaders.DownwardGate);
        ushort sleepingGateInstruction = gate.InstructionPointer;
        while (plms.ActiveCount < 40)
        {
            AssertTrue(plms.TrySpawnProjectileShotBlock(
                    level,
                    gate.BlockIndex,
                    DownwardGatePlmRomData.ClosedGateBts,
                    SamusProjectileTypeWord.CreateBeam((ushort)SamusBeamFlags.Wave, charged: false),
                    solidBlock: true),
                "direct G-Mode fixture fills the native forty-slot PLM pool");
        }

        SamusState samus = runtime.Samus!;
        EnterDirectGMode(bus, samus);
        samus.XPosition = 140;
        samus.YPosition = 360;
        samus.Pose = SamusPoseIds.FacingLeftNormalPose;
        samus.EquippedBeams = (ushort)SamusBeamFlags.Wave;
        samus.SelectedHudItem = 0;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        int openedFrame = -1;
        int passedGateFrame = -1;
        for (int frame = 0; frame < 20; frame++)
        {
            if (frame == reactivateFrame)
            {
                AssertTrue(samus.Xray.TryBegin(bus, samus, SamusMovementType.Standing),
                    $"direct G-Mode cancellation is admitted on frame {frame}");
                AssertTrue(!samus.Xray.IsGMode && !samus.Xray.ArePlmsSuspended,
                    $"frame {frame} X-Ray activation restores native subsystem words");
            }

            ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
            bombs.StepFrame(bus, level, samus, input, input);
            shots.StepFrame(
                bus,
                level,
                samus,
                input,
                input,
                layer1X: 0,
                layer1Y: 224,
                bombs,
                roomPlms: plms);

            SamusProjectileSlot shot = shots.Slots[0];
            if (shot.Type != 0 && shot.XPosition < 112 && passedGateFrame < 0)
                passedGateFrame = frame;

            if (!samus.Xray.ArePlmsSuspended)
            {
                plms.Step(
                    bus,
                    level,
                    runtime.BackgroundStreamer!,
                    layer1XPosition: 0,
                    layer1YPosition: 224,
                    bg1XOffset: 0);
            }

            RoomPlmSlotSnapshot currentGate = plms.PopulationSlots.Single(slot =>
                slot.HeaderPointer == RoomPlmHeaders.DownwardGate);
            if (currentGate.InstructionPointer != sleepingGateInstruction && openedFrame < 0)
                openedFrame = frame;
        }

        AssertEqual(expectedPassedFrame, passedGateFrame,
            $"reactivation frame {reactivateFrame} matches native first full gate passage");
        AssertEqual(expectedOpenFrame, openedFrame,
            $"reactivation frame {reactivateFrame} matches native switch timing");
    }

    private static void VerifyIndirectGModeOmitsGateActor()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();

        SamusState samus = runtime.Samus!;
        EnterDirectGMode(bus, samus);
        samus.Xray.TransitionDirectGModeToIndirect();
        AssertTrue(!samus.Xray.IsActive && samus.Xray.IsGMode,
            "door teardown removes direct X-Ray object but retains subsystem disables");
        AssertEqual(SamusSpecialPaletteType.None, samus.Xray.SpecialPaletteKind,
            "indirect G-Mode transition removes the stranded X-Ray palette handler");

        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.KronicBoost);
        AssertTrue(runtime.Enemies.EnemyProjectiles.All(projectile =>
                !projectile.IsActive || projectile.Kind is not (
                    RoomEnemyProjectileKind.DownwardGateClosed or
                    RoomEnemyProjectileKind.DownwardGateMoving)),
            "indirect G-Mode room load drops the disabled gate enemy projectile");
        AssertTrue(samus.Xray.ArePlmsSuspended && samus.Xray.AreEnemyProjectilesSuspended,
            "indirect room load retains independent PLM/enemy-projectile disable words");
    }

    private static void EnterDirectGMode(ISnesAddressSpace bus, SamusState samus)
    {
        samus.Pose = SamusPoseIds.FacingLeftNormalPose;
        samus.EquippedItems = samus.EquippedItems.With(SamusEquipmentFlags.XrayScope);
        samus.Health = 0;
        samus.MaxHealth = 99;
        samus.ReserveEnergy = 4;
        samus.MaxReserveEnergy = 100;
        samus.ReserveTankMode = 1;
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        AssertTrue(samus.Xray.TryBegin(bus, samus, SamusMovementType.Standing),
            "direct G-Mode fixture activates X-Ray");

        var recovery = new SamusReserveAutoRecoveryState();
        recovery.Begin(samus);
        for (ushort frame = 0; recovery.IsActive; frame++)
            recovery.StepAfterNmi(samus, frame);

        AssertTrue(samus.Xray.IsReserveMode && samus.Xray.IsGMode,
            "Reserve completion clears only shared freeze and retains all subsystem disables");
        AssertEqual(XraySuspendedSubsystems.All, samus.Xray.SuspendedSubsystems,
            "direct G-Mode owns all four cartridge disable words");
    }
}
