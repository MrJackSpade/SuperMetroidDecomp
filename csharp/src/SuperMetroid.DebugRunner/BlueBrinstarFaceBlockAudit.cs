using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed behavioral audit for Blue Brinstar face-block definition $A0:EA7F. It proves
/// every retail population record, both directional one-shot animations, the collected-item
/// gate, the singleton palette hook and door-transition pause, inert touch behavior, and the
/// private pass-through shot callback against an untouched cartridge image.
/// </summary>
internal static class BlueBrinstarFaceBlockAudit
{
    private const ushort DefinitionPointer = 0xea7f;
    private const ushort AuditRoomPointer = 0x9e9f;
    private const ushort AuditStatePointer = 0x9eb1;
    private const ushort AuditPopulationPointer = 0x93ac;
    private const ushort MorphBallItemMask = 0x0004;

    private static readonly FaceBlockPopulation[] RetailPopulations =
    [
        new(0x918d,
        [
            new(0x0048, 0x0088, 0x0000, 0x0000),
            new(0x00b8, 0x0088, 0x0000, 0x0000),
            new(0x00e8, 0x0116, 0x0000, 0x0000),
            new(0x00e8, 0x0166, 0x0000, 0x0000),
            new(0x0018, 0x0116, 0x0000, 0x0000),
        ]),
        new(0x9200,
        [
            new(0x0038, 0x0228, 0x0000, 0x0000),
            new(0x0038, 0x02a8, 0x0000, 0x0000),
            new(0x02e8, 0x02b8, 0x0000, 0x0000),
            new(0x0268, 0x0088, 0x0000, 0x0000),
            new(0x0288, 0x0088, 0x0000, 0x0000),
        ]),
        new(0x9326,
        [
            new(0x0548, 0x0240, 0x0000, 0x0000),
            new(0x05b8, 0x0240, 0x0000, 0x0000),
            new(0x0488, 0x02b8, 0x0000, 0x0000),
            new(0x0428, 0x02b8, 0x0000, 0x0000),
        ]),
        new(0x93ac,
        [
            new(0x0548, 0x0240, 0x0040, 0x0001),
            new(0x05b8, 0x0240, 0x0040, 0x0000),
            new(0x0488, 0x02b8, 0x0040, 0x0000),
            new(0x0428, 0x02b8, 0x0040, 0x0001),
        ]),
        new(0x966f,
        [
            new(0x0038, 0x0228, 0x0000, 0x0000),
            new(0x0038, 0x02a8, 0x0000, 0x0000),
            new(0x02e8, 0x02b8, 0x0000, 0x0000),
            new(0x0268, 0x0088, 0x0000, 0x0000),
            new(0x0288, 0x0088, 0x0000, 0x0000),
        ]),
        new(0x9bc6,
        [
            new(0x0048, 0x0088, 0x0000, 0x0000),
            new(0x00b8, 0x0088, 0x0000, 0x0001),
            new(0x00e8, 0x0116, 0x0000, 0x0000),
            new(0x00e8, 0x0166, 0x0000, 0x0000),
            new(0x0018, 0x0116, 0x0000, 0x0000),
        ]),
    ];

    private static readonly ushort[] BasePalette =
    [
        0x3800, 0x72b2, 0x71c7, 0x2461,
        0x1840, 0x7a8e, 0x660b, 0x4d03,
        0x30a4, 0x30a4, 0x2461, 0x1840,
        0x0800, 0, 0, 0,
    ];

    private static readonly ushort[][] PaletteFrames =
    [
        [0x001f, 0x0012, 0x000a, 0x002b],
        [0x051f, 0x0096, 0x0011, 0x0007],
        [0x0a3f, 0x013b, 0x0018, 0x000d],
        [0x0f3f, 0x01bf, 0x001f, 0x0012],
        [0x0f3f, 0x01bf, 0x001f, 0x0012],
        [0x0a3f, 0x013b, 0x0018, 0x000d],
        [0x051f, 0x0096, 0x0011, 0x0007],
        [0x001f, 0x0012, 0x000a, 0x002b],
    ];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyDefinition(bus);
        VerifyRetailPopulations(bus);
        VerifyRomTables(bus);

