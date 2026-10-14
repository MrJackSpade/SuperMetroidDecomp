using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static AssertionsVerification;
using static CollectiblesVerification;
using static NoobTubePlmVerification;
using static RetailRuntimeFixtureVerification;
using static RoomPlmPopulationVerification;
using static SuiteTimingVerification;
using static SyntheticRoomCartridgeFixturesVerification;
using static VerificationHelpersVerification;

/// <summary>Checks the reported G-Mode gate route and the PLM suspension effects that drive it.</summary>
internal static class GModeGateGlitchVerification
{
    /// <summary>Compares the three native gate-timing rows and checks related suspended-PLM contracts.</summary>
    internal static void VerifyGModeGateGlitch()
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
        Suite(nameof(VerifyIndirectGModeOmitsGateActor), () => VerifyIndirectGModeOmitsGateActor());
        Suite(nameof(VerifyGModeBlueDoorAllocation), () => VerifyGModeBlueDoorAllocation());
        Suite(nameof(VerifyGModeSandOverload), () => VerifyGModeSandOverload());
        Suite(nameof(VerifyGModeStationSoftlock), () => VerifyGModeStationSoftlock());
        Suite(nameof(VerifyGModeRemoteItemAcquisition), () => VerifyGModeRemoteItemAcquisition());
        Suite(nameof(VerifyGModeNoobTubeSuspension), () => VerifyGModeNoobTubeSuspension());
        Suite(nameof(VerifyGModeDefaultBlockCollision), () => VerifyGModeDefaultBlockCollision());
        Console.WriteLine(
            "  G-Mode: native gate timing, direct/indirect ownership, sand overload, and default block collision pass.");
    }

    /// <summary>Replays one native reactivation timing row through the Kronic Boost gate room.</summary>
    private static void VerifyDirectGModeGateTiming(
        int reactivateFrame,
        int expectedPassedFrame,
        int expectedOpenFrame)
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.KronicBoost);

        RoomLevelData level = runtime.Room.LoadedLevelData;
        RoomPlmSystem plms = runtime.Plms;
        RoomPlmSlotSnapshot gate = plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == PlmHeaderId.DownwardGate);
        for (int warm = 0; warm < 2; warm++)
            plms.Step(bus, level, (runtime.Room.BackgroundStreamer ?? throw new InvalidOperationException("Verification requires runtime.Room.BackgroundStreamer.")), new PlmView { Layer1Position = new(0, 224), Bg1XOffset = 0 });
        gate = plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == PlmHeaderId.DownwardGate);
        ushort sleepingGateInstruction = gate.InstructionPointer;
        while (plms.SlotTable.ActiveCount < 40)
        {
            AssertTrue(plms.TrySpawnProjectileShotBlock(
                    level,
                    gate.BlockIndex,
                    DownwardGatePlmRomData.ClosedGateBts,
                    SamusProjectileTypeWord.CreateBeam((ushort)SamusBeamFlags.Wave, charged: false),
                    solidBlock: true),
                "direct G-Mode fixture fills the native forty-slot PLM pool");
        }

        SamusState samus = runtime.Player.IntroducedSamus;
        EnterDirectGMode(bus, samus);
        samus.Position = samus.Position.WithWhole(new(140, 360));
        samus.Pose = SamusPoseId.FacingLeftNormalPose;
        samus.EquippedBeams = (ushort)SamusBeamFlags.Wave;
        samus.SelectedHudItem = 0;
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);

        var shots = CreateProjectileFixture();
        var bombs = CreateBombFixture();
        int openedFrame = -1;
        int passedGateFrame = -1;
        for (int frame = 0; frame < 20; frame++)
        {
            if (frame == reactivateFrame)
            {
                AssertTrue(samus.Xray.TryBegin(bus, samus, SamusMovementType.Standing, new XrayBeginConditions { GameState = (ushort)SuperMetroidGameState.MainGameplay }),
                    $"direct G-Mode cancellation is admitted on frame {frame}");
                AssertTrue(!samus.Xray.IsGMode && !samus.Xray.ArePlmsSuspended,
                    $"frame {frame} X-Ray activation restores native subsystem words");
            }

            ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
            bombs.StepFrame(new SamusTerrain { Bus = bus, Level = level }, samus, input, input);
            shots.StepFrame(new SamusTerrain { Bus = bus, Level = level, Plms = plms }, samus, new ProjectileButtonInput { ControllerInput = input, ControllerNewInput = input }, layer1: new(0, 224), sharedProjectiles: bombs);

            SamusProjectileSlot shot = shots.Slots[0];
            if (shot.Type != 0 && shot.Position.X.Whole < 112 && passedGateFrame < 0)
                passedGateFrame = frame;

            if (!samus.Xray.ArePlmsSuspended)
            {
                plms.Step(bus, level, (runtime.Room.BackgroundStreamer ?? throw new InvalidOperationException("Verification requires runtime.Room.BackgroundStreamer.")), new PlmView { Layer1Position = new(0, 224), Bg1XOffset = 0 });
            }

            RoomPlmSlotSnapshot currentGate = plms.PopulationSlots.Single(slot =>
                slot.HeaderPointer == PlmHeaderId.DownwardGate);
            if (currentGate.InstructionPointer != sleepingGateInstruction && openedFrame < 0)
                openedFrame = frame;
        }

        AssertEqual(expectedPassedFrame, passedGateFrame,
            $"reactivation frame {reactivateFrame} matches native first full gate passage");
        AssertEqual(expectedOpenFrame, openedFrame,
            $"reactivation frame {reactivateFrame} matches native switch timing");
    }

    /// <summary>Confirms a door transition keeps subsystem disables while discarding the gate projectile actor.</summary>
    private static void VerifyIndirectGModeOmitsGateActor()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();

        SamusState samus = runtime.Player.IntroducedSamus;
        EnterDirectGMode(bus, samus);
        samus.Xray.TransitionDirectGModeToIndirect();
        AssertTrue(!samus.Xray.IsActive && samus.Xray.IsGMode,
            "door teardown removes direct X-Ray object but retains subsystem disables");
        AssertEqual(SamusSpecialPaletteType.None, samus.Xray.SpecialPaletteKind,
            "indirect G-Mode transition removes the stranded X-Ray palette handler");

        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.KronicBoost);
        AssertTrue(runtime.Enemies.Projectiles.EnemyProjectiles.All(projectile =>
                !projectile.IsActive || projectile.Kind is not (
                    RoomEnemyProjectileKind.DownwardGateClosed or
                    RoomEnemyProjectileKind.DownwardGateMoving)),
            "indirect G-Mode room load drops the disabled gate enemy projectile");
        AssertTrue(samus.Xray.ArePlmsSuspended && samus.Xray.AreEnemyProjectilesSuspended,
            "indirect room load retains independent PLM/enemy-projectile disable words");
    }

    /// <summary>Checks that suspended quicksand setup fills forty PLM slots and stops adding sink displacement at capacity.</summary>
    private static void VerifyGModeSandOverload()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(24, 24)),
        };
        samus.Kinematics.Radius = new(5, 8);
        EnterDirectGMode(bus, samus);

        ushort[] words = Enumerable.Repeat((ushort)0x3000, 16).ToArray();
        byte[] behavior = Enumerable.Repeat((byte)0x80, 16).ToArray();
        RoomLevelData level = CreateRoom(new(4, 4), words, behavior);
        var plms = new RoomPlmSystem();

        int contact = 0;
        while (!plms.Quicksand.IsAllocationFull)
        {
            contact++;
            int before = plms.SlotTable.ActiveCount;
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeY(0).WithFractionY(0);
            samus.HorizontalSpeed.BaseSpeed = 4;
            samus.HorizontalSpeed.HasRunningMomentum = true;
            SamusInsideBlockReactions.PrepareFrame(
                level,
                samus,
                AreaId.Maridia,
                plms: plms);
            AssertTrue(plms.SlotTable.ActiveCount > before && plms.SlotTable.ActiveCount <= before + 3,
                $"G-Mode sand contact {contact} consumes its bottom/center/top suspended PLM slots");
            AssertTrue(samus.Kinematics.ExtraDisplacement.Y.RawValue != 0,
                $"G-Mode sand contact {contact} runs setup while a slot remains");
        }
        AssertEqual(14, contact,
            "standing Samus overloads forty PLM slots after fourteen three-point sand samples");

        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeY(0).WithFractionY(0);
        samus.HorizontalSpeed.BaseSpeed = 4;
        samus.HorizontalSpeed.HasRunningMomentum = true;
        SamusInsideBlockReactions.PrepareFrame(
            level,
            samus,
            AreaId.Maridia,
            plms: plms);
        AssertEqual(40, plms.SlotTable.ActiveCount,
            "forty-first G-Mode sand contact cannot exceed the cartridge PLM pool");
        AssertEqual(0, samus.Kinematics.ExtraDisplacement.Y.RawValue,
            "overloaded G-Mode sand runs no setup and contributes no sinking displacement");
        AssertEqual((ushort)4, samus.HorizontalSpeed.BaseSpeed,
            "overloaded G-Mode sand no longer cancels horizontal momentum");
        AssertTrue(samus.HorizontalSpeed.HasRunningMomentum,
            "overloaded G-Mode sand preserves the running-momentum flag");

        AssertTrue(samus.Xray.TryBegin(bus, samus, SamusMovementType.Standing, new XrayBeginConditions { GameState = (ushort)SuperMetroidGameState.MainGameplay }),
            "using X-Ray again cancels direct G-Mode after sand overload");
        AssertTrue(!samus.Xray.ArePlmsSuspended,
            "G-Mode cancellation restores the cartridge PLM-enable word");
        plms.Step(bus, level, level.CreateBackgroundStreamer(), new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0, plms.SlotTable.ActiveCount,
            "first restored PLM pass executes all forty transient sand delete lists");
    }

    /// <summary>Checks that an exhausted PLM pool prevents a blue-door opening actor and its block mutation.</summary>
    private static void VerifyGModeBlueDoorAllocation()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        const int width = 4;
        const int height = 4;
        const int doorIndex = 5;

        RoomLevelData DoorRoom()
        {
            ushort[] words = new ushort[width * height];
            byte[] behavior = new byte[words.Length];
            words[doorIndex] = 0xc123;
            behavior[doorIndex] = RoomBlockBehaviorValues.BlueDoorFacingRight.Value;
            return CreateRoom(new(width, height), words, behavior);
        }

        RoomLevelData freeLevel = DoorRoom();
        var freePlms = new RoomPlmSystem();
        AssertTrue(freePlms.DoorPlms.TrySpawnBlueDoorOpening(
                freeLevel,
                doorIndex,
                RoomBlockBehaviorValues.BlueDoorFacingRight,
                SamusProjectileTypeWord.CreateBeam(0, charged: false)),
            "G-Mode shot at a blue door allocates its inert opening actor while a slot remains");
        AssertEqual(RoomCollisionType.SolidBlock,
            freeLevel.GetCollisionBlockByIndex(doorIndex).CollisionType,
            "blue-door setup changes only the cap origin to type-eight solid before its suspended list");
        AssertEqual(1, freePlms.SlotTable.ActiveCount,
            "suspended blue-door opening actor remains allocated");

        RoomLevelData fullLevel = DoorRoom();
        var fullPlms = new RoomPlmSystem();
        FillSuspendedPlmPool(fullPlms);
        AssertTrue(!fullPlms.DoorPlms.TrySpawnBlueDoorOpening(
                fullLevel,
                doorIndex,
                RoomBlockBehaviorValues.BlueDoorFacingRight,
                SamusProjectileTypeWord.CreateBeam(0, charged: false)),
            "overloaded G-Mode PLM pool drops a blue-door opening request");
        AssertEqual(RoomCollisionType.ShootableBlock,
            fullLevel.GetCollisionBlockByIndex(doorIndex).CollisionType,
            "dropped blue-door request leaves its shootable-solid cap untouched");
    }

    /// <summary>Checks default collision outcomes for several block types when their PLM reaction cannot allocate.</summary>
    private static void VerifyGModeDefaultBlockCollision()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));

        Suite(nameof(VerifyFullPoolBlock), () => VerifyFullPoolBlock(
            RoomCollisionType.SpecialBlock,
            bts: 0,
            expectedPass: true,
            "untouched crumble block"));
        Suite(nameof(VerifyFullPoolBlock), () => VerifyFullPoolBlock(
            RoomCollisionType.SpecialBlock,
            bts: 0x0e,
            expectedPass: true,
            "inactive Speed Booster block"));
        Suite(nameof(VerifyFullPoolBlock), () => VerifyFullPoolBlock(
            RoomCollisionType.BombableBlock,
            bts: 0,
            expectedPass: true,
            "untouched bomb block"));
        Suite(nameof(VerifyFullPoolBlock), () => VerifyFullPoolBlock(
            RoomCollisionType.ShootableBlock,
            bts: 0,
            expectedPass: false,
            "shot block"));

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
                return CreateRoom(new(width, height), words, behaviors);
            }

            SamusState CreateSamus()
            {
                var result = new SamusState
                {
                    Pose = SamusPoseId.FacingRightNormalPose,
                    Position = FixedPosition<RoomPixels>.FromWhole(new(40, 54)),
                };
                result.Kinematics.Radius = new(5, 8);
                return result;
            }

            RoomLevelData controlLevel = CreateLevel();
            var controlPlms = new RoomPlmSystem();
            SamusState controlSamus = CreateSamus();
            BlockMoveResult control = SamusBlockCollision.MoveVertical(new SamusTerrain { Bus = bus, Level = controlLevel, Plms = controlPlms }, controlSamus.Kinematics, new VerticalBlockMove { Displacement = 4 << 16, ScanLeftToRight = true });
            AssertTrue(control.Collided,
                $"available PLM allocation retains ordinary solid contact for {description}");

            RoomLevelData level = CreateLevel();
            var plms = new RoomPlmSystem();
            FillSuspendedPlmPool(plms);
            SamusState samus = CreateSamus();

            BlockMoveResult result = SamusBlockCollision.MoveVertical(new SamusTerrain { Bus = bus, Level = level, Plms = plms }, samus.Kinematics, new VerticalBlockMove { Displacement = 4 << 16, ScanLeftToRight = true });
            AssertEqual(expectedPass, !result.Collided,
                $"full PLM pool gives cartridge default collision for {description}");
            AssertEqual(collisionType,
                level.GetCollisionBlockByIndex(blockIndex).CollisionType,
                $"full-pool {description} contact runs no setup mutation");
        }
    }

    /// <summary>Confirms station touch locks Samus while the suspended refill sequence remains pending.</summary>
    private static void VerifyGModeStationSoftlock()
    {
        var bus = new TestAddressSpace();
        SeedRoomPlmPopulationRom(bus);
        const ushort population = 0x9700;
        const int width = 16;
        const int stationX = 6;
        const int stationY = 6;
        SeedRoomPlmPopulation(bus, population,
        [
            0xdf, 0xb6, stationX, stationY, 0x00, 0x00,
            0x00, 0x00,
        ]);

        RoomLevelData level = CreateRoom(new(width, 16),
            new ushort[width * 16],
            new byte[width * 16],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var samus = new SamusState { Health = 1, MaxHealth = 99 };
        var plms = new RoomPlmSystem();
        plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, streamer, new SnesVram(), new Bank80SystemState(), AreaId.Crateria), RoomPlmPopulationImporter.Read(bus, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });

        int accessBlock = level.GetBlockIndex(new(stationX + 1, stationY));
        AssertTrue(plms.TryNotifyStationTouch(
                accessBlock,
                new RoomBlockBehavior((byte)StationAccessBehavior.EnergyRight)),
            "G-Mode fixture reaches the resident Energy Station through its real access setup");
        AssertTrue(samus.InputLocked,
            "station setup executes Samus command six before the suspended PLM handler");
        AssertTrue(plms.StationSnapshots.Single().Triggered,
            "disabled PLM handler leaves the station activation pending");
        AssertEqual(0, plms.Stations.StationActivationEvents.Count,
            "suspended station cannot publish its refill/message event");

        // A normal frame in G-Mode omits this call entirely. Executing it once represents
        // cancellation by a second X-Ray use: the pending access list finally begins, but
        // Samus remains locked through its authored insertion/refill/retraction sequence.
        plms.Step(bus, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertTrue(!plms.StationSnapshots.Single().Triggered,
            "first post-G-Mode PLM pass consumes the pending station setup");
        AssertTrue(samus.InputLocked,
            "resumed station owns Samus until the full access sequence finishes");
    }

    /// <summary>Confirms a collectible touched during suspension is awarded after PLM processing resumes, even from a distance.</summary>
    private static void VerifyGModeRemoteItemAcquisition()
    {
        var bus = new TestAddressSpace();
        SeedCollectibleRom(bus);
        CollectibleFixture fixture = LoadCollectible(bus, header: 0xeed7, roomArgument: 17, precollected: false);
        fixture.Plms.Step(bus, fixture.Level, fixture.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertTrue(fixture.Plms.CollectiblePlms.TryNotifyCollectibleTouch(fixture.BlockIndex),
            "direct G-Mode contact records the visible item's native trigger");

        fixture.Samus.Position = fixture.Samus.Position.WithWhole(new(0x0200, 0x0200));
        AssertTrue(!fixture.System.HasCollectedItemBit(17),
            "suspended item PLM remains uncollected after Samus leaves its block");
        AssertEqual(0, fixture.Plms.CollectiblePlms.CollectiblePickupEvents.Count,
            "suspended item PLM publishes no pickup event");

        // The omitted handler pass above represents G-Mode. Once a second X-Ray use
        // restores PLMs, the resident trigger is consumed without rechecking distance.
        fixture.Plms.Step(bus, fixture.Level, fixture.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertTrue(fixture.System.HasCollectedItemBit(17),
            "first post-G-Mode handler remotely acquires the previously touched item");
        AssertEqual(1, fixture.Plms.CollectiblePlms.CollectiblePickupEvents.Count,
            "remote acquisition publishes exactly one cartridge pickup event");
    }

    /// <summary>Checks that a Power Bomb mark on the glass tube waits for the restored PLM pass.</summary>
    private static void VerifyGModeNoobTubeSuspension()
    {
        var bus = new TestAddressSpace();
        SeedNoobTubeRom(bus);
        const ushort population = 0x9400;
        const byte blockX = 2;
        const byte blockY = 2;
        const int width = 16;
        int blockIndex = blockY * width + blockX;
        SeedRoomPlmPopulation(bus, population,
        [
            0x0c, 0xd7, blockX, blockY, 0x00, 0x00,
            0x00, 0x00,
        ]);
        RoomLevelData level = CreateRoom(new(width, 16),
            new ushort[width * 16],
            new byte[width * 16],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(0, 0x0100)) };
        RoomLayer3FxState roomFx = CreateNoobTubeWaterFx(bus);
        bool brokenEvent = false;
        var projectiles = new List<NoobTubeProjectileRequest>();
        var plms = new RoomPlmSystem();
        plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, streamer, new SnesVram(), new Bank80SystemState(), AreaId.Maridia), RoomPlmPopulationImporter.Read(bus, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Events = new FixtureEventFlags { HasEvent = _ => false, SetEvent = _ => brokenEvent = true }, RoomFx = roomFx, Earthquake = new FixtureRoomEarthquake { WriteTimer = _ => { }, WriteType = _ => { } }, Enemies = new FixturePlmEnemyEffects { NoobTubeProjectile = projectiles.Add } });
        StepNoobTube(plms, bus, level, streamer);

        AssertTrue(plms.ProjectileTriggers.TryNotifyResidentProjectileHit(
                blockIndex,
                SamusBombProjectileSystem.PowerBombType),
            "Power Bomb publishes its hit to the intact glass-tube resident");
        AssertEqual((ushort)0x8300, plms.PopulationSlots.Single().LoopTimer,
            "G-Mode-disabled tube retains the marked Power Bomb request");
        AssertTrue(!brokenEvent && projectiles.Count == 0 && !samus.InputLocked,
            "without a PLM handler pass the glass tube cannot arm, break, or lock Samus");
        AssertTrue((roomFx.LiquidOptions & RoomFxRomData.Water.PhysicsDisabledOption) != 0,
            "suspended glass tube leaves its water-physics disable bit intact");

        StepNoobTube(plms, bus, level, streamer);
        AssertEqual((ushort)NoobTubePlmPreInstruction.WakeOnAcceptedInput,
            plms.PopulationSlots.Single().PreInstruction,
            "first post-G-Mode PLM pass consumes the Power Bomb and arms the input callback");
    }

    /// <summary>Occupies all forty native PLM slots with suspended quicksand reactions.</summary>
    private static void FillSuspendedPlmPool(
        RoomPlmSystem plms)
    {
        for (int slot = 0; slot < 40; slot++)
        {
            AssertTrue(plms.Quicksand.TrySpawnQuicksandReaction(
                    blockIndex: 0,
                    header: PlmHeaderId.InsideReactionQuicksandSurface),
                $"G-Mode fixture allocates suspended PLM slot {slot}");
        }
        AssertTrue(plms.Quicksand.IsAllocationFull,
            "G-Mode fixture occupies all forty native PLM slots");
    }

    /// <summary>Uses fatal X-Ray damage and reserve recovery to reach direct G-Mode with all subsystem words disabled.</summary>
    private static void EnterDirectGMode(ISnesAddressSpace bus, SamusState samus)
    {
        samus.Pose = SamusPoseId.FacingLeftNormalPose;
        samus.EquippedItems = samus.EquippedItems.With(SamusEquipmentFlags.XrayScope);
        samus.Health = 0;
        samus.MaxHealth = 99;
        samus.ReserveEnergy = 4;
        samus.MaxReserveEnergy = 100;
        samus.ReserveTankMode = 1;
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0;
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        AssertTrue(samus.Xray.TryBegin(bus, samus, SamusMovementType.Standing, new XrayBeginConditions { GameState = (ushort)SuperMetroidGameState.MainGameplay }),
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

    /// <summary>Runs the focused G-Mode gate-glitch verification suite.</summary>
    [VerificationCommand("--gmode-gate-glitch")]
    internal static int GmodeGateGlitchCommand()
    {
        Suite(nameof(VerifyGModeGateGlitch), () => VerifyGModeGateGlitch());
        return 0;
    }
}
