using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Plms.DownwardGateVisualsVerification;
using static SuperMetroid.Verification.Rooms.Plms.LookupDownwardGateDrawsVerification;
using static SuperMetroid.Verification.Rooms.Plms.LookupDownwardGateSelectionVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.Plms;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Focused room-PLM and bank-$86 actor checks for downward gates and their shot triggers.</summary>
internal static class DownwardGatePlmsVerification
{
    /// <summary>Compares native selection tables, then proves every production setup row works without reading those source tables.</summary>
    internal static void VerifyDownwardGateShotBlockDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyDownwardGateListSelection), () => VerifyDownwardGateListSelection(rom));
        Suite(nameof(VerifyDownwardGateLeftSelection), () => VerifyDownwardGateLeftSelection(rom));
        Suite(nameof(VerifyDownwardGateRightSelection), () => VerifyDownwardGateRightSelection(rom));

        // The synthetic address space intentionally omits all three source tables. Running
        // every row through production setup proves room loading no longer reads them.
        Suite(nameof(VerifyDownwardGatePlms), () => VerifyDownwardGatePlms());
    }

    /// <summary>
    /// Reproduces all eight gate-trigger dispatches in constructed rooms and checks the
    /// resident gate plus bank-$86 actor handoff independently of any long controller route.
    /// </summary>
    internal static void VerifyDownwardGatePlms()
    {
        Suite(nameof(VerifyDownwardGateHeaderDefinitions), () => VerifyDownwardGateHeaderDefinitions());
        Suite(nameof(VerifyDownwardGateProgramDefinitions), () => VerifyDownwardGateProgramDefinitions());
        Suite(nameof(VerifyDownwardGateDrawDefinitions), () => VerifyDownwardGateDrawDefinitions());
        Suite(nameof(VerifyDownwardGateVisuals), () => VerifyDownwardGateVisuals());
        Suite(nameof(VerifyDownwardGateSetupAndProjectile), () => VerifyDownwardGateSetupAndProjectile());

        var cases = new DownwardGatePlmsCase[]
        {
            new DownwardGatePlmsCase(DownwardGateTriggerBehavior.BlueLeft, 0x0004, true),
            new DownwardGatePlmsCase(DownwardGateTriggerBehavior.BlueRight, 0x0300, false),
            new DownwardGatePlmsCase(DownwardGateTriggerBehavior.GreenLeft, 0x0200, true),
            new DownwardGatePlmsCase(DownwardGateTriggerBehavior.GreenRight, 0x0000, false),
            new DownwardGatePlmsCase(DownwardGateTriggerBehavior.RedLeft, 0x0100, true),
            new DownwardGatePlmsCase(DownwardGateTriggerBehavior.RedRight, 0x0200, true),
            new DownwardGatePlmsCase(DownwardGateTriggerBehavior.YellowLeft, 0x0300, true),
            // This counterintuitive inequality is present in the retail right-hand routine.
            new DownwardGatePlmsCase(DownwardGateTriggerBehavior.YellowRight, 0x0000, true),
        };
        foreach ((DownwardGateTriggerBehavior trigger, ushort projectile, bool accepted) in cases)
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
        foreach ((PlmHeaderId header, int source) in new[] { new VerifyDownwardGateHeaderDefinitionsCase(PlmHeaderId.DownwardGate, 0x84c82c), new VerifyDownwardGateHeaderDefinitionsCase(PlmHeaderId.DownwardGateShotBlock, 0x84c838) })
        {
            AssertEqual(header, RoomPlmHeaders.Get(header).Header, "Gate compiled-population header");
            AssertEqual(unchecked((ushort)(source - 2)), PlmNativeWords.Word(header), "Gate header word precedes its first-list word");
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
        Suite(nameof(RoomPlmProgramsVerification.VerifyDownwardGatePrograms), () => RoomPlmProgramsVerification.VerifyDownwardGatePrograms(rom));
    }

    /// <summary>Checks resident slot order, gate collision, closed-actor spawning, shot wake, four-tile opening, and delayed moving-close actor.</summary>
    private static void VerifyDownwardGateSetupAndProjectile()
    {
        (TestAddressSpace bus, RoomLevelData level, RoomPlmSystem plms, int gateBlockIndex) = CreateDownwardGateFixture(
                DownwardGateTriggerBehavior.BlueLeft);

        AssertEqual(2, plms.SlotTable.ActiveCount, "gate and shot-block records retain separate PLM slots");
        RoomPlmSlotSnapshot[] slots = plms.PopulationSlots.ToArray();
        AssertEqual(0, slots[0].ProcessingPosition, "gate is processed first");
        AssertEqual(PlmHeaderId.DownwardGate, slots[0].HeaderPointer,
            "first resident slot is the closed downward gate");
        AssertEqual(1, slots[1].ProcessingPosition, "shot block is processed next");
        AssertTrue(slots[1].Animation is DownwardGatePlmAnimation { Program: DownwardGateProgram.TriggerBlueLeft },
            "argument zero selects the blue-left trigger program");

        for (int row = 0; row < DownwardGateDefinitions.GateHeightInBlocks; row++)
        {
            AssertEqual(DownwardGateDefinitions.ClosedGateBehavior,
                level.GetCollisionBlockByIndex(gateBlockIndex + row * level.Size.X).Behavior,
                $"gate setup writes BTS $10 to row {row}");
        }
        AssertEqual((int)RoomCollisionType.ShootableBlock,
            (int)level.GetCollisionBlockByIndex(gateBlockIndex - 1).CollisionType,
            "left trigger table installs a shootable block");
        AssertEqual((int)DownwardGateTriggerBehavior.BlueLeft,
            level.GetCollisionBlockByIndex(gateBlockIndex - 1).Behavior.Value,
            "argument zero installs retail blue-left BTS $46");

        DownwardGateProjectileRequest request = plms.Gates.TakeDownwardGateProjectileRequests().Single();
        AssertEqual(DownwardGateProjectileOperation.Spawn, request.Operation,
            "closed gate setup publishes one spawn");
        AssertEqual((DownwardGateProjectileKind?)DownwardGateProjectileKind.Closed,
            request.Kind, "closed gate setup spawns the initially closed gate projectile");

        // The production room loader consumes this request after Enemies.Load has cleared
        // the shared projectile pool. Recreate that exact boundary and inspect initializer E.
        SeedEmptyRoomEnemyPopulation(bus, 0x9600);
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: bus, services: new StandaloneRoomEnemyServices(() => 0));
        enemies.Load(new RoomEnemyLoadRequest(bus.ReadEnemyPopulation(0x9600), TestAddressSpace.UnusedGraphicsSet, BgArtworkFixtures.For(new SnesVram()), new GameplayColours(new SpriteColourSets())) { Level = level, Samus = new SamusState() });
        enemies.DownwardGateBehavior.ApplyDownwardGateProjectileRequest(request, level.Size.X);
        RoomEnemyProjectileSlot actor = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive);
        AssertEqual(RoomEnemyProjectileKind.DownwardGateClosed, actor.Kind,
            "bank-$86 pool owns the closed gate actor");
        AssertEqual(gateBlockIndex, actor.StateOf<DownwardGateState>().PlmBlockIndex,
            "gate actor is tied to its gate's PLM block");
        AssertEqual((ushort)96, actor.Position.Y.Whole,
            "closed gate actor begins four tiles beneath a row-two PLM origin");
        enemies.StepEnemyProjectiles(level, samus: null);
        enemies.StepEnemyProjectiles(level, samus: null);
        AssertTrue(actor.Animation is DownwardGateProjectileAnimation { Cursor.Program: DownwardGateProjectileProgram.Closed },
            "closed gate actor holds its closed program's sleep before a shot can wake it");

        // The closed resident list first draws its collision state and then sleeps under
        // $BB6B. The Green Hill Zone fixture uses room argument zero, so an ordinary beam
        // must wake its blue-left gate exactly as it does in room $8F:9E52.
        StepDownwardGatePlm(plms, level);
        StepDownwardGatePlm(plms, level);
        AssertEqual(RoomTerrainBlock.Of(0x0ff, BlockMirroring.None, RoomCollisionType.ShootableBlock),
            level.GetCollisionBlockByIndex(gateBlockIndex + level.Size.X).Terrain,
            "compiled initial gate draw installs the second solid column word");
        AssertTrue(plms.Gates.TrySpawnDownwardGateTrigger(
                level,
                gateBlockIndex - 1,
                level.GetCollisionBlockByIndex(gateBlockIndex - 1).Behavior,
                SamusProjectileType.Beam(SamusBeamCombination.Spazer, charged: false)),
            "power beam reaches the sleeping blue gate");
        StepDownwardGatePlm(plms, level);
        DownwardGateProjectileRequest wake = plms.Gates.TakeDownwardGateProjectileRequests().Single();
        AssertEqual(DownwardGateProjectileOperation.Wake, wake.Operation,
            "closed gate publishes a wake request");
        AssertTrue(plms.Sounds.SoundRequests.Any(sound =>
                sound.SoundEffect == DownwardGateDefinitions.MovementSound),
            "opening gate queues its cartridge movement sound");
        enemies.DownwardGateBehavior.ApplyDownwardGateProjectileRequest(wake, level.Size.X);

        // Run the PLM and bank-$86 actor together until the open-state coroutine sleeps.
        // This exercises all four 16-pixel movement segments and the list's final goto.
        for (int frame = 0; frame < 96; frame++)
        {
            StepDownwardGatePlm(plms, level);
            enemies.StepEnemyProjectiles(level, samus: null);
        }
        AssertEqual(0, enemies.ActiveEnemyProjectileCount,
            "opened gate actor retracts four tiles and deletes");
        AssertEqual(RoomTerrainBlock.Of(0x0ff, BlockMirroring.None, RoomCollisionType.Air),
            level.GetCollisionBlockByIndex(gateBlockIndex + level.Size.X).Terrain,
            "compiled final open draw clears the second column collision word");
        RoomPlmSlotSnapshot openedGate = plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == PlmHeaderId.DownwardGate);
        AssertEqual(PlmPreInstructionNativeWords.DownwardGate.Word(DownwardGatePreInstruction.WakeIfTriggered),
            openedGate.PreInstruction, "open gate sleeps under shot-only callback");

        AssertTrue(plms.Gates.TrySpawnDownwardGateTrigger(
                level,
                gateBlockIndex - 1,
                level.GetCollisionBlockByIndex(gateBlockIndex - 1).Behavior,
                SamusProjectileType.Beam(SamusBeamCombination.Spazer, charged: false)),
            "second power beam reaches the sleeping open blue gate");
        // The open-state collision image remains for sixteen frames before $BBE1 spawns
        // the downward-moving actor, exactly as list $BC13 specifies.
        for (int frame = 0; frame < 17; frame++)
            StepDownwardGatePlm(plms, level);
        DownwardGateProjectileRequest close = plms.Gates.TakeDownwardGateProjectileRequests().Single();
        AssertEqual(DownwardGateProjectileOperation.Spawn, close.Operation,
            "open gate publishes a moving close actor");
        AssertEqual((DownwardGateProjectileKind?)DownwardGateProjectileKind.Moving,
            close.Kind, "close request spawns the moving gate projectile");
        enemies.DownwardGateBehavior.ApplyDownwardGateProjectileRequest(close, level.Size.X);
        for (int frame = 0; frame < 80; frame++)
            enemies.StepEnemyProjectiles(level, samus: null);
        RoomEnemyProjectileSlot closedActor = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive);
        AssertEqual((ushort)96, closedActor.Position.Y.Whole,
            "moving gate travels four tiles down and remains parked closed");
    }

    /// <summary>Checks one color/direction BTS row's projectile acceptance and physical shot-block draw.</summary>
    private static void VerifyDownwardGateTrigger(
        DownwardGateTriggerBehavior trigger,
        ushort projectile,
        bool accepted)
    {
        (TestAddressSpace bus, RoomLevelData level, RoomPlmSystem plms, int gateBlockIndex) =
            CreateDownwardGateFixture(trigger);
        int triggerBlockIndex = ((byte)trigger & 1) == 0
            ? gateBlockIndex - 1
            : gateBlockIndex + 1;
        RoomBlockBehavior bts = level.GetCollisionBlockByIndex(triggerBlockIndex).Behavior;
        AssertTrue(plms.Gates.TrySpawnDownwardGateTrigger(level, triggerBlockIndex, bts, SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(projectile)),
            $"{trigger} is claimed by the gate dispatcher");
        RoomPlmSlotSnapshot gate = plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == PlmHeaderId.DownwardGate);
        AssertEqual(accepted ? 1 : 0, gate.LoopTimer,
            $"{trigger} projectile ${projectile:X4} acceptance matches cartridge");
        StepDownwardGatePlm(plms, level);
        StepDownwardGatePlm(plms, level);
        int color = ((byte)trigger - (byte)DownwardGateTriggerBehavior.BlueLeft) / 2;
        RoomTerrainBlock expectedTriggerWord = RoomTerrainBlock.Of(0x0db - color,
            ((byte)trigger & 1) == 0 ? BlockMirroring.None : BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock);
        AssertEqual(expectedTriggerWord,
            level.GetCollisionBlockByIndex(triggerBlockIndex).Terrain,
            $"{trigger} compiled shot-trigger draw installs its physical block word");
        AssertTrue(plms.PopulationSlots.All(slot =>
                slot.HeaderPointer != PlmHeaderId.DownwardGateShotBlock),
            $"{trigger} trigger list draws once and deletes without ROM control bytes");
    }

    /// <summary>Loads a gate and adjacent shot block at room cell (6,2) with compiled headers and synthetic actor instructions.</summary>
    internal static CreateDownwardGateFixtureResult
        CreateDownwardGateFixture(DownwardGateTriggerBehavior trigger,
            InstalledRoomPlmDownwardGateVisuals? visuals = null)
    {
        var bus = new TestAddressSpace();
        const ushort populationPointer = 0x9400;
        const byte gateX = 6;
        const byte gateY = 2;
        const int roomWidth = 16;
        ushort argument = unchecked((ushort)(((byte)trigger - 0x46) * 2));
        // Supply imported placements directly. Cartridge header reads belong to import;
        // the guard below prohibits them only while the production runtime sets up slots.
        var population = new RoomPlmPopulation($"$8F:{populationPointer:X4}",
        [
            new RoomPlmPlacement(RoomPlmHeaders.Get(PlmHeaderId.DownwardGate),
                new(gateX, gateY), null),
            new RoomPlmPlacement(RoomPlmHeaders.Get(PlmHeaderId.DownwardGateShotBlock),
                new(gateX, gateY), PlmRoomArgumentWords.Decode(PlmHeaderId.DownwardGateShotBlock, argument)),
        ]);
        SeedDownwardGateProjectileRom(bus);

        RoomLevelData level = CreateRoom(new(roomWidth, 16),
            new RoomTerrainBlock[roomWidth * 16],
            new RoomBlockBehavior[roomWidth * 16]);
        var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmDownwardGateVisuals = visuals });
        int parsed = PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(), AreaId.Brinstar), population, new RoomPlmServices { Samus = new FixtureRoomSamus(() => null), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
        AssertEqual(2, parsed, $"{trigger} synthetic population parses both records");
        return new CreateDownwardGateFixtureResult(bus, level, plms, gateY * roomWidth + gateX);
    }


    /// <summary>Advances one gate PLM frame with stationary view and no scroll, enemy-death, or controller events.</summary>
    internal static void StepDownwardGatePlm(
        RoomPlmSystem plms,
        RoomLevelData level) =>
        plms.Step(level, new PlmFrameInputs { Scrolls = null, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0 });

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