        // The selected state is the cartridge's Morph-Ball-and-missiles branch. The default
        // state uses the same positions with activation distance zero; this exact state owns
        // the four records whose proximity behavior can actually execute.
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            AuditRoomPointer,
            new RoomStateSelectionContext(default, 0, true, false));
        if (room.State.Pointer != AuditStatePointer ||
            room.State.EnemyPopulationPointer != AuditPopulationPointer)
        {
            throw new InvalidDataException(
                $"Face-block audit selected state/population " +
                $"$8F:{room.State.Pointer:X4}/$A1:{room.State.EnemyPopulationPointer:X4}.");
        }
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);

        VerifyDirectionalActivationAndAnimation(bus, room, assets);
        VerifyStrictRangeAndCollectedItemGate(bus, room, assets);
        VerifyPaletteHook(bus, room, assets);
        VerifyInertCombatAndDrawing(bus, room, assets);

        Console.WriteLine(
            "Blue Brinstar face-block audit passed: six retail populations / 28 records, " +
            "header and ROM tables, collected-Morph-Ball gate, strict side/range activation, " +
            "both one-shot animations, stationary OBJ output, eight-frame palette cycle with " +
            "door-transition pause, no-op touch, grapple cancel, and pass-through shots verified.");
        return 0;
    }

    private static void VerifyDefinition(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, DefinitionPointer);
        if (definition.TileDataSize != 0x0200 || definition.PalettePointer != 0xe7ac ||
            definition.Health != 20 || definition.Damage != 0 ||
            definition.XRadius != 8 || definition.YRadius != 8 ||
            definition.Bank != 0xa8 || definition.HurtAiTime != 0 ||
            definition.HurtSoundEffect != 0 || definition.BossId != 0 ||
            definition.InitializationAiPointer != 0xe82e || definition.PartCount != 1 ||
            definition.MainAiPointer != 0xe8ae || definition.GrappleAiPointer != 0x800f ||
            definition.HurtAiPointer != 0x804c || definition.FrozenAiPointer != 0x8041 ||
            definition.TimeFrozenAiPointer != 0 || definition.DeathAnimation != 0 ||
            definition.PowerBombReactionPointer != 0 || definition.VariantIndex != 0 ||
            definition.TouchAiPointer != 0x804c || definition.ShotAiPointer != 0xe91d ||
            definition.InitialSpritemapPointer != 0 || definition.TileDataAddress != 0xb1ba00 ||
            definition.Layer != 2 || definition.ItemDropChancesPointer != 0xf45e ||
            definition.VulnerabilityPointer != 0xeec6 || definition.NamePointer != 0xdf3b)
        {
            throw new InvalidDataException(
                "Blue Brinstar face-block header disagrees with retail definition $A0:EA7F.");
        }
    }

    private static void VerifyRetailPopulations(ISnesAddressSpace bus)
    {
        int totalRecords = 0;
        foreach (FaceBlockPopulation population in RetailPopulations)
        {
            var actual = new List<FaceBlockRecord>();
            int cursor = 0xa10000 | population.Pointer;
            for (int recordIndex = 0;
                recordIndex < RoomEnemySystem.MaximumEnemyCount;
                recordIndex++, cursor += 16)
            {
                ushort definition = ReadWord(bus, cursor);
                if (definition == 0xffff)
                    break;
                if (definition != DefinitionPointer)
                    continue;

                ushort initialization = ReadWord(bus, cursor + 6);
                ushort properties = ReadWord(bus, cursor + 8);
                ushort extraProperties = ReadWord(bus, cursor + 10);
                if (initialization != 0 || properties != 0xa000 || extraProperties != 0)
                {
                    throw new InvalidDataException(
                        $"Population $A1:{population.Pointer:X4} has a non-retail face-block record.");
                }
                actual.Add(new FaceBlockRecord(
                    ReadWord(bus, cursor + 2),
                    ReadWord(bus, cursor + 4),
                    ReadWord(bus, cursor + 12),
                    ReadWord(bus, cursor + 14)));
            }

            if (!actual.SequenceEqual(population.Records))
            {
                throw new InvalidDataException(
                    $"Population $A1:{population.Pointer:X4} lost its exact face-block records.");
            }
            totalRecords += actual.Count;
        }

        if (totalRecords != 28)
            throw new InvalidDataException($"Face-block inventory found {totalRecords}, expected 28 records.");
    }

    private static void VerifyRomTables(ISnesAddressSpace bus)
    {
        VerifyWords(bus, 0xa8e7ac, BasePalette, "base palette");
        VerifyWords(bus, 0xa8e7cc, PaletteFrames.SelectMany(frame => frame).ToArray(),
            "animated palette");
        VerifyWords(bus, 0xa8e80c,
            [0x0030, 0xe92c, 0x0010, 0xe942, 0x0010, 0xe958, 0x812f],
            "Samus-left instruction list");
        VerifyWords(bus, 0xa8e81a,
            [0x0030, 0xe92c, 0x0010, 0xe96e, 0x0010, 0xe984, 0x812f],
            "Samus-right instruction list");
        VerifyWords(bus, 0xa8e828, [0x0001, 0xe92c, 0x812f], "neutral instruction list");
    }

    private static void VerifyDirectionalActivationAndAnimation(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedFaceBlocks loaded = Load(bus, room, assets, retainedRecordCount: 2, hasMorphBall: true);
        RoomEnemySlot rightActor = loaded.Enemies.Slots[0];
        RoomEnemySlot leftActor = loaded.Enemies.Slots[1];
        BlueBrinstarFaceBlockEnemyState rightState = State(loaded.Enemies, rightActor);
        BlueBrinstarFaceBlockEnemyState leftState = State(loaded.Enemies, leftActor);

        if (rightActor.Parameter2 != 0x8000 || leftActor.Parameter2 != 0 ||
            rightState.ActivationSide != BlueBrinstarFaceBlockActivationSide.Right ||
            leftState.ActivationSide != BlueBrinstarFaceBlockActivationSide.Left ||
            rightActor.CurrentInstruction != 0xe828 || leftActor.CurrentInstruction != 0xe828)
        {
            throw new InvalidDataException("Face-block parameter normalization/init list diverged from ROM.");
        }

        // X=$587 is 63 pixels right of the first actor and 49 pixels left of the second.
        // One cartridge frame therefore proves both opposite-side branches without changing
        // either authored actor position or manufacturing a second population.
        loaded.Samus.XPosition = 0x0587;
        loaded.Samus.YPosition = 0x0240;
        ushort rightX = rightActor.XPosition;
        ushort leftX = leftActor.XPosition;
        Step(loaded, room, assets);
        if (!rightState.Activated || !leftState.Activated ||
            rightState.LastHorizontalDelta != 0x003f ||
            leftState.LastHorizontalDelta != 0xffcf ||
            rightActor.SpritemapPointer != 0xe92c || leftActor.SpritemapPointer != 0xe92c ||
            rightActor.XPosition != rightX || leftActor.XPosition != leftX)
        {
            throw new InvalidDataException(
                $"Face-block simultaneous side activation failed: right={rightState.Activated}/" +
                $"${rightState.LastHorizontalDelta:X4}, left={leftState.Activated}/" +
                $"${leftState.LastHorizontalDelta:X4}.");
        }

        // The neutral pose lasts 48 actor frames. Each list then selects a distinct middle
        // face for 16 frames and a distinct final face before sleeping forever on that map.
        for (int frame = 0; frame < 48; frame++)
            Step(loaded, room, assets);
        if (rightActor.SpritemapPointer != 0xe96e || leftActor.SpritemapPointer != 0xe942)
            throw new InvalidDataException("Face blocks did not select their directional middle maps.");

        for (int frame = 0; frame < 16; frame++)
            Step(loaded, room, assets);
        if (rightActor.SpritemapPointer != 0xe984 || leftActor.SpritemapPointer != 0xe958)
            throw new InvalidDataException("Face blocks did not select their directional final maps.");

        for (int frame = 0; frame < 48; frame++)
            Step(loaded, room, assets);
        if (rightActor.SpritemapPointer != 0xe984 || leftActor.SpritemapPointer != 0xe958 ||
            rightActor.XPosition != rightX || leftActor.XPosition != leftX)
        {
            throw new InvalidDataException("Face-block sleep state changed map or moved an actor.");
        }
    }

    private static void VerifyStrictRangeAndCollectedItemGate(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedFaceBlocks loaded = Load(bus, room, assets, retainedRecordCount: 1, hasMorphBall: true);
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        BlueBrinstarFaceBlockEnemyState state = State(loaded.Enemies, actor);
        loaded.Samus.YPosition = actor.YPosition;

        // The first record requires Samus on the right. A near point on the wrong side and
        // an exactly-64-pixel point on the correct side must both fail (`CMP; BPL`), while
        // 63 pixels on the correct side must wake the actor.
        loaded.Samus.XPosition = unchecked((ushort)(actor.XPosition - 1));
        Step(loaded, room, assets);
        if (state.Activated)
            throw new InvalidDataException("Face block activated from the population-forbidden side.");

        loaded.Samus.XPosition = unchecked((ushort)(actor.XPosition + actor.Parameter1));
        Step(loaded, room, assets);
        if (state.Activated)
            throw new InvalidDataException("Face block admitted equality at its strict range boundary.");

        loaded.Samus.XPosition = unchecked((ushort)(actor.XPosition + actor.Parameter1 - 1));
        Step(loaded, room, assets);
        if (!state.Activated)
            throw new InvalidDataException("Face block rejected an in-range target on its authored side.");

        LoadedFaceBlocks gated = Load(bus, room, assets, retainedRecordCount: 1, hasMorphBall: false);
        RoomEnemySlot gatedActor = gated.Enemies.Slots[0];
        BlueBrinstarFaceBlockEnemyState gatedState = State(gated.Enemies, gatedActor);
        gated.Samus.XPosition = unchecked((ushort)(gatedActor.XPosition + 1));
        gated.Samus.YPosition = gatedActor.YPosition;
        for (int frame = 0; frame < 32; frame++)
            Step(gated, room, assets);
        if (gatedState.Activated)
            throw new InvalidDataException("Face block woke before Morph Ball was collected.");

        // Main AI rechecks collected-items every frame; acquiring Morph Ball in-place must
        // install the palette hook and wake the same already-loaded actor without a reload.
        gated.Samus.CollectedItems |= MorphBallItemMask;
        Step(gated, room, assets);
        if (!gatedState.Activated)
            throw new InvalidDataException("Face block did not react to late Morph Ball acquisition.");
    }

    private static void VerifyPaletteHook(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedFaceBlocks loaded = Load(bus, room, assets, retainedRecordCount: 2, hasMorphBall: true);
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        int destination = 128 + ((actor.PaletteIndex >> 9) & 7) * 16 + 9;
        ushort[] initial = loaded.Cgram.Colors.Slice(destination, 4).ToArray();

        // Keep Samus outside both actors' strict squares so activation cannot restart the
        // timer. The hook's first palette record arrives only on the sixteenth enemy frame.
        loaded.Samus.XPosition = 0;
        loaded.Samus.YPosition = 0;
        for (int frame = 0; frame < 15; frame++)
            Step(loaded, room, assets);
        if (!loaded.Cgram.Colors.Slice(destination, 4).SequenceEqual(initial))
            throw new InvalidDataException("Face-block palette hook fired before frame 16.");

        Step(loaded, room, assets);
        AssertPaletteFrame(loaded.Cgram, destination, frame: 0);

        // WRAM $0795 suspends the graphics hook without decrementing its countdown. Thirty-
        // two paused frames must leave frame zero intact; sixteen resumed frames then select
        // frame one, proving this is a paused native clock rather than a visual-rate modulo.
        loaded.Enemies.ElevatorDoorTransitionActive = true;
        for (int frame = 0; frame < 32; frame++)
            Step(loaded, room, assets);
        AssertPaletteFrame(loaded.Cgram, destination, frame: 0);

        loaded.Enemies.ElevatorDoorTransitionActive = false;
        for (int frame = 0; frame < 16; frame++)
            Step(loaded, room, assets);
        AssertPaletteFrame(loaded.Cgram, destination, frame: 1);

        // Walk the remaining six records and the wrapped frame zero. This covers the entire
        // symmetric eight-record ROM table rather than accepting two hand-picked colors.
        for (int paletteFrame = 2; paletteFrame < PaletteFrames.Length; paletteFrame++)
        {
            for (int frame = 0; frame < 16; frame++)
                Step(loaded, room, assets);
            AssertPaletteFrame(loaded.Cgram, destination, paletteFrame);
        }
        for (int frame = 0; frame < 16; frame++)
            Step(loaded, room, assets);
        AssertPaletteFrame(loaded.Cgram, destination, frame: 0);
    }

    private static void VerifyInertCombatAndDrawing(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedFaceBlocks loaded = Load(bus, room, assets, retainedRecordCount: 1, hasMorphBall: true);
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        loaded.Samus.XPosition = unchecked((ushort)(actor.XPosition + 1));
        loaded.Samus.YPosition = actor.YPosition;
        Step(loaded, room, assets);

        var oam = new OamBuffer();
        oam.BeginFrame();
        (ushort cameraX, ushort cameraY) = CenterCamera(room, actor);
        loaded.Enemies.DrawLayers(oam, cameraX, cameraY, firstLayer: 0, lastLayer: 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Face-block ROM spritemap emitted no OBJ pieces.");

        ushort healthBeforeTouch = loaded.Samus.Health;
        loaded.Samus.KnockbackActive = false;
        if (!loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0) ||
            loaded.Samus.Health != healthBeforeTouch || loaded.Samus.KnockbackActive)
        {
            throw new InvalidDataException("Face-block literal-RTL touch callback was not inert.");
        }

        var shots = new SamusProjectileSystem();
        SamusProjectileSlot shot = shots.Slots[0];
        ArmProjectile(shot, actor);
        ushort actorHealth = actor.Health;
        if (loaded.Enemies.ResolveOrdinaryProjectileHits(
                bus,
                shots,
                new SamusBombProjectileSystem(),
                loaded.Samus) != 1 ||
            actor.Health != actorHealth || shot.Direction != 0x0002 ||
            shot.InstructionPointer != 0x9000 || !shot.IsActive)
        {
            throw new InvalidDataException(
                $"Face-block pass-through shot failed: health={actorHealth}->{actor.Health}, " +
                $"direction=${shot.Direction:X4}, list=${shot.InstructionPointer:X4}, " +
                $"active={shot.IsActive}.");
        }

        if (loaded.Enemies.ResolveOrdinaryPowerBombHits(
                bus,
                actor.XPosition,
                actor.YPosition,
                explosionRadius: 64) != 0 || actor.Health != actorHealth)
        {
            throw new InvalidDataException("Face block incorrectly accepted power-bomb damage.");
        }

        GrappleEnemyCollision grapple = loaded.Enemies.ResolveGrappleEndpoint(
            actor.XPosition,
            actor.YPosition);
        if (!grapple.Collided || grapple.Reaction != GrappleEnemyReaction.Cancel ||
            actor.AiHandlerBits != 1)
        {
            throw new InvalidDataException(
                $"Face-block grapple reaction failed: collided={grapple.Collided}, " +
                $"reaction={grapple.Reaction}, handler=${actor.AiHandlerBits:X4}.");
        }
        Step(loaded, room, assets);
        if (actor.AiHandlerBits != 4)
            throw new InvalidDataException("Face-block grapple cancel did not enter frozen/cancel AI.");
    }

    private static LoadedFaceBlocks Load(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        int retainedRecordCount,
        bool hasMorphBall)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        // The retail population also contains ordinary Blue Brinstar enemies before this
        // contiguous face-block run. Begin at the first $EA7F record, then install the same
        // synthetic terminator used by other focused audits after the requested number of
        // untouched records; tileset, definitions, coordinates, and parameters remain ROM.
        ushort faceBlockRunPointer = FindFirstDefinitionRecordPointer(
            bus,
            AuditPopulationPointer,
            DefinitionPointer);
        var isolatedPopulation = new PopulationPrefixAddressSpace(
            bus,
            faceBlockRunPointer,
            retainedRecordCount,
            deathQuota: 0);
        var random = new Bank80SystemState(0x4567);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            CollectedItems = hasMorphBall ? MorphBallItemMask : (ushort)0,
        };
        samus.RefreshCollisionRadii(isolatedPopulation);
        samus.InitializeAnimation(isolatedPopulation);

        var enemies = new RoomEnemySystem();
        enemies.Load(
            isolatedPopulation,
            faceBlockRunPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus);

        if (enemies.EnemyCount != retainedRecordCount ||
            enemies.Slots.Take(retainedRecordCount).Any(
                actor => actor.EnemyDefinitionPointer != DefinitionPointer))
        {
            throw new InvalidDataException("Focused face-block population did not load exact prefix.");
        }
        return new LoadedFaceBlocks(enemies, samus, cgram);
    }

    private static ushort FindFirstDefinitionRecordPointer(
        ISnesAddressSpace bus,
        ushort populationPointer,
        ushort definitionPointer)
    {
        ushort cursor = populationPointer;
        for (int recordIndex = 0;
            recordIndex < RoomEnemySystem.MaximumEnemyCount;
            recordIndex++, cursor = unchecked((ushort)(cursor + 16)))
        {
            ushort definition = ReadWord(bus, 0xa10000 | cursor);
            if (definition == definitionPointer)
                return cursor;
            if (definition == 0xffff)
                break;
        }
        throw new InvalidDataException(
            $"Population $A1:{populationPointer:X4} contains no ${definitionPointer:X4} record.");
    }

    private static void Step(
        LoadedFaceBlocks loaded,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        (ushort cameraX, ushort cameraY) = CenterCamera(room, actor);
        loaded.Enemies.StepFrame(
            cameraX,
            cameraY,
            timeIsFrozen: false,
            loaded.Samus,
            level: assets.LevelData);
    }

    private static (ushort X, ushort Y) CenterCamera(
        CartridgeRoomHeader room,
        RoomEnemySlot actor)
    {
        int maximumX = Math.Max(0, room.WidthInScreens * 256 - 256);
        int maximumY = Math.Max(0, room.HeightInScreens * 256 - 224);
        return (
            unchecked((ushort)Math.Clamp(actor.XPosition - 128, 0, maximumX)),
            unchecked((ushort)Math.Clamp(actor.YPosition - 112, 0, maximumY)));
    }

    private static BlueBrinstarFaceBlockEnemyState State(
        RoomEnemySystem enemies,
        RoomEnemySlot actor) =>
        enemies.BlueBrinstarFaceBlockStates[actor.SlotIndex] ??
        throw new InvalidDataException(
            $"Face-block slot {actor.SlotIndex} has no typed state.");

    private static void AssertPaletteFrame(SnesCgram cgram, int destination, int frame)
    {
        if (!cgram.Colors.Slice(destination, 4).SequenceEqual(PaletteFrames[frame]))
        {
            throw new InvalidDataException(
                $"Face-block palette frame {frame} was " +
                $"{string.Join(' ', cgram.Colors.Slice(destination, 4).ToArray().Select(c => $"{c:X4}"))}.");
        }
    }

    private static void ArmProjectile(SamusProjectileSlot projectile, RoomEnemySlot target)
    {
        projectile.ClearFields();
        projectile.Type = 0;
        projectile.Damage = 20;
        projectile.Direction = 0x0012;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static void VerifyWords(
        ISnesAddressSpace bus,
        int address,
        ushort[] expected,
        string name)
    {
        for (int index = 0; index < expected.Length; index++)
        {
            ushort actual = ReadWord(bus, address + index * 2);
            if (actual != expected[index])
            {
                throw new InvalidDataException(
                    $"Face-block {name} word {index} is ${actual:X4}, " +
                    $"expected ${expected[index]:X4}.");
            }
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct FaceBlockRecord(
        ushort X,
        ushort Y,
        ushort ActivationDistance,
        ushort SideSelector);

    private readonly record struct FaceBlockPopulation(
        ushort Pointer,
        IReadOnlyList<FaceBlockRecord> Records);

    private readonly record struct LoadedFaceBlocks(
        RoomEnemySystem Enemies,
        SamusState Samus,
        SnesCgram Cgram);
}
