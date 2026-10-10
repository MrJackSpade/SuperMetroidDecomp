using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Drives the complete n00b-tube coroutine in a deliberately small synthetic room. The
    /// fixture contains the cartridge's real instruction words and timing, while using
    /// one-block draw lists so this test isolates PLM control flow from unrelated art data.
    /// </summary>
    private static void VerifyNoobTubePlm()
    {
        Suite(nameof(VerifyNoobTubeProgramDefinitions), () => VerifyNoobTubeProgramDefinitions());
        var bus = new TestAddressSpace();
        SeedNoobTubeRom(bus);

        const ushort populationPointer = 0x9400;
        const byte blockX = 2;
        const byte blockY = 2;
        const int roomWidth = 16;
        const int blockIndex = blockY * roomWidth + blockX;
        bus.WriteBytes(0x8f0000 | populationPointer, [
            0x0c, 0xd7, blockX, blockY, 0x00, 0x00,
            0x00, 0x00,
        ]);

        RoomLevelData level = CreateRoom(
            roomWidth,
            16,
            new ushort[roomWidth * 16],
            new byte[roomWidth * 16],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var samus = new SamusState();
        samus.YPosition = 0x0100;
        RoomLayer3FxState roomFx = CreateNoobTubeWaterFx(bus);
        roomFx.ApplyToSamusLiquidPhysics(samus.LiquidPhysics);
        var guarded = new NoobTubeProgramReadGuard(bus);
        var projectiles = new List<NoobTubeProjectileRequest>();
        var earthquakes = new List<ushort>();
        bool brokenEvent = false;
        var plms = new RoomPlmSystem();

        int parsed = plms.LoadRoomPopulation(
            guarded,
            level,
            streamer,
            new SnesVram(),
            RoomPlmPopulationImporter.Read(guarded, populationPointer),
            new Bank80SystemState(),
            AreaId.Maridia,
            getSamus: () => samus,
            isAreaTorizoDefeated: () => false,
            hasEvent: eventNumber => eventNumber == NoobTubePlmRomData.BrokenEvent && brokenEvent,
            setEvent: eventNumber =>
            {
                AssertEqual(NoobTubePlmRomData.BrokenEvent, eventNumber,
                    "n00b tube writes its cartridge event");
                brokenEvent = true;
            },
            roomFx: roomFx,
            setEarthquakeTimer: earthquakes.Add,
            setEarthquakeType: earthquakes.Add,
            spawnNoobTubeProjectile: projectiles.Add);

        AssertEqual(1, parsed, "n00b-tube population record loads");
        AssertEqual(NoobTubePlmRomData.SolidProjectileTriggerLevelWord,
            level.GetCollisionBlock(blockX, blockY).LevelWord,
            "n00b-tube setup installs the complete type-$8 collision word");
        AssertEqual((int)RoomBlockBehaviorValues.ResidentPlmProjectileTrigger.Value,
            level.GetCollisionBlock(blockX, blockY).Behavior,
            "n00b-tube setup installs BTS $44");

        // First pass follows the event-not-set branch, publishes intact art, and sleeps
        // under the power-bomb detector with its link pointing at the input-wait stage.
        StepNoobTube(plms, guarded, level, streamer);
        RoomPlmSlotSnapshot intact = plms.PopulationSlots.Single();
        AssertEqual((ushort)NoobTubePlmPreInstruction.WakeOnPowerBomb,
            intact.PreInstruction, "intact tube waits for a power bomb");
        AssertEqual(0xd4e8, intact.LinkInstruction,
            "power-bomb callback retains the cartridge link target");
        AssertEqual(0xd4e6, intact.InstructionPointer,
            "intact tube sleeps after its first draw");

        // Repeated #339: BTS $44 is a resident trigger, not a breakable-block index.
        // Exercise StepFiring (including endpoint collision), rather than bypassing
        // the missing caller dispatch with TryNotifyResidentProjectileHit directly.
        var grappleSamus = new SamusState
        {
            XPosition = blockX * 16 + 8,
            YPosition = blockY * 16 + 8,
        };
        SeedStationaryGrappleCollisionProbe(bus, grappleSamus);
        var grappleHit = SamusGrappleMovement.StepFiring(bus, level, grappleSamus,
            (ushort)SnesButton.X, plms);
        AssertEqual(GrapplePhase.CancelPending, grappleSamus.Grapple.Phase,
            "solid resident trigger cancels the grapple endpoint");
        AssertEqual(0x8000, plms.PopulationSlots.Single().LoopTimer,
            "grapple publishes the native hit bit without a missile/power-bomb family");
        StepNoobTube(plms, guarded, level, streamer);
        AssertEqual(0, plms.PopulationSlots.Single().LoopTimer, "resident consumes grapple notification once");
        AssertTrue(!brokenEvent && projectiles.Count == 0, "grapple must not shatter the power-bomb-only tube");
        AssertTrue(plms.SoundRequests.Contains(new PlmSoundRequest(
            SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, NoobTubePlmRomData.IneffectiveShotSound), 6)),
            "resident emits its cartridge ineffective-shot response to grapple");

        // A non-power-bomb hit is consumed by the same resident collision dispatcher but
        // only queues the retail ineffective-shot sound and clears the transient hit word.
        AssertTrue(plms.TryNotifyResidentProjectileHit(blockIndex, 0x0100),
            "n00b-tube collision block locates its resident PLM");
        StepNoobTube(plms, guarded, level, streamer);
        AssertTrue(plms.SoundRequests.Contains(new PlmSoundRequest(
                SoundEffectId.FromCartridge(
                    SoundEffectLibrary.Library2,
                    NoobTubePlmRomData.IneffectiveShotSound),
                6)),
            "ordinary shot queues the cartridge rejection sound");
        AssertEqual((ushort)0, plms.PopulationSlots.Single().LoopTimer,
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
            blockX,
            blockY,
            reactions,
            plms,
            AreaId.Maridia,
            SamusBombProjectileSystem.PowerBombType);
        AssertEqual(1, reactions.Count,
            "Power Bomb boundary visits the live n00b-tube origin");
        AssertEqual(RoomCollisionType.ShootableBlock, level.GetCollisionBlockByIndex(blockIndex).CollisionType,
            "Power Bomb observes the intact tube's type-$C collision");
        AssertEqual(RoomBlockBehaviorValues.ResidentPlmProjectileTrigger,
            level.GetCollisionBlockByIndex(blockIndex).Bts,
            "Power Bomb observes the intact tube's BTS-$44 trigger");
        AssertEqual((ushort)0x8300, plms.PopulationSlots.Single().LoopTimer,
            "generic trigger publishes the native marked Power Bomb word");
        StepNoobTube(plms, guarded, level, streamer);
        RoomPlmSlotSnapshot armed = plms.PopulationSlots.Single();
        AssertEqual((ushort)NoobTubePlmPreInstruction.WakeOnAcceptedInput,
            armed.PreInstruction, "power bomb arms the input wake callback");
        AssertEqual(0xd4f2, armed.LinkInstruction,
            "input callback retains the cartridge break target");

        StepNoobTube(plms, guarded, level, streamer, (ushort)SnesButton.Right);
        AssertTrue(samus.InputLocked, "break sequence locks Samus");
        AssertTrue(samus.StationaryScriptControlLocked, "break sequence installs stationary command-zero animation ownership");
        var fields = typeof(SamusState).GetFields(System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
            .Where(field => !field.IsDefined(typeof(NonSerializedAttribute), false))
            .OrderBy(field => field.MetadataToken).ToArray();
        AssertTrue(fields.Any(field => field.Name == "<InputLocked>k__BackingField"),
            "existing input-lock serialized identity remains unchanged");
        var legacy = SuperMetroid.Desktop.DebuggerStateFieldMigrations.WithoutIntroductions(typeof(SamusState), fields,
            "<StationaryScriptControlLocked>k__BackingField", "<PreviousHealthForHurtCheck>k__BackingField");
        AssertTrue(legacy.SequenceEqual(fields.Where(field => field.Name is not "<StationaryScriptControlLocked>k__BackingField" and not "<PreviousHealthForHurtCheck>k__BackingField")),
            "pre-stationary-command Samus layout retains original fields in order without later draw-time health history");
        using (var saved = new MemoryStream())
        {
            SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(saved, samus);
            saved.Position = 0;
            var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SamusState>(saved);
            AssertTrue(restored.InputLocked && restored.StationaryScriptControlLocked,
                "mid-script state retains input and animation ownership");
            restored.InputLocked = false;
            AssertTrue(!restored.StationaryScriptControlLocked, "generic unlock clears stale stationary ownership");
        }
        AssertEqual(1, projectiles.Count, "break sequence initially spawns only the crack");
        AssertEqual(NoobTubeProjectileDefinition.Crack, projectiles[0].DefinitionPointer,
            "first spawned actor is the n00b-tube crack");

        // The first broken image lasts $30 frames. Two one-frame images follow, after which
        // the cartridge emits the sound, ten shards, six bubbles, and earthquake together.
        for (int frame = 0; frame < 50; frame++)
            StepNoobTube(plms, guarded, level, streamer);
        AssertEqual(17, projectiles.Count,
            "break burst contains one crack, ten shards, and six bubbles");
        AssertEqual(NoobTubeProjectileDefinition.Shard,
            projectiles[1].DefinitionPointer, "burst begins with shard parameter zero");
        AssertEqual((ushort)0x12, projectiles[10].Parameter,
            "tenth shard retains the final even ROM parameter");
        AssertEqual(NoobTubeProjectileDefinition.ReleasedAirBubble,
            projectiles[11].DefinitionPointer, "six bubbles follow all shards");
        AssertEqual((ushort)0x0a, projectiles[^1].Parameter,
            "sixth bubble retains the final even ROM parameter");
        AssertEqual(2, earthquakes.Count,
            "break burst writes both earthquake type and timer");
        AssertEqual(NoobTubePlmRomData.EarthquakeType, earthquakes[0],
            "earthquake type matches cartridge value");
        AssertEqual(NoobTubePlmRomData.EarthquakeTimer, earthquakes[1],
            "earthquake timer matches cartridge value");

        // The final broken frame lasts $60 frames, then event $0B is committed, water
        // physics are enabled, Samus is unlocked, and the one-shot actor deletes itself.
        for (int frame = 0; frame < 96; frame++)
            StepNoobTube(plms, guarded, level, streamer);
        AssertTrue(brokenEvent, "n00b-tube completion sets event $0B");
        AssertTrue(!samus.InputLocked, "n00b-tube completion unlocks Samus");
        AssertTrue(!samus.StationaryScriptControlLocked, "completion restores normal animation ownership");
        AssertEqual((ushort)RoomFxRomData.LiquidTide.SmallTideOption,
            roomFx.LiquidOptions,
            "n00b-tube completion clears the disable bit on shared room FX");
        roomFx.ApplyToSamusLiquidPhysics(samus.LiquidPhysics);
        AssertEqual((ushort)RoomFxRomData.LiquidTide.SmallTideOption,
            samus.LiquidPhysics.LiquidOptions,
            "next room-FX publication preserves enabled water physics");
        AssertEqual(SamusLiquidMedium.Water,
            samus.LiquidPhysics.DetermineMovementMedium(samus),
            "fully submerged Samus uses underwater movement after tube break");
        AssertEqual(0, plms.ActiveCount, "completed n00b-tube PLM deletes itself");

        Suite(nameof(VerifyNoobTubeProjectileInitializers), () => VerifyNoobTubeProjectileInitializers(bus, level, blockIndex, projectiles));
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "n00b-tube complete break never rereads its program source bytes");
        Suite(nameof(VerifyAlreadyBrokenNoobTube), () => VerifyAlreadyBrokenNoobTube(bus, level, streamer, samus));
        Console.WriteLine(
            "  N00b tube: setup, two-stage wake, debris, earthquake, event, water, and reload agree.");
    }

    private static void VerifyNoobTubeProgramDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Tube oracle revision");
        Suite(nameof(VerifyTubeControls), () => VerifyTubeControls(rom));
        Suite(nameof(VerifyTubeDraws), () => VerifyTubeDraws(rom));
        Suite(nameof(VerifyTubeTargets), () => VerifyTubeTargets(rom));
        Suite(nameof(VerifyTubeCallbacks), () => VerifyTubeCallbacks(rom));
        Suite(nameof(VerifyTubeEvents), () => VerifyTubeEvents(rom));
        Suite(nameof(VerifyTubeSound), () => VerifyTubeSound(rom));
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
        bus.WriteBytes(0xa19600, [0xff, 0xff]);
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            populationPointer: 0x9600,
            tilesetPointer: 0,
            new SnesVram(),
            new SnesCgram(),
            nextRandom: () => 0x4040,
            level: level,
            samus: new SamusState());

        foreach (NoobTubeProjectileRequest request in requests)
            enemies.SpawnNoobTubeProjectile(request, level.WidthInBlocks);
        AssertEqual(17, enemies.ActiveEnemyProjectileCount,
            "complete n00b-tube burst fits the native eighteen-slot projectile pool");

        RoomEnemyProjectileSlot crack = enemies.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.NoobTubeCrack);
        AssertEqual(0x0080, crack.XPosition,
            "crack initializer uses PLM X plus six blocks");
        AssertEqual(0x0050, crack.YPosition,
            "crack initializer uses PLM Y plus three blocks");

        RoomEnemyProjectileSlot firstShard = enemies.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.NoobTubeShard &&
            projectile.InstructionPointer == 0xd47d);
        AssertEqual(0x0048, firstShard.Variable1,
            "first shard stores signed X origin in variable F");
        AssertEqual(0x0058, firstShard.YPosition,
            "first shard stores its signed Y offset");
        AssertEqual(0xfe80, firstShard.XVelocity,
            "first shard retains negative 8.8 X velocity");
        AssertEqual(0x0140, firstShard.YVelocity,
            "first shard retains positive 8.8 Y velocity");

        RoomEnemyProjectileSlot lastShard = enemies.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.NoobTubeShard &&
            projectile.InstructionPointer == 0xd5bd);
        AssertEqual(0x0078, lastShard.Variable1,
            "last shard uses final signed X offset");
        AssertEqual(0x0060, lastShard.YPosition,
            "last shard uses final signed Y offset");
        AssertEqual(0xffc0, lastShard.XVelocity,
            "last shard uses final signed X velocity");
        AssertEqual(0x0180, lastShard.YVelocity,
            "last shard uses final signed Y velocity");

        RoomEnemyProjectileSlot firstBubble = enemies.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.NoobTubeReleasedAirBubble &&
            projectile.Variable1 == 0x0048);
        AssertEqual(0x0070, firstBubble.YPosition,
            "first bubble uses its first absolute PLM-origin offset");
        AssertEqual(NoobTubeProjectileRomData.BubbleInitialYVelocity,
            firstBubble.YVelocity, "first bubble starts with cartridge upward velocity");

        RoomEnemyProjectileSlot lastBubble = enemies.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.NoobTubeReleasedAirBubble &&
            projectile.Variable1 == 0x00d8);
        AssertEqual(0x0074, lastBubble.YPosition,
            "last bubble uses its final absolute PLM-origin offset");

        AssertTrue(requests.All(request => request.PlmBlockIndex == blockIndex),
            "every burst actor retains the executing PLM block index");
    }

    /// <summary>Confirms event $0B skips every one-shot effect on subsequent room loads.</summary>
    private static void VerifyAlreadyBrokenNoobTube(
        TestAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        SamusState samus)
    {
        const ushort populationPointer = 0x9400;
        var projectiles = new List<NoobTubeProjectileRequest>();
        samus.InputLocked = false;
        RoomLayer3FxState roomFx = CreateNoobTubeWaterFx(bus);
        roomFx.ApplyToSamusLiquidPhysics(samus.LiquidPhysics);
        var guarded = new NoobTubeProgramReadGuard(bus);
        var plms = new RoomPlmSystem();
        plms.LoadRoomPopulation(
            guarded,
            level,
            streamer,
            new SnesVram(),
            RoomPlmPopulationImporter.Read(guarded, populationPointer),
            new Bank80SystemState(),
            AreaId.Maridia,
            getSamus: () => samus,
            isAreaTorizoDefeated: () => false,
            hasEvent: eventNumber => eventNumber == NoobTubePlmRomData.BrokenEvent,
            setEvent: _ => throw new InvalidOperationException(
                "Already-broken n00b tube must not set its event again."),
            roomFx: roomFx,
            setEarthquakeTimer: _ => throw new InvalidOperationException(
                "Already-broken n00b tube must not start an earthquake."),
            setEarthquakeType: _ => throw new InvalidOperationException(
                "Already-broken n00b tube must not start an earthquake."),
            spawnNoobTubeProjectile: projectiles.Add);

        StepNoobTube(plms, guarded, level, streamer);
        AssertEqual(0, plms.ActiveCount, "event-$0B n00b tube deletes on its first step");
        AssertEqual(0, projectiles.Count, "event-$0B n00b tube spawns no break debris");
        AssertEqual((ushort)RoomFxRomData.LiquidTide.SmallTideOption,
            roomFx.LiquidOptions,
            "event-$0B room load clears the shared water-disable bit");
        roomFx.ApplyToSamusLiquidPhysics(samus.LiquidPhysics);
        AssertEqual(SamusLiquidMedium.Water,
            samus.LiquidPhysics.DetermineMovementMedium(samus),
            "event-$0B reload keeps fully submerged movement underwater");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "already-broken n00b-tube branch never rereads program bytes");
    }

    private sealed class NoobTubeProgramReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (address is >= 0x84d4d4 and <= 0x84d518 or
                >= 0x84d521 and <= 0x84d524)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"N00b-tube PLM reread compiled program byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    /// <summary>
    /// Loads the Glass Tunnel's relevant FX words: water at Y $0010 with the small-tide
    /// option and the bit that suppresses water physics until the tube breaks.
    /// </summary>
    private static RoomLayer3FxState CreateNoobTubeWaterFx(TestAddressSpace bus)
    {
        // FX mechanics now come from compiled records. Use the actual Glass Tunnel
        // record and import its presentation dependencies at this verification boundary.
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var roomFx = CreateRetailFxState(rom);
        roomFx.Load(bus, new SnesVram(), new SnesCgram(), 0x9c94, doorPointer: 0,
            randomNumber: 0);
        return roomFx;
    }

    private static void StepNoobTube(
        RoomPlmSystem plms,
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        ushort controllerNewInput = 0) =>
        plms.Step(
            bus,
            level,
            streamer,
            layer1XPosition: 0,
            layer1YPosition: 0,
            bg1XOffset: 0,
            scrolls: null,
            enemyDeaths: 0,
            enemyDeathQuota: 0,
            controllerNewInput);

    /// <summary>Writes the exact n00b-tube list words plus intentionally tiny draw data.</summary>
    private static void SeedNoobTubeRom(TestAddressSpace bus)
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
        WriteWord(bus, 0x84d70e, RoomPlmInstructionLists.NoobTube);
        WriteWord(bus, 0x84d4d4, (ushort)RoomPlmInstruction.GotoIfEventSet);
        WriteWord(bus, 0x84d4d6, (ushort)NoobTubePlmRomData.BrokenEvent);
        WriteWord(bus, 0x84d4d8, 0xd521);
        WriteWord(bus, 0x84d4da, (ushort)RoomPlmInstruction.LinkInstruction);
        WriteWord(bus, 0x84d4dc, 0xd4e8);
        WriteWord(bus, 0x84d4de, (ushort)RoomPlmInstruction.InstallPreInstruction);
        WriteWord(bus, 0x84d4e0, (ushort)NoobTubePlmPreInstruction.WakeOnPowerBomb);
        WriteWord(bus, 0x84d4e2, 1);
        WriteWord(bus, 0x84d4e4, 0x98d1);
        WriteWord(bus, 0x84d4e6, (ushort)RoomPlmInstruction.Sleep);
        WriteWord(bus, 0x84d4e8, (ushort)RoomPlmInstruction.LinkInstruction);
        WriteWord(bus, 0x84d4ea, 0xd4f2);
        WriteWord(bus, 0x84d4ec, (ushort)RoomPlmInstruction.InstallPreInstruction);
        WriteWord(bus, 0x84d4ee, (ushort)NoobTubePlmPreInstruction.WakeOnAcceptedInput);
        WriteWord(bus, 0x84d4f0, (ushort)RoomPlmInstruction.Sleep);
        WriteWord(bus, 0x84d4f2, (ushort)RoomPlmInstruction.ClearPreInstruction);
        WriteWord(bus, 0x84d4f4, (ushort)RoomPlmInstruction.LockSamus);
        WriteWord(bus, 0x84d4f6, (ushort)RoomPlmInstruction.SpawnNoobTubeCrack);
        WriteWord(bus, 0x84d4f8, 0x0030);
        WriteWord(bus, 0x84d4fa, 0x98d7);
        WriteWord(bus, 0x84d4fc, 1);
        WriteWord(bus, 0x84d4fe, 0x9991);
        WriteWord(bus, 0x84d500, 1);
        WriteWord(bus, 0x84d502, 0x99e5);
        WriteWord(bus, 0x84d504, (ushort)RoomPlmInstruction.QueueSoundLibrary2Maximum6);
        bus.WriteByte(0x84d506, NoobTubePlmRomData.BreakSound);
        WriteWord(bus, 0x84d507, (ushort)RoomPlmInstruction.SpawnNoobTubeShardsAndBubbles);
        WriteWord(bus, 0x84d509, (ushort)RoomPlmInstruction.TriggerNoobTubeEarthquake);
        WriteWord(bus, 0x84d50b, 0x0060);
        WriteWord(bus, 0x84d50d, 0x98dd);
        WriteWord(bus, 0x84d50f, (ushort)RoomPlmInstruction.SetEvent);
        WriteWord(bus, 0x84d511, (ushort)NoobTubePlmRomData.BrokenEvent);
        WriteWord(bus, 0x84d513, (ushort)RoomPlmInstruction.EnableNoobTubeWaterPhysics);
        WriteWord(bus, 0x84d515, (ushort)RoomPlmInstruction.UnlockSamus);
        WriteWord(bus, 0x84d517, (ushort)RoomPlmInstruction.Delete);
        WriteWord(bus, 0x84d521, (ushort)RoomPlmInstruction.EnableNoobTubeWaterPhysics);
        WriteWord(bus, 0x84d523, (ushort)RoomPlmInstruction.Delete);

        WriteOneBlockDraw(bus, 0x98d1,
            unchecked((ushort)(((ushort)RoomCollisionType.ShootableBlock << 12) | 0x0001)));
        WriteOneBlockDraw(bus, 0x98d7, 0x8002);
        WriteOneBlockDraw(bus, 0x9991, 0x8003);
        WriteOneBlockDraw(bus, 0x99e5, 0x8004);
        WriteOneBlockDraw(bus, 0x98dd, 0x8005);
    }
}
