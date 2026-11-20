using SuperMetroid.AssetExtraction;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.RoomObjects.DownwardGateVisualsVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.LookupDownwardGateDrawsVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.LookupDownwardGateSelectionVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.RoomObjects;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Focused room-PLM and bank-$86 actor checks for downward gates and their shot triggers.</summary>
internal static class DownwardGateObjectsVerification
{
    /// <summary>Compares native selection tables, then proves every production setup row works without reading those source tables.</summary>
    internal static void VerifyDownwardGateShotBlockDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyDownwardGateListSelection), () => VerifyDownwardGateListSelection(rom));
        Suite(nameof(VerifyDownwardGateLeftSelection), () => VerifyDownwardGateLeftSelection(rom));
        Suite(nameof(VerifyDownwardGateRightSelection), () => VerifyDownwardGateRightSelection(rom));

        // The synthetic address space intentionally omits all three source tables. Running
        // every row through production setup proves room loading no longer reads them.
        Suite(nameof(VerifyDownwardGateObjects), () => VerifyDownwardGateObjects());
    }

    /// <summary>
    /// Checks the resident gate's column test as a plain range (#2095): Samus lies inside a gate standing at row 2 from
    /// its top row through the fifth row down, in its own column only. The rows above its top, which the cartridge's
    /// unsigned compare reads as far below, and the row past its fifth lie outside it.
    /// </summary>
    internal static void VerifyDownwardGateColumn()
    {
        SamusState? samus = null;
        var places = new RoomObjectPlaceList(new SuperMetroid.Core.Compatibility.RoomObjectPlaces());
        var gates = new GateObjects(new FixtureRoomSamus(() => samus ?? throw new InvalidOperationException("The column check places Samus before it reads her.")), places, new RoomObjectSoundQueue(GameplayAudioRequests.Standalone()));
        RoomObjectPlace gate = places.Spawn() ?? throw new InvalidOperationException("An empty room has a place for the gate.");
        gate.Block = new Position16<RoomBlocks>(6, 2);
        foreach ((int column, int row, bool inside) in new[]
        {
            (6, 0, false), (6, 1, false), (6, 2, true), (6, 4, true), (6, 6, true), (6, 7, false), (5, 3, false), (7, 3, false),
        })
        {
            samus = CreateSamus(SuperMetroid.Core.Samus.Poses.SamusPoseId.ForwardFacingPowerSuitPose, new Position16<RoomPixels>(column * 16 + 8, row * 16 + 8));
            AssertEqual(inside, gates.IsSamusInsideDownwardGateColumn(gate),
                $"Samus in block ({column}, {row}) lies {(inside ? "inside" : "outside")} the column of the gate at (6, 2)");
        }
        Console.WriteLine("Downward gate column: Samus wakes a gate only from its own column, its top row through its fifth.");
    }

    /// <summary>
    /// Reproduces all eight gate-trigger dispatches in constructed rooms and checks the
    /// resident gate plus bank-$86 actor handoff independently of any long controller route.
    /// </summary>
    internal static void VerifyDownwardGateObjects()
    {
        Suite(nameof(VerifyDownwardGateHeaderDefinitions), () => VerifyDownwardGateHeaderDefinitions());
        Suite(nameof(VerifyDownwardGateProgramDefinitions), () => VerifyDownwardGateProgramDefinitions());
        Suite(nameof(VerifyDownwardGateDrawDefinitions), () => VerifyDownwardGateDrawDefinitions());
        Suite(nameof(VerifyDownwardGateVisuals), () => VerifyDownwardGateVisuals());
        Suite(nameof(VerifyDownwardGateSetupAndProjectile), () => VerifyDownwardGateSetupAndProjectile());
        Suite(nameof(VerifyDownwardGateColumn), () => VerifyDownwardGateColumn());

        var cases = new DownwardGateObjectsCase[]
        {
            new DownwardGateObjectsCase(new DownwardGateTrigger(DownwardGateTriggerColor.Blue, HorizontalDirection.Left), 0x0004, true),
            new DownwardGateObjectsCase(new DownwardGateTrigger(DownwardGateTriggerColor.Blue, HorizontalDirection.Right), 0x0300, false),
            new DownwardGateObjectsCase(new DownwardGateTrigger(DownwardGateTriggerColor.Green, HorizontalDirection.Left), 0x0200, true),
            new DownwardGateObjectsCase(new DownwardGateTrigger(DownwardGateTriggerColor.Green, HorizontalDirection.Right), 0x0000, false),
            new DownwardGateObjectsCase(new DownwardGateTrigger(DownwardGateTriggerColor.Red, HorizontalDirection.Left), 0x0100, true),
            new DownwardGateObjectsCase(new DownwardGateTrigger(DownwardGateTriggerColor.Red, HorizontalDirection.Right), 0x0200, true),
            new DownwardGateObjectsCase(new DownwardGateTrigger(DownwardGateTriggerColor.Yellow, HorizontalDirection.Left), 0x0300, true),
            // This counterintuitive inequality is present in the retail right-hand routine.
            new DownwardGateObjectsCase(new DownwardGateTrigger(DownwardGateTriggerColor.Yellow, HorizontalDirection.Right), 0x0000, true),
        };
        foreach ((DownwardGateTrigger trigger, ushort projectile, bool accepted) in cases)
            VerifyDownwardGateTrigger(trigger, projectile, accepted);

        Console.WriteLine(
            "  Downward gates: setup, slot order, actor handoff, and all eight shot filters agree.");
    }

    /// <summary>
    /// Checks both gate PLMs are compiled population headers named by the cartridge's header words. The compiled header
    /// record no longer carries the first instruction word (#1414); this check now asserts each gate's identity survives
    /// the compiled lookup and translates to the header whose first-list word sits at its source.
    /// </summary>
    private static void VerifyDownwardGateHeaderDefinitions()
    {
        foreach ((RoomObjectKind header, int source) in new[] { new VerifyDownwardGateHeaderDefinitionsCase(RoomObjectKind.DownwardGate, 0x84c82c), new VerifyDownwardGateHeaderDefinitionsCase(RoomObjectKind.DownwardGateShotBlock, 0x84c838) })
        {
            AssertEqual(unchecked((ushort)(source - 2)), RoomObjectNativeWords.Word(header), "Gate header word precedes its first-list word");
        }
    }

    /// <summary>Runs retail geometry, collision-word, and editable visual comparisons for gate draw lists.</summary>
    private static void VerifyDownwardGateDrawDefinitions()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Gate draw oracle revision");
        Suite(nameof(VerifyDownwardGateDrawGeometry), () => VerifyDownwardGateDrawGeometry(rom));
        Suite(nameof(VerifyDownwardGateDrawCollision), () => VerifyDownwardGateDrawCollision(rom));
        Suite(nameof(VerifyDownwardGateDrawVisuals), () => VerifyDownwardGateDrawVisuals(rom));
    }

    /// <summary>Compares the downward gate's typed programs with its retail instruction lists.</summary>
    private static void VerifyDownwardGateProgramDefinitions()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Gate program oracle revision");
        Suite(nameof(RoomObjectProgramsVerification.VerifyDownwardGatePrograms), () => RoomObjectProgramsVerification.VerifyDownwardGatePrograms(rom));
    }

    /// <summary>Checks resident place order, gate collision, closed-actor spawning, shot wake, four-tile opening, and delayed moving-close actor.</summary>
    private static void VerifyDownwardGateSetupAndProjectile()
    {
        (TestAddressSpace bus, RoomLevelData level, RoomObjectSystem roomObjects, int gateBlockIndex) = CreateDownwardGateFixture(
                new DownwardGateTrigger(DownwardGateTriggerColor.Blue, HorizontalDirection.Left));

        AssertEqual(2, roomObjects.Places.ActiveCount, "gate and shot-block records retain separate PLM slots");
        RoomObjectPlaceSnapshot[] places = roomObjects.PopulationPlaces.ToArray();
        AssertEqual(0, places[0].UpdatePosition, "gate is processed first");
        AssertTrue(places[0].Behaviour is DownwardGateObject,
            "first resident slot holds the closed downward gate");
        AssertEqual(1, places[1].UpdatePosition, "shot block is processed next");
        AssertTrue(places[1].Behaviour is TerrainChangeObject { Change: var triggerChange }
                && triggerChange == DownwardGateTriggerDefinitions.Triggers[new DownwardGateTrigger(DownwardGateTriggerColor.Blue, HorizontalDirection.Left)].Change,
            "argument zero selects the blue-left trigger's draw");

        for (int row = 0; row < DownwardGateDefinitions.GateHeightInBlocks; row++)
        {
            AssertEqual(DownwardGateDefinitions.ClosedGateBehavior,
                level.GetCollisionBlockByIndex(gateBlockIndex + row * level.Size.X).Behavior,
                $"gate setup writes BTS $10 to row {row}");
        }
        AssertEqual((int)RoomCollisionType.ShootableBlock,
            (int)level.GetCollisionBlockByIndex(gateBlockIndex - 1).CollisionType,
            "left trigger table installs a shootable block");
        AssertEqual(RoomObjectBlockBehaviorNativeWords.DownwardGateTriggers.Word(new DownwardGateTrigger(DownwardGateTriggerColor.Blue, HorizontalDirection.Left)),
            RoomBlockBehaviorCodec.Encode(level.GetCollisionBlockByIndex(gateBlockIndex - 1).Behavior),
            "argument zero installs retail blue-left BTS $46");

        DownwardGateProjectileRequest request = roomObjects.Gates.TakeDownwardGateProjectileRequests().Single();
        AssertTrue(request is DownwardGateProjectileSpawn { Kind: DownwardGateProjectileKind.Closed },
            "closed gate setup publishes one spawn of the initially closed gate projectile");

        // The production room loader consumes this request after Enemies.Load has cleared
        // the shared projectile pool. Recreate that exact boundary and inspect initializer E.
        SeedEmptyRoomEnemyPopulation(bus, 0x9600);
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: bus, services: new StandaloneRoomEnemyServices(() => 0));
        enemies.Load(RoomEnemyFrameFixtures.StandaloneLoad(bus.ReadEnemyPopulation(0x9600), BgArtworkFixtures.For(new SnesVram()), level, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())));
        enemies.DownwardGateBehavior.ApplyDownwardGateProjectileRequest(request);
        EnemyProjectile actor = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive);
        AssertEqual(RoomEnemyProjectileKind.DownwardGateClosed, actor.Kind,
            "bank-$86 pool owns the closed gate actor");
        AssertEqual(level.BlockAt(gateBlockIndex), actor.StateOf<DownwardGateState>().RoomObjectBlock,
            "gate actor is tied to its gate's PLM block");
        AssertEqual((ushort)96, actor.Position.Y.Whole,
            "closed gate actor begins four tiles beneath a row-two PLM origin");
        enemies.StepEnemyProjectiles(level, samus: RoomEnemyFrameFixtures.StandaloneSamus());
        enemies.StepEnemyProjectiles(level, samus: RoomEnemyFrameFixtures.StandaloneSamus());
        AssertTrue(actor.Animation is DownwardGateProjectileAnimation { PlayingSegment: DownwardGateProjectileSegment.Closed },
            "closed gate actor holds its closed program's sleep before a shot can wake it");

        // The closed resident list first draws its collision state and then sleeps under
        // $BB6B. The Green Hill Zone fixture uses room argument zero, so an ordinary beam
        // must wake its blue-left gate exactly as it does in room $8F:9E52.
        StepDownwardGateObject(roomObjects, level);
        StepDownwardGateObject(roomObjects, level);
        AssertEqual(new RoomTerrainBlock(RoomBlockAppearance.Nothing, RoomCollisionType.ShootableBlock),
            level.GetCollisionBlockByIndex(gateBlockIndex + level.Size.X).Terrain,
            "compiled initial gate draw installs the second solid column word");
        AssertTrue(roomObjects.PopulationPlaces.Single(place => place.Behaviour is DownwardGateObject).Behaviour is
                DownwardGateObject { Phase: DownwardGatePhase.RestingClosed, Resting: true } restingGate &&
                restingGate.Animation.Shown == DownwardGateMovementDefinitions.Closed,
            "a gate that has not moved rests on its closed image, not on an image of a movement (#1627)");
        AssertTrue(roomObjects.Gates.TrySpawnDownwardGateTrigger(
                level,
                level.BlockAt(gateBlockIndex - 1),
                level.GetCollisionBlockByIndex(gateBlockIndex - 1).Behavior,
                BlockStrike.By(SamusProjectileType.Beam(SamusBeamCombination.Spazer, charged: false))),
            "power beam reaches the sleeping blue gate");
        StepDownwardGateObject(roomObjects, level);
        AssertTrue(roomObjects.PopulationPlaces.Single(place => place.Behaviour is DownwardGateObject).Behaviour is
                DownwardGateObject { Phase: DownwardGatePhase.Reopening, Resting: false } wakingGate &&
                wakingGate.Animation.Shown == DownwardGateMovementDefinitions.Reopening[0],
            "the waking update shows the reopening's first image (#1627)");
        DownwardGateProjectileRequest wake = roomObjects.Gates.TakeDownwardGateProjectileRequests().Single();
        AssertTrue(wake is DownwardGateProjectileWake, "closed gate publishes a wake request");
        AssertTrue(roomObjects.Sounds.SoundRequests.Any(sound =>
                sound.SoundEffect == DownwardGateDefinitions.MovementSound),
            "opening gate queues its cartridge movement sound");
        enemies.DownwardGateBehavior.ApplyDownwardGateProjectileRequest(wake);

        // Run the PLM and bank-$86 actor together until the open-state coroutine sleeps.
        // This exercises all four 16-pixel movement segments and the list's final goto.
        for (int frame = 0; frame < 96; frame++)
        {
            StepDownwardGateObject(roomObjects, level);
            enemies.StepEnemyProjectiles(level, samus: RoomEnemyFrameFixtures.StandaloneSamus());
        }
        AssertEqual(0, enemies.ActiveEnemyProjectileCount,
            "opened gate actor retracts four tiles and deletes");
        AssertEqual(new RoomTerrainBlock(RoomBlockAppearance.Nothing, RoomCollisionType.Air),
            level.GetCollisionBlockByIndex(gateBlockIndex + level.Size.X).Terrain,
            "compiled final open draw clears the second column collision word");
        RoomObjectPlaceSnapshot openedGate = roomObjects.PopulationPlaces.Single(place =>
            place.Behaviour is DownwardGateObject);
        AssertTrue(openedGate.Behaviour is DownwardGateObject { Phase: DownwardGatePhase.RestingOpen },
            "open gate rests, waiting only for a trigger");

        AssertTrue(roomObjects.Gates.TrySpawnDownwardGateTrigger(
                level,
                level.BlockAt(gateBlockIndex - 1),
                level.GetCollisionBlockByIndex(gateBlockIndex - 1).Behavior,
                BlockStrike.By(SamusProjectileType.Beam(SamusBeamCombination.Spazer, charged: false))),
            "second power beam reaches the sleeping open blue gate");
        // The open-state collision image remains for sixteen frames before $BBE1 spawns
        // the downward-moving actor, exactly as list $BC13 specifies.
        for (int frame = 0; frame < 17; frame++)
            StepDownwardGateObject(roomObjects, level);
        DownwardGateProjectileRequest close = roomObjects.Gates.TakeDownwardGateProjectileRequests().Single();
        AssertTrue(close is DownwardGateProjectileSpawn { Kind: DownwardGateProjectileKind.Moving },
            "open gate publishes a spawn of the moving gate projectile");
        enemies.DownwardGateBehavior.ApplyDownwardGateProjectileRequest(close);
        for (int frame = 0; frame < 80; frame++)
            enemies.StepEnemyProjectiles(level, samus: RoomEnemyFrameFixtures.StandaloneSamus());
        EnemyProjectile closedActor = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive);
        AssertEqual((ushort)96, closedActor.Position.Y.Whole,
            "moving gate travels four tiles down and remains parked closed");
    }

    /// <summary>Checks one color/direction BTS row's projectile acceptance and physical shot-block draw.</summary>
    private static void VerifyDownwardGateTrigger(
        DownwardGateTrigger trigger,
        ushort projectile,
        bool accepted)
    {
        (TestAddressSpace bus, RoomLevelData level, RoomObjectSystem roomObjects, int gateBlockIndex) =
            CreateDownwardGateFixture(trigger);
        // The native trigger parameters alternate left (even) and right (odd), one pair per colour from blue.
        int nativeTrigger = RoomObjectBlockBehaviorNativeWords.DownwardGateTriggers.Word(trigger);
        int triggerBlockIndex = (nativeTrigger & 1) == 0
            ? gateBlockIndex - 1
            : gateBlockIndex + 1;
        RoomBlockBehavior bts = level.GetCollisionBlockByIndex(triggerBlockIndex).Behavior;
        AssertTrue(roomObjects.Gates.TrySpawnDownwardGateTrigger(level, level.BlockAt(triggerBlockIndex), bts, BlockStrike.By(SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(projectile))),
            $"{trigger} is claimed by the gate dispatcher");
        RoomObjectPlaceSnapshot gate = roomObjects.PopulationPlaces.Single(place =>
            place.Behaviour is DownwardGateObject);
        AssertEqual(accepted, gate.Behaviour is DownwardGateObject { Triggered: true },
            $"{trigger} projectile ${projectile:X4} acceptance matches cartridge");
        StepDownwardGateObject(roomObjects, level);
        StepDownwardGateObject(roomObjects, level);
        int color = (nativeTrigger - RoomObjectBlockBehaviorNativeWords.DownwardGateTriggers.Word(new DownwardGateTrigger(DownwardGateTriggerColor.Blue, HorizontalDirection.Left))) / 2;
        RoomTerrainBlock expectedTriggerWord = RoomTerrainBlock.Of(0x0db - color,
            (nativeTrigger & 1) == 0 ? BlockMirroring.None : BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock);
        AssertEqual(expectedTriggerWord,
            level.GetCollisionBlockByIndex(triggerBlockIndex).Terrain,
            $"{trigger} compiled shot-trigger draw installs its physical block word");
        AssertTrue(roomObjects.PopulationPlaces.All(place =>
                !place.HoldsDownwardGateShotBlock),
            $"{trigger} trigger list draws once and deletes without ROM control bytes");
    }

    /// <summary>
    /// Loads a gate and adjacent shot block at room cell (6,2) with compiled headers and synthetic actor instructions,
    /// drawing with the repository installation's pictures.
    /// </summary>
    internal static CreateDownwardGateFixtureResult
        CreateDownwardGateFixture(DownwardGateTrigger trigger)
    {
        var bus = new TestAddressSpace();
        const ushort populationPointer = 0x9400;
        const byte gateX = 6;
        const byte gateY = 2;
        const int roomWidth = 16;
        ushort argument = unchecked((ushort)((RoomObjectBlockBehaviorNativeWords.DownwardGateTriggers.Word(trigger) - 0x46) * 2));
        // Supply imported placements directly. Cartridge header reads belong to import;
        // the guard below prohibits them only while the production runtime sets up slots.
        var population = new RoomObjectPopulation($"$8F:{populationPointer:X4}",
        [
            RoomObjectPlacementAccess.Placed(RoomObjectKind.DownwardGate,
                new(gateX, gateY)),
            RoomObjectPlacementAccess.Placed(RoomObjectKind.DownwardGateShotBlock,
                new(gateX, gateY), RoomObjectRoomArgumentWords.Decode(RoomObjectKind.DownwardGateShotBlock, argument)),
        ]);
        SeedDownwardGateProjectileRom(bus);

        RoomLevelData level = CreateRoom(new(roomWidth, 16),
            new RoomTerrainBlock[roomWidth * 16],
            new RoomBlockBehavior[roomWidth * 16]);
        var roomObjects = RoomObjectFixtures.Unbound();
        // A new Samus at the room's origin, outside the gate's column, which the gate's wake check reads.
        SamusState outsideColumn = RoomEnemyFrameFixtures.StandaloneSamus();
        RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, new SharedGameState(new SharedRandom()), AreaId.Brinstar), population, RoomObjectServiceFixtures.Uninstalled with { Samus = new FixtureRoomSamus(() => outsideColumn), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
        return new CreateDownwardGateFixtureResult(bus, level, roomObjects, gateY * roomWidth + gateX);
    }


    /// <summary>Advances one gate PLM frame with stationary view and no scroll, enemy-death, or controller events.</summary>
    internal static void StepDownwardGateObject(
        RoomObjectSystem roomObjects,
        RoomLevelData level) =>
        roomObjects.Step(level, new RoomObjectFrameInputs { Scrolls = SuperMetroid.Core.Rooms.RoomObjects.RoomObjectFixtures.UnreadScrolls(), EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0, CollectedItems = SamusEquipmentFlags.None, PowerBombExplosionActive = false });

    /// <summary>Seeds exact bank-$86 closed/moving actor definitions and their shared movement instruction span.</summary>
    private static void SeedDownwardGateProjectileRom(TestAddressSpace bus)
    {
        // Both exact fourteen-byte definitions share the same inert initial preinstruction.
        bus.WriteBytes(0x86e64b, [
            0xd0, 0xe5, 0x04, 0xe6, 0x3c, 0xe5, 0x00, 0x00,
            0x00, 0x20, 0x00, 0x00, 0xfc, 0x84,
            0xd5, 0xe5, 0x04, 0xe6, 0x5e, 0xe5, 0x00, 0x00,
            0x00, 0x20, 0x00, 0x00, 0xfc, 0x84,
        ]);
        // Exact list/code span $86:E533-$E585. The first nine bytes are the native opcode
        // routine itself; list $E53C begins immediately afterward.
        bus.WriteBytes(0x86e533, [
            0xb9, 0x00, 0x00, 0x9d, 0xdb, 0x1a, 0xc8, 0xc8,
            0x60, 0x33, 0xe5, 0x00, 0x01, 0x61, 0x81, 0x05,
            0xe6, 0x01, 0x00, 0xee, 0xb4, 0x59, 0x81, 0x01,
            0x00, 0xf5, 0xb4, 0x59, 0x81, 0x01, 0x00, 0x01,
            0xb5, 0x59, 0x81, 0x01, 0x00, 0x12, 0xb5, 0x59,
            0x81, 0x6a, 0x81, 0x33, 0xe5, 0x00, 0xff, 0x01,
            0x00, 0x12, 0xb5, 0x59, 0x81, 0x61, 0x81, 0x05,
            0xe6, 0x01, 0x00, 0x12, 0xb5, 0x59, 0x81, 0x01,
            0x00, 0x01, 0xb5, 0x59, 0x81, 0x01, 0x00, 0xf5,
            0xb4, 0x59, 0x81, 0x01, 0x00, 0xee, 0xb4, 0x59,
            0x81, 0x54, 0x81,
        ]);
    }
}
