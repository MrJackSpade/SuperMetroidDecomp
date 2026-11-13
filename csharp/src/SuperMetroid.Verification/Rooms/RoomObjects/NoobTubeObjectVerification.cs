using SuperMetroid.AssetExtraction;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.RoomObjects.CollectiblesVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleDoorsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Earthquakes;
namespace SuperMetroid.Verification.Rooms.RoomObjects;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.NoobTube;

/// <summary>Exercises the Glass Tunnel PLM's two-stage trigger, break sequence, and reload state.</summary>
internal static class NoobTubeObjectVerification
{
    /// <summary>
    /// Drives the complete n00b-tube coroutine in a deliberately small synthetic room. The
    /// fixture contains the cartridge's real instruction words and timing, while using
    /// one-block draw lists so this test isolates PLM control flow from unrelated art data.
    /// </summary>
    internal static void VerifyNoobTubeObject()
    {
        Suite(nameof(VerifyNoobTubeProgramDefinitions), () => VerifyNoobTubeProgramDefinitions());
        var bus = new TestAddressSpace();
        SeedNoobTubeRom(bus);

        const ushort populationPointer = 0x9400;
        const byte blockX = 2;
        const byte blockY = 2;
        const int roomWidth = 16;
        const int blockIndex = blockY * roomWidth + blockX;
        SeedRoomObjectPopulation(bus, populationPointer, [
            0x0c, 0xd7, blockX, blockY, 0x00, 0x00,
            0x00, 0x00,
        ]);

        RoomLevelData level = CreateRoom(new(roomWidth, 16),
            new RoomTerrainBlock[roomWidth * 16],
            new RoomBlockBehavior[roomWidth * 16]);
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
        samus.Position = samus.Position.WithWholeY(0x0100);
        RoomFxOverlayState roomFx = CreateNoobTubeWaterFx();
        samus.LiquidPhysics.ConfigureFor(roomFx);
        var guarded = NewNoobTubeProgramReadGuard(bus);
        var projectiles = new List<NoobTubeProjectileRequest>();
        var earthquakeShakes = new List<EarthquakeShake>();
        var earthquakeFrames = new List<int>();
        bool brokenEvent = false;
        var roomObjects = RoomObjectFixtures.Unbound();
        RoomBlockAppearance originShown = level.GetCollisionBlock(new(blockX, blockY)).Terrain.Appearance;

        int parsed = RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, new SharedGameState(new SharedRandom()), AreaId.Maridia), RoomObjectPopulationImporter.Read(guarded, populationPointer), new RoomObjectServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Events = new FixtureEventFlags { HasEvent = eventNumber => eventNumber == NoobTubeObjectDefinitions.BrokenEvent && brokenEvent, SetEvent = eventNumber =>
            {
                AssertEqual(NoobTubeObjectDefinitions.BrokenEvent, eventNumber,
                    "n00b tube writes its cartridge event");
                brokenEvent = true;
            } }, RoomFx = new RoomObjectFx(roomFx, () => samus), Earthquake = new FixtureRoomEarthquake { WriteStart = (shake, frames) => { earthquakeShakes.Add(shake); earthquakeFrames.Add(frames); } }, Enemies = new FixtureObjectEnemyEffects { NoobTubeProjectile = projectiles.Add } });

        AssertEqual(1, parsed, "n00b-tube population record loads");
        AssertEqual(new RoomTerrainBlock(originShown, RoomCollisionType.SolidBlock),
            level.GetCollisionBlock(new(blockX, blockY)).Terrain,
            "n00b-tube setup makes its origin solid and keeps what it shows");
        AssertEqual(RoomBlockBehaviorCodec.Encode(RoomBlockBehavior.Reacting(ShootableReaction.ResidentObjectTrigger)),
            RoomBlockBehaviorCodec.Encode(level.GetCollisionBlock(new(blockX, blockY)).Behavior),
            "n00b-tube setup installs BTS $44");

        // First pass follows the event-not-set branch, publishes intact art, and sleeps
        // under the power-bomb detector with its link pointing at the input-wait stage.
        StepNoobTube(roomObjects, level);
        RoomObjectSlotSnapshot intact = roomObjects.PopulationSlots.Single();
        AssertTrue(intact.Behaviour is NoobTubeObject { Phase: NoobTubePhase.WaitingForPowerBomb },
            "intact tube waits for a power bomb");

        // Repeated #339: BTS $44 is a resident trigger, not a breakable-block index.
        // Exercise StepFiring (including endpoint collision), rather than bypassing
        // the missing caller dispatch with TryNotifyResidentProjectileHit directly.
        var grappleSamus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(blockX * 16 + 8, blockY * 16 + 8)),
        };
        SeedStationaryGrappleCollisionProbe(grappleSamus);
        var grappleHit = SamusGrappleMovement.StepFiring(level, grappleSamus,
            ControllerButton.X, roomObjects);
        AssertEqual(GrapplePhase.CancelPending, grappleSamus.Grapple.Phase,
            "solid resident trigger cancels the grapple endpoint");
        AssertEqual(new ResidentObjectProjectileHit(SamusProjectileFamily.Beam), PendingTubeHit(roomObjects),
            "grapple publishes a hit that reads as a beam, not a missile or power bomb");
        StepNoobTube(roomObjects, level);
        AssertEqual(null, PendingTubeHit(roomObjects), "resident consumes grapple notification once");
        AssertTrue(!brokenEvent && projectiles.Count == 0, "grapple must not shatter the power-bomb-only tube");
        AssertTrue(roomObjects.Sounds.SoundRequests.Contains(new SoundRequest(
            NoobTubeObjectDefinitions.IneffectiveShotSound, SoundRequestPriority.Normal)),
            "resident emits its cartridge ineffective-shot response to grapple");

        // A non-power-bomb hit is consumed by the same resident collision dispatcher but
        // only queues the retail ineffective-shot sound and clears the transient hit word.
        AssertTrue(roomObjects.ProjectileTriggers.TryNotifyResidentProjectileHit(new(blockX, blockY), SamusProjectileType.Of(SamusProjectileFamily.Missile)),
            "n00b-tube collision block locates its resident PLM");
        StepNoobTube(roomObjects, level);
        AssertTrue(roomObjects.Sounds.SoundRequests.Contains(new SoundRequest(
                NoobTubeObjectDefinitions.IneffectiveShotSound,
                SoundRequestPriority.Normal)),
            "ordinary shot queues the cartridge rejection sound");
        AssertEqual(null, PendingTubeHit(roomObjects),
            "projectile notification is consumed exactly once");

        // The intact retail draw changes the origin from setup's type $8 to type $C while
        // retaining BTS $44. Issue #304 reached this exact state through the expanding
        // Power Bomb boundary and previously threw before notifying the resident tube.
        AssertEqual((int)RoomCollisionType.ShootableBlock,
            (int)level.GetCollisionBlockByIndex(blockIndex).CollisionType,
            "intact n00b-tube draw installs live type-$C collision");
        var reactions = new List<BombBlockReaction>();
        SamusBombProjectileSystem.CollectSingleBombedBlockReaction(
            level,
            new(blockX, blockY),
            reactions,
            roomObjects,
            AreaId.Maridia,
            SamusBombProjectileSystem.PowerBombType);
        AssertEqual(1, reactions.Count,
            "Power Bomb boundary visits the live n00b-tube origin");
        AssertEqual(RoomCollisionType.ShootableBlock, level.GetCollisionBlockByIndex(blockIndex).CollisionType,
            "Power Bomb observes the intact tube's type-$C collision");
        AssertEqual(RoomBlockBehavior.Reacting(ShootableReaction.ResidentObjectTrigger),
            level.GetCollisionBlockByIndex(blockIndex).Behavior,
            "Power Bomb observes the intact tube's BTS-$44 trigger");
        AssertEqual(new ResidentObjectProjectileHit(SamusProjectileFamily.PowerBomb), PendingTubeHit(roomObjects),
            "generic trigger publishes the Power Bomb hit");
        StepNoobTube(roomObjects, level);
        RoomObjectSlotSnapshot armed = roomObjects.PopulationSlots.Single();
        AssertTrue(armed.Behaviour is NoobTubeObject { Phase: NoobTubePhase.WaitingForInput },
            "power bomb wakes the tube to wait for input");

        StepNoobTube(roomObjects, level, ControllerButton.Right);
        AssertTrue(samus.Control.AlphaLocked, "break sequence locks Samus");
        AssertTrue(samus.Control.Mode.Beta == SamusBetaHandler.ScriptLocked, "break sequence installs stationary command-zero animation ownership");
        var fields = MetadataShape.Fields(typeof(SamusState), System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
            .Where(field => !field.IsDefined(typeof(NonSerializedAttribute), false))
            .OrderBy(field => field.MetadataToken).ToArray();
        AssertTrue(fields.Any(field => field.Name == "<Control>k__BackingField"),
            "the installed handler pair is serialized with Samus");
        using (var saved = new MemoryStream())
        {
            SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(saved, samus);
            saved.Position = 0;
            var restored = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Deserialize<SamusState>(saved);
            AssertTrue(restored.Control.AlphaLocked && restored.Control.Mode.Beta == SamusBetaHandler.ScriptLocked,
                "mid-script state retains input and animation ownership");
            restored.Control.Unlock();
            AssertTrue(restored.Control.Mode.Beta != SamusBetaHandler.ScriptLocked, "generic unlock clears stale stationary ownership");
        }
        AssertEqual(1, projectiles.Count, "break sequence initially spawns only the crack");
        AssertEqual(NoobTubeProjectileDefinition.Crack, projectiles[0].Definition,
            "first spawned actor is the n00b-tube crack");

        // The first broken image lasts $30 frames. Two one-frame images follow, after which
        // the cartridge emits the sound, ten shards, six bubbles, and earthquake together.
        for (int frame = 0; frame < 50; frame++)
            StepNoobTube(roomObjects, level);
        AssertEqual(17, projectiles.Count,
            "break burst contains one crack, ten shards, and six bubbles");
        AssertEqual(NoobTubeProjectileDefinition.Shard,
            projectiles[1].Definition, "burst begins with the first shard");
        AssertEqual<int?>(0, projectiles[1].Piece, "the first shard names the burst's first piece");
        AssertEqual<int?>(9, projectiles[10].Piece,
            "the tenth shard names the burst's last shard");
        AssertEqual(NoobTubeProjectileDefinition.ReleasedAirBubble,
            projectiles[11].Definition, "six bubbles follow all shards");
        AssertEqual<int?>(5, projectiles[^1].Piece,
            "the sixth bubble names the burst's last bubble");
        AssertEqual(1, earthquakeShakes.Count,
            "break burst starts one earthquake");
        AssertEqual(11, NativeEarthquakeTypes.TypeOf(earthquakeShakes[0]),
            "earthquake shake matches the cartridge's type");
        AssertEqual(NoobTubeObjectDefinitions.EarthquakeFrames, earthquakeFrames[0],
            "earthquake length matches cartridge value");

        // The final broken frame lasts $60 frames, then event $0B is committed, water
        // physics are enabled, Samus is unlocked, and the one-shot actor deletes itself.
        for (int frame = 0; frame < 96; frame++)
            StepNoobTube(roomObjects, level);
        AssertTrue(brokenEvent, "n00b-tube completion sets event $0B");
        AssertTrue(!samus.Control.AlphaLocked, "n00b-tube completion unlocks Samus");
        AssertTrue(samus.Control.Mode.Beta != SamusBetaHandler.ScriptLocked, "completion restores normal animation ownership");
        AssertEqual(new RoomWater(LiquidTide.Small, SurfaceDrift: false, BackgroundWave: false, IgnoresSurfaceUntilEnabled: false),
            roomFx.Liquid,
            "n00b-tube completion lets the shared room water's surface affect Samus");
        samus.LiquidPhysics.ConfigureFor(roomFx);
        AssertTrue(!samus.LiquidPhysics.IgnoresWaterSurface,
            "next room-FX publication preserves enabled water physics");
        AssertEqual(SamusLiquidMedium.Water,
            samus.LiquidPhysics.DetermineMovementMedium(samus),
            "fully submerged Samus uses underwater movement after tube break");
        AssertEqual(0, roomObjects.SlotTable.ActiveCount, "completed n00b-tube PLM deletes itself");

        Suite(nameof(VerifyNoobTubeProjectileInitializers), () => VerifyNoobTubeProjectileInitializers(bus, level, blockIndex, projectiles));
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "n00b-tube complete break never rereads its program source bytes");
        Suite(nameof(VerifyAlreadyBrokenNoobTube), () => VerifyAlreadyBrokenNoobTube(bus, level, samus));
        Console.WriteLine(
            "  N00b tube: setup, two-stage wake, debris, earthquake, event, water, and reload agree.");
    }

    /// <summary>Checks the pinned cartridge revision before comparing the tube's typed programs with its native lists.</summary>
    private static void VerifyNoobTubeProgramDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Tube oracle revision");
        Suite(nameof(RoomObjectProgramsVerification.VerifyNoobTubePrograms), () => RoomObjectProgramsVerification.VerifyNoobTubePrograms(rom));
    }

    /// <summary>
    /// Sends the complete cartridge-authored burst through the production bank-$86 allocator
    /// and checks both ends of each initializer table. This catches mismatched PLM coordinates,
    /// parameter scaling, list selection, and 8.8 velocity signs without needing a real room.
    /// </summary>
    private static void VerifyNoobTubeProjectileInitializers(
        TestAddressSpace bus,
        RoomLevelData level,
        int blockIndex,
        IReadOnlyList<NoobTubeProjectileRequest> requests)
    {
        SeedEmptyRoomEnemyPopulation(bus, 0x9600);
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: bus, services: new StandaloneRoomEnemyServices(() => 0x4040));
        enemies.Load(new RoomEnemyLoadRequest(bus.ReadEnemyPopulation(0x9600), BgArtworkFixtures.For(new SnesVram())) { Level = level, Samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) });

        foreach (NoobTubeProjectileRequest request in requests)
            enemies.NoobTubeBehavior.SpawnNoobTubeProjectile(request);
        AssertEqual(17, enemies.ActiveEnemyProjectileCount,
            "complete n00b-tube burst fits the native eighteen-slot projectile pool");

        EnemyProjectile crack = enemies.Projectiles.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.NoobTubeCrack);
        AssertEqual(0x0080, crack.Position.X.Whole,
            "crack initializer uses PLM X plus six blocks");
        AssertEqual(0x0050, crack.Position.Y.Whole,
            "crack initializer uses PLM Y plus three blocks");

        EnemyProjectile firstShard = enemies.Projectiles.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.NoobTubeShard &&
            projectile.Animation is NoobTubeProjectileAnimation { PlayingSegment: NoobTubeProjectileSegment.Shard0 });
        AssertEqual(0x0048, firstShard.StateOf<NoobTubeDebrisState>().X.Whole,
            "first shard keeps its signed X origin as its own column");
        AssertEqual(0x0058, firstShard.Position.Y.Whole,
            "first shard stores its signed Y offset");
        AssertEqual(0xfe80, firstShard.Velocity.X.RawValue,
            "first shard retains negative 8.8 X velocity");
        AssertEqual(0x0140, firstShard.Velocity.Y.RawValue,
            "first shard retains positive 8.8 Y velocity");

        EnemyProjectile lastShard = enemies.Projectiles.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.NoobTubeShard &&
            projectile.Animation is NoobTubeProjectileAnimation { PlayingSegment: NoobTubeProjectileSegment.Shard9 });
        AssertEqual(0x0078, lastShard.StateOf<NoobTubeDebrisState>().X.Whole,
            "last shard uses final signed X offset");
        AssertEqual(0x0060, lastShard.Position.Y.Whole,
            "last shard uses final signed Y offset");
        AssertEqual(0xffc0, lastShard.Velocity.X.RawValue,
            "last shard uses final signed X velocity");
        AssertEqual(0x0180, lastShard.Velocity.Y.RawValue,
            "last shard uses final signed Y velocity");

        EnemyProjectile firstBubble = enemies.Projectiles.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.NoobTubeReleasedAirBubble &&
            projectile.StateOf<NoobTubeDebrisState>().X.Whole == 0x0048);
        AssertEqual(0x0070, firstBubble.Position.Y.Whole,
            "first bubble uses its first absolute PLM-origin offset");
        AssertEqual(NoobTubeDebrisDefinitions.BubbleInitialYVelocity.RawValue,
            firstBubble.Velocity.Y.RawValue, "first bubble starts with cartridge upward velocity");

        EnemyProjectile lastBubble = enemies.Projectiles.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.NoobTubeReleasedAirBubble &&
            projectile.StateOf<NoobTubeDebrisState>().X.Whole == 0x00d8);
        AssertEqual(0x0074, lastBubble.Position.Y.Whole,
            "last bubble uses its final absolute PLM-origin offset");

        AssertTrue(requests.All(request => request.RoomObjectBlock == level.BlockAt(blockIndex)),
            "every burst actor retains the executing PLM block");
    }

    /// <summary>Confirms event $0B skips every one-shot effect on subsequent room loads.</summary>
    private static void VerifyAlreadyBrokenNoobTube(
        TestAddressSpace bus,
        RoomLevelData level,
        SamusState samus)
    {
        const ushort populationPointer = 0x9400;
        var projectiles = new List<NoobTubeProjectileRequest>();
        samus.Control.Unlock();
        RoomFxOverlayState roomFx = CreateNoobTubeWaterFx();
        samus.LiquidPhysics.ConfigureFor(roomFx);
        var guarded = NewNoobTubeProgramReadGuard(bus);
        var roomObjects = RoomObjectFixtures.Unbound();
        RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, new SharedGameState(new SharedRandom()), AreaId.Maridia), RoomObjectPopulationImporter.Read(guarded, populationPointer), new RoomObjectServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Events = new FixtureEventFlags { HasEvent = eventNumber => eventNumber == NoobTubeObjectDefinitions.BrokenEvent, SetEvent = _ => throw new InvalidOperationException(
                "Already-broken n00b tube must not set its event again.") }, RoomFx = new RoomObjectFx(roomFx, () => samus), Earthquake = new FixtureRoomEarthquake { WriteStart = (_, _) => throw new InvalidOperationException(
                "Already-broken n00b tube must not start an earthquake.") }, Enemies = new FixtureObjectEnemyEffects { NoobTubeProjectile = projectiles.Add } });

        StepNoobTube(roomObjects, level);
        AssertEqual(0, roomObjects.SlotTable.ActiveCount, "event-$0B n00b tube deletes on its first step");
        AssertEqual(0, projectiles.Count, "event-$0B n00b tube spawns no break debris");
        AssertEqual(new RoomWater(LiquidTide.Small, SurfaceDrift: false, BackgroundWave: false, IgnoresSurfaceUntilEnabled: false),
            roomFx.Liquid,
            "event-$0B room load lets the shared room water's surface affect Samus");
        samus.LiquidPhysics.ConfigureFor(roomFx);
        AssertEqual(SamusLiquidMedium.Water,
            samus.LiquidPhysics.DetermineMovementMedium(samus),
            "event-$0B reload keeps fully submerged movement underwater");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "already-broken n00b-tube branch never rereads program bytes");
    }

    /// <summary>Rejects reads from the compiled tube instruction ranges while the fixture runs.</summary>
    private static CartridgeReadGuard NewNoobTubeProgramReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x84d4d4 and <= 0x84d518 or
                >= 0x84d521 and <= 0x84d524,
                address => $"N00b-tube PLM reread compiled program byte ${address:X6}.");

    /// <summary>
    /// Loads the Glass Tunnel's relevant FX words: water at Y $0010 with the small-tide
    /// option and the bit that suppresses water physics until the tube breaks.
    /// </summary>
    internal static RoomFxOverlayState CreateNoobTubeWaterFx()
    {
        // FX mechanics now come from compiled records. Use the actual Glass Tunnel record.
        var roomFxSession = new RoomFxSession();
        var roomFx = RoomFxOverlayState.ForRoom(roomFxSession, RoomFxDefinitions.Suffix(0x9c94), door: null,
            randomNumber: 0);
        return roomFx;
    }

    /// <summary>The strike the live glass tube has not yet read, or null for none.</summary>
    private static ResidentObjectProjectileHit? PendingTubeHit(RoomObjectSystem roomObjects) =>
        (roomObjects.PopulationSlots.Single().Behaviour as NoobTubeObject
            ?? throw new InvalidOperationException("The glass tube is not a typed object.")).PendingHit;

    /// <summary>Advances one PLM tick with the requested new-input edge and a fixed camera.</summary>
    internal static void StepNoobTube(
        RoomObjectSystem roomObjects,
        RoomLevelData level,
        ControllerButton controllerNewInput = ControllerButton.None) =>
        roomObjects.Step(level, new RoomObjectFrameInputs { Scrolls = null, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = controllerNewInput });

    /// <summary>Writes the exact n00b-tube list words plus intentionally tiny draw data.</summary>
    internal static void SeedNoobTubeRom(TestAddressSpace bus)
    {
        // Three seven-word enemy-projectile definitions. Their lists do not need draw data
        // for initializer checks, but the pointers themselves remain the cartridge values.
        bus.WriteBytes(0x86d904, [
            0xa5, 0xd6, 0xfb, 0x84, 0xd7, 0xd3, 0x00, 0x00,
            0x00, 0x30, 0x00, 0x00, 0xfc, 0x84,
            0xc9, 0xd6, 0xfd, 0xd7, 0x7d, 0xd4, 0x00, 0x00,
            0x00, 0x30, 0x00, 0x00, 0xfc, 0x84,
            0x74, 0xd7, 0xfb, 0x84, 0x52, 0xd6, 0x00, 0x00,
            0x00, 0x30, 0x00, 0x00, 0xfc, 0x84,
        ]);
        SeedObjectProgramWords(bus, 0xd70e, RoomObjectNativeListAddresses.NoobTubeList);

        WriteOneBlockDraw(bus, 0x98d1,
            unchecked(((ushort)RoomCollisionType.ShootableBlock << 12) | 0x0001));
        WriteOneBlockDraw(bus, 0x98d7, 0x8002);
        WriteOneBlockDraw(bus, 0x9991, 0x8003);
        WriteOneBlockDraw(bus, 0x99e5, 0x8004);
        WriteOneBlockDraw(bus, 0x98dd, 0x8005);
    }

    /// <summary>Runs the full tube coroutine verification from the command line.</summary>
    [VerificationCommand("--noob-tube-plm-program")]
    internal static int NoobTubeObjectProgramCommand()
    {
        Suite(nameof(VerifyNoobTubeObject), () => VerifyNoobTubeObject());
        return 0;
    }

    /// <summary>Runs the tube and permanent-collectible checks for resident PLM triggers.</summary>
    [VerificationCommand("--grapple-resident-trigger")]
    internal static int GrappleResidentTriggerCommand()
    {
        Suite(nameof(VerifyNoobTubeObject), () => VerifyNoobTubeObject());
        Suite(nameof(VerifyPermanentCollectibles), () => VerifyPermanentCollectibles());
        return 0;
    }
}
