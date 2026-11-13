using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.AssetExtraction;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Rooms.RoomObjects.CollectiblesVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.NoobTubeObjectVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectPopulationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.RoomObjects;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

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
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.KronicBoost);

        RoomLevelData level = runtime.Room.LoadedLevelData;
        RoomObjectSystem roomObjects = runtime.RoomObjects;
        RoomObjectSlotSnapshot gate = roomObjects.PopulationSlots.Single(slot =>
            slot.HeaderPointer == RoomObjectHeaderId.DownwardGate);
        for (int warm = 0; warm < 2; warm++)
            roomObjects.Step(level);
        gate = roomObjects.PopulationSlots.Single(slot =>
            slot.HeaderPointer == RoomObjectHeaderId.DownwardGate);
        AssertTrue(gate.Behaviour is DownwardGateObject { Phase: DownwardGatePhase.ShowingClosed or DownwardGatePhase.RestingClosed },
            "the gate is closed before the shot");
        while (roomObjects.SlotTable.ActiveCount < 40)
        {
            AssertTrue(roomObjects.BlockReactions.TrySpawnProjectileShotBlock(
                    level,
                    gate.Block,
                    DownwardGateDefinitions.ClosedGateBehavior,
                    SamusProjectileType.Beam(SamusBeamCombination.Wave, charged: false),
                    solidBlock: true),
                "direct G-Mode fixture fills the native forty-slot PLM pool");
        }

        SamusState samus = runtime.Player.IntroducedSamus;
        EnterDirectGMode(samus);
        samus.Position = samus.Position.WithWhole(new(140, 360));
        samus.Pose = SamusPoseId.FacingLeftNormalPose;
        samus.EquippedBeams = SamusBeamFlags.Wave;
        samus.SelectedHudItem = HudItem.Beam;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);

        var shots = CreateProjectileFixture();
        var bombs = CreateBombFixture();
        int openedFrame = -1;
        int passedGateFrame = -1;
        for (int frame = 0; frame < 20; frame++)
        {
            if (frame == reactivateFrame)
            {
                AssertTrue(samus.Xray.TryBegin(samus, SamusMovementType.Standing, new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }),
                    $"direct G-Mode cancellation is admitted on frame {frame}");
                AssertTrue(!samus.Xray.IsGMode && !samus.Xray.AreObjectsSuspended,
                    $"frame {frame} X-Ray activation restores native subsystem words");
            }

            ControllerButton input = frame == 0 ? ControllerButton.X : 0;
            bombs.StepFrame(new SamusTerrain { Level = level }, samus, input, input);
            shots.StepFrame(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus, new ProjectileButtonInput { ControllerInput = input, ControllerNewInput = input }, cameraPosition: new(0, 224), sharedProjectiles: bombs);

            SamusProjectile shot = shots.Slots[0];
            if (!shot.Type.IsNone && shot.Position.X.Whole < 112 && passedGateFrame < 0)
                passedGateFrame = frame;

            if (!samus.Xray.AreObjectsSuspended)
            {
                roomObjects.Step(level);
            }

            RoomObjectSlotSnapshot currentGate = roomObjects.PopulationSlots.Single(slot =>
                slot.HeaderPointer == RoomObjectHeaderId.DownwardGate);
            if (currentGate.Behaviour is not DownwardGateObject { Phase: DownwardGatePhase.ShowingClosed or DownwardGatePhase.RestingClosed } && openedFrame < 0)
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
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();

        SamusState samus = runtime.Player.IntroducedSamus;
        EnterDirectGMode(samus);
        samus.Xray.TransitionDirectGModeToIndirect();
        AssertTrue(!samus.Xray.IsActive && samus.Xray.IsGMode,
            "door teardown removes direct X-Ray object but retains subsystem disables");
        AssertEqual(SamusSpecialPaletteType.None, samus.Xray.SpecialPaletteKind,
            "indirect G-Mode transition removes the stranded X-Ray palette handler");

        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.KronicBoost);
        AssertTrue(runtime.Enemies.Projectiles.EnemyProjectiles.All(projectile =>
                !projectile.IsActive || projectile.Kind is not (
                    RoomEnemyProjectileKind.DownwardGateClosed or
                    RoomEnemyProjectileKind.DownwardGateMoving)),
            "indirect G-Mode room load drops the disabled gate enemy projectile");
        AssertTrue(samus.Xray.AreObjectsSuspended && samus.Xray.AreEnemyProjectilesSuspended,
            "indirect room load retains independent PLM/enemy-projectile disable words");
    }

    /// <summary>Checks that suspended quicksand setup fills forty PLM slots and stops adding sink displacement at capacity.</summary>
    private static void VerifyGModeSandOverload()
    {
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(24, 24)),
        };
        samus.Kinematics.Radius = new(5, 8);
        EnterDirectGMode(samus);

        RoomTerrainBlock[] words = Enumerable.Repeat(RoomTerrainBlock.Of(0x000, BlockMirroring.None, RoomCollisionType.SpecialAir), 16).ToArray();
        RoomBlockBehavior[] behavior = Enumerable.Repeat(RoomBlockBehaviorCodec.Decode(0x80), 16).ToArray();
        RoomLevelData level = CreateRoom(new(4, 4), words, behavior);
        var roomObjects = RoomObjectFixtures.Unbound();

        int contact = 0;
        while (!roomObjects.Quicksand.IsAllocationFull)
        {
            contact++;
            int before = roomObjects.SlotTable.ActiveCount;
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeY(0).WithFractionY(0);
            samus.HorizontalSpeed.BaseSpeed = samus.HorizontalSpeed.BaseSpeed.WithWhole(4);
            samus.HorizontalSpeed.HasRunningMomentum = true;
            SamusInsideBlockReactions.PrepareFrame(
                level,
                samus,
                AreaId.Maridia,
                roomObjects: roomObjects);
            AssertTrue(roomObjects.SlotTable.ActiveCount > before && roomObjects.SlotTable.ActiveCount <= before + 3,
                $"G-Mode sand contact {contact} consumes its bottom/center/top suspended PLM slots");
            AssertTrue(samus.Kinematics.ExtraDisplacement.Y.RawValue != 0,
                $"G-Mode sand contact {contact} runs setup while a slot remains");
        }
        AssertEqual(14, contact,
            "standing Samus overloads forty PLM slots after fourteen three-point sand samples");

        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeY(0).WithFractionY(0);
        samus.HorizontalSpeed.BaseSpeed = samus.HorizontalSpeed.BaseSpeed.WithWhole(4);
        samus.HorizontalSpeed.HasRunningMomentum = true;
        SamusInsideBlockReactions.PrepareFrame(
            level,
            samus,
            AreaId.Maridia,
            roomObjects: roomObjects);
        AssertEqual(40, roomObjects.SlotTable.ActiveCount,
            "forty-first G-Mode sand contact cannot exceed the cartridge PLM pool");
        AssertEqual(0, samus.Kinematics.ExtraDisplacement.Y.RawValue,
            "overloaded G-Mode sand runs no setup and contributes no sinking displacement");
        AssertEqual(4, samus.HorizontalSpeed.BaseSpeed.Whole,
            "overloaded G-Mode sand no longer cancels horizontal momentum");
        AssertTrue(samus.HorizontalSpeed.HasRunningMomentum,
            "overloaded G-Mode sand preserves the running-momentum flag");

        AssertTrue(samus.Xray.TryBegin(samus, SamusMovementType.Standing, new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }),
            "using X-Ray again cancels direct G-Mode after sand overload");
        AssertTrue(!samus.Xray.AreObjectsSuspended,
            "G-Mode cancellation restores the cartridge PLM-enable word");
        roomObjects.Step(level);
        AssertEqual(0, roomObjects.SlotTable.ActiveCount,
            "first restored PLM pass executes all forty transient sand delete lists");
    }

    /// <summary>Checks that an exhausted PLM pool prevents a blue-door opening actor and its block mutation.</summary>
    private static void VerifyGModeBlueDoorAllocation()
    {
        const int width = 4;
        const int height = 4;
        const int doorIndex = 5;

        RoomLevelData DoorRoom()
        {
            var words = new RoomTerrainBlock[width * height];
            var behavior = new RoomBlockBehavior[words.Length];
            words[doorIndex] = RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.ShootableBlock);
            behavior[doorIndex] = RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(ColoredDoorOrientation.Right));
            return CreateRoom(new(width, height), words, behavior);
        }

        RoomLevelData freeLevel = DoorRoom();
        var freeObjects = RoomObjectFixtures.Unbound();
        AssertTrue(freeObjects.DoorObjects.TrySpawnBlueDoorOpening(
                freeLevel,
                freeLevel.BlockAt(doorIndex),
                RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(ColoredDoorOrientation.Right)),
                SamusProjectileType.Beam(SamusBeamCombination.Power, charged: false)),
            "G-Mode shot at a blue door allocates its inert opening actor while a slot remains");
        AssertEqual(RoomCollisionType.SolidBlock,
            freeLevel.GetCollisionBlockByIndex(doorIndex).CollisionType,
            "blue-door setup changes only the cap origin to type-eight solid before its suspended list");
        AssertEqual(1, freeObjects.SlotTable.ActiveCount,
            "suspended blue-door opening actor remains allocated");

        RoomLevelData fullLevel = DoorRoom();
        var fullObjects = RoomObjectFixtures.Unbound();
        FillSuspendedObjectPool(fullObjects);
        AssertTrue(!fullObjects.DoorObjects.TrySpawnBlueDoorOpening(
                fullLevel,
                fullLevel.BlockAt(doorIndex),
                RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(ColoredDoorOrientation.Right)),
                SamusProjectileType.Beam(SamusBeamCombination.Power, charged: false)),
            "overloaded G-Mode PLM pool drops a blue-door opening request");
        AssertEqual(RoomCollisionType.ShootableBlock,
            fullLevel.GetCollisionBlockByIndex(doorIndex).CollisionType,
            "dropped blue-door request leaves its shootable-solid cap untouched");
    }

    /// <summary>Checks default collision outcomes for several block types when their PLM reaction cannot allocate.</summary>
    private static void VerifyGModeDefaultBlockCollision()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));

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
                var words = new RoomTerrainBlock[width * height];
                var behaviors = new RoomBlockBehavior[words.Length];
                words[blockIndex] = RoomTerrainBlock.Of(0x123, BlockMirroring.None, collisionType);
                behaviors[blockIndex] = RoomBlockBehaviorCodec.Decode(bts);
                return CreateRoom(new(width, height), words, behaviors);
            }

            SamusState CreateSamus()
            {
                var result = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
                {
                    Pose = SamusPoseId.FacingRightNormalPose,
                    Position = FixedPosition<RoomPixels>.FromWhole(new(40, 54)),
                };
                result.Kinematics.Radius = new(5, 8);
                return result;
            }

            RoomLevelData controlLevel = CreateLevel();
            var controlObjects = RoomObjectFixtures.Unbound();
            SamusState controlSamus = CreateSamus();
            BlockMoveResult control = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = controlLevel, RoomObjects = controlObjects }, controlSamus.Kinematics, controlSamus, new VerticalBlockMove { Displacement = SignedSixteenSixteen.FromRaw(4 << 16), ScanLeftToRight = true });
            AssertTrue(control.Collided,
                $"available PLM allocation retains ordinary solid contact for {description}");

            RoomLevelData level = CreateLevel();
            var roomObjects = RoomObjectFixtures.Unbound();
            FillSuspendedObjectPool(roomObjects);
            SamusState samus = CreateSamus();

            BlockMoveResult result = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new VerticalBlockMove { Displacement = SignedSixteenSixteen.FromRaw(4 << 16), ScanLeftToRight = true });
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
        SeedRoomObjectPopulationRom(bus);
        const ushort population = 0x9700;
        const int width = 16;
        const int stationX = 6;
        const int stationY = 6;
        SeedRoomObjectPopulation(bus, population,
        [
            0xdf, 0xb6, stationX, stationY, 0x00, 0x00,
            0x00, 0x00,
        ]);

        RoomLevelData level = CreateRoom(new(width, 16),
            new RoomTerrainBlock[width * 16],
            new RoomBlockBehavior[width * 16]);
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Health = 1, MaxHealth = 99 };
        var roomObjects = RoomObjectFixtures.Unbound();
        RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(new SharedRandom()), AreaId.Crateria), RoomObjectPopulationImporter.Read(bus, population), new RoomObjectServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });

        var accessBlock = new Position16<RoomBlocks>(stationX + 1, stationY);
        AssertTrue(roomObjects.TryNotifyStationTouch(
                level,
                accessBlock,
                RoomBlockBehavior.Reacting(BlockReactionDefinitions.StationAccesses.ReactionFor(StationAccessBehavior.EnergyRight))),
            "G-Mode fixture reaches the resident Energy Station through its real access setup");
        AssertTrue(samus.Control.AlphaLocked,
            "station setup executes Samus command six before the suspended PLM handler");
        AssertTrue(roomObjects.StationSnapshots.Single().Triggered,
            "disabled PLM handler leaves the station activation pending");
        AssertEqual(0, roomObjects.Stations.StationActivationEvents.Count,
            "suspended station cannot publish its refill/message event");

        // A normal frame in G-Mode omits this call entirely. Executing it once represents
        // cancellation by a second X-Ray use: the pending access list finally begins, but
        // Samus remains locked through its authored insertion/refill/retraction sequence.
        roomObjects.Step(level);
        AssertTrue(!roomObjects.StationSnapshots.Single().Triggered,
            "first post-G-Mode PLM pass consumes the pending station setup");
        AssertTrue(samus.Control.AlphaLocked,
            "resumed station owns Samus until the full access sequence finishes");
    }

    /// <summary>Confirms a collectible touched during suspension is awarded after PLM processing resumes, even from a distance.</summary>
    private static void VerifyGModeRemoteItemAcquisition()
    {
        var bus = new TestAddressSpace();
        SeedCollectibleRom(bus);
        CollectibleFixture fixture = LoadCollectible(bus, header: 0xeed7, roomArgument: 17, precollected: false);
        fixture.RoomObjects.Step(fixture.Level);
        AssertTrue(fixture.RoomObjects.CollectibleObjects.TryNotifyCollectibleTouch(fixture.Block),
            "direct G-Mode contact records the visible item's native trigger");

        fixture.Samus.Position = fixture.Samus.Position.WithWhole(new(0x0200, 0x0200));
        AssertTrue(!fixture.System.HasCollectedItemBit(17),
            "suspended item PLM remains uncollected after Samus leaves its block");
        AssertEqual(0, fixture.RoomObjects.CollectibleObjects.CollectiblePickupEvents.Count,
            "suspended item PLM publishes no pickup event");

        // The omitted handler pass above represents G-Mode. Once a second X-Ray use
        // restores PLMs, the resident trigger is consumed without rechecking distance.
        fixture.RoomObjects.Step(fixture.Level);
        AssertTrue(fixture.System.HasCollectedItemBit(17),
            "first post-G-Mode handler remotely acquires the previously touched item");
        AssertEqual(1, fixture.RoomObjects.CollectibleObjects.CollectiblePickupEvents.Count,
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
        var block = new Position16<RoomBlocks>(blockX, blockY);
        SeedRoomObjectPopulation(bus, population,
        [
            0x0c, 0xd7, blockX, blockY, 0x00, 0x00,
            0x00, 0x00,
        ]);
        RoomLevelData level = CreateRoom(new(width, 16),
            new RoomTerrainBlock[width * 16],
            new RoomBlockBehavior[width * 16]);
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Position = FixedPosition<RoomPixels>.FromWhole(new(0, 0x0100)) };
        RoomFxOverlayState roomFx = CreateNoobTubeWaterFx();
        bool brokenEvent = false;
        var projectiles = new List<NoobTubeProjectileRequest>();
        var roomObjects = RoomObjectFixtures.Unbound();
        RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(new SharedRandom()), AreaId.Maridia), RoomObjectPopulationImporter.Read(bus, population), new RoomObjectServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Events = new FixtureEventFlags { HasEvent = _ => false, SetEvent = _ => brokenEvent = true }, RoomFx = new RoomObjectFx(roomFx, () => samus), Earthquake = new FixtureRoomEarthquake { WriteStart = (_, _) => { } }, Enemies = new FixtureObjectEnemyEffects { NoobTubeProjectile = projectiles.Add } });
        StepNoobTube(roomObjects, level);

        AssertTrue(roomObjects.ProjectileTriggers.TryNotifyResidentProjectileHit(
                block,
                SamusBombProjectileSystem.PowerBombType),
            "Power Bomb publishes its hit to the intact glass-tube resident");
        AssertEqual(new ResidentObjectProjectileHit(SamusProjectileFamily.PowerBomb), (roomObjects.PopulationSlots.Single().Behaviour as NoobTubeObject)?.PendingHit,
            "G-Mode-disabled tube retains the pending Power Bomb hit");
        AssertTrue(!brokenEvent && projectiles.Count == 0 && !samus.Control.AlphaLocked,
            "without a PLM handler pass the glass tube cannot arm, break, or lock Samus");
        AssertTrue((roomFx.LiquidOptions & RoomLiquidOptions.WaterPhysicsDisabledOrHorizontalHeatWave) != 0,
            "suspended glass tube leaves its water-physics disable bit intact");

        StepNoobTube(roomObjects, level);
        AssertTrue(roomObjects.PopulationSlots.Single().Behaviour is NoobTubeObject { Phase: NoobTubePhase.WaitingForInput },
            "first post-G-Mode PLM pass consumes the Power Bomb and arms the input callback");
    }

    /// <summary>Occupies all forty native PLM slots with suspended quicksand reactions.</summary>
    private static void FillSuspendedObjectPool(
        RoomObjectSystem roomObjects)
    {
        for (int slot = 0; slot < 40; slot++)
        {
            AssertTrue(roomObjects.Quicksand.TrySpawnQuicksandReaction(
                    block: Position16<RoomBlocks>.Zero,
                    reaction: SpecialAirInsideReaction.QuicksandSurface),
                $"G-Mode fixture allocates suspended PLM slot {slot}");
        }
        AssertTrue(roomObjects.Quicksand.IsAllocationFull,
            "G-Mode fixture occupies all forty native PLM slots");
    }

    /// <summary>Uses fatal X-Ray damage and reserve recovery to reach direct G-Mode with all subsystem words disabled.</summary>
    private static void EnterDirectGMode(SamusState samus)
    {
        samus.Pose = SamusPoseId.FacingLeftNormalPose;
        samus.EquippedItems |= SamusEquipmentFlags.XrayScope;
        samus.Health = 0;
        samus.MaxHealth = 99;
        samus.ReserveEnergy = 4;
        samus.MaxReserveEnergy = 100;
        samus.ReserveTankMode = ReserveTankMode.Auto;
        samus.Kinematics.YSpeed = default;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        AssertTrue(samus.Xray.TryBegin(samus, SamusMovementType.Standing, new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }),
            "direct G-Mode fixture activates X-Ray");

        var recovery = new SamusReserveAutoRecoveryState();
        recovery.Begin(samus);
        for (ushort frame = 0; recovery.IsActive; frame++)
            recovery.StepAfterFrame(samus, frame);

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
