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
        VerifyGModeBlueDoorAllocation();
        VerifyGModeSandOverload();
        VerifyGModeDefaultBlockCollision();
        Console.WriteLine(
            "  G-Mode: native gate timing, direct/indirect ownership, sand overload, and default block collision pass.");
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

    private static void VerifyGModeSandOverload()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState
        {
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 24,
            YPosition = 24,
        };
        samus.Kinematics.XRadius = 5;
        samus.Kinematics.YRadius = 8;
        EnterDirectGMode(bus, samus);

        ushort[] words = Enumerable.Repeat((ushort)0x3000, 16).ToArray();
        byte[] behavior = Enumerable.Repeat((byte)0x80, 16).ToArray();
        RoomLevelData level = CreateRoom(4, 4, words, behavior);
        var plms = new RoomPlmSystem();

        int contact = 0;
        while (!plms.IsAllocationFull)
        {
            contact++;
            int before = plms.ActiveCount;
            samus.Kinematics.ExtraYDisplacement = 0;
            samus.Kinematics.ExtraYSubdisplacement = 0;
            samus.HorizontalSpeed.BaseSpeed = 4;
            samus.HorizontalSpeed.HasRunningMomentum = true;
            SamusInsideBlockReactions.PrepareFrame(
                bus,
                level,
                samus,
                AreaId.Maridia,
                plms: plms);
            AssertTrue(plms.ActiveCount > before && plms.ActiveCount <= before + 3,
                $"G-Mode sand contact {contact} consumes its bottom/center/top suspended PLM slots");
            AssertTrue(samus.Kinematics.ExtraYFixed != 0,
                $"G-Mode sand contact {contact} runs setup while a slot remains");
        }
        AssertEqual(14, contact,
            "standing Samus overloads forty PLM slots after fourteen three-point sand samples");

        samus.Kinematics.ExtraYDisplacement = 0;
        samus.Kinematics.ExtraYSubdisplacement = 0;
        samus.HorizontalSpeed.BaseSpeed = 4;
        samus.HorizontalSpeed.HasRunningMomentum = true;
        SamusInsideBlockReactions.PrepareFrame(
            bus,
            level,
            samus,
            AreaId.Maridia,
            plms: plms);
        AssertEqual(40, plms.ActiveCount,
            "forty-first G-Mode sand contact cannot exceed the cartridge PLM pool");
        AssertEqual(0, samus.Kinematics.ExtraYFixed,
            "overloaded G-Mode sand runs no setup and contributes no sinking displacement");
        AssertEqual((ushort)4, samus.HorizontalSpeed.BaseSpeed,
            "overloaded G-Mode sand no longer cancels horizontal momentum");
        AssertTrue(samus.HorizontalSpeed.HasRunningMomentum,
            "overloaded G-Mode sand preserves the running-momentum flag");

        AssertTrue(samus.Xray.TryBegin(bus, samus, SamusMovementType.Standing),
            "using X-Ray again cancels direct G-Mode after sand overload");
        AssertTrue(!samus.Xray.ArePlmsSuspended,
            "G-Mode cancellation restores the cartridge PLM-enable word");
        plms.Step(bus, level, level.CreateBackgroundStreamer(), 0, 0, 0);
        AssertEqual(0, plms.ActiveCount,
            "first restored PLM pass executes all forty transient sand delete lists");
    }

    private static void VerifyGModeBlueDoorAllocation()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        const int width = 4;
        const int height = 4;
        const int doorIndex = 5;

        RoomLevelData DoorRoom()
        {
            ushort[] words = new ushort[width * height];
            byte[] behavior = new byte[words.Length];
            words[doorIndex] = 0xc123;
            behavior[doorIndex] = RoomBlockBehaviorValues.BlueDoorFacingRight.Value;
            return CreateRoom(width, height, words, behavior);
        }

        RoomLevelData freeLevel = DoorRoom();
        var freePlms = new RoomPlmSystem();
        AssertTrue(freePlms.TrySpawnBlueDoorOpening(
                freeLevel,
                doorIndex,
                RoomBlockBehaviorValues.BlueDoorFacingRight,
                SamusProjectileTypeWord.CreateBeam(0, charged: false)),
            "G-Mode shot at a blue door allocates its inert opening actor while a slot remains");
        AssertEqual(RoomCollisionType.SolidBlock,
            freeLevel.GetCollisionBlockByIndex(doorIndex).CollisionType,
            "blue-door setup changes only the cap origin to type-eight solid before its suspended list");
        AssertEqual(1, freePlms.ActiveCount,
            "suspended blue-door opening actor remains allocated");

        RoomLevelData fullLevel = DoorRoom();
        var fullPlms = new RoomPlmSystem();
        FillSuspendedPlmPool(bus, fullPlms);
        AssertTrue(!fullPlms.TrySpawnBlueDoorOpening(
                fullLevel,
                doorIndex,
                RoomBlockBehaviorValues.BlueDoorFacingRight,
                SamusProjectileTypeWord.CreateBeam(0, charged: false)),
            "overloaded G-Mode PLM pool drops a blue-door opening request");
        AssertEqual(RoomCollisionType.ShootableBlock,
            fullLevel.GetCollisionBlockByIndex(doorIndex).CollisionType,
            "dropped blue-door request leaves its shootable-solid cap untouched");
    }

    private static void VerifyGModeDefaultBlockCollision()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));

        VerifyFullPoolBlock(
            RoomCollisionType.SpecialBlock,
            bts: 0,
            expectedPass: true,
            "untouched crumble block");
        VerifyFullPoolBlock(
            RoomCollisionType.SpecialBlock,
            bts: 0x0e,
            expectedPass: true,
            "inactive Speed Booster block");
        VerifyFullPoolBlock(
            RoomCollisionType.BombableBlock,
            bts: 0,
            expectedPass: true,
            "untouched bomb block");
        VerifyFullPoolBlock(
            RoomCollisionType.ShootableBlock,
            bts: 0,
            expectedPass: false,
            "shot block");

        void VerifyFullPoolBlock(
            RoomCollisionType collisionType,
            byte bts,
            bool expectedPass,
            string description)
        {
            const int width = 8;
            const int height = 8;
            const int blockIndex = 4 * width + 2;
            RoomLevelData CreateLevel()
            {
                ushort[] words = new ushort[width * height];
                byte[] behaviors = new byte[words.Length];
                words[blockIndex] = unchecked((ushort)(((int)collisionType << 12) | 0x0123));
                behaviors[blockIndex] = bts;
                return CreateRoom(width, height, words, behaviors);
            }

            SamusState CreateSamus()
            {
                var result = new SamusState
                {
                    Pose = SamusPoseIds.FacingRightNormalPose,
                    XPosition = 40,
                    YPosition = 54,
                };
                result.Kinematics.XRadius = 5;
                result.Kinematics.YRadius = 8;
                return result;
            }

            RoomLevelData controlLevel = CreateLevel();
            var controlPlms = new RoomPlmSystem();
            SamusState controlSamus = CreateSamus();
            BlockMoveResult control = SamusBlockCollision.MoveVertical(
                bus,
                controlLevel,
                controlSamus.Kinematics,
                4 << 16,
                scanLeftToRight: true,
                plms: controlPlms);
            AssertTrue(control.Collided,
                $"available PLM allocation retains ordinary solid contact for {description}");

            RoomLevelData level = CreateLevel();
            var plms = new RoomPlmSystem();
            FillSuspendedPlmPool(bus, plms);
            SamusState samus = CreateSamus();

            BlockMoveResult result = SamusBlockCollision.MoveVertical(
                bus,
                level,
                samus.Kinematics,
                4 << 16,
                scanLeftToRight: true,
                plms: plms);
            AssertEqual(expectedPass, !result.Collided,
                $"full PLM pool gives cartridge default collision for {description}");
            AssertEqual(collisionType,
                level.GetCollisionBlockByIndex(blockIndex).CollisionType,
                $"full-pool {description} contact runs no setup mutation");
        }
    }

    private static void FillSuspendedPlmPool(
        ISnesAddressSpace bus,
        RoomPlmSystem plms)
    {
        for (int slot = 0; slot < 40; slot++)
        {
            AssertTrue(plms.TrySpawnQuicksandReaction(
                    bus,
                    blockIndex: 0,
                    header: QuicksandRomData.SurfaceInsideHeader),
                $"G-Mode fixture allocates suspended PLM slot {slot}");
        }
        AssertTrue(plms.IsAllocationFull,
            "G-Mode fixture occupies all forty native PLM slots");
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
