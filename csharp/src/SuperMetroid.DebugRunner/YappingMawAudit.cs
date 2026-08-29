using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed behavioral audit for Yapping Maw definition $A0:E7BF. This deliberately
/// verifies the three native actor pools separately: the bank-$A8 mouth enemy, four bank-$86
/// body links, and the bank-$B4 root object. A flat host sprite would look plausible while
/// losing the exact animation, freeze-palette, grab, and cleanup ownership rules.
/// </summary>
internal static class YappingMawAudit
{
    private const ushort DefinitionPointer = 0xe7bf;
    private const ushort AuditRoomPointer = 0x965b;
    private const ushort AuditStatePointer = 0x9668;
    private const ushort AuditPopulationPointer = 0x89f2;

    private static readonly MawPopulation[] RetailPopulations =
    [
        new(0x89f2, [new(0x0088, 0x0038, 0x0070, 0)]),
        new(0x90c7, [new(0x0170, 0x00d0, 0x0036, 1), new(0x00f0, 0x00d0, 0x0036, 1)]),
        new(0x95e4, [new(0x0190, 0x01c8, 0x0030, 1), new(0x0080, 0x01c8, 0x0030, 1)]),
        new(0x9b13, [new(0x0198, 0x00c8, 0x0050, 1), new(0x0258, 0x00c0, 0x0050, 1), new(0x03a8, 0x00c0, 0x0050, 1)]),
        new(0xa3f5, [new(0x0188, 0x00d8, 0x0080, 1), new(0x0219, 0x00d7, 0x0080, 1), new(0x02f8, 0x00d8, 0x0080, 1)]),
        new(0xdf30, [new(0x00b0, 0x00f0, 0x0040, 1), new(0x004d, 0x00f0, 0x0040, 1)]),
    ];

    private static readonly ushort[] Palette =
    [
        0x3800, 0x57ff, 0x42f7, 0x0929,
        0x00a5, 0x4f5a, 0x36b5, 0x2610,
        0x1dce, 0, 0, 0, 0, 0, 0, 0,
    ];

    private static readonly ushort[] DirectionLists =
        [0x9f6f, 0x9f85, 0x9f9b, 0x9fb1, 0x9fc7, 0x9fdd, 0x9ff3, 0xa009];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyDefinition(bus);
        VerifyRetailPopulations(bus);
        VerifyRomTables(bus);

        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, AuditRoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        VerifyInitializationAnimationAndGrab(bus, room, assets);
        VerifyShotDeathAndMultipartCleanup(bus, room, assets);

        Console.WriteLine(
            "Yapping Maw audit passed: all six retail populations, ROM palette/direction/" +
            "projectile/root tables, four-link extension, OBJ output, custom no-damage grab, " +
            "ice release/frozen palette, ordinary shot death, and multipart cleanup were verified.");
        return 0;
    }

    private static void VerifyDefinition(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, DefinitionPointer);
        if (definition.TileDataSize != 0x0400 || definition.PalettePointer != 0x9f4f ||
            definition.Health != 20 || definition.Damage != 30 ||
            definition.XRadius != 8 || definition.YRadius != 8 ||
            definition.Bank != 0xa8 || definition.HurtAiTime != 0 ||
            definition.HurtSoundEffect != 0x003e || definition.BossId != 0 ||
            definition.InitializationAiPointer != 0xa148 || definition.PartCount != 1 ||
            definition.MainAiPointer != 0xa211 || definition.GrappleAiPointer != 0x800f ||
            definition.HurtAiPointer != 0x804c || definition.FrozenAiPointer != 0xa835 ||
            definition.TimeFrozenAiPointer != 0 || definition.DeathAnimation != 4 ||
            definition.PowerBombReactionPointer != 0 || definition.VariantIndex != 0 ||
            definition.TouchAiPointer != 0xa799 || definition.ShotAiPointer != 0xa7bd ||
            definition.InitialSpritemapPointer != 0 || definition.TileDataAddress != 0xb1aa00 ||
            definition.Layer != 5 || definition.ItemDropChancesPointer != 0xf3e0 ||
            definition.VulnerabilityPointer != 0xee9a || definition.NamePointer != 0xdeaf)
        {
            throw new InvalidDataException(
                "Yapping Maw header disagrees with retail definition $A0:E7BF.");
        }
    }

    private static void VerifyRetailPopulations(ISnesAddressSpace bus)
    {
        foreach (MawPopulation population in RetailPopulations)
        {
            var actual = new List<MawRecord>();
            int cursor = 0xa10000 | population.Pointer;
            for (int recordIndex = 0; recordIndex < 32; recordIndex++, cursor += 16)
            {
                ushort definition = ReadWord(bus, cursor);
                if (definition == 0xffff)
                    break;
                if (definition != DefinitionPointer)
                    continue;

                ushort properties = ReadWord(bus, cursor + 8);
                ushort extraProperties = ReadWord(bus, cursor + 10);
                ushort initialization = ReadWord(bus, cursor + 6);
                if (properties != 0x2000 || extraProperties != 0 || initialization != 0)
                {
                    throw new InvalidDataException(
                        $"Population $A1:{population.Pointer:X4} has a non-retail Maw record.");
                }
                actual.Add(new MawRecord(
                    ReadWord(bus, cursor + 2), ReadWord(bus, cursor + 4),
                    ReadWord(bus, cursor + 12), ReadWord(bus, cursor + 14)));
            }

            if (!actual.SequenceEqual(population.Records))
            {
                throw new InvalidDataException(
                    $"Population $A1:{population.Pointer:X4} lost its exact Yapping Maw records.");
            }
        }
    }

    private static void VerifyRomTables(ISnesAddressSpace bus)
    {
        VerifyWords(bus, 0xa89f4f, Palette, "palette");
        VerifyWords(bus, 0xa8a097, DirectionLists, "direction-list table");
        VerifyWords(bus, 0x86ec95,
            [0xec62, 0xec94, 0xec56, 0x0202, 0x2005, 0, 0x84fc],
            "body-projectile definition");
        VerifyWords(bus, 0xb4be18, [0xc5d8, 0xc5de], "root-object instruction pointers");
        VerifyWords(bus, 0xb4c5d8,
            [0x0001, 0xd8af, 0xbcf0, 0x0001, 0xd8b6, 0xbcf0],
            "root-object instruction lists");
    }

    private static void VerifyInitializationAnimationAndGrab(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedMaw loaded = Load(bus, room, assets);
        RoomEnemySlot actor = loaded.Actor;
        YappingMawEnemyState state = State(loaded);

        RoomEnemyProjectileSlot[] bodies = state.BodyProjectiles
            .Select(body => body ?? throw new InvalidDataException("Maw omitted a body link."))
            .ToArray();
        if (actor.CurrentInstruction != 0x9fc7 || actor.InstructionTimer != 1 ||
            state.Function != YappingMawAiFunction.WaitingForSamus ||
            state.OriginX != 0x0088 || state.OriginY != 0x0038 ||
            state.ActivationDistance != 0x0070 || state.RetractedDelayTimer != 64 ||
            !bodies.Select(body => body.SlotIndex).SequenceEqual([14, 15, 16, 17]) ||
            bodies.Any(body => body.Kind != RoomEnemyProjectileKind.YappingMawBody ||
                body.InstructionPointer != 0xec56 || body.PreInstruction != 0xec94) ||
            state.RootSpriteObject is not { SlotIndex: 31,
                Kind: RoomSpriteObjectKind.YappingMawRootVariantZero,
                XPosition: 0x0088, YPosition: 0x0030 })
        {
            throw new InvalidDataException("Yapping Maw multipart initialization diverged from ROM.");
        }

        // Samus begins exactly 64 pixels below the root. The first main pass schedules setup,
        // the second selects the directional list, and subsequent passes solve the four-link
        // curve. This proves the state transition rather than accepting a static assembled pose.
        Step(loaded, assets, frame: 0);
        if (state.Function != YappingMawAiFunction.BeginExtension)
            throw new InvalidDataException("Maw did not admit the strict in-range target.");
        Step(loaded, assets, frame: 1);
        if (state.Function != YappingMawAiFunction.ExtendingOrRetracting ||
            state.SegmentRadius != 32 || state.DirectionTableByteOffset != 8 ||
            state.AimAngle != 128)
        {
            throw new InvalidDataException(
                $"Maw extension setup mismatch: function={state.Function}, radius=" +
                $"{state.SegmentRadius}, direction={state.DirectionTableByteOffset}, " +
                $"aim={state.AimAngle}, distance={state.DistanceToSamus}.");
        }

        var observedMouthPositions = new HashSet<(ushort X, ushort Y)>();
        bool heardAttack = false;
        for (int frame = 2; frame < 34; frame++)
        {
            Step(loaded, assets, frame);
            observedMouthPositions.Add((actor.XPosition, actor.YPosition));
            heardAttack |= loaded.Enemies.LastYappingMawSoundEffect == 0x002f;
        }
        if (observedMouthPositions.Count < 4 ||
            bodies.Any(body => body.SpritemapPointer is not (0xb8d7 or 0xb8de)) ||
            bodies[0].XPosition != state.OriginX || bodies[0].YPosition != state.OriginY ||
            !heardAttack)
        {
            throw new InvalidDataException(
                $"Maw curve mismatch: positions={observedMouthPositions.Count}, " +
                $"maps={string.Join(',', bodies.Select(body => $"${body.SpritemapPointer:X4}"))}, " +
                $"root=({bodies[0].XPosition},{bodies[0].YPosition}), " +
                $"origin=({state.OriginX},{state.OriginY}), sound={heardAttack}, " +
                $"function={state.Function}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawLayers(oam, 0, 0, 0, 7);
        loaded.Enemies.DrawEnemyProjectiles(oam, 0, 0);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount < 6)
            throw new InvalidDataException("Maw mouth/body/root actors did not all emit OBJ pieces.");

        // Its private touch callback grabs without applying the header's 30 damage. Place
        // Samus on the live mouth after the cooldown has wrapped negative, then let the next
        // main pass demonstrate position ownership using the animation-selected offset.
        loaded.Samus.XPosition = actor.XPosition;
        loaded.Samus.YPosition = actor.YPosition;
        ushort healthBefore = loaded.Samus.Health;
        if (!loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0) ||
            !state.HasGrabbedSamus || !loaded.Samus.InputLocked ||
            loaded.Samus.Health != healthBefore)
        {
            throw new InvalidDataException("Maw touch did not perform its non-damaging custom grab.");
        }
        Step(loaded, assets, frame: 34);
        if (loaded.Samus.XPosition != unchecked((ushort)(actor.XPosition + state.HeldSamusXOffset)) ||
            loaded.Samus.YPosition != unchecked((ushort)(actor.YPosition + state.HeldSamusYOffset)))
        {
            throw new InvalidDataException("Maw did not retain Samus at its ROM animation offset.");
        }

        // Ice is vulnerability $FF in this table. Common shot AI installs the 400-tick
        // freeze, then $A8:A7BD must release the captive and the custom frozen tail recolors
        // all four links plus the root to OBJ palette six.
        var shots = new SamusProjectileSystem();
        ArmBeam(shots.Slots[0], actor, damage: 20, type: 2);
        if (loaded.Enemies.ResolveOrdinaryProjectileHits(
                bus, shots, new SamusBombProjectileSystem(), loaded.Samus) != 1 ||
            actor.FrozenTimer != 400 || state.HasGrabbedSamus || loaded.Samus.InputLocked)
        {
            throw new InvalidDataException("Ice did not freeze the Maw and release Samus.");
        }
        Step(loaded, assets, frame: 35);
        if (bodies.Any(body => (body.GraphicsIndex & 0x0e00) != 0x0c00) ||
            (state.RootSpriteObject!.GraphicsIndex & 0x0e00) != 0x0c00)
        {
            throw new InvalidDataException("Maw frozen palette did not propagate to auxiliary actors.");
        }
    }

    private static void VerifyShotDeathAndMultipartCleanup(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedMaw loaded = Load(bus, room, assets);
        YappingMawEnemyState state = State(loaded);
        Step(loaded, assets, frame: 0);

        var shots = new SamusProjectileSystem();
        // Retail beam bytes are immune while the missile byte is multiplier two. A
        // 20-damage missile therefore produces exactly 20 common-AI damage after pre-scale.
        ArmBeam(shots.Slots[0], loaded.Actor, damage: 20, type: 0x0200);
        if (loaded.Enemies.ResolveOrdinaryProjectileHits(
                bus, shots, new SamusBombProjectileSystem(), loaded.Samus) != 1 ||
            loaded.Actor.Health != 0 ||
            !loaded.Actor.Properties.HasAny(EnemyProperties.Deleted) ||
            state.BodyProjectiles.Any(body => body is not null && body.IsActive) ||
            state.RootSpriteObject is null || state.RootSpriteObject.IsActive ||
            loaded.Enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Maw lethal cleanup mismatch: health={loaded.Actor.Health}, properties=" +
                $"${loaded.Actor.Properties:X4}, bodies=" +
                $"{string.Join(',', state.BodyProjectiles.Select(body => body?.IsActive))}, " +
                $"root={state.RootSpriteObject?.IsActive}, killed={loaded.Enemies.EnemiesKilled}.");
        }
    }

    private static LoadedMaw Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        if (room.State.Pointer != AuditStatePointer ||
            room.State.EnemyPopulationPointer != AuditPopulationPointer)
        {
            throw new InvalidDataException(
                $"Maw audit room selected ${room.State.Pointer:X4}/" +
                $"${room.State.EnemyPopulationPointer:X4}.");
        }

        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var isolatedPopulation = new PopulationPrefixAddressSpace(
            bus,
            AuditPopulationPointer,
            retainedRecordCount: 1,
            deathQuota: 0);
        var random = new Bank80SystemState();
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = 0x0088,
            YPosition = 0x0078,
        };
        samus.RefreshCollisionRadii(isolatedPopulation);
        samus.InitializeAnimation(isolatedPopulation);

        var enemies = new RoomEnemySystem();
        enemies.Load(
            isolatedPopulation,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus);
        RoomEnemySlot actor = enemies.Slots[0];
        if (actor.EnemyDefinitionPointer != DefinitionPointer)
            throw new InvalidDataException("Maw audit population did not load $E7BF in slot zero.");

        // The wrapper retains the exact first retail record and terminates the population
        // before the untranslated neighboring actor. Room, tileset, palette, and the Maw's
        // authored record remain cartridge data; only this focused audit's actor count is cut.
        return new LoadedMaw(enemies, actor, samus);
    }

    private static void Step(LoadedMaw loaded, CartridgeRoomAssets assets, int frame)
    {
        loaded.Enemies.StepFrame(
            cameraX: 0,
            cameraY: 0,
            timeIsFrozen: false,
            loaded.Samus,
            level: assets.LevelData,
            nmiFrameCounter8: unchecked((byte)frame));
        loaded.Enemies.StepEnemyProjectiles(
            assets.LevelData,
            loaded.Samus,
            cameraX: 0,
            cameraY: 0,
            nmiFrameCounter8: unchecked((byte)frame));
    }

    private static void ArmBeam(
        SamusProjectileSlot projectile,
        RoomEnemySlot actor,
        ushort damage,
        ushort type)
    {
        projectile.ClearFields();
        projectile.Type = type;
        projectile.Damage = damage;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = actor.XPosition;
        projectile.YPosition = actor.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static YappingMawEnemyState State(LoadedMaw loaded) =>
        loaded.Enemies.YappingMawStates[loaded.Actor.SlotIndex] ??
        throw new InvalidDataException("Maw slot zero has no typed state.");

    private static void VerifyWords(
        ISnesAddressSpace bus,
        int address,
        IReadOnlyList<ushort> expected,
        string name)
    {
        for (int index = 0; index < expected.Count; index++)
        {
            ushort actual = ReadWord(bus, address + index * 2);
            if (actual != expected[index])
            {
                throw new InvalidDataException(
                    $"Maw {name} word {index} is ${actual:X4}, expected ${expected[index]:X4}.");
            }
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct MawRecord(
        ushort X,
        ushort Y,
        ushort ActivationDistance,
        ushort RootVariant);

    private readonly record struct MawPopulation(ushort Pointer, IReadOnlyList<MawRecord> Records);

    private sealed record LoadedMaw(
        RoomEnemySystem Enemies,
        RoomEnemySlot Actor,
        SamusState Samus);
}
